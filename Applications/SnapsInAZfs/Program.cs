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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using CommandLine;
using Interop;
using Interop.Zfs.ZfsCommandRunner;
using Interop.Zfs.ZfsTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Monitoring;
using NLog.Config;
using NLog.Extensions.Logging;
using PowerArgs;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;
using SCL = System.CommandLine;

[UsedImplicitly]
internal static class Program
{
  private const           string               SnapsInAZfsAppName = "SnapsInAZfs";
  private static          Logger               _logger            = LogManager.CreateNullLogger ( );
  private static readonly IMonitor             ServiceObserver    = new Monitor ( );
  private static          IConfigurationRoot?  _configurationRoot;
  internal static         SnapsInAZfsSettings? Settings;
  internal static         IZfsCommandRunner    ZfsCommandRunnerSingleton = null!;
  internal static         IHost?               ServiceContainer;
  private static readonly ManualResetEventSlim Blocker = new ( false );

  [ExcludeFromCodeCoverage ( Justification = "Largely un-testable" )]
  public static async Task<int> Main ( string[] argv )
  {
    // Rough program execution outline:
    // 1. Immediately set up early logging from hard-coded default and environment variable override before anything else.
    //   * This writes to stderr, at warn level, unless overridden by environment variables.
    //   * Environment variables use dotnet configuration style. Early logging uses the prefix SnapsInAZfs_EarlyLogger_
    //     SnapsInAZfs_EarlyLogger_NLog would therefore correspond to the NLog section, SnapsInAZfs_EarlyLogger_NLog:rules
    //     would be the NLog:rules section, and so on.
    //   * Much of the early logging is conditionally compiled only if the DEBUG symbol is set at compile time, as some of it can interfere with
    //     normal command line interactions - particularly tab completion.
    // 2. Parse the command line
    //   A. If --config option exists, load those files and set application configuration. Otherwise, load standard files and set configuration.
    //   B. Apply any overrides from environment variables to configuration.
    //   C. Apply any global overrides from command line options to configuration.
    //     * NOTE: Only global options are applied at this stage. Individual commands are responsible for any further configuration
    //       relevant to them based on their options and arguments.
    // 3. Register handlers for the various commands via events on the command line wrapper class.
    //   * Handlers get invoked when Invoke is called on the ParseResult from step 2. The actions raise the events, which are subscribed to here.
    //     Invoke happens in a following step.
    // 4. Invoke the command line, which will raise events as appropriate.
    //   * These events are raised asynchronously, so handlers are responsible for application lifetime from that point on and will
    //     signal the wait handle the wait handle as appropriate.
    //   * SCL creates and passes a CancellationToken with a default 2 second timeout. May need to deal with that.
    //   * Event handlers SHOULD destroy the SiazCommandLine object, once it is no longer needed, so it doesn't live forever.
    // 5. Event handlers either carry out their operations directly or signal that program execution should continue in some specific way.
    //   A. If running as a service, set up a wait handle and block on it to keep the app alive. When stop is requested, shut down the service host
    //      and signal the wait handle, so the program can clean up and exit.
    //   B. If running as a one-shot invocation, be sure not to prompt the user in a blocking way if the terminal has been redirected.

    InitializeEarlyLogging ( out _logger );

    SiazCommandLine? siazCli = new ( );

    try
    {
      siazCli.Parse ( argv );
      FileInfo[] configFiles = siazCli.GetConfigurationFileCollection ( );

      WebApplicationBuilder appBuilder = WebApplication.CreateBuilder ( new WebApplicationOptions { ApplicationName = SnapsInAZfsAppName, Args = argv } );

      foreach ( FileInfo configFile in configFiles )
      {
        appBuilder.Configuration.AddJsonFile ( configFile.FullName, true, false );
      }

      _logger.Debug ( "Adding environment variables to configuration." );
      appBuilder.Configuration.AddEnvironmentVariables ( static filter => filter.Prefix = EnvironmentVariableFilterPrefix );

      siazCli.ZfsSchemaCheckInvoked              += SiazCli_ZfsSchemaCheckInvoked;
      siazCli.ZfsSchemaCleanInvoked              += SiazCli_ZfsSchemaCleanInvoked;
      siazCli.ZfsSchemaInitializeInvoked         += SiazCli_ZfsSchemaInitializeInvoked;
      siazCli.GlobalConfigurationChangeRequested += SiazCli_GlobalConfigurationChangeRequested;
      siazCli.RunSiazInvoked                     += SiazCli_RunSiazInvoked;
      siazCli.InvokeCompleted                    += SiazCli_InvokeCompleted;
      
      Blocker.Reset ( );
      ExitCode siazCliExitCode = siazCli.Invoke ( Console.Out, Console.Error );
      Blocker.Wait ( );

      if ( siazCli.RootCommandParseResult.Errors.Count > 0 )
      {
        return (int)siazCliExitCode;
      }

      switch ( siazCli.RootCommandParseResult.CommandResult.Command.Name )
      {
        case SiazCommandLine.ConfigConsoleCommandName:
          _logger.Debug ( "Would have launched the config console." );
          break;

        case SiazCommandLine.ConfigGlobalCommandName:
          _logger.Debug ( "Would have modified config files." );
          break;

        case SiazCommandLine.CronCommandName or SiazCommandLine.RunCommandName:
          _logger.Debug ( "Would have built and run the appBuilder." );
          break;
      }
    }
    finally
    {
      await siazCli.DisposeAsync ( );
      siazCli = null;
    }

    // Just cutting it off here for now.
    return 0;

    CommandLineArguments args = await Args.ParseAsync<CommandLineArguments> ( argv ).ConfigureAwait ( true );

    ApplyCommandLineArgumentOverrides ( in args, Settings );

    if ( args.ConfigConsole )
    {
      try
      {
        if ( TryGetZfsCommandRunner<ZfsCommandRunner> ( Settings, out IZfsCommandRunner zfsCommandRunner ) )
        {
          ConfigConsole.ConfigConsole.RunConsoleInterface ( zfsCommandRunner );
        }
      }
      catch ( Exception e )
      {
        _logger.Fatal ( e, "Error in configuration console - Exiting" );
        LogManager.Shutdown ( );

        return (int)ExitCode.GenericError;
      }

      LogManager.Shutdown ( );

      return 0;
    }

    if ( ValidateSettings ( in Settings ) is not ExitCode.EOK and var badResult )
    {
      return (int)badResult;
    }

    _logger.Debug ( "Settings passed basic validation checks." );
    _logger.Trace ( $"Final settings object: {JsonSerializer.Serialize ( Settings )}" );

    return Settings.Monitoring.EnableHttp switch
           {
             true => await RunWithKestrelAsync ( Settings, _configurationRoot ).ConfigureAwait ( true ),
             _    => await RunWithoutKestrelAsync ( Settings ).ConfigureAwait ( true )
           };
  }

