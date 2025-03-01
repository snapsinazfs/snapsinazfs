using System.ComponentModel.DataAnnotations;
using SnapsInAZfs.Interop.Zfs.Libzfs_core;

namespace SnapsInAZfs.Interop.Zfs.libzfs.libzfs_impl;

public struct zpool_handle
{
    public unsafe libzfs_handle* zpool_hdl;
    public unsafe zpool_handle* zpool_next;

    [MaxLength( 256 )]
    public string zpool_name;

    public int zpool_state;
    public size_t zpool_config_size;
    public unsafe nvlist_t* zpool_config;
    public unsafe nvlist_t* zpool_old_config;
    public unsafe nvlist_t* zpool_props;
    public diskaddr_t zpool_start_block;
}
