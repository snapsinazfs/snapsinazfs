// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public record Snapshot : ZfsRecord, IComparable<Snapshot>
{
    public Snapshot( string name, SnapshotPeriodKind periodKind, DateTimeOffset timestamp, ZfsRecord parentDataset )
        : this(
            name,
            parentDataset.Enabled,
            parentDataset.TakeSnapshots,
            parentDataset.PruneSnapshots,
            parentDataset.LastFrequentSnapshotTimestamp,
            parentDataset.LastHourlySnapshotTimestamp,
            parentDataset.LastDailySnapshotTimestamp,
            parentDataset.LastWeeklySnapshotTimestamp,
            parentDataset.LastMonthlySnapshotTimestamp,
            parentDataset.LastYearlySnapshotTimestamp,
            parentDataset.Recursion with { Source = ZfsPropertySourceConstants.Local },
            parentDataset.Template with { Source = ZfsPropertySourceConstants.Local },
            parentDataset.SnapshotRetentionFrequent,
            parentDataset.SnapshotRetentionHourly,
            parentDataset.SnapshotRetentionDaily,
            parentDataset.SnapshotRetentionWeekly,
            parentDataset.SnapshotRetentionMonthly,
            parentDataset.SnapshotRetentionYearly,
            parentDataset.SnapshotRetentionPruneDeferral,
            new( ZfsPropertyNames.SnapshotNamePropertyName, name, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotPeriodPropertyName, periodKind, ZfsPropertySourceConstants.Local ),
            new( ZfsPropertyNames.SnapshotTimestampPropertyName, timestamp, ZfsPropertySourceConstants.Local ),
            parentDataset )
    {
    }

    public Snapshot( string snapName, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, ZfsProperty<string> snapshotName, ZfsProperty<SnapshotPeriod> snapshotPeriod, ZfsProperty<DateTimeOffset> snapshotTimestamp, ZfsRecord parent )
        : base( snapName,
                ZfsPropertyValueConstants.Snapshot,
                enabled,
                takeSnapshots,
                pruneSnapshots,
                lastFrequentSnapshotTimestamp,
                lastHourlySnapshotTimestamp,
                lastDailySnapshotTimestamp,
                lastWeeklySnapshotTimestamp,
                lastMonthlySnapshotTimestamp,
                lastYearlySnapshotTimestamp,
                recursion,
                template,
                retentionFrequent,
                retentionHourly,
                retentionDaily,
                retentionWeekly,
                retentionMonthly,
                retentionYearly,
                retentionPruneDeferral,
                0,
                0,
                parent )
    {
        SnapshotName = snapshotName;
        Period = snapshotPeriod;
        Timestamp = snapshotTimestamp;
    }

    public ZfsProperty<SnapshotPeriod> Period { get; private set; } = new( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.Frequent, ZfsPropertySourceConstants.Local );
    public ZfsProperty<string> SnapshotName { get; private set; }
    public ZfsProperty<DateTimeOffset> Timestamp { get; private set; } = new( ZfsPropertyNames.SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );

    /// <summary>
    ///     Compares the current instance with another <see cref="Snapshot" /> and returns an integer that indicates
    ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
    ///     <see cref="Snapshot" />.
    /// </summary>
    /// <param name="other">Another <see cref="Snapshot" /> to compare with this instance.</param>
    /// <returns>
    ///     A value that indicates the relative order of the <see cref="Snapshot" />s being compared. The return value
    ///     has
    ///     these meanings:
    ///     <list type="table">
    ///         <listheader>
    ///             <term> Value</term><description> Meaning</description>
    ///         </listheader>
    ///         <item>
    ///             <term> Less than zero</term>
    ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term> Zero</term>
    ///             <description>
    ///                 This instance occurs in the same position in the sort order as <paramref name="other" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term> Greater than zero</term>
    ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Sort order is as follows:
    ///     <list type="number">
    ///         <listheader>
    ///             <term>Condition</term><description>Result</description>
    ///         </listheader>
    ///         <item>
    ///             <term>Other <see cref="Snapshot" /> is null or has a null <see cref="Timestamp" /></term>
    ///             <description>
    ///                 This <see cref="Snapshot" /> precedes <paramref name="other" /> in the sort order.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of this <see cref="Snapshot" /> is null</term>
    ///             <description>
    ///                 This <see cref="Snapshot" /> follows <paramref name="other" /> in the sort order.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of each <see cref="Snapshot" /> is different</term>
    ///             <description>
    ///                 Sort by <see cref="Timestamp" />, using system rules for the <see cref="DateTimeOffset" />
    ///                 type
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period" /> of each <see cref="Snapshot" /> is different</term>
    ///             <description>
    ///                 Delegate sort order to <see cref="SnapshotPeriod" />, using <see cref="Period" /> of each
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period" />s of both <see cref="Snapshot" />s are equal</term>
    ///             <description>Sort by <see cref="ZfsRecord.Name" /></description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public int CompareTo( Snapshot? other )
    {
        // If the other snapshot is null, consider this snapshot earlier rank
        if ( other?.Timestamp is null )
        {
            return -1;
        }

        // If timestamps are different, sort on timestamps
        if ( other.Timestamp.Value != Timestamp.Value )
        {
            return Timestamp.Value.CompareTo( other.Timestamp.Value );
        }

        // If timestamps are different, sort on period
        return !Period.Value.Equals( other.Period.Value )
            ? Period.Value.CompareTo( other.Period.Value )
            :
            // If periods are the same, sort by name
            String.Compare( Name, other.Name, StringComparison.Ordinal );
    }

    public string GetSnapshotOptionsStringForZfsSnapshot( )
    {
        return $"-o {SnapshotName.SetString} -o {Period.SetString} -o {Timestamp.SetString} -o {Recursion.SetString}";
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{SnapshotName}";
    }

    public new IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotNamePropertyName => SnapshotName = SnapshotName with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotPeriodPropertyName => UpdateProperty( propertyName, (SnapshotPeriod)propertyValue, propertySource ),
            ZfsPropertyNames.SnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            _ => base.UpdateProperty( propertyName, propertyValue, propertySource )
        };
    }

    public ZfsProperty<SnapshotPeriod> UpdateProperty( string propertyName, SnapshotPeriod propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotPeriodPropertyName => Period = Period with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported SnapshotKind property" )
        };
    }

    public new ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotTimestampPropertyName => Timestamp = Timestamp with { Value = propertyValue, Source = propertySource },
            _ => base.UpdateProperty( propertyName, propertyValue, propertySource )
        };
    }
}
