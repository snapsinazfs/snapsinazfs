// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.CodeDom;
using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public record SanoidZfsDataset( string Name, string Kind, bool IsPoolRoot, TreeNode ConfigConsoleTreeNode )
{
    public ZfsProperty<bool> Enabled { get; private set; } = new( ZfsPropertyNames.EnabledPropertyName, false, "local" );
    public ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<bool> PruneSnapshots { get; private set; } = new( ZfsPropertyNames.PruneSnapshotsPropertyName, false, "local" );
    public ZfsProperty<string> Recursion { get; private set; } = new( ZfsPropertyNames.RecursionPropertyName, "sanoid", "local" );
    public ZfsProperty<int> SnapshotRetentionDaily { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionFrequent { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionHourly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionMonthly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionPruneDeferral { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, "local" );
    public ZfsProperty<int> SnapshotRetentionWeekly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionYearly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, "local" );
    public ZfsProperty<bool> TakeSnapshots { get; private set; } = new( ZfsPropertyNames.TakeSnapshotsPropertyName, false, "local" );
    public ZfsProperty<string> Template { get; private set; } = new( ZfsPropertyNames.TemplatePropertyName, "default", "local" );
    public TreeNode ConfigConsoleTreeNode { get; set; } = ConfigConsoleTreeNode;

    /// <exception cref="FormatException"><paramref name="propertyValue" /> is not a valid string representation of the target property value type.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="propertyValue" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">For <see cref="DateTimeOffset"/> properties, the offset is greater than 14 hours or less than -14 hours.</exception>
    /// <exception cref="OverflowException">For <see langword="int"/> properties, <paramref name="propertyValue" /> represents a number less than <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>.</exception>
    public void UpdateProperty( string propertyName, string propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
                UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource );
                return;
            case ZfsPropertyNames.TemplatePropertyName:
                Template = Template with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.RecursionPropertyName:
                Recursion = Recursion with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName:
                UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource );
                return;
            case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
            case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
                UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource );
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported property" );
        }
    }

    public ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
                Enabled = Enabled with { Value = propertyValue, Source = propertySource };
                return Enabled;
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
                TakeSnapshots = TakeSnapshots with { Value = propertyValue, Source = propertySource };
                return TakeSnapshots;
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
                PruneSnapshots = PruneSnapshots with { Value = propertyValue, Source = propertySource };
                return PruneSnapshots;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" );
        }
    }

    public void UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName:
                LastFrequentSnapshotTimestamp = LastFrequentSnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName:
                LastHourlySnapshotTimestamp = LastHourlySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName:
                LastDailySnapshotTimestamp = LastDailySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName:
                LastWeeklySnapshotTimestamp = LastWeeklySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName:
                LastMonthlySnapshotTimestamp = LastMonthlySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName:
                LastYearlySnapshotTimestamp = LastYearlySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported DateTimeOffset property" );
        }
    }

    public void UpdateProperty( string propertyName, int propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
                SnapshotRetentionFrequent = SnapshotRetentionFrequent with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
                SnapshotRetentionHourly = SnapshotRetentionHourly with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
                SnapshotRetentionDaily = SnapshotRetentionDaily with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
                SnapshotRetentionWeekly = SnapshotRetentionWeekly with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
                SnapshotRetentionMonthly = SnapshotRetentionMonthly with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
                SnapshotRetentionYearly = SnapshotRetentionYearly with { Value = propertyValue, Source = propertySource };
                return;
            case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
                SnapshotRetentionPruneDeferral = SnapshotRetentionPruneDeferral with { Value = propertyValue, Source = propertySource };
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" );
        }
    }
}
