#region MIT LICENSE

// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

using System.Numerics;

public sealed partial record Snapshot : ZfsRecord, IComparable<Snapshot>, IEqualityOperators<Snapshot, Snapshot, bool>
{
    /// <summary>
    ///     Creates a new snapshot from all properties
    /// </summary>
    public Snapshot (
        string                         snapName,
        in ZfsProperty<bool>           enabled,
        in ZfsProperty<bool>           takeSnapshots,
        in ZfsProperty<bool>           pruneSnapshots,
        in ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp,
        in ZfsProperty<string>         recursion,
        in ZfsProperty<string>         template,
        in ZfsProperty<int>            retentionFrequent,
        in ZfsProperty<int>            retentionHourly,
        in ZfsProperty<int>            retentionDaily,
        in ZfsProperty<int>            retentionWeekly,
        in ZfsProperty<int>            retentionMonthly,
        in ZfsProperty<int>            retentionYearly,
        in ZfsProperty<int>            retentionPruneDeferral,
        in ZfsProperty<string>         sourceSystem,
        in string                      snapshotPeriod,
        in DateTimeOffset              snapshotTimestamp,
        ZfsRecord                      parent
    )
        : base (
                snapName,
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
                sourceSystem,
                0,
                0,
                parent,
                true )
    {
        if ( string.IsNullOrWhiteSpace ( sourceSystem.Value ) )
        {
            throw new ArgumentException ( "sourceSystem must have a non-null, non-whitespace-only Value", nameof (sourceSystem) );
        }

        _period    = new ( this, ZfsPropertyNames.SnapshotPeriodPropertyName, snapshotPeriod );
        _timestamp = new ( this, ZfsPropertyNames.SnapshotTimestampPropertyName, snapshotTimestamp );
    }

    /// <summary>
    ///     Creates a new snapshot with the given values and all other properties inherited from <paramref name="parentDataset"/>
    /// </summary>
    /// <param name="name">The name of the <see cref="Snapshot"/></param>
    /// <param name="periodKind">The <see cref="SnapshotPeriodKind"/> of the <see cref="Snapshot"/></param>
    /// <param name="sourceSystem">
    ///     The <see cref="ZfsRecord.SourceSystem"/> property to use for the new <see cref="Snapshot"/>
    /// </param>
    /// <param name="timestamp">The timestamp of the <see cref="Snapshot"/></param>
    /// <param name="parentDataset">The <see cref="ZfsRecord"/> this <see cref="Snapshot"/> belongs to</param>
    /// <remarks>
    ///     The <see cref="ZfsRecord.Recursion">Recursion</see> and <see cref="ZfsRecord.Template">Template</see> properties will be set
    ///     to local.
    /// </remarks>
    /// <exception cref="ArgumentException">sourceSystem must have a non-null, non-whitespace-only Value</exception>
    public Snapshot ( string name, in SnapshotPeriodKind periodKind, in ZfsProperty<string> sourceSystem, in DateTimeOffset timestamp, ZfsRecord parentDataset )
        : this (
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
                parentDataset.Recursion,
                parentDataset.Template,
                parentDataset.SnapshotRetentionFrequent,
                parentDataset.SnapshotRetentionHourly,
                parentDataset.SnapshotRetentionDaily,
                parentDataset.SnapshotRetentionWeekly,
                parentDataset.SnapshotRetentionMonthly,
                parentDataset.SnapshotRetentionYearly,
                parentDataset.SnapshotRetentionPruneDeferral,
                sourceSystem,
                (SnapshotPeriod)periodKind,
                timestamp,
                parentDataset )
    {
        if ( string.IsNullOrWhiteSpace ( sourceSystem.Value ) )
        {
            throw new ArgumentException ( "sourceSystem must have a non-null, non-whitespace-only Value", nameof (sourceSystem) );
        }
    }

