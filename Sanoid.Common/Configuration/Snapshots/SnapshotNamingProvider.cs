// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

#pragma warning disable CS8618
namespace Sanoid.Common.Configuration.Snapshots;

/// <summary>
///     Interface for classes that provide snapshot naming properties
/// </summary>
public class SnapshotNamingProvider
{
    /// <summary>The string that separates each component of a snapshot name</summary>
    public string ComponentSeparator { get; }

    /// <summary>The last component of a daily snapshot's name</summary>
    public string DailySuffix { get; }

    /// <summary>The last component of a frequent snapshot's name</summary>
    public string FrequentSuffix { get; }

    /// <summary>The last component of an hourly snapshot's name</summary>
    public string HourlySuffix { get; }

    /// <summary>The last component of a manual snapshot's name</summary>
    public string ManualSuffix { get; }

    /// <summary>The last component of a monthly snapshot's name</summary>
    public string MonthlySuffix { get; }

    /// <summary>The first component of the snapshot name</summary>
    public string Prefix { get; }

    /// <summary>The last component of a temporary snapshot's name</summary>
    public string TemporarySuffix { get; }

    /// <summary>Gets or sets the <see cref="IFormatProvider" /> used to create the timestamp portion of a snapshot's name.</summary>
    public string TimestampFormatString { get; set; }

    /// <summary>The last component of a weekly snapshot's name</summary>
    public string WeeklySuffix { get; }

    /// <summary>The last component of a yearly snapshot's name</summary>
    public string YearlySuffix { get; }
}
#pragma warning restore CS8618
