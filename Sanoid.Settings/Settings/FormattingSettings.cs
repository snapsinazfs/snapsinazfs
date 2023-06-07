// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;

namespace Sanoid.Settings.Settings;

public sealed class FormattingSettings
{
    [JsonPropertyOrder( 1 )]
    public required string ComponentSeparator { get; set; }

    [JsonPropertyOrder( 5 )]
    public required string DailySuffix { get; set; }

    [JsonPropertyOrder( 4 )]
    public required string FrequentSuffix { get; set; }

    [JsonPropertyOrder( 5 )]
    public required string HourlySuffix { get; set; }

    [JsonPropertyOrder( 7 )]
    public required string MonthlySuffix { get; set; }

    [JsonPropertyOrder( 2 )]
    public required string Prefix { get; set; }

    [JsonPropertyOrder( 3 )]
    public required string TimestampFormatString { get; set; }

    [JsonPropertyOrder( 6 )]
    public required string WeeklySuffix { get; set; }

    [JsonPropertyOrder( 8 )]
    public required string YearlySuffix { get; set; }

    public string GenerateFullSnapshotName( string datasetName, SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        return $"{datasetName}@{GenerateShortSnapshotName( periodKind, timestamp )}";
    }

    public string GenerateShortSnapshotName( SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        return $"{Prefix}{ComponentSeparator}{timestamp.ToString( TimestampFormatString )}{ComponentSeparator}{periodKind switch
        {
            SnapshotPeriodKind.Temporary => "temporary",
            SnapshotPeriodKind.Frequent => FrequentSuffix,
            SnapshotPeriodKind.Hourly => HourlySuffix,
            SnapshotPeriodKind.Daily => DailySuffix,
            SnapshotPeriodKind.Weekly => WeeklySuffix,
            SnapshotPeriodKind.Monthly => MonthlySuffix,
            SnapshotPeriodKind.Yearly => YearlySuffix,
            SnapshotPeriodKind.Manual => "manual",
            _ => throw new ArgumentOutOfRangeException( nameof( periodKind ), periodKind, null )
        }}";
    }
}
