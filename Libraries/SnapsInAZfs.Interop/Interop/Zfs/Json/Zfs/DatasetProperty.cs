namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;

[PublicAPI]
public sealed record DatasetProperty<T> (
  [property: JsonPropertyName ( "source" )]
  DatasetPropertySource? Source,
  T Value
)
{
  [JsonPropertyName ( "value" )]
  public T Value { get; set; } = Value;
}
