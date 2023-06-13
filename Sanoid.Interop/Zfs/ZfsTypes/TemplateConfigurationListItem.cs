// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public record TemplateConfigurationListItem
{
    public TemplateConfigurationListItem(string TemplateName, TemplateSettings ViewSettings, TemplateSettings BaseSettings )
    {
        this.TemplateName = TemplateName;
        this.ViewSettings = ViewSettings;
        this.BaseSettings = BaseSettings;
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return TemplateName;
    }

    public string TemplateName { get; init; }
    public TemplateSettings ViewSettings { get; init; }
    public TemplateSettings BaseSettings { get; init; }
}
