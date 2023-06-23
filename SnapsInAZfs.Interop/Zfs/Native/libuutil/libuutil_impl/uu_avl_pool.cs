using System.ComponentModel.DataAnnotations;

namespace Sanoid.Interop.Zfs.libuutil.libuutil_impl;

using uu_avl_pool_t = uu_avl_pool;
using uu_avl_t = uu_avl;

public struct uu_avl_pool
{
    public unsafe uu_avl_pool_t* uap_next;
    public unsafe uu_avl_pool_t* uap_prev;

    [MaxLength( 64 )]
    public string uap_name;

    public size_t uap_nodeoffset;
    public size_t uap_objsize;
    public uu_compare_fn_t uap_cmp;
    public uint8_t uap_debug;
    public uint8_t uap_last_index;
    public pthread_mutex_t uap_lock; /* protects null_avl */
    public uu_avl_t uap_null_avl;
}