  private static async void SiazCli_InvokeCompleted ( object? sender, SiazCommandLine siazCommandLine )
  {
    try
    {
      _logger.Trace ( "Command line invocation completed. Signaling wait handle." );
      Blocker.Set ( );
    }
    catch ( Exception e )
    {
      _logger.Error ( e );
    }
  }

  private static void SiazCli_RunSiazInvoked ( object? sender, RunSiazActionEventArgs e )
  {
    _logger.Fatal ( "run not yet implemented. No operations have been carried out." );
  }

  private static void SiazCli_GlobalConfigurationChangeRequested ( object? sender, GlobalConfigChangeEventArgs e )
  {
    _logger.Fatal ( "config global not yet implemented. No settings have been modified." );
  }

  private static void SiazCli_ZfsSchemaInitializeInvoked ( object? sender, ZfsSchemaChangeEventArgs e )
  {
    _logger.Fatal ( $"zfs schema initialize not yet implemented. Would {( e.AllowedToProceed ? string.Empty : "not " )}execute, as entered. Requested pools: {( e.Pools.Length > 0 ? e.Pools : [ "<all pools>" ] ).ToSpaceSeparatedSingleLineString ( )}" );
  }

  private static void SiazCli_ZfsSchemaCleanInvoked ( object? sender, ZfsSchemaChangeEventArgs e )
  {
    _logger.Fatal ( $"zfs schema clean not yet implemented. Would {( e.AllowedToProceed ? string.Empty : "not " )}execute, as entered. Requested pools: {( e.Pools.Length > 0 ? e.Pools : [ "<all pools>" ] ).ToSpaceSeparatedSingleLineString ( )}" );
  }

