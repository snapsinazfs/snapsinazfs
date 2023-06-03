// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class ZfsProperty
{
    public ZfsProperty( string propertyName, string propertyValue, string valueSource )
    {
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
        IsSanoidProperty = propertyName.StartsWith( "sanoid.net:" );
    }

    public static ImmutableDictionary<string, ZfsProperty> DefaultDatasetProperties { get; } = ImmutableDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { EnabledPropertyName, new( EnabledPropertyName, "false", ZfsPropertySourceConstants.Local ) },
        { TakeSnapshotsPropertyName, new( TakeSnapshotsPropertyName, "false", ZfsPropertySourceConstants.Local ) },
        { PruneSnapshotsPropertyName, new( PruneSnapshotsPropertyName, "false", ZfsPropertySourceConstants.Local ) },
        { RecursionPropertyName, new( RecursionPropertyName, "default", ZfsPropertySourceConstants.Local ) },
        { TemplatePropertyName, new( TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ) },
        { DatasetLastFrequentSnapshotTimestampPropertyName, new( DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), ZfsPropertySourceConstants.Local ) },
        { DatasetLastHourlySnapshotTimestampPropertyName, new( DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), ZfsPropertySourceConstants.Local ) },
        { DatasetLastDailySnapshotTimestampPropertyName, new( DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), ZfsPropertySourceConstants.Local ) },
        { DatasetLastWeeklySnapshotTimestampPropertyName, new( DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), ZfsPropertySourceConstants.Local ) },
        { DatasetLastMonthlySnapshotTimestampPropertyName, new( DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), ZfsPropertySourceConstants.Local ) },
        { DatasetLastYearlySnapshotTimestampPropertyName, new( DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), ZfsPropertySourceConstants.Local ) },
        { SnapshotRetentionFrequentPropertyName, new( SnapshotRetentionFrequentPropertyName, "0", ZfsPropertySourceConstants.Local ) },
        { SnapshotRetentionHourlyPropertyName, new( SnapshotRetentionHourlyPropertyName, "48", ZfsPropertySourceConstants.Local ) },
        { SnapshotRetentionDailyPropertyName, new( SnapshotRetentionDailyPropertyName, "90", ZfsPropertySourceConstants.Local ) },
        { SnapshotRetentionWeeklyPropertyName, new( SnapshotRetentionWeeklyPropertyName, "0", ZfsPropertySourceConstants.Local ) },
        { SnapshotRetentionMonthlyPropertyName, new( SnapshotRetentionMonthlyPropertyName, "6", ZfsPropertySourceConstants.Local ) },
        { SnapshotRetentionYearlyPropertyName, new( SnapshotRetentionYearlyPropertyName, "0", ZfsPropertySourceConstants.Local ) }
    } );

    public static ImmutableSortedDictionary<string, ZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { SnapshotNamePropertyName, new( SnapshotNamePropertyName, ZfsPropertyValueConstants.None, ZfsPropertySourceConstants.Sanoid ) },
        { SnapshotPeriodPropertyName, new( SnapshotPeriodPropertyName, ZfsPropertyValueConstants.None, ZfsPropertySourceConstants.Sanoid ) },
        { SnapshotTimestampPropertyName, new( SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( ), ZfsPropertySourceConstants.Sanoid ) }
    } );

    public bool IsSanoidProperty { get; }

    [JsonIgnore]
    public bool IsUndefined => IsSanoidProperty
                               && ( Value == ZfsPropertyValueConstants.None || Source == ZfsPropertySourceConstants.None );

    public static ImmutableSortedSet<string> KnownDatasetProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        EnabledPropertyName,
        TakeSnapshotsPropertyName,
        PruneSnapshotsPropertyName,
        RecursionPropertyName,
        TemplatePropertyName,
        DatasetLastFrequentSnapshotTimestampPropertyName,
        DatasetLastHourlySnapshotTimestampPropertyName,
        DatasetLastDailySnapshotTimestampPropertyName,
        DatasetLastWeeklySnapshotTimestampPropertyName,
        DatasetLastMonthlySnapshotTimestampPropertyName,
        DatasetLastYearlySnapshotTimestampPropertyName,
        SnapshotRetentionFrequentPropertyName,
        SnapshotRetentionHourlyPropertyName,
        SnapshotRetentionDailyPropertyName,
        SnapshotRetentionWeeklyPropertyName,
        SnapshotRetentionMonthlyPropertyName,
        SnapshotRetentionYearlyPropertyName
    } );

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        PruneSnapshotsPropertyName,
        RecursionPropertyName,
        SnapshotNamePropertyName,
        SnapshotPeriodPropertyName,
        SnapshotTimestampPropertyName,
        TemplatePropertyName,
        SnapshotRetentionDailyPropertyName,
        SnapshotRetentionFrequentPropertyName,
        SnapshotRetentionHourlyPropertyName,
        SnapshotRetentionMonthlyPropertyName,
        SnapshotRetentionWeeklyPropertyName,
        SnapshotRetentionYearlyPropertyName
    } );

    public string Name { get; }

    [JsonIgnore]
    public string SetString => $"{Name}={Value}";

    public string Source { get; set; }

    [MaxLength( 8192 )]
    public string Value { get; set; }

    public const string DatasetLastDailySnapshotTimestampPropertyName = "sanoid.net:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = "sanoid.net:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = "sanoid.net:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = "sanoid.net:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = "sanoid.net:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = "sanoid.net:lastyearlysnapshottimestamp";
    public const string EnabledPropertyName = "sanoid.net:enabled";
    public const string PruneSnapshotsPropertyName = "sanoid.net:prunesnapshots";
    public const string RecursionPropertyName = "sanoid.net:recursion";
    public const string SnapshotNamePropertyName = "sanoid.net:snapshot:name";
    public const string SnapshotPeriodPropertyName = "sanoid.net:snapshot:period";
    public const string SnapshotRetentionDailyPropertyName = "sanoid.net:retention:daily";
    public const string SnapshotRetentionFrequentPropertyName = "sanoid.net:retention:frequent";
    public const string SnapshotRetentionHourlyPropertyName = "sanoid.net:retention:hourly";
    public const string SnapshotRetentionMonthlyPropertyName = "sanoid.net:retention:monthly";
    public const string SnapshotRetentionWeeklyPropertyName = "sanoid.net:retention:weekly";
    public const string SnapshotRetentionYearlyPropertyName = "sanoid.net:retention:yearly";
    public const string SnapshotTimestampPropertyName = "sanoid.net:snapshot:timestamp";
    public const string TakeSnapshotsPropertyName = "sanoid.net:takesnapshots";
    public const string TemplatePropertyName = "sanoid.net:template";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static (bool success, ZfsProperty? prop, string? parent) FromZfsGetLine( string zfsGetLine )
    {
        Logger.Trace( "Using regex to parse new ZfsProperty from {0}", zfsGetLine );
        Regex parseRegex = ZfsPropertyParseRegexes.FullFeatured( );
        MatchCollection matches = parseRegex.Matches( zfsGetLine );
        Match firstMatch = matches[ 0 ];
        GroupCollection groups = firstMatch.Groups;

        // If the regex matched exactly once, and got all 4 of the expected capture groups, this is a good parse.
        if ( firstMatch.Success
             && groups[ "Name" ].Success
             && groups[ "Property" ].Success
             && groups[ "Value" ].Success
             && groups[ "Source" ].Success )
        {
            Logger.Trace( "Match succeeded. Returning new ZfsProperty" );
            return new( true, new( groups[ "Property" ].Value, groups[ "Value" ].Value, groups[ "Source" ].Value ), groups[ "Name" ].Value );
        }

        return ( false, null, null );
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{Name}: {Value}";
    }
}
