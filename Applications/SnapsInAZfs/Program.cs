#region MIT LICENSE

// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using LogLevel = NLog.LogLevel;

namespace SnapsInAZfs;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ConfigConsole;
using Interop.Libc.Enums;
using Interop.Zfs.ZfsCommandRunner;
using Interop.Zfs.ZfsTypes;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Monitoring;
using NLog.Config;
using NLog.Extensions.Logging;
using PowerArgs;
using Settings.Logging;

[UsedImplicitly]
internal partial class Program
{
    // Note that logging will be at whatever level is defined in SnapsInAZfs.nlog.json until configuration is initialized, regardless of command-line parameters.
    // Desired logging parameters should be set in SnapsInAZfs.nlog.json
    private static readonly  Logger               Logger = LogManager.GetCurrentClassLogger ( );
    private static           IConfigurationRoot?  _configurationRoot;
    internal static readonly IMonitor             ServiceObserver = new Monitor ( );
    internal static          SnapsInAZfsSettings? Settings;

    internal static IZfsCommandRunner? ZfsCommandRunnerSingleton;

    [ExcludeFromCodeCoverage ( Justification = "Largely un-testable" )]
    public static async Task<int> Main ( string [] argv )
    {
        CommandLineArguments? args = await Args.ParseAsync<CommandLineArguments> ( argv ).ConfigureAwait ( true );

        // The nullability context in PowerArgs is wrong, so this absolutely can be null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ( args is null || args.Help )
        {
            LogManager.Shutdown ( );

            return (int)Errno.ECANCELED;
        }

        if ( !LoadConfigurationFromConfigurationFiles ( ref Settings, out _configurationRoot, in args ) )
        {
            LogManager.Shutdown ( );

            return (int)Errno.EFTYPE;
        }

        SetCommandLineLoggingOverride ( args );

        if ( args.Version )
        {
            // ReSharper disable once ExceptionNotDocumented
            // ReSharper disable once HeapView.ObjectAllocation
            string versionString = $"SnapsInAZfs Version: {Assembly.GetEntryAssembly ( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute> ( )?.InformationalVersion}";
            Console.WriteLine ( versionString );
            Logger.Debug ( versionString );
            Logger.Trace ( "Version argument provided. Exiting." );
            LogManager.Shutdown ( );

            return (int)Errno.ECANCELED;
        }

        ApplyCommandLineArgumentOverrides ( in args, Settings );

        // TODO: Validate critical settings before continuing.
        // Only need to validate before running config console.
        // But need to terminate if not running config console.

        if ( args.ConfigConsole )
        {
            try
            {
                if ( TryGetZfsCommandRunner ( Settings, out IZfsCommandRunner? zfsCommandRunner ) )
                {
                    ConfigConsole.ConfigConsole.RunConsoleInterface ( zfsCommandRunner );
                }
            }
            catch ( Exception e )
            {
                Logger.Fatal ( e, "Error in configuration console - Exiting" );
                LogManager.Shutdown ( );

                return (int)Errno.GenericError;
            }

            LogManager.Shutdown ( );

            return 0;
        }

        return Settings.Monitoring.EnableHttp switch
               {
                   true => await RunWithKestrelAsync ( Settings, _configurationRoot ).ConfigureAwait ( true ),
                   _    => await RunWithoutKestrelAsync ( Settings ).ConfigureAwait ( true )
               };
    }

