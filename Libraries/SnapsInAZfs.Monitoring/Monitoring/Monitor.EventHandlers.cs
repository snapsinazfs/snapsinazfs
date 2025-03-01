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

namespace SnapsInAZfs.Monitoring;

public sealed partial class Monitor
{
    private void ServiceOnApplicationStateChanged( object? sender, ApplicationStateChangedEventArgs e )
    {
    #if DEBUG
        Logger.Trace( "Service state changed from {0:G} to {1:G}", e.Previous, e.Current );
    #endif
        _applicationState = e.Current;
    }

    private void ServiceOnBeginPruningSnapshots( object? sender, DateTimeOffset timestamp )
    {
    #if DEBUG
        Logger.Trace( "Received BeginPruningSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
    #endif
        Interlocked.Exchange( ref _snapshotsPrunedSucceededLastRun, 0u );
        Interlocked.Exchange( ref _snapshotsPrunedFailedLastRun, 0u );
        lock ( _snapshotsPrunedFailedLastRunNamesLock )
        {
            _snapshotsPrunedFailedLastRunNames.Clear( );
        }
    }

    private void ServiceOnBeginTakingSnapshots( object? sender, DateTimeOffset timestamp )
    {
    #if DEBUG
        Logger.Trace( "Received BeginTakingSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
    #endif
        Interlocked.Exchange( ref _snapshotsTakenSucceededLastRun, 0u );
        Interlocked.Exchange( ref _snapshotsTakenFailedLastRun, 0u );
        lock ( _snapshotsTakenFailedLastRunNamesLock )
        {
            _snapshotsTakenFailedLastRunNames.Clear( );
        }
    }

    private void ServiceOnEndPruningSnapshots( object? sender, DateTimeOffset timestamp )
    {
    #if DEBUG
        Logger.Trace( "Received EndPruningSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
    #endif
        SnapshotsPrunedLastEnded = timestamp;
    }

    private void ServiceOnEndTakingSnapshots( object? sender, DateTimeOffset timestamp )
    {
    #if DEBUG
        Logger.Trace( "Received EndTakingSnapshots event from {0}, sent at {1:O}", sender?.GetType( ).Name, timestamp );
    #endif
        SnapshotsTakenLastEnded = timestamp;
    }

    private void ServiceOnNextRunTimeChanged( object? sender, long e )
    {
    #if DEBUG
        Logger.Trace( "Received NextRunTimeChanged event from {0} with value {1:D}", sender?.GetType( ).Name, e );
    #endif
        Interlocked.Exchange( ref _nextRunTime, e );
    }

    private void ServiceOnPruneSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
    #if DEBUG
        Logger.Trace( "Received PruneSnapshotFailed event from {0}", sender?.GetType( ).Name );
    #endif
        Interlocked.Increment( ref _snapshotsPrunedFailedLastRun );
        Interlocked.Increment( ref _snapshotsPrunedFailedSinceStart );
        lock ( _snapshotsPrunedFailedLastRunNamesLock )
        {
            _snapshotsPrunedFailedLastRunNames.Add( e.Name );
        }
    }

    private void ServiceOnPruneSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
    #if DEBUG
        Logger.Trace( "Received PruneSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
    #endif
        Interlocked.Increment( ref _snapshotsPrunedSucceededLastRun );
        Interlocked.Increment( ref _snapshotsPrunedSucceededSinceStart );
    }

    private void ServiceOnTakeSnapshotFailed( object? sender, SnapshotOperationEventArgs e )
    {
    #if DEBUG
        Logger.Trace( "Received TakeSnapshotFailed event from {0}", sender?.GetType( ).Name );
    #endif
        Interlocked.Increment( ref _snapshotsTakenFailedLastRun );
        Interlocked.Increment( ref _snapshotsTakenFailedSinceStart );
        lock ( _snapshotsTakenFailedLastRunNamesLock )
        {
            _snapshotsTakenFailedLastRunNames.Add( e.Name );
        }
    }

    private void ServiceOnTakeSnapshotSucceeded( object? sender, SnapshotOperationEventArgs e )
    {
    #if DEBUG
        Logger.Trace( "Received TakeSnapshotSucceeded event from {0} for {1}", sender?.GetType( ).Name, e.Name );
    #endif
        Interlocked.Increment( ref _snapshotsTakenSucceededLastRun );
        Interlocked.Increment( ref _snapshotsTakenSucceededSinceStart );
    }
}
