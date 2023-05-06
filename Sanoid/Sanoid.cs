// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;
using PowerArgs;
using Sanoid;
using Sanoid.Common.Configuration;
using Sanoid.Common.Posix;

Logging.ConfigureLogger();

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
            // 2 is typically ENOENT on linux systems, indicating file or directory not found.
            return (int)Errno.ENOENT;
        }

        logger.Fatal( "UseSanoidConfiguration specified, but directory '{0}' specified in Sanoid.json#/SanoidConfigurationPathBase does not exist. Sanoid will now exit.", Configuration.SanoidConfigurationPathBase );
        return (int)Errno.ENOENT;
    }

    //ConfigDir exists. Now check for files.
    string sanoidDefaultsFileFullPath = Path.Combine( Configuration.SanoidConfigurationPathBase, Configuration.SanoidConfigurationDefaultsFile );
    FileInfo defaultsFileInfo = new( sanoidDefaultsFileFullPath );
    logger.Debug( "Checking for existence of {0}", sanoidDefaultsFileFullPath );
    if ( !defaultsFileInfo.Exists )
    {
        if ( argParseReults.Args.ConfigDir is not null )
        {
            logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} does not exist. Sanoid will now exit.", Path.Combine( argParseReults.Args.ConfigDir, Configuration.SanoidConfigurationDefaultsFile ) );
            return (int)Errno.ENOENT;
        }

        logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} specified in Sanoid.json#/SanoidConfigurationDefaultsFile does not exist in directory {1} specified in Sanoid.json#/SanoidConfigurationPathBase.", Configuration.SanoidConfigurationDefaultsFile, Configuration.SanoidConfigurationPathBase );
        return (int)Errno.ENOENT;
    }

    logger.Debug( "{0} exists.", sanoidDefaultsFileFullPath );
    logger.Debug( "Checking for existence of {0}" );
    //Defaults file exists. Now check for local config file.
    if ( !File.Exists( Path.Combine( Configuration.SanoidConfigurationPathBase, Configuration.SanoidConfigurationLocalFile ) ) )
    {
        if ( argParseReults.Args.ConfigDir is not null )
        {
            logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} does not exist. Sanoid will now exit.", Path.Combine( argParseReults.Args.ConfigDir, Configuration.SanoidConfigurationLocalFile ) );
            return (int)Errno.ENOENT;
        }

        logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} specified in Sanoid.json#/SanoidConfigurationLocalFile does not exist in directory {1} specified in Sanoid.json#/SanoidConfigurationPathBase.", Configuration.SanoidConfigurationLocalFile, Configuration.SanoidConfigurationPathBase );
        return (int)Errno.ENOENT;
    }
}

logger.Error( "Not yet implemented." );
logger.Error( "Please use the Perl-based sanoid/syncoid for now." );
logger.Error( "This program will now exit with an error (status -1) to prevent accidental usage in scripts." );

return -1;
