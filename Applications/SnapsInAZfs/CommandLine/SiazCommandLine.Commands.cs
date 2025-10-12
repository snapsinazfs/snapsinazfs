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
using System.Diagnostics.CodeAnalysis;
using ConfigConsole;
using Interop.Zfs.ZfsTypes;
using NLog.Config;
using NLog.Extensions.Logging;
using F = StringFormattingConstants;

public partial class SiazCommandLine
{
  /// <summary>
  ///   A reference to the <see cref="RootCommand" /> of the command line.
  /// </summary>
  public RootCommand RootCommand { get; private set; }

  private Command ConfigCommand { get; } = new (
                                                ConfigCommandName,
                                                "Perform configuration operations on SIAZ and managed pools/datasets directly or via the configuration console."
                                               );

  private Command ConfigConsoleCommand { get; } = new (
                                                       ConfigConsoleCommandName,
                                                       "Launches the configuration console TUI."
                                                      );

  private Command ConfigGlobalCommand { get; } = new (
                                                      ConfigGlobalCommandName,
                                                      """
                                                      Modify global settings in the root of the JSON configuration files.
                                                      If no --output-file option is specified, resulting changes will be written to the last configuration file loaded, including any specified on the command line.
                                                      """
                                                     );

  private Command CronCommand { get; } = new (
                                              CronCommandName,
                                              $"""
                                               {F.FGYELLOW}(DEPRECATED){F._FGCOLOR} An alias for the {F.B}run{F._B} command.
                                               Update to use the {F.B}run{F._B} command, as this alias will be removed in a future version.
                                               """
                                             );

  private Command RunCommand { get; } = new (
                                             RunCommandName,
                                             """
                                             Run SIAZ, optionally specifying override options.
                                             Use this context when executing one-off operations or for custom service/script-based invocations.
                                             """
                                            );

  private Command TemplatesCommand { get; } = new (
                                                   TemplatesCommandName,
                                                   """
                                                   View, modify, create, or remove templates.
                                                   """
                                                  );

  private Command TemplatesCreateCommand { get; } = new (
                                                         TemplatesCreateCommandName,
                                                         """
                                                         Create a new template.
                                                         """
                                                        );

  private Command TemplatesListCommand { get; } = new (
                                                       TemplatesListCommandName,
                                                       """
                                                       List existing templates.
                                                       """
                                                      );

  private Command TemplatesModifyCommand { get; } = new (
                                                         TemplatesModifyCommandName,
                                                         """
                                                         Modify an existing template.
                                                         """
                                                        );

  private Command TemplatesRemoveCommand { get; } = new (
                                                         TemplatesRemoveCommandName,
                                                         """
                                                         Remove an existing template.
                                                         """
                                                        );

  private Command TemplatesShowCommand { get; } = new (
                                                       TemplatesShowCommandName,
                                                       """
                                                       Show the full configuration of an existing template.
                                                       """
                                                      );

  private Command ZfsCommand { get; } = new (
                                             ZfsCommandName,
                                             "Perform operations on ZFS pools and datasets managed by SIAZ."
                                            );

  private Command ZfsSchemaCheckCommand { get; } = new (
                                                        ZfsSchemaCheckCommandName,
                                                        "Checks the property schema for SnapsInAZfs in ZFS and reports any missing properties for pool roots. Checks all pools by default."
                                                       );

  private Command ZfsSchemaCleanCommand { get; } = new (
                                                        ZfsSchemaCleanCommandName,
                                                        "Completely removes all pool and dataset properties that came from SIAZ."
                                                       );

  private Command ZfsSchemaCommand { get; } = new (
                                                   ZfsSchemaCommandName,
                                                   "Perform operations on properties of ZFS pools and datasets used by SIAZ."
                                                  );

  private Command ZfsSchemaInitializeCommand { get; } = new (
                                                             ZfsSchemaInitializeCommandName,
                                                             "Updates the property schema for SnapsInAZfs in ZFS, using default values. Will not overwrite StandardBooleanOptions that are already set."
                                                            );

  internal const string ConfigCommandName               = "config";
  internal const string ConfigConsoleCommandName        = "console";
  internal const string ConfigGlobalCommandName         = "global";
  internal const string ConfigGlobalDryRunCommandName   = "dry-run";
  internal const string CronCommandName                 = "--cron";
  internal const string KestrelConfigurationSectionName = "Kestrel";
  internal const string RunCommandName                  = "run";
  internal const string TemplatesCommandName            = "templates";
  internal const string TemplatesCreateCommandName      = "create";
  internal const string TemplatesListCommandName        = "list";
  internal const string TemplatesModifyCommandName      = "modify";
  internal const string TemplatesRemoveCommandName      = "remove";
  internal const string TemplatesShowCommandName        = "show";
  internal const string ZfsCommandName                  = "zfs";
  internal const string ZfsSchemaCheckCommandName       = "check";
  internal const string ZfsSchemaCleanCommandName       = "clean";
  internal const string ZfsSchemaCommandName            = "schema";
  internal const string ZfsSchemaInitializeCommandName  = "initialize";

  [MemberNotNullWhen ( true )]
  internal static bool LoadConfigurationFromConfigurationFiles (
    [NotNullWhen ( true )] out SnapsInAZfsSettings? settings,
    [NotNullWhen ( true )] out IConfigurationRoot?  rootConfiguration,
    FileInfo[]                                      configFiles
  )
  {
    _logger.Trace ( "Loading configuration." );

    ConfigurationBuilder configBuilder = new ( );

    foreach ( FileInfo file in configFiles )
    {
      if ( !file.Exists )
      {
        _logger.Warn ( "Configuration file not found at {0}", file.FullName );

        continue;
      }

      _logger.Debug ( "Loading configuration file {0}", file.FullName );

      configBuilder.AddJsonFile ( file.FullName, true, false );
    }

    if ( configBuilder.Sources.Count == 0 )
    {
      _logger.Fatal ( "Configuration files not found at any of these locations: {0}", configFiles.ToCommaSeparatedSingleLineString ( true ) );
      rootConfiguration = null;
      settings          = null;

      return false;
    }

    _logger.Trace ( $"Building {nameof (IConfigurationRoot)} from configuration files." );

    rootConfiguration = configBuilder.Build ( );

    _logger.Trace ( $"Binding settings objects from {nameof (IConfigurationRoot)}." );

    try
    {
      settings = rootConfiguration.Get<SnapsInAZfsSettings> ( ) ?? throw new InvalidOperationException ( );

      _logger.ConditionalDebug ( "Initial configuration built from parsed files: {0}", rootConfiguration.SerializeToJson ( ) );

      // ReSharper disable once SettingNotFoundInConfiguration
      IConfigurationSection kestrelSection = rootConfiguration.GetRequiredSection ( "Monitoring" ).GetSection ( "Kestrel" );

      if ( kestrelSection.Exists ( ) )
      {
        IEnumerable<IConfigurationSection> kestrelSettings = kestrelSection.GetChildren ( );
        settings.Monitoring.Kestrel = kestrelSettings.ToDictionary ( static k => k.Key, static v => v.SerializeToJson ( ) );
      }

      IConfigurationSection nlogConfigSection = rootConfiguration.GetSection ( "NLog" );
      LogManager.Configuration = nlogConfigSection.Exists ( ) ? new NLogLoggingConfiguration ( nlogConfigSection ) : new LoggingConfiguration ( );
      LogManager.ReconfigExistingLoggers ( true );
    }
    catch ( Exception ex )
    {
      _logger.Fatal ( ex, "Unable to parse settings from JSON" );

      settings = null;

      return false;
    }

    return true;
  }
}
