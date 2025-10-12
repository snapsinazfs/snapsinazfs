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
using System.Runtime.CompilerServices;
using Extensions;
using Interop;
using F = StringFormattingConstants;

/// <summary>
///   The high-level interface to the command line functionality.
/// </summary>
/// <remarks>
///   <para>
///     Note that this type has multiple parts in files named SiazCommandLine.[Category].cs.
///   </para>
///   <para>
///     For the configuration portion of the CLI, certain configuration elements are intentionally omitted, to discourage unnecessary
///     modification without reading the docs.
///   </para>
/// </remarks>
[PublicAPI]
public sealed partial class SiazCommandLine
{
  private static Logger _logger = LogManager.GetCurrentClassLogger ( );

  /// <summary>
  ///   Creates a new instance of <see cref="SiazCommandLine" /> and initializes its structure.
  /// </summary>
  public SiazCommandLine ( )
  {
    ConfigureCommandLineTree ( );
  }

  private IConfigurationRoot? _configurationRoot;

  /// <summary>
  ///   A reference to the result of parsing the root command, for convenience.
  /// </summary>
  public ParseResult? RootCommandParseResult { get; private set; }

  /// <summary>
  ///   Builds the command line parser configuration.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     See <c>SnapsInAZfs(8)</c> and <c>SnapsInAZfs(5)</c> for operation and configuration details.
  ///   </para>
  /// </remarks>
  [MemberNotNull ( nameof (RootCommand) )]
  public RootCommand ConfigureCommandLineTree ( )
  {
    return RootCommand = new RootCommand (
                                          $"""
                                           SnapsInAZfs - A snapshot management system for OpenZFS.

                                           All commands, options, arguments, and values are case-sensitive.
                                           See {F.B}SnapsInAZfs(8){F._B} for detailed information.

                                           Basic operation:
                                           {F.B}SnapsInAZfs run{F._B}

                                           Runs SnapsInAZfs using the configuration from JSON files and environment variables.
                                           See {F.B}SnapsInAZfs(8){F._B} and {F.B}SnapsInAZfs.json(5){F._B} for detailed usage and configuration information.
                                           """
                                         )
// TODO: Remove this suppression once the next .net 10 build is released (fix provided in https://github.com/dotnet/roslyn/pull/80433)
// False positive. Suppress it.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                        .WithOptions (
                                      ConfigOption,
                                      LogLevelOption
                                       .WithValueOrderedEnumHelpText ( )
                                     )
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                        .WithCommand
                           (
                            ConfigCommand
                             .WithCommand
                                (
                                 ConfigGlobalCommand
                                  .RequiringOneOrMoreOptionsIn (
// TODO: Remove this suppression once the next .net 10 build is released (fix provided in https://github.com/dotnet/roslyn/pull/80433)
// False positive. Suppress it.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                                                ConfigGlobalCommand_DaemonizeOption
                                                                 .WithCompletionSource ( GetTriStateOptionValueCompletions ),
                                                                ConfigGlobalCommand_DaemonTimerIntervalSecondsOption
                                                                 .WithSuggestedCompletionValues ( 5, 6, 10, 15, 20, 30 ),
                                                                ConfigGlobalCommand_DryRunOption
                                                                 .WithCompletionSource ( GetTriStateOptionValueCompletions ),
                                                                ConfigGlobalCommand_LocalSystemNameOption,
                                                                ConfigGlobalCommand_TakeSnapshotsOption
                                                                 .WithCompletionSource ( GetTriStateOptionValueCompletions ),
                                                                ConfigGlobalCommand_PruneSnapshotsOption
                                                                 .WithCompletionSource ( GetTriStateOptionValueCompletions ),
                                                                ConfigGlobalCommand_ZfsPathOption,
                                                                ConfigGlobalCommand_ZpoolPathOption
                                                               )
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                  .WithOption ( ConfigGlobalCommandOutputFileOption )
                                  .WithAction ( SetGlobalOptions )
                                )
                             .WithCommand
                                (
                                 ConfigConsoleCommand
                                  .WithAction ( StartConfigConsole )
                                )
                           )
                        .WithCommand
                           (
                            RunCommand
// TODO: Remove this suppression once the next .net 10 build is released (fix provided in https://github.com/dotnet/roslyn/pull/80433)
// False positive. Suppress it.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                             .WithOptions (
                                           DaemonizeOption,
                                           MonitorOption,
                                           PruneSnapshotsOption,
                                           TakeSnapshotsOption
                                          )
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                             .WithArgument
                                (
                                 RunCommandModeArgument
                                  .AcceptingOnlyValuesIn ( "once", "service" )
                                )
                             .WithArgument ( PoolsArgument )
                             .WithAction ( RunSiaz )
                           )
                        .WithCommand
                           (
                            CronCommand
// TODO: Remove this suppression once the next .net 10 build is released (fix provided in https://github.com/dotnet/roslyn/pull/80433)
// False positive. Suppress it.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                             .WithOptions (
                                           DaemonizeOption,
                                           MonitorOption,
                                           PruneSnapshotsOption,
                                           TakeSnapshotsOption
                                          )
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                             .WithArgument
                                (
                                 RunCommandModeArgument
                                  .AcceptingOnlyValuesIn ( "once", "service" )
                                )
                             .WithArgument ( PoolsArgument )
                             .WithAction ( RunSiaz )
                           )
                        .WithCommand
                           (
                            TemplatesCommand
                             .WithCommand
                                (
                                 TemplatesCreateCommand
                                  .WithAction ( CommandNotImplemented )
                                )
                             .WithCommand
                                (
                                 TemplatesModifyCommand
                                  .WithAction ( CommandNotImplemented )
                                )
                             .WithCommand
                                (
                                 TemplatesRemoveCommand
                                  .WithAction ( CommandNotImplemented )
                                )
                             .WithCommand
                                (
                                 TemplatesListCommand
                                  .WithAction ( CommandNotImplemented )
                                )
                             .WithCommand
                                (
                                 TemplatesShowCommand
                                  .WithAction ( CommandNotImplemented )
                                )
                           )
                        .WithCommand
                           (
                            ZfsCommand
                             .WithCommand
                                (
                                 ZfsSchemaCommand
                                  .WithCommand
                                     (
                                      ZfsSchemaCheckCommand
                                       .WithArgument ( PoolsArgument )
                                       .WithAction ( ZfsSchemaCheck )
                                     )
                                  .WithCommand
                                     (
                                      ZfsSchemaInitializeCommand
                                       .WithOption ( ZfsSchemaChangeCommands_ConfirmImpactOption )
                                       .WithOption ( ZfsSchemaChangeCommands_ReallyConfirmImpactOption )
                                       .WithArgument ( PoolsArgument )
                                       .WithAction ( ZfsSchemaInitialize )
                                     )
                                  .WithCommand
                                     (
                                      ZfsSchemaCleanCommand
                                       .WithOption ( ZfsSchemaChangeCommands_ConfirmImpactOption )
                                       .WithOption ( ZfsSchemaChangeCommands_ReallyConfirmImpactOption )
                                       .WithArgument ( PoolsArgument )
                                       .WithAction ( ZfsSchemaClean )
                                     )
                                )
                           );
  }

  /// <summary>
  ///   Gets the collection of configuration files to load, after considering any given on the command line.
  /// </summary>
  /// <param name="rootCommandParseResult"></param>
  /// <returns></returns>
  public FileInfo[] GetConfigurationFileCollection ( ParseResult? rootCommandParseResult = null )
  {
    rootCommandParseResult ??= RootCommandParseResult ??= Parse ( Environment.GetCommandLineArgs ( ) );

    if ( rootCommandParseResult.RootCommandResult.GetResult ( ConfigOption ) is not { Option: Option<FileInfo[]> } fileInfoResult )
    {
      rootCommandParseResult.RootCommandResult.AddError ( $"Unexpected input to {ConfigOptionName} option." );
      return [ ];
    }

    if ( fileInfoResult is { Implicit: false, Tokens.Count: < 1 } )
    {
      fileInfoResult.AddError ( $"One or more configuration files must be given to the {ConfigOptionName} option." );
      return [ ];
    }

    FileInfo[] fileCollection = [ ..fileInfoResult.GetValueOrDefault<FileInfo[]> ( ) ];
    _logger.Debug ( "Configuration will be loaded from these files: {0}", string.Join ( Environment.NewLine, fileCollection.Select ( static f => f.FullName ) ) );

    return fileCollection;
  }

  /// <summary>
  ///   Invokes the command line.
  /// </summary>
  /// <param name="outputStream">The stream to use for normal output.</param>
  /// <param name="errorStream">The stream to use for error output.</param>
  /// <remarks>
  ///   This is mainly here so that tests can redirect output to a null stream to reduce verbosity of test output, but may be useful
  ///   for alternate interfaces, such as a web UI.
  /// </remarks>
  /// <returns></returns>
  [PublicAPI]
  [MethodImpl ( MethodImplOptions.AggressiveInlining )]
  public ExitCode Invoke ( TextWriter outputStream, TextWriter errorStream )
  {
    ExitCode exitCode = (ExitCode)( RootCommandParseResult?.Invoke ( new ( ) { Error = errorStream, Output = outputStream } ) ?? -1 );
    ThreadPool.QueueUserWorkItem ( _ => InvokeCompleted?.Invoke ( this, this ) );
    return exitCode;
  }

  public event EventHandler<SiazCommandLine>? InvokeCompleted;

  /// <summary>
  ///   Parses the command line and returns the result.
  /// </summary>
  /// <param name="args">The command line arguments to parse.</param>
  /// <returns>
  ///   The <see cref="ParseResult" /> returned by the call to
  ///   <see cref="Command.Parse(IReadOnlyList{string}, System.CommandLine.ParserConfiguration)" />.
  /// </returns>
  public ParseResult Parse ( IReadOnlyList<string> args )
  {
    return RootCommandParseResult = RootCommand.Parse ( args, ParserConfiguration );
  }

  private static IEnumerable<FileInfo> GetConfigFileListFromEnvironmentOrDefault ( )
  {
    string[]? configurationFiles = Environment.GetEnvironmentVariable ( BaseConfigFilesEnvVarName )?.SplitAndClean ( Path.PathSeparator );

    configurationFiles ??=
    [
      "SnapsInAZfs.json",
      "SnapsInAZfs.local.json",
      "SnapsInAZfs.nlog.json",
      "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json",
      "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json",
      "/etc/SnapsInAZfs/SnapsInAZfs.local.json",
      "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json"
    ];

    return configurationFiles.Select ( StringAsFileInfo );
  }

  [MethodImpl ( MethodImplOptions.AggressiveInlining )]
  private static FileInfo StringAsFileInfo ( string fileName )
  {
    return new ( fileName );
  }
}
