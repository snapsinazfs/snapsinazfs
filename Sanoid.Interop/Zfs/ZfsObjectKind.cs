// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Interop.Zfs.Native.Enums;

namespace Sanoid.Interop.Zfs;

/// <summary>
///     An enumeration used for calls to zfs list, which contains a subset of the values in <see cref="zfs_type_t" />
/// </summary>
[Flags]
public enum ZfsObjectKind
{
    /// <summary>A zfs file system (dataset)</summary>
    /// <value>Equivalent to <see cref="zfs_type_t.ZFS_TYPE_FILESYSTEM" /> (1)</value>
    FileSystem = zfs_type_t.ZFS_TYPE_FILESYSTEM,

    /// <summary>A zfs block volume (zvol)</summary>
    /// <value>Equivalent to <see cref="zfs_type_t.ZFS_TYPE_SNAPSHOT" /> (2)</value>
    Snapshot = zfs_type_t.ZFS_TYPE_SNAPSHOT,

    /// <summary>A zfs block volume (zvol)</summary>
    /// <value>Equivalent to <see cref="zfs_type_t.ZFS_TYPE_VOLUME" /> (4)</value>
    Volume = zfs_type_t.ZFS_TYPE_VOLUME
}
