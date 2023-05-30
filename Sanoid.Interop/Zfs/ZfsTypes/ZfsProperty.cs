// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using NLog;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class ZfsProperty
{
    public const string DatasetLastDailySnapshotTimestampPropertyName = "sanoid.net:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = "sanoid.net:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = "sanoid.net:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = "sanoid.net:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = "sanoid.net:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = "sanoid.net:lastyearlysnapshottimestamp";

    public ZfsProperty( string propertyNamespace, string propertyName, string propertyValue, string valueSource )
    {
        Namespace = propertyNamespace;
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
    }

    protected internal ZfsProperty( string name, string value, ZfsPropertySource source )
    {
        Namespace = "sanoid.net:";
        Name = name;
        Value = value;
        Source = source;
    }

    private ZfsProperty( string[] components )
    {
        Logger.Trace( "Creating new ZfsProperty from array: [{0}]", string.Join( ",", components ) );
        string[] nameComponents = components[ 0 ].Split( ':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
        switch ( nameComponents.Length )
        {
            case 2:
            {
                string namespaceComponent = nameComponents[ 0 ];
                Logger.Trace( "New property is in namespace {0}", namespaceComponent );
                Namespace = $"{namespaceComponent}:";
                Name = nameComponents[ 1 ];
                break;
            }
            default:
            {
                Logger.Trace( "New property has no namespace" );
                Namespace = string.Empty;
                Name = components[ 0 ];
                break;
            }
        }

        Value = components[ 1 ];
        Source = components[ 2 ];

        Logger.Trace( "ZfsProperty created: {0}({1})", FullName, Value );
    }


    public string FullName => $"{Namespace}{Name}";

    public static ImmutableSortedSet<string> KnownDatasetProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        "sanoid.net:enabled",
        DatasetLastDailySnapshotTimestampPropertyName,
        DatasetLastFrequentSnapshotTimestampPropertyName,
        DatasetLastHourlySnapshotTimestampPropertyName,
        DatasetLastMonthlySnapshotTimestampPropertyName,
        DatasetLastWeeklySnapshotTimestampPropertyName,
        DatasetLastYearlySnapshotTimestampPropertyName,
        "sanoid.net:prunesnapshots",
        "sanoid.net:recursion",
        "sanoid.net:takesnapshots",
        "sanoid.net:template"
    } );

    [JsonIgnore]
    public string Name { get; protected init; }

    [JsonIgnore]
    public string Namespace { get; protected init; }

    [JsonIgnore]
    public ZfsPropertySource PropertySource
    {
        get => Source;
        set => Source = value;
    }

    public static ImmutableDictionary<string, ZfsProperty> DefaultDatasetProperties { get; } = ImmutableDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { "sanoid.net:template", new( "sanoid.net:", "template", "default", "local" ) },
        { "sanoid.net:enabled", new( "sanoid.net:", "enabled", "false", "local" ) },
        { DatasetLastDailySnapshotTimestampPropertyName, new( DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastFrequentSnapshotTimestampPropertyName, new( DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastHourlySnapshotTimestampPropertyName, new( DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastMonthlySnapshotTimestampPropertyName, new( DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastWeeklySnapshotTimestampPropertyName, new( DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastYearlySnapshotTimestampPropertyName, new( DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { "sanoid.net:prunesnapshots", new( "sanoid.net:", "prunesnapshots", "false", "local" ) },
        { "sanoid.net:takesnapshots", new( "sanoid.net:", "takesnapshots", "false", "local" ) },
        { "sanoid.net:recursion", new( "sanoid.net:", "recursion", "default", "local" ) }
    } );

    [JsonIgnore]
    public string SetString => $"{FullName}={Value}";

    public string Source { get; set; }
    public string Value { get; set; }
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{FullName}: {Value}";
    }

    public static bool TryParse( string value, out ZfsProperty? property )
    {
        Logger.Trace( "Trying to parse new ZfsProperty from {0}", value );
        property = null;
        try
        {
            property = Parse( value );
        }
        catch ( ArgumentOutOfRangeException ex )
        {
            Logger.Error( ex, "Error parsing new ZfsProperty from '{0}'", value );
            return false;
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Error( ex, "Error parsing new ZfsProperty from '{0}'", value );
            return false;
        }

        Logger.Debug( "Successfully parsed ZfsProperty {0}", property );

        return true;
    }

    /// <summary>
    ///     Gets a <see cref="ZfsProperty" /> parsed from the supplied string
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     If <paramref propertyName="value" /> is a null, empty, or entirely whitespace
    ///     string
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the provided property string has less than 3 components separated by a
    ///     tab character.
    /// </exception>
    /// <remarks>
    ///     Expected format is `peropertyName,value,source[,inheritedFrom]`
    /// </remarks>
    public static ZfsProperty Parse( string value )
    {
        if ( string.IsNullOrWhiteSpace( value ) )
        {
            const string errorString = "Unable to parse ZfsProperty. String must not be null, empty, or whitespace.";
            Logger.Error( errorString );
            throw new ArgumentNullException( nameof( value ), errorString );
        }

        Logger.Trace( "Parsing ZfsProperty from {0}", value );

        string[] components = value.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
        if ( components.Length < 3 )
        {
            const string errorString = "ZfsProperty value string is invalid.";
            Logger.Error( errorString );
            throw new ArgumentOutOfRangeException( nameof( value ), errorString );
        }

        return new( components );
    }

    internal static ZfsProperty Parse( string[] tokens )
    {
        Logger.Trace( "Parsing ZfsProperty from array [{0}]", string.Join( ',', tokens ) );
        return new( tokens );
    }
}
