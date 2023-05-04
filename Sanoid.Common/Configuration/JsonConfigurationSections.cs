using System.Text.Json;

using Json.More;
using Json.Schema;

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration
{
    /// <summary>
    /// Singleton class for easy global access to utility functions and properties such as configuration.
    /// </summary>
    public static class JsonConfigurationSections
    {
        private static IConfigurationRoot? _rootConfiguration;

        /// <summary>
        /// Gets the root configuration section of Sanoid.json
        /// </summary>
        /// <remarks>
        /// Should only explicitly be used for access to properties in the configuration root.<br />
        /// Other static properties are exposed in <see cref="JsonConfigurationSections"/> for sub-sections of Sanoid.json.
        /// </remarks>
        /// <seealso cref="FormattingConfiguration"/>
        /// <seealso cref="SnapshotNamingConfiguration"/>
        /// <seealso cref="ValidateSanoidConfiguration()"/>
        public static IConfigurationRoot RootConfiguration
        {
            get
            {
                ValidateSanoidConfiguration( );
                return _rootConfiguration ??= new ConfigurationManager( ).AddJsonFile( "Sanoid.json", false, true ).Build( );
            }
        }

        /// <summary>
        /// Validates Sanoid.json against Sanoid.schema.json.<br />
        /// If the method does not throw, the configuration is valid for use.
        /// </summary>
        /// <exception cref="JsonException">If Sanoid.json is invalid according to Sanoid.schema.json</exception>
        private static void ValidateSanoidConfiguration( )
        {
            JsonSchema sanoidConfigJsonSchema = JsonSchema.FromFile("Sanoid.schema.json");
            using JsonDocument sanoidConfigJsonDocument = JsonDocument.Parse( File.ReadAllText( "Sanoid.json" ) );
            EvaluationOptions evaluationOptions = new()
            {
                EvaluateAs = SpecVersion.Draft7,
                RequireFormatValidation = true,
                OnlyKnownFormats = true,
                OutputFormat = OutputFormat.List,
                ValidateAgainstMetaSchema = false
            };

            EvaluationResults configValidationResults = sanoidConfigJsonSchema.Evaluate(sanoidConfigJsonDocument, evaluationOptions);

            if ( !configValidationResults.IsValid )
            {
                Console.WriteLine( "Sanoid.json validation failed." );
                Console.WriteLine( "Correct the following issues in Sanois.json and try again:" );
                foreach ( EvaluationResults validationDetail in configValidationResults.Details )
                {
                    if ( validationDetail is { IsValid: false, HasErrors: true } )
                    {
                        Console.WriteLine( $"{validationDetail.InstanceLocation} has {validationDetail.Errors!.Count} problems:" );
                        foreach (KeyValuePair<string, string> error in validationDetail.Errors)
                        {
                            Console.WriteLine($"  Problem: {error.Key}");
                            Console.WriteLine($"  Details: {error.Value}");
                        }
                    }
                }

                throw new ConfigurationValidationException( "Sanoid.json validation failed. Please check Sanoid.json and ensure it complies with the schema specified in Sanoid.schema.json." );
            }
        }

        /// <summary>
        /// Gets the /Formatting configuration section of Sanoid.json
        /// </summary>
        /// <seealso cref="SnapshotNamingConfiguration"/>
        public static IConfigurationSection FormattingConfiguration => RootConfiguration.GetRequiredSection( "Formatting" );

        /// <summary>
        /// Gets the /Formatting/SnapshotNaming configuration section of Sanoid.json
        /// </summary>
        public static IConfigurationSection SnapshotNamingConfiguration => FormattingConfiguration.GetRequiredSection( "SnapshotNaming" );

    }
}
