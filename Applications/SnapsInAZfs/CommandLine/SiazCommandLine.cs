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
using System.Runtime.CompilerServices;
using Extensions;
using Interop;

/// <summary>
///     The high-level interface to the command line functionality.
/// </summary>
/// <remarks>
///     <para>
///         A design goal of this type is to make use of the <c>System.CommandLine</c> functionality and then hand the results back
///         to the caller, so that the SCL objects can be short-lived and not stick around for the lifetime of the application.
///     </para>
///     <para>
///         However, SCL does most of its work in delegates provided when the tree is configured, and those delegates are pretty
///         inflexible with regard to injection of external data.
///     </para>
///     <para>
///         Therefore, to avoid excessive use of static objects, this type is essentially a wrapper for SCL that provides durable
///         references to common items (to avoid re-creating them, e.g., in validators) and to objects that may be configured in
///         multiple passes or that may be modified by or relevant to more than one delegate, especially for use in subsequent
///         execution.<br />
///         CommandName examples are the <see cref="SnapsInAZfsSettings" /> and <see cref="IConfigurationRoot" /> instances which are built
///         from a combination of command line input and other configuration sources.
///     </para>
///     <para>
///         In order to avoid SCL essentially owning the whole program, this class is used to set up the CLI, use it for parsing and
///         validation, and then get out of the way and let the program run as it did prior to the introduction of SCL.
///     </para>
///     <para>
///         Note that this type has multiple parts in files named SiazCommandLine.[Category].cs.
///     </para>
/// </remarks>
[PublicAPI]
public sealed partial class SiazCommandLine
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public SiazCommandLine ( )
    {
        ConfigureCommandLineTree( );
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SiazCommandLine" /> using the provided <paramref name="rootCommand" /> instead of the
    ///     default implementation.
    /// </summary>
    /// <param name="rootCommand">
    ///     An alternative <see cref="System.CommandLine.RootCommand" /> to use instead of the default implementation.
    /// </param>
    /// <remarks>
    ///     This constructor WILL NOT create the default command line tree and will only have whatever is defined by
    ///     <paramref name="rootCommand" />.
    /// </remarks>
    public SiazCommandLine( RootCommand rootCommand )
    {
        RootCommand = rootCommand;
    }

    private IConfigurationRoot? _configurationRoot;

    private SnapsInAZfsSettings? _settings;

    public RootCommand? RootCommand { get; private set; }

    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public ExitCode Invoke(
        IReadOnlyList<string>    args,
        out RootCommand          rootCommand,
        out ParseResult          rootCommandParseResult,
        out SnapsInAZfsSettings? siazSettings,
        out IConfigurationRoot?  configurationRoot,
        ParserConfiguration?     parserConfiguration     = null,
        InvocationConfiguration? invocationConfiguration = null
    )
    {
        _settings = new ( );

        rootCommandParseResult = Parse ( args, out rootCommand, parserConfiguration );
        int invokeResult = rootCommandParseResult.Invoke ( invocationConfiguration );
        siazSettings      = _settings;
        configurationRoot = _configurationRoot;

        return (ExitCode)invokeResult;
    }

    /// <summary>
    ///     Builds the command line parser configuration.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <c>SnapsInAZfs(8)</c> and <c>SnapsInAZfs(5)</c> for operation and configuration details.
    ///     </para>
    /// </remarks>
    private RootCommand ConfigureCommandLineTree ( )
    {
        return RootCommand = new RootCommand ( "SnapsInAZfs" )
                            .WithOption ( TakeSnapshotsOption )
                            .WithOption ( PruneSnapshotsOption )
                            .WithOption ( ConfigOption.WithValidator ( ValidateFileExistsAndIsWriteable ) )
                            .WithOption ( DebugOption )
                            .WithOption ( DaemonizeOption )
                            .WithCommand
                                 (
                                  new Command (
                                               ConfigCommandName,
                                               "Perform configuration operations on SIAZ and managed pools/datasets directly or via the configuration console."
                                              )
                                     .WithCommand
                                          (
                                           new Command (
                                                        ConfigGlobalCommandName,
                                                        """
                                                        Modify global settings in the root of the JSON configuration files.
                                                        If no --output-file option is specified, resulting changes will be written to the last configuration file loaded, including any specified on the command line.
                                                        """
                                                       )
                                              .WithOption ( ConfigGlobalCommandOutputFileOption )
                                              .WithCommand
                                                   (
                                                    new Command (
                                                                 ConfigGlobalDryRunCommandName,
                                                                 "Set the DryRun option, which controls whether SIAZ can make changes (false) or not (true)."
                                                                )
                                                        {
                                                            TreatUnmatchedTokensAsErrors = true
                                                        }
                                                       .WithArgument ( ConfigStateArgument )
                                                       .WithAction ( SetGlobalOption )
                                                   )
                                          )
                                     .With
                                          (
                                           new Command (
                                                        ConfigConsoleCommandName,
                                                        "Launches the configuration console TUI."
                                                       )
                                              .WithAction ( StartConfigConsole )
                                          )
                                 )
                            .With
                                 (
                                  new Command (
                                               RunCommandName,
                                               $"Run SIAZ, optionally specifying override options.{Environment.NewLine}Use this context when executing one-off operations or for custom service/script-based invocations."
                                              )
                                      // The --cron alias is for backward compatibility with the sanoid-compatible CLI only.
                                     .WithAlias ( "--cron" )
                                     .WithAction ( RunSiaz )
                                 )
                            .With
                                 (
                                  new Command (
                                               ZfsCommandName,
                                               "Perform operations on ZFS pools and datasets managed by SIAZ."
                                              )
                                     .With
                                          (
                                           new Command (
                                                        ZfsSchemaCommandName,
                                                        "Perform operations on properties of ZFS pools and datasets used by SIAZ."
                                                       )
                                              .With
                                                   (
                                                    new Command (
                                                                 ZfsSchemaCheckCommandName,
                                                                 "Checks the property schema for SnapsInAZfs in ZFS and reports any missing properties for pool roots. Checks all pools by default."
                                                                )
                                                       .WithAction ( ZfsSchemaCheck )
                                                       .WithArgument ( PoolsArgument )
                                                   )
                                              .With
                                                   (
                                                    new Command (
                                                                 ZfsSchemaInitializeCommandName,
                                                                 "Updates the property schema for SnapsInAZfs in ZFS, using default values. Will not overwrite StandardBooleanOptions that are already set."
                                                                )
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
                                                    new Command (
                                                                 ZfsSchemaCleanCommandName,
                                                                 "Completely removes all pool and dataset properties that came from SIAZ."
                                                                )
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
