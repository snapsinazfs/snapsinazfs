// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Interop.Zfs.sys.avl_impl;

namespace Sanoid.Interop.Zfs.libuutil.libuutil_impl;

using uu_avl_t = uu_avl;
using uu_avl_pool_t = uu_avl_pool;
using uu_avl_walk_t = uu_avl_walk;

public struct uu_avl
{
    public unsafe uu_avl_t* ua_next;
    public unsafe uu_avl_t* ua_prev;
    public unsafe uu_avl_pool_t* ua_pool;
    public unsafe void* ua_parent;
    public uint8_t ua_debug;
    public uint8_t ua_index; /* mark for uu_avl_index_ts */
    public avl_tree ua_tree;
    public uu_avl_walk_t ua_null_walk;
}
