// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.ComponentModel.DataAnnotations;
using Sanoid.Interop.Zfs.Libzfs_core;

namespace Sanoid.Interop.Zfs.libzfs.libzfs_impl;

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
