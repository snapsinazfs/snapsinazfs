// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Sanoid.Interop.Zfs.ZfsTypes;

[UsedImplicitly]
public class SnapshotProperty : ZfsProperty
{
    /// <inheritdoc />
    private SnapshotProperty( string name, string value, ZfsPropertySource source )
        : base( name, value, source )
    {
    }

    public static ImmutableSortedDictionary<string, ZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { "sanoid.net:snapshotname", new( "sanoid.net:", "snapshotname", "@@INVALID@@", ZfsPropertySource.Sanoid ) },
        { "sanoid.net:snapshotperiod", new( "sanoid.net:", "snapshotperiod", "temporary", ZfsPropertySource.Sanoid ) },
        { "sanoid.net:snapshottimestamp", new( "sanoid.net:", "snapshottimestamp", DateTimeOffset.MinValue.ToString( ), ZfsPropertySource.Sanoid ) }
    } );

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        "sanoid.net:prunesnapshots",
        "sanoid.net:recursion",
        "sanoid.net:snapshotname",
        "sanoid.net:snapshotperiod",
        "sanoid.net:snapshottimestamp",
        "sanoid.net:template"
    } );

    public const string NamePropertyName = "sanoid.net:snapshotname";
    public const string NamePropertyShortName = "snapshotname";
    public const string PeriodPropertyName = "sanoid.net:snapshotperiod";
    public const string PeriodPropertyShortName = "snapshotperiod";
    public const string PrunePropertyName = "sanoid.net:prunesnapshots";
    public const string PrunePropertyShortName = "prunesnapshots";
    public const string RecursionPropertyName = "sanoid.net:recursion";
    public const string RecursionPropertyShortName = "recursion";
    public const string TemplatePropertyName = "sanoid.net:template";
    public const string TemplatePropertyShortName = "template";
    public const string TimestampPropertyName = "sanoid.net:snapshottimestamp";
    public const string TimestampPropertyShortName = "snapshottimestamp";

    public enum SnapshotPropertyKind
    {
        Name,
        Period,
        Prune,
        Recursion,
        Template,
        Timestamp
    }

    public static SnapshotProperty GetNewSnapshotProperty( SnapshotPropertyKind kind, string value, ZfsPropertySource source )
    {
        return kind switch
        {
            SnapshotPropertyKind.Name => new( NamePropertyShortName, value, source ),
            SnapshotPropertyKind.Period => new( PeriodPropertyShortName, value, source ),
            SnapshotPropertyKind.Prune => new( PrunePropertyShortName, value, source ),
            SnapshotPropertyKind.Recursion => new( RecursionPropertyShortName, value, source ),
            SnapshotPropertyKind.Template => new( TemplatePropertyShortName, value, source ),
            SnapshotPropertyKind.Timestamp => new( TimestampPropertyShortName, value, source ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), kind, "Invalid snapshot property kind provided" )
        };
    }
}