    /// <summary>
    ///     Overrides configuration values specified in configuration files or environment variables with arguments supplied on
    ///     the CLI
    /// </summary>
    /// <param name="args"></param>
    /// <param name="programSettings">
    ///     A reference to an instance of a <see cref="SnapsInAZfsSettings"/> object to modify
    /// </param>
    internal static void ApplyCommandLineArgumentOverrides ( in CommandLineArguments args, SnapsInAZfsSettings programSettings )
    {
        Logger.Debug ( "Overriding settings using arguments from command line." );

        programSettings.DryRun                |= args.DryRun;
        programSettings.TakeSnapshots         =  ( programSettings.TakeSnapshots         || args.TakeSnapshots  || args.Cron )                    && !args.NoTakeSnapshots;
        programSettings.PruneSnapshots        =  ( programSettings.PruneSnapshots        || args.PruneSnapshots || args.ForcePrune || args.Cron ) && !args.NoPruneSnapshots;
        programSettings.Daemonize             =  ( programSettings.Daemonize             || args.Daemonize )                                      && args is { NoDaemonize: false, ConfigConsole: false };
        programSettings.Monitoring.EnableHttp =  ( programSettings.Monitoring.EnableHttp || args.Monitor )                                        && args is { NoMonitor  : false, ConfigConsole: false };

        if ( args.DaemonTimerInterval > 0 )
        {
            programSettings.DaemonTimerIntervalSeconds = Math.Clamp ( args.DaemonTimerInterval, 1u, 60u );
        }
    }

    internal static bool LoadConfigurationFromConfigurationFiles ( [NotNullWhen ( true )] ref SnapsInAZfsSettings? settings, [NotNullWhen ( true )] out IConfigurationRoot? rootConfiguration, in CommandLineArguments args )
    {
        // Configuration is built in the following order from various sources.
        // Configurations from all sources are merged, and the final configuration that will be used is the result of the merged configurations.
        // If conflicting items exist in multiple configuration sources, the configuration of the configuration source added latest will
        // override earlier values.
        // Note that nlog-specific configuration is separate, in SnapsInAZfs.nlog.json, and is not affected by the configuration specified below,
        // and is loaded/parsed FIRST, before any configuration specified below.
        // See the SnapsInAZfs.Settings.Logging.LoggingSettings class for nlog configuration details.
        // See snapsinazfs(5) for detailed configuration documentation.
        // Configuration order, if not overridden by command-line options:
        // 1. /usr/local/share/SnapsInAZfs/SnapsInAZfs.json   #(Required - Base configuration - Should not be modified by the user)
        // 2. /etc/SnapsInAZfs/SnapsInAZfs.local.json
        // 6. Command-line arguments passed on invocation of SnapsInAZfs
        Logger.Debug ( "Getting base configuration from files" );
        ConfigurationBuilder configBuilder = new ( );

        IEnumerable<string> requestedFiles = args.ConfigFiles.Length > 0 ? args.ConfigFiles : [ "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json", "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json", "/etc/SnapsInAZfs/SnapsInAZfs.local.json", "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", "SnapsInAZfs.json", "SnapsInAZfs.local.json", "SnapsInAZfs.nlog.json" ];

        foreach ( string filePath in requestedFiles )
        {
            if ( !File.Exists ( filePath ) )
            {
                Logger.Debug ( "Configuration file not found at {0}", filePath );

                continue;
            }

            Logger.Trace ( "Loading configuration file {0}", filePath );
            configBuilder.AddJsonFile ( filePath, false, false );
        }

        if ( configBuilder.Sources.Count == 0 )
        {
            Logger.Fatal ( "Configuration files not found at any of these locations: {0}", requestedFiles.ToCommaSeparatedSingleLineString ( true ) );
            rootConfiguration = null;

            return false;
        }

        rootConfiguration = configBuilder.Build ( );

        Logger.Trace ( "Building settings objects from IConfiguration" );

        try
        {
            settings = rootConfiguration.Get<SnapsInAZfsSettings> ( ) ?? throw new InvalidOperationException ( );
            IConfigurationSection kestrelSection = rootConfiguration.GetRequiredSection ( "Monitoring" ).GetSection ( "Kestrel" );

            if ( kestrelSection.Exists ( ) )
            {
                IEnumerable<IConfigurationSection> kestrelSettings = kestrelSection.GetChildren ( );
                settings.Monitoring.Kestrel = kestrelSettings.ToDictionary ( static k => k.Key, static v => v.SerializeToJson ( ) );
            }

            IConfigurationSection nlogConfigSection = rootConfiguration.GetSection ( "NLog" );
            LogManager.Configuration = nlogConfigSection.Exists ( ) ? new NLogLoggingConfiguration ( nlogConfigSection ) : new LoggingConfiguration ( );
        }
        catch ( Exception ex )
        {
            Logger.Fatal ( ex, "Unable to parse settings from JSON" );

            return false;
        }

        return true;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal static void SetCommandLineLoggingOverride ( CommandLineArguments args )
    {
        if ( args.Debug )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Debug! );
        }

