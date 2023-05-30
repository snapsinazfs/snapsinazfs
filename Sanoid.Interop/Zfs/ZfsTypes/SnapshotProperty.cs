// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class SnapshotProperty : ZfsProperty
{
    /// <inheritdoc />
    private SnapshotProperty( string name, string value, ZfsPropertySource source )
        : base( name, value, source )
    {
    }

    public const string NamePropertyName = "sanoid.net:snapshotname";
    public const string NamePropertyShortName = "snapshotname";
    public const string PeriodPropertyName = "sanoid.net:snapshotperiod";
    public const string PeriodPropertyShortName = "snapshotperiod";
    public const string PrunePropertyName = "sanoid.net:prunesnapshots";
    public const string PrunePropertyShortName = "prunesnapshots";
    public const string RecursionPropertyName = "sanoid.net:snapshotrecursion";
    public const string RecursionPropertyShortName = "snapshotrecursion";
    public const string TimestampPropertyName = "sanoid.net:snapshottimestamp";
    public const string TimestampPropertyShortName = "snapshottimestamp";

    public enum SnapshotPropertyKind
    {
        Name,
        Period,
        Prune,
        Recursion,
        Timestamp
    }

    public static ZfsProperty GetNewSnapshotProperty( SnapshotPropertyKind kind, string value, ZfsPropertySource source )
    {
        return kind switch
        {
            SnapshotPropertyKind.Name => new( NamePropertyShortName, value, source ),
            SnapshotPropertyKind.Period => new( PeriodPropertyShortName, value, source ),
            SnapshotPropertyKind.Prune => new( PrunePropertyShortName, value, source ),
            SnapshotPropertyKind.Recursion => new( RecursionPropertyShortName, value, source ),
            SnapshotPropertyKind.Timestamp => new( TimestampPropertyShortName, value, source ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), kind, "Invalid snapshot property kind provided" )
        };
    }
}
