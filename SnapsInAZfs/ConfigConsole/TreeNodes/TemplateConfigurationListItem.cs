// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

/// <summary>
///     Represents an entry in the <see cref="TemplateConfigurationWindow.templateListView" />, with references to the corresponding
///     settings objects
/// </summary>
/// <param name="TemplateName">The name of the template. Will be used as the display text in the list</param>
/// <param name="ViewSettings">The view copy of the template</param>
/// <param name="BaseSettings">The base copy of the template</param>
public record TemplateConfigurationListItem( string TemplateName, TemplateSettings ViewSettings, TemplateSettings BaseSettings )
{
    /// <summary>
    ///     Gets or sets a reference to the base copy of the template
    /// </summary>
    /// <remarks>
    ///     Used primarily for comparison against <see cref="ViewSettings" />, to determine if changes have been made
    /// </remarks>
    public TemplateSettings BaseSettings { get; set; } = BaseSettings;

    /// <summary>
    ///     Gets <see cref="ViewSettings" /> != <see cref="BaseSettings" />
    /// </summary>
    public bool IsModified => ViewSettings != BaseSettings;

    /// <summary>
    ///     Gets or sets a reference to the view copy of the template
    /// </summary>
    /// <remarks>
    ///     This is the reference used for display purposes
    /// </remarks>
    public TemplateSettings ViewSettings { get; set; } = ViewSettings;

    /// <inheritdoc />
    public override string ToString( )
    {
        return TemplateName;
    }
}
