// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using PowerArgs;
using Sanoid.Common;
using Sanoid.Common.Configuration;
using Sanoid.Common.Zfs;

Logging.ConfigureLogger( );

// Note that logging will be at whatever level is defined in Sanoid.nlog.json until configuration is initialized, regardless of command-line parameters.
// Desired logging parameters should be set in Sanoid.nlog.json
Logger logger = LogManager.GetCurrentClassLogger( );

// PowerArgs takes over execution to parse command-line arguments.
// We're going to cheat and let it parse and then deal with the aftermath.
ArgAction<CommandLineArguments> argParseReults = Args.InvokeMain<CommandLineArguments>( args );

// If PowerArgs cancelled the parser invocation, either it handled the help method or it encountered an error.
// Either way, exit now.
if ( argParseReults.Cancelled )
{
    return 0;
}

// Version output handled by the PowerArgs parser invocation, to avoid unnecessary further setup.
// Can just exit now without doing anything else.
if ( argParseReults.Args.Version )
{
    return 0;
}

ConfigurationValidators.ValidateSanoidConfigurationSchema( );

// Configuration is built in the following order from various sources.
// Configurations from all sources are merged, and the final configuration that will be used is the result of the merged configurations.
// If conflicting items exist in multiple configuration sources, the configuration of the configuration source added latest will
// override earlier values.
// Note that nlog-specific configuration is separate, in Sanoid.nlog.json, and is not affected by the configuration specified below,
// and is loaded/parsed FIRST, before any configuration specified below.
// See the Sanoid.Common.Configuration.Logging class for nlog configuration details.
// See documentation for a more detailed explanation with examples.
// Configuration order:
// 1. /usr/local/share/Sanoid.net/Sanoid.json   #(Required - Base configuration - Should not be modified by the user)
// 2. /etc/sanoid/Sanoid.local.json
// 3. ~/.config/Sanoid.net/Sanoid.user.json     #(Located in executing user's home directory)
// 4. ./Sanoid.local.json                       #(Located in Sanoid.net's working directory)
// 5. Environment variables prefixed with 'Sanoid.net:' and following standard .net configuration nomenclature from there
// 6. Command-line arguments passed on invocation of Sanoid.net

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

#if !WINDOWS
IZfsCommandRunner zfsCommandRunner = new DummyZfsCommandRunner( );
#else
IZfsCommandRunner zfsCommandRunner = new ZfsCommandRunner( rootConfiguration.GetRequiredSection( "PlatformUtilities" ) );
#endif
Configuration sanoidConfiguration = new( rootConfiguration, zfsCommandRunner );
sanoidConfiguration.LoadConfigurationFromIConfiguration( );
sanoidConfiguration.SetValuesFromArgs( argParseReults );

logger.Fatal( "Not yet implemented." );
logger.Fatal( "Please use the Perl-based sanoid/syncoid for now." );
logger.Fatal( "This program will now exit with an error (status 38 - ENOSYS) to prevent accidental usage in scripts." );

return (int)Errno.ENOSYS;
