#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     Snapshot timing policies
/// </summary>
public sealed record SnapshotTimingSettings
{
    /// <summary>
    ///     Gets or sets the time of day that daily snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 3 )]
    public required TimeOnly DailyTime { get; init; }

    /// <summary>
    ///     Gets or sets the interval, in minutes, between frequent snapshots
    /// </summary>
    /// <remarks>
    ///     Should be a whole number factor of 60, such as 5, 10, 15, 20, or 30
    /// </remarks>
    [JsonPropertyOrder( 1 )]
    public required int FrequentPeriod { get; init; }

    /// <summary>
    ///     Gets or sets the minute of the hour that hourly snapshots are taken
    /// </summary>
    [Range( 0, 59 )]
    [JsonPropertyOrder( 2 )]
    public required int HourlyMinute { get; init; }

    /// <summary>
    ///     Gets or sets the day of the month that monthly snapshots are taken
    /// </summary>
    /// <remarks>
    ///     If the current month has fewer days than the specified value, monthly snapshots will be taken on the last day of
    ///     the month
    /// </remarks>
    [Range( 1, 31 )]
    [JsonPropertyOrder( 6 )]
    public required int MonthlyDay { get; init; }

    /// <summary>
    ///     Gets or sets the time of day that monthly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 7 )]
    public required TimeOnly MonthlyTime { get; init; }

    /// <summary>
    ///     Gets or sets the day of the week on which weekly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 4 )]
    public required DayOfWeek WeeklyDay { get; init; }

    /// <summary>
    ///     Gets or sets the time of day that weekly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 5 )]
    public required TimeOnly WeeklyTime { get; init; }

    /// <summary>
    ///     Gets or sets the day of the <see cref="YearlyMonth" /> that yearly snapshots are taken
    /// </summary>
    /// <remarks>
    ///     If the current month has fewer days than the specified value, yearly snapshots will be taken on the last day of
    ///     <see cref="YearlyMonth" />
    /// </remarks>
    [Range( 1, 31 )]
    [JsonPropertyOrder( 9 )]
    public required int YearlyDay { get; init; }

    /// <summary>
    ///     Gets or sets the month of the year in which yearly snapshots will be taken
    /// </summary>
    [Range( 1,12 )]
    [JsonPropertyOrder( 8 )]
    public required int YearlyMonth { get; init; }

    /// <summary>
    ///     Gets or sets the time of day that yearly snapshots are taken
    /// </summary>
    [JsonPropertyOrder( 10 )]
    public required TimeOnly YearlyTime { get; init; }

    /// <summary>
    ///     Gets a default instance of a <see cref="SnapshotTimingSettings" /> object, with hard-coded default values
    /// </summary>
    /// <returns></returns>
    public static SnapshotTimingSettings GetDefault( )
    {
        return new( )
        {
            FrequentPeriod = 15,
            HourlyMinute = 0,
            DailyTime = TimeOnly.MinValue,
            WeeklyDay = DayOfWeek.Monday,
            WeeklyTime = TimeOnly.MinValue,
            MonthlyDay = 1,
            MonthlyTime = TimeOnly.MinValue,
            YearlyMonth = 1,
            YearlyDay = 1,
            YearlyTime = TimeOnly.MinValue
        };
    }

    /// <summary>
    ///     Gets the period of the hour the given <paramref name="timestamp" /> is in, according to the value set for
    ///     <see cref="FrequentPeriod" />
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public int GetPeriodOfHour( DateTimeOffset timestamp )
    {
        return timestamp.Minute / FrequentPeriod;
    }
}
