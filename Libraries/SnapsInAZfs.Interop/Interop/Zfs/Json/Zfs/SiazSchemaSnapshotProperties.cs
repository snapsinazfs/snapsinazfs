// ReSharper disable StringLiteralTypo
namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;
using ZfsTypes;

[PublicAPI]
public record SiazSchemaSnapshotProperties : SiazSchemaDatasetProperties
{
  [JsonPropertyName ( ZfsPropertyNames.SnapshotPeriod )]
  public DatasetProperty<string>? SnapshotPeriod { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? SnapshotTimestamp { get; set; }

  [JsonPropertyName ( "userrefs" )]
  public DatasetProperty<int>? UserRefs { get; set; }
}
