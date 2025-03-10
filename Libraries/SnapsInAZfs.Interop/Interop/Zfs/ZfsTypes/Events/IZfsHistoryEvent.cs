namespace SnapsInAZfs.Interop.Zfs.ZfsTypes.Events;

public interface IZfsHistoryEvent
{
    ulong          EventID            { get; }
    string         Operation          { get; }
    string         PoolName           { get; }
    DateTimeOffset Timestamp          { get; }
    ulong          TransactionGroupId { get; }
}