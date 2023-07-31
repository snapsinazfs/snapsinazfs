#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Reflection;
using NLog;

namespace SnapsInAZfs.Monitoring;

public sealed class Monitor : IMonitor, IApplicationStateObserver, ISnapshotOperationsObserver
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private ApplicationState _applicationState;

    public DateTimeOffset SnapshotsPrunedLastEnded { get; set; } = DateTimeOffset.UnixEpoch;

    public DateTimeOffset SnapshotsTakenLastEnded { get; set; } = DateTimeOffset.UnixEpoch;

    public string GetApplicationState( )
    {
        return _applicationState.ToString( "G" );
    }

    public void RegisterApplicationStateObservable( IApplicationStateObservable observableObject )
    {
        observableObject.ApplicationStateChanged += ServiceOnApplicationStateChanged;
    }

    public void RegisterSnapshotOperationsObservable( ISnapshotOperationsObservable observableObject )
    {
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
    public uint SnapshotsTakenFailedSinceStart { get; set; }

    /// <inheritdoc />
    public uint SnapshotsTakenSucceededSinceStart { get; set; }

    /// <inheritdoc />
    public uint SnapshotsPrunedFailedLastExecution { get; set; }

    /// <inheritdoc />
    public uint SnapshotsPrunedFailedSinceStart { get; set; }

    /// <inheritdoc />
    public uint SnapshotsTakenFailedLastExecution { get; set; }

    /// <inheritdoc />
    public uint SnapshotsTakenSucceededLastExecution { get; set; }

    /// <inheritdoc />
    public uint SnapshotsPrunedSucceededSinceStart { get; set; }

    /// <inheritdoc />
    public uint SnapshotsPrunedSucceededLastExecution { get; set; }

    public string GetVersion( )
    {
        // ReSharper disable once ExceptionNotDocumented
        return Assembly.GetEntryAssembly( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute>( )?.InformationalVersion ?? string.Empty;
    }

    public long GetWorkingSet( )
    {
        return Environment.WorkingSet;
    }

    private void ServiceOnApplicationStateChanged( object? sender, ApplicationStateChangedEventArgs e )
    {
        Logger.Debug( "Service state changed from {0:G} to {1:G}", e.Previous, e.Current );
        _applicationState = e.Current;
    }

    private void ServiceOnBeginPruningSnapshots( object? sender, EventArgs e )
    {
        Logger.Debug( "Received BeginPruningSnapshots event from {0}", sender?.GetType( ).Name );
        SnapshotsPrunedSucceededLastExecution = 0u;
        SnapshotsPrunedFailedLastExecution = 0u;
    }

    private void ServiceOnBeginTakingSnapshots( object? sender, EventArgs e )
    {
        Logger.Debug( "Received BeginTakingSnapshots event from {0}", sender?.GetType( ).Name );
        SnapshotsTakenSucceededLastExecution = 0u;
        SnapshotsTakenFailedLastExecution = 0u;
    }

    private void ServiceOnEndPruningSnapshots( object? sender, EventArgs e )
    {
        Logger.Debug( "Received EndPruningSnapshots event from {0}", sender?.GetType( ).Name );
        SnapshotsPrunedLastEnded = DateTimeOffset.Now;
    }

    private void ServiceOnEndTakingSnapshots( object? sender, EventArgs e )
    {
        Logger.Debug( "Received EndTakingSnapshots event from {0}", sender?.GetType( ).Name );
        SnapshotsTakenLastEnded = DateTimeOffset.Now;
    }

    private void ServiceOnPruneSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Debug( "Received PruneSnapshotFailed event from {0}", sender?.GetType( ).Name );
        SnapshotsPrunedFailedLastExecution++;
        SnapshotsPrunedFailedSinceStart++;
    }

    private void ServiceOnPruneSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Debug( "Received PruneSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
        SnapshotsPrunedSucceededSinceStart++;
        SnapshotsPrunedSucceededLastExecution++;
    }

    private void ServiceOnTakeSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Debug( "Received TakeSnapshotFailed event from {0}", sender?.GetType( ).Name );
        SnapshotsTakenFailedLastExecution++;
        SnapshotsTakenFailedSinceStart++;
    }

    private void ServiceOnTakeSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
        Logger.Debug( "Received TakeSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
        SnapshotsTakenSucceededSinceStart++;
        SnapshotsTakenSucceededLastExecution++;
    }
}
