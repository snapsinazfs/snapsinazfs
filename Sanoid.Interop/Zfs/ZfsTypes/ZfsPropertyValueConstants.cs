// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.ComponentModel.DataAnnotations;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyValueConstants
{
    public const string Default = "default";
    public const string None = "-";
    public const string Sanoid = "sanoid";
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
