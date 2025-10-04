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

namespace SnapsInAZfs.CommandLine;

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ConfigConsole;
using Interop.Zfs.ZfsTypes;
using NLog.Config;
using NLog.Extensions.Logging;

public partial class SiazCommandLine
{
  internal const string RunCommandName                  = "run";
  private const  string ConfigCommandName               = "config";
  private const  string ConfigConsoleCommandName        = "console";
  private const  string ConfigGlobalCommandName         = "global";
  private const  string ConfigGlobalDryRunCommandName   = "dry-run";
  private const  string KestrelConfigurationSectionName = "Kestrel";
  private const  string ZfsCommandName                  = "zfs";
  private const  string ZfsSchemaCheckCommandName       = "check";
  private const  string ZfsSchemaCleanCommandName       = "clean";
  private const  string ZfsSchemaCommandName            = "schema";
  private const  string ZfsSchemaInitializeCommandName  = "initialize";

  private bool LoadAdditionalConfigurationFiles (
    ParseResult                              parseResult,
    ConfigurationBuilder                     builder,
    out                        OptionResult? additionalConfigResult,
    [NotNullWhen ( true )] out string[]?     additionalRequestedFiles
  )
  {
    Unsafe.SkipInit ( out additionalRequestedFiles );
    bool success = true;

    additionalConfigResult = parseResult.CommandResult.GetResult ( AdditionalConfigOption );
    if ( additionalConfigResult is { Implicit: true } )
    {
      additionalRequestedFiles = [ ];

      return true;
    }

    if ( additionalConfigResult is not { Implicit: false, Tokens.Count: > 0 } )
    {
      return false;
    }

    additionalRequestedFiles = additionalConfigResult.GetValueOrDefault<string[]> ( );

    if ( additionalRequestedFiles is not { Length: > 0 } )
    {
      return false;
    }

    foreach ( string filePath in additionalRequestedFiles )
    {
      FileInfo fileInfo = new ( filePath );
      if ( !fileInfo.Exists )
      {
        Logger.Warn ( $"Configuration file not found at {filePath}." );
        success = false;

        continue;
      }

      Logger.Trace ( "Adding base configuration file {0} to configuration.", filePath );
      builder.AddJsonFile ( filePath, false, false );
    }

    return success;
  }

  private bool LoadBaseConfigurationFiles (
    ParseResult          parseResult,
    ConfigurationBuilder configurationBuilder,
    out OptionResult     configResult,
    out string[]         strings
  )
  {
    bool success = true;
    configResult = parseResult.CommandResult.GetResult ( ConfigOption ) ??
                   throw new CommandLineInvocationException ( "Could not determine the set of configuration files to load." );
    strings = configResult.GetValueOrDefault<string[]> ( );

    foreach ( string filePath in strings )
    {
      FileInfo fileInfo = new ( filePath );
      if ( !fileInfo.Exists )
      {
        Logger.Warn ( $"Configuration file not found at {filePath}." );

        success = false;

        continue;
      }

      Logger.Trace ( "Adding base configuration file {0} to configuration.", filePath );
      configurationBuilder.AddJsonFile ( filePath, false, false );
    }

    return success;
  }

  private bool LoadConfigurationFiles (
    [NotNullWhen ( true )] ref SnapsInAZfsSettings? settings,
    [NotNullWhen ( true )] out IConfigurationRoot?  rootConfiguration,
    in                         ParseResult          cliParseResult
  )
  {
    Unsafe.SkipInit ( out rootConfiguration );
    // Configuration is built in the following order from various sources.
    // Configurations from all sources are merged, and the final configuration that will be used is the result of the merged configurations.
    // If conflicting items exist in multiple configuration sources, the configuration of the configuration source added latest will
    // override earlier values.
    // See the SnapsInAZfs.Settings.Logging.LoggingSettings class for nlog configuration details.
    // See SnapsInAZfs(5) for detailed configuration documentation.
    // Configuration order:
    // 1a. (if --config option provided) Base configuration files specified with the --config option, in the order entered.
    // 1b. (if no --config option provided):
    //       /usr/local/share/SnapsInAZfs/SnapsInAZfs.json
    //       /etc/SnapsInAZfs/SnapsInAZfs.local.json
    //       /etc/SnapsInAZfs/SnapsInAZfs.nlog.json
    //       ./SnapsInAZfs.json
    //       ./SnapsInAZfs.local.json
    //       ./SnapsInAZfs.nlog.json
    //       ~/.config/SnapsInAZfs/SnapsInAZfs.local.json
    // 2. Supplementary configuration files specified with the --additional-config option, in the order entered.
    // 3. Environment variables
    // 4. Command-line options passed to the current invocation of SIAZ.
    Logger.Debug ( "Getting base configuration from files" );
    ConfigurationBuilder configBuilder = new ( );

    if ( !LoadBaseConfigurationFiles ( cliParseResult, configBuilder, out OptionResult baseConfigOptionResult, out string[] requestedFiles ) )
    {
      Logger.Error ( "One or more base configuration files could not be loaded. Aborting." );

      rootConfiguration = null;

      return false;
    }

    if ( !LoadAdditionalConfigurationFiles (
                                            cliParseResult,
                                            configBuilder,
                                            out OptionResult? additionalConfigOptionResult,
                                            out string[]? additionalRequestedFiles
                                           ) )
    {
      Logger.Error (
                    $"""
                     One or more supplementary configuration files (--additional-config option) could not be loaded. Aborting.
                     Requested files:
                     {string.Join ( Environment.NewLine, additionalRequestedFiles ?? [ "[NO FILES REQUESTED]" ] )}
                     """
                   );

      rootConfiguration = null;

      return false;
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
      IConfigurationSection kestrelSection = rootConfiguration.GetRequiredSection ( "Monitoring" ).GetSection ( KestrelConfigurationSectionName );

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

  private Task<int> RunSiaz ( ParseResult parseResult, CancellationToken cancellation )
  {
    LoadConfigurationFiles ( ref Program.Settings, out IConfigurationRoot? rootConfiguration, parseResult );
    string[] configFiles = parseResult.CommandResult.GetRequiredValue<string[]> ( ConfigOptionName );
    Console.WriteLine ( $"Running siaz with command line {string.Join ( ' ', parseResult.Tokens )}." );
    Console.WriteLine ( parseResult.CommandResult.GetResult ( ConfigOptionName ) );
    Console.WriteLine ( string.Join ( ',', configFiles ) );
    Console.WriteLine ( "Not yet implemented." );

    return Task.FromResult ( 0 );
  }

  private static int SetGlobalOption ( ParseResult parseResult )
  {
    Console.WriteLine ( "Requested to set global option." );
    Console.WriteLine ( parseResult.CommandResult.ToString ( ) );
    Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

    TriStateOptionValue dryRun = parseResult.GetValue<TriStateOptionValue> ( ConfigStateArgumentName );

    return (int)dryRun;
  }

  private static void StartConfigConsole ( ParseResult parseResult )
  {
    Console.WriteLine ( parseResult.CommandResult.ToString ( ) );
    Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );
  }

  private static int ZfsSchemaCheck ( ParseResult parseResult )
  {
    Console.WriteLine ( parseResult.CommandResult.ToString ( ) );
    Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

    return 0;
  }

  private static int ZfsSchemaClean ( ParseResult arg )
  {
    Console.WriteLine ( "Cleaning SIAZ schema from ZFS" );

    return 0;
  }

  private static int ZfsSchemaInitialize ( ParseResult parseResult )
  {
    Console.WriteLine ( parseResult.CommandResult.ToString ( ) );
    Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

    return 0;
  }
}
