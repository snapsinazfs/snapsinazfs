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
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NLog;

namespace SnapsInAZfs.Monitoring;

/// <summary>
///     A general monitoring class that implements <see cref="IMonitor" />, <see cref="IApplicationStateObserver" />, and
///     <see cref="ISnapshotOperationsObserver" />, and allows monitoring of one of each (which can be the same object)
/// </summary>
public sealed class Monitor : IMonitor
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private readonly List<string> _snapshotsPrunedFailedLastRunNames = new( );
    private readonly object _snapshotsPrunedFailedLastRunNamesLock = new( );
    private readonly List<string> _snapshotsTakenFailedLastRunNames = new( );
    private readonly object _snapshotsTakenFailedLastRunNamesLock = new( );
    private ApplicationState _applicationState;
    private IApplicationStateObservable? _applicationStateObservable;
    private bool _applicationStateObservableEventSubscribed;
    private long _nextRunTime = DateTimeOffset.UnixEpoch.ToUnixTimeMilliseconds( );
    private ISnapshotOperationsObservable? _snapshotOperationsObservable;
    private uint _snapshotsPrunedFailedLastRun;
    private uint _snapshotsPrunedFailedSinceStart;
    private uint _snapshotsPrunedSucceededLastRun;
    private uint _snapshotsPrunedSucceededSinceStart;
    private uint _snapshotsTakenFailedLastRun;
    private uint _snapshotsTakenFailedSinceStart;
    private uint _snapshotsTakenSucceededLastRun;
    private uint _snapshotsTakenSucceededSinceStart;
    public uint SnapshotsPrunedFailedLastRun => _snapshotsPrunedFailedLastRun;
    public uint SnapshotsPrunedFailedSinceStart => _snapshotsPrunedFailedSinceStart;
    public DateTimeOffset SnapshotsPrunedLastEnded { get; set; } = DateTimeOffset.UnixEpoch;
    public uint SnapshotsPrunedSucceededLastRun => _snapshotsPrunedSucceededLastRun;
    public uint SnapshotsPrunedSucceededSinceStart => _snapshotsPrunedSucceededSinceStart;
    public uint SnapshotsTakenFailedLastRun => _snapshotsTakenFailedLastRun;
    public uint SnapshotsTakenFailedSinceStart => _snapshotsTakenFailedSinceStart;
    public DateTimeOffset SnapshotsTakenLastEnded { get; set; } = DateTimeOffset.UnixEpoch;
    public uint SnapshotsTakenSucceededLastRun => _snapshotsTakenSucceededLastRun;
    public uint SnapshotsTakenSucceededSinceStart => _snapshotsTakenSucceededSinceStart;
    internal DateTimeOffset NextRunTime => DateTimeOffset.FromUnixTimeMilliseconds( Interlocked.Read( ref _nextRunTime ) );
    internal DateTimeOffset ServiceStartTime => _applicationStateObservable?.ServiceStartTime ?? DateTimeOffset.UnixEpoch;

    private static string? Version => Assembly.GetEntryAssembly( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute>( )?.InformationalVersion;
    private const string ErrorGettingSnapshotCount = "Error getting snapshot count";

    /// <summary>
    ///     Gets the state of the registered <see cref="IApplicationStateObservable" /> object.
    /// </summary>
    /// <returns>
    ///     If subscribed to the <see cref="IApplicationStateObservable.ApplicationStateChanged" /> event, returns the last known state
    ///     this object was informed of.<br />
    ///     If not subscribed, gets the current <see cref="IApplicationStateObservable.State" /> value from the registered object, as a
    ///     string.<br />
    ///     If no object is registered, returns the string "Not Registered"
    /// </returns>
    public async Task<Results<Ok<string>, StatusCodeHttpResult>> GetApplicationStateAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( _applicationStateObservable switch
            {
                not null => TypedResults.Ok( GetApplicationState( ) ),
                _ => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application state" );
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<ApplicationStateMetrics>, StatusCodeHttpResult>> GetFullApplicationStateAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<ApplicationStateMetrics>, StatusCodeHttpResult>>( _applicationStateObservable switch
            {
                not null => TypedResults.Ok( new ApplicationStateMetrics( GetApplicationState( ), ServiceStartTime, NextRunTime, Version ?? "Unknown" ) ),
                _ => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application state" );
            return await Task.FromResult<Results<Ok<ApplicationStateMetrics>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetServiceStartTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( _applicationStateObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( ServiceStartTime )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting service start time" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
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
    [SuppressMessage( "ReSharper", "HeapView.ObjectAllocation.Possible", Justification = "Event subscription" )]
    [SuppressMessage( "ReSharper", "HeapView.DelegateAllocation", Justification = "Event subscription" )]
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
                _applicationStateObservable.NextRunTimeChanged += ServiceOnNextRunTimeChanged;
                _applicationStateObservableEventSubscribed = true;
                break;
            case false when _applicationStateObservableEventSubscribed:
                _applicationStateObservable.ApplicationStateChanged -= ServiceOnApplicationStateChanged;
                _applicationStateObservable.NextRunTimeChanged -= ServiceOnNextRunTimeChanged;
                _applicationStateObservableEventSubscribed = false;
                break;
        }
    }

    public async Task<Results<Ok<string>, StatusCodeHttpResult>> GetVersionAsync( )
    {
        try
        {
            // ReSharper disable once ExceptionNotDocumented
            string? informationalVersion = Version;
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( informationalVersion switch
            {
                null or "" => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( informationalVersion )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application version" );
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<long>, StatusCodeHttpResult>> GetWorkingSetAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<long>, StatusCodeHttpResult>>( TypedResults.Ok( Environment.WorkingSet ) ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application working set" );
            return await Task.FromResult<Results<Ok<long>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenSucceededLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenSucceededLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenSucceededSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenSucceededSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedSucceededLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedSucceededLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedSucceededSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedSucceededSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenFailedLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenFailedLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenFailedSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenFailedSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>> GetAllSnapshotCountsAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( new SnapshotCountMetrics( in _snapshotsPrunedFailedLastRun, in _snapshotsPrunedFailedSinceStart, in _snapshotsPrunedSucceededLastRun, in _snapshotsPrunedSucceededSinceStart, in _snapshotsTakenFailedLastRun, in _snapshotsTakenFailedSinceStart, in _snapshotsTakenSucceededLastRun, in _snapshotsTakenSucceededSinceStart ) )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedFailedLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedFailedSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <summary>
    ///     Registers an <see cref="ISnapshotOperationsObservable" /> object with this <see cref="Monitor" /> instance by subscribing to
    ///     the <see cref="ISnapshotOperationsObservable" /> object's <see langword="event" />s and setting a local reference to the
    ///     registered object.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Monitor object has already registered an ISnapshotOperationsObservable instance. Only one is allowed per Monitor object.
    /// </exception>
    [SuppressMessage( "ReSharper", "HeapView.ObjectAllocation.Possible", Justification = "Event subscription" )]
    [SuppressMessage( "ReSharper", "HeapView.DelegateAllocation", Justification = "Event subscription" )]
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
    public Task<Results<Ok<List<string>>, StatusCodeHttpResult>> GetSnapshotsTakenFailedLastRunNamesAsync( )
    {
        if ( _snapshotOperationsObservable is null )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ) );
        }

        lock ( _snapshotsTakenFailedLastRunNamesLock )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.Ok( _snapshotsTakenFailedLastRunNames.ToList( ) ) );
        }
    }

    /// <inheritdoc />
    public Task<Results<Ok<List<string>>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedLastRunNamesAsync( )
    {
        if ( _snapshotOperationsObservable is null )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ) );
        }

        lock ( _snapshotsPrunedFailedLastRunNamesLock )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.Ok( _snapshotsPrunedFailedLastRunNames.ToList( ) ) );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetLastSnapshotTakenTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( SnapshotsTakenLastEnded )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting timestamp" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetLastSnapshotPrunedTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( SnapshotsPrunedLastEnded )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting timestamp" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetNextRunTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>(
                _applicationStateObservable switch
                {
                    null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                    _ => TypedResults.Ok( DateTimeOffset.FromUnixTimeMilliseconds( Interlocked.Read( ref _nextRunTime ) ) )
                } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting timestamp" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    private string GetApplicationState( )
    {
        return _applicationStateObservable switch
        {
            null => "Not Registered",
            not null when _applicationStateObservableEventSubscribed => _applicationState.ToString( "G" ),
            _ => _applicationStateObservable.State.ToString( "G" )
        };
    }

    private void ServiceOnApplicationStateChanged( object? sender, ApplicationStateChangedEventArgs e )
    {
        Logger.Trace( "Service state changed from {0:G} to {1:G}", e.Previous, e.Current );
        _applicationState = e.Current;
    }

    private void ServiceOnBeginPruningSnapshots( object? sender, DateTimeOffset timestamp )
    {
        Logger.Trace( "Received BeginPruningSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
        Interlocked.Exchange( ref _snapshotsPrunedSucceededLastRun, 0u );
        Interlocked.Exchange( ref _snapshotsPrunedFailedLastRun, 0u );
        lock ( _snapshotsPrunedFailedLastRunNamesLock )
        {
            _snapshotsPrunedFailedLastRunNames.Clear( );
        }
    }

    private void ServiceOnBeginTakingSnapshots( object? sender, DateTimeOffset timestamp )
    {
        Logger.Trace( "Received BeginTakingSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
        Interlocked.Exchange( ref _snapshotsTakenSucceededLastRun, 0u );
        Interlocked.Exchange( ref _snapshotsTakenFailedLastRun, 0u );
        lock ( _snapshotsTakenFailedLastRunNamesLock )
        {
            _snapshotsTakenFailedLastRunNames.Clear( );
        }
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

    private void ServiceOnNextRunTimeChanged( object? sender, long e )
    {
        Interlocked.Exchange( ref _nextRunTime, e );
    }

    private void ServiceOnPruneSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received PruneSnapshotFailed event from {0}", sender?.GetType( ).Name );
        Interlocked.Increment( ref _snapshotsPrunedFailedLastRun );
        Interlocked.Increment( ref _snapshotsPrunedFailedSinceStart );
        lock ( _snapshotsPrunedFailedLastRunNamesLock )
        {
            _snapshotsPrunedFailedLastRunNames.Add( e.Name );
        }
    }

    private void ServiceOnPruneSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received PruneSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
        Interlocked.Increment( ref _snapshotsPrunedSucceededLastRun );
        Interlocked.Increment( ref _snapshotsPrunedSucceededSinceStart );
    }

    private void ServiceOnTakeSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received TakeSnapshotFailed event from {0}", sender?.GetType( ).Name );
        Interlocked.Increment( ref _snapshotsTakenFailedLastRun );
        Interlocked.Increment( ref _snapshotsTakenFailedSinceStart );
        lock ( _snapshotsTakenFailedLastRunNamesLock )
        {
            _snapshotsTakenFailedLastRunNames.Add( e.Name );
        }
    }

    private void ServiceOnTakeSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Trace( "Received TakeSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
        Interlocked.Increment( ref _snapshotsTakenSucceededLastRun );
        Interlocked.Increment( ref _snapshotsTakenSucceededSinceStart );
    }
}
