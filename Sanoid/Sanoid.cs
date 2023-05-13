// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using PowerArgs;
using Sanoid;
using Sanoid.Common;
using Sanoid.Common.Configuration;
using Sanoid.Common.Posix;
using Sanoid.Common.Zfs;

Logging.ConfigureLogger( );

// Note that logging will be at whatever level is defined in Sanoid.nlog.json until configuration is initialized, regardless of command-line parameters.
Logger logger = LogManager.GetCurrentClassLogger( );

ArgAction<CommandLineArguments> argParseReults = Args.InvokeMain<CommandLineArguments>( args );

//If PowerArgs cancelled the parser invocation, either it handled the help method or it encountered an error.
//Either way, exit now.
if ( argParseReults.Cancelled )
{
    return 0;
}

//Version output handled by the PowerArgs parser invocation, to avoid unnecessary further setup.
//Can just exit now without doing anything else.
if ( argParseReults.Args.Version )
{
    return 0;
}

IConfigurationRoot rootConfiguration = new ConfigurationBuilder( )
                                   #if WINDOWS
                                       .AddJsonFile( "Sanoid.json", true, false )
                                       .AddJsonFile( "Sanoid.local.json", true, false )
                                       .AddEnvironmentVariables( "Sanoid.net:" )
                                   #else
                                       .AddJsonFile( "/usr/local/share/Sanoid.net/Sanoid.json", true, false )
                                       .AddJsonFile( "/etc/sanoid/Sanoid.local.json", true, false )
                                       .AddJsonFile( Path.Combine( Path.GetFullPath( Environment.GetEnvironmentVariable( "HOME" ) ?? "~/" ), ".config/Sanoid.net/Sanoid.user.json" ), true, false )
                                       .AddJsonFile( "Sanoid.local.json", true, false )
                                       .AddEnvironmentVariables( "Sanoid.net:" )
                                   #endif
                                       .Build( );

IZfsCommandRunner zfsCommandRunner = new DummyZfsCommandRunner( );
Configuration sanoidConfiguration = new( rootConfiguration, zfsCommandRunner );
sanoidConfiguration.LoadConfigurationFromIConfiguration( );
sanoidConfiguration.SetValuesFromArgs( argParseReults );

logger.Fatal( "Not yet implemented." );
logger.Fatal( "Please use the Perl-based sanoid/syncoid for now." );
logger.Fatal( "This program will now exit with an error (status 38 - ENOSYS) to prevent accidental usage in scripts." );

return (int)Errno.ENOSYS;
