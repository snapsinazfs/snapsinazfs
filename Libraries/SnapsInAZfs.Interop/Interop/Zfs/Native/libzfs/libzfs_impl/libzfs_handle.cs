global using uint8_t = System.Byte;
using System.ComponentModel.DataAnnotations;
using SnapsInAZfs.Interop.Zfs.Enums;
using SnapsInAZfs.Interop.Zfs.libuutil.libuutil_impl;
using SnapsInAZfs.Interop.Zfs.sys.avl_impl;

namespace SnapsInAZfs.Interop.Zfs.libzfs.libzfs_impl;

using zpool_handle_t = zpool_handle;
using uu_avl_pool_t = uu_avl_pool;
using uu_avl_t = uu_avl;
using avl_tree_t = avl_tree;

public struct libzfs_handle
{
    public int libzfs_error;
    public int libzfs_fd;
    public unsafe zpool_handle_t* libzfs_pool_handles;
    public unsafe uu_avl_pool_t* libzfs_ns_avlpool;
    public unsafe uu_avl_t* libzfs_ns_avl;
    public uint64_t libzfs_ns_gen;
    public int libzfs_desc_active;

    [MaxLength( 1024 )]
    public string libzfs_action;

    [MaxLength( 1024 )]
    public string libzfs_desc;

    public int libzfs_printerr;

    public boolean_t libzfs_mnttab_enable;

    public pthread_mutex_t libzfs_mnttab_cache_lock;
    public avl_tree_t libzfs_mnttab_cache;
    public int libzfs_pool_iter;
    public boolean_t libzfs_prop_debug;
    public regex_t libzfs_urire;
    public uint64_t libzfs_max_nvlist;
    public unsafe void* libfetch;
    public unsafe char* libfetch_load_error;
}
