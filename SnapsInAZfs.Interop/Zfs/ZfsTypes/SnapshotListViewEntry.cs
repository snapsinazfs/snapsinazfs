// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public record SnapshotListViewEntry( string ListViewText, Snapshot BaseSnapshot, Snapshot ListViewSnapshot )
{
    public bool IsModified => BaseSnapshot != ListViewSnapshot;
    public Snapshot ListViewSnapshot { get; private set; } = ListViewSnapshot;

    /// <inheritdoc />
    public override string ToString( ) => ListViewText;

    public void ResetSnapshot( )
    {
        ListViewSnapshot = BaseSnapshot with { };
    }
}
