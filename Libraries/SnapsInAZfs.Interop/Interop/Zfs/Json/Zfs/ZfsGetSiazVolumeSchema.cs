namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

public sealed record ZfsGetSiazVolumeSchema : ZfsGetSiazDatasetSchemaBase
{
  public SiazSchemaDatasetProperties? properties { get; set; }
}
