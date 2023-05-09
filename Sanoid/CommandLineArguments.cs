// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Reflection;
using NLog.Config;
using PowerArgs;
using Sanoid.Common.Configuration.Monitoring;
using BaseConfiguration = Sanoid.Common.Configuration.Configuration;
using MonitoringConfiguration = Sanoid.Common.Configuration.Monitoring.Configuration;

namespace Sanoid;

/// <summary>
///     The command line arguments that sanoid can accept
/// </summary>
[ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
[UsedImplicitly]
internal class CommandLineArguments
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
    [ArgCantBeCombinedWith( "Debug|Verbose|ReadOnly" )]
    public bool Quiet { get; set; }

    [ArgDescription( "Skip creation/deletion of snapshots (Simulate)." )]
    [ArgShortcut( "--readonly" )]
    [ArgShortcut( "--read-only" )]
    [ArgShortcut( "--dryrun" )]
    [ArgShortcut( "--dry-run" )]
    public bool ReadOnly { get; set; }

    [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json to set for normal usage. Will not warn when used." )]
    [ArgShortcut( "--really-quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose|ReadOnly" )]
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
    public void Main( )
    {
        if ( Version )
        {
            LogManager.GetLogger( "MessageOnly" ).Info( "Sanoid.net version {0}", Assembly.GetExecutingAssembly( ).GetName( ).Version! );
            return;
        }

        if ( ReallyQuiet )
        {
            LogManager.Configuration.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Off, LogLevel.Off ) );
        }

        if ( Quiet )
        {
            LogManager.Configuration.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Error, LogLevel.Fatal ) );
        }

        if ( Verbose )
        {
            LogManager.Configuration.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Info, LogLevel.Fatal ) );
        }

        if ( Debug )
        {
            LogManager.Configuration.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Debug, LogLevel.Fatal ) );
        }

        if ( Trace )
        {
            LogManager.Configuration.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Trace, LogLevel.Fatal ) );
        }

        if ( ReallyQuiet || Quiet || Verbose || Debug || Trace )
        {
            LogManager.ReconfigExistingLoggers( );
        }

        //Call this so that settings are first retrieved from configuration, to allow arguments to override them.
        BaseConfiguration.Initialize( );

        if ( Quiet )
        {
            BaseConfiguration.DefaultLoggingLevel = LogLevel.Off;
        }

        if ( ReallyQuiet )
        {
            BaseConfiguration.DefaultLoggingLevel = LogLevel.Off;
        }

        if ( Trace )
        {
            BaseConfiguration.DefaultLoggingLevel = LogLevel.Trace;
        }

        if ( Verbose )
        {
            BaseConfiguration.DefaultLoggingLevel = LogLevel.Info;
        }

        if ( CacheDir is not null )
        {
            BaseConfiguration.SanoidConfigurationCacheDirectory = CacheDir;
        }

        if ( ConfigDir is not null )
        {
            BaseConfiguration.SanoidConfigurationPathBase = ConfigDir;
        }

        if ( Cron )
        {
            PruneSnapshots = true;
            TakeSnapshots = true;
            //TODO: Implement cron
        }

        if ( Debug )
        {
            BaseConfiguration.DefaultLoggingLevel = LogLevel.Debug;
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
            if ( MonitoringConfiguration.MonitorConfigurations.TryGetValue( "Nagios", out MonitoringConfigurationBase? nagiosConfig ) )
            {
                nagiosConfig.MonitorCapacity = true;
            }
        }

        if ( MonitorHealth )
        {
            if ( MonitoringConfiguration.MonitorConfigurations.TryGetValue( "Nagios", out MonitoringConfigurationBase? nagiosConfig ) )
            {
                nagiosConfig.MonitorHealth = true;
            }
        }

        if ( MonitorSnapshots )
        {
            if ( MonitoringConfiguration.MonitorConfigurations.TryGetValue( "Nagios", out MonitoringConfigurationBase? nagiosConfig ) )
            {
                nagiosConfig.MonitorSnapshots = true;
            }
        }

        if ( PruneSnapshots )
        {
            BaseConfiguration.PruneSnapshots = true;
        }

        if ( ReadOnly )
        {
            _logger.Info( "Performing a dry run. No changes will be made to ZFS." );
            BaseConfiguration.DryRun = true;
        }

        if ( RunDir is not null )
        {
            BaseConfiguration.SanoidConfigurationRunDirectory = RunDir;
        }

        if ( TakeSnapshots )
        {
            BaseConfiguration.TakeSnapshots = true;
        }

        if ( UseSanoidConfig )
        {
            BaseConfiguration.UseSanoidConfiguration = true;
        }
    }
}