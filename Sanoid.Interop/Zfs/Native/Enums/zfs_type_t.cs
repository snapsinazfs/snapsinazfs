// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics.CodeAnalysis;

namespace Sanoid.Interop.Zfs.Native.Enums;

#pragma warning disable IDE1006 // Naming Styles
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Keeping names as defined in libzfs, per recommendations for native types used with P/Invoke")]
[Flags]
public enum zfs_type_t
{
    ZFS_TYPE_INVALID = 0,
    ZFS_TYPE_FILESYSTEM = 1 << 0,
    ZFS_TYPE_SNAPSHOT = 1 << 1,
    ZFS_TYPE_VOLUME = 1 << 2,
    ZFS_TYPE_POOL = 1 << 3,
    ZFS_TYPE_BOOKMARK = 1 << 4,
    ZFS_TYPE_VDEV = 1 << 5
}
#pragma warning restore IDE1006 // Naming Styles
