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
    public ZfsProperty( string propertyName, string propertyValue, string valueSource )
    {
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
    }

    protected internal ZfsProperty( string propertyName, string propertyValue, ZfsPropertySource valueSource )
    {
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
    }

    private ZfsProperty( string[] components )
    {
        Logger.Trace( "Creating new ZfsProperty from array: [{0}]", string.Join( ",", components ) );
        
        Name = components[ 0 ];
        Value = components[ 1 ];
        Source = components[ 2 ];

        Logger.Trace( "ZfsProperty created: {0}({1})", Name, Value );
    }

    public static ImmutableDictionary<string, ZfsProperty> DefaultDatasetProperties { get; } = ImmutableDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { TemplatePropertyName, new( TemplatePropertyName, "default", "local" ) },
        { EnabledPropertyName, new( EnabledPropertyName, "false", "local" ) },
        { DatasetLastDailySnapshotTimestampPropertyName, new( DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastFrequentSnapshotTimestampPropertyName, new( DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastHourlySnapshotTimestampPropertyName, new( DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastMonthlySnapshotTimestampPropertyName, new( DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastWeeklySnapshotTimestampPropertyName, new( DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { DatasetLastYearlySnapshotTimestampPropertyName, new( DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch.ToString( "O" ), "local" ) },
        { PruneSnapshotsPropertyName, new( PruneSnapshotsPropertyName, "false", "local" ) },
        { TakeSnapshotsPropertyName, new( TakeSnapshotsPropertyName, "false", "local" ) },
        { RecursionPropertyName, new( RecursionPropertyName, "default", "local" ) }
    } );

    public static ImmutableSortedSet<string> KnownDatasetProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        EnabledPropertyName,
        DatasetLastDailySnapshotTimestampPropertyName,
        DatasetLastFrequentSnapshotTimestampPropertyName,
        DatasetLastHourlySnapshotTimestampPropertyName,
        DatasetLastMonthlySnapshotTimestampPropertyName,
        DatasetLastWeeklySnapshotTimestampPropertyName,
        DatasetLastYearlySnapshotTimestampPropertyName,
        PruneSnapshotsPropertyName,
        RecursionPropertyName,
        TakeSnapshotsPropertyName,
        TemplatePropertyName
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
    public string Value { get; set; }
    public const string DatasetLastDailySnapshotTimestampPropertyName = "sanoid.net:lastdailysnapshottimestamp";
    public const string DatasetLastFrequentSnapshotTimestampPropertyName = "sanoid.net:lastfrequentsnapshottimestamp";
    public const string DatasetLastHourlySnapshotTimestampPropertyName = "sanoid.net:lasthourlysnapshottimestamp";
    public const string DatasetLastMonthlySnapshotTimestampPropertyName = "sanoid.net:lastmonthlysnapshottimestamp";
    public const string DatasetLastWeeklySnapshotTimestampPropertyName = "sanoid.net:lastweeklysnapshottimestamp";
    public const string DatasetLastYearlySnapshotTimestampPropertyName = "sanoid.net:lastyearlysnapshottimestamp";
    public const string PruneSnapshotsPropertyName = "sanoid.net:prunesnapshots";
    public const string TakeSnapshotsPropertyName = "sanoid.net:takesnapshots";
    public const string RecursionPropertyName = "sanoid.net:recursion";
    public const string TemplatePropertyName = "sanoid.net:template";
    public const string EnabledPropertyName = "sanoid.net:enabled";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{Name}: {Value}";
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
        switch ( components.Length )
        {
            case < 3:
            {
                const string errorString = "ZfsProperty value string is invalid.";
                Logger.Error( errorString );
                throw new ArgumentOutOfRangeException( nameof( value ), errorString );
            }
            default:
                return new( components );
        }
    }

    internal static ZfsProperty Parse( string[] tokens )
    {
        Logger.Trace( "Parsing ZfsProperty from array [{0}]", string.Join( ',', tokens ) );
        return new( tokens );
    }
}
