// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyValueConstants
{
    public const string Default = "default";
    public const string None = "-";
    public const string SnapsInAZfs = "siaz";
    public const string ZfsRecursion = "zfs";

    public static readonly Dictionary<string, (int Min, int Max)> IntPropertyRanges = new( )
    {
        { ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionDailyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, new( 0, 100 ) }
    };
}
