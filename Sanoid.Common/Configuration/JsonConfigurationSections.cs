// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Json.Schema;
using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Singleton class for easy global access to Sanoid.json configuration.
/// </summary>
public static class JsonConfigurationSections
{
    /// <summary>
    ///     Validates json configuration files upon first use
    /// </summary>
    /// <seealso cref="ValidateSanoidConfiguration()" />
    static JsonConfigurationSections( )
    {
        ValidateSanoidConfiguration( );
    }

    /// <summary>
    ///     Gets the /Datasets configuration section of Sanoid.json
    /// </summary>
    public static IConfigurationSection DatasetsConfiguration => RootConfiguration.GetRequiredSection( "Datasets" );

    /// <summary>
    ///     Gets the /Formatting configuration section of Sanoid.json
    /// </summary>
    /// <seealso cref="SnapshotNamingConfiguration" />
    public static IConfigurationSection FormattingConfiguration => RootConfiguration.GetRequiredSection( "Formatting" );

    /// <summary>
    ///     Gets the /Monitoring configuration section of Sanoid.json
    /// </summary>
    public static IConfigurationSection MonitoringConfiguration => RootConfiguration.GetRequiredSection( "Monitoring" );

    /// <summary>
    ///     Gets the root configuration section of Sanoid.json
    /// </summary>
    /// <remarks>
    ///     Should only explicitly be used for access to properties in the configuration root.<br />
    ///     Other static properties are exposed in <see cref="JsonConfigurationSections" /> for sub-sections of Sanoid.json.
    /// </remarks>
    /// <seealso cref="FormattingConfiguration" />
    /// <seealso cref="SnapshotNamingConfiguration" />
    /// <seealso cref="MonitoringConfiguration" />
    /// <seealso cref="TemplatesConfiguration" />
    /// <seealso cref="DatasetsConfiguration" />
#pragma warning disable CA2000
    public static IConfigurationRoot RootConfiguration => _rootConfiguration ??= new ConfigurationManager( )
                                                                                 .AddEnvironmentVariables( "Sanoid.net:" )
                                                                             #if WINDOWS
                                                                                 .AddJsonFile( "Sanoid.json", true, false )
                                                                                 .AddJsonFile( "Sanoid.local.json", true, false )
                                                                             #else
                                                                                 .AddJsonFile( "/usr/local/share/Sanoid.net/Sanoid.json", true, false )
                                                                                 .AddJsonFile( "/etc/sanoid/Sanoid.local.json", true, false )
                                                                                 .AddJsonFile( Path.Combine( Path.GetFullPath( Environment.GetEnvironmentVariable( "HOME" ) ?? "~/" ), ".config/Sanoid.net/Sanoid.user.json" ), true, false )
                                                                                 .AddJsonFile( "Sanoid.local.json", true, false )
                                                                             #endif
                                                                                 .Build( );
#pragma warning restore CA2000

    /// <summary>
    ///     Gets the /Formatting/SnapshotNaming configuration section of Sanoid.json
    /// </summary>
    public static IConfigurationSection SnapshotNamingConfiguration => FormattingConfiguration.GetRequiredSection( "SnapshotNaming" );

    /// <summary>
    ///     Gets the /Templates configuration section of Sanoid.json
    /// </summary>
    public static IConfigurationSection TemplatesConfiguration => RootConfiguration.GetRequiredSection( "Templates" );

    private static IConfigurationRoot? _rootConfiguration;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Validates Sanoid.json against Sanoid.schema.json.<br />
    ///     If the method does not throw, the configuration is valid for use.
    /// </summary>
    /// <exception cref="JsonException">If Sanoid.json is invalid according to Sanoid.schema.json</exception>
    private static void ValidateSanoidConfiguration( )
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
                continue;
            }

            Logger.Debug( "Validating {0} configuration file.", filePath );
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
    }
}
