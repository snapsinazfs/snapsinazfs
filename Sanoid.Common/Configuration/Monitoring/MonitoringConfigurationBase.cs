// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Configuration.Monitoring;

/// <summary>
/// Base type for monitoring configurations
/// </summary>
internal abstract class MonitoringConfigurationBase
{
    protected MonitoringConfigurationBase( string monitorName )
    {
        MonitorName = monitorName;
    }
    internal bool MonitorCapacity { get; set; }
    internal bool MonitorHealth { get; set; }
    internal bool MonitorSnapshots { get; set; }

    /// <summary>
    /// Gets or sets the name of the monitor. A matching section must be defined in Sanoid.json, as a child of /Monitoring
    /// </summary>
    /// <value>A string value representing the name of the monitor configuration</value>
    internal string MonitorName { get; set; }
}