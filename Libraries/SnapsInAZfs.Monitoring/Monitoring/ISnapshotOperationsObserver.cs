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

using Microsoft.AspNetCore.Http.HttpResults;

namespace SnapsInAZfs.Monitoring;

/// <summary>
///     Interface defining minimum implementation of an object that intends to observe an
///     <see cref="ISnapshotOperationsObservable" />,
///     via direct polling and by event subscription
/// </summary>
public interface ISnapshotOperationsObserver
{
    Task<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>> GetAllSnapshotCountsAsync( );
    Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetLastSnapshotPrunedTimeAsync( );
    Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetLastSnapshotTakenTimeAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedLastRunCountAsync( );
    Task<Results<Ok<List<string>>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedLastRunNamesAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedSinceStartCountAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedSucceededLastRunCountAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedSucceededSinceStartCountAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenFailedLastRunCountAsync( );
    Task<Results<Ok<List<string>>, StatusCodeHttpResult>> GetSnapshotsTakenFailedLastRunNamesAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenFailedSinceStartCountAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenSucceededLastRunCountAsync( );
    Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenSucceededSinceStartCountAsync( );
    void RegisterSnapshotOperationsObservable( ISnapshotOperationsObservable observableObject );
}
