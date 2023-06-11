// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class ZfsObjectConfigurationTreeViewData
{
    public ZfsObjectConfigurationTreeViewData( SanoidZfsDataset treeDataset, SanoidZfsDataset baseDataset, ITreeNode treeNode )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
        TreeNode = treeNode;
    }

    public  ITreeNode TreeNode { get; set; }
    public bool IsModified { get; set; } = false;
    public SanoidZfsDataset TreeDataset { get; set; }
    public SanoidZfsDataset BaseDataset { get; set; }
}
