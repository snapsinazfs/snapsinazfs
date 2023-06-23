// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyNames
{
    public const string DatasetLastDailySnapshotTimestampPropertyName = "sanoid.net:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = "sanoid.net:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = "sanoid.net:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = "sanoid.net:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = "sanoid.net:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = "sanoid.net:lastyearlysnapshottimestamp";
    public const string EnabledPropertyName = "sanoid.net:enabled";
    public const string PruneSnapshotsPropertyName = "sanoid.net:prunesnapshots";
    public const string RecursionPropertyName = "sanoid.net:recursion";
    public const string SnapshotNamePropertyName = "sanoid.net:snapshot:name";
    public const string SnapshotPeriodPropertyName = "sanoid.net:snapshot:period";
    public const string SnapshotRetentionDailyPropertyName = "sanoid.net:retention:daily";
    public const string SnapshotRetentionFrequentPropertyName = "sanoid.net:retention:frequent";
    public const string SnapshotRetentionHourlyPropertyName = "sanoid.net:retention:hourly";
    public const string SnapshotRetentionMonthlyPropertyName = "sanoid.net:retention:monthly";
    public const string SnapshotRetentionPruneDeferralPropertyName = "sanoid.net:retention:prunedeferral";
    public const string SnapshotRetentionWeeklyPropertyName = "sanoid.net:retention:weekly";
    public const string SnapshotRetentionYearlyPropertyName = "sanoid.net:retention:yearly";
    public const string SnapshotTimestampPropertyName = "sanoid.net:snapshot:timestamp";
    public const string TakeSnapshotsPropertyName = "sanoid.net:takesnapshots";
    public const string TemplatePropertyName = "sanoid.net:template";
}
