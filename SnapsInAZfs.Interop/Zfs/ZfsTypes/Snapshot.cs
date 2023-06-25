﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public record Snapshot : ZfsRecord, IComparable<Snapshot>
{
    public Snapshot( string name, ZfsRecord parentDataset ) : base( name, "snapshot", parentDataset.PoolRoot )
    {
        ParentDataset = parentDataset;
        SnapshotName = new( ZfsPropertyNames.SnapshotNamePropertyName, name, ZfsPropertySourceConstants.Local );
        Recursion = parentDataset.Recursion with { };
    }

    public Snapshot( string name, DateTimeOffset timestamp, ZfsRecord parentDataset ) : this( name, parentDataset )
    {
        Timestamp = new( ZfsPropertyNames.SnapshotTimestampPropertyName, timestamp, ZfsPropertySourceConstants.Local );
    }

    public Snapshot( string name, bool pruneSnapshots, SnapshotPeriod period, DateTimeOffset timestamp, ZfsRecord parentDataset ) : this( name, timestamp, parentDataset )
    {
        PruneSnapshots = new( ZfsPropertyNames.PruneSnapshotsPropertyName, pruneSnapshots, ZfsPropertySourceConstants.ZfsList );
        Period = new( ZfsPropertyNames.SnapshotPeriodPropertyName, period, ZfsPropertySourceConstants.Local );
    }

    public ZfsRecord ParentDataset { get; }

    public ZfsProperty<SnapshotPeriod> Period { get; private set; } = new( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.Frequent, ZfsPropertySourceConstants.Local );
    public ZfsProperty<string> SnapshotName { get; private set; } = new( ZfsPropertyNames.SnapshotNamePropertyName, "", ZfsPropertySourceConstants.Local );
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
    ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
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
    ///             <description>This <see cref="Snapshot" /> precedes <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of this <see cref="Snapshot" /> is null</term>
    ///             <description>This <see cref="Snapshot" /> follows <paramref name="other" /> in the sort order.</description>
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
    ///             <description>Delegate sort order to <see cref="SnapshotPeriod" />, using <see cref="Period" /> of each</description>
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
            Name.CompareTo( other.Name );
    }

    public void Deconstruct( out string name, out ZfsRecord parentDataset )
    {
        name = Name;
        parentDataset = ParentDataset;
    }

    public string GetSnapshotOptionsStringForZfsSnapshot( )
    {
        return $"-o {SnapshotName.SetString} -o {Period.SetString} -o {Timestamp.SetString} -o {Recursion.SetString}";
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
