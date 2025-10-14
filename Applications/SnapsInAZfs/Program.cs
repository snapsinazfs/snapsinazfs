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

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CommandLine;
using CommandLine.Extensions;
using Interop;
using Interop.Zfs.ZfsCommandRunner;
using Interop.Zfs.ZfsTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Monitoring;
using NLog.Config;
using NLog.Extensions.Logging;
using Settings.Validation;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;
using SCL = System.CommandLine;
using F = StringFormattingConstants;

[UsedImplicitly]
internal static class Program
{
  private const           string               SnapsInAZfsAppName = "SnapsInAZfs";
  private static          Logger               _logger            = LogManager.CreateNullLogger ( );
  private static readonly IMonitor             ServiceObserver    = new Monitor ( );
  internal static         SnapsInAZfsSettings? Settings;
  internal static         IZfsCommandRunner    ZfsCommandRunnerSingleton = null!;
  private static readonly ManualResetEventSlim Blocker                   = new ( false );
  private static          IConfigurationRoot?  _configurationRoot;

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

    siazCli.Parse ( argv );

    ( FileInfo[] configFiles, bool configFilesImplicit ) = siazCli.GetConfigurationFileCollection ( );

    ConfigurationBuilder configBuilder = new ( );

    foreach ( FileInfo configFile in configFiles )
    {
      configBuilder.AddJsonFile ( configFile.FullName, configFilesImplicit, false );
    }

    _logger.Debug ( $"Adding environment variables with prefix {F.U}{EnvironmentVariableFilterPrefix}{F._U} to configuration." );
    configBuilder.AddEnvironmentVariables ( static filter => filter.Prefix = EnvironmentVariableFilterPrefix );

    _configurationRoot = configBuilder.Build ( );
    LogManager.Flush ( );
    LogManager.Setup ( static logConfigBuilder => { logConfigBuilder.LoadConfigurationFromSection ( _configurationRoot ).ReloadConfiguration ( ); }
                     );
    LogManager.ReconfigExistingLoggers ( true );

    _logger = LogManager.GetCurrentClassLogger ( );

    _logger.Debug ( "Logging reconfigured." );

    if ( siazCli.RootCommandParseResult is not { Errors.Count: 0 } noErrorsParseResult )
    {
      return (int)siazCli.Invoke ( Console.Out, Console.Error );
    }

    siazCli.ZfsSchemaCheckInvoked              += SiazCli_ZfsSchemaCheckInvoked;
    siazCli.ZfsSchemaCleanInvoked              += SiazCli_ZfsSchemaCleanInvoked;
    siazCli.ZfsSchemaInitializeInvoked         += SiazCli_ZfsSchemaInitializeInvoked;
    siazCli.GlobalConfigurationChangeRequested += SiazCli_GlobalConfigurationChangeRequested;
    siazCli.RunSiazInvoked                     += SiazCli_RunSiazInvoked;
    siazCli.InvokeCompleted                    += SiazCli_InvokeCompleted;

    Blocker.Reset ( );
    ExitCode siazCliExitCode = siazCli.Invoke ( Console.Out, Console.Error );
    Blocker.Wait ( );

    if ( siazCliExitCode != ExitCode.EOK )
    {
      return (int)siazCliExitCode;
    }

    if ( noErrorsParseResult.RootCommandResult.GetResult ( siazCli.LogLevelOption ) is { Implicit: false, Tokens.Count: 1 } logLevelOptionResult )
    {
      foreach ( LoggingRule rule in LogManager.Configuration!.LoggingRules )
      {
        rule.EnableLoggingForLevels ( logLevelOptionResult.GetValue ( siazCli.LogLevelOption ).ToNLogLevel ( ), NLogLevel.Off );
      }
    }

