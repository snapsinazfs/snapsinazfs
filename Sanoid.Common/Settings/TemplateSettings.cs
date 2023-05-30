// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using Sanoid.Interop.Zfs.ZfsTypes;

namespace Sanoid.Common.Settings;

/// <summary>
///     Settings definitions for templates
/// </summary>
public sealed class TemplateSettings
{
    /// <summary>
    ///     Gets or sets the global PruneSnapshots setting
    /// </summary>
    public required bool PruneSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the recursion mode for the template
    /// </summary>
    public required string Recursion { get; set; }

    [JsonIgnore]
    public SnapshotRecursionMode RecursionMode => Recursion;

    /// <summary>
    ///     Gets or sets the snapshot retention settings sub-section
    /// </summary>
    public required SnapshotRetentionSettings SnapshotRetention { get; set; }

    /// <summary>
    ///     Gets or sets the snapshot timing settings sub-section
    /// </summary>
    public required SnapshotTimingSettings SnapshotTiming { get; set; }

    /// <summary>
    ///     Gets or sets the global TakeSnapshots setting
    /// </summary>
    public required bool TakeSnapshots { get; set; }
}
