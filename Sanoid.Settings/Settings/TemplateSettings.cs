// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using JetBrains.Annotations;

namespace Sanoid.Settings.Settings;

/// <summary>
///     Settings definitions for templates
/// </summary>
[UsedImplicitly( ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers )]
public record TemplateSettings
{
    /// <summary>
    ///     Gets or sets the Formatting sub-section for this <see cref="TemplateSettings" /> object
    /// </summary>
    public FormattingSettings Formatting { get; set; } = FormattingSettings.GetDefault( );

    /// <summary>
    ///     Gets or sets the snapshot timing settings sub-section
    /// </summary>
    public SnapshotTimingSettings SnapshotTiming { get; set; } = SnapshotTimingSettings.GetDefault( );

    /// <inheritdoc cref="FormattingSettings.GenerateFullSnapshotName" />
    public string GenerateFullSnapshotName( string datasetName, SnapshotPeriodKind periodKind, DateTimeOffset timestamp )
    {
        return Formatting.GenerateFullSnapshotName( datasetName, periodKind, timestamp );
    }
}
