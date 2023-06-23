// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     Settings class for use with the .net <see cref="IConfiguration" /> binder
/// </summary>
public record SnapsInAZfsSettings
{
    /// <summary>
    ///     Gets or sets whether a dry run will be performed, which means no changes will be made to ZFS
    /// </summary>
    [JsonPropertyOrder( 1 )]
    public bool DryRun { get; set; }

    /// <summary>
    ///     Gets or sets the global PruneSnapshots setting
    /// </summary>
    [JsonPropertyOrder( 3 )]
    public bool PruneSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the global TakeSnapshots setting
    /// </summary>
    [JsonPropertyOrder( 2 )]
    public bool TakeSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the templates sub-section
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    [JsonPropertyOrder( 6 )]
    public Dictionary<string, TemplateSettings> Templates { get; set; } = new( );

    /// <summary>
    ///     Gets or sets the path to the zfs utility
    /// </summary>
    [JsonPropertyOrder( 4 )]
    public string ZfsPath { get; set; } = "/usr/local/sbin/zfs";

    /// <summary>
    ///     Gets or sets the path to the zpool utility
    /// </summary>
    [JsonPropertyOrder( 5 )]
    public string ZpoolPath { get; set; } = "/usr/local/sbin/zpool";
}
