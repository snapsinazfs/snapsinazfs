// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Json.Schema;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Sanoid.Common.Configuration
{
    /// <summary>
    ///     Singleton class for easy global access to Sanoid.json configuration.
    /// </summary>
    public static class JsonConfigurationSections
    {
        /// <summary>
        ///     Validates json configuration files upon first use
        /// </summary>
        /// <seealso cref="ValidateSanoidConfiguration()" />
        static JsonConfigurationSections()
        {
            ValidateSanoidConfiguration();
        }

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
        public static IConfigurationRoot RootConfiguration => _rootConfiguration ??= new ConfigurationManager().AddJsonFile( "Sanoid.json", false, true ).Build();

        /// <summary>
        ///     Gets the /Formatting/SnapshotNaming configuration section of Sanoid.json
        /// </summary>
        public static IConfigurationSection SnapshotNamingConfiguration => FormattingConfiguration.GetRequiredSection( "SnapshotNaming" );

        /// <summary>
        ///     Gets the /Templates configuration section of Sanoid.json
        /// </summary>
        public static IConfigurationSection TemplatesConfiguration => RootConfiguration.GetRequiredSection( "Templates" );

        private static IConfigurationRoot? _rootConfiguration;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Validates Sanoid.json against Sanoid.schema.json.<br />
        ///     If the method does not throw, the configuration is valid for use.
        /// </summary>
        /// <exception cref="JsonException">If Sanoid.json is invalid according to Sanoid.schema.json</exception>
        private static void ValidateSanoidConfiguration()
        {
            using JsonDocument sanoidConfigJsonDocument = JsonDocument.Parse( File.ReadAllText( "Sanoid.json" ) );
            EvaluationOptions evaluationOptions = new()
            {
                EvaluateAs = SpecVersion.Draft201909,
                RequireFormatValidation = true,
                OnlyKnownFormats = true,
                OutputFormat = OutputFormat.List,
                ValidateAgainstMetaSchema = false
            };

            SchemaRegistry.Global.Register( JsonSchema.FromFile( "Sanoid.monitoring.schema.json" ) );
            SchemaRegistry.Global.Register( JsonSchema.FromFile( "Sanoid.template.schema.json" ) );

            JsonSchema sanoidConfigJsonSchema = JsonSchema.FromFile( "Sanoid.schema.json" );
            EvaluationResults configValidationResults = sanoidConfigJsonSchema.Evaluate( sanoidConfigJsonDocument, evaluationOptions );

            if ( !configValidationResults.IsValid )
            {
                Logger.Error( "Sanoid.json validation failed." );
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

                throw new ConfigurationValidationException( "Sanoid.json validation failed. Please check Sanoid.json and ensure it complies with the schema specified in Sanoid.schema.json." );
            }
        }
    }
}