    /// <summary>
    ///     Compares the current instance with another <see cref="Snapshot"/> and returns an integer that indicates
    ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
    ///     <see cref="Snapshot"/>.
    /// </summary>
    /// <param name="other">Another <see cref="Snapshot"/> to compare with this instance.</param>
    /// <returns>
    ///     A value that indicates the relative order of the <see cref="Snapshot"/>s being compared. The return value
    ///     has
    ///     these meanings:
    ///     <list type="table">
    ///         <listheader>
    ///             <term> Value</term><description> Meaning</description>
    ///         </listheader>
    ///         <item>
    ///             <term> Less than zero</term>
    ///             <description> This instance precedes <paramref name="other"/> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term> Zero</term>
    ///             <description>
    ///                 This instance occurs in the same position in the sort order as <paramref name="other"/>.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term> Greater than zero</term>
    ///             <description> This instance follows <paramref name="other"/> in the sort order.</description>
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
    ///             <term>Other <see cref="Snapshot"/> is null or has a null <see cref="Timestamp"/></term>
    ///             <description>
    ///                 This <see cref="Snapshot"/> precedes <paramref name="other"/> in the sort order.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp"/> of this <see cref="Snapshot"/> is null</term>
    ///             <description>
    ///                 This <see cref="Snapshot"/> follows <paramref name="other"/> in the sort order.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp"/> of each <see cref="Snapshot"/> is different</term>
    ///             <description>
    ///                 Sort by <see cref="Timestamp"/>, using system rules for the <see cref="DateTimeOffset"/>
    ///                 type
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period"/> of each <see cref="Snapshot"/> is different</term>
    ///             <description>
    ///                 Delegate sort order to <see cref="SnapshotPeriod"/>, using <see cref="Period"/> of each
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period"/>s of both <see cref="Snapshot"/>s are equal</term>
    ///             <description>Sort by <see cref="ZfsRecord.Name"/></description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public int CompareTo ( Snapshot? other )
    {
        // If the other snapshot is null, consider this snapshot earlier rank
        if ( other?.Timestamp is null )
        {
            return -1;
        }

        // If timestamps are different, sort on timestamps
        if ( other.Timestamp.Value != Timestamp.Value )
        {
            return Timestamp.Value.CompareTo ( other.Timestamp.Value );
        }

        // If timestamps are different, sort on period, by its SnapshotPeriodKind equivalent
        return !Period.Value.Equals ( other.Period.Value )
                   ? Period.Value.ToSnapshotPeriodKind ( ).CompareTo ( other.Period.Value.ToSnapshotPeriodKind ( ) )
                   : string.Compare ( Name, other.Name, StringComparison.Ordinal );
    }

    /// <inheritdoc/>
    bool IEquatable<Snapshot?>.Equals ( Snapshot? other )
    {
        if ( other is null )
        {
            return false;
        }

        if ( ReferenceEquals ( this, other ) )
        {
            return true;
        }

        return _period.Equals ( other._period )
            && Timestamp.Equals ( other._timestamp )
            && Enabled.Equals ( other.Enabled )
            && Kind == other.Kind
            && Name == other.Name
            && PruneSnapshots.Equals ( other.PruneSnapshots )
            && Recursion.Equals ( other.Recursion )
            && Template.Equals ( other.Template );
    }

    /// <summary>
    ///     Performs a deep copy of this <see cref="Snapshot"/>
    /// </summary>
    /// <param name="parent">
    ///     A reference to the parent of the new <see cref="Snapshot"/>. Must not be another <see cref="Snapshot"/>.
    /// </param>
    /// <returns>
    ///     A new instance of a <see cref="ZfsRecord"/>, with all properties, both reference and value, cloned to new instances
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="parent"/> is of type <see cref="Snapshot"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="parent"/> is null</exception>
    public override Snapshot DeepCopyClone ( ZfsRecord? parent = null )
    {
        switch ( parent )
        {
            case null:
                throw new ArgumentNullException ( nameof (parent), "A snapshot must have a parent. Be sure to assign the cloned snapshot to the correct parent." );
            case Snapshot:
                throw new ArgumentException ( $"Unable to clone Snapshot {Name}", nameof (parent), new NotSupportedException ( "Snapshots with parents of type Snapshot are not supported." ) );
        }

        return new (
                    Name,
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
                    SourceSystem,
                    Period.Value,
                    Timestamp.Value,
                    parent );
    }

    public bool Equals ( Snapshot other )
    {
        if ( ReferenceEquals ( this, other ) )
        {
            return true;
        }

        return _period.Equals ( other._period )
            && Timestamp.Equals ( other._timestamp )
            && Enabled.Equals ( other.Enabled )
            && Kind == other.Kind
            && Name == other.Name
            && PruneSnapshots.Equals ( other.PruneSnapshots )
            && Recursion.Equals ( other.Recursion )
            && Template.Equals ( other.Template );
    }

    /// <inheritdoc/>
    public override int GetHashCode ( ) => HashCode.Combine ( base.GetHashCode ( ), _period, Name, _timestamp );

    public string GetSnapshotOptionsStringForZfsSnapshot ( ) => $"-o {Period.SetString} -o {Timestamp.SetString} -o {Recursion.SetString} -o {SourceSystem.SetString}";

    /// <inheritdoc/>
    public override string ToString ( ) => $"{Name}";
}
