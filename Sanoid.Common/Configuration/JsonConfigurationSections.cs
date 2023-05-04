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
        /// Should only explicitly be used for access to properties in the configuration root.<para />
        /// Other static properties are exposed in <see cref="JsonConfigurationSections"/> for sub-sections of Sanoid.json.
        /// </remarks>
        /// <seealso cref="FormattingConfiguration"/>
        /// <seealso cref="SnapshotNamingConfiguration"/>
        public static IConfigurationRoot? RootConfiguration => _rootConfiguration ??= new ConfigurationManager().AddJsonFile("Sanoid.json", false, true).Build();

        /// <summary>
        /// Gets the /Formatting configuration section of Sanoid.json
        /// </summary>
        /// <seealso cref="SnapshotNamingConfiguration"/>
        public static IConfigurationSection? FormattingConfiguration => RootConfiguration?.GetRequiredSection( "Formatting" );

        /// <summary>
        /// Gets the /Formatting/SnapshotNaming configuration section of Sanoid.json
        /// </summary>
        public static IConfigurationSection? SnapshotNamingConfiguration => FormattingConfiguration?.GetRequiredSection( "SnapshotNaming" );

    }
}