  private static void SiazCli_ZfsSchemaCheckInvoked ( object? sender, ZfsSchemaActionEventArgs e )
  {
    _logger.Fatal ( $"zfs schema check not yet implemented. Requested pools: {( e.Pools.Length > 0 ? e.Pools : [ "<all pools>" ] ).ToSpaceSeparatedSingleLineString ( )}" );
  }

  /// <summary>
  ///   Loads the appsettings.json file from the working directory and configures logging ONLY.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     After this method is called, the LogManager will have the hard-coded configuration in <see cref="NLogInitialConfiguration" />
  ///     .<br />
  ///     This is only used until the real configuration has been loaded, as a fallback to protect against missing configuration.<br />
  ///     The configuration is written to a temporary file, loaded, and then the file is deleted.
  ///   </para>
  ///   <para>
  ///     The file is opened with <see cref="FileShare.None" /> and will be truncated before writing the initial configuration to it.
  ///   </para>
  ///   <para>
  ///     The configuration can be overridden with environment variables using the prefix <c>SnapsInAZfs_NLog__</c> (note the
  ///     double-underscore).<br />
  ///     The possible variables are all settings that appear in the JSON configuration.<br />
  ///     They are case-sensitive, and the key for a setting is its JSON path, with each level of the hierarchy separated with a
  ///     double-underscore on Linux or a colon on Windows.<br />
  ///     For example, the NLog.variables.var_logdir, as an environment variable name, is:
  ///     <c>SnapsInAZfs_NLog__variables__var_logdir</c> on Linux or <c>SnapsInAZfs_NLog:variables:var_logdir</c> on Windows.
  ///   </para>
  /// </remarks>
  /// <returns>The <see cref="ISetupBuilder" /> created from the configuration.</returns>
  private static bool InitializeEarlyLogging ( out Logger logger )
  {
    FileInfo tempNLogConfigFile = new ( Path.GetTempFileName ( ) );

    if ( Environment.GetEnvironmentVariable ( "SnapsInAZfs_EnableEarlyLogging" ) is "1" )
    {
      // Write to stdout since we don't have a logger yet.
      Console.WriteLine ( $"Initial logging configuration temporary file: {tempNLogConfigFile.FullName}" );
    }

    using FileStream tempNLogConfigFileStream = tempNLogConfigFile.Open ( FileMode.Truncate, FileAccess.ReadWrite, FileShare.None );
    Logger?          earlyLogger              = null;

    try
    {
      using ( StreamWriter tempNLogConfigWriter = new ( tempNLogConfigFileStream, Encoding.UTF8, 2048 ) )
      {
        tempNLogConfigWriter.Write ( NLogInitialConfiguration );
      }

      IConfigurationRoot appSettingsJson = new ConfigurationBuilder ( )
                                          .SetBasePath ( Directory.GetCurrentDirectory ( ) )
                                          .AddJsonFile ( tempNLogConfigFile.FullName, false, false )
                                          .AddEnvironmentVariables ( static filter => filter.Prefix = EarlyLoggingOverrideEnvironmentVariableFilterPrefix )
                                          .Build ( );

      ISetupBuilder? builder = LogManager.Setup ( )
                                         .LoadConfigurationFromSection ( appSettingsJson );

      earlyLogger = builder.GetLogger ( $"{nameof (SnapsInAZfs)}.{nameof (Program)}" );
      LogManager.ReconfigExistingLoggers ( true );

      tempNLogConfigFileStream.Close ( );
      earlyLogger.Debug ( $"Early Logging config:\n{appSettingsJson.GetDebugView ( )}" );
    }
    finally
    {
      earlyLogger ??= LogManager.CreateNullLogger ( );

      if ( Environment.GetEnvironmentVariable ( $"{EnvironmentVariableFilterPrefix}EnableEarlyLogging" ) is "1" )
      {
        earlyLogger.Debug ( $"Deleting initial logging configuration temporary file {tempNLogConfigFile.FullName}." );
      }

      logger = earlyLogger;
      tempNLogConfigFile.Delete ( );
    }

    return true;
  }

