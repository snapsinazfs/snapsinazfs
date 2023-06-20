// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using NLog.Config;
using NLog.Extensions.Logging;

namespace Sanoid.Common.Logging;

/// <summary>
///     Configuration for logging using NLog
/// </summary>
public static class LoggingSettings
{
    /// <summary>
    ///     Configures NLog using Sanoid.nlog.json
    /// </summary>
    public static void ConfigureLogger( )
    {
#pragma warning disable CA2000
        IConfigurationRoot nlogJsonConfigRoot = new ConfigurationManager( )
                                            #if WINDOWS
                                                .AddJsonFile( "Sanoid.nlog.json", true, false )
                                            #else
                                                .AddJsonFile( "/usr/local/share/Sanoid.net/Sanoid.nlog.json", false, false )
                                                .AddJsonFile( "/etc/sanoid/Sanoid.nlog.json", true, false )
                                                .AddJsonFile( "Sanoid.nlog.json", true, false )
                                                .AddJsonFile( Path.Combine( Path.GetFullPath( Environment.GetEnvironmentVariable( "HOME" ) ?? "~/" ), ".config/Sanoid.net/Sanoid.nlog.json" ), true, false )
                                            #endif
                                                .Build( );
#pragma warning restore CA2000
        LogManager.Configuration = new NLogLoggingConfiguration( nlogJsonConfigRoot.GetSection( "NLog" ) );
    }

    public static void OverrideConsoleLoggingLevel( LogLevel level )
    {
        if ( LogManager.Configuration == null )
        {
            return;
        }

        foreach ( LoggingRule? rule in LogManager.Configuration.LoggingRules )
        {
            rule?.SetLoggingLevels( level, LogLevel.Off );
        }
    }
}
