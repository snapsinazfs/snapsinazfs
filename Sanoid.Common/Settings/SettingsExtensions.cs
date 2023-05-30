// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using System.Text.Json.Serialization;
using PowerArgs;
using Sanoid.Interop.Libc;
using Sanoid.Interop.Libc.Enums;
using Sanoid.Settings.Settings;

namespace Sanoid.Common.Settings;

public static class SettingsExtensions
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Overrides configuration values specified in configuration files or environment variables with arguments supplied on
    ///     the CLI
    /// </summary>
    /// <param name="argParseReults"></param>
    public static void SetValuesFromArgs( this SanoidSettings settings, ArgAction<CommandLineArguments> argParseReults )
    {
        Logger.Debug( "Overriding settings using arguments from command line." );
        Logger.Trace( "Arguments object: {0}", JsonSerializer.Serialize( argParseReults.Args, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull } ) );
        // Let's go through all args in an order that makes sense
        CommandLineArguments args = argParseReults.Args;
        if ( !string.IsNullOrEmpty( args.CacheDir ) )
        {
            Logger.Debug( "CacheDir argument specified. Value: {0}", args.CacheDir );
            string canonicalCacheDirPath = NativeMethods.CanonicalizeFileName( args.CacheDir );
            Logger.Debug( "CacheDir canonical path: {0}", canonicalCacheDirPath );
            if ( !Directory.Exists( canonicalCacheDirPath ) )
            {
                string badDirectoryMessage = $"CacheDir argument value {canonicalCacheDirPath} is a non-existent directory. Program will terminate.";
                Logger.Error( badDirectoryMessage );
                throw new DirectoryNotFoundException( badDirectoryMessage );
            }

            if ( NativeMethods.EuidAccess( canonicalCacheDirPath, UnixFileTestMode.Read ) != 0 )
            {
                string cantReadDirMessage = $"CacheDir {canonicalCacheDirPath} is not readable by the current user {Environment.UserName}. Program will terminate.";
                Logger.Error( cantReadDirMessage );
                throw new UnauthorizedAccessException( cantReadDirMessage );
            }

            if ( NativeMethods.EuidAccess( canonicalCacheDirPath, UnixFileTestMode.Write ) != 0 )
            {
                string cantWriteDirMessage = $"CacheDir {canonicalCacheDirPath} is not writeable by the current user {Environment.UserName}. Program will terminate.";
                Logger.Error( cantWriteDirMessage );
                throw new UnauthorizedAccessException( cantWriteDirMessage );
            }

            settings.CacheDirectory = args.CacheDir;
            Logger.Debug( "CacheDirectory is now {0}", canonicalCacheDirPath );
        }

        if ( args.TakeSnapshots is not null )
        {
            Logger.Debug( "TakeSnapshots argument specified. Value: {0}", args.TakeSnapshots );

            settings.TakeSnapshots = args.TakeSnapshots!.Value;
            Logger.Debug( "TakeSnapshots is now {0}", settings.TakeSnapshots );
        }
    }
}
