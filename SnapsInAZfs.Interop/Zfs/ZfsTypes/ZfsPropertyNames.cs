// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

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
    public const string SnapshotNamePropertyName = "snapsinazfs.com:snapshot:name";
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
