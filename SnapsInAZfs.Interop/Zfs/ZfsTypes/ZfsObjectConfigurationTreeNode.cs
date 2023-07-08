// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    public ZfsObjectConfigurationTreeNode( string name, ZfsRecord baseObject, ZfsRecord treeObject )
        : base( name )
    {
        TreeObject = treeObject;
        BaseObject = baseObject;
        TreeSnapshots = TreeObject.Snapshots.Values.SelectMany( periodCollection => periodCollection.Values, ( _, snap ) => new SnapshotListViewEntry( snap.Name, snap, snap with { } ) ).ToList( );
        BaseSnapshots = BaseObject.Snapshots.Values.SelectMany( periodCollection => periodCollection.Values, ( _, snap ) => new SnapshotListViewEntry( snap.Name, snap, snap with { } ) ).ToList( );
    }

    public ZfsRecord BaseObject { get; set; }
    public List<SnapshotListViewEntry> BaseSnapshots { get; set; }
    public bool IsModified => TreeObject != BaseObject;
    public ZfsRecord TreeObject { get; set; }
    public List<SnapshotListViewEntry> TreeSnapshots { get; set; }
}
