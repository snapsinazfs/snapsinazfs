// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public sealed partial record Snapshot
{
    private ZfsProperty<SnapshotPeriod> _period = new( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.Frequent, ZfsPropertySourceConstants.Local );

    private ZfsProperty<string> _snapshotName;

    private ZfsProperty<DateTimeOffset> _timestamp = new( ZfsPropertyNames.SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );

    public ref readonly ZfsProperty<SnapshotPeriod> Period => ref _period;

    public ref readonly ZfsProperty<string> SnapshotName => ref _snapshotName;

    public ref readonly ZfsProperty<DateTimeOffset> Timestamp => ref _timestamp;

    /// <exception cref="OverflowException">
    ///     For <see langword="int" /> properties, <paramref name="propertyValue" /> represents
    ///     a number less than <see cref="int.MinValue" /> or greater than <see cref="int.MaxValue" />.
    /// </exception>
    public override IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotNamePropertyName:
                _snapshotName = _snapshotName with { Value = propertyValue, Source = propertySource };
                return _snapshotName;
            case ZfsPropertyNames.SnapshotPeriodPropertyName:
                return UpdateProperty( propertyName, (SnapshotPeriod)propertyValue, propertySource );
            case ZfsPropertyNames.SnapshotTimestampPropertyName:
                return UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource );
            default:
                return base.UpdateProperty( propertyName, propertyValue, propertySource );
        }
    }

    public ref readonly ZfsProperty<SnapshotPeriod> UpdateProperty( string propertyName, SnapshotPeriod propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotPeriodPropertyName:
                _period = _period with { Value = propertyValue, Source = propertySource };
                return ref _period;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported SnapshotKind property" );
        }
    }

    public override ref readonly ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotTimestampPropertyName:
                _timestamp = _timestamp with { Value = propertyValue, Source = propertySource };
                return ref _timestamp;
            default:
                return ref base.UpdateProperty( propertyName, propertyValue, propertySource );
        }
    }
}
