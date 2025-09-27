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

namespace SnapsInAZfs;

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CommandLine;
using ConfigConsole;
using Interop;
using Interop.Zfs.ZfsCommandRunner;
using Interop.Zfs.ZfsTypes;
using JetBrains.Annotations;
using LogLevel = NLog.LogLevel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Monitoring;
using NLog.Config;
using NLog.Extensions.Logging;
using PowerArgs;
using Settings.Logging;
using SCL = System.CommandLine;

[UsedImplicitly]
internal static class Program
{
    // Note that logging will be at whatever level is defined in SnapsInAZfs.nlog.json until configuration is initialized, regardless of command-line parameters.
    // Desired logging parameters should be set in SnapsInAZfs.nlog.json
    private static readonly Logger               Logger          = LogManager.GetLogger ( "SnapsInAZfs" );
    private static readonly IMonitor             ServiceObserver = new Monitor( );
    private static          IConfigurationRoot?  _configurationRoot;
    internal static         SnapsInAZfsSettings? Settings;
    internal static IZfsCommandRunner? ZfsCommandRunnerSingleton;

    [ExcludeFromCodeCoverage ( Justification = "Largely un-testable" )]
    public static async Task<int> Main( string[] argv )
    {
        if ( !ProcessCommandLine ( argv, out SCL.ParseResult siazCliParseResult, out Settings, out _configurationRoot,out ExitCode siazCliInvocationExitCode )
          || siazCliInvocationExitCode is not ExitCode.EOK )
        {
            LogManager.Shutdown( );

            return (int)siazCliInvocationExitCode;
        }

        switch ( siazCliParseResult )
        {
            case { RootCommandResult.IdentifierToken.Value: SiazCommandLine.RunCommandName }:
                return Settings.Monitoring.EnableHttp switch
                       {
                           true => await RunWithKestrelAsync ( Settings, _configurationRoot ).ConfigureAwait ( true ),
                           _    => await RunWithoutKestrelAsync ( Settings ).ConfigureAwait ( true )
                       };
            default:
                break;
        }

        return 0;

        CommandLineArguments args           = await Args.ParseAsync<CommandLineArguments> ( argv ).ConfigureAwait ( true );

        // Implicit null check here is important.
        if ( args is not { Help: false } )
        {
            LogManager.Shutdown( );

            return (int)ExitCode.ECANCELED;
        }

        if ( !LoadConfigurationFromConfigurationFiles ( ref Settings, out _configurationRoot, in args ) )
        {
            LogManager.Shutdown( );

            return (int)ExitCode.EFTYPE;
        }

        SetCommandLineLoggingOverride ( args );

        if ( args.Version )
        {
            // ReSharper disable once ExceptionNotDocumented
            // ReSharper disable once HeapView.ObjectAllocation
            string versionString = $"SnapsInAZfs Version: {Assembly.GetEntryAssembly( )?.GetCustomAttribute<AssemblyInformationalVersionAttribute>( )?.InformationalVersion}";
            Console.WriteLine ( versionString );
            Logger.Debug ( versionString );
            Logger.Trace ( "Version argument provided. Exiting." );
            LogManager.Shutdown( );

            return (int)ExitCode.ECANCELED;
        }

        ApplyCommandLineArgumentOverrides ( in args, Settings );

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
                LogManager.Shutdown( );

                return (int)ExitCode.GenericError;
            }

            LogManager.Shutdown( );

