// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

global using size_t = System.UInt32;

namespace SnapsInAZfs.Interop.Zfs.sys.avl_impl;

public struct avl_tree
{
    public unsafe avl_node* avl_root; /* root node in tree */

    public unsafe delegate int* avl_compar( void* l, void* r );

    public size_t avl_offset;  /* offsetof(type, avl_link_t field) */
    public ulong avl_numnodes; /* number of nodes in the tree */
}
