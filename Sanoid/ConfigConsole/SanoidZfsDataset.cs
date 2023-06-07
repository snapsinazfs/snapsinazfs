// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Interop.Zfs.ZfsTypes;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace Sanoid.ConfigConsole;

internal sealed class SanoidZfsDataset : TreeNode, ITreeNode
{
    internal string Name { get; set; }
    internal ZfsProperty<bool> Enabled { get; set; }
    internal ZfsProperty<bool> TakeSnapshots { get; set; }
    internal ZfsProperty<bool> PruneSnapshots { get; set; }
    internal ZfsProperty<string> Template { get; set; }
    internal ZfsProperty<string> Recursion { get; set; }
    internal ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp { get; set; }
    internal ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp { get; set; }
    internal ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp { get; set; }
    internal ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp { get; set; }
    internal ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp { get; set; }
    internal ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp { get; set; }
    internal ZfsProperty<int> SnapshoRetentionFrequent { get; set; }
    internal ZfsProperty<int> SnapshoRetentionHourly { get; set; }
    internal ZfsProperty<int> SnapshoRetentionDaily { get; set; }
    internal ZfsProperty<int> SnapshoRetentionWeekly { get; set; }
    internal ZfsProperty<int> SnapshoRetentionMonthly { get; set; }
    internal ZfsProperty<int> SnapshoRetentionYearly { get; set; }
    internal ZfsProperty<int> SnapshoRetentionPruneDeferral { get; set; }

    /// <inheritdoc />
    public new string Text
    {
        get
        {
            return Name;
        }
        set
        {
            Name = value;
        }
    }
}
