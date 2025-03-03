#region MIT LICENSE

// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

namespace SnapsInAZfs.Settings;

using System.Text.Json.Serialization;

/// <summary>
///     Settings class for use with the .net IConfiguration binder
/// </summary>
[JsonSerializable ( typeof (SnapsInAZfsSettings) )]
[PublicAPI]
public sealed record SnapsInAZfsSettings
{
    [JsonPropertyOrder ( 5 )]
    public bool Daemonize { get; set; }

    /// <summary>
    ///     Gets or sets how often the timer runs when running as a service. Values greater than 1 minute are not supported and are
    ///     advised against
    /// </summary>
    [JsonPropertyOrder ( 6 )]
    public uint DaemonTimerIntervalSeconds { get; set; } = 10;

    /// <summary>
    ///     Gets or sets whether a dry run will be performed, which means no changes will be made to ZFS
    /// </summary>
    [JsonPropertyOrder ( 1 )]
    public bool DryRun { get; set; }

    /// <summary>
    ///     Gets or sets the local system name SnapsInAZfs will use.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This setting is mandatory ***(EVEN IF NOT USING REPLICATION)*** and cannot be an empty or all-whitespace string.
    ///     </para>
    ///     <para>
    ///         This value is critical for proper operation of SIAZ, whether replication is currently or ever has been in use or not, as
    ///         it is part of the logic used to determine if a snapshot is eligible for pruning or not, and is written explicitly to
    ///         every snapshot SIAZ creates.<br/>
    ///         Snapshots missing this property MAY be ignored by SIAZ entirely, for any or all purposes.
    ///     </para>
    ///     <para>
    ///         In addition to its definition in this property, this value WILL be stored in ZFS properties of snapshots created by SIAZ
    ///         and MAY be stored in one or more additional places, such as configuration files and other ZFS properties, as needed.
    ///     </para>
    ///     <para>
    ///         In replication configurations, this setting SHOULD be unique between each pair of systems using SIAZ to manage snapshots,
    ///         whether replication is performed using SIAZ or another method, or else SIAZ will not be able to properly differentiate
    ///         locally-created snapshots from snapshots received from another system, as well as other potentially unwanted or undefined
    ///         behaviors.
    ///     </para>
    ///     <para>
    ///         The recommended value for this property is the fully-qualified DNS name of the local system formatted in accordance with
    ///         RFC8499 for the "Global DNS", with or without the terminating root dot namespace octet.
    ///     </para>
    ///     <para>
    ///         Valid values must validate against the following .net regular expression: `([0-9A-Za-z_-]+)+\.?`.
    ///     </para>
    ///     <para>
    ///         While SIAZ MAY terminate if invalid values are encountered in operation, that behavior is not guaranteed for any value,
    ///         and deviation from the recommended FQDN setting is unsupported, undefined, and is at your own risk.
    ///     </para>
    /// </remarks>
    [JsonPropertyOrder ( 4 )]
    public string LocalSystemName { get; set; } = string.Empty;

    [JsonPropertyOrder ( 10 )]
    public MonitoringSettings Monitoring { get; set; } = new ( ) { EnableHttp = false };

    /// <summary>
    ///     Gets or sets the global PruneSnapshots setting
    /// </summary>
    [JsonPropertyOrder ( 3 )]
    public bool PruneSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the global TakeSnapshots setting
    /// </summary>
    [JsonPropertyOrder ( 2 )]
    public bool TakeSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets the templates sub-section
    /// </summary>

    // ReSharper disable once CollectionNeverUpdated.Global
    [JsonPropertyOrder ( 9 )]
    public Dictionary<string, TemplateSettings> Templates { get; set; } = new ( );

    /// <summary>
    ///     Gets or sets the path to the zfs utility, as a fully-qualified path or the special value `auto`, which triggers built-in
    ///     auto-detection functionality.
    /// </summary>
    /// <remarks>
    ///     While this type does not implicitly validate this property, SIAZ will perform at least basic validation of it at startup.
    /// </remarks>
    [JsonPropertyOrder ( 7 )]
    public string ZfsPath { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the path to the zpool utility, as a fully-qualified path or the special value `auto`, which triggers built-in
    ///     auto-detection functionality.
    /// </summary>
    /// <remarks>
    ///     While this type does not implicitly validate this property, SIAZ will perform at least basic validation of it at startup.
    /// </remarks>
    [JsonPropertyOrder ( 8 )]
    public string ZpoolPath { get; set; } = string.Empty;
}
