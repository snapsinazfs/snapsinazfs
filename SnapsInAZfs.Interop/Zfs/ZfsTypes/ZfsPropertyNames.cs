// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyNames
{
    public const string DatasetLastDailySnapshotTimestampPropertyName = "snapsinazfs.com:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = "snapsinazfs.com:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = "snapsinazfs.com:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = "snapsinazfs.com:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = "snapsinazfs.com:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = "snapsinazfs.com:lastyearlysnapshottimestamp";
    public const string EnabledPropertyName = "snapsinazfs.com:enabled";
    public const string PruneSnapshotsPropertyName = "snapsinazfs.com:prunesnapshots";
    public const string RecursionPropertyName = "snapsinazfs.com:recursion";
    public const string SnapshotPeriodPropertyName = "snapsinazfs.com:snapshot:period";
    public const string SnapshotRetentionDailyPropertyName = "snapsinazfs.com:retention:daily";
    public const string SnapshotRetentionFrequentPropertyName = "snapsinazfs.com:retention:frequent";
    public const string SnapshotRetentionHourlyPropertyName = "snapsinazfs.com:retention:hourly";
    public const string SnapshotRetentionMonthlyPropertyName = "snapsinazfs.com:retention:monthly";
    public const string SnapshotRetentionPruneDeferralPropertyName = "snapsinazfs.com:retention:prunedeferral";
    public const string SnapshotRetentionWeeklyPropertyName = "snapsinazfs.com:retention:weekly";
    public const string SnapshotRetentionYearlyPropertyName = "snapsinazfs.com:retention:yearly";
    public const string SnapshotTimestampPropertyName = "snapsinazfs.com:snapshot:timestamp";
    public const string TakeSnapshotsPropertyName = "snapsinazfs.com:takesnapshots";
    public const string TemplatePropertyName = "snapsinazfs.com:template";
}
