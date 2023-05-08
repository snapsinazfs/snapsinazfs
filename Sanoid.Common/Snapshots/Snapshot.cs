// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Configuration;

namespace Sanoid.Common.Snapshots;

/// <summary>
///     A representation of a ZFS snapshot of a dataset or zvol.
/// </summary>
/// <seealso cref="SnapshotPeriod" />
/// <seealso cref="DateTimeOffset" />
public class Snapshot
{
    /// <summary>
    ///     Gets the full name of this <see cref="Snapshot" />, using configured formatting options.
    /// </summary>
    /// <value>A string of the form <c>ZfsPath@Prefix_Timestamp_Period</c></value>
    public string FullName => $"{ZfsPath}@{ShortName}";

    /// <summary>
    ///     Gets or sets the <see cref="SnapshotPeriod" />of this <see cref="Snapshot" />, denoting the periodicity of this
    ///     Snapshot.
    /// </summary>
    /// <value>A <see cref="SnapshotPeriod" /> denoting the periodicity of this Snapshot</value>
    public SnapshotPeriod Period { get; set; }

    /// <summary>
    ///     Gets the short name (everything after "@") of this <see cref="Snapshot" />, using configured formatting options.
    /// </summary>
    /// <returns>A string of the form <c>prefix_timestamp_kind</c></returns>
    /// <exception cref="InvalidOperationException">If an unexpected value is supplied to <see cref="Period" /></exception>
    /// <value>
    ///     A <see langword="string" /> representing the name of the snapshot, without the <see cref="ZfsPath" /> component
    ///     or the @ symbol
    /// </value>
    public string ShortName =>
        Period switch
        {
            SnapshotPeriod.Temporary => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.TemporarySuffix}",
            SnapshotPeriod.Frequent => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.FrequentSuffix}",
            SnapshotPeriod.Hourly => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.HourlySuffix}",
            SnapshotPeriod.Daily => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.DailySuffix}",
            SnapshotPeriod.Weekly => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.WeeklySuffix}",
            SnapshotPeriod.Monthly => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.MonthlySuffix}",
            SnapshotPeriod.Yearly => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.YearlySuffix}",
            SnapshotPeriod.Manual => $"{SnapshotNaming.Prefix}{SnapshotNaming.ComponentSeparator}{Timestamp.ToSnapshotDateTimeString( )}{SnapshotNaming.ComponentSeparator}{SnapshotNaming.ManualSuffix}",
            _ => throw new InvalidOperationException( )
        };

    /// <summary>
    ///     Gets or sets the absolute timestamp of this <see cref="Snapshot" />, as a <see cref="DateTimeOffset" />.<br />
    ///     By default, uses UTC offset.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> indicating when this snapshot was taken</value>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the name of the zvol or dataset this <see cref="Snapshot" /> belongs to.
    /// </summary>
    /// <value>A string indicating the ZFS path to the dataset or zvol this snapshot belongs to.</value>
    public string? ZfsPath { get; set; }
}