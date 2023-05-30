// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class SnapshotRecursionMode
{
    private SnapshotRecursionKind _kind;

    public static SnapshotRecursionMode Default { get; } = new ( SnapshotRecursionKind.Default );
    public static SnapshotRecursionMode Zfs { get; } = new ( SnapshotRecursionKind.Zfs );

    private SnapshotRecursionMode( )
    {
    }
    private SnapshotRecursionMode( SnapshotRecursionKind kind )
    {
        _kind = kind;
    }

    public static implicit operator string( SnapshotRecursionMode obj )
    {
        return obj.ToString( );
    }
    public static implicit operator SnapshotRecursionMode( string value )
    {
        return value switch
        {
            "sanoid"=> Default,
            "Sanoid"=> Default,
            "zfs"=> Zfs,
            "Zfs"=> Zfs,
            "ZFS"=> Zfs,
            _=> Default
        };
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return _kind switch
        {
            SnapshotRecursionKind.Default => "default",
            SnapshotRecursionKind.Zfs => "zfs",
        };
    }
}
