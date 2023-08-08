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

/// <summary>
///     Interface defining minimum implementation of an object that can be observed by an <see cref="ISnapshotOperationsObserver" />
///     <br />
///     Events are expected to be invoked so that subscribed <see cref="ISnapshotOperationsObserver" />s can react to them
/// </summary>
public interface ISnapshotOperationsObservable
{
    /// <summary>
    ///     <see langword="event" /> invoked when the process of pruning snapshots has begun, before any snapshots are pruned, but after
    ///     the list of snapshots to prune has been calculated
    /// </summary>
    /// <remarks>
    ///     Note this event is slightly different than <see cref="BeginTakingSnapshots" />, as this event happens AFTER determination of
    ///     snapshots to prune.
    /// </remarks>
    event EventHandler BeginPruningSnapshots;

    /// <summary>
    ///     <see langword="event" /> invoked when the process of taking new snapshots has begun, before any snapshots are taken, and
    ///     before snapshots to be taken have been calculated
    /// </summary>
    /// <remarks>
    ///     Note this event is slightly different than <see cref="BeginPruningSnapshots" />, as this event happens before determination
    ///     of snapshots to take, since those are calculated on-the-fly.
    /// </remarks>
    event EventHandler BeginTakingSnapshots;

    /// <summary>
    ///     <see langword="event" /> invoked after all eligible snapshots have been pruned
    /// </summary>
    /// <remarks>
    ///     This event will still be invoked if no snapshots were pruned.
    /// </remarks>
    event EventHandler EndPruningSnapshots;

    /// <summary>
    ///     <see langword="event" /> invoked after all configured snapshots have been taken
    /// </summary>
    /// <remarks>
    ///     This event will still be invoked if no new snapshots were taken.
    /// </remarks>
    event EventHandler EndTakingSnapshots;

    event EventHandler<SnapshotOperationEventArgs> PruneSnapshotFailed;
    event EventHandler<SnapshotOperationEventArgs> PruneSnapshotSucceeded;
    event EventHandler<SnapshotOperationEventArgs> TakeSnapshotFailed;
    event EventHandler<SnapshotOperationEventArgs> TakeSnapshotSucceeded;
}
