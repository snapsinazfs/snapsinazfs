// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using JetBrains.Annotations;

namespace SnapsInAZfs.Settings.Settings;

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
