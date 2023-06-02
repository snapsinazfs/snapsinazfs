// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PowerArgs;
using Sanoid.Common.Logging;
using Sanoid.Interop.Concurrency;
using Sanoid.Interop.Libc;
using Sanoid.Interop.Libc.Enums;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;


namespace Sanoid;

internal class Program
{
    // Note that logging will be at whatever level is defined in Sanoid.nlog.json until configuration is initialized, regardless of command-line parameters.
    // Desired logging parameters should be set in Sanoid.nlog.json
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static int Main( string[] args )
    {
        LoggingSettings.ConfigureLogger( );

        Logger.Trace("Parsing command-line arguments");
        CommandLineArguments? argParseResults = Args.Parse<CommandLineArguments>( args );

        // The nullability context in PowerArgs is wrong, so this absolutely can be null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ( argParseResults is null )
        {
            Logger.Trace( "Help argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        if ( argParseResults.Help )
        {
            Logger.Trace( "Help argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        if ( argParseResults.Version )
        {
            Logger.Trace( "Version argument provided. Exiting." );
            return (int)Errno.ECANCELED;
        }

        using Mutexes mutexes = Mutexes.Instance;
        using MutexAcquisitionResult mutexResult = Mutexes.GetAndWaitMutex( "Global\\Sanoid.net" );

        switch ( mutexResult.ErrorCode )
        {
            case MutexAcquisitionErrno.Success:
                Logger.Debug( "Succesfully acquired global mutex." );
                break;
            case MutexAcquisitionErrno.IoException:
                Logger.Fatal( mutexResult.Exception, "Exiting due to IOException: {0}", mutexResult.Exception.Message );
                return (int)mutexResult.ErrorCode;
            case MutexAcquisitionErrno.AbandonedMutex:
                Logger.Fatal( mutexResult.Exception, "A previous instance of Sanoid.net exited without properly releasing the mutex. Sanoid.net will now exit after releasing the abandoned mutex. Try running again." );
                return (int)mutexResult.ErrorCode;
            case MutexAcquisitionErrno.WaitHandleCannotBeOpened:
                Logger.Fatal( mutexResult.Exception, "Unable to acquire mutex. Sanoid.net will exit." );
                return (int)mutexResult.ErrorCode;
            case MutexAcquisitionErrno.PossiblyNullMutex:
                Logger.Fatal( "Unable to acquire mutex. Sanoid.net will exit." );
                return (int)mutexResult.ErrorCode;
            case MutexAcquisitionErrno.AnotherProcessIsBusy:
                Logger.Fatal( "Another Sanoid.net process is running. This process will terminate." );
                return (int)mutexResult.ErrorCode;
            case MutexAcquisitionErrno.InvalidMutexNameRequested:
                return (int)mutexResult.ErrorCode;
            default:
                Logger.Fatal( mutexResult.Exception, "Failed to get mutex. Exiting." );
                return (int)mutexResult.ErrorCode;
        }

        // Configuration is built in the following order from various sources.
        // Configurations from all sources are merged, and the final configuration that will be used is the result of the merged configurations.
        // If conflicting items exist in multiple configuration sources, the configuration of the configuration source added latest will
        // override earlier values.
        // Note that nlog-specific configuration is separate, in Sanoid.nlog.json, and is not affected by the configuration specified below,
        // and is loaded/parsed FIRST, before any configuration specified below.
        // See the Sanoid.Common.Logging.LoggingSettings class for nlog configuration details.
        // See documentation for a more detailed explanation with examples.
        // Configuration order:
        // 1. /usr/local/share/Sanoid.net/Sanoid.json   #(Required - Base configuration - Should not be modified by the user)
        // 2. /etc/sanoid/Sanoid.local.json
        // 3. ~/.config/Sanoid.net/Sanoid.user.json     #(Located in executing user's home directory)
        // 4. ./Sanoid.local.json                       #(Located in Sanoid.net's working directory)
        // 6. Command-line arguments passed on invocation of Sanoid.net
        Logger.Debug( "Getting base configuration from files" );
        IConfigurationRoot rootConfiguration = new ConfigurationBuilder( )
                                           #if WINDOWS
                                               .AddJsonFile( "Sanoid.json", true, false )
                                               .AddJsonFile( "Sanoid.local.json", true, false )
                                           #else
                                               .AddJsonFile( "/usr/local/share/Sanoid.net/Sanoid.json", true, false )
                                               .AddJsonFile( "/etc/sanoid/Sanoid.local.json", true, false )
                                               .AddJsonFile( Path.Combine( Path.GetFullPath( Environment.GetEnvironmentVariable( "HOME" ) ?? "~/" ), ".config/Sanoid.net/Sanoid.user.json" ), true, false )
                                               .AddJsonFile( "Sanoid.local.json", true, false )
                                           #endif
                                               .Build( );

        Logger.Trace( "Building settings objects from IConfiguration" );
        SanoidSettings? settings = rootConfiguration.Get<SanoidSettings>( );

        if ( settings is null )
        {
            Logger.Fatal( "Unable to parse settings from JSON" );
            return (int)Errno.EFTYPE;
        }

        ApplyCommandLineArgumentOverrides( in argParseResults, ref settings );

        Logger.Trace( "Getting ZFS command runner for the current environment" );
        IZfsCommandRunner zfsCommandRunner = Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => new ZfsCommandRunner( settings.ZfsPath ),
            _ => new DummyZfsCommandRunner( )
        };

        Logger.Debug( "Using settings: {0}", JsonSerializer.Serialize( settings ) );

        Dictionary<string, Dataset> poolRoots = zfsCommandRunner.GetZfsPoolRoots( );

        Logger.Debug( "Checking zfs properties schema" );
        Dictionary<string, Dictionary<string, ZfsProperty>> missingPoolPropertyCollections = new( );
        bool missingPropertiesFound = false;
        foreach ( ( string poolName, Dataset? pool ) in poolRoots )
        {
            Logger.Info( "Checking properties for pool {0}", poolName );
            Logger.Trace( "Pool {0} current properties collection: {1}", poolName, JsonSerializer.Serialize( pool.Properties ) );
            Dictionary<string, ZfsProperty> missingProperties = new( );

            foreach ( ( string? propertyName, ZfsProperty? property ) in ZfsProperty.DefaultDatasetProperties )
            {
                Logger.Trace( "Checking pool {0} for property {1}", poolName, propertyName );
                if ( pool.HasProperty( propertyName ) )
                {
                    Logger.Trace( "Pool {0} already has property {1}", poolName, propertyName );
                    continue;
                }

                Logger.Debug( "Pool {0} does not have property {1}", poolName, property );
                pool.AddProperty( ZfsProperty.DefaultDatasetProperties[ propertyName ] );
                missingProperties.Add( propertyName, ZfsProperty.DefaultDatasetProperties[ propertyName ] );
            }

            if ( missingProperties.Count > 0 )
            {
                missingPoolPropertyCollections.Add( poolName, missingProperties );
            }

            Logger.Debug( "Finished checking properties for pool {0}", poolName );

            missingPropertiesFound = missingPoolPropertyCollections.Any( );
            bool missingPropertiesFoundForPool = missingProperties.Any( );
            switch ( argParseResults )
            {
                case { CheckZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Warn( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
                    break;
                case { CheckZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    break;
                case { PrepareZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Info( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
                    break;
                case { PrepareZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    break;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when missingPropertiesFoundForPool:
                    Logger.Fatal( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
                    break;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when !missingPropertiesFoundForPool:
                    Logger.Debug( "No missing properties in pool {0}", poolName );
                    break;
            }
        }

        switch ( argParseResults )
        {
            case { CheckZfsProperties: true } when !missingPropertiesFound:
            {
                return (int)Errno.EOK;
            }
            case { CheckZfsProperties: true } when missingPropertiesFound:
            {
                return (int)Errno.ENOATTR;
            }
            case { CheckZfsProperties: false, PrepareZfsProperties: false } when missingPropertiesFound:
            {
                Logger.Fatal( "Missing properties were found in zfs. Cannot continue. Exiting" );
                return (int)Errno.ENOATTR;
            }
            case { PrepareZfsProperties: true }:
            {
                ZfsTasks.UpdateZfsDatasetSchema( settings.DryRun, ref missingPoolPropertyCollections, zfsCommandRunner );
                return (int)Errno.EOK;
            }
        }

        Dictionary<string, Dataset> datasets = zfsCommandRunner.GetZfsDatasetConfiguration( );

//Logger.Debug( "Getting sanoid snapshots" );
//Dictionary<string, Snapshot> snapshots = zfsCommandRunner.GetZfsSanoidSnapshots( ref datasets );
//Logger.Debug( "Finished getting sanoid snapshots" );

        if ( settings is { TakeSnapshots: true } )
        {
            DateTimeOffset currentTimestamp = DateTimeOffset.Now;
            Logger.Debug( "TakeSnapshots is true. Taking daily snapshots for testing purposes using timestamp {0:O}", currentTimestamp );
            ZfsTasks.TakeAllConfiguredSnapshots( zfsCommandRunner, settings, SnapshotPeriod.Daily, currentTimestamp, ref datasets );
        }
        else
        {
            Logger.Warn( "TakeSnapshots is false" );
        }

        Logger.Fatal( "Not yet implemented." );
        Logger.Fatal( "Please use the Perl-based sanoid/syncoid for now." );
        Logger.Fatal( "This program will now exit with an error (status 38 - ENOSYS) to prevent accidental usage in scripts." );

// Let's be tidy and clean up the default mutex ourselves
        Mutexes.ReleaseMutex( );
        Mutexes.DisposeMutexes( );

// Be sure we clean up any mutexes we have acquired, and log warnings for those that this has to deal with.
        return (int)Errno.ENOSYS;
    }

    /// <summary>
    ///     Overrides configuration values specified in configuration files or environment variables with arguments supplied on
    ///     the CLI
    /// </summary>
    /// <param name="settings">The <see cref="SanoidSettings" /> object to get <see cref="TemplateSettings" /> from</param>
    /// <param name="args"></param>
    public static void ApplyCommandLineArgumentOverrides( in CommandLineArguments args, ref SanoidSettings settings )
    {
        Logger.Debug( "Overriding settings using arguments from command line." );

        // Let's go through all args in an order that makes sense
        if ( !string.IsNullOrEmpty( args.CacheDir ) )
        {
            Logger.Debug( "CacheDir argument specified. Value: {0}", args.CacheDir );
            string canonicalCacheDirPath = NativeMethods.CanonicalizeFileName( args.CacheDir );
            Logger.Trace( "CacheDir canonical path: {0}", canonicalCacheDirPath );
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
            Logger.Trace( "CacheDirectory is now {0}", canonicalCacheDirPath );
        }

        if ( args.TakeSnapshots )
        {
            Logger.Debug( "TakeSnapshots argument specified" );

            settings.TakeSnapshots = true;
        }

        if ( args.NoTakeSnapshots )
        {
            Logger.Debug( "NoTakeSnapshots argument specified" );

            settings.TakeSnapshots = false;
        }

        if ( args.PruneSnapshots )
        {
            Logger.Debug( "PruneSnapshots argument specified" );

            settings.PruneSnapshots = true;
        }

        if ( args.NoPruneSnapshots )
        {
            Logger.Debug( "NoPruneSnapshots argument specified" );

            settings.PruneSnapshots = false;
        }

        if ( args.DryRun )
        {
            Logger.Debug( "DryRun argument specified" );

            settings.DryRun = true;
        }
    }
}
