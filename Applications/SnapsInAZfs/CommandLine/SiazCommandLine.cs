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
  private static readonly Logger Logger = LogManager.GetCurrentClassLogger ( );

  /// <summary>
  ///   Creates a new instance of <see cref="SiazCommandLine" /> and initializes its structure.
  /// </summary>
  public SiazCommandLine ( )
  {
    ConfigureCommandLineTree ( );
  }

  private IConfigurationRoot? _configurationRoot;

  private ParseResult? _rootCommandParseResult;

  private SnapsInAZfsSettings? _settings;

  /// <summary>
  ///   Parses the command line, updates the internal settings and configuration references, and invokes the System.CommandLine
  ///   functionality based on the input.
  /// </summary>
  /// <param name="rootCommand"></param>
  /// <param name="rootCommandParseResult"></param>
  /// <param name="siazSettings"></param>
  /// <param name="configurationRoot"></param>
  /// <param name="args">
  ///   If not <see langword="null" />, specifies an explicit collection of command line arguments to parse, of which the first is
  ///   interpreted as the executable name.<br />
  ///   Otherwise, the result of <see cref="Environment.GetCommandLineArgs" /> will be used if this parameter is not provided or is
  ///   explicitly <see langword="null" />.
  /// </param>
  /// <param name="invocationConfiguration"></param>
  /// <returns></returns>
  [PublicAPI]
  [MethodImpl ( MethodImplOptions.AggressiveInlining )]
  public ExitCode Invoke (
    out RootCommand          rootCommand,
    out ParseResult          rootCommandParseResult,
    out SnapsInAZfsSettings? siazSettings,
    out IConfigurationRoot?  configurationRoot,
    IReadOnlyList<string>?   args                    = null,
    InvocationConfiguration? invocationConfiguration = null
  )
  {
    RootCommand cmd = RootCommand;

    if ( _rootCommandParseResult is null )
    {
      _settings ??= new ( );

      args ??= Environment.GetCommandLineArgs ( );

      _rootCommandParseResult = Parse ( args, out cmd );
    }

    rootCommand            = cmd;
    rootCommandParseResult = _rootCommandParseResult;
    int invokeResult = _rootCommandParseResult.Invoke ( invocationConfiguration );
    siazSettings      = _settings;
    configurationRoot = _configurationRoot;

    return (ExitCode)invokeResult;
  }

  /// <summary>
  ///   Builds the command line parser configuration.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     See <c>SnapsInAZfs(8)</c> and <c>SnapsInAZfs(5)</c> for operation and configuration details.
  ///   </para>
  /// </remarks>
  [MemberNotNull ( nameof (RootCommand) )]
  internal RootCommand ConfigureCommandLineTree ( )
  {
    return RootCommand = new RootCommand (
                                          $"""
                                           SnapsInAZfs - A snapshot management system for OpenZFS.

                                           Per POSIX standards, all commands, options, arguments, and values are case-sensitive.
                                           {F.FGYELLOW}If alternative case or other forms for a token are allowed, they will appear in the usage text below.{F._FGCOLOR}
                                           """
                                         )
                        .WithOptions (
                                      ConfigOption,
                                      LogLevelOption
                                     )
                        .WithCommand
                           (
                            _configCommand
                             .WithCommand
                                (
                                 _configGlobalCommand
                                  .RequiringOneOrMoreOptionsIn (
// TODO: Remove this suppression once the next .net 10 build is released (fix provided in https://github.com/dotnet/roslyn/pull/80433)
// False positive. Suppress it.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                                                _configGlobalCommand_DaemonizeOption,
                                                                _configGlobalCommand_DaemonTimerIntervalSecondsOption,
                                                                _configGlobalCommand_DryRunOption,
                                                                _configGlobalCommand_LocalSystemNameOption,
                                                                _configGlobalCommand_TakeSnapshotsOption,
                                                                _configGlobalCommand_PruneSnapshotsOption,
                                                                _configGlobalCommand_ZfsPathOption,
                                                                _configGlobalCommand_ZpoolPathOption
                                                               )
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                                  .WithOption ( ConfigGlobalCommandOutputFileOption )
                                  .WithAction ( SetGlobalOptions )
                                )
                             .WithCommand
                                (
                                 _configConsoleCommand
                                  .WithAction ( StartConfigConsole )
                                )
                           )
                        .WithCommand
                           (
                            RunCommand
                              // The --cron alias is for backward compatibility with the sanoid-compatible CLI only.
                             .WithAlias ( "--cron" )
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
                             .WithAction ( RunSiaz )
                           )
                        .WithCommand
                           (
                            _zfsCommand
                             .WithCommand
                                (
                                 _zfsSchemaCommand
                                  .WithCommand
                                     (
                                      _zfsSchemaCheckCommand
                                       .WithAction ( ZfsSchemaCheck )
                                       .WithArgument ( PoolsArgument )
                                     )
                                  .WithCommand
                                     (
                                      _zfsSchemaInitializeCommand
                                       .WithAction ( ZfsSchemaInitialize )
                                       .WithArgument ( _zfsSchemaInitializeCommand_PoolsArgument )
                                     )
                                  .WithCommand
                                     (
                                      _zfsSchemaCleanCommand
                                       .WithAction ( ZfsSchemaClean )
                                       .WithArgument ( _zfsSchemaCleanCommand_ConfigArgument )
                                       .WithArgument ( _zfsSchemaCleanCommand_ConfirmImpactArgument )
                                     )
                                )
                           );
  }

  private FileInfo[] GetConfigurationFileCollection ( ParseResult rootCommandParseResult )
  {
    if ( rootCommandParseResult.RootCommandResult.GetResult ( ConfigOption ) is not { Option: Option<FileInfo[]>, Tokens.Count: > 1 } configOptionResult )
    {
      return [ ];
    }

    FileInfo[] fileCollection = [ ..configOptionResult.GetValueOrDefault<FileInfo[]> ( ) ];
    Logger.Debug ( "Configuration will be loaded from these files: {0}", string.Join ( Environment.NewLine, fileCollection.Select ( static f => f.FullName ) ) );

    return fileCollection;
  }
}
