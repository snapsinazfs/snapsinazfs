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
using LogLevel = NLog.LogLevel;

public partial class SiazCommandLine
{
  private Option<string[]> AdditionalConfigOption { get; }
    = new ( AdditionalConfigOptionName )
      {
        Arity = ArgumentArity.OneOrMore,
        Description = $"""
                       One or more supplementary configuration files to MERGE WITH the base configuration, for this invocation.
                       These files are processed in the order they are specified, AFTER all base configuration files or files provided to the {ConfigOptionName} option are processed.
                       See SnapsInAZfs.json(5) for details about using the {ConfigOptionName} and {AdditionalConfigOptionName} options together.
                       """,
        Recursive           = true,
        Required            = false,
        DefaultValueFactory = static _ => EnvironmentOrDefaultAdditionalConfigurationFiles
      };

  private Option<string> ConfigGlobalCommandOutputFileOption { get; }
    = new ( OutputFileOptionName )
      {
        Description = """
                      Absolute or relative path to the file to which changes will be written.
                      If the file already exists, it must be a JSON text file.
                      The JSON node at the path corresponding to the modified setting will be REPLACED by this operation.
                      If the file does not exist, a new JSON file will be created containing only the modified setting.
                      """,
        Recursive = true,
        Arity     = ArgumentArity.ZeroOrOne
      };

  private Option<string[]> ConfigOption { get; }
    = new ( ConfigOptionName, "--config-file", "--config-files" )
      {
        Arity = ArgumentArity.OneOrMore,
        Description = $"""
                       One or more configuration files to REPLACE the default base configuration files, for this invocation.
                       Configuration files at the standard paths will be ignored unless included in your list.
                       To add additional layers of configuration files on top of the default configuration files, see the {AdditionalConfigOptionName} option.
                       See SnapsInAZfs.json(5) for details about using the {ConfigOptionName} and {AdditionalConfigOptionName} options together.
                       """,
        Recursive           = true,
        Required            = false,
        DefaultValueFactory = static _ => EnvironmentOrDefaultBaseConfigurationFiles
      };

  private Option<bool> DaemonizeOption { get; }
    = new ( DaemonizeOptionName, "-D" )
      {
        Arity       = ArgumentArity.ZeroOrOne,
        Description = "Run SnapsInAZfs as a daemon.",
        Required    = false
      };

  private Option<int> DaemonTimerIntervalOption { get; }
    = new ( DaemonTimerIntervalOptionName )
      {
        Arity       = ArgumentArity.ZeroOrOne,
        Description = "Override the configured daemon event processing timer. Specified as a whole number of seconds.",
        Required    = false
      };

  private Option<bool> DebugOption { get; }
    = new ( DebugOptionName )
      {
        Arity = ArgumentArity.ZeroOrOne,
        Description = """
                      (DEPRECATED) Debug level output logging.
                      Change log level in SnapsInAZfs.nlog.json for normal usage.
                      """,
        Recursive = true
      };

  private static string[] EnvironmentOrDefaultAdditionalConfigurationFiles { get; }
    = Environment.GetEnvironmentVariable ( AdditionalConfigFilesEnvVarName )
                ?.Split ( ':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
    ??
      [
        "/etc/SnapsInAZfs/SnapsInAZfs.local.json", "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", "SnapsInAZfs.json", "SnapsInAZfs.local.json",
        "SnapsInAZfs.nlog.json"
      ];

  private static string[] EnvironmentOrDefaultBaseConfigurationFiles { get; }
    = Environment.GetEnvironmentVariable ( BaseConfigFilesEnvVarName )
                ?.Split ( ':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
   ?? [ "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json", "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json" ];

  private Option<LogLevel> LogLevelOption { get; }
    = new ( LogLevelOptionName )
      {
        Description = $"""
                       Override global logging level.
                       Possible values: {string.Join ( ',', LogLevel.AllLevels )}
                       """,
        Recursive = true,
        Arity     = ArgumentArity.ZeroOrOne
      };

  private Option<bool> MonitorOption { get; }
    = new ( MonitorOptionName )
      {
        Arity       = ArgumentArity.ZeroOrOne,
        Description = "Enable the monitoring endpoints.",
        Recursive   = true
      };

  private Option<bool> PruneSnapshotsOption { get; }
    = new ( PruneSnapshotsOptionName )
      {
        Arity = ArgumentArity.ZeroOrOne,
        Description
          = $"(DEPRECATED) Enables expired snapshot pruning. If dry-run is enabled, reports snapshots that would be destroyed but does not perform the destroy operations.{Environment.NewLine}This option is deprecated in this context. Use in the `siaz run` context instead.",
        Required = false
      };

  private Option<bool> TakeSnapshotsOption { get; }
    = new ( TakeSnapshotsOptionName )
      {
        Arity = ArgumentArity.ZeroOrOne,
        Description
          = $"(DEPRECATED) Enables new snapshot processing. If dry-run is enabled, reports snapshots that would be taken but does not perform the snapshot operations.{Environment.NewLine}This option is deprecated in this context. Use in the `siaz run` context instead.",
        Required = false
      };

  private const string AdditionalConfigFilesEnvVarName = "SnapsInAZfs_AdditionalConfigFiles";
  private const string AdditionalConfigOptionName      = "--additional-config";
  private const string BaseConfigFilesEnvVarName       = "SnapsInAZfs_BaseConfigFiles";
  private const string ConfigOptionName                = "--config";
  private const string DaemonizeOptionName             = "--daemonize";
  private const string DaemonTimerIntervalOptionName   = "--daemon-timer-interval";
  private const string DebugOptionName                 = "--debug";
  private const string LogLevelOptionName              = "--log-level";
  private const string MonitorOptionName               = "--monitor";
  private const string OutputFileOptionName            = "--output-file";
  private const string PruneSnapshotsOptionName        = "--prune-snapshots";
  private const string TakeSnapshotsOptionName         = "--take-snapshots";
}
