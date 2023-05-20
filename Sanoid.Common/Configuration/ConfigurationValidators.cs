// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Json.Schema;
using System.Text.Json;

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

    /// <summary>
    ///     Validates the Sanoid json files against the Sanoid.net configuration schemas.<br />
    ///     If the method does not throw, the configuration is valid for use.
    /// </summary>
    /// <exception cref="JsonException">If Sanoid.json, Sanoid.local.json, or Sanoid.user.json are invalid, according to their respective shemas.</exception>
    internal static void ValidateSanoidConfigurationSchema( )
    {
        EvaluationOptions evaluationOptions = new( )
        {
            EvaluateAs = SpecVersion.Draft201909,
            RequireFormatValidation = true,
            OutputFormat = OutputFormat.List,
            ValidateAgainstMetaSchema = false
        };
        List< (string FilePath,bool IsRootConfig)> configFilePaths = new( );
    #if WINDOWS
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "Sanoid.monitoring.schema.json" ) );
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "Sanoid.template.schema.json" ) );
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "Sanoid.dataset.schema.json" ) );
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "Sanoid.local.schema.json" ) );
        configFilePaths.Add(("Sanoid.json",true)  );
        configFilePaths.Add(("Sanoid.local.json",false)  );
        JsonSchema sanoidBaseConfigJsonSchema = JsonSchema.FromFile( "Sanoid.schema.json" );
        JsonSchema sanoidLocalConfigJsonSchema = JsonSchema.FromFile( "Sanoid.local.schema.json" );
    #else
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "/usr/local/share/Sanoid.net/Sanoid.monitoring.schema.json" ) );
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "/usr/local/share/Sanoid.net/Sanoid.template.schema.json" ) );
        SchemaRegistry.Global.Register( JsonSchema.FromFile( "/usr/local/share/Sanoid.net/Sanoid.dataset.schema.json" ) );
        configFilePaths.Add( ( "/usr/local/share/Sanoid.net/Sanoid.json", true ) );
        configFilePaths.Add( ( "/etc/sanoid/Sanoid.local.json", false ) );
        configFilePaths.Add((Path.Combine( Path.GetFullPath( Environment.GetEnvironmentVariable( "HOME" ) ?? "~/" ), ".config/Sanoid.net/Sanoid.user.json" ),false)  );
        configFilePaths.Add( ( "Sanoid.local.json", false ) );
        JsonSchema sanoidBaseConfigJsonSchema = JsonSchema.FromFile( "/usr/local/share/Sanoid.net/Sanoid.schema.json" );
        JsonSchema sanoidLocalConfigJsonSchema = JsonSchema.FromFile( "/usr/local/share/Sanoid.net/Sanoid.local.schema.json" );
    #endif

        foreach ( (string? filePath, bool isRootConfig) in configFilePaths )
        {
            if ( !File.Exists( filePath ) )
            {
                Logger.Debug( "File {filePath} does not exist. Skipping validation for {filePath}.", filePath );
                continue;
            }

            Logger.Debug( "Validating configuration file {filePath}.", filePath );
            EvaluationResults configValidationResults = isRootConfig switch
            {
                true => sanoidBaseConfigJsonSchema.Evaluate( JsonDocument.Parse( File.ReadAllText( filePath ) ), evaluationOptions ),
                _ => sanoidLocalConfigJsonSchema.Evaluate( JsonDocument.Parse( File.ReadAllText( filePath ) ), evaluationOptions )
            };

            if ( !configValidationResults.IsValid )
            {
                Logger.Error( "{0} validation failed.", filePath );
                foreach ( EvaluationResults validationDetail in configValidationResults.Details )
                {
                    if ( validationDetail is { IsValid: false, HasErrors: true } )
                    {
                        Logger.Error( $"{validationDetail.InstanceLocation} has {validationDetail.Errors!.Count} problems:" );
                        foreach ( KeyValuePair<string, string> error in validationDetail.Errors )
                        {
                            Logger.Error( $"  Problem: {error.Key}; Details: {error.Value}" );
                        }
                    }
                }

                throw new ConfigurationValidationException( $"{filePath} validation failed. Please check {filePath} and ensure it complies with the schema specified in Sanoid.{( isRootConfig ? string.Empty : "local." )}schema.json." );
            }
        }
        Logger.Debug( "Configuration schema validation successful" );
    }
}
