namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;

public sealed record ZfsGetSiazSnapshotSchema : ZfsGetSiazDatasetSchemaBase
{
  [JsonPropertyName ( "dataset" )]
  public string? Dataset { get; set; }

  public SiazSchemaSnapshotProperties? properties { get; set; }

  [JsonPropertyName ( "snapshot_name" )]
  public string? SnapshotName { get; set; }
}
