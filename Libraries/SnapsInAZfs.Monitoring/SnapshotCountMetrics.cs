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

using System.Text.Json.Serialization;

namespace SnapsInAZfs.Monitoring;

public sealed class SnapshotCountMetrics
{
    public SnapshotCountMetrics( )
    {
    }

    public SnapshotCountMetrics( in uint snapshotsPrunedFailedLastExecution, in uint snapshotsPrunedFailedSinceStart, in uint snapshotsPrunedSucceededLastExecution, in uint snapshotsPrunedSucceededSinceStart, in uint snapshotsTakenFailedLastExecution, in uint snapshotsTakenFailedSinceStart, in uint snapshotsTakenSucceededLastExecution, in uint snapshotsTakenSucceededSinceStart )
    {
        SnapshotsPrunedFailedLastExecution = snapshotsPrunedFailedLastExecution;
        SnapshotsPrunedFailedSinceStart = snapshotsPrunedFailedSinceStart;
        SnapshotsPrunedSucceededLastExecution = snapshotsPrunedSucceededLastExecution;
        SnapshotsPrunedSucceededSinceStart = snapshotsPrunedSucceededSinceStart;
        SnapshotsTakenFailedLastExecution = snapshotsTakenFailedLastExecution;
        SnapshotsTakenFailedSinceStart = snapshotsTakenFailedSinceStart;
        SnapshotsTakenSucceededLastExecution = snapshotsTakenSucceededLastExecution;
        SnapshotsTakenSucceededSinceStart = snapshotsTakenSucceededSinceStart;
    }

    [JsonPropertyOrder( 1 )]
    public uint SnapshotsPrunedFailedLastExecution { get; set; }

    [JsonPropertyOrder( 2 )]
    public uint SnapshotsPrunedFailedSinceStart { get; set; }

    [JsonPropertyOrder( 3 )]
    public uint SnapshotsPrunedSucceededLastExecution { get; set; }

    [JsonPropertyOrder( 4 )]
    public uint SnapshotsPrunedSucceededSinceStart { get; set; }

    [JsonPropertyOrder( 5 )]
    public uint SnapshotsTakenFailedLastExecution { get; set; }

    [JsonPropertyOrder( 6 )]
    public uint SnapshotsTakenFailedSinceStart { get; set; }

    [JsonPropertyOrder( 7 )]
    public uint SnapshotsTakenSucceededLastExecution { get; set; }

    [JsonPropertyOrder( 8 )]
    public uint SnapshotsTakenSucceededSinceStart { get; set; }
}
