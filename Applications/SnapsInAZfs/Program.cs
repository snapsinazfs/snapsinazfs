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
using System.Text.Json;
using CommandLine;
using Interop;
using Interop.Zfs.ZfsCommandRunner;
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
  private static          Logger               _logger         = LogManager.CreateNullLogger ( );
  private static readonly IMonitor             ServiceObserver = new Monitor ( );
  private static          IConfigurationRoot?  _configurationRoot;
  internal static         SnapsInAZfsSettings? Settings;
  internal static         IZfsCommandRunner    ZfsCommandRunnerSingleton = null!;

  [ExcludeFromCodeCoverage ( Justification = "Largely un-testable" )]
  public static async Task<int> Main ( string[] argv )
  {
    // Program startup sequence is:
    // 1. Immediately set up early logging from appsettings.json before anything else.
    // 2. Parse the command line
    //    A. If --config option exists, load those files and set application configuration. Otherwise, load standard files and set configuration.
    //    B. Apply any overrides to loaded configuration according to options on the command line.
    // 3. Invoke the command line.
    // 4. Proceed according to command line invocation result or terminate if command line errors are indicated.

    await using LogFactory earlyLogFactory = InitializeEarlyLogging ( out _logger );

    if ( !ProcessCommandLine ( argv, out SCL.ParseResult siazCliParseResult, out Settings, out _configurationRoot, out ExitCode siazCliInvocationExitCode )
      || siazCliInvocationExitCode is not ExitCode.EOK )
    {
      LogManager.Shutdown ( );

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
    }

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

  /// <summary>
  ///   Loads the appsettings.json file from the working directory and configures logging ONLY.
  /// </summary>
  /// <remarks>
  ///   After this method is called, the LogManager will have the configuration from appsettings.json.<br />
  ///   Any configuration changes made after this need to be followed by a call to <see cref="LogManager.ReconfigExistingLoggers()" />
  ///   to continue.
  /// </remarks>
  /// <returns>The <see cref="ISetupBuilder" /> created from the configuration.</returns>
  [MustDisposeResource]
  private static LogFactory InitializeEarlyLogging ( out Logger logger )
  {
    IConfigurationRoot appSettingsJson = new ConfigurationBuilder ( )
                                        .SetBasePath ( Directory.GetCurrentDirectory ( ) )
                                        .AddJsonFile ( "appsettings.json", false, false )
                                        .AddEnvironmentVariables ( )
                                        .Build ( );
    ISetupBuilder builder = LogManager.Setup ( )
                                      .LoadConfigurationFromSection ( appSettingsJson );

    logger = LogManager.GetLogger ( $"{nameof (SnapsInAZfs)}.{nameof (Program)}" );
    LogManager.ReconfigExistingLoggers ( true );

    return builder.LogFactory;
  }

  private static bool ProcessCommandLine (
    string[]                                        arguments,
    out                        SCL.ParseResult      siazCliParseResult,
    [NotNullWhen ( true )] out SnapsInAZfsSettings? settings,
    [NotNullWhen ( true )] out IConfigurationRoot?  configurationRoot,
    out                        ExitCode             exitCode
  )
  {
    SiazCommandLine siazCli = new ( );
    siazCliParseResult = siazCli.Parse (
                                        arguments,
                                        out SCL.RootCommand _
                                       );
    exitCode = siazCli.Invoke (
                               out SCL.RootCommand _,
                               out siazCliParseResult,
                               out settings,
                               out configurationRoot,
                               arguments
                              );

    return exitCode == ExitCode.EOK;
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
