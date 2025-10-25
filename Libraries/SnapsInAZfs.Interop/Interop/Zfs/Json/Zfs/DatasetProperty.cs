namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;

/// <summary>
///   Represents a generic zfs property of type <typeparamref name="T" />, as provided in JSON output of the <c>zfs</c> utility.
/// </summary>
/// <typeparam name="T">The .net type of the property.</typeparam>
/// <param name="Source">The source of the property value.</param>
/// <param name="Value">The value of the property.</param>
/// <remarks>
///   The type parameter <typeparamref name="T" /> should be restricted to types supported by fast-path source-generated JSON
///   serialization, which is a pretty restrictive list.
/// </remarks>
[PublicAPI]
public sealed record DatasetProperty<T> (
  [property: JsonPropertyName ( "source" )]
  DatasetPropertySource? Source,
  T Value
)
{
  /// <summary>
  ///   Gets or sets the value of the property.
  /// </summary>
  [JsonPropertyName ( "value" )]
  public T Value { get; set; } = Value;
}
