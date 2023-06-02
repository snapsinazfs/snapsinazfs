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
using Sanoid.Interop.Zfs.ZfsTypes.Properties;

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

    protected internal ZfsProperty( string propertyName, string propertyValue, ZfsPropertySource valueSource )
    {
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
        IsSanoidProperty = propertyName.StartsWith( "sanoid.net:" );
    }

    public bool IsSanoidProperty { get; }

    public bool HasValidValue
    {
        get
        {
            switch ( Name )
            {
                case DatasetLastFrequentSnapshotTimestampPropertyName:
                case DatasetLastHourlySnapshotTimestampPropertyName:
                case DatasetLastDailySnapshotTimestampPropertyName:
                case DatasetLastWeeklySnapshotTimestampPropertyName:
                case DatasetLastMonthlySnapshotTimestampPropertyName:
                case DatasetLastYearlySnapshotTimestampPropertyName:
                    return DateTimeOffset.TryParse( Value, out _ );
                case RecursionPropertyName:
                {
                    return Value switch
                    {
                        "default" => true,
                        "sanoid" => true,
                        "zfs" => true,
                        _ => false
                    };
                }
                case TemplatePropertyName:
                    return !string.IsNullOrWhiteSpace( Value ) && Value != "-";
                case EnabledPropertyName:
                case PruneSnapshotsPropertyName:
                case TakeSnapshotsPropertyName:
                    return Value == bool.FalseString || Value == bool.TrueString;
                case SnapshotRetentionFrequentPropertyName:
                    case SnapshotRetentionHourlyPropertyName:
                    case SnapshotRetentionDailyPropertyName:
                    case SnapshotRetentionWeeklyPropertyName:
                    case SnapshotRetentionMonthlyPropertyName:
                    case SnapshotRetentionYearlyPropertyName:
                    return int.TryParse( Value, out _ );
            }
            if ( IsSanoidProperty )
            {
                return Value != "-";
            }
            return true;
        }
    }

    public static (bool success, ZfsProperty? prop, string? parent) FromZfsGetLine( string zfsGetLine )
    {
        Regex parseRegex = ZfsPropertyParseRegexes.ZfsPropertyParseRegex( );
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
            return new( true, new ( groups[ "Property" ].Value, groups[ "Value" ].Value, groups[ "Source" ].Value ), groups["Name"].Value );
        }

        return ( false, null, null );
    }

    public static ImmutableDictionary<string, ZfsProperty> DefaultDatasetProperties { get; } = ImmutableDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { EnabledPropertyName, new( EnabledPropertyName, "false", "local" ) },
        { TakeSnapshotsPropertyName, new( TakeSnapshotsPropertyName, "false", "local" ) },
        { PruneSnapshotsPropertyName, new( PruneSnapshotsPropertyName, "false", "local" ) },
        { RecursionPropertyName, new( RecursionPropertyName, "default", "local" ) },
        { TemplatePropertyName, new( TemplatePropertyName, "default", "local" ) },
        { DatasetLastFrequentSnapshotTimestampPropertyName, new( DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastHourlySnapshotTimestampPropertyName, new( DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastDailySnapshotTimestampPropertyName, new( DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastWeeklySnapshotTimestampPropertyName, new( DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastMonthlySnapshotTimestampPropertyName, new( DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastYearlySnapshotTimestampPropertyName, new( DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { SnapshotRetentionFrequentPropertyName, new( SnapshotRetentionFrequentPropertyName, "0", "local" ) },
        { SnapshotRetentionHourlyPropertyName, new( SnapshotRetentionHourlyPropertyName, "48", "local" ) },
        { SnapshotRetentionDailyPropertyName, new( SnapshotRetentionDailyPropertyName, "90", "local" ) },
        { SnapshotRetentionWeeklyPropertyName, new( SnapshotRetentionWeeklyPropertyName, "0", "local" ) },
        { SnapshotRetentionMonthlyPropertyName, new( SnapshotRetentionMonthlyPropertyName, "6", "local" ) },
        { SnapshotRetentionYearlyPropertyName, new( SnapshotRetentionYearlyPropertyName, "0", "local" ) }
    } );

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
        SnapshotRetentionYearlyPropertyName,
    } );

    [JsonIgnore]
    public string Name { get; }

    [JsonIgnore]
    public ZfsPropertySource PropertySource
    {
        get => Source;
        set => Source = value;
    }

    [JsonIgnore]
    public string SetString => $"{Name}={Value}";

    public string Source { get; set; }

    [MaxLength(8192)]
    public string Value { get; set; }

    public static ImmutableSortedDictionary<string, ZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { SnapshotNamePropertyName, new( SnapshotNamePropertyName, "@@INVALID@@", (string)ZfsPropertySource.Sanoid ) },
        { SnapshotPeriodPropertyName, new(SnapshotPeriodPropertyName, "temporary", (string)ZfsPropertySource.Sanoid ) },
        { SnapshotTimestampPropertyName, new(SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( ), (string)ZfsPropertySource.Sanoid ) }
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

    public const string DatasetLastDailySnapshotTimestampPropertyName = "sanoid.net:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = "sanoid.net:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = "sanoid.net:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = "sanoid.net:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = "sanoid.net:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = "sanoid.net:lastyearlysnapshottimestamp";
    public const string SnapshotRetentionDailyPropertyName = "sanoid.net:retention:daily";
    public const string SnapshotRetentionFrequentPropertyName = "sanoid.net:retention:frequent";
    public const string SnapshotRetentionHourlyPropertyName = "sanoid.net:retention:hourly";
    public const string SnapshotRetentionMonthlyPropertyName = "sanoid.net:retention:monthly";
    public const string SnapshotRetentionWeeklyPropertyName = "sanoid.net:retention:weekly";
    public const string SnapshotRetentionYearlyPropertyName = "sanoid.net:retention:yearly";
    public const string PruneSnapshotsPropertyName = "sanoid.net:prunesnapshots";
    public const string TakeSnapshotsPropertyName = "sanoid.net:takesnapshots";
    public const string RecursionPropertyName = "sanoid.net:recursion";
    public const string TemplatePropertyName = "sanoid.net:template";
    public const string EnabledPropertyName = "sanoid.net:enabled";
    public const string SnapshotNamePropertyName = "sanoid.net:snapshotname";
    public const string SnapshotPeriodPropertyName = "sanoid.net:snapshotperiod";
    public const string SnapshotTimestampPropertyName = "sanoid.net:snapshottimestamp";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{Name}: {Value}";
    }
}
