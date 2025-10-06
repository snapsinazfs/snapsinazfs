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

/// <summary>
///   The high-level interface to the command line functionality.
/// </summary>
/// <remarks>
///   <para>
///     Note that this type has multiple parts in files named SiazCommandLine.[Category].cs.
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
  /// <param name="parserConfiguration"></param>
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
    ParserConfiguration?     parserConfiguration     = null,
    InvocationConfiguration? invocationConfiguration = null
  )
  {
    _settings = new ( );

    args ??= Environment.GetCommandLineArgs ( );

    rootCommandParseResult = Parse ( args, out rootCommand, parserConfiguration );
    int invokeResult = rootCommandParseResult.Invoke ( invocationConfiguration );
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
  private RootCommand ConfigureCommandLineTree ( )
  {
    return RootCommand = new RootCommand ( "SnapsInAZfs" )
                        .WithOption ( AdditionalConfigOption )
                        .WithOption ( ConfigOption )
                        .WithOption ( DaemonizeOption )
                        .WithOption ( DaemonTimerIntervalOption )
                        .WithOption ( LogLevelOption )
                        .WithOption ( MonitorOption )
                        .WithOption ( PruneSnapshotsOption )
                        .WithOption ( TakeSnapshotsOption )
                        .WithCommand
                           (
                            _configCommand
                             .WithCommand
                                (
                                 _configGlobalCommand
                                  .WithOption ( ConfigGlobalCommandOutputFileOption )
                                  .WithCommand
                                     (
                                      _configGlobalDryRunCommand
                                       .WithArgument ( ConfigStateArgument )
                                       .WithAction ( SetGlobalOption )
                                     )
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
                             .WithAction ( RunSiaz )
                           )
                        .WithCommand
                           (
                            _zfsCommand
                             .With
                                (
                                 _zfsSchemaCommand
                                  .With
                                     (
                                      _zfsSchemaCheckCommand
                                       .WithAction ( ZfsSchemaCheck )
                                       .WithArgument ( PoolsArgument )
                                     )
                                  .With
                                     (
                                      _zfsSchemaInitializeCommand
                                       .WithAction ( ZfsSchemaInitialize )
                                       .WithArgument<string[]>
                                          (
                                           new ( PoolsArgumentName )
                                           {
                                             Arity               = ArgumentArity.ZeroOrMore,
                                             Description         = "If specified, limits the initialization of the schema to the named pools.",
                                             DefaultValueFactory = static _ => [ ]
                                           }
                                          )
                                     )
                                  .With
                                     (
                                      _zfsSchemaCleanCommand
                                       .WithAction ( ZfsSchemaClean )
                                       .WithArgument<bool>
                                          (
                                           new ( "--confirm" )
                                           {
                                             Arity = ArgumentArity.ExactlyOne
                                           }
                                          )
                                       .WithArgument<bool>
                                          (
                                           new ( "--i-understand-this-cannot-be-undone" )
                                           {
                                             Arity = ArgumentArity.ExactlyOne
                                           }
                                          )
                                     )
                                )
                           );
  }
}
