// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PowerArgs;
using Sanoid;
using Sanoid.Common.Logging;
using Sanoid.Interop.Concurrency;
using Sanoid.Interop.Libc.Enums;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;

// Note that logging will be at whatever level is defined in Sanoid.nlog.json until configuration is initialized, regardless of command-line parameters.
// Desired logging parameters should be set in Sanoid.nlog.json
LoggingSettings.ConfigureLogger( );
Logger logger = LogManager.GetCurrentClassLogger( );

using Mutexes mutexes = Mutexes.Instance;
using MutexAcquisitionResult mutexResult = Mutexes.GetAndWaitMutex( "Global\\Sanoid.net" );

switch ( mutexResult.ErrorCode )
{
    case MutexAcquisitionErrno.Success:
        logger.Debug( "Succesfully acquired global mutex." );
        break;
    case MutexAcquisitionErrno.IoException:
        logger.Fatal( mutexResult.Exception, "Exiting due to IOException: {0}", mutexResult.Exception.Message );
        return (int)mutexResult.ErrorCode;
    case MutexAcquisitionErrno.AbandonedMutex:
        logger.Fatal( mutexResult.Exception, "A previous instance of Sanoid.net exited without properly releasing the mutex. Sanoid.net will now exit after releasing the abandoned mutex. Try running again." );
        return (int)mutexResult.ErrorCode;
    case MutexAcquisitionErrno.WaitHandleCannotBeOpened:
        logger.Fatal( mutexResult.Exception, "Unable to acquire mutex. Sanoid.net will exit." );
        return (int)mutexResult.ErrorCode;
    case MutexAcquisitionErrno.PossiblyNullMutex:
        logger.Fatal( "Unable to acquire mutex. Sanoid.net will exit." );
        return (int)mutexResult.ErrorCode;
    case MutexAcquisitionErrno.AnotherProcessIsBusy:
        logger.Fatal( "Another Sanoid.net process is running. This process will terminate." );
        return (int)mutexResult.ErrorCode;
    case MutexAcquisitionErrno.InvalidMutexNameRequested:
        return (int)mutexResult.ErrorCode;
    default:
        logger.Fatal( mutexResult.Exception, "Failed to get mutex. Exiting." );
        return (int)mutexResult.ErrorCode;
}

// PowerArgs takes over execution to parse command-line arguments.
// We're going to cheat and let it parse and then deal with the aftermath.
ArgAction<CommandLineArguments> argParseReults = Args.InvokeMain<CommandLineArguments>( args );

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
if ( argParseReults is null )
{
    logger.Debug( "Arg parse results object was null. Exiting." );
    return 0;
}

if ( argParseReults.Args is null )
{
    logger.Debug( "args object was null. Exiting." );
    return 0;
}
// ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

// If PowerArgs cancelled the parser invocation, either it handled the help method or it encountered an error.
// Either way, exit now.
if ( argParseReults.Cancelled )
{
    logger.Trace( "Help method invoked by command-line argument. Exiting with status {0}", Errno.ECANCELED );
    return (int)Errno.ECANCELED;
}

