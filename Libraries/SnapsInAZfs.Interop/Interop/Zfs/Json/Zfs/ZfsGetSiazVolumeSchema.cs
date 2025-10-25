namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;

/// <summary>
///   A <see cref="ZfsGetSiazDatasetSchemaBase" /> type with <see cref="Properties" /> specific to ZFS volumes.
/// </summary>
public sealed record ZfsGetSiazVolumeSchema : ZfsGetSiazDatasetSchemaBase
{
  [JsonPropertyName ( "properties" )]
  public SiazSchemaDatasetProperties? Properties { get; set; }
}
