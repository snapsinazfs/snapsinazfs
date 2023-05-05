using System.Text.Json.Serialization;

using JetBrains.Annotations;

using Json.Schema.Serialization;

namespace Sanoid.Common.Configuration;

/// <summary>
/// Corresponds to the root section of Sanoid.json
/// </summary>
public static class Configuration
{
    static Configuration( )
    {
        UseSanoidConfiguration = JsonConfigurationSections.RootConfiguration.GetBoolean( "UseSanoidConfiguration" );
        SanoidConfigurationPathBase = JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationPathBase" ] ?? "/etc/sanoid";
        SanoidConfigurationDefaultsFile = JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationDefaultsFile" ] ?? "sanoid.defaults.conf";
        SanoidConfigurationLocalFile = JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationLocalFile" ] ?? "sanoid.conf";
        SanoidConfigurationCacheDirectory = JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationCacheDirectory" ] ?? "/var/cache/sanoid";
        SanoidConfigurationRunDirectory = JsonConfigurationSections.RootConfiguration[ "SanoidConfigurationCacheDirectory" ] ?? "/var/run/sanoid";
    }

    /// <summary>
    /// Gets or sets whether Sanoid.net should use ini-formatted configuration files using PERL sanoid's schema.<br />
    /// Corresponds to the /UseSanoidConfiguration property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is true<br />
    /// If <c>true</c>, uses <see cref="SanoidConfigurationDefaultsFile"/> and <see cref="SanoidConfigurationLocalFile"/> in the <see cref="SanoidConfigurationPathBase"/> directory.<br />
    /// If <c>false</c>, uses configuration in Sanoid.json only.
    /// </remarks>
    /// <value>A <see langword="bool"/> indicating whether PERL sanoid's configuration will be respected (<see langword="true"/>) or not (<see langword="false"/>).</value>
    [JsonPropertyName( "UseSanoidConfiguration" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static bool UseSanoidConfiguration { get; [NotNull] set; }

    /// <summary>
    /// Gets or sets the absolute path to the directory containing PERL sanoid's configuration files.<br />
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
    public static string SanoidConfigurationPathBase { get; [NotNull] set; }

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
    public static string SanoidConfigurationDefaultsFile { get; [NotNull] set; }


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
    public static string SanoidConfigurationLocalFile { get; [NotNull] set; }

    /// <summary>
    /// Gets or sets sanoid's cache path.<br />
    /// Corresponds to the /SanoidConfigurationCacheDirectory property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "/var/cache/sanoid"<br />
    /// </remarks>
    /// <value>A <see langword="string"/> indicating the path for sanoid-generated cache files</value>
    [JsonPropertyName( "SanoidConfigurationCacheDirectory" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string SanoidConfigurationCacheDirectory { get; [NotNull] set; }

    /// <summary>
    /// Gets or sets sanoid's run path.<br />
    /// Corresponds to the /SanoidConfigurationRunDirectory property of Sanoid.json.
    /// </summary>
    /// <remarks>
    /// Default value is "/var/run/sanoid"<br />
    /// </remarks>
    /// <value>A <see langword="string"/> indicating the path for sanoid-generated runtime files</value>
    [JsonPropertyName( "SanoidConfigurationRunDirectory" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public static string SanoidConfigurationRunDirectory { get; [NotNull] set; }

}
