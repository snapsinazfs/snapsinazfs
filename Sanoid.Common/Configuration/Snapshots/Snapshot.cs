// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Configuration.Snapshots;

/// <summary>
///     A representation of a ZFS snapshot of a dataset or zvol.
/// </summary>
/// <seealso cref="SnapshotPeriod" />
/// <seealso cref="DateTimeOffset" />
public class Snapshot
{
    /// <summary>
    /// Creates a new Snapshot configuration object, with the specified <see cref="SnapshotNamingProvider"/>
    /// </summary>
    /// <param name="namingProvider"></param>
    public Snapshot(SnapshotNamingProvider namingProvider)
    {
        _namingProvider = namingProvider;
    }

    private readonly SnapshotNamingProvider _namingProvider;

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
    public string ShortName
    {
        get
        {
            string withoutSuffix = $"{_namingProvider.Prefix}{_namingProvider.ComponentSeparator}{Timestamp.ToString( _namingProvider.TimestampFormatString )}{_namingProvider.ComponentSeparator}";
            return Period switch
            {
                SnapshotPeriod.Temporary => $"{withoutSuffix}{_namingProvider.TemporarySuffix}",
                SnapshotPeriod.Frequent => $"{withoutSuffix}{_namingProvider.FrequentSuffix}",
                SnapshotPeriod.Hourly => $"{withoutSuffix}{_namingProvider.HourlySuffix}",
                SnapshotPeriod.Daily => $"{withoutSuffix}{_namingProvider.DailySuffix}",
                SnapshotPeriod.Weekly => $"{withoutSuffix}{_namingProvider.WeeklySuffix}",
                SnapshotPeriod.Monthly => $"{withoutSuffix}{_namingProvider.MonthlySuffix}",
                SnapshotPeriod.Yearly => $"{withoutSuffix}{_namingProvider.YearlySuffix}",
                SnapshotPeriod.Manual => $"{withoutSuffix}{_namingProvider.ManualSuffix}",
                _ => throw new InvalidOperationException( )
            };
        }
    }

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
