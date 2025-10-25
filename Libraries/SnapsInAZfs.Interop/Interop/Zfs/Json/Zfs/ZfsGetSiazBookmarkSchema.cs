namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

public sealed record ZfsGetSiazBookmarkSchema : ZfsGetSiazDatasetSchemaBase
{
  public SiazSchemaSnapshotProperties? properties { get; set; }
}
