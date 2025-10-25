// ReSharper disable StringLiteralTypo
namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;

[PublicAPI]
public record SiazSchemaSnapshotProperties : SiazSchemaDatasetProperties
{
  [JsonPropertyName ( "snapsinazfs.com:snapshot:period" )]
  public DatasetProperty<string>? snapshotPeriod { get; set; }

  [JsonPropertyName ( "snapsinazfs.com:snapshot:timestamp" )]
  public DatasetProperty<DateTimeOffset>? snapshotTimestamp { get; set; }

  [JsonPropertyName ( "userrefs" )]
  public DatasetProperty<int> UserRefs { get; set; }
}
