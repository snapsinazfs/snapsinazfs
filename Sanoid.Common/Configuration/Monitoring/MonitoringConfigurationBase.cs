// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration.Monitoring;

/// <summary>
///     Base type for monitoring configurations
/// </summary>
public abstract class MonitoringConfigurationBase
{
    protected MonitoringConfigurationBase( string monitorName )
    {
        MonitorName = monitorName;
        _monitorConfigurationSection = JsonConfigurationSections.MonitoringConfiguration.GetRequiredSection( monitorName );
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
    ///     Gets or sets the name of the monitor. A matching object must be defined in Sanoid.json, as an item in the /Monitoring array.
    /// </summary>
    /// <value>A string value representing the name of the monitor configuration</value>
    public string MonitorName { get; set; }

    /// <summary>
    ///     Gets or sets whether or not to monitor snapshots
    /// </summary>
    public bool MonitorSnapshots { get; [NotNull] set; }
}
