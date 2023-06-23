// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics.CodeAnalysis;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

/// <summary>
///     Extension methods to simplify common operations of the <see cref="ZfsObjectKind" /> <see langword="enum" />
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Gets a string, suitable for use at the command line, of all flags specified
    /// </summary>
    /// <param name="value">The <see cref="ZfsObjectKind" /> value to convert</param>
    /// <returns>
    ///     A string, on a single line, with comma-separated string representations of each specified flag,
    ///     in lower case, and with no whitespace
    /// </returns>
    [SuppressMessage( "ReSharper", "ExceptionNotDocumentedOptional", Justification = "Inputs are incapable of causing these exceptions" )]
    public static string ToStringForCommandLine( this ZfsObjectKind value )
    {
        return value.ToString( ).Replace( " ", "" ).ToLower( );
    }

    /// <summary>
    ///     Gets an array of <see langword="string" />s representing each of the flags specified
    /// </summary>
    /// <param name="value">The <see cref="ZfsObjectKind" /> value to convert</param>
    /// <returns>
    ///     A string array, containing one value per specified flag, converted to lower-case using the invariant culture
    /// </returns>
    public static string[] ToStringArray( this ZfsObjectKind value )
    {
        return value.ToString( ).ToLowerInvariant( ).Split( ",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
    }

    public static string GetZfsPathRoot( this string value )
    {
        int endIndex = value.IndexOf( '/' );
        if ( endIndex == -1 )
        {
            endIndex = value.IndexOf( '@' );
        }

        if ( endIndex == -1 )
        {
            return value;
        }

        string rootPath = value[ ..endIndex ];
        return rootPath;
    }

    public static string GetZfsPathParent( this string value )
    {
        int endIndex = value.IndexOf( '@' );
        if ( endIndex == -1 )
        {
            // This means it's not a snapshot.
            // Check for the last '/' character
            endIndex = value.LastIndexOf( '/' );
        }

        if ( endIndex == -1 )
        {
            // This is a pool root.
            // Returned value is the same as input
            return value;
        }

        // This is a non-root dataset or is a snapshot
        // Return its parent dataset name
        string rootPath = value[ ..endIndex ];
        return rootPath;
    }

    public static string ToStringForZfsSet( this IEnumerable<ZfsProperty> properties )
    {
        return string.Join( ' ', properties.Select( p => p.SetString ) );
    }

    public static string ToStringForZfsSet( this IDictionary<string, IZfsProperty> properties )
    {
        return string.Join( ' ', properties.Select( kvp => kvp.Value.SetString ) );
    }

    /// <summary>
    ///     Gets a string of all <see cref="IZfsProperty.SetString" /> values, separated by spaces, to be used in zfs set
    ///     operations
    /// </summary>
    /// <param name="properties">
    ///     An <see cref="IEnumerable{T}" /> of <see cref="IZfsProperty" /> objects to get a set string
    ///     for
    /// </param>
    /// <returns></returns>
    public static string ToStringForZfsSet( this List<IZfsProperty> properties )
    {
        if ( properties is null )
        {
            throw new ArgumentNullException( nameof( properties ), "Null collection provided" );
        }

        if ( !properties.Any( ) )
        {
            throw new ArgumentException( "Empty collection provided", nameof( properties ) );
        }

        return string.Join( ' ', properties.Select( p => p.SetString ) );
    }

    public static string ToCommaSeparatedSingleLineString( this IEnumerable<string> strings )
    {
        return string.Join( ',', strings );
    }

    /// <summary>
    ///     Gets an integer index for radio button groups assuming the order of true,false,inherited from this
    ///     <see cref="ZfsProperty{T}" />
    /// </summary>
    /// <param name="property">The <see cref="ZfsProperty{T}" /> to convert to an integer index for radio button groups</param>
    /// <returns>
    ///     An <see langword="int" /> representing the index in a radio button group for this property's source<br />
    ///     0: true<br />
    ///     1: false<br />
    ///     2: inherited
    /// </returns>
    public static int AsTrueFalseRadioIndex( this ZfsProperty<bool> property )
    {
        return property.Value ? 0 : 1;
    }
}
