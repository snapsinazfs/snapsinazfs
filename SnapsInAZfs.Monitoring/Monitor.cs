#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NLog;

namespace SnapsInAZfs.Monitoring;

/// <summary>
///     A general monitoring class that implements <see cref="IMonitor" />, <see cref="IApplicationStateObserver" />, and
///     <see cref="ISnapshotOperationsObserver" />, and allows monitoring of one of each (which can be the same object)
/// </summary>
public sealed class Monitor : IMonitor
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private ApplicationState _applicationState;
    private IApplicationStateObservable? _applicationStateObservable;
    private bool _applicationStateObservableEventSubscribed;
    private ISnapshotOperationsObservable? _snapshotOperationsObservable;

    private uint _snapshotsPrunedFailedLastExecution;

    private uint _snapshotsPrunedFailedSinceStart;

    private uint _snapshotsPrunedSucceededLastExecution;

    private uint _snapshotsPrunedSucceededSinceStart;

    private uint _snapshotsTakenFailedLastExecution;

    private uint _snapshotsTakenFailedSinceStart;

    private uint _snapshotsTakenSucceededLastExecution;

    private uint _snapshotsTakenSucceededSinceStart;

    public DateTimeOffset SnapshotsPrunedLastEnded { get; set; } = DateTimeOffset.UnixEpoch;

    public DateTimeOffset SnapshotsTakenLastEnded { get; set; } = DateTimeOffset.UnixEpoch;

    /// <summary>
    ///     Gets the state of the registered <see cref="IApplicationStateObservable" /> object.
    /// </summary>
    /// <returns>
    ///     If subscribed to the <see cref="IApplicationStateObservable.ApplicationStateChanged" /> event, returns the last known state
    ///     this object was informed of.<br />
    ///     If not subscribed, gets the current <see cref="IApplicationStateObservable.State" /> value from the registered object.<br />
    ///     If no object is registered, returns the string "Not Registered"
    /// </returns>
    public string GetApplicationState( )
    {
        return _applicationStateObservable switch
        {
            null => "Not Registered",
            _ when _applicationStateObservableEventSubscribed => _applicationState.ToString( "G" ),
            _ => _applicationStateObservable.State.ToString( "G" )
        };
    }

    /// <inheritdoc />
    public DateTimeOffset GetServiceStartTime( )
    {
        return _applicationStateObservable?.ServiceStartTime ?? DateTimeOffset.UnixEpoch;
    }

    /// <summary>
    ///     Registers an <see cref="IApplicationStateObservable" /> object with this <see cref="Monitor" /> instance by subscribing to
    ///     the <see cref="IApplicationStateObservable" /> object's <see cref="IApplicationStateObservable.ApplicationStateChanged" />
    ///     <see langword="event" />, if <paramref name="subscribeToEvents" /> is <see langword="true" />, and setting a local reference
    ///     to the registered object.
    /// </summary>
    /// <paramref name="observableObject">
    ///     The <see cref="IApplicationStateObservable" /> object instance to register to this <see cref="Monitor" />
    /// </paramref>
    /// <paramref name="subscribeToEvents">
    ///     Whether to subscribe to <paramref name="observableObject" />'s
    ///     <see cref="IApplicationStateObservable.ApplicationStateChanged" /> event.<br />
    ///     Default is <see langword="true" />.
    /// </paramref>
    /// <remarks>
    ///     Enforces one monitored object per Observer.<br />
    ///     Can also be used to re-register the same object to subscribe or unsubscribe to the event.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Monitor object has already registered an IApplicationStateObservable instance. Only one is allowed per Monitor object.
    /// </exception>
    public void RegisterApplicationStateObservable( IApplicationStateObservable observableObject, bool subscribeToEvents = true )
    {
        if ( _applicationStateObservable is not null && !ReferenceEquals( _applicationStateObservable, observableObject ) )
        {
            throw new InvalidOperationException( "Monitor object has already registered an IApplicationStateObservable instance. Only one is allowed per Monitor object." );
        }

        _applicationStateObservable ??= observableObject;
        switch ( subscribeToEvents )
        {
            case true when !_applicationStateObservableEventSubscribed:
                _applicationStateObservable.ApplicationStateChanged += ServiceOnApplicationStateChanged;
                _applicationStateObservableEventSubscribed = true;
                break;
            case false when _applicationStateObservableEventSubscribed:
                _applicationStateObservable.ApplicationStateChanged -= ServiceOnApplicationStateChanged;
                _applicationStateObservableEventSubscribed = false;
                break;
        }
    }

    [ExcludeFromCodeCoverage( Justification = "Not useful to test this" )]
    public string GetVersion( )
    {
        // ReSharper disable once ExceptionNotDocumented
        return Assembly.GetEntryAssembly( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute>( )?.InformationalVersion ?? string.Empty;
    }

    [ExcludeFromCodeCoverage( Justification = "Not useful to test this" )]
    public long GetWorkingSet( )
    {
        return Environment.WorkingSet;
    }

    /// <inheritdoc />
    public uint GetSnapshotsTakenSucceededSinceStart( )
    {
        return SnapshotsTakenSucceededSinceStart;
    }

    /// <inheritdoc />
    public uint GetSnapshotsPrunedSucceededSinceStart( )
    {
        return SnapshotsPrunedSucceededSinceStart;
    }

    public SnapshotCountMetrics GetAllCounts( )
    {
        return new( in _snapshotsPrunedFailedLastExecution, in _snapshotsPrunedFailedSinceStart, in _snapshotsPrunedSucceededLastExecution, in _snapshotsPrunedSucceededSinceStart, in _snapshotsTakenFailedLastExecution, in _snapshotsTakenFailedSinceStart, in _snapshotsTakenSucceededLastExecution, in _snapshotsTakenSucceededSinceStart );
    }

    /// <summary>
    ///     Registers an <see cref="ISnapshotOperationsObservable" /> object with this <see cref="Monitor" /> instance by subscribing to
    ///     the <see cref="ISnapshotOperationsObservable" /> object's <see langword="event" />s and setting a local reference to the
    ///     registered object.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Monitor object has already registered an ISnapshotOperationsObservable instance. Only one is allowed per Monitor object.
    /// </exception>
    public void RegisterSnapshotOperationsObservable( ISnapshotOperationsObservable observableObject )
    {
        if ( _snapshotOperationsObservable is not null && !ReferenceEquals( _snapshotOperationsObservable, observableObject ) )
        {
            throw new InvalidOperationException( "Monitor object has already registered an ISnapshotOperationsObservable instance. Only one is allowed per Monitor object." );
        }

        if ( _snapshotOperationsObservable is not null )
        {
            return;
        }

        _snapshotOperationsObservable = observableObject;
        observableObject.BeginTakingSnapshots += ServiceOnBeginTakingSnapshots;
        observableObject.TakeSnapshotSucceeded += ServiceOnTakeSnapshotSucceeded;
        observableObject.TakeSnapshotFailed += ServiceOnTakeSnapshotFailed;
        observableObject.EndTakingSnapshots += ServiceOnEndTakingSnapshots;
        observableObject.BeginPruningSnapshots += ServiceOnBeginPruningSnapshots;
        observableObject.PruneSnapshotSucceeded += ServiceOnPruneSnapshotSucceeded;
        observableObject.PruneSnapshotFailed += ServiceOnPruneSnapshotFailed;
        observableObject.EndPruningSnapshots += ServiceOnEndPruningSnapshots;
    }

    /// <inheritdoc />
    public uint SnapshotsTakenFailedSinceStart => _snapshotsTakenFailedSinceStart;

    /// <inheritdoc />
    public uint SnapshotsTakenSucceededSinceStart => _snapshotsTakenSucceededSinceStart;

    /// <inheritdoc />
    public uint SnapshotsPrunedFailedLastExecution => _snapshotsPrunedFailedLastExecution;

    /// <inheritdoc />
    public uint SnapshotsPrunedFailedSinceStart => _snapshotsPrunedFailedSinceStart;

    /// <inheritdoc />
    public uint SnapshotsTakenFailedLastExecution => _snapshotsTakenFailedLastExecution;

    /// <inheritdoc />
    public uint SnapshotsTakenSucceededLastExecution => _snapshotsTakenSucceededLastExecution;

    /// <inheritdoc />
    public uint SnapshotsPrunedSucceededSinceStart => _snapshotsPrunedSucceededSinceStart;

    /// <inheritdoc />
    public uint SnapshotsPrunedSucceededLastExecution => _snapshotsPrunedSucceededLastExecution;

    public ApplicationStateMetrics GetFullApplicationState( )
    {
        return new( GetApplicationState( ), GetServiceStartTime( ), GetVersion( ) );
    }

    private void ServiceOnApplicationStateChanged( object? sender, ApplicationStateChangedEventArgs e )
    {
        Logger.Trace( "Service state changed from {0:G} to {1:G}", e.Previous, e.Current );
        _applicationState = e.Current;
    }

    private void ServiceOnBeginPruningSnapshots( object? sender, DateTimeOffset timestamp )
    {
        Logger.Trace( "Received BeginPruningSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
        Interlocked.Exchange( ref _snapshotsPrunedSucceededLastExecution, 0u );
        Interlocked.Exchange( ref _snapshotsPrunedFailedLastExecution, 0u );
    }

    private void ServiceOnBeginTakingSnapshots( object? sender, DateTimeOffset timestamp )
    {
        Logger.Trace( "Received BeginTakingSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
        Interlocked.Exchange( ref _snapshotsTakenSucceededLastExecution, 0u );
        Interlocked.Exchange( ref _snapshotsTakenFailedLastExecution, 0u );
    }

    private void ServiceOnEndPruningSnapshots( object? sender, DateTimeOffset timestamp )
    {
        Logger.Trace( "Received EndPruningSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
        SnapshotsPrunedLastEnded = timestamp;
    }

    private void ServiceOnEndTakingSnapshots( object? sender, DateTimeOffset timestamp )
    {
        Logger.Trace( "Received EndTakingSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
        SnapshotsTakenLastEnded = timestamp;
    }

    private void ServiceOnPruneSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received PruneSnapshotFailed event from {0}", sender?.GetType( ).Name );
        Interlocked.Increment( ref _snapshotsPrunedFailedLastExecution );
        Interlocked.Increment( ref _snapshotsPrunedFailedSinceStart );
    }

    private void ServiceOnPruneSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received PruneSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
        Interlocked.Increment( ref _snapshotsPrunedSucceededLastExecution );
        Interlocked.Increment( ref _snapshotsPrunedSucceededSinceStart );
    }

    private void ServiceOnTakeSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received TakeSnapshotFailed event from {0}", sender?.GetType( ).Name );
        Interlocked.Increment( ref _snapshotsTakenFailedLastExecution );
        Interlocked.Increment( ref _snapshotsTakenFailedSinceStart );
    }

    private void ServiceOnTakeSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received TakeSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
        Interlocked.Increment( ref _snapshotsTakenSucceededLastExecution );
        Interlocked.Increment( ref _snapshotsTakenSucceededSinceStart );
    }
}
