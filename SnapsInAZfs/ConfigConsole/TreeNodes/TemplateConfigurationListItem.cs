// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Text.Json.Serialization;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

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
