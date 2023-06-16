// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    public ZfsObjectConfigurationTreeNode( string name, SanoidZfsDataset baseDataset, SanoidZfsDataset treeDataset, SanoidZfsDataset? baseParentDataset = null, SanoidZfsDataset? treeParentDataset = null )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
        TreeParentDataset = treeParentDataset ?? treeDataset;
        BaseParentDataset = baseParentDataset ?? baseDataset;
    }

    public SanoidZfsDataset BaseDataset { get; set; }
    public SanoidZfsDataset BaseParentDataset { get; set; }

    public bool IsModified => TreeDataset != BaseDataset;
    public SanoidZfsDataset TreeDataset { get; set; }
    public SanoidZfsDataset TreeParentDataset { get; set; }
}
