// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

namespace SnapsInAZfs.Settings.Logging;

/// <summary>
///     Configuration for logging using NLog
/// </summary>
public static class LoggingSettings
{
    /// <summary>
    ///     Configures NLog using SnapsInAZfs.nlog.json
    /// </summary>
    public static void ConfigureLogger( )
    {
#pragma warning disable CA2000
        IConfigurationRoot nlogJsonConfigRoot = new ConfigurationManager( )
                                            #if WINDOWS
                                                .AddJsonFile("SnapsInAZfs.nlog.json", true, false)
                                            #else
                                                .AddJsonFile( "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json", false, false )
                                                .AddJsonFile( "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", true, true )
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

        for ( int ruleIndex = 0; ruleIndex < LogManager.Configuration.LoggingRules.Count; ruleIndex++ )
        {
            LoggingRule? rule = LogManager.Configuration.LoggingRules[ ruleIndex ];
            rule?.SetLoggingLevels( level, LogLevel.Off );
        }
    }
}