            return 0;
        }

        if ( ValidateSettings ( in Settings ) is not ExitCode.EOK and var badResult )
        {
            return (int)badResult;
        }

        Logger.Debug ( "Settings passed basic validation checks." );
        Logger.Trace ( $"Final settings object: {JsonSerializer.Serialize ( Settings )}" );

        return Settings.Monitoring.EnableHttp switch
               {
                   true => await RunWithKestrelAsync ( Settings, _configurationRoot ).ConfigureAwait ( true ),
                   _    => await RunWithoutKestrelAsync ( Settings ).ConfigureAwait ( true )
               };
    }

    private static bool ProcessCommandLine(
        string[] arguments,
        out SCL.ParseResult siazCliParseResult,
        [NotNullWhen ( true )] out SnapsInAZfsSettings? settings,
        [NotNullWhen ( true )] out IConfigurationRoot? configurationRoot,
        out ExitCode exitCode
        )
    {
        SiazCommandLine siazCli = new ( );
        siazCliParseResult = siazCli.Parse ( arguments, out RootCommand _, new ( ) { EnablePosixBundling = true } );
        exitCode = siazCli.Invoke (
                                 arguments,
                                 out RootCommand _,
                                 out siazCliParseResult,
                                 out settings,
                                 out configurationRoot,
                                 parserConfiguration: new ( ) { EnablePosixBundling = true }
                                );

        return exitCode == ExitCode.EOK;
    }

    /// <summary>
    ///     Overrides configuration values specified in configuration files or environment variables with arguments supplied on
    ///     the CLI
    /// </summary>
    /// <param name="args"></param>
    /// <param name="programSettings">
    ///     A reference to an instance of a <see cref="SnapsInAZfsSettings" /> object to modify
    /// </param>
    internal static void ApplyCommandLineArgumentOverrides( in CommandLineArguments args, SnapsInAZfsSettings programSettings )
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

    internal static bool LoadConfigurationFromConfigurationFiles( [NotNullWhen ( true )] ref SnapsInAZfsSettings? settings, [NotNullWhen ( true )] out IConfigurationRoot? rootConfiguration, in CommandLineArguments args )
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

        rootConfiguration = configBuilder.Build( );

        Logger.Trace ( "Building settings objects from IConfiguration" );

        try
        {
            settings = rootConfiguration.Get<SnapsInAZfsSettings>( ) ?? throw new InvalidOperationException( );
            IConfigurationSection kestrelSection = rootConfiguration.GetRequiredSection ( "Monitoring" ).GetSection ( "Kestrel" );

            if ( kestrelSection.Exists( ) )
            {
                IEnumerable<IConfigurationSection> kestrelSettings = kestrelSection.GetChildren( );
                settings.Monitoring.Kestrel = kestrelSettings.ToDictionary ( static k => k.Key, static v => v.SerializeToJson( ) );
            }

            IConfigurationSection nlogConfigSection = rootConfiguration.GetSection ( "NLog" );
            LogManager.Configuration = nlogConfigSection.Exists( ) ? new NLogLoggingConfiguration ( nlogConfigSection ) : new LoggingConfiguration( );
        }
        catch ( Exception ex )
        {
            Logger.Fatal ( ex, "Unable to parse settings from JSON" );

            return false;
        }

        return true;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal static void SetCommandLineLoggingOverride( CommandLineArguments args )
    {
        if ( args.Debug )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Debug );
        }

        if ( args.Quiet )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Warn );
        }

        if ( args.ReallyQuiet )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Off );
        }

        if ( args.Trace )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Trace );
        }

        if ( args.Verbose )
        {
            LoggingSettings.OverrideConsoleLoggingLevel ( LogLevel.Info );
        }
    }

    internal static bool TryGetZfsCommandRunner( SnapsInAZfsSettings settings, [NotNullWhen ( true )] out IZfsCommandRunner? zfsCommandRunner, bool reuseSingleton = true )
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

    private static SiazService? GetSiazServiceInstance( SnapsInAZfsSettings settings )
    {
        if ( !TryGetZfsCommandRunner ( settings, out IZfsCommandRunner? zfsCommandRunner ) )
        {
            return null;
        }

        if ( settings.Monitoring.EnableHttp )
        {
            return new SiazService ( settings, zfsCommandRunner, ServiceObserver, ServiceObserver );
        }

        return new SiazService ( settings, zfsCommandRunner );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static void GetZfsCommandRunner( SnapsInAZfsSettings settings, out IZfsCommandRunner zfsCommandRunner )
    {
        // This conditional is to avoid compiling the DummyZfsCommandRunner class if it isn't needed.
    #if INCLUDE_DUMMY_ZFSCOMMANDRUNNER || WINDOWS
            zfsCommandRunner = new DummyZfsCommandRunner ( settings.ZfsPath, settings.ZpoolPath );
    #else
        zfsCommandRunner = new ZfsCommandRunner ( settings.ZfsPath, settings.ZpoolPath );
    #endif
    }

    private static async Task<int> RunWithKestrelAsync( SnapsInAZfsSettings settings, IConfigurationRoot configurationRoot )
    {
        SiazService.Timestamp = DateTimeOffset.Now;
        using SiazService? serviceInstance = GetSiazServiceInstance ( settings );

        if ( serviceInstance is null )
        {
            Logger.Fatal ( "Failed to create service instance - exiting" );
            LogManager.Shutdown( );

            return (int)ExitCode.ENOATTR;
        }

        WebApplicationBuilder serviceBuilder = WebApplication.CreateBuilder( );

        // ReSharper disable once AccessToDisposedClosure
        // Disposal happens after service shutdown, so this inspection can be ignored here.
        serviceBuilder.Host
                      .UseSystemd( )
                      .ConfigureServices ( ( _, services ) => services.AddHostedService ( _ => serviceInstance ) );

        serviceBuilder.WebHost
                      .UseConfiguration (
                                         configurationRoot
                                            .GetRequiredSection ( "Monitoring" )
                                             // ReSharper disable once SettingNotFoundInConfiguration
                                             // Disabling because this is, in fact, in the configuration, where the code says it is.
                                            .GetSection ( "Kestrel" )
                                        )
                      .UseKestrel ( ( _, kestrelOptions ) =>
                                    {
                                        kestrelOptions.Configure (
                                                                  configurationRoot
                                                                     .GetRequiredSection ( "Monitoring" )
                                                                      // ReSharper disable once SettingNotFoundInConfiguration
                                                                      // Disabling because this is, in fact, in the configuration, where the code says it is.
                                                                     .GetSection ( "Kestrel" )
                                                                 )
                                                      .Load( );
                                    }
                                  );
        WebApplication svc = serviceBuilder.Build( );

        RouteGroupBuilder statusGroup = svc.MapGroup ( "/" );
        statusGroup.MapGet ( "/",                 ServiceObserver.GetApplicationStateAsync );
        statusGroup.MapGet ( "/state",            ServiceObserver.GetApplicationStateAsync );
        statusGroup.MapGet ( "/fullState",        ServiceObserver.GetFullApplicationStateAsync );
        statusGroup.MapGet ( "/workingSet",       ServiceObserver.GetWorkingSetAsync );
        statusGroup.MapGet ( "/version",          ServiceObserver.GetVersionAsync );
        statusGroup.MapGet ( "/serviceStartTime", ServiceObserver.GetServiceStartTimeAsync );
        statusGroup.MapGet ( "/nextRunTime",      ServiceObserver.GetNextRunTimeAsync );

        RouteGroupBuilder snapshotsGroup = svc.MapGroup ( "/snapshots" );
        snapshotsGroup.MapGet ( "/", ServiceObserver.GetAllSnapshotCountsAsync );

        snapshotsGroup.MapGet ( "/allCounts",                      ServiceObserver.GetAllSnapshotCountsAsync );
        snapshotsGroup.MapGet ( "/lastSnapshotPrunedTime",         ServiceObserver.GetLastSnapshotPrunedTimeAsync );
        snapshotsGroup.MapGet ( "/lastSnapshotTakenTime",          ServiceObserver.GetLastSnapshotTakenTimeAsync );
        snapshotsGroup.MapGet ( "/prunedFailedLastRunCount",       ServiceObserver.GetSnapshotsPrunedFailedLastRunCountAsync );
        snapshotsGroup.MapGet ( "/prunedFailedLastRunNames",       ServiceObserver.GetSnapshotsPrunedFailedLastRunNamesAsync );
        snapshotsGroup.MapGet ( "/prunedFailedsinceStartCount",    ServiceObserver.GetSnapshotsPrunedFailedSinceStartCountAsync );
        snapshotsGroup.MapGet ( "/prunedSucceededLastRunCount",    ServiceObserver.GetSnapshotsPrunedSucceededLastRunCountAsync );
        snapshotsGroup.MapGet ( "/prunedSucceededSinceStartCount", ServiceObserver.GetSnapshotsPrunedSucceededSinceStartCountAsync );
        snapshotsGroup.MapGet ( "/takenFailedLastRunCount",        ServiceObserver.GetSnapshotsTakenFailedLastRunCountAsync );
        snapshotsGroup.MapGet ( "/takenFailedLastRunNames",        ServiceObserver.GetSnapshotsTakenFailedLastRunNamesAsync );
        snapshotsGroup.MapGet ( "/takenFailedSinceStartCount",     ServiceObserver.GetSnapshotsTakenFailedSinceStartCountAsync );
        snapshotsGroup.MapGet ( "/takenSucceededLastRunCount",     ServiceObserver.GetSnapshotsTakenSucceededLastRunCountAsync );
        snapshotsGroup.MapGet ( "/takenSucceededSinceStartCount",  ServiceObserver.GetSnapshotsTakenSucceededSinceStartCountAsync );

        // ReSharper restore HeapView.DelegateAllocation
        using CancellationTokenSource tokenSource = new ( );
        CancellationToken             masterToken = tokenSource.Token;
        await svc.StartAsync ( masterToken ).ConfigureAwait ( true );
        await svc.WaitForShutdownAsync ( masterToken ).ConfigureAwait ( true );

        return SiazService.ExitStatus;
    }

    private static async Task<int> RunWithoutKestrelAsync( SnapsInAZfsSettings settings )
    {
        SiazService.Timestamp = DateTimeOffset.Now;
        using SiazService? serviceInstance = GetSiazServiceInstance ( settings );

        if ( serviceInstance is null )
        {
            Logger.Fatal ( "Failed to create service instance - exiting" );
            LogManager.Shutdown( );

            return (int)ExitCode.ENOATTR;
        }

        // Disposal happens after service shutdown, so this inspection can be ignored here
        // ReSharper disable once AccessToDisposedClosure
        IHost serviceHost = Host.CreateDefaultBuilder( )
                                .UseSystemd( )
                                .ConfigureServices ( ( _, services ) => services.AddHostedService ( _ => serviceInstance ) )
                                .Build( );
        using CancellationTokenSource tokenSource = new ( );
        CancellationToken             masterToken = tokenSource.Token;
        await serviceHost.StartAsync ( masterToken ).ConfigureAwait ( true );
        await serviceHost.WaitForShutdownAsync ( masterToken ).ConfigureAwait ( true );

        return SiazService.ExitStatus;
    }

    private static ExitCode ValidateSettings( ref readonly SnapsInAZfsSettings settings )
    {
        SettingsValidator validator = SettingsValidator.Validate ( in settings );

        if ( validator.IsSettingsObjectNull )
        {
            Logger.Fatal ( "Failed to validate settings. Settings null. SnapsInAZfs will now terminate." );

            return ExitCode.EFTYPE;
        }

        bool autoDetectionInvoked = false;

        if ( validator is { IsAutoConfigureLocalSystemNameRequested: true } )
        {
            settings.AutoDetectAndSetLocalSystemName( );
            autoDetectionInvoked = true;
        }

        if ( validator is { IsAutoConfigureZfsPathRequested: true } )
        {
            settings.AutoDetectAndSetZfsPath( );
            autoDetectionInvoked = true;
        }

        if ( validator is { IsAutoConfigureZpoolPathRequested: true } )
        {
            settings.AutoDetectAndSetZpoolPath( );
            autoDetectionInvoked = true;
        }

        if ( autoDetectionInvoked )
        {
            Logger.Debug ( "Re-validating configuration after one or more auto-detected settings altered." );
            SettingsValidator.Validate ( in settings, validator );
        }

        if ( !validator.IsInvalid )
        {
            return ExitCode.EOK;
        }

        Logger.Fatal ( "Failed to validate settings." );
        Logger.Debug ( $"{validator.ValidationErrors} errors found in validation." );
        Logger.Debug ( $"Validation status: {JsonSerializer.Serialize ( validator )}" );
        Logger.Debug ( $"Settings object including all files and overrides: {JsonSerializer.Serialize ( settings )}: " );
        Logger.Fatal ( "SnapsInAZfs will now terminate." );

        return ExitCode.EFTYPE;
    }
}
