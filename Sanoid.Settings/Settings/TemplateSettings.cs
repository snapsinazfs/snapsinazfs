// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;

namespace Sanoid.Settings.Settings;

/// <summary>
///     Settings definitions for templates
/// </summary>
public sealed class TemplateSettings
{
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public FormattingSettings? Formatting { get; set; }

    /// <summary>
    ///     Gets or sets the global PruneSnapshots setting
    /// </summary>
    public required bool PruneSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the snapshot timing settings sub-section
    /// </summary>
    public required SnapshotTimingSettings SnapshotTiming { get; set; }

    /// <summary>
    ///     Gets or sets the global TakeSnapshots setting
    /// </summary>
    public required bool TakeSnapshots { get; set; }

    /// <summary>
    ///     Gets formatting settings for this template, or the fallback settings provided
    /// </summary>
    /// <param name="fallbackSettings">Fallback settings to use if this template doesn't have a Formatting section.</param>
    public FormattingSettings GetFormattingSettings( FormattingSettings fallbackSettings )
    {
        return Formatting ?? fallbackSettings;
    }

    public string GenerateFullSnapshotName( string datasetName, SnapshotPeriodKind periodKind, DateTimeOffset timestamp, FormattingSettings fallbackSettings )
    {
        Formatting ??= fallbackSettings;
        return Formatting.GenerateFullSnapshotName( datasetName, periodKind, timestamp );
    }
}
