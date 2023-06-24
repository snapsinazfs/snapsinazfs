// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    public ZfsObjectConfigurationTreeNode( string name, ZfsRecord baseDataset, ZfsRecord treeDataset, ZfsRecord? baseParentDataset = null, ZfsRecord? treeParentDataset = null )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
        TreeParentDataset = treeParentDataset ?? treeDataset;
        BaseParentDataset = baseParentDataset ?? baseDataset;
    }

    public ZfsRecord BaseDataset { get; set; }
    public ZfsRecord BaseParentDataset { get; set; }

    public bool IsModified => TreeDataset != BaseDataset;
    public ZfsRecord TreeDataset { get; set; }
    public ZfsRecord TreeParentDataset { get; set; }
}
