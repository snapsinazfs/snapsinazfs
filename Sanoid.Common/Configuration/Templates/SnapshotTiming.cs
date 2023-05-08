// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration.Templates;

/// <summary>
///     Snapshot timing policies for use in <see cref="Template" />s
/// </summary>
public record struct SnapshotTiming
{
    /// <summary>
    ///     Gets or sets the time of day that daily snapshots are taken
    /// </summary>
    public TimeOnly DailyTime { get; set; }

    /// <summary>
    ///     Gets or sets the minute of the hour that hourly snapshots are taken
    /// </summary>
    public int HourlyMinute { get; set; }

    /// <summary>
    ///     Gets or sets the day of the month that monthly snapshots are taken
    /// </summary>
    /// <remarks>
    ///     If the current month has fewer days than the specified value, monthly snapshots will be taken on the last day of
    ///     the month
    /// </remarks>
    public int MonthlyDay { get; set; }

    /// <summary>
    ///     Gets or sets the time of day that monthly snapshots are taken
    /// </summary>
    public TimeOnly MonthlyTime { get; set; }

    /// <summary>
    ///     Gets or sets whether local time is used for snapshot naming and processing.
    /// </summary>
    public bool UseLocalTime { get; set; }

    /// <summary>
    ///     Gets or sets the day of the week on which weekly snapshots are taken
    /// </summary>
    public DayOfWeek WeeklyDay { get; set; }

    /// <summary>
    ///     Gets or sets the time of day that weekly snapshots are taken
    /// </summary>
    public TimeOnly WeeklyTime { get; set; }

    /// <summary>
    ///     Gets or sets the day of the <see cref="YearlyMonth" /> that yearly snapshots are taken
    /// </summary>
    /// <remarks>
    ///     If the current month has fewer days than the specified value, yearly snapshots will be taken on the last day of
    ///     <see cref="YearlyMonth" />
    /// </remarks>
    public int YearlyDay { get; set; }

    /// <summary>
    ///     Gets or sets the month of the year in which yearly snapshots will be taken
    /// </summary>
    public int YearlyMonth { get; set; }

    /// <summary>
    ///     Gets or sets the time of day that yearly snapshots are taken
    /// </summary>
    public TimeOnly YearlyTime { get; set; }

    /// <summary>
    ///     Gets a new immutable <see cref="SnapshotTiming" /> record, parsed from an <see cref="IConfiguration" /> object
    /// </summary>
    /// <param name="config">
    ///     A reference to an <see cref="IConfiguration" /> object containing a single instance of a snapshot
    ///     timing policy.
    /// </param>
    /// <returns>
    ///     A new immutable <see cref="SnapshotTiming" /> record, parsed from <paramref name="config" />
    /// </returns>
    public static SnapshotTiming FromConfiguration( IConfiguration config )
    {
        return new SnapshotTiming
        {
            DailyTime = TimeOnly.Parse( config[ "DailyTime" ] ?? "00:00:00" ),
            HourlyMinute = config.GetInt( "HourlyMinute" ),
            MonthlyDay = config.GetInt( "MonthlyDay" ),
            MonthlyTime = TimeOnly.Parse( config[ "MonthlyTime" ] ?? "00:00:00" ),
            UseLocalTime = config.GetBoolean( "UseLocalTime", true ),
            WeeklyDay = (DayOfWeek)config.GetInt( "WeeklyDay", 1 ),
            WeeklyTime = TimeOnly.Parse( config[ "WeeklyTime" ] ?? "00:00:00" ),
            YearlyDay = config.GetInt( "YearlyDay", 31 ),
            YearlyMonth = config.GetInt( "YearlyMonth" ),
            YearlyTime = TimeOnly.Parse( config[ "YearlyTime" ] ?? "00:00:00" )
        };
    }
}
