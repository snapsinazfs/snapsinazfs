// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Zfs;

/// <summary>
///     A ZFS Dataset object. Can be a filesystem or volume.
/// </summary>
public class Dataset : IZfsObject
{
    /// <summary>
    ///     Creates a new <see cref="Dataset" /> with the specified name and parent <see cref="IZfsObject" />
    /// </summary>
    /// <param name="name">The name of the new <see cref="Dataset" /></param>
    /// <param name="parent">The parent <see cref="IZfsObject" /> of the <see cref="Dataset" /></param>
    /// <param name="kind">The kind of Dataset to create (FileSystem or ZVol)</param>
    public Dataset( string name, IZfsObject parent, DatasetKind kind )
    {
        Name = name;
        Parent = parent;
        Kind = kind;
    }

    public DatasetKind Kind { get; }

    /// <summary>
    ///     An enumeration of the type of a Dataset
    /// </summary>
    public enum DatasetKind
    {
        /// <summary>A zfs filesystem</summary>
        FileSystem = 1,

        /// <summary>A zfs zvol</summary>
        ZVol = 2
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public IZfsObject Parent { get; set; }
}
