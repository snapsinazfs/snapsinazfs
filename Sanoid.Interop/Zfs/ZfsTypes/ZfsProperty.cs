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
    public ZfsProperty( string propertyNamespace, string propertyName, string propertyValue, string valueSource, string? inheritedFrom = null )
    {
        Namespace = propertyNamespace;
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
    }

    private ZfsProperty( string[] components )
    {
        Logger.Debug( "Creating new ZfsProperty from array" );
        string[] nameComponents = components[ 0 ].Split( ':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
        switch ( nameComponents.Length )
        {
            case 2:
            {
                string namespaceComponent = nameComponents[ 0 ];
                Logger.Debug( "New property is in namespace {0}", namespaceComponent );
                Namespace = $"{namespaceComponent}:";
                Name = nameComponents[ 1 ];
                break;
            }
            default:
            {
                Logger.Debug( "New property has no namespace" );
                Namespace = string.Empty;
                Name = components[ 0 ];
                break;
            }
        }

        Value = components[ 1 ];
        Source = components[ 2 ];

        Logger.Debug( "ZfsProperty created: {0}({1})", FullName, Value );
    }

    public static ImmutableDictionary<string, ZfsProperty> DefaultProperties { get; } = ImmutableDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { "sanoid.net:template", new( "sanoid.net:", "template", "default", "sanoid" ) },
        { "sanoid.net:enabled", new( "sanoid.net:", "enabled", "false", "sanoid" ) },
        { "sanoid.net:skipchildren", new( "sanoid.net:", "skipchildren", "true", "sanoid" ) },
        { "sanoid.net:autoprune", new( "sanoid.net:", "autoprune", "false", "sanoid" ) },
        { "sanoid.net:autosnapshot", new( "sanoid.net:", "autosnapshot", "false", "sanoid" ) },
        { "sanoid.net:recursive", new( "sanoid.net:", "recursive", "false", "sanoid" ) }
    } );

    public string FullName => $"{Namespace}{Name}";

    [JsonIgnore]
    public string Name { get; }

    [JsonIgnore]
    public string Namespace { get; }

    [JsonIgnore]
    public string SetString => $"{Namespace}{Name}={Value}";

    public string Source { get; set; }
    public string Value { get; }
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{Namespace}{Name}: {Value}";
    }

    public static bool TryParse( string value, out ZfsProperty? property )
    {
        Logger.Debug( "Trying to parse new ZfsProperty from {0}", value );
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

        Logger.Debug( "Parsing ZfsProperty from {0}", value );

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
        Logger.Debug( "Parsing ZfsProperty from array [{0}]", string.Join( ',', tokens ) );
        return new( tokens );
    }
}
