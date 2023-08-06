#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Text.Json.Serialization;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     Settings class for use with the .net IConfiguration binder
/// </summary>
public sealed record SnapsInAZfsSettings
{
    [JsonPropertyOrder( 5 )]
    public bool Daemonize { get; set; }

    /// <summary>
    ///     Gets or sets how often the timer runs when running as a service. Values greater than 1 minute are not supported and are
    ///     advised against
    /// </summary>
    [JsonPropertyOrder( 6 )]
    public uint DaemonTimerIntervalSeconds { get; set; } = 10;

    /// <summary>
    ///     Gets or sets whether a dry run will be performed, which means no changes will be made to ZFS
    /// </summary>
    [JsonPropertyOrder( 1 )]
    public bool DryRun { get; set; }

    // ReSharper disable once CommentTypo
    /// <summary>
    ///     Gets or sets the local system name SnapsInAZfs will use
    /// </summary>
    /// <remarks>
    ///     This is used for operations involving the snapsinazfs.com:sourcesystem property.<br />
    ///     This setting is mandatory and cannot be an empty or all-whitespace string.<br />
    ///     This setting SHOULD be unique among all systems involved in replicating snapshots managed by SnapsInAZfs, and the recommended
    ///     value is the FQDN of the local system.<br />
    ///     If this value is invalid upon startup, SnapsInAZfs will log an error and terminate.
    /// </remarks>
    [JsonPropertyOrder( 4 )]
    public string LocalSystemName { get; set; } = String.Empty;

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
    [JsonPropertyOrder( 9 )]
    public Dictionary<string, TemplateSettings> Templates { get; set; } = new( );

    /// <summary>
    ///     Gets or sets the path to the zfs utility
    /// </summary>
    [JsonPropertyOrder( 7 )]
    public string ZfsPath { get; set; } = "/usr/local/sbin/zfs";

    /// <summary>
    ///     Gets or sets the path to the zpool utility
    /// </summary>
    [JsonPropertyOrder( 8 )]
    public string ZpoolPath { get; set; } = "/usr/local/sbin/zpool";
}