    switch ( noErrorsParseResult.CommandResult.Command.Name )
    {
      case SiazCommandLine.ConfigConsoleCommandName:
      {
        Settings = _configurationRoot.Get<SnapsInAZfsSettings> ( );

        if ( ValidateSettings ( in Settings ) is not ExitCode.EOK and var badResult )
        {
          return (int)badResult;
        }

        _logger.Debug ( "Settings passed basic validation checks." );
        _logger.ConditionalTrace ( $"Final settings object: {JsonSerializer.Serialize ( Settings )}" );

        try
        {
          if ( TryGetZfsCommandRunner<ZfsCommandRunner> ( Settings, out IZfsCommandRunner? zfsCommandRunner ) )
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
      }
        break;

      case SiazCommandLine.ConfigGlobalCommandName:
        _logger.Debug ( "Would have modified config files." );
        break;

      case SiazCommandLine.CronCommandName or SiazCommandLine.RunCommandName:
      {
        Settings = _configurationRoot.Get<SnapsInAZfsSettings> ( );

        if ( ValidateSettings ( in Settings ) is not ExitCode.EOK and var badResult )
        {
          return (int)badResult;
        }

        _logger.Debug ( "Settings passed basic validation checks." );
        _logger.Trace ( $"Final settings object: {JsonSerializer.Serialize ( Settings )}" );

        string mode = noErrorsParseResult.GetRequiredValue ( siazCli.RunCommandModeArgument );

        OptionResult[] options = noErrorsParseResult.GetExplicitSettingsKeyedResults ( );

        if ( options.Length > 0 )
        {
          _logger.Debug ( "Explicit options provided to {0} command. Processing overrides.", noErrorsParseResult.CommandResult.Command.Name );
          ApplyCommandLineOverridesToSettings ( _configurationRoot, options, ref Settings );
        }

        if ( Settings.Monitoring.EnableHttp && mode == "service" )
        {
          return await RunWithKestrelAsync ( Settings, _configurationRoot ).ConfigureAwait ( true );
        }

        await RunWithoutMonitoring ( ).ConfigureAwait ( true );

        return SiazService.ExitStatus;
      }
    }

    return 0;
  }

  private static async Task RunWithoutMonitoring ( )
  {
    IHost appHost = Host.CreateDefaultBuilder ( )
                        .UseSystemd ( )
                        .ConfigureAppConfiguration ( static builder => builder.AddConfiguration ( _configurationRoot ?? throw new InvalidOperationException ( "Configuration not built." ) ) )
                        .ConfigureServices
                           ( static serviceCollection => serviceCollection.AddHostedService<ISiazService>
                               ( static _ => GetSiazServiceInstance ( Settings! ) ?? throw new InvalidOperationException ( "Unable to get service instance." ) )
                           )
                        .Build ( );

    using CancellationTokenSource tokenSource = new ( );
    CancellationToken             masterToken = tokenSource.Token;
    await appHost.StartAsync ( masterToken ).ConfigureAwait ( true );
    await appHost.WaitForShutdownAsync ( masterToken ).ConfigureAwait ( true );
  }

  private static void SiazCli_InvokeCompleted ( object? sender, SiazCommandLine siazCommandLine )
  {
    try
    {
      _logger.Trace ( "Invoked event received. Signaling wait handle." );
      Blocker.Set ( );
    }
    catch ( Exception e )
    {
      _logger.Error ( e );
    }
  }

