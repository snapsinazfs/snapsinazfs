// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    public ZfsObjectConfigurationTreeNode( string name, SnapsInAZfsZfsDataset baseDataset, SnapsInAZfsZfsDataset treeDataset, SnapsInAZfsZfsDataset? baseParentDataset = null, SnapsInAZfsZfsDataset? treeParentDataset = null )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
        TreeParentDataset = treeParentDataset ?? treeDataset;
        BaseParentDataset = baseParentDataset ?? baseDataset;
    }

    public SnapsInAZfsZfsDataset BaseDataset { get; set; }
    public SnapsInAZfsZfsDataset BaseParentDataset { get; set; }

    public bool IsModified => TreeDataset != BaseDataset;
    public SnapsInAZfsZfsDataset TreeDataset { get; set; }
    public SnapsInAZfsZfsDataset TreeParentDataset { get; set; }
}
