// /*
// *  LICENSE:
// *
// *  This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// *  from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// *  project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.
// */

using System.Reflection;
using JetBrains.Annotations;
using NLog;
using PowerArgs;
using Sanoid.Common.Configuration;

namespace Sanoid;

/// <summary>
///     The command line arguments that sanoid can accept
/// </summary>
[ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
[UsedImplicitly]
internal class CommandLineArguments
{
    [ArgDescription( "Cache directory for sanoid" )]
    [ArgShortcut( "--cache-dir" )]
    public string? CacheDir { get; set; }

    [ArgDescription( "Base configuration directory for sanoid" )]
    [ArgShortcut( "--configdir" )]
    [ArgShortcut( "--config-dir" )]
    public string? ConfigDir { get; set; }

    [ArgDescription( "Create snapshots and prune expired snapshots. Equivalent to --take-snapshots --prune-snapshots" )]
    [ArgShortcut( "--cron" )]
    public bool Cron { get; set; }

    [ArgDescription( "Debug level output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--debug" )]
    [ArgCantBeCombinedWith( "Verbose|Quiet|ReallyQuiet" )]
    public bool Debug { get; set; }

    [ArgDescription( "Prunes expired snapshots, even if their parent datasets are currently involved in a send or receive operation. Implies --prune-snapshots as well." )]
    [ArgShortcut( "--force-prune" )]
    [ArgShortcut( "--force-prune-snapshots" )]
    [ArgCantBeCombinedWith( "ReadOnly" )]
    public bool ForcePrune { get; set; }

    [ArgDescription( "This clears out sanoid's zfs snapshot listing cache. This is normally not needed." )]
    [ArgShortcut( "--force-update" )]
    public bool ForceUpdate { get; set; }

    [ArgDescription( "Shows this help" )]
    [ArgShortcut( "-h" )]
    [ArgShortcut( "--help" )]
    [HelpHook]
    public bool Help { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the capacity of the zpool your filesystems are on. It only monitors pools that are configured in the sanoid.conf file." )]
    [ArgShortcut( "--monitor-capacity" )]
    [ArgShortcut( "--monitor-capacity-nagios" )]
    public bool MonitorCapacity { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the health of the zpool your filesystems are on. It only monitors filesystems that are configured in the sanoid.conf file." )]
    [ArgShortcut( "--monitor-health" )]
    [ArgShortcut( "--monitor-health-nagios" )]
    public bool MonitorHealth { get; set; }

    [ArgDescription( "This option is designed to be run by a Nagios monitoring system. It reports on the health of your snapshots." )]
    [ArgShortcut( "--monitor-snapshots" )]
    [ArgShortcut( "--monitor-snapshots-nagios" )]
    public bool MonitorSnapshots { get; set; }

    [ArgDescription( "Prunes expired snapshots, except for snapshots of datasets currently involved in a send or receive operation." )]
    [ArgShortcut( "--prune-snapshots" )]
    public bool PruneSnapshots { get; set; }

    [ArgDescription( "Suppress non-error output. WILL WARN BEFORE SETTING IS APPLIED. Configure in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose" )]
    public bool Quiet { get; set; }

    [ArgDescription( "Skip creation/deletion of snapshots (Simulate)." )]
    [ArgShortcut( "--readonly" )]
    [ArgShortcut( "--read-only" )]
    public bool ReadOnly { get; set; }

    [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json to set for normal usage. Will not warn when used." )]
    [ArgShortcut( "--really-quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose" )]
    public bool ReallyQuiet { get; set; }

    [ArgDescription( "Runtime directory for sanoid" )]
    [ArgShortcut( "--run-dir" )]
    public string? RunDir { get; set; }

    [ArgDescription( "Will make sanoid take snapshots, but will not prune unless --prune-snapshots is also specified." )]
    [ArgShortcut( "--take-snapshots" )]
    public bool TakeSnapshots { get; set; }

    [ArgDescription( "Trace level output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--trace" )]
    [ArgCantBeCombinedWith( "Verbose|Debug|Quiet|ReallyQuiet" )]
    public bool Trace { get; set; }

    [ArgDescription( "Forces loading of PERL sanoid's configuration files" )]
    [ArgShortcut( "--use-sanoid-config" )]
    public bool UseSanoidConfig { get; set; }

    [ArgDescription( "Verbose (Info level) output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "v" )]
    [ArgShortcut( "--verbose" )]
    [ArgEnforceCase]
    [ArgCantBeCombinedWith( "Debug|Quiet|ReallyQuiet" )]
    public bool Verbose { get; set; }

    [ArgDescription( "Outputs Sanoid.net version to configured logging targets and exits, making no changes." )]
    [ArgShortcut( "V" )]
    [ArgShortcut( "--version" )]
    [ArgEnforceCase]
    public bool Version { get; set; }

    /// <summary>
    ///     Called by main thread to override configured settings with any arguments passed at the command line.
    /// </summary>
    public void Main()
    {
        if ( Version )
        {
            LogManager.GetLogger( "MessageOnly" ).Info( "Sanoid.net version {0}", Assembly.GetExecutingAssembly().GetName().Version! );
            return;
        }

        if ( Quiet )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Off;
        }

        if ( ReallyQuiet )
        {
            LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Off, LogLevel.Off ) );
        }

        if ( Trace )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Trace;
        }

        if ( Verbose )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Info;
        }

        if ( CacheDir is not null )
        {
            Configuration.SanoidConfigurationCacheDirectory = CacheDir;
        }

        if ( ConfigDir is not null )
        {
            Configuration.SanoidConfigurationPathBase = ConfigDir;
        }

        if ( Cron )
        {
            PruneSnapshots = true;
            TakeSnapshots = true;
            //TODO: Implement cron
        }

        if ( Debug )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Debug;
        }

        if ( ForcePrune )
        {
            PruneSnapshots = true;
            //TODO: Implement ForcePrune
        }

        if ( ForceUpdate )
        {
            //TODO: Implement ForceUpdate
        }

        if ( MonitorCapacity )
        {
            //TODO: Implement MonitorCapacity
        }

        if ( MonitorHealth )
        {
            //TODO: Implement MonitorHealth
        }

        if ( MonitorSnapshots )
        {
            //TODO: Implement MonitorSnapshots
        }

        if ( PruneSnapshots )
        {
            //TODO: Implement PruneSnapshots
        }

        if ( ReadOnly )
        {
            //TODO: Implement ReadOnly
        }

        if ( RunDir is not null )
        {
            Configuration.SanoidConfigurationRunDirectory = RunDir;
        }

        if ( TakeSnapshots )
        {
            //TODO: Implement TakeSnapshots
        }

        if ( UseSanoidConfig )
        {
            Configuration.UseSanoidConfiguration = true;
        }
    }
}
