// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.Native.Enums;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

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
    Volume = zfs_type_t.ZFS_TYPE_VOLUME,

    /// <summary>An unknown type</summary>
    /// <value>
    ///     Equivalent to <see cref="zfs_type_t.ZFS_TYPE_INVALID" /> (0)<br />
    ///     Also used for when we don't care about the type
    /// </value>
    Unknown = zfs_type_t.ZFS_TYPE_INVALID
}
