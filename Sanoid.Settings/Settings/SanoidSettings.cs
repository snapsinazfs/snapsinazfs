// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Sanoid.Settings.Settings;

/// <summary>
///     Settings class for use with the .net <see cref="IConfiguration" /> binder
/// </summary>
public sealed class SanoidSettings
{
    /// <summary>
    ///     Gets or sets sanoid.net's directory for temporary files
    /// </summary>
    [JsonPropertyOrder( 6 )]
    public required string CacheDirectory { get; set; }

    /// <summary>
    ///     Gets or sets whether a dry run will be performed, which means no changes will be made to ZFS
    /// </summary>
    [JsonPropertyOrder( 1 )]
    public required bool DryRun { get; set; }

    /// <summary>
    ///     Gets or sets the global formatting settings sanoid.net will use
    /// </summary>
    [JsonPropertyOrder( 7 )]
    public required FormattingSettings Formatting { get; set; }

    /// <summary>
    ///     Gets or sets the global PruneSnapshots setting
    /// </summary>
    [JsonPropertyOrder( 3 )]
    public required bool PruneSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the global TakeSnapshots setting
    /// </summary>
    [JsonPropertyOrder( 2 )]
    public required bool TakeSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the templates sub-section
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    [JsonPropertyOrder( 8 )]
    public required Dictionary<string, TemplateSettings> Templates { get; set; } = new( );

    /// <summary>
    ///     Gets or sets the path to the zfs utility
    /// </summary>
    [JsonPropertyOrder( 4 )]
    public required string ZfsPath { get; set; }

    /// <summary>
    ///     Gets or sets the path to the zpool utility
    /// </summary>
    [JsonPropertyOrder( 5 )]
    public required string ZpoolPath { get; set; }
}
