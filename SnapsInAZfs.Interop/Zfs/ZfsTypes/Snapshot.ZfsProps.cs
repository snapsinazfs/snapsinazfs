// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public sealed partial record Snapshot
{
    private ZfsProperty<string> _period;

    private ZfsProperty<string> _snapshotName;

    private ZfsProperty<DateTimeOffset> _timestamp;

    public ref readonly ZfsProperty<string> Period => ref _period;

    public ref readonly ZfsProperty<string> SnapshotName => ref _snapshotName;

    public ref readonly ZfsProperty<DateTimeOffset> Timestamp => ref _timestamp;

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public override ref readonly ZfsProperty<string> UpdateProperty( string propertyName, string propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotNamePropertyName:
                _snapshotName = _snapshotName with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke( this, ref _snapshotName );
                return ref _snapshotName;
            case ZfsPropertyNames.SnapshotPeriodPropertyName:
                _period = _period with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke( this, ref _period );
                return ref _period;
            default:
                return ref base.UpdateProperty( propertyName, propertyValue, isLocal );
        }
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public override ref readonly ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotTimestampPropertyName:
                _timestamp = _timestamp with { Value = propertyValue, IsLocal = isLocal };
                DateTimeOffsetPropertyChanged?.Invoke( this, ref _timestamp );
                return ref _timestamp;
            default:
                return ref base.UpdateProperty( propertyName, propertyValue, isLocal );
        }
    }

    /// <inheritdoc cref="ZfsRecord.StringPropertyChanged" />
    public new event StringPropertyChangedEventHandler? StringPropertyChanged;
    public new event DateTimeOffsetPropertyChangedEventHandler? DateTimeOffsetPropertyChanged;
    public new delegate void StringPropertyChangedEventHandler(Snapshot sender,ref ZfsProperty<string> property );
    public new delegate void DateTimeOffsetPropertyChangedEventHandler(Snapshot sender,ref ZfsProperty<DateTimeOffset> property );
}
