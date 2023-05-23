// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration.Monitoring;

internal class Configuration
{
    internal Configuration( IConfiguration monitoringConfigurationSection )
    {
        MonitorConfigurations = new( );
        foreach ( IConfigurationSection section in monitoringConfigurationSection.GetChildren( ) )
        {
            string monitorType = section[ "MonitorType" ]!;
            string monitorName = section.Key;
            switch ( monitorType )
            {
                case "Nagios":
                    MonitorConfigurations.Add( monitorName, new NagiosMonitoringConfiguration( section ) );
                    break;
            }
        }
    }

    internal Dictionary<string, MonitoringConfigurationBase> MonitorConfigurations { get; set; }
}
