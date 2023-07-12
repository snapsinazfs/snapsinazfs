// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     An enumeration of possible snapshot periods
/// </summary>
public enum SnapshotPeriodKind
{
    /// <summary>
    ///     An un-set value. Should be treated as invalid.
    /// </summary>
    /// <value>0</value>
    NotSet = 0,

    /// <summary>
    ///     Snapshots that are taken according to the "frequently" setting
    /// </summary>
    /// <value>1</value>
    Frequent = 1,
    /// <summary>
    ///     Snapshots that are taken according to the "hourly" setting
    /// </summary>
    /// <value>2</value>
    Hourly = 2,

    /// <summary>
    ///     Snapshots that are taken according to the "daily" setting
    /// </summary>
    /// <value>3</value>
    Daily = 3,

    /// <summary>
    ///     Snapshots that are taken according to the "weekly" setting
    /// </summary>
    /// <value>4</value>
    Weekly = 4,

    /// <summary>
    ///     Snapshots that are taken according to the "monthly" setting
    /// </summary>
    /// <value>5</value>
    Monthly = 5,

    /// <summary>
    ///     Snapshots that are taken according to the "yearly" setting
    /// </summary>
    /// <value>6</value>
    Yearly = 6
}
