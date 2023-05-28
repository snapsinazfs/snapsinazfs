// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;

namespace Sanoid.Interop.Zfs;

public class ZfsProperty
{
    public ZfsProperty( string name, string value, string source, string? inheritedFrom = null )
    {
        Name = name;
        Value = value;
        Source = source;
        InheritedFrom = inheritedFrom;
    }

    public ZfsProperty( string[] components )
    {
        Name = components[ 0 ];
        Value = components[ 1 ];
        Source = components[ 2 ];
        if ( components.Length > 3 && components[ 3 ].Length >= 16 )
        {
            InheritedFrom = components[ 3 ][ 16.. ];
        }
    }

    public string? InheritedFrom { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
    public string Value { get; set; }
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static bool TryParse( string value, out ZfsProperty? property )
    {
        property = null;
        try
        {
            property = Parse( value );
        }
        catch ( ArgumentOutOfRangeException ex )
        {
            return false;
        }
        catch ( ArgumentNullException ex )
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Gets a <see cref="ZfsProperty" /> parsed from the supplied string
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="value" /> is a null, empty, or entirely whitespace string</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the provided property string has less than 3 components separated by a
    ///     tab character.
    /// </exception>
    public static ZfsProperty Parse( string value )
    {
        if ( string.IsNullOrWhiteSpace( value ) )
        {
            const string errorString = "Unable to parse ZfsProperty. String must not be null, empty, or whitespace.";
            Logger.Error( errorString );
            throw new ArgumentNullException( nameof( value ), errorString );
        }

        string[] components = value.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
        if ( components.Length < 3 )
        {
            const string errorString = "ZfsProperty value string is invalid.";
            Logger.Error( errorString );
            throw new ArgumentOutOfRangeException( nameof( value ), errorString );
        }

        return new( components );
    }
}
