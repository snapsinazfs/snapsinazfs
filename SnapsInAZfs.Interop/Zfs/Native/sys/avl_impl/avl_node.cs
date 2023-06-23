// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.sys.avl_impl;

public struct avl_node
{
    public unsafe avl_node* avl_child_left;  /* left/right children */
    public unsafe avl_node* avl_child_right; /* left/right children */
    public unsafe avl_node* avl_parent;      /* this node's parent */
    public ushort avl_child_index;           /* my index in parent's avl_child[] */
    public short avl_balance;                /* balance value: -1, 0, +1 */
}
