// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

global using int8_t = System.SByte;

namespace Sanoid.Interop.Zfs.libuutil.libuutil_impl;

using uu_avl_walk_t = uu_avl_walk;
using uu_avl_t = uu_avl;

public struct uu_avl_walk
{
    public unsafe uu_avl_walk_t* uaw_next;
    public unsafe uu_avl_walk_t* uaw_prev;
    public unsafe uu_avl_t* uaw_avl;
    public unsafe void* uaw_next_result;
    public int8_t uaw_dir;
    public uint8_t uaw_robust;
}
