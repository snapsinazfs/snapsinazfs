// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs;

/// <summary>
///     An enumeration of the type of a Dataset
/// </summary>
/// <remarks>
///     Values are a subset of <see cref="ZfsObjectKind" />
/// </remarks>
public enum DatasetKind
{
    /// <summary>A zfs filesystem</summary>
    FileSystem = ZfsObjectKind.FileSystem,

    /// <summary>A zfs zvol</summary>
    Volume = ZfsObjectKind.Volume
}
