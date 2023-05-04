using System.Text.Json.Serialization;

namespace Sanoid.Common.Configuration;

/// <summary>
/// Corresponds to the /Formatting/SnapshotNaming section of Sanoid.json
/// </summary>
public static class SnapshotNaming
{
    /// <summary>
    /// Gets or sets the string used to separate components of a snapshot name.
    /// </summary>
    /// <remarks>Default value is "_"</remarks>
    [JsonPropertyName("ComponentSeparator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string ComponentSeparator { get; set; } = "_";


    /// <summary>
    /// Gets or sets the string used as the first component of a snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "autosnap"
    /// </remarks>
    [JsonPropertyName("Prefix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string Prefix { get; set; } = "autosnap";


    /// <summary>
    /// Gets or sets the format string used to create the timestamp component of a snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yyyy-MM-dd_HH\:mm\:ss"<para />
    /// See <see cref="DateTimeOffset.ToString()"/> documentation for format string details.
    /// </remarks>
    [JsonPropertyName("TimestampFormatString")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string TimestampFormatString { get; set; } = "yyyy-MM-dd_HH\\:mm\\:ss";


    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Frequent">frequent</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "frequently"
    /// </remarks>
    [JsonPropertyName("FrequentSuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string FrequentSuffix { get; set; } = "frequently";


    /// <summary>
    /// Gets or sets the string used as the suffix component of an <see cref="SnapshotKind.Hourly">hourly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "hourly"
    /// </remarks>
    [JsonPropertyName("HourlySuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string HourlySuffix { get; set; } = "hourly";


    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Daily">daily</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "daily"
    /// </remarks>
    [JsonPropertyName("DailySuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string DailySuffix { get; set; } = "daily";


    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Weekly">weekly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "weekly"
    /// </remarks>
    [JsonPropertyName("WeeklySuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string WeeklySuffix { get; set; } = "weekly";


    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Monthly">monthly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "monthly"
    /// </remarks>
    [JsonPropertyName("MonthlySuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string MonthlySuffix { get; set; } = "monthly";

    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Manual">manual</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yearly"
    /// </remarks>
    [JsonPropertyName("ManualSuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string ManualSuffix { get; set; } = "manual";

    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Temporary">temporary</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yearly"
    /// </remarks>
    [JsonPropertyName("TemporarySuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string TemporarySuffix { get; set; } = "temporary";

    /// <summary>
    /// Gets or sets the string used as the suffix component of a <see cref="SnapshotKind.Yearly">yearly</see> snapshot name.
    /// </summary>
    /// <remarks>
    /// Default value is "yearly"
    /// </remarks>
    [JsonPropertyName("YearlySuffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string YearlySuffix { get; set; } = "yearly";
}