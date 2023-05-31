// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Reflection;
using JetBrains.Annotations;
using NLog;
using PowerArgs;

#pragma warning disable CS1591

namespace Sanoid.Settings.Settings;

/// <summary>
///     The command line arguments that sanoid can accept
/// </summary>
[ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
public class CommandLineArguments
{
    /// <summary>
    ///     Gets or sets the cache directory to use, overriding the same setting from all other levels
    /// </summary>
    [ArgDescription( "Cache directory for sanoid" )]
    [ArgShortcut( "--cache-dir" )]
    public string? CacheDir { get; set; }

    [ArgDescription( "Checks the property schema for sanoid.net in zfs and reports any missing properties for pool roots." )]
    [ArgShortcut( "--check-zfs-properties" )]
    public bool CheckZfsProperties { get; set; }

    /// <summary>
    ///     Gets or sets the configuration directory to use, overriding the same setting from all other levels
    /// </summary>
    [ArgDescription( "Base configuration directory for sanoid" )]
    [ArgShortcut( "--configdir" )]
    [ArgShortcut( "--config-dir" )]
    public string? ConfigDir { get; set; }

    [ArgDescription( "Create snapshots and prune expired snapshots. Equivalent to --take-snapshots --prune-snapshots" )]
    [ArgShortcut( "--cron" )]
    public bool? Cron { get; set; }

    [ArgDescription( "Debug level output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--debug" )]
    [ArgCantBeCombinedWith( "Verbose|Quiet|ReallyQuiet" )]
    public bool? Debug { get; set; }

    [ArgDescription( "Prunes expired snapshots, even if their parent datasets are currently involved in a send or receive operation. Implies --prune-snapshots as well." )]
    [ArgShortcut( "--force-prune" )]
    [ArgShortcut( "--force-prune-snapshots" )]
    public bool? ForcePrune { get; set; }

    [ArgDescription( "This clears out sanoid's zfs snapshot listing cache. This is normally not needed." )]
    [ArgShortcut( "--force-update" )]
    public bool? ForceUpdate { get; set; }

    [ArgDescription( "Shows this help" )]
    [ArgShortcut( "-h" )]
    [ArgShortcut( "--help" )]
    [HelpHook]
    public bool Help { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the capacity of the zpool your filesystems are on. It only monitors pools that are configured in the sanoid.conf file." )]
    [ArgShortcut( "--monitor-capacity" )]
    [ArgShortcut( "--monitor-capacity-nagios" )]
    public bool? MonitorCapacity { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the health of the zpool your filesystems are on. It only monitors filesystems that are configured in the sanoid.conf file." )]
    [ArgShortcut( "--monitor-health" )]
    [ArgShortcut( "--monitor-health-nagios" )]
    public bool? MonitorHealth { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the health of your snapshots." )]
    [ArgShortcut( "--monitor-snapshots" )]
    [ArgShortcut( "--monitor-snapshots-nagios" )]
    public bool? MonitorSnapshots { get; set; }

    [ArgDescription( "Updates the property schema for sanoid.net in zfs, using default values. Will not overwrite values that are already set." )]
    [ArgShortcut( "--prepare-zfs-properties" )]
    public bool PrepareZfsProperties { get; set; }

    [ArgDescription( "Prunes expired snapshots, except for snapshots of datasets currently involved in a send or receive operation." )]
    [ArgShortcut( "--prune-snapshots" )]
    public bool? PruneSnapshots { get; set; }

    [ArgDescription( "Suppress non-error output. WILL WARN BEFORE SETTING IS APPLIED. Configure in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose|DryRun" )]
    public bool? Quiet { get; set; }

    [ArgDescription( "Skip creation/deletion of snapshots (Simulate)." )]
    [ArgShortcut( "--readonly" )]
    [ArgShortcut( "--read-only" )]
    [ArgShortcut( "--dryrun" )]
    [ArgShortcut( "--dry-run" )]
    public bool? DryRun { get; set; }

    [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json to set for normal usage. Will not warn when used." )]
    [ArgShortcut( "--really-quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose|DryRun" )]
    public bool? ReallyQuiet { get; set; }

    [ArgDescription( "Will make sanoid take snapshots, but will not prune unless --prune-snapshots is also specified." )]
    [ArgShortcut( "--take-snapshots" )]
    public bool? TakeSnapshots { get; set; }

    [ArgDescription( "Trace level output logging. Change log level in Sanoid.nlog.json for normal usage. Has no effect until configuration is parsed." )]
    [ArgShortcut( "--trace" )]
    [ArgCantBeCombinedWith( "Verbose|Debug|Quiet|ReallyQuiet" )]
    public bool? Trace { get; set; }

    [ArgDescription( "Verbose (Info level) output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "v" )]
    [ArgShortcut( "--verbose" )]
    [ArgEnforceCase]
    [ArgCantBeCombinedWith( "Debug|Quiet|ReallyQuiet" )]
    public bool? Verbose { get; set; }

    [ArgDescription( "Outputs Sanoid.net version to configured logging targets and exits, making no changes." )]
    [ArgShortcut( "V" )]
    [ArgShortcut( "--version" )]
    [ArgEnforceCase]
    public bool Version { get; set; }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Called by main thread to set logging settings early, before we load the rest of the configuration.
    /// </summary>
    [UsedImplicitly]
    public void Main( )
    {
        if ( Version )
        {
            Version version = Assembly.GetExecutingAssembly( ).GetName( ).Version!;
            LogManager.GetLogger( "MessageOnly" ).Info( "Sanoid.net version {version}", version );
            return;
        }

        if ( ReallyQuiet ?? false )
        {
            LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Off, LogLevel.Off ) );
        }

        if ( Quiet ?? false )
        {
            LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Error, LogLevel.Fatal ) );
        }

        if ( Verbose ?? false )
        {
            LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Info, LogLevel.Fatal ) );
        }

        if ( Debug ?? false )
        {
            LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Debug, LogLevel.Fatal ) );
        }

        if ( Trace ?? false )
        {
            LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Trace, LogLevel.Fatal ) );
        }

        if ( ( ReallyQuiet ?? false ) || ( Quiet ?? false ) || ( Verbose ?? false ) || ( Debug ?? false ) || ( Trace ?? false ) )
        {
            LogManager.ReconfigExistingLoggers( );
        }
    }
}
