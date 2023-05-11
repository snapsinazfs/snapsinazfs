// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration;
using Sanoid.Common.Posix;

namespace Sanoid;

internal static class ConfigurationConverter
{
    //private static Logger Logger = LogManager.GetCurrentClassLogger( );
    //internal static int ConvertPerlSanoidConfigurationToSanoidDotnet( CommandLineArguments argParseReults )
    //{
    //    DirectoryInfo configDirInfo = new( Configuration.ConfigurationPathBase );
    //    if ( !configDirInfo.Exists )
    //    {
    //        if ( argParseReults.ConfigDir != null )
    //        {
    //            Logger.Fatal( "UseSanoidConfiguration specified, but directory '{0}' specified in --configdir argument does not exist. Sanoid will now exit.", argParseReults.ConfigDir );
    //            return (int)Errno.ENOENT;
    //        }

    //        Logger.Fatal( "UseSanoidConfiguration specified, but directory '{0}' specified in Sanoid.json#/SanoidConfigurationPathBase does not exist. Sanoid will now exit.", Configuration.ConfigurationPathBase );
    //        return (int)Errno.ENOENT;
    //    }

    //    //ConfigDir exists. Now check for files.
    //    FileInfo defaultsFileInfo = new( Path.Combine( Configuration.ConfigurationPathBase, Configuration.SanoidConfigurationDefaultsFile ) );
    //    Logger.Debug( "Checking for existence of {0}", defaultsFileInfo.FullName );
    //    if ( !defaultsFileInfo.Exists )
    //    {
    //        if ( argParseReults.ConfigDir is not null )
    //        {
    //            Logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} does not exist. Sanoid will now exit.", defaultsFileInfo.FullName );
    //            return (int)Errno.ENOENT;
    //        }

    //        Logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} specified in Sanoid.json#/SanoidConfigurationDefaultsFile does not exist in directory {1} specified in Sanoid.json#/SanoidConfigurationPathBase.", Configuration.SanoidConfigurationDefaultsFile, Configuration.ConfigurationPathBase );
    //        return (int)Errno.ENOENT;
    //    }

    //    Logger.Debug( "{0} exists.", defaultsFileInfo.FullName );

    //    //Defaults file exists. Now check for local config file.
    //    FileInfo localConfigFileInfo = new( Path.Combine( Configuration.ConfigurationPathBase, Configuration.SanoidConfigurationLocalFile ) );
    //    Logger.Debug( "Checking for existence of {0}", localConfigFileInfo.FullName );
    //    if ( !localConfigFileInfo.Exists )
    //    {
    //        if ( argParseReults.ConfigDir is not null )
    //        {
    //            Logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} does not exist. Sanoid will now exit.", localConfigFileInfo.FullName );
    //            return (int)Errno.ENOENT;
    //        }

    //        Logger.Fatal( "UseSanoidConfiguration specified, but sanoid defaults file {0} specified in Sanoid.json#/SanoidConfigurationLocalFile does not exist in directory {1} specified in Sanoid.json#/SanoidConfigurationPathBase.", Configuration.SanoidConfigurationLocalFile, Configuration.ConfigurationPathBase );
    //        return (int)Errno.ENOENT;
    //    }

    //    Logger.Debug( "{0} exists.", localConfigFileInfo.FullName );

    //    //All sanoid ini files exist. Delegate loading/parsing them to Microsoft.Extensions.Configuration
    //    Logger.Info( "Loading configuration files." );
    //    IConfigurationSection templateDefaultSection = SanoidIniConfiguration.IniConfiguration.GetSection( "template_default" );
    //    Logger.Info( "{0}", templateDefaultSection[ "script_timeout" ] );

    //    return 0;
    //}

}
