// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Text.Json.Serialization;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     An enumeration of possible snapshot periods
/// </summary>
public enum SnapshotPeriodKind
{
    /// <summary>
    ///     Temporary snapshots taken by SnapsInAZfs.
    /// </summary>
    /// <remarks>Not intended to be used by an end-user.</remarks>
    /// <value>0</value>
    [JsonPropertyName( "temporary" )]
    Temporary,

    /// <summary>
    ///     Snapshots that are taken according to the "frequently" setting
    /// </summary>
    /// <value>1</value>
    [JsonPropertyName( "frequent" )]
    Frequent,

    /// <summary>
    ///     Snapshots that are taken according to the "hourly" setting
    /// </summary>
    /// <value>2</value>
    [JsonPropertyName( "hourly" )]
    Hourly,

    /// <summary>
    ///     Snapshots that are taken according to the "daily" setting
    /// </summary>
    /// <value>3</value>
    [JsonPropertyName( "daily" )]
    Daily,

    /// <summary>
    ///     Snapshots that are taken according to the "weekly" setting
    /// </summary>
    /// <value>4</value>
    [JsonPropertyName( "weekly" )]
    Weekly,

    /// <summary>
    ///     Snapshots that are taken according to the "monthly" setting
    /// </summary>
    /// <value>5</value>
    [JsonPropertyName( "monthly" )]
    Monthly,

    /// <summary>
    ///     Snapshots that are taken according to the "yearly" setting
    /// </summary>
    /// <value>6</value>
    [JsonPropertyName( "yearly" )]
    Yearly,

    /// <summary>
    ///     Snapshots that are taken manually by the user.
    /// </summary>
    /// <value>100</value>
    [JsonPropertyName( "manual" )]
    Manual = 100
}
