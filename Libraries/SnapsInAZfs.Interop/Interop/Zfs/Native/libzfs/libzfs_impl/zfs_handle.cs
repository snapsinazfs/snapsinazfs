using System.ComponentModel.DataAnnotations;
using SnapsInAZfs.Interop.Zfs.Enums;
using SnapsInAZfs.Interop.Zfs.Libzfs_core;

namespace SnapsInAZfs.Interop.Zfs.libzfs.libzfs_impl;

public struct zfs_handle
{
    public unsafe libzfs_handle* zfs_hdl;
    public unsafe zpool_handle* zpool_hdl;

    [MaxLength( 256 )]
    public string zfs_name;

    public zfs_type_t zfs_type;      /* type including snapshot */
    public zfs_type_t zfs_head_type; /* type excluding snapshot */
    public dmu_objset_stats_t zfs_dmustats;
    public unsafe nvlist_t* zfs_props;
    public unsafe nvlist_t* zfs_user_props;
    public unsafe nvlist_t* zfs_recvd_props;
    public boolean_t zfs_mntcheck;
    public unsafe char* zfs_mntopts;
    public unsafe uint8_t* zfs_props_table;
}
