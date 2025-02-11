using System.Diagnostics.CodeAnalysis;

namespace SnapsInAZfs.Interop.Zfs.Native.Enums;

#pragma warning disable IDE1006 // Naming Styles
[SuppressMessage( "ReSharper", "InconsistentNaming", Justification = "Keeping names as defined in libzfs, per recommendations for native types used with P/Invoke" )]
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
