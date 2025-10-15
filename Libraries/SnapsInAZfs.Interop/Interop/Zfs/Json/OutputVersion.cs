namespace SnapsInAZfs.Interop.Zfs.Json;

/// <summary>
///   Metadata output as the first object in zfs get -j output.
/// </summary>
/// <remarks>
///   This type is not used by SIAZ, and is included solely for JSON serialization source generator use.
/// </remarks>
public class OutputVersion
{
  /// <summary>
  ///   The command that was run to obtain the output.
  /// </summary>
  /// <remarks>
  ///   This property contains the root command and sub-command, such as <c>zfs get</c> only.<br />
  ///   Other options or arguments are not included.
  /// </remarks>
  public string? command { get; set; }

  /// <summary>
  ///   Major version.
  /// </summary>
  /// <remarks>
  ///   Not used by SIAZ.
  /// </remarks>
  public int vers_major { get; set; }

  /// <summary>
  ///   Minor version.
  /// </summary>
  /// <remarks>
  ///   Not used by SIAZ.
  /// </remarks>
  public int vers_minor { get; set; }
}
