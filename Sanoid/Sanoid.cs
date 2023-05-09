// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Json.More;
using Microsoft.Extensions.Configuration;
using NLog;
using PowerArgs;
using Sanoid;
using Sanoid.Common.Configuration;
using Sanoid.Common.Posix;

Logging.ConfigureLogger();

// Note that logging will be at whatever level is defined in Sanoid.nlog.json until configuration is initialized, regardless of command-line parameters.
Logger logger = LogManager.GetCurrentClassLogger();

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

if ( Configuration.UseSanoidConfiguration )
{
    DirectoryInfo configDirInfo = new( Configuration.SanoidConfigurationPathBase );
    if ( !configDirInfo.Exists )
    {
        if ( argParseReults.Args.ConfigDir != null )
        {
            logger.Fatal( "UseSanoidConfiguration specified, but directory '{0}' specified in --configdir argument does not exist. Sanoid will now exit.", argParseReults.Args.ConfigDir );
            return (int)Errno.ENOENT;
        }

        logger.Fatal( "UseSanoidConfiguration specified, but directory '{0}' specified in Sanoid.json#/SanoidConfigurationPathBase does not exist. Sanoid will now exit.", Configuration.SanoidConfigurationPathBase );
        return (int)Errno.ENOENT;
    }

    //ConfigDir exists. Now check for files.
    FileInfo defaultsFileInfo = new( Path.Combine( Configuration.SanoidConfigurationPathBase, Configuration.SanoidConfigurationDefaultsFile ) );
    logger.Debug( "Checking for existence of {0}", defaultsFileInfo.FullName );
    if ( !defaultsFileInfo.Exists )
    {
        if ( argParseReults.Args.ConfigDir is not null )
        {
            logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} does not exist. Sanoid will now exit.", defaultsFileInfo.FullName );
            return (int)Errno.ENOENT;
        }

        logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} specified in Sanoid.json#/SanoidConfigurationDefaultsFile does not exist in directory {1} specified in Sanoid.json#/SanoidConfigurationPathBase.", Configuration.SanoidConfigurationDefaultsFile, Configuration.SanoidConfigurationPathBase );
        return (int)Errno.ENOENT;
    }

    logger.Debug( "{0} exists.", defaultsFileInfo.FullName );

    //Defaults file exists. Now check for local config file.
    FileInfo localConfigFileInfo = new( Path.Combine( Configuration.SanoidConfigurationPathBase, Configuration.SanoidConfigurationLocalFile ) );
    logger.Debug( "Checking for existence of {0}", localConfigFileInfo.FullName );
    if ( !localConfigFileInfo.Exists )
    {
        if ( argParseReults.Args.ConfigDir is not null )
        {
            logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} does not exist. Sanoid will now exit.", localConfigFileInfo.FullName );
            return (int)Errno.ENOENT;
        }

        logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} specified in Sanoid.json#/SanoidConfigurationLocalFile does not exist in directory {1} specified in Sanoid.json#/SanoidConfigurationPathBase.", Configuration.SanoidConfigurationLocalFile, Configuration.SanoidConfigurationPathBase );
        return (int)Errno.ENOENT;
    }
    logger.Debug( "{0} exists.", localConfigFileInfo.FullName );

}

logger.Fatal( "Not yet implemented." );
logger.Fatal( "Please use the Perl-based sanoid/syncoid for now." );
logger.Fatal( "This program will now exit with an error (status 38 - ENOSYS) to prevent accidental usage in scripts." );

return (int)Errno.ENOSYS;
