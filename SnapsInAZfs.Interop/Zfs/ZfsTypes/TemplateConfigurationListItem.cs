// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public record TemplateConfigurationListItem( string TemplateName, TemplateSettings ViewSettings, TemplateSettings BaseSettings )
{
    public TemplateSettings BaseSettings { get; set; } = BaseSettings;

    [JsonIgnore]
    public bool IsModified => ViewSettings != BaseSettings;

    public TemplateSettings ViewSettings { get; set; } = ViewSettings;

    /// <inheritdoc />
    public override string ToString( )
    {
        return TemplateName;
    }
}
