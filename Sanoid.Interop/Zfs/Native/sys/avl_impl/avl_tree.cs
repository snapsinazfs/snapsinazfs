// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

global using size_t = System.UInt32;

namespace Sanoid.Interop.Zfs.sys.avl_impl;

public struct avl_tree
{
    public unsafe avl_node* avl_root; /* root node in tree */

    public unsafe delegate int* avl_compar( void* l, void* r );

    public size_t avl_offset;  /* offsetof(type, avl_link_t field) */
    public ulong avl_numnodes; /* number of nodes in the tree */
}
