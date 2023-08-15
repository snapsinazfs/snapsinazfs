﻿#region MIT LICENSE

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

public sealed class ApplicationStateMetrics
{
    public ApplicationStateMetrics( string state, DateTimeOffset serviceStartTime, DateTimeOffset nextRunTime, string version )
    {
        Version = version;
        State = state;
        ServiceStartTime = serviceStartTime.ToLocalTime( );
        NextRunTime = nextRunTime.ToLocalTime( );
    }

    [JsonPropertyOrder( 3 )]
    public DateTimeOffset NextRunTime { get; set; }

    [JsonPropertyOrder( 2 )]
    public DateTimeOffset ServiceStartTime { get; set; }

    [JsonPropertyOrder( 1 )]
    public string? State { get; set; }

    [JsonPropertyOrder( 4 )]
    public string? Version { get; set; }

    [JsonIgnore]
    internal static ApplicationStateMetrics Empty => new( string.Empty, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, string.Empty );
}
