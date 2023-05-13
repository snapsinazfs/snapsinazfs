// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Snapshots;

/// <summary>
///     Interface for classes that provide snapshot naming properties
/// </summary>
public interface ISnapshotNamingProvider
{
    string Prefix { get; }
    string ComponentSeparator { get; }
    string TemporarySuffix { get; }
    string FrequentSuffix { get; }
    string HourlySuffix { get; }
    string DailySuffix { get; }
    string WeeklySuffix { get; }
    string MonthlySuffix { get; }
    string YearlySuffix { get; }
    string ManualSuffix { get; }
    IFormatProvider TimestampFormatString { get; set; }
}
