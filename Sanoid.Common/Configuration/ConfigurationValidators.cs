// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Basic checks for configuration
/// </summary>
public static class ConfigurationValidators
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Checks that the SnapshotTiming section exists in this configuration and returns it as an out parameter.
    /// </summary>
    /// <param name="templateSection"></param>
    /// <param name="templateSnapshotSection"></param>
    public static bool CheckDefaultTemplateSnapshotTimingSectionExists( this IConfigurationSection templateSection, out IConfigurationSection templateSnapshotSection )
    {
        try
        {
            Logger.Trace( "Checking for existence of 'SnapshotTiming' section in {0} Template", templateSection.Key );
            templateSnapshotSection = templateSection.GetRequiredSection( "SnapshotTiming" );
            Logger.Trace( "'SnapshotTiming' section found" );
            return true;
        }
        catch ( InvalidOperationException ex )
        {
            Logger.Fatal( "Template {0} does not contain the required SnapshotTiming section. Program will terminate.", templateSection.Key, ex );
            throw;
        }
    }

    /// <summary>
    ///     Checks that the SnapshotRetention section exists in this configuration and returns it as an out parameter.
    /// </summary>
    /// <param name="templateSection"></param>
    /// <param name="templateSnapshotRetentionSection"></param>
    public static bool CheckTemplateSnapshotRetentionSectionExists( this IConfigurationSection templateSection, out IConfigurationSection templateSnapshotRetentionSection )
    {
        try
        {
            Logger.Trace( "Checking for existence of 'SnapshotRetention' section in {0} Template", templateSection.Key );
            templateSnapshotRetentionSection = templateSection.GetRequiredSection( "SnapshotRetention" );
            Logger.Trace( "'SnapshotRetention' section found" );
            return true;
        }
        catch ( InvalidOperationException ex )
        {
            Logger.Fatal( "Template {0} does not contain the required SnapshotRetention section. Program will terminate.", templateSection.Key, ex );
            throw;
        }
    }

    /// <summary>
    ///     Checks that the named template exists in this configuration and returns it as an out parameter.
    /// </summary>
    /// <param name="baseConfiguration"></param>
    /// <param name="templateName"></param>
    /// <param name="defaultTemplateSection"></param>
    public static bool CheckTemplateSectionExists( this IConfiguration baseConfiguration, string templateName, out IConfigurationSection defaultTemplateSection )
    {
        try
        {
            Logger.Trace( "Checking for existence of {0} Template", templateName );
            IConfigurationSection templatesSection = baseConfiguration.GetSection( "Templates" );
            defaultTemplateSection = templatesSection.GetRequiredSection( templateName );
            Logger.Trace( "{0} Template found", templateName );
            return true;
        }
        catch ( InvalidOperationException ex )
        {
            Logger.Fatal( "Template {0} not found in Sanoid.json#/Templates. Program will terminate.", templateName, ex );
            throw;
        }
    }
}
