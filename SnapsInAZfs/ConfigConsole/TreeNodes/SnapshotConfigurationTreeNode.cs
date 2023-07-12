// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

public sealed class SnapshotConfigurationTreeNode : ZfsObjectConfigurationTreeNode
{
    /// <inheritdoc />
    public SnapshotConfigurationTreeNode( string name, Snapshot baseSnapshot, Snapshot treeSnapshot ) : base( name, baseSnapshot, treeSnapshot )
    {
        BaseObject = baseSnapshot;
        TreeObject = treeSnapshot;
    }

    public Snapshot BaseObject { get; set; }

    public new bool IsModified => TreeObject != BaseObject;
    public Snapshot TreeObject { get; set; }
}
