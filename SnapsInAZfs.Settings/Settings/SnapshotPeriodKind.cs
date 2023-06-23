// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     An enumeration of possible snapshot periods
/// </summary>
public enum SnapshotPeriodKind
{
    /// <summary>
    ///     Temporary snapshots taken by sanoid/syncoid themselves.
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

/// <summary>
///     Extension methods for the <see cref="SnapshotPeriodKind" /> <see langword="enum" />
/// </summary>
public static class SnapshotPeriodKindExtensions
{
    /// <summary>
    ///     Gets a <see cref="SnapshotPeriodKind" /> <see langword="enum" /> value for the provided string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>Faster and slightly more flexible than Enum.Parse</remarks>
    [Pure]
    public static SnapshotPeriodKind ToSnapshotPeriodKind( this string value )
    {
        return value switch
        {
            "frequent" => SnapshotPeriodKind.Frequent,
            "Frequent" => SnapshotPeriodKind.Frequent,
            "frequently" => SnapshotPeriodKind.Frequent,
            "Frequently" => SnapshotPeriodKind.Frequent,
            "hourly" => SnapshotPeriodKind.Hourly,
            "Hourly" => SnapshotPeriodKind.Hourly,
            "daily" => SnapshotPeriodKind.Daily,
            "Daily" => SnapshotPeriodKind.Daily,
            "weekly" => SnapshotPeriodKind.Weekly,
            "Weekly" => SnapshotPeriodKind.Weekly,
            "monthly" => SnapshotPeriodKind.Monthly,
            "Monthly" => SnapshotPeriodKind.Monthly,
            "yearly" => SnapshotPeriodKind.Yearly,
            "Yearly" => SnapshotPeriodKind.Yearly,
            _ => throw new ArgumentOutOfRangeException( nameof( value ), $"Invalid value ({value}) for SnapshotPeriodKind" )
        };
    }

    /// <summary>
    ///     Gets a standardized string representation of a <see cref="SnapshotPeriodKind" /> value, for internal use, in
    ///     English lower case
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString( this SnapshotPeriodKind value )
    {
        return value switch
        {
            SnapshotPeriodKind.Temporary => "temporary",
            SnapshotPeriodKind.Frequent => "frequently",
            SnapshotPeriodKind.Hourly => "hourly",
            SnapshotPeriodKind.Daily => "daily",
            SnapshotPeriodKind.Weekly => "weekly",
            SnapshotPeriodKind.Monthly => "monthly",
            SnapshotPeriodKind.Yearly => "yearly",
            SnapshotPeriodKind.Manual => "manual",
            _ => throw new ArgumentOutOfRangeException( nameof( value ), "Unknown value for SnapshotPeriodKind" )
        };
    }
}