  private static void SiazCli_RunSiazInvoked ( object? sender, RunSiazActionEventArgs e )
  {
    _logger.Debug ( "Running SIAZ." );
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
  ///   Loads the built-in NLog configuration, allowing environment variable overrides.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     After this method is called, the LogManager will have the hard-coded configuration in
  ///     <see cref="EarlyNLogConfiguration" />.<br />
  ///     This is only used until the real configuration has been loaded, as a fallback to protect against missing configuration.<br />
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
  private static void InitializeEarlyLogging ( out Logger logger )
  {
    IConfigurationRoot earlyLoggerConfig = new ConfigurationBuilder ( )
                                          .AddInMemoryCollection ( EarlyNLogConfiguration )
                                          .AddEnvironmentVariables ( static filter => filter.Prefix = EarlyLoggingOverrideEnvironmentVariableFilterPrefix )
                                          .Build ( );
    ISetupBuilder builder = LogManager.Setup ( ).LoadConfigurationFromSection ( earlyLoggerConfig );

    Logger earlyLogger = builder.GetLogger ( $"{nameof (SnapsInAZfs)}.{nameof (Program)}" );
    LogManager.ReconfigExistingLoggers ( true );

    earlyLogger.Debug ( ( ) => $"Early logging config:\n{earlyLoggerConfig.GetDebugView ( )}" );
    logger = earlyLogger;
  }

  /// <summary>
  ///   Overrides configuration values specified in configuration files or environment variables with arguments supplied on
  ///   the CLI.
  /// </summary>
  /// <param name="baseConfig"></param>
  /// <param name="optionResults"></param>
  /// <param name="programSettings">
  ///   A reference to an instance of a <see cref="SnapsInAZfsSettings" /> object to modify
  /// </param>
  private static void ApplyCommandLineOverridesToSettings ( in IConfigurationRoot baseConfig, OptionResult[] optionResults, ref SnapsInAZfsSettings programSettings )
  {
    _logger.Debug ( "Overriding settings using options from command line." );

    ConfigurationBuilder builder = new ( );
    builder.AddConfiguration ( baseConfig );
    Dictionary<string, string?> settingsOverrides = [ ];

    foreach ( OptionResult optionResult in optionResults )
    {
      ISiazSettingsKeyedOption option = (ISiazSettingsKeyedOption)optionResult.Option;

      switch ( option )
      {
        case SiazSettingsOption<bool>:
          settingsOverrides.Add ( option.SettingsKey, $"{optionResult.GetValueOrDefault ( false )}" );
          continue;

        case SCL.Option<uint>:
          settingsOverrides.Add ( option.SettingsKey, $"{optionResult.GetValueOrDefault<uint> ( ):D}" );
          continue;

        case SCL.Option<TriStateOptionValue>:
          settingsOverrides.Add ( option.SettingsKey, $"{optionResult.GetValueOrDefault<TriStateOptionValue> ( ).ToBoolean ( )}" );
          continue;

        case SCL.Option<string>:
          settingsOverrides.Add ( option.SettingsKey, optionResult.GetValueOrDefault<string> ( ) );
          continue;
      }
    }

    builder.AddInMemoryCollection ( settingsOverrides );
    IConfigurationRoot overriddenConfig = builder.Build ( );

    programSettings = overriddenConfig.Get<SnapsInAZfsSettings> ( ) ?? programSettings;
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

  [SuppressMessage ( "ReSharper", "StringLiteralTypo", Justification = "It's nlog configuration syntax..." )]
  private static Dictionary<string, string?> EarlyNLogConfiguration { get; }
    = new ( )
      {
        [ "NLog:autoReload" ]                                             = "false",
        [ "NLog:throwConfigurationExceptions" ]                           = "true",
        [ "NLog:internalLogLevel" ]                                       = "Warn",
        [ "NLog:internalLogFile" ]                                        = "${basedir}/internal-nlog.txt",
        [ "NLog:variables:var_logdir" ]                                   = "/var/log/SnapsInAZfs",
        [ "NLog:time:type" ]                                              = "FastLocal",
        [ "NLog:default-wrapper:type" ]                                   = "AsyncWrapper",
        [ "NLog:default-wrapper:overflowAction" ]                         = "Block",
        [ "NLog:targets:console:type" ]                                   = "ColoredConsole",
        [ "NLog:targets:console:detectConsoleAvailable" ]                 = "false",
        [ "NLog:targets:console:enableAnsiOutput" ]                       = "true",
        [ "NLog:targets:console:layout" ]                                 = "${longdate}|${pad:padding=-6:${uppercase:${level}}}|${message} ${exception:format=tostring}",
        [ "NLog:targets:console:rowHighlightingRules:0:condition" ]       = "level == LogLevel.Warn",
        [ "NLog:targets:console:rowHighlightingRules:0:foregroundColor" ] = "Yellow",
        [ "NLog:targets:console:rowHighlightingRules:1:condition" ]       = "level == LogLevel.Error",
        [ "NLog:targets:console:rowHighlightingRules:1:foregroundColor" ] = "Red",
        [ "NLog:targets:console:rowHighlightingRules:2:condition" ]       = "level == LogLevel.Fatal",
        [ "NLog:targets:console:rowHighlightingRules:2:foregroundColor" ] = "White",
        [ "NLog:targets:console:rowHighlightingRules:2:backgroundColor" ] = "DarkRed",
        [ "NLog:rules:0:ruleName" ]                                       = "Console",
        [ "NLog:rules:0:logger" ]                                         = "*",
        [ "NLog:rules:0:minLevel" ]                                       = "Warn",
        [ "NLog:rules:0:writeTo" ]                                        = "console",
        [ "NLog:rules:0:filterDefaultAction" ]                            = "Log",
        [ "NLog:rules:0:enabled" ]                                        = "true"
      };

  private const string? EarlyLoggingOverrideEnvironmentVariableFilterPrefix = $"{EnvironmentVariableFilterPrefix}EarlyLogger_";

  private const string? EnvironmentVariableFilterPrefix = $"{SnapsInAZfsAppName}_";

  private static ExitCode ValidateSettings ( ref readonly SnapsInAZfsSettings settings )
  {
    if ( Environment.GetEnvironmentVariable ( $"{EnvironmentVariableFilterPrefix}DisableConfigurationValidation" ) is not null )
    {
      _logger.Warn ( "Configuration validation has been disabled. This is unsupported and intended for debugging and development only." );
      return ExitCode.EOK;
    }

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
    _logger.ConditionalDebug ( $"{validator.ValidationErrors} errors found in validation." );
    _logger.ConditionalTrace ( $"Validation status: {JsonSerializer.Serialize ( validator )}" );
    _logger.ConditionalTrace ( $"Settings object including all files and overrides: {JsonSerializer.Serialize ( settings )}: " );
    _logger.Fatal ( "SnapsInAZfs will now terminate." );
    _logger.Factory.Flush ( );

    return ExitCode.EFTYPE;
  }
}
