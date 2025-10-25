namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Numerics;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using ZfsTypes;

/// <summary>
///   A type encapsulating all the ZFS user properties common to all dataset types that SIAZ uses.
/// </summary>
/// <remarks>
///   Boolean properties are present both in <see langword="string" /> and <see langword="bool" /> form as an optimization to allow
///   source-generated fast-path serialization to work.<br />
///   In code, use the <see langword="bool" /> forms of the properties. They set their corresponding <see langword="string" />
///   properties in the exact format the serializer wants to see.
/// </remarks>
[UsedImplicitly ( ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors, Reason = "Used by the JSON serializer." )]
public record SiazSchemaDatasetProperties
  : ISiazSchemaDatasetProperties,
    IEqualityOperators<SiazSchemaDatasetProperties, SiazSchemaDatasetProperties, bool>
{
  /// <summary>
  ///   Gets or sets whether the dataset is enabled for processing by SIAZ, as a string.<br />
  ///   Use the <see cref="Enabled" /> property instead.
  /// </summary>
  /// <remarks>
  ///   Source-generated JSON serialization does not handle booleans well if the value isn't all lower-case.<br />
  ///   The <see cref="IConfiguration" /> API likes to write booleans with the first character capitalized.<br />
  ///   The JSON serializer is capable of handling that in reflection mode and metadata-only mode, but not in full source-generated
  ///   fast-path serialization, so this is an optimization to cater to that.<br />
  ///   Just use the <see cref="Enabled" /> property, as it gets and sets this value for you.
  /// </remarks>
  [JsonPropertyName ( ZfsPropertyNames.Enabled )]
  public DatasetProperty<string>? EnabledString { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.PruneSnapshots )]
  public DatasetProperty<string>? PruneSnapshotsString { get; set; }

  [JsonPropertyName ( ZfsNativePropertyNames.SnapshotsChanged )]
  public DatasetProperty<long>? SnapshotsChangedUnix { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.TakeSnapshots )]
  public DatasetProperty<string>? TakeSnapshotsString { get; set; }

  /// <inheritdoc />
  /// <remarks>
  ///   This property is ignored by the JSON serializer.<br />
  ///   See <see cref="EnabledString" /> for why this exists.
  /// </remarks>
  [JsonIgnore]
  public bool Enabled
  {
    get => bool.Parse ( EnabledString?.Value ?? bool.FalseString );
    set => EnabledString?.Value = value.ToString ( );
  }

  [JsonPropertyName ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? LastDailySnapshotTimestamp { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? LastFrequentSnapshotTimestamp { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? LastHourlySnapshotTimestamp { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? LastMonthlySnapshotTimestamp { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? LastWeeklySnapshotTimestamp { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp )]
  public DatasetProperty<DateTimeOffset>? LastYearlySnapshotTimestamp { get; set; }

  [JsonIgnore]
  public bool PruneSnapshots
  {
    get => bool.Parse ( PruneSnapshotsString?.Value ?? bool.FalseString );
    set => PruneSnapshotsString?.Value = value.ToString ( );
  }

  [JsonPropertyName ( ZfsPropertyNames.Recursion )]
  public DatasetProperty<string>? Recursion { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionDaily )]
  public DatasetProperty<int>? RetentionDaily { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionFrequent )]
  public DatasetProperty<int>? RetentionFrequent { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionHourly )]
  public DatasetProperty<int>? RetentionHourly { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionMonthly )]
  public DatasetProperty<int>? RetentionMonthly { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionPruneDeferral )]
  public DatasetProperty<int>? RetentionPruneDeferral { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionWeekly )]
  public DatasetProperty<int>? RetentionWeekly { get; set; }

  [JsonPropertyName ( ZfsPropertyNames.SnapshotRetentionYearly )]
  public DatasetProperty<int>? RetentionYearly { get; set; }

  [JsonIgnore]
  public DateTimeOffset SnapshotsChanged
  {
    get => DateTimeOffset.FromUnixTimeSeconds ( SnapshotsChangedUnix?.Value ?? 0L );
    set => SnapshotsChangedUnix?.Value = value.ToUnixTimeSeconds ( );
  }

  [JsonPropertyName ( ZfsPropertyNames.SourceSystem )]
  public DatasetProperty<string>? SourceSystem { get; set; }

  [JsonIgnore]
  public bool TakeSnapshots
  {
    get => bool.Parse ( TakeSnapshotsString?.Value ?? bool.FalseString );
    set => TakeSnapshotsString?.Value = value.ToString ( );
  }

  [JsonPropertyName ( ZfsPropertyNames.Template )]
  public DatasetProperty<string>? Template { get; set; }
}
