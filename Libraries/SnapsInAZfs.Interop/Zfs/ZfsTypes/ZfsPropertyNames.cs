#region MIT LICENSE

// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyNames
{
    public const   string DatasetLastDailySnapshotTimestampPropertyName    = $"{SiazZfsPropNamespace}:lastdailysnapshottimestamp";
    public const   string DatasetLastFrequentSnapshotTimestampPropertyName = $"{SiazZfsPropNamespace}:lastfrequentsnapshottimestamp";
    public const   string DatasetLastHourlySnapshotTimestampPropertyName   = $"{SiazZfsPropNamespace}:lasthourlysnapshottimestamp";
    public const   string DatasetLastMonthlySnapshotTimestampPropertyName  = $"{SiazZfsPropNamespace}:lastmonthlysnapshottimestamp";
    public const   string DatasetLastWeeklySnapshotTimestampPropertyName   = $"{SiazZfsPropNamespace}:lastweeklysnapshottimestamp";
    public const   string DatasetLastYearlySnapshotTimestampPropertyName   = $"{SiazZfsPropNamespace}:lastyearlysnapshottimestamp";
    public const   string EnabledPropertyName                              = $"{SiazZfsPropNamespace}:enabled";
    public const   string PruneSnapshotsPropertyName                       = $"{SiazZfsPropNamespace}:prunesnapshots";
    public const   string RecursionPropertyName                            = $"{SiazZfsPropNamespace}:recursion";
    public const   string SnapshotPeriodPropertyName                       = $"{SiazZfsPropNamespace}:snapshot:period";
    public const   string SnapshotRetentionDailyPropertyName               = $"{SiazZfsPropNamespace}:retention:daily";
    public const   string SnapshotRetentionFrequentPropertyName            = $"{SiazZfsPropNamespace}:retention:frequent";
    public const   string SnapshotRetentionHourlyPropertyName              = $"{SiazZfsPropNamespace}:retention:hourly";
    public const   string SnapshotRetentionMonthlyPropertyName             = $"{SiazZfsPropNamespace}:retention:monthly";
    public const   string SnapshotRetentionPruneDeferralPropertyName       = $"{SiazZfsPropNamespace}:retention:prunedeferral";
    public const   string SnapshotRetentionWeeklyPropertyName              = $"{SiazZfsPropNamespace}:retention:weekly";
    public const   string SnapshotRetentionYearlyPropertyName              = $"{SiazZfsPropNamespace}:retention:yearly";
    public const   string SnapshotTimestampPropertyName                    = $"{SiazZfsPropNamespace}:snapshot:timestamp";
    public const   string SourceSystem                                     = $"{SiazZfsPropNamespace}:sourcesystem";
    public const   string TakeSnapshotsPropertyName                        = $"{SiazZfsPropNamespace}:takesnapshots";
    public const   string TemplatePropertyName                             = $"{SiazZfsPropNamespace}:template";
    internal const string SiazZfsPropNamespace                             = "snapsinazfs.com";
}
