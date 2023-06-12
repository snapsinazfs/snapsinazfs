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
[JsonSerializable(typeof(TemplateSettings) )]
[JsonSourceGenerationOptions( DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed class TemplateSettings
{
    public required FormattingSettings Formatting { get; set; }

    /// <summary>
    ///     Gets or sets the snapshot timing settings sub-section
    /// </summary>
    public required SnapshotTimingSettings SnapshotTiming { get; set; }

    public string GenerateFullSnapshotName( string datasetName, SnapshotPeriodKind periodKind, DateTimeOffset timestamp, FormattingSettings fallbackSettings )
    {
        Formatting ??= fallbackSettings;
        return Formatting.GenerateFullSnapshotName( datasetName, periodKind, timestamp );
    }
}