  /// <summary>
  ///   Overrides configuration values specified in configuration files or environment variables with arguments supplied on
  ///   the CLI.
  /// </summary>
  /// <param name="args"></param>
  /// <param name="programSettings">
  ///   A reference to an instance of a <see cref="SnapsInAZfsSettings" /> object to modify
  /// </param>
  internal static void ApplyCommandLineArgumentOverrides ( in CommandLineArguments args, SnapsInAZfsSettings programSettings )
  {
    _logger.Debug ( "Overriding settings using arguments from command line." );

    programSettings.DryRun         |= args.DryRun;
    programSettings.TakeSnapshots  =  ( programSettings.TakeSnapshots  || args.TakeSnapshots  || args.Cron )                    && !args.NoTakeSnapshots;
    programSettings.PruneSnapshots =  ( programSettings.PruneSnapshots || args.PruneSnapshots || args.ForcePrune || args.Cron ) && !args.NoPruneSnapshots;
    programSettings.Daemonize      =  ( programSettings.Daemonize      || args.Daemonize )                                      && args is { NoDaemonize: false, ConfigConsole: false };
    programSettings.Monitoring.EnableHttp
      = ( programSettings.Monitoring.EnableHttp || args.Monitor ) && args is { NoMonitor : false, ConfigConsole: false };

    if ( args.DaemonTimerInterval > 0 )
    {
      programSettings.DaemonTimerIntervalSeconds = Math.Clamp ( args.DaemonTimerInterval, 1u, 60u );
    }
  }

  internal static bool TryGetZfsCommandRunner<TRunner> (
    SnapsInAZfsSettings                           settings,
    [NotNullWhen ( true )] out IZfsCommandRunner? zfsCommandRunner,
    bool                                          reuseSingleton = true
  )
    where TRunner : IZfsCommandRunner<TRunner>, new ( )
  {
    if ( reuseSingleton && ZfsCommandRunnerSingleton is IZfsCommandRunner<TRunner> singleton )
    {
      zfsCommandRunner = singleton;

      return true;
    }

    _logger.Trace ( "Getting ZFS command runner for the current environment" );

    if ( string.IsNullOrWhiteSpace ( settings.ZfsPath ) || string.IsNullOrWhiteSpace ( settings.ZpoolPath ) )
    {
      zfsCommandRunner = null;
      return false;
    }

    try
    {
      zfsCommandRunner = TRunner.Create ( settings.ZfsPath, settings.ZpoolPath );
    }
    catch ( ArgumentNullException ex )
    {
      _logger.Fatal ( ex, "Null or empty string provided for ZfsPath or ZpoolPath - Cannot continue" );
      zfsCommandRunner = null;

      return false;
    }
    catch ( FileNotFoundException ex )
    {
      _logger.Fatal ( ex, ex.Message );
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
    if ( !TryGetZfsCommandRunner<ZfsCommandRunner> ( settings, out IZfsCommandRunner? zfsCommandRunner ) )
    {
      return null;
    }

    if ( settings.Monitoring.EnableHttp )
    {
      return new ( settings, zfsCommandRunner, ServiceObserver, ServiceObserver );
    }

    return new ( settings, zfsCommandRunner );
  }

  private static async Task<int> RunWithKestrelAsync ( SnapsInAZfsSettings settings, IConfigurationRoot configurationRoot )
  {
    SiazService.Timestamp = DateTimeOffset.Now;
    using SiazService? serviceInstance = GetSiazServiceInstance ( settings );

    if ( serviceInstance is null )
    {
      _logger.Fatal ( "Failed to create service instance - exiting" );
      LogManager.Shutdown ( );

      return (int)ExitCode.ENOATTR;
    }

    WebApplicationBuilder serviceBuilder = WebApplication.CreateBuilder ( );

    // ReSharper disable once AccessToDisposedClosure
    // Disposal happens after service shutdown, so this inspection can be ignored here.
    serviceBuilder.Host
                  .UseSystemd ( )
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
                                                .Load ( );
                                }
                              );
    WebApplication svc = serviceBuilder.Build ( );

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

  internal const string? EnvironmentVariableFilterPrefix = $"{SnapsInAZfsAppName}_";
  internal const string? EarlyLoggingOverrideEnvironmentVariableFilterPrefix = $"{EnvironmentVariableFilterPrefix}EarlyLogger_";

