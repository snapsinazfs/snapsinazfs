namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;
using ZfsTypes;

/// <summary>
///   A <see cref="ZfsGetSiazDatasetSchemaBase" /> type with <see cref="Properties" /> specific to ZFS volumes.
/// </summary>
public sealed record ZfsGetSiazSnapshotSchema : ZfsGetSiazDatasetSchemaBase
{
  [JsonPropertyName ( ZfsNativePropertyNames.Dataset )]
  public string? Dataset { get; set; }

  public SiazSchemaSnapshotProperties? Properties { get; set; }

  [JsonPropertyName ( ZfsNativePropertyNames.SnapshotName )]
  public string? SnapshotName { get; set; }
}
