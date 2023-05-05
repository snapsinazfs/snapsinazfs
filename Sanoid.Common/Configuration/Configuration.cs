using System.Text.Json.Serialization;
using Json.Schema.Serialization;
using NLog;

namespace Sanoid.Common.Configuration;

/// <summary>
/// Corresponds to the root section of Sanoid.json
/// </summary>
public static class Configuration
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Gets whether Sanoid.net should use ini-formatted configuration files using PERL sanoid's schema.<br />
    /// Corresponds to the /UseSanoidConfiguration property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is true<br />
    /// If <c>true</c>, uses <see cref="SanoidConfigurationDefaultsFile"/> and <see cref="SanoidConfigurationLocalFile"/> in the <see cref="SanoidConfigurationPathBase"/> directory.<br />
    /// If <c>false</c>, uses configuration in Sanoid.json only - 
    /// </remarks>
    /// <value>A <see langword="bool"/> indicating whether PERL sanoid's configuration will be respected (<see langword="true"/>) or not (<see langword="false"/>).</value>
    [JsonPropertyName( "UseSanoidConfiguration" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static bool UseSanoidConfiguration => JsonConfigurationSections.RootConfiguration.GetBoolean( "UseSanoidConfiguration" );


    /// <summary>
    /// Gets the absolute path to the directory containing PERL sanoid's configuration files.<br />
    /// Corresponds to the /SanoidConfigurationPathBase property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "/etc/sanoid"<br />
    /// Should not contain a trailing slash.<br />
    /// Not guaranteed to work with a relative path. Use an absolute path.
    /// </remarks>
    /// <value>A <see langword="string"/> indicating the absolute path to the directory containing PERL sanoid's configuration files</value>
    [JsonPropertyName( "SanoidConfigurationPathBase" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string SanoidConfigurationPathBase => JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationPathBase" ] ?? "/etc/sanoid";


    /// <summary>
    /// Gets or sets the name of the ini-formatted file inside the <see cref="SanoidConfigurationPathBase"/> folder containing PERL sanoid's default configuration.<br />
    /// Corresponds to the /SanoidConfigurationDefaultsFile property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "sanoid.defaults.conf"<br />
    /// Assumed to be a file name only.
    /// </remarks>
    /// <value>A <see langword="string"/> indicating the file name of the sanoid.defaults.conf file</value>
    [JsonPropertyName( "SanoidConfigurationDefaultsFile" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string SanoidConfigurationDefaultsFile => JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationDefaultsFile" ] ?? "sanoid.defaults.conf";


    /// <summary>
    /// Gets or sets the name of the ini-formatted file inside the <see cref="SanoidConfigurationPathBase"/> folder containing PERL sanoid's local configuration.<br />
    /// Corresponds to the /SanoidConfigurationLocalFile property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "sanoid.conf"<br />
    /// Assumed to be a file name only.
    /// </remarks>
    /// <value>A <see langword="string"/> indicating the file name of the sanoid.conf file</value>
    [JsonPropertyName( "SanoidConfigurationLocalFile" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string SanoidConfigurationLocalFile => JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationLocalFile" ] ?? "sanoid.conf";
}
