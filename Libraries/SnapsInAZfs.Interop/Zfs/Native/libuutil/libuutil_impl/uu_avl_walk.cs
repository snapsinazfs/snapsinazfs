global using int8_t = System.SByte;

namespace SnapsInAZfs.Interop.Zfs.libuutil.libuutil_impl;

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
