// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration.Monitoring;

/// <summary>
///     Base type for monitoring configurations
/// </summary>
public abstract class MonitoringConfigurationBase
{
    /// <summary>
    ///     Creates a new instance of a <see cref="MonitoringConfigurationBase" />
    /// </summary>
    /// <param name="monitorName">The key value for the monitor in Sanoid.json#/Monitoring</param>
    protected MonitoringConfigurationBase( IConfigurationSection singleMonitorConfigurationSection )
    {
        MonitorName = singleMonitorConfigurationSection.Key;
        _monitorConfigurationSection = singleMonitorConfigurationSection;
        MonitorCapacity = _monitorConfigurationSection.GetBoolean( "Capacity" );
        MonitorHealth = _monitorConfigurationSection.GetBoolean( "Health" );
        MonitorSnapshots = _monitorConfigurationSection.GetBoolean( "Snapshots" );
    }

    private readonly IConfigurationSection _monitorConfigurationSection;

    /// <summary>
    ///     Gets or sets whether or not to monitor pool capacity
    /// </summary>
    public bool MonitorCapacity { get; [NotNull] set; }

    /// <summary>
    ///     Gets or sets whether or not to monitor health
    /// </summary>
    public bool MonitorHealth { get; [NotNull] set; }

    /// <summary>
    ///     Gets or sets the name of the monitor. A matching object must be defined in Sanoid.json, as an item in the
    ///     /Monitoring dictionary.
    /// </summary>
    /// <value>A string value representing the name of the monitor configuration</value>
    public string MonitorName { get; set; }

    /// <summary>
    ///     Gets or sets whether or not to monitor snapshots
    /// </summary>
    public bool MonitorSnapshots { get; [NotNull] set; }
}
