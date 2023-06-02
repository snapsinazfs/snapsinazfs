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
        { SnapshotNamePropertyName, new( SnapshotNamePropertyName, "@@INVALID@@", (string)ZfsPropertySource.Sanoid ) },
        { PeriodPropertyName, new(PeriodPropertyName, "temporary", (string)ZfsPropertySource.Sanoid ) },
        { TimestampPropertyName, new(TimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( ), (string)ZfsPropertySource.Sanoid ) }
    } );

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        PruneSnapshotsPropertyName,
        RecursionPropertyName,
        SnapshotNamePropertyName,
        PeriodPropertyName,
        TimestampPropertyName,
        TemplatePropertyName,
        SnapshotRetentionDailyPropertyName,
        SnapshotRetentionFrequentPropertyName,
        SnapshotRetentionHourlyPropertyName,
        SnapshotRetentionMonthlyPropertyName,
        SnapshotRetentionWeeklyPropertyName,
        SnapshotRetentionYearlyPropertyName

    } );

    public const string SnapshotNamePropertyName = "sanoid.net:snapshotname";
    public const string PeriodPropertyName = "sanoid.net:snapshotperiod";
    public const string TimestampPropertyName = "sanoid.net:snapshottimestamp";

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
            SnapshotPropertyKind.Name => new( SnapshotNamePropertyName, value, source ),
            SnapshotPropertyKind.Period => new( PeriodPropertyName, value, source ),
            SnapshotPropertyKind.Prune => new( PruneSnapshotsPropertyName, value, source ),
            SnapshotPropertyKind.Recursion => new( RecursionPropertyName, value, source ),
            SnapshotPropertyKind.Template => new( TemplatePropertyName, value, source ),
            SnapshotPropertyKind.Timestamp => new( TimestampPropertyName, value, source ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), kind, "Invalid snapshot property kind provided" )
        };
    }
}
