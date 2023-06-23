// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using Microsoft.Extensions.Configuration;
using NLog;
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
                                                .AddJsonFile( "/usr/local/share/SnapsInAZfs.net/SnapsInAZfs.nlog.json", false, false )
                                                .AddJsonFile( "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", true, false )
                                                .AddJsonFile( Path.Combine( Path.GetFullPath( Environment.GetEnvironmentVariable( "HOME" ) ?? "~/" ), ".config/SnapsInAZfs.net/SnapsInAZfs.nlog.json" ), true, false )
                                                .AddJsonFile( "SnapsInAZfs.nlog.json", true, false )
                                            #endif
                                                .Build( );
#pragma warning restore CA2000
        LogManager.Configuration = new NLogLoggingConfiguration( nlogJsonConfigRoot.GetSection( "NLog" ) );
    }
}
