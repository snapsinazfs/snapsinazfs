// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Sanoid.Settings.Settings;

/// <summary>
///     Snapshot timing policies for use in <see cref="TemplateSettings" />
/// </summary>
public sealed class SnapshotTimingSettings
{
    /// <summary>
    ///     Gets or sets the time of day that daily snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 4 )]
    public required TimeOnly DailyTime { get; init; }

    /// <summary>
    ///     Gets or sets the interval, in minutes, between frequent snapshots
    /// </summary>
    /// <remarks>
    ///     Should be a whole number factor of 60, such as 5, 10, 15, 20, or 30
    /// </remarks>
    [JsonPropertyOrder( 2 )]
    public required int FrequentPeriod { get; init; }

    /// <summary>
    ///     Gets or sets the minute of the hour that hourly snapshots are taken
    /// </summary>
    [ValueRange( 0, 59 )]
    [JsonPropertyOrder( 3 )]
    public required int HourlyMinute { get; init; }

    /// <summary>
    ///     Gets or sets the day of the month that monthly snapshots are taken
    /// </summary>
    /// <remarks>
    ///     If the current month has fewer days than the specified value, monthly snapshots will be taken on the last day of
    ///     the month
    /// </remarks>
    [ValueRange( 1, 31 )]
    [JsonPropertyOrder( 7 )]
    public required int MonthlyDay { get; init; }

    /// <summary>
    ///     Gets or sets the time of day that monthly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 8 )]
    public required TimeOnly MonthlyTime { get; init; }

    /// <summary>
    ///     Gets or sets whether local time is used for snapshot naming and processing.
    /// </summary>
    [JsonPropertyOrder( 1 )]
    public required bool UseLocalTime { get; init; }

    /// <summary>
    ///     Gets or sets the day of the week on which weekly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 5 )]
    public required DayOfWeek WeeklyDay { get; init; }

    /// <summary>
    ///     Gets or sets the time of day that weekly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 6 )]
    public required TimeOnly WeeklyTime { get; init; }

    /// <summary>
    ///     Gets or sets the day of the <see cref="YearlyMonth" /> that yearly snapshots are taken
    /// </summary>
    /// <remarks>
    ///     If the current month has fewer days than the specified value, yearly snapshots will be taken on the last day of
    ///     <see cref="YearlyMonth" />
    /// </remarks>
    [ValueRange( 1, 31 )]
    [JsonPropertyOrder( 10 )]
    public required int YearlyDay { get; init; }

    /// <summary>
    ///     Gets or sets the month of the year in which yearly snapshots will be taken
    /// </summary>
    [ValueRange( 1, 12 )]
    [JsonPropertyOrder( 9 )]
    public required int YearlyMonth { get; init; }

    /// <summary>
    ///     Gets or sets the time of day that yearly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 11 )]
    public required TimeOnly YearlyTime { get; init; }

    public int GetPeriodOfHour( DateTimeOffset timestamp )
    {
        return timestamp.Minute / FrequentPeriod;
    }
}
