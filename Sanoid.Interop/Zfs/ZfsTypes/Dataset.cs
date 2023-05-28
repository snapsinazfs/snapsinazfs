// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS Dataset object. Can be a filesystem or volume.
/// </summary>
public class Dataset : IZfsObject
{
    /// <summary>
    ///     Creates a new <see cref="Dataset" /> with the specified name and kind <see cref="IZfsObject" />
    /// </summary>
    /// <param name="name">The name of the new <see cref="Dataset" /></param>
    /// <param name="kind">The <see cref="DatasetKind" /> of Dataset to create</param>
    public Dataset( string name, DatasetKind kind )
    {
        Name = name;
        Kind = kind;
    }

    /// <summary>
    ///     Creates a new <see cref="Dataset" /> with the specified name with <see cref="Kind" /> set to
    ///     <see cref="DatasetKind.Unknown" />
    /// </summary>
    /// <param name="name">The name of the new <see cref="Dataset" /></param>
    public Dataset( string name )
    {
        Name = name;
        Kind = DatasetKind.Unknown;
    }

    /// <summary>
    ///     Gets the <see cref="DatasetKind" /> represented by this <see cref="Dataset" />
    /// </summary>
    public DatasetKind Kind { get; }

    public ZfsObjectKind ZfsKind => (ZfsObjectKind)Kind;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ConcurrentDictionary<string, ZfsProperty> Properties { get; } = new( );

    public static Dataset Parse( string value )
    {
        if ( string.IsNullOrWhiteSpace( value ) )
        {
            throw new ArgumentNullException( nameof( value ), "Dataset string cannot be null, empty, or only whitespace." );
        }

        string[] components = value.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

        if ( components.Length < ZfsProperty.DefaultProperties.Count )
        {
            throw new InvalidOperationException( $"String provided to Dataset.Parse must contain at least {ZfsProperty.DefaultProperties.Count} components" );
        }

        return PrivateParse( components );
    }

    private static Dataset PrivateParse( string[] components )
    {
        Dataset ds = new( components[ 0 ], components[ 1 ].ToDatasetKind( ) );
        for ( int i = 2; i < components.Length; i++ )
        {
            ZfsProperty prop = ZfsProperty.Parse( components[ i ] );
            ds.Properties[ prop.FullName ] = prop;
        }

        return ds;
    }
}
