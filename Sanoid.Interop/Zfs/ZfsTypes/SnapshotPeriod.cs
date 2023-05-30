// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     An enumeration of possible <see cref="Snapshot" /> periods
/// </summary>
public enum SnapshotPeriod
{
    /// <summary>
    ///     Temporary snapshots taken by sanoid/syncoid themselves.
    /// </summary>
    /// <remarks>Not intended to be used by an end-user.</remarks>
    /// <value>0</value>
    Temporary,

    /// <summary>
    ///     Snapshots that are taken according to the "frequently" setting in sanoid.conf.
    /// </summary>
    /// <value>1</value>
    Frequent,

    /// <summary>
    ///     Snapshots that are taken according to the "hourly" setting in sanoid.conf.
    /// </summary>
    /// <value>2</value>
    Hourly,

    /// <summary>
    ///     Snapshots that are taken according to the "daily" setting in sanoid.conf.
    /// </summary>
    /// <value>3</value>
    Daily,

    /// <summary>
    ///     Snapshots that are taken according to the "weekly" setting in sanoid.conf.
    /// </summary>
    /// <value>4</value>
    Weekly,

    /// <summary>
    ///     Snapshots that are taken according to the "monthly" setting in sanoid.conf.
    /// </summary>
    /// <value>5</value>
    Monthly,

    /// <summary>
    ///     Snapshots that are taken according to the "yearly" setting in sanoid.conf.
    /// </summary>
    /// <value>6</value>
    Yearly,

    /// <summary>
    ///     Snapshots that are taken manually by the user.
    /// </summary>
    /// <value>100</value>
    Manual = 100
}