  private const string NLogInitialConfiguration = """
                                                  {
                                                    "NLog": {
                                                      "autoReload": false,
                                                      "throwConfigExceptions": true,
                                                      "internalLogLevel": "Warn",
                                                      "internalLogFile": "${basedir}/internal-nlog.txt",
                                                      "extensions": [
                                                        { "assembly": "NLog.Extensions.Logging" }
                                                      ],
                                                      "variables": {
                                                        "var_logdir": "/var/log/SnapsInAZfs"
                                                      },
                                                      "time": {
                                                        "type": "FastLocal"
                                                      },
                                                      "default-wrapper": {
                                                        "type": "AsyncWrapper",
                                                        "overflowAction": "Block"
                                                      },
                                                      "targets": {
                                                        "early-console": {
                                                          "type": "ColoredConsole",
                                                          "detectConsoleAvailable": false,
                                                          "enableAnsiOutput ": true,
                                                          "StdErr": true,
                                                          "layout": "${longdate}|${pad:padding=-6:${uppercase:${level}}}|${message} ${exception:format=tostring}",
                                                          "rowHighlightingRules": [
                                                            {
                                                              "condition": "level == LogLevel.Warn",
                                                              "foregroundColor": "Yellow"
                                                            },
                                                            {
                                                              "condition": "level == LogLevel.Error",
                                                              "foregroundColor": "Red"
                                                            },
                                                            {
                                                              "condition": "level == LogLevel.Fatal",
                                                              "foregroundColor": "White",
                                                              "backgroundColor": "DarkRed"
                                                            }
                                                          ]
                                                        }
                                                      },
                                                      "rules": {
                                                        "0": {
                                                          "ruleName": "Console",
                                                          "logger": "*",
                                                          "minLevel": "Warn",
                                                          "writeTo": "early-console",
                                                          "filterDefaultAction": "Log",
                                                          "enabled": true
                                                        }
                                                      }
                                                    }
                                                  }


                                                  """;

  private static async Task<int> RunWithoutKestrelAsync ( SnapsInAZfsSettings settings )
  {
    SiazService.Timestamp = DateTimeOffset.Now;
    using SiazService? serviceInstance = GetSiazServiceInstance ( settings );

    if ( serviceInstance is null )
    {
      _logger.Fatal ( "Failed to create service instance - exiting" );
      LogManager.Shutdown ( );

      return (int)ExitCode.ENOATTR;
    }

    // Disposal happens after service shutdown, so this inspection can be ignored here
    // ReSharper disable once AccessToDisposedClosure
    IHost serviceHost = Host.CreateDefaultBuilder ( )
                            .UseSystemd ( )
                            .ConfigureServices ( ( _, services ) => services.AddHostedService ( _ => serviceInstance ) )
                            .Build ( );
    using CancellationTokenSource tokenSource = new ( );
    CancellationToken             masterToken = tokenSource.Token;
    await serviceHost.StartAsync ( masterToken ).ConfigureAwait ( true );
    await serviceHost.WaitForShutdownAsync ( masterToken ).ConfigureAwait ( true );

    return SiazService.ExitStatus;
  }

  private static ExitCode ValidateSettings ( ref readonly SnapsInAZfsSettings settings )
  {
    SettingsValidator validator = SettingsValidator.Validate ( in settings );

    if ( validator.IsSettingsObjectNull )
    {
      _logger.Fatal ( "Failed to validate settings. Settings null. SnapsInAZfs will now terminate." );

      return ExitCode.EFTYPE;
    }

    bool autoDetectionInvoked = false;

    if ( validator is { IsAutoConfigureLocalSystemNameRequested: true } )
    {
      settings.AutoDetectAndSetLocalSystemName ( );
      autoDetectionInvoked = true;
    }

    if ( validator is { IsAutoConfigureZfsPathRequested: true } )
    {
      settings.AutoDetectAndSetZfsPath ( );
      autoDetectionInvoked = true;
    }

    if ( validator is { IsAutoConfigureZpoolPathRequested: true } )
    {
      settings.AutoDetectAndSetZpoolPath ( );
      autoDetectionInvoked = true;
    }

    if ( autoDetectionInvoked )
    {
      _logger.Debug ( "Re-validating configuration after one or more auto-detected settings altered." );
      SettingsValidator.Validate ( in settings, validator );
    }

    if ( !validator.IsInvalid )
    {
      return ExitCode.EOK;
    }

    _logger.Fatal ( "Failed to validate settings." );
    _logger.Debug ( $"{validator.ValidationErrors} errors found in validation." );
    _logger.Debug ( $"Validation status: {JsonSerializer.Serialize ( validator )}" );
    _logger.Debug ( $"Settings object including all files and overrides: {JsonSerializer.Serialize ( settings )}: " );
    _logger.Fatal ( "SnapsInAZfs will now terminate." );

    return ExitCode.EFTYPE;
  }
}
