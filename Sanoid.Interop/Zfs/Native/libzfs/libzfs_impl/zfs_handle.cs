// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.ComponentModel.DataAnnotations;
using Sanoid.Interop.Zfs.Enums;
using Sanoid.Interop.Zfs.Libzfs_core;

namespace Sanoid.Interop.Zfs.libzfs.libzfs_impl;

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
