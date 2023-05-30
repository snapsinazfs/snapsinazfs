// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Settings.Settings;

/// <summary>
///     Snapshot retention policy for use within <see cref="TemplateSettings" />
/// </summary>
public sealed class SnapshotRetentionSettings
{
    /// <summary>
    ///     Gets or sets how many daily snapshots will be retained
    /// </summary>
    public required int Daily { get; init; }

    /// <summary>
    ///     Gets or sets how many frequent snapshots will be retained
    /// </summary>
    public required int Frequent { get; init; }

    /// <summary>
    ///     Gets or sets how many hourly snapshots will be retained
    /// </summary>
    public required int Hourly { get; init; }

    /// <summary>
    ///     Gets or sets how many monthly snapshots will be retained
    /// </summary>
    public required int Monthly { get; init; }

    /// <summary>
    ///     Gets or sets what percentage of remaining pool capacity must be reached before snapshots will be pruned by this
    ///     policy
    /// </summary>
    public required int PruneDeferral { get; init; }

    /// <summary>
    ///     Gets or sets how many weekly snapshots will be retained
    /// </summary>
    public required int Weekly { get; init; }

    /// <summary>
    ///     Gets or sets how many yearly snapshots will be retained
    /// </summary>
    public required int Yearly { get; init; }
}
