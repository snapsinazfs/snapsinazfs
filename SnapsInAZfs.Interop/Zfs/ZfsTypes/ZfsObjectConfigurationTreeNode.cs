// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    public ZfsObjectConfigurationTreeNode( string name, ZfsRecord baseDataset, ZfsRecord treeDataset )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
        TreeSnapshots = TreeDataset.Snapshots.Values.SelectMany( periodCollection => periodCollection.Values, ( _, snap ) => new SnapshotListViewEntry( snap.Name, snap, snap with { } ) ).ToList( );
        BaseSnapshots = BaseDataset.Snapshots.Values.SelectMany( periodCollection => periodCollection.Values, ( _, snap ) => new SnapshotListViewEntry( snap.Name, snap, snap with { } ) ).ToList( );
    }

    public ZfsRecord BaseDataset { get; set; }
    public List<SnapshotListViewEntry> BaseSnapshots { get; set; }
    public bool IsModified => TreeDataset != BaseDataset;
    public ZfsRecord TreeDataset { get; set; }
    public List<SnapshotListViewEntry> TreeSnapshots { get; set; }
}
