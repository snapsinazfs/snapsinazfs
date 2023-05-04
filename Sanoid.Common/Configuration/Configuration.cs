using System.Text.Json.Serialization;

namespace Sanoid.Common.Configuration;

/// <summary>
/// Corresponds to the root section of Sanoid.json
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Gets or sets whether Sanoid.net should use ini-formatted configuration files using PERL sanoid's schema.<para />
    /// Corresponds to the /UseSanoidConfiguration property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is true<para />
    /// If <c>true</c>, uses <see cref="SanoidConfigurationDefaultsFile"/> and <see cref="SanoidConfigurationLocalFile"/> in the <see cref="SanoidConfigurationPathBase"/> directory.<para />
    /// If <c>false</c>, uses configuration in Sanoid.json only - 
    /// </remarks>
    [JsonPropertyName("UseSanoidConfiguration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static bool UseSanoidConfiguration { get; set; } = true;


    /// <summary>
    /// Gets or sets the absolute path to the directory containing PERL sanoid's configuration files.<para />
    /// Corresponds to the /SanoidConfigurationPathBase property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "/etc/sanoid"<para />
    /// Should not contain a trailing slash.<para />
    /// Not guaranteed to work with a relative path. Use an absolute path.
    /// </remarks>
    [JsonPropertyName("SanoidConfigurationPathBase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string SanoidConfigurationPathBase { get; set; } = "/etc/sanoid";


    /// <summary>
    /// Gets or sets the name of the ini-formatted file inside the <see cref="SanoidConfigurationPathBase"/> folder containing PERL sanoid's default configuration.<para />
    /// Corresponds to the /SanoidConfigurationDefaultsFile property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "sanoid.defaults.conf"<para />
    /// Assumed to be a file name only.
    /// </remarks>
    [JsonPropertyName("SanoidConfigurationDefaultsFile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string SanoidConfigurationDefaultsFile { get; set; } = "sanoid.defaults.conf";


    /// <summary>
    /// Gets or sets the name of the ini-formatted file inside the <see cref="SanoidConfigurationPathBase"/> folder containing PERL sanoid's local configuration.<para />
    /// Corresponds to the /SanoidConfigurationLocalFile property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "sanoid.conf"<para />
    /// Assumed to be a file name only.
    /// </remarks>
    [JsonPropertyName("SanoidConfigurationLocalFile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public static string SanoidConfigurationLocalFile { get; set; } = "sanoid.conf";
}