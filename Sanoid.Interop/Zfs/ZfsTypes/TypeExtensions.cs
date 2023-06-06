// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics.CodeAnalysis;

namespace Sanoid.Interop.Zfs.ZfsTypes;

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

    /// <summary>
    ///     Gets an equivalent <see cref="DatasetKind" /> from this <see langword="string" /> value.
    /// </summary>
    /// <param name="value">The input string to convert to <see cref="DatasetKind" /></param>
    /// <returns>
    ///     A <see cref="DatasetKind" /> for the given <see langword="string" /> value, or throws a
    ///     <see cref="NotSupportedException" /> if an unsupported value is provided
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     The <paramref name="value" /> does not correspond to a supported conversion to
    ///     <see cref="DatasetKind" />
    /// </exception>
    public static DatasetKind ToDatasetKind( this string value )
    {
        return value switch
        {
            "volume" => DatasetKind.Volume,
            "filesystem" => DatasetKind.FileSystem,
            _ => throw new NotSupportedException( $"Conversion from {value} to a DatasetKind is not supported." )
        };
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
}
