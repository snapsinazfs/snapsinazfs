// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Configuration.Snapshots;

/// <summary>
///     Interface for classes that provide snapshot naming properties
/// </summary>
public interface ISnapshotNamingProvider
{
    /// <summary>The string that separates each component of a snapshot name</summary>
    string ComponentSeparator { get; }

    /// <summary>The last component of a daily snapshot's name</summary>
    string DailySuffix { get; }

    /// <summary>The last component of a frequent snapshot's name</summary>
    string FrequentSuffix { get; }

    /// <summary>The last component of an hourly snapshot's name</summary>
    string HourlySuffix { get; }

    /// <summary>The last component of a manual snapshot's name</summary>
    string ManualSuffix { get; }

    /// <summary>The last component of a monthly snapshot's name</summary>
    string MonthlySuffix { get; }

    /// <summary>The first component of the snapshot name</summary>
    string Prefix { get; }

    /// <summary>The last component of a temporary snapshot's name</summary>
    string TemporarySuffix { get; }

    IFormatProvider TimestampFormatString { get; set; }

    /// <summary>The last component of a weekly snapshot's name</summary>
    string WeeklySuffix { get; }

    /// <summary>The last component of a yearly snapshot's name</summary>
    string YearlySuffix { get; }
}
