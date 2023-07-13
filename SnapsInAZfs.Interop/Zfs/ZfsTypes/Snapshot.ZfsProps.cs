// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using NLog;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public sealed partial record Snapshot
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
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
                return ref _snapshotName;
            case ZfsPropertyNames.SnapshotPeriodPropertyName:
                _period = _period with { Value = propertyValue, IsLocal = isLocal };
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
                return ref _timestamp;
            default:
                return ref base.UpdateProperty( propertyName, propertyValue, isLocal );
        }
    }

    /// <inheritdoc />
    protected override void OnParentUpdatedStringProperty( ZfsRecord sender, ref ZfsProperty<string> updatedProperty )
    {
        Logger.Trace( "{2} received boolean property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( this[ updatedProperty.Name ].IsInherited )
        {
            UpdateProperty( updatedProperty.Name, updatedProperty.Value, false );
        }
    }
}
