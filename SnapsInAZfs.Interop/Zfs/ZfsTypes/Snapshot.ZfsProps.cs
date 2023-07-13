// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using NLog;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public sealed partial record Snapshot
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private readonly ZfsProperty<string> _period;

    private readonly ZfsProperty<string> _snapshotName;

    private ZfsProperty<DateTimeOffset> _timestamp;

    public ref readonly ZfsProperty<string> Period => ref _period;

    public ref readonly ZfsProperty<string> SnapshotName => ref _snapshotName;

    public ref readonly ZfsProperty<DateTimeOffset> Timestamp => ref _timestamp;

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If an attempt is made to change the SnapshotName property</exception>
    public override ref readonly ZfsProperty<string> UpdateProperty( string propertyName, string propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotNamePropertyName:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), "Snapshot name cannot be changed." );
            case ZfsPropertyNames.SnapshotPeriodPropertyName:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), "Snapshot period cannot be changed." );
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
