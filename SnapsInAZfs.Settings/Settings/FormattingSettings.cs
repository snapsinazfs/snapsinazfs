// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     Settings used for naming of snapshots
/// </summary>
public record FormattingSettings
{
    /// <summary>
    ///     Gets or sets the string used to separate components of a snapshot name
    /// </summary>
    [JsonPropertyOrder( 1 )]
    public required string ComponentSeparator { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the end of a daily snapshot name
    /// </summary>
    [JsonPropertyOrder( 6 )]
    public required string DailySuffix { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the end of a frequent snapshot name
    /// </summary>
    [JsonPropertyOrder( 4 )]
    public required string FrequentSuffix { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the end of an hourly snapshot name
    /// </summary>
    [JsonPropertyOrder( 5 )]
    public required string HourlySuffix { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the end of a monthly snapshot name
    /// </summary>
    [JsonPropertyOrder( 8 )]
    public required string MonthlySuffix { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the beginning of a snapshot name
    /// </summary>
    [JsonPropertyOrder( 2 )]
    public required string Prefix { get; init; }

    /// <summary>
    ///     Gets or sets the format string used in the <see cref="DateTimeOffset.ToString()" /> method, for the timestamp
    ///     portion of a snapshot name
    /// </summary>
    [JsonPropertyOrder( 3 )]
    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    public required string TimestampFormatString { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the end of a weekly snapshot name
    /// </summary>
    [JsonPropertyOrder( 7 )]
    public required string WeeklySuffix { get; init; }

    /// <summary>
    ///     Gets or sets the string used at the end of a yearly snapshot name
    /// </summary>
    [JsonPropertyOrder( 9 )]
    public required string YearlySuffix { get; init; }

    /// <summary>
    ///     Gets a fully-qualified zfs name for a snapshot, using the given <paramref name="datasetName" />,
    ///     <paramref name="periodKind" />, and <paramref name="timestamp" />, in conjunction with configured settings for this
    ///     object
    /// </summary>
    public string GenerateFullSnapshotName( string datasetName, SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        return $"{datasetName}@{GenerateShortSnapshotName( periodKind, timestamp )}";
    }

    /// <summary>
    ///     Gets ONLY the snapshot name for a snapshot, using the given <paramref name="periodKind" /> and
    ///     <paramref name="timestamp" />, in conjunction with configured settings for this
    ///     object
    /// </summary>
    public string GenerateShortSnapshotName( SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        return $"{Prefix}{ComponentSeparator}{timestamp.ToString( TimestampFormatString )}{ComponentSeparator}{periodKind switch
        {
            SnapshotPeriodKind.Frequent => FrequentSuffix,
            SnapshotPeriodKind.Hourly => HourlySuffix,
            SnapshotPeriodKind.Daily => DailySuffix,
            SnapshotPeriodKind.Weekly => WeeklySuffix,
            SnapshotPeriodKind.Monthly => MonthlySuffix,
            SnapshotPeriodKind.Yearly => YearlySuffix,
            _ => throw new ArgumentOutOfRangeException( nameof( periodKind ), periodKind, null )
        }}";
    }

    /// <summary>
    ///     Gets a default <see cref="FormattingSettings" /> object, with hard-coded default values
    /// </summary>
    /// <returns></returns>
    public static FormattingSettings GetDefault( )
    {
        return new( )
        {
            Prefix = "autosnap",
            ComponentSeparator = "_",
            TimestampFormatString = "yyyy-MM-dd_HH\\:mm\\:ss",
            FrequentSuffix = "frequently",
            HourlySuffix = "hourly",
            DailySuffix = "daily",
            WeeklySuffix = "weekly",
            MonthlySuffix = "monthly",
            YearlySuffix = "yearly"
        };
    }
}
