using System.Text.Json.Serialization;

namespace Sanoid.Common.Configuration;

/// <summary>
/// Corresponds to the /Formatting/SnapshotNaming section of Sanoid.json
/// </summary>
public static class SnapshotNaming
{
    /// <summary>
    /// Gets the string used to separate components of a <see cref="Snapshot"/> name.
    /// </summary>
    /// <remarks>Default value is "_"</remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, between components.</value>
    [JsonPropertyName( "ComponentSeparator" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string ComponentSeparator => JsonConfigurationSections.SnapshotNamingConfiguration[ "ComponentSeparator" ] ?? "_";


    /// <summary>
    /// Gets the string used as the first component of a snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "autosnap"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the first component of the name.</value>
    [JsonPropertyName( "Prefix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string Prefix => JsonConfigurationSections.SnapshotNamingConfiguration[ "Prefix" ] ?? "autosnap";


    /// <summary>
    /// Gets the format string used to create the timestamp component of a snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yyyy-MM-dd_HH\:mm\:ss"<br />
    /// See <see cref="DateTimeOffset.ToString(string)"/> documentation for format string details.
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, used as the argument to <see cref="DateTimeOffset.ToString(string?)"/>.</value>
    /// <seealso cref="DateTimeOffset.ToString(string)" href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#table-of-format-specifiers"/>
    [JsonPropertyName( "TimestampFormatString" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string TimestampFormatString => JsonConfigurationSections.SnapshotNamingConfiguration[ "TimestampFormatString" ] ?? "yyyy-MM-dd_HH\\:mm\\:ss";


    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Frequent">frequent</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "frequently"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Frequent"/>.</value>
    [JsonPropertyName( "FrequentSuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string FrequentSuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "FrequentSuffix" ] ?? "frequently";


    /// <summary>
    /// Gets the string used as the suffix component of an <see cref="SnapshotPeriod.Hourly">hourly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "hourly"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Hourly"/>.</value>
    [JsonPropertyName( "HourlySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string HourlySuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "HourlySuffix" ] ?? "hourly";


    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Daily">daily</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "daily"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Daily"/>.</value>
    [JsonPropertyName( "DailySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string DailySuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "DailySuffix" ] ?? "daily";


    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Weekly">weekly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "weekly"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Weekly"/>.</value>
    [JsonPropertyName( "WeeklySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string WeeklySuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "WeeklySuffix" ] ?? "weekly";


    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Monthly">monthly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "monthly"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Monthly"/>.</value>
    [JsonPropertyName( "MonthlySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string MonthlySuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "MonthlySuffix" ] ?? "monthly";

    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Manual">manual</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yearly"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Manual"/>.</value>
    [JsonPropertyName( "ManualSuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string ManualSuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "ManualSuffix" ] ?? "manual";

    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Temporary">temporary</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yearly"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Temporary"/>.</value>
    [JsonPropertyName( "TemporarySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string TemporarySuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "TemporarySuffix" ] ?? "temporary";

    /// <summary>
    /// Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Yearly">yearly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yearly"
    /// </remarks>
    /// <value>A <see langword="string"/> value for use in <see cref="Snapshot"/> names, as the final component of the name for Snapshots with <see cref="Snapshot.Period"/> = <see cref="SnapshotPeriod.Yearly"/>.</value>
    [JsonPropertyName( "YearlySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string YearlySuffix => JsonConfigurationSections.SnapshotNamingConfiguration[ "YearlySuffix" ] ?? "yearly";
}