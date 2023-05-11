// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration;

public static class ConfigurationValidators
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Checks that the SnapshotTiming section exists and is fully configured
    /// </summary>
    /// <param name="templateSection"></param>
    /// <param name="templateSnapshotSection"></param>
    public static void CheckDefaultTemplateSnapshotTimingSectionExists( IConfigurationSection templateSection, out IConfigurationSection templateSnapshotSection )
    {
        try
        {
            Logger.Trace( "Checking for existence of 'SnapshotTiming' section in 'default' Template" );
            templateSnapshotSection = templateSection.GetRequiredSection( "SnapshotTiming" );
            Logger.Trace( "'SnapshotTiming' section found" );
        }
        catch ( InvalidOperationException ex )
        {
            // ReSharper disable FormatStringProblem
            Logger.Fatal( "Template 'default' does not contain the required SnapshotTiming section. Program will terminate.", ex );
            // ReSharper restore FormatStringProblem
            throw;
        }
    }

    public static void CheckDefaultTemplateSnapshotRetentionSectionExists( IConfigurationSection defaultTemplateSection, out IConfigurationSection defaultTemplateSnapshotRetentionSection )
    {
        try
        {
            Logger.Trace( "Checking for existence of 'SnapshotRetention' section in 'default' Template" );
            defaultTemplateSnapshotRetentionSection = defaultTemplateSection.GetRequiredSection( "SnapshotRetention" );
            Logger.Trace( "'SnapshotRetention' section found" );
        }
        catch ( InvalidOperationException ex )
        {
            // ReSharper disable FormatStringProblem
            Logger.Fatal( "Template 'default' does not contain the required SnapshotRetention section. Program will terminate.", ex );
            // ReSharper restore FormatStringProblem
            throw;
        }
    }

    /// <summary>
    ///     Checks that the named template exists in this configuration and returns it as an out parameter.
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="defaultTemplateSection"></param>
    public static void CheckTemplateSectionExists( this IConfiguration baseConfiguration, string templateName, out IConfigurationSection defaultTemplateSection )
    {
        try
        {
            Logger.Trace( "Checking for existence of 'default' Template" );
            defaultTemplateSection = baseConfiguration.GetRequiredSection( templateName );
            Logger.Trace( "'default' Template found" );
        }
        catch ( InvalidOperationException ex )
        {
            // ReSharper disable FormatStringProblem
            Logger.Fatal( "Template 'default' not found in Sanoid.json#/Templates. Program will terminate.", ex );
            // ReSharper restore FormatStringProblem
            throw;
        }
    }
}
