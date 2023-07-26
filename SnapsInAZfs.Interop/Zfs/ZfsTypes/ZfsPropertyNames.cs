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
    public const string DatasetLastDailySnapshotTimestampPropertyName = $"{SiazNamespace}:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = $"{SiazNamespace}:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = $"{SiazNamespace}:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = $"{SiazNamespace}:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = $"{SiazNamespace}:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = $"{SiazNamespace}:lastyearlysnapshottimestamp";
    public const string EnabledPropertyName = $"{SiazNamespace}:enabled";
    public const string PruneSnapshotsPropertyName = $"{SiazNamespace}:prunesnapshots";
    public const string RecursionPropertyName = $"{SiazNamespace}:recursion";
    public const string SnapshotPeriodPropertyName = $"{SiazNamespace}:snapshot:period";
    public const string SnapshotRetentionDailyPropertyName = $"{SiazNamespace}:retention:daily";
    public const string SnapshotRetentionFrequentPropertyName = $"{SiazNamespace}:retention:frequent";
    public const string SnapshotRetentionHourlyPropertyName = $"{SiazNamespace}:retention:hourly";
    public const string SnapshotRetentionMonthlyPropertyName = $"{SiazNamespace}:retention:monthly";
    public const string SnapshotRetentionPruneDeferralPropertyName = $"{SiazNamespace}:retention:prunedeferral";
    public const string SnapshotRetentionWeeklyPropertyName = $"{SiazNamespace}:retention:weekly";
    public const string SnapshotRetentionYearlyPropertyName = $"{SiazNamespace}:retention:yearly";
    public const string SnapshotTimestampPropertyName = $"{SiazNamespace}:snapshot:timestamp";
    public const string SourceSystem = $"{SiazNamespace}:sourcesystem";
    public const string TakeSnapshotsPropertyName = $"{SiazNamespace}:takesnapshots";
    public const string TemplatePropertyName = $"{SiazNamespace}:template";
    internal const string SiazNamespace = "snapsinazfs.com";
}
