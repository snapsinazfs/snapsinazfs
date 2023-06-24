// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

/// <summary>
///     Extension methods to simplify common operations of the <see cref="ZfsObjectKind" /> <see langword="enum" />
/// </summary>
public static class TypeExtensions
{
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

    public static string ToCommaSeparatedSingleLineString( this IEnumerable<string> strings )
    {
        return string.Join( ',', strings );
    }
    public static string ToSpaceSeparatedSingleLineString( this IEnumerable<string> strings )
    {
        return string.Join( ' ', strings );
    }

    public static string ToStringForZfsSet( this IEnumerable<IZfsProperty> properties )
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

    public static bool IsWanted( this ZfsProperty<int> retentionProperty )
    {
        return retentionProperty.Value != 0;
    }
    public static bool IsNotWanted( this ZfsProperty<int> retentionProperty )
    {
        return retentionProperty.Value == 0;
    }
}
