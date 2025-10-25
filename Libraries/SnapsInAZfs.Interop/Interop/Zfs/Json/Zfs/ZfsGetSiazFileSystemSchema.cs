namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

public sealed record ZfsGetSiazFileSystemSchema : ZfsGetSiazDatasetSchemaBase
{
  public SiazSchemaDatasetProperties? properties { get; set; }
}
