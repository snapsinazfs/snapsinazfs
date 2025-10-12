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
using System.CommandLine.Completions;
using Extensions;
using F = StringFormattingConstants;

public partial class SiazCommandLine
{
  private Option<TriStateOptionValue> ConfigGlobalCommand_DaemonizeOption { get; } =
    new ( nameof (SnapsInAZfsSettings.Daemonize), nameof (SnapsInAZfsSettings.Daemonize).ToLowerInvariant ( ), "-D" )
    {
      Arity       = ArgumentArity.ExactlyOne,
      Description = $"The {nameof (SnapsInAZfsSettings.Daemonize)} option controls whether SIAZ executes interactively or as a service.",
      Required    = false
    };

  private Option<uint> ConfigGlobalCommand_DaemonTimerIntervalSecondsOption { get; } =
    new ( nameof (SnapsInAZfsSettings.DaemonTimerIntervalSeconds) )
    {
      Arity       = ArgumentArity.ExactlyOne,
      Description = $"""
                     The {nameof (SnapsInAZfsSettings.DaemonTimerIntervalSeconds)} option controls the interval, in seconds, of timer ticks for the daemon's operation scheduling.
                     Values other than the suggested values may be used but sticking to the suggested values is strongly recommended.
                     """,
      Hidden      = false,
      Required    = false
    };

  private Option<TriStateOptionValue> ConfigGlobalCommand_DryRunOption { get; } =
    new ( nameof (SnapsInAZfsSettings.DryRun), nameof (SnapsInAZfsSettings.DryRun).ToLowerInvariant ( ), "-n" )
    {
      Arity       = ArgumentArity.ExactlyOne,
      Description = $"The {nameof (SnapsInAZfsSettings.DryRun)} option controls whether SIAZ can make changes.",
      Required    = false
    };

  private Option<string> ConfigGlobalCommand_LocalSystemNameOption { get; } =
    new ( nameof (SnapsInAZfsSettings.LocalSystemName) )
    {
      Arity = ArgumentArity.ExactlyOne,
      Description = $"""
                     The {nameof (SnapsInAZfsSettings.LocalSystemName)} option sets the system name that will be used by SIAZ for replication scenarios.
                     Recommended value is the fully-qualified domain name of the system or leaving it unset, which will result in the FQDN being used.
                     """,
      HelpName = "hostname",
      Required = false
    };

  private Option<TriStateOptionValue> ConfigGlobalCommand_PruneSnapshotsOption { get; } =
    new ( nameof (SnapsInAZfsSettings.PruneSnapshots) )
    {
      Arity       = ArgumentArity.ExactlyOne,
      Description = $"The {nameof (SnapsInAZfsSettings.PruneSnapshots)} option controls whether SIAZ will destroy expired snapshots.",
      Required    = false
    };

  private Option<TriStateOptionValue> ConfigGlobalCommand_TakeSnapshotsOption { get; } =
    new ( nameof (SnapsInAZfsSettings.TakeSnapshots) )
    {
      Arity       = ArgumentArity.ExactlyOne,
      Description = $"The {nameof (SnapsInAZfsSettings.TakeSnapshots)} option controls whether SIAZ will create new snapshots.",
      Required    = false
    };

  private Option<FileInfo> ConfigGlobalCommand_ZfsPathOption { get; } =
    new ( nameof (SnapsInAZfsSettings.ZfsPath) )
    {
      Arity    = ArgumentArity.ExactlyOne,
      HelpName = "path",
      Description = $"""
                     The {F.B}{nameof (SnapsInAZfsSettings.ZfsPath)}{F._B} option specifies the path to the zfs utility.
                     <path> must be resolvable in the context in which SIAZ will be run.
                     """,
      Required = false
    };

  private Option<FileInfo> ConfigGlobalCommand_ZpoolPathOption { get; } =
    new ( nameof (SnapsInAZfsSettings.ZpoolPath) )
    {
      Arity    = ArgumentArity.ExactlyOne,
      HelpName = "path",
      Description = $"""
                     The {F.B}{nameof (SnapsInAZfsSettings.ZpoolPath)}{F._B} option specifies the path to the zpool utility.
                     <path> must be resolvable in the context in which SIAZ will be run.
                     """,
      Required = false
    };

  private Option<string> ConfigGlobalCommandOutputFileOption { get; }
    = new ( OutputFileOptionName )
      {
        Description = $"""
                       Absolute or relative path to the file to which changes will be written.
                       If the file already exists, it must be a JSON text file.
                       The value of the JSON node at the path corresponding to the modified setting will be {F.U}replaced{F._U} by this operation.
                       If the file does not exist, a new JSON file will be created containing only the modified setting.
                       """,
        Recursive = true,
        Arity     = ArgumentArity.ZeroOrOne
      };

  private Option<FileInfo[]> ConfigOption { get; }
    = new ( ConfigOptionName, "--config-file", "--config-files" )
      {
        Arity = ArgumentArity.OneOrMore,
        Description = $"""
                       Path to a configuration file to use instead of the default configuration files, for this invocation.
                       {F.U}Missing files will be ignored.{F._U}
                       Specify the option multiple times to use multiple files, with one path per option instance.
                       Files are processed in the order they appear on the command line.
                       {F.FGYELLOW}{F.N}ALL{F.P} configuration files at the standard paths will be ignored unless included in your list.{F._FGCOLOR}
                       """,
        HelpName            = "path",
        Recursive           = true,
        Required            = false,
        DefaultValueFactory = static _ => GetConfigFileListFromEnvironmentOrDefault ( ).ToArray ( )
      };

