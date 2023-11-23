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
public sealed partial class Monitor : IMonitor
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

    private string GetApplicationState( )
    {
        return _applicationStateObservable switch
        {
            null => "Not Registered",
            // This warning is obsolete on .net7, as the implementation now caches the strings on first use
            // ReSharper disable HeapView.BoxingAllocation
            not null when _applicationStateObservableEventSubscribed => _applicationState.ToString( "G" ),
            _ => _applicationStateObservable.State.ToString( "G" )
            // ReSharper restore HeapView.BoxingAllocation
        };
    }
}
