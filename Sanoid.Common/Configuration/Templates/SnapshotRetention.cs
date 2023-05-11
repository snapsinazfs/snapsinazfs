// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration.Templates;

/// <summary>
///     Snapshot retention policy for use within <see cref="Template" />s
/// </summary>
public readonly record struct SnapshotRetention
{
    /// <summary>
    ///     Gets or sets how many daily snapshots will be retained
    /// </summary>
    public int Daily { get; init; }

    /// <summary>
    ///     Gets or sets how many frequent snapshots will be retained
    /// </summary>
    public int Frequent { get; init; }

    /// <summary>
    ///     Gets or sets the interval, in minutes, between frequent snapshots
    /// </summary>
    /// <remarks>
    ///     Should be a whole number factor of 60, such as 5, 10, 15, 20, or 30
    /// </remarks>
    public int FrequentPeriod { get; init; }

    /// <summary>
    ///     Gets or sets how many hourly snapshots will be retained
    /// </summary>
    public int Hourly { get; init; }

    /// <summary>
    ///     Gets or sets how many monthly snapshots will be retained
    /// </summary>
    public int Monthly { get; init; }

    /// <summary>
    ///     Gets or sets what percentage of remaining pool capacity must be reached before snapshots will be pruned by this
    ///     policy
    /// </summary>
    public int PruneDeferral { get; init; }

    /// <summary>
    ///     Gets or sets how many weekly snapshots will be retained
    /// </summary>
    public int Weekly { get; init; }

    /// <summary>
    ///     Gets or sets how many yearly snapshots will be retained
    /// </summary>
    public int Yearly { get; init; }

    /// <summary>
    ///     Gets a new immutable <see cref="SnapshotRetention" /> record, parsed from an <see cref="IConfiguration" /> object
    /// </summary>
    /// <param name="config">
    ///     A reference to an <see cref="IConfiguration" /> object containing a single instance of a snapshot
    ///     retention policy.
    /// </param>
    /// <returns>
    ///     A new immutable <see cref="SnapshotRetention" /> record, parsed from <paramref name="config" />
    /// </returns>
    public static SnapshotRetention FromConfiguration( IConfiguration config )
    {
        return new SnapshotRetention
        {
            Daily = config.GetInt( "Daily" ),
            Frequent = config.GetInt( "Frequent" ),
            FrequentPeriod = config.GetInt( "FrequentPeriod" ),
            Hourly = config.GetInt( "Hourly" ),
            Monthly = config.GetInt( "Monthly" ),
            PruneDeferral = config.GetInt( "PruneDeferral" ),
            Yearly = config.GetInt( "Yearly" )
        };
    }
}
