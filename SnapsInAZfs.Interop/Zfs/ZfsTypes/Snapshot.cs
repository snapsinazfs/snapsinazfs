// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public sealed partial record Snapshot : ZfsRecord, IComparable<Snapshot>
{
    /// <summary>
    /// Creates a new snapshot with the given values and all other properties inherited from <paramref name="parentDataset"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="Snapshot"/></param>
    /// <param name="periodKind">The <see cref="SnapshotPeriodKind"/> of the <see cref="Snapshot"/></param>
    /// <param name="timestamp">The timestamp of the <see cref="Snapshot"/></param>
    /// <param name="parentDataset">The <see cref="ZfsRecord"/> this <see cref="Snapshot"/> belongs to</param>
    /// <remarks>
    /// The <see cref="ZfsRecord.Recursion">Recursion</see> and <see cref="ZfsRecord.Template">Template</see> properties will be set to local.
    /// </remarks>
    public Snapshot( string name, SnapshotPeriodKind periodKind, DateTimeOffset timestamp, ZfsRecord parentDataset )
        : this(
            name,
            parentDataset.Enabled with { IsLocal = false },
            parentDataset.TakeSnapshots with { IsLocal = false },
            parentDataset.PruneSnapshots with { IsLocal = false },
            parentDataset.LastFrequentSnapshotTimestamp with { IsLocal = false },
            parentDataset.LastHourlySnapshotTimestamp with { IsLocal = false },
            parentDataset.LastDailySnapshotTimestamp with { IsLocal = false },
            parentDataset.LastWeeklySnapshotTimestamp with { IsLocal = false },
            parentDataset.LastMonthlySnapshotTimestamp with { IsLocal = false },
            parentDataset.LastYearlySnapshotTimestamp with { IsLocal = false },
            parentDataset.Recursion with { IsLocal = true },
            parentDataset.Template with { IsLocal = true },
            parentDataset.SnapshotRetentionFrequent with { IsLocal = false },
            parentDataset.SnapshotRetentionHourly with { IsLocal = false },
            parentDataset.SnapshotRetentionDaily with { IsLocal = false },
            parentDataset.SnapshotRetentionWeekly with { IsLocal = false },
            parentDataset.SnapshotRetentionMonthly with { IsLocal = false },
            parentDataset.SnapshotRetentionYearly with { IsLocal = false },
            parentDataset.SnapshotRetentionPruneDeferral with { IsLocal = false },
            name,
            (SnapshotPeriod)periodKind,
            timestamp,
            parentDataset )
    {
    }

    /// <summary>
    /// Creates a new snapshot from all properties
    /// </summary>
    public Snapshot( string snapName, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, string snapshotName, string snapshotPeriod, DateTimeOffset snapshotTimestamp, ZfsRecord parent )
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
                parent,
                true )
    {
        _snapshotName = new( this, ZfsPropertyNames.SnapshotNamePropertyName, snapName );
        _period = new( this, ZfsPropertyNames.SnapshotPeriodPropertyName, snapshotPeriod );
        _timestamp = new( this, ZfsPropertyNames.SnapshotTimestampPropertyName, snapshotTimestamp );
    }

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

        // If timestamps are different, sort on period, by its SnapshotPeriodKind equivalent
        return !Period.Value.Equals( other.Period.Value )
            ? Period.Value.ToSnapshotPeriodKind( ).CompareTo( other.Period.Value.ToSnapshotPeriodKind( ) ) 
            : string.Compare( SnapshotName.Value, other.SnapshotName.Value, StringComparison.Ordinal );
    }

    /// <summary>
    ///     Performs a deep copy of this <see cref="Snapshot" />
    /// </summary>
    /// <param name="parent">
    ///     A reference to the parent of the new <see cref="Snapshot" />
    /// </param>
    /// <returns>
    ///     A new instance of a <see cref="ZfsRecord" />, with all properties, both reference and value, cloned to new instances
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="parent"/> is any type other than <see cref="ZfsRecord"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="parent"/> is null</exception>
    public override Snapshot DeepCopyClone( ZfsRecord? parent = null )
    {
        switch ( parent )
        {
            case null:
                throw new ArgumentNullException( nameof( parent ), "A snapshot must have a parent. Be sure to assign the cloned snapshot to the correct parent." );
            case not null when parent.GetType( ) != typeof( ZfsRecord ):
                throw new ArgumentException( "A Snapshot must have a ZfsRecord parent.", nameof( parent ) );
        }

        // Pass the original references, because the constructor will copy them and set ownership appropriately.
        Snapshot newSnapshot = new( new( Name ),
                                    Enabled,
                                    TakeSnapshots,
                                    PruneSnapshots,
                                    LastFrequentSnapshotTimestamp,
                                    LastHourlySnapshotTimestamp,
                                    LastDailySnapshotTimestamp,
                                    LastWeeklySnapshotTimestamp,
                                    LastMonthlySnapshotTimestamp,
                                    LastYearlySnapshotTimestamp,
                                    Recursion,
                                    Template,
                                    SnapshotRetentionFrequent,
                                    SnapshotRetentionHourly,
                                    SnapshotRetentionDaily,
                                    SnapshotRetentionWeekly,
                                    SnapshotRetentionMonthly,
                                    SnapshotRetentionYearly,
                                    SnapshotRetentionPruneDeferral,
                                    SnapshotName.Value,
                                    Period.Value,
                                    Timestamp.Value,
                                    parent );
        return newSnapshot;
    }

    public string GetSnapshotOptionsStringForZfsSnapshot( )
    {
        return $"-o {SnapshotName.SetString} -o {Period.SetString} -o {Timestamp.SetString} -o {Recursion.SetString}";
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{SnapshotName.Value}";
    }

    /// <inheritdoc />
    bool IEquatable<Snapshot?>.Equals( Snapshot? other )
    {
        if ( ReferenceEquals( null, other ) )
        {
            return false;
        }

        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        return _period.Equals( other._period )
               && _snapshotName.Equals( other._snapshotName )
               && _timestamp.Equals( other._timestamp )
               && _enabled.Equals( other.Enabled )
               && Kind == other.Kind
               && Name == other.Name
               && _pruneSnapshotsField.Equals( other.PruneSnapshots )
               && _recursion.Equals( other.Recursion )
               && _template.Equals( other.Template );
    }

    public bool Equals( Snapshot other )
    {
        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        return _period.Equals( other._period )
               && _snapshotName.Equals( other._snapshotName )
               && _timestamp.Equals( other._timestamp )
               && _enabled.Equals( other.Enabled )
               && Kind == other.Kind
               && Name == other.Name
               && _pruneSnapshotsField.Equals( other.PruneSnapshots )
               && _recursion.Equals( other.Recursion )
               && _template.Equals( other.Template );
    }

    /// <inheritdoc />
    public override int GetHashCode( )
    {
        return HashCode.Combine( base.GetHashCode( ), _period, _snapshotName, _timestamp );
    }
}
