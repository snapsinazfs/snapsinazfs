namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Text.Json.Serialization;

/// <summary>
///   An <see langword="enum" /> of all the possible values of a ZFS dataset property's source.
/// </summary>
/// <remarks>
///   Note that the JSON forms of these values are NOT the same as the non-JSON forms.<br />
///   JSON output writes these in all-caps, while the non-JSON output is all-lower and, in the case of <see cref="None" />, is a
///   completely different value ('-').
/// </remarks>
public enum SourceType
{
  /// <summary>
  ///   No source. This value typically means the property is a read-only calculated native property.
  /// </summary>
  [JsonStringEnumMemberName ( "NONE" )]
  None,

  /// <summary>
  ///   Default source. This value typically means the property is a read-write property that has not been explicitly set, and is also
  ///   not being inherited.
  /// </summary>
  [JsonStringEnumMemberName ( "DEFAULT" )]
  Default,

  /// <summary>
  ///   The property value is set locally, on this dataset.
  /// </summary>
  /// <remarks>Properties with this source may be inherited by other datasets.</remarks>
  [JsonStringEnumMemberName ( "LOCAL" )]
  Local,

  /// <summary>
  ///   The property value is set on this dataset, but on another system. This only appears for datasets that were created as the
  ///   result of a `zfs receive` operation.
  /// </summary>
  [JsonStringEnumMemberName ( "RECEIVED" )]
  Received,

  /// <summary>
  ///   The property value is not explicitly set on this dataset, and is being inherited from another dataset.
  /// </summary>
  /// <remarks>
  ///   The source of an inherited property is given in the <see cref="DatasetPropertySource.Data" /> property of the property.<br />
  ///   The source is not guaranteed to be in the same branch of the ZFS path hierarchy as the dataset inheriting the property.
  /// </remarks>
  [JsonStringEnumMemberName ( "INHERITED" )]
  Inherited
}
