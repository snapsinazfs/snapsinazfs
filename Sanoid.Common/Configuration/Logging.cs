// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Configuration for logging using NLog
/// </summary>
public static class Logging
{
    /// <summary>
    ///     Configures NLog using Sanoid.nlog.json
    /// </summary>
    public static void ConfigureLogger( )
    {
#pragma warning disable CA2000
        IConfigurationRoot jsonConfigRoot = new ConfigurationManager( )
                                            .SetBasePath( Directory.GetCurrentDirectory( ) )
                                            .AddJsonFile( "Sanoid.nlog.json", false, true )
                                            .Build( );
#pragma warning restore CA2000
        LogManager.Configuration = new NLogLoggingConfiguration( jsonConfigRoot.GetSection( "NLog" ) );
    }
}
