namespace SnapsInAZfs.Interop.Zfs.ZfsTypes.Events;

public interface IZfsDatasetHistoryEvent : IZfsHistoryEvent
{
    string ZfsPath { get; }
}