  private Option<bool> DaemonizeOption { get; }
    = new ( DaemonizeOptionName, "-D" )
      {
        Arity       = ArgumentArity.ZeroOrOne,
        Description = "Run SnapsInAZfs as a daemon.",
        Required    = false
      };

  internal Option<LoggingLevel> LogLevelOption { get; }
    = new ( LogLevelOptionName )
      {
        Description = "Override global logging level.",
        Recursive   = true,
        Arity       = ArgumentArity.ZeroOrOne
      };

  private Option<bool> MonitorOption { get; }
    = new ( MonitorOptionName )
      {
        Arity       = ArgumentArity.ZeroOrOne,
        Description = "Enable the monitoring endpoints.",
        Recursive   = false,
        Required    = false
      };

  private Option<bool> PruneSnapshotsOption { get; }
    = new ( PruneSnapshotsOptionName )
      {
        Arity = ArgumentArity.ZeroOrOne,
        Description
          = $"Enables expired snapshot pruning, {F.U}overriding{F._U} the PruneSnapshots setting from configuration. If dry-run is enabled, reports snapshots that would be destroyed but does not perform the destroy operations.",
        Required = false
      };

  private Option<bool> TakeSnapshotsOption { get; }
    = new ( TakeSnapshotsOptionName )
      {
        Arity = ArgumentArity.ZeroOrOne,
        Description
          = $"Enables new snapshot processing, {F.U}overriding{F._U} the TakeSnapshots setting from configuration. If dry-run is enabled, reports snapshots that would be taken but does not perform the snapshot operations.",
        Required = false
      };

  private Option<bool> ZfsSchemaChangeCommands_ConfirmImpactOption { get; } = new ( ZfsSchemaChangeCommands_ConfirmImpactOptionName )
                                                                              {
                                                                                Arity       = ArgumentArity.ExactlyOne,
                                                                                Required    = true,
                                                                                Description = "Required option to indicate that you want this action to be carried out without further interaction."
                                                                              };

  private Option<bool> ZfsSchemaChangeCommands_ReallyConfirmImpactOption { get; } = new ( ZfsSchemaChangeCommands_ReallyConfirmImpactOptionName )
                                                                                    {
                                                                                      Arity       = ArgumentArity.ExactlyOne,
                                                                                      Required    = true,
                                                                                      Hidden      = true,
                                                                                      Description = "Required option to indicate that you understand this is an immediate, permanent, and unrecoverable action without complete backups and that you accept all responsibility for anything that happens, including data loss."
                                                                                    };

  private const string             BaseConfigFilesEnvVarName                             = "SIAZ_ConfigFiles_";
  private const string             ConfigOptionName                                      = "--config";
  private const string             DaemonizeOptionName                                   = "--daemonize";
  private const string             DaemonTimerIntervalOptionName                         = "--daemon-timer-interval";
  private const string             DebugOptionName                                       = "--debug";
  private const string             LogLevelOptionName                                    = "--log-level";
  private const string             MonitorOptionName                                     = "--monitor";
  private const string             OutputFileOptionName                                  = "--output-file";
  private const string             PruneSnapshotsOptionName                              = "--prune-snapshots";
  private const StringSplitOptions RemoveAndTrimStringSplitEntries                       = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
  private const string             TakeSnapshotsOptionName                               = "--take-snapshots";
  private const string             ZfsSchemaChangeCommands_ConfirmImpactOptionName       = "--confirm";
  private const string             ZfsSchemaChangeCommands_ReallyConfirmImpactOptionName = "--i-understand-this-cannot-be-undone";

  /// <inheritdoc cref="CompletionItemExtensions.ToOrderableCompletionItem{TEnum}(TEnum)" />
  /// <remarks>
  ///   This method is just a proxy to <see cref="CompletionItemExtensions.ToOrderableCompletionItem{TEnum}(TEnum)" /> for concise
  ///   syntax in LINQ method calls.
  /// </remarks>
  /// <typeparam name="TEnum">An <see langword="enum" /> type.</typeparam>
  /// <param name="enumMember">
  ///   The <see langword="enum" /> member of type <typeparamref name="TEnum" /> from which to create the <see cref="CompletionItem" />
  ///   .
  /// </param>
  /// <returns>
  ///   A <see cref="CompletionItem" /> with <see cref="CompletionItem.Label" /> set to the name of the <see langword="enum" /> member
  ///   and <see cref="CompletionItem.SortText" /> set to the value of the <see langword="enum" /> member.
  /// </returns>
  private static CompletionItem EnumToOrderableCompletionItem<TEnum> ( TEnum enumMember ) where TEnum : unmanaged, Enum
  {
    return enumMember.ToOrderableCompletionItem ( );
  }

  /// <summary>
  ///   Collection of <see cref="LoggingLevel" /> completions in value order.
  /// </summary>
  private static IEnumerable<CompletionItem> GetLoggingLevelCompletionItems ( CompletionContext context )
  {
    return Enum
          .GetValues<LoggingLevel> ( )
          .Select ( EnumToOrderableCompletionItem );
  }

  /// <summary>
  ///   Collection of <see cref="TriStateOptionValue" /> completions in value order.
  /// </summary>
  private static IEnumerable<CompletionItem> GetTriStateOptionValueCompletions ( CompletionContext context )
  {
    return Enum
          .GetValues<TriStateOptionValue> ( )
          .Select ( EnumToOrderableCompletionItem );
  }
}
