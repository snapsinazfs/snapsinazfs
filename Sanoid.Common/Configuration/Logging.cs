using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;

namespace Sanoid.Common.Configuration
{
    /// <summary>
    /// Configuration for logging using NLog
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Configures NLog using Sanoid.nlog.json
        /// </summary>
        public static void ConfigureLogger( )
        {
            IConfigurationRoot jsonConfigRoot = new ConfigurationManager()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Sanoid.nlog.json", false, true)
                .Build();
            LogManager.Configuration = new NLogLoggingConfiguration( jsonConfigRoot.GetSection( "NLog" ) );
        }
    }
}
