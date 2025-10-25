namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
///   The source of a ZFS dataset zfsproperty.
/// </summary>
/// <remarks>
///   See the <see cref="SourceType" /> <see langword="enum" /> for possible values.<br />
///   This type handles conversion from the JSON string value to a <see cref="SourceType" /> value on its own, with a simple
///   <see langword="switch" /> expression.
/// </remarks>
[UsedImplicitly ( ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors, Reason = "Used by the JSON serializer." )]
public class DatasetPropertySource
{
  /// <summary>
  ///   If <see cref="Type" /> is <see cref="SourceType.Inherited" />, contains the ZFS path of the dataset from which the value of the
  ///   <see cref="DatasetProperty{T}" /> was inherited.
  /// </summary>
  /// <remarks>
  ///   The value of this property could be any dataset in the entire pool, depending on the operations that led to the creation of
  ///   this dataset. Do not assume that it is always going to be in the same branch of the tree.
  /// </remarks>
  [JsonPropertyName ( "data" )]
  public string? Data { get; set; }

  /// <summary>
  ///   Gets the value of <see cref="TypeString" /> as the corresponding <see cref="SourceType" /> <see langword="enum" /> value.
  /// </summary>
  /// <remarks>
  ///   This property is ignored by the JSON serializer.
  /// </remarks>
  [JsonIgnore]
  public SourceType Type => TypeString switch
                            {
                              "LOCAL"     => SourceType.Local,
                              "INHERITED" => SourceType.Inherited,
                              "DEFAULT"   => SourceType.Default,
                              "RECEIVED"  => SourceType.Received,
                              "NONE"      => SourceType.None,
                              _           => throw new InvalidEnumArgumentException ( $"Property source {TypeString} unrecognized." )
                            };

  [JsonPropertyName ( "type" )]
  public string? TypeString { get; set; }
}
