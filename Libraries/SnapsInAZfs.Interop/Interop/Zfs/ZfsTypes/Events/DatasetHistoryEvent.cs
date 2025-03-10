namespace SnapsInAZfs.Interop.Zfs.ZfsTypes.Events;

public readonly record struct DatasetHistoryEvent ( ulong EventID, string Operation, string PoolName, DateTimeOffset Timestamp, ulong TransactionGroupId, string ZfsPath ) : IZfsDatasetHistoryEvent;