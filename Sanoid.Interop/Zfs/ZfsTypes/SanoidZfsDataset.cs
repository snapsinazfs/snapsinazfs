// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class SanoidZfsDataset : TreeNode, ITreeNode
{
    /// <exception cref="InvalidOperationException">Cannot create a non-root dataset without a parent</exception>
    internal SanoidZfsDataset( string name, string kind, SanoidZfsDataset? parent = null )
    {
        Name = name;
        Kind = kind;
        if ( parent is null )
        {
            if ( Name.LastIndexOf( '/' ) != -1 )
            {
                throw new InvalidOperationException( "Cannot create a non-root dataset without a parent" );
            }

            _parent = this;
            _root = this;
            IsPoolRoot = true;
        }
        else
        {
            _parent = parent;
            _root = parent.Root;
        }
    }

    public ZfsProperty<bool> Enabled { get; } = new( ZfsProperty.EnabledPropertyName, false, "local" );
    public bool IsPoolRoot { get; }
    public string Kind { get; private set; }
    public ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp { get; } = new( ZfsProperty.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp { get; } = new( ZfsProperty.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp { get; } = new( ZfsProperty.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp { get; } = new( ZfsProperty.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp { get; } = new( ZfsProperty.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp { get; } = new( ZfsProperty.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public string Name { get; private set; }
    private readonly SanoidZfsDataset _parent;
    public SanoidZfsDataset Parent => IsPoolRoot ? this : _parent;
    public int PathDepth => IsPoolRoot ? 0 : 1 + _parent.PathDepth;
    public ZfsProperty<bool> PruneSnapshots { get; } = new( ZfsProperty.PruneSnapshotsPropertyName, false, "local" );
    public ZfsProperty<string> Recursion { get; } = new( ZfsProperty.RecursionPropertyName, "sanoid", "local" );
    private readonly SanoidZfsDataset _root;
    public SanoidZfsDataset Root => IsPoolRoot ? this : _root;
    public ZfsProperty<int> SnapshotRetentionDaily { get; } = new( ZfsProperty.SnapshotRetentionDailyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionFrequent { get; } = new( ZfsProperty.SnapshotRetentionFrequentPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionHourly { get; } = new( ZfsProperty.SnapshotRetentionHourlyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionMonthly { get; } = new( ZfsProperty.SnapshotRetentionMonthlyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionPruneDeferral { get; } = new( ZfsProperty.SnapshotRetentionPruneDeferralPropertyName, 0, "local" );
    public ZfsProperty<int> SnapshotRetentionWeekly { get; } = new( ZfsProperty.SnapshotRetentionWeeklyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionYearly { get; } = new( ZfsProperty.SnapshotRetentionYearlyPropertyName, -1, "local" );
    public ZfsProperty<bool> TakeSnapshots { get; } = new( ZfsProperty.TakeSnapshotsPropertyName, false, "local" );
    public ZfsProperty<string> Template { get; } = new( ZfsProperty.TemplatePropertyName, "default", "local" );

    /// <inheritdoc />
    public new string Text
    {
        get => Name;
        set => Name = value;
    }

    public void UpdateProperty( string propertyName, string propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case "type":
                Kind = propertyValue;
                return;
            case ZfsProperty.EnabledPropertyName:
                Enabled.Value = bool.Parse( propertyValue );
                Enabled.Source = propertySource;
                return;
            case ZfsProperty.TakeSnapshotsPropertyName:
                TakeSnapshots.Value = bool.Parse( propertyValue );
                TakeSnapshots.Source = propertySource;
                return;
            case ZfsProperty.PruneSnapshotsPropertyName:
                PruneSnapshots.Value = bool.Parse( propertyValue );
                PruneSnapshots.Source = propertySource;
                return;
            case ZfsProperty.TemplatePropertyName:
                Template.Value = propertyValue;
                Template.Source = propertySource;
                return;
            case ZfsProperty.RecursionPropertyName:
                Recursion.Value = propertyValue;
                Recursion.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastFrequentSnapshotTimestampPropertyName:
                LastFrequentSnapshotTimestamp.Value = DateTimeOffset.Parse( propertyValue );
                LastFrequentSnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastHourlySnapshotTimestampPropertyName:
                LastHourlySnapshotTimestamp.Value = DateTimeOffset.Parse( propertyValue );
                LastHourlySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastDailySnapshotTimestampPropertyName:
                LastDailySnapshotTimestamp.Value = DateTimeOffset.Parse( propertyValue );
                LastDailySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastWeeklySnapshotTimestampPropertyName:
                LastWeeklySnapshotTimestamp.Value = DateTimeOffset.Parse( propertyValue );
                LastWeeklySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastMonthlySnapshotTimestampPropertyName:
                LastMonthlySnapshotTimestamp.Value = DateTimeOffset.Parse( propertyValue );
                LastMonthlySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastYearlySnapshotTimestampPropertyName:
                LastYearlySnapshotTimestamp.Value = DateTimeOffset.Parse( propertyValue );
                LastYearlySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionFrequentPropertyName:
                SnapshotRetentionFrequent.Value = int.Parse( propertyValue );
                SnapshotRetentionFrequent.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionHourlyPropertyName:
                SnapshotRetentionHourly.Value = int.Parse( propertyValue );
                SnapshotRetentionHourly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionDailyPropertyName:
                SnapshotRetentionDaily.Value = int.Parse( propertyValue );
                SnapshotRetentionDaily.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionWeeklyPropertyName:
                SnapshotRetentionWeekly.Value = int.Parse( propertyValue );
                SnapshotRetentionWeekly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionMonthlyPropertyName:
                SnapshotRetentionMonthly.Value = int.Parse( propertyValue );
                SnapshotRetentionMonthly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionYearlyPropertyName:
                SnapshotRetentionYearly.Value = int.Parse( propertyValue );
                SnapshotRetentionYearly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionPruneDeferralPropertyName:
                SnapshotRetentionPruneDeferral.Value = int.Parse( propertyValue );
                SnapshotRetentionPruneDeferral.Source = propertySource;
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported property" );
        }
    }

    public void UpdateProperty( string propertyName, bool propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsProperty.EnabledPropertyName:
                Enabled.Value = propertyValue;
                Enabled.Source = propertySource;
                return;
            case ZfsProperty.TakeSnapshotsPropertyName:
                TakeSnapshots.Value = propertyValue;
                TakeSnapshots.Source = propertySource;
                return;
            case ZfsProperty.PruneSnapshotsPropertyName:
                PruneSnapshots.Value = propertyValue;
                PruneSnapshots.Source = propertySource;
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" );
        }
    }

    public void UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsProperty.DatasetLastFrequentSnapshotTimestampPropertyName:
                LastFrequentSnapshotTimestamp.Value = propertyValue;
                LastFrequentSnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastHourlySnapshotTimestampPropertyName:
                LastHourlySnapshotTimestamp.Value = propertyValue;
                LastHourlySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastDailySnapshotTimestampPropertyName:
                LastDailySnapshotTimestamp.Value = propertyValue;
                LastDailySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastWeeklySnapshotTimestampPropertyName:
                LastWeeklySnapshotTimestamp.Value = propertyValue;
                LastWeeklySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastMonthlySnapshotTimestampPropertyName:
                LastMonthlySnapshotTimestamp.Value = propertyValue;
                LastMonthlySnapshotTimestamp.Source = propertySource;
                return;
            case ZfsProperty.DatasetLastYearlySnapshotTimestampPropertyName:
                LastYearlySnapshotTimestamp.Value = propertyValue;
                LastYearlySnapshotTimestamp.Source = propertySource;
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported DateTimeOffset property" );
        }
    }

    public void UpdateProperty( string propertyName, int propertyValue, string propertySource )
    {
        switch ( propertyName )
        {
            case ZfsProperty.SnapshotRetentionFrequentPropertyName:
                SnapshotRetentionFrequent.Value = propertyValue;
                SnapshotRetentionFrequent.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionHourlyPropertyName:
                SnapshotRetentionHourly.Value = propertyValue;
                SnapshotRetentionHourly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionDailyPropertyName:
                SnapshotRetentionDaily.Value = propertyValue;
                SnapshotRetentionDaily.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionWeeklyPropertyName:
                SnapshotRetentionWeekly.Value = propertyValue;
                SnapshotRetentionWeekly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionMonthlyPropertyName:
                SnapshotRetentionMonthly.Value = propertyValue;
                SnapshotRetentionMonthly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionYearlyPropertyName:
                SnapshotRetentionYearly.Value = propertyValue;
                SnapshotRetentionYearly.Source = propertySource;
                return;
            case ZfsProperty.SnapshotRetentionPruneDeferralPropertyName:
                SnapshotRetentionPruneDeferral.Value = propertyValue;
                SnapshotRetentionPruneDeferral.Source = propertySource;
                return;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" );
        }
    }

}