        if ( args.Quiet )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Warn! );
        }

        if ( args.ReallyQuiet )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Off! );
        }

        if ( args.Trace )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Trace! );
        }

        if ( args.Verbose )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Info! );
        }
    }

    internal static bool TryGetZfsCommandRunner ( SnapsInAZfsSettings settings, [NotNullWhen ( true )] out IZfsCommandRunner? zfsCommandRunner, bool reuseSingleton = true )
    {
        if ( reuseSingleton && ZfsCommandRunnerSingleton is { } singleton )
        {
            zfsCommandRunner = singleton;

            return true;
        }

        Logger.Trace ( "Getting ZFS command runner for the current environment" );

        try
        {
            GetZfsCommandRunner ( settings, out zfsCommandRunner );
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Fatal ( ex, "Null or empty string provided for ZfsPath or ZpoolPath - Cannot continue" );
            zfsCommandRunner = null;

            return false;
        }
        catch ( FileNotFoundException ex )
        {
            Logger.Fatal ( ex, ex.Message );
            zfsCommandRunner = null;

            return false;
        }

        if ( reuseSingleton )
        {
            ZfsCommandRunnerSingleton = zfsCommandRunner;
        }

        return true;
    }

    private static SiazService? GetSiazServiceInstance ( SnapsInAZfsSettings settings )
    {
        if ( !TryGetZfsCommandRunner ( settings, out IZfsCommandRunner? zfsCommandRunner ) )
        {
            return null;
        }

        if ( settings.Monitoring.EnableHttp )
        {
            return new ( settings, zfsCommandRunner, ServiceObserver, ServiceObserver );
        }

        return new ( settings, zfsCommandRunner );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static void GetZfsCommandRunner ( SnapsInAZfsSettings settings, out IZfsCommandRunner zfsCommandRunner )
    {
        // This conditional is to avoid compiling the DummyZfsCommandRunner class if it isn't needed
    #if INCLUDE_DUMMY_ZFSCOMMANDRUNNER || WINDOWS
            zfsCommandRunner = new DummyZfsCommandRunner( settings.ZfsPath, settings.ZpoolPath );
    #else
        zfsCommandRunner = new ZfsCommandRunner ( settings.ZfsPath, settings.ZpoolPath );
    #endif
    }

    private static async Task<int> RunWithKestrelAsync ( SnapsInAZfsSettings settings, IConfigurationRoot configurationRoot )
    {
        SiazService.Timestamp = DateTimeOffset.Now;
        using SiazService? serviceInstance = GetSiazServiceInstance ( settings );

        if ( serviceInstance is null )
        {
            Logger.Fatal ( "Failed to create service instance - exiting" );
            LogManager.Shutdown ( );

            return (int)Errno.ENOATTR;
        }

        WebApplicationBuilder serviceBuilder = WebApplication.CreateBuilder ( );

        // Disposal happens after service shutdown, so this inspection can be ignored here
        // ReSharper disable once AccessToDisposedClosure
        serviceBuilder.Host
                      .UseSystemd ( )
                      .ConfigureServices ( ( _, services ) => { services.AddHostedService ( _ => serviceInstance ); } );

        serviceBuilder.WebHost
                      .UseConfiguration (
                                         configurationRoot
                                            .GetRequiredSection ( "Monitoring" )
                                            .GetSection ( "Kestrel" ) )
                      .UseKestrel ( ( _, kestrelOptions ) =>
                                    {
                                        kestrelOptions.Configure (
                                                                  configurationRoot
                                                                     .GetRequiredSection ( "Monitoring" )
                                                                     .GetSection ( "Kestrel" ) )
                                                      .Load ( );
                                    } );
        WebApplication svc = serviceBuilder.Build ( );

        // ReSharper disable HeapView.DelegateAllocation
        RouteGroupBuilder statusGroup = svc.MapGroup ( "/" );
        statusGroup.MapGet ( "/",                 ServiceObserver.GetApplicationStateAsync );
        statusGroup.MapGet ( "/state",            ServiceObserver.GetApplicationStateAsync );
        statusGroup.MapGet ( "/fullstate",        ServiceObserver.GetFullApplicationStateAsync );
        statusGroup.MapGet ( "/workingset",       ServiceObserver.GetWorkingSetAsync );
        statusGroup.MapGet ( "/version",          ServiceObserver.GetVersionAsync );
        statusGroup.MapGet ( "/servicestarttime", ServiceObserver.GetServiceStartTimeAsync );
        statusGroup.MapGet ( "/nextruntime",      ServiceObserver.GetNextRunTimeAsync );

        RouteGroupBuilder snapshotsGroup = svc.MapGroup ( "/snapshots" );
        snapshotsGroup.MapGet ( "/", ServiceObserver.GetAllSnapshotCountsAsync );

        snapshotsGroup.MapGet ( "/allcounts",                      ServiceObserver.GetAllSnapshotCountsAsync );
        snapshotsGroup.MapGet ( "/lastsnapshotprunedtime",         ServiceObserver.GetLastSnapshotPrunedTimeAsync );
        snapshotsGroup.MapGet ( "/lastsnapshottakentime",          ServiceObserver.GetLastSnapshotTakenTimeAsync );
        snapshotsGroup.MapGet ( "/prunedfailedlastruncount",       ServiceObserver.GetSnapshotsPrunedFailedLastRunCountAsync );
        snapshotsGroup.MapGet ( "/prunedfailedlastrunnames",       ServiceObserver.GetSnapshotsPrunedFailedLastRunNamesAsync );
        snapshotsGroup.MapGet ( "/prunedfailedsincestartcount",    ServiceObserver.GetSnapshotsPrunedFailedSinceStartCountAsync );
        snapshotsGroup.MapGet ( "/prunedsucceededlastruncount",    ServiceObserver.GetSnapshotsPrunedSucceededLastRunCountAsync );
        snapshotsGroup.MapGet ( "/prunedsucceededsincestartcount", ServiceObserver.GetSnapshotsPrunedSucceededSinceStartCountAsync );
        snapshotsGroup.MapGet ( "/takenfailedlastruncount",        ServiceObserver.GetSnapshotsTakenFailedLastRunCountAsync );
        snapshotsGroup.MapGet ( "/takenfailedlastrunnames",        ServiceObserver.GetSnapshotsTakenFailedLastRunNamesAsync );
        snapshotsGroup.MapGet ( "/takenfailedsincestartcount",     ServiceObserver.GetSnapshotsTakenFailedSinceStartCountAsync );
        snapshotsGroup.MapGet ( "/takensucceededlastruncount",     ServiceObserver.GetSnapshotsTakenSucceededLastRunCountAsync );
        snapshotsGroup.MapGet ( "/takensucceededsincestartcount",  ServiceObserver.GetSnapshotsTakenSucceededSinceStartCountAsync );

        // ReSharper restore HeapView.DelegateAllocation
        using CancellationTokenSource tokenSource = new ( );
        CancellationToken             masterToken = tokenSource.Token;
        await svc.StartAsync ( masterToken ).ConfigureAwait ( true );
        await svc.WaitForShutdownAsync ( masterToken ).ConfigureAwait ( true );

        return SiazService.ExitStatus;
    }

    private static async Task<int> RunWithoutKestrelAsync ( SnapsInAZfsSettings settings )
    {
        SiazService.Timestamp = DateTimeOffset.Now;
        using SiazService? serviceInstance = GetSiazServiceInstance ( settings );

        if ( serviceInstance is null )
        {
            Logger.Fatal ( "Failed to create service instance - exiting" );
            LogManager.Shutdown ( );

            return (int)Errno.ENOATTR;
        }

        // Disposal happens after service shutdown, so this inspection can be ignored here
        // ReSharper disable once AccessToDisposedClosure
        IHost serviceHost = Host.CreateDefaultBuilder ( )
                                .UseSystemd ( )
                                .ConfigureServices ( ( _, services ) => { services.AddHostedService ( _ => serviceInstance ); } )
                                .Build ( );
        using CancellationTokenSource tokenSource = new ( );
        CancellationToken             masterToken = tokenSource.Token;
        await serviceHost.StartAsync ( masterToken ).ConfigureAwait ( true );
        await serviceHost.WaitForShutdownAsync ( masterToken ).ConfigureAwait ( true );

        return SiazService.ExitStatus;
    }

    private static int ValidateOrSetCriticalSettings ( SnapsInAZfsSettings settings )
    {
        ArgumentNullException.ThrowIfNull ( settings );

        // Flags with values >= 0 being OK and negative values indicating an error.
        int       statusFlags                       = 0;
        const int errorMask                         = int.MinValue; //leftmost bit set
        const int localSystemNameInvalidMask        = 1 | errorMask;
        const int zfsPathInvalidMask                = 2 | errorMask;
        const int zpoolPathInvalidMask              = 4 | errorMask;
        const int localSystemNameAutoConfiguredMask = 8;

        if ( string.IsNullOrWhiteSpace ( settings.LocalSystemName ) )
        {
            Logger.Fatal ( "Missing, empty, or all-whitespace value for LocalSystemName in JSON configuration. This setting is required and must be valid. SIAZ will terminate. See documentation for requirements." );
            statusFlags |= localSystemNameInvalidMask;
        }
        else if ( settings.LocalSystemName == "auto" )
        {
            string localSystemFqdn = Utility.GetFullyQualifiedDomainName ( );
            statusFlags              |= localSystemNameAutoConfiguredMask;
            settings.LocalSystemName =  localSystemFqdn;
            Logger.Info ( $"Using auto-detected FQDN value `{localSystemFqdn}` for  {nameof (settings.LocalSystemName)} during this instance of SnapsInAZfs." );
        }
        else if ( settings.LocalSystemName.Length > 255 )
        {
            Logger.Fatal ( "LocalSystemName is longer than the maximum length of 255 characters. This setting is required and must be valid. SIAZ will terminate. See documentation for requirements." );
            statusFlags |= localSystemNameInvalidMask;
        }

        if ( !LocalSystemNameRegex ( ).IsMatch ( settings.LocalSystemName ) )
        {
            statusFlags |= localSystemNameInvalidMask;
            Logger.Fatal ( "Invalid value for LocalSystemName. This setting is required and must be valid. SIAZ will terminate. See documentation for requirements." );
        }

        // TODO: Finish validating.
        // Need to check ZfsPath and ZpoolPath
        // Should probably also either ditch the status flags (most likely - kinda redundant if it's already logged) or define them more formally as a type or something

        return statusFlags;
    }

    /// <summary>
    /// Source-generated regex for validating the <see cref="SnapsInAZfsSettings.LocalSystemName"/> property value.
    /// </summary>
    [GeneratedRegex ( @"^(?:[a-zA-Z0-9_-]+\.)*[a-zA-Z0-9_-]+\.?$", RegexOptions.Singleline| RegexOptions.CultureInvariant )]
    private static partial Regex LocalSystemNameRegex ( );
}