// Version output handled by the PowerArgs parser invocation, to avoid unnecessary further setup.
// Can just exit now without doing anything else.
if ( argParseReults.Args.Version )
{
    logger.Trace( "Version method invoked by command-line argument. Exiting with status {0}", Errno.ECANCELED );
    return (int)Errno.ECANCELED;
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
logger.Debug( "Building base configuration from files and environment variables." );
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

logger.Debug( "Building settings objects from IConfiguration" );
SanoidSettings settings = rootConfiguration.Get<SanoidSettings>( );

logger.Debug( "Getting ZFS command runner for the current environment" );
IZfsCommandRunner zfsCommandRunner = Environment.OSVersion.Platform switch
{
    PlatformID.Unix => new ZfsCommandRunner( settings.ZfsPath ),
    _ => new DummyZfsCommandRunner( )
};

logger.Debug( "Using settings: {0}", JsonSerializer.Serialize( settings ) );
Dictionary<string, Dataset> poolRoots = zfsCommandRunner.GetZfsPoolRoots( );
bool missingPropertiesFound = false;
logger.Debug( "Requested check of zfs properties schema" );
Dictionary<string, Dictionary<string, ZfsProperty>> missingPoolPropertyCollections = new( );
foreach ( ( string poolName, Dataset? pool ) in poolRoots )
{
    logger.Debug( "Checking properties for pool {0}", poolName );
    logger.Debug( "Pool {0} current properties collection: {1}", poolName, JsonSerializer.Serialize( pool.Properties ) );
    Dictionary<string, ZfsProperty> missingProperties = new( );

    foreach ( ( string? propertyName, ZfsProperty? property ) in ZfsProperty.SanoidDefaultDatasetProperties )
    {
        logger.Debug( "Checking pool {0} for property {1}", poolName, propertyName );
        if ( pool.HasProperty( propertyName ) )
        {
            logger.Debug( "Pool {0} already has property {1}", poolName, propertyName );
            continue;
        }

        logger.Debug( "Pool {0} does not have property {1}", poolName, propertyName );
        missingPropertiesFound = true;
        pool.AddProperty( ZfsProperty.SanoidDefaultDatasetProperties[ propertyName ] );
        missingProperties.Add( propertyName, ZfsProperty.SanoidDefaultDatasetProperties[ propertyName ] );
    }

    if ( missingProperties.Count > 0 )
    {
        missingPoolPropertyCollections.Add( poolName, missingProperties );
    }

    logger.Debug( "Finished checking properties for pool {0}", poolName );

    if ( argParseReults.Args.CheckZfsProperties )
    {
        logger.Warn( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
    }
    else if ( !argParseReults.Args.PrepareZfsProperties )
    {
        logger.Fatal( "Pool {0} is missing the following properties: {1}", poolName, string.Join( ", ", missingProperties.Keys ) );
    }
}

logger.Debug( "Finished checking zfs properties schema for all pool roots" );
if ( argParseReults.Args.CheckZfsProperties )
{
    return (int)Errno.EOK;
}

if ( !argParseReults.Args.CheckZfsProperties && !argParseReults.Args.PrepareZfsProperties && missingPropertiesFound )
{
    logger.Fatal( "Missing properties were found in zfs. Cannot continue. Exiting" );
    return (int)Errno.ENOATTR;
}

if ( argParseReults.Args.PrepareZfsProperties )
{
    logger.Debug( "Requested update of zfs properties schema" );
    foreach ( ( string poolName, Dictionary<string, ZfsProperty> propertiesToAdd ) in missingPoolPropertyCollections )
    {
        logger.Debug( "Updating properties for pool {0}", poolName );
        zfsCommandRunner.SetZfsProperty( poolName, propertiesToAdd.Values.ToArray( ) );

        logger.Debug( "Finished updating properties for pool {0}", poolName );
    }

    logger.Debug( "Finished updating zfs properties schema for all pool roots" );
    return (int)Errno.EOK;
}

logger.Debug( "Checking for command-line overrides" );
argParseReults.Args.UpdateSettingsFromArgs( settings );

Dictionary<string, Dataset> datasets = zfsCommandRunner.GetZfsDatasetConfiguration( );

if ( settings is { TakeSnapshots: true } )
{
    DateTimeOffset currentTimestamp = DateTimeOffset.Now;
    logger.Debug( "TakeSnapshots is true. Taking daily snapshots for testing purposes using timestamp {0:O}", currentTimestamp );
    SnapshotTasks.TakeAllConfiguredSnapshots( zfsCommandRunner, datasets, settings, SnapshotPeriod.Daily, currentTimestamp );
}
else
{
    logger.Warn( "TakeSnapshots is false" );
}

logger.Debug("Getting sanoid snapshots");
Dictionary<string, Snapshot> snapshots = zfsCommandRunner.GetZfsSanoidSnapshots( );
logger.Debug("Finished getting sanoid snapshots");

logger.Warn( "Snapshots: {0}", JsonSerializer.Serialize( snapshots ) );

logger.Fatal( "Not yet implemented." );
logger.Fatal( "Please use the Perl-based sanoid/syncoid for now." );
logger.Fatal( "This program will now exit with an error (status 38 - ENOSYS) to prevent accidental usage in scripts." );

// Let's be tidy and clean up the default mutex ourselves
Mutexes.ReleaseMutex( );
Mutexes.DisposeMutexes( );

// Be sure we clean up any mutexes we have acquired, and log warnings for those that this has to deal with.
return (int)Errno.ENOSYS;
