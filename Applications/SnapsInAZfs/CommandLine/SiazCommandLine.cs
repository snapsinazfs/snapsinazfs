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

using System.Buffers;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using ConfigConsole;
using Extensions;
using Interop;
using Interop.Zfs.ZfsTypes;
using NLog.Config;
using NLog.Extensions.Logging;

/// <summary>
///     The high-level interface to the command line functionality.
/// </summary>
public static class SiazCommandLine
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private static readonly string[] StandardBooleanFalseStrings =
    [
        "0",
        bool.FalseString,
        CultureInfo.CurrentUICulture.TextInfo.ToLower ( bool.FalseString ),
        "disable",
        "disabled",
        "no",
        "off"
    ];

    private static readonly string[] StandardBooleanTrueStrings =
    [
        "1",
        bool.TrueString,
        CultureInfo.CurrentUICulture.TextInfo.ToLower ( bool.TrueString ),
        "enable",
        "enabled",
        "yes",
        "on"
    ];

    private static readonly string[] StandardBooleanFormsSet =
    [
        ..StandardBooleanTrueStrings,
        ..StandardBooleanFalseStrings
    ];

    private static readonly SearchValues<string> StandardBooleanTrueValuesSearch = SearchValues.Create ( StandardBooleanTrueStrings.AsSpan( ), StringComparison.OrdinalIgnoreCase );

    /// <summary>
    ///     Locally-significant reference to a <see cref="SnapsInAZfsSettings" /> instance, for use across method calls.
    /// </summary>
    private static SnapsInAZfsSettings? _settings;

    /// <summary>
    ///     Builds and parses the command line.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is organized hierarchically using extension methods defined for the <see cref="System" />.
    ///         <see cref="System.CommandLine" /> types, so the code forms a tree that matches the CLI layout.
    ///     </para>
    ///     <para>
    ///         Configuration files should be loaded after parsing the command line, regardless of order of arguments, or else
    ///         configuration parsing will clobber the results of the command line.<br />
    ///         If no configuration file path override is specified, the default locations will be searched and applied as specified in
    ///         the documentation, and then results of the command line should be applied as appropriate in context.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public static RootCommand ConfigureCommandLineTree ( )
    {
        return
            new RootCommand ( "SnapsInAZfs" )
               .WithOption<bool>
                    (
                     new ( "--take-snapshots" )
                     {
                         Arity       = ArgumentArity.ZeroOrOne,
                         Description = $"(DEPRECATED) Enables new snapshot processing. If dry-run is enabled, reports snapshots that would be taken but does not perform the snapshot operations.{Environment.NewLine}This option is deprecated in this context. Use in the `siaz run` context instead.",
                         Required    = false
                     }
                    )
               .WithOption<bool>
                    (
                     new ( "--prune-snapshots" )
                     {
                         Arity       = ArgumentArity.ZeroOrOne,
                         Description = $"(DEPRECATED) Enables expired snapshot pruning. If dry-run is enabled, reports snapshots that would be destroyed but does not perform the destroy operations.{Environment.NewLine}This option is deprecated in this context. Use in the `siaz run` context instead.",
                         Required    = false
                     }
                    )
               .WithOption<string[]>
                    (
                     new ( "--config", "--config-file", "--config-files" )
                     {
                         Arity = ArgumentArity.OneOrMore,
                         Description = """
                                       One or more configuration files to REPLACE the default configuration files, for this invocation.
                                       Configuration files at the standard paths will be ignored unless included in your list.
                                       To add additional layers of configuration files on top of the default configuration files, see the --additional-config-file option.
                                       See SnapsInAZfs.json(5) for details about using the --config and --additional-config options together.
                                       """,
                         Recursive           = true,
                         DefaultValueFactory = static _ => [ "/usr/local/share/SnapsInAZfs/SnapsInAZfs.json", "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json", "/etc/SnapsInAZfs/SnapsInAZfs.local.json", "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", "SnapsInAZfs.json", "SnapsInAZfs.local.json", "SnapsInAZfs.nlog.json" ]
                     }
                    )
               .WithOption<bool>
                    (
                     new ( "--debug" )
                     {
                         Arity = ArgumentArity.ZeroOrOne,
                         Description = """
                                       Debug level output logging.
                                       Change log level in SnapsInAZfs.nlog.json for normal usage.
                                       """,
                         Recursive = true
                     }
                    )
               .WithOption<bool>
                    (
                     new ( "--daemonize", "-D" )
                     {
                         Arity       = ArgumentArity.ZeroOrOne,
                         Description = "Run SnapsInAZfs as a daemon.",
                         Required    = false
                     }
                    )
               .WithCommand
                    (
                     new Command (
                                  "config",
                                  "Perform configuration operations on SIAZ and managed pools/datasets directly or via the configuration console."
                                 )
                        .WithCommand
                             (
                              new Command (
                                           "global",
                                           """
                                           Modify global settings in the root of the JSON configuration files.
                                           If no --output-file option is specified, resulting changes will be written to the last configuration file loaded, including any specified on the command line.
                                           """
                                          )
                                 .WithOption<string> (
                                                      new Option<string> ( "--output-file" )
                                                      {
                                                          Description = """
                                                                        Absolute or relative path to the file to which changes will be written.
                                                                        If the file already exists, it must be a JSON text file.
                                                                        The JSON node at the path corresponding to the modified setting will be REPLACED by this operation.
                                                                        If the file does not exist, a new JSON file will be created containing only the modified setting.
                                                                        """,
                                                          Recursive = true,
                                                          Arity     = ArgumentArity.ZeroOrOne
                                                      }
                                                     )
                                 .WithCommand
                                      (
                                       new Command (
                                                    "dry-run",
                                                    "Set the DryRun option, which controls whether SIAZ can make changes (false) or not (true)."
                                                   )
                                           {
                                               TreatUnmatchedTokensAsErrors = true
                                           }
                                          .WithArgument
                                               (
                                                new Argument<TriStateOptionValue> ( "state" )
                                                    {
                                                        Arity       = ArgumentArity.ExactlyOne,
                                                        Description = "Set to true to set SIAZ to dry run mode (no changes made), false to disable dry run mode (normal operation). Default: false"
                                                    }
                                                   .WithCustomParser ( TriStateArgumentValuesParser )
                                                   .AcceptingOnlyValuesIn ( [ ..StandardBooleanFormsSet, "default" ] )
                                               )
                                          .WithAction ( SetGlobalOption )
                                      )
                             )
                        .With
                             (
                              new Command (
                                           "console",
                                           "Launches the configuration console TUI."
                                          )
                                 .WithAction ( StartConfigConsole )
                             )
                    )
               .With
                    (
                     new Command (
                                  "run",
                                  $"Run SIAZ, optionally specifying override options.{Environment.NewLine}Use this context when executing one-off operations or for custom service/script-based invocations."
                                 )
                         // The --cron alias is for backward compatibility with the sanoid-compatible CLI only.
                        .WithAlias ( "--cron" )
                        .WithAction ( RunSiaz )
                    )
               .With
                    (
                     new Command (
                                  "zfs",
                                  "Perform operations on ZFS pools and datasets managed by SIAZ."
                                 )
                        .With
                             (
                              new Command (
                                           "schema",
                                           "Perform operations on properties of ZFS pools and datasets used by SIAZ."
                                          )
                                 .With
                                      (
                                       new Command (
                                                    "check",
                                                    "Checks the property schema for SnapsInAZfs in ZFS and reports any missing properties for pool roots. Checks all pools by default."
                                                   )
                                          .WithAction ( ZfsSchemaCheck )
                                          .WithArgument<string[]>
                                               (
                                                // IDEA: It would be cool and user-friendly to have this call zpool-list during tab-completion to suggest pool names.
                                                new
                                                ( "pools" )
                                                {
                                                    Arity               = ArgumentArity.ZeroOrMore,
                                                    Description         = "If specified, limits the check to the named pools.",
                                                    DefaultValueFactory = static _ => [ ]
                                                }
                                               )
                                      )
                                 .With
                                      (
                                       new Command (
                                                    "initialize",
                                                    "Updates the property schema for SnapsInAZfs in ZFS, using default values. Will not overwrite StandardBooleanOptions that are already set."
                                                   )
                                          .WithAction ( ZfsSchemaInitialize )
                                          .WithArgument<string[]>
                                               (
                                                new ( "pools" )
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
                                                    "clean",
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

    /// <inheritdoc cref="ParseResult.Invoke(InvocationConfiguration?)" />
    /// <param name="args">The string arguments to parse.</param>
    /// <param name="parserConfiguration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <param name="invocationConfiguration">The configuration used to define invocation behaviors.</param>
    /// <remarks>
    ///     This method first calls <see cref="Parse(IReadOnlyList{string}, ParserConfiguration?)" /> and then calls
    ///     <see cref="ParseResult.Invoke(InvocationConfiguration?)" /> on
    ///     the resulting <see cref="ParseResult" />, passing the provided arguments to each method.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static ExitCode Invoke( IReadOnlyList<string> args, ParserConfiguration? parserConfiguration = null, InvocationConfiguration? invocationConfiguration = null )
    {
        return (ExitCode)Parse ( args, parserConfiguration ).Invoke ( invocationConfiguration );
    }

    /// <inheritdoc cref="ParseResult.Invoke(InvocationConfiguration?)" />
    /// <param name="args">The string arguments to parse.</param>
    /// <param name="parserConfiguration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <param name="invocationConfiguration">The configuration used to define invocation behaviors.</param>
    /// <param name="rootCommand">
    ///     Provides an <see langword="out" /> reference to the <see cref="RootCommand" /> that was created and parsed.
    /// </param>
    /// <remarks>
    ///     This method first calls <see cref="Parse(IReadOnlyList{string}, out RootCommand, ParserConfiguration?)" /> and then calls
    ///     <see cref="ParseResult.Invoke(InvocationConfiguration?)" /> on
    ///     the resulting <see cref="ParseResult" />, passing the provided arguments to each method.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static int Invoke( IReadOnlyList<string> args, out RootCommand rootCommand, ParserConfiguration? parserConfiguration = null, InvocationConfiguration? invocationConfiguration = null )
    {
        return Parse ( args, out rootCommand, parserConfiguration ).Invoke ( invocationConfiguration );
    }

    /// <inheritdoc cref="ParseResult.Invoke(InvocationConfiguration?)" />
    /// <param name="args">The string arguments to parse.</param>
    /// <param name="rootCommand">
    ///     Provides an <see langword="out" /> reference to the <see cref="RootCommand" /> returned by the call to
    ///     <see
    ///         cref="Parse(System.Collections.Generic.IReadOnlyList{string},out System.CommandLine.RootCommand,System.CommandLine.ParserConfiguration?)" />
    ///     .
    /// </param>
    /// <param name="rootCommandParseResult">
    ///     Provides an <see langword="out" /> reference to the <see cref="ParseResult" /> returned by the call to
    ///     <see cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" />
    /// </param>
    /// <param name="parserConfiguration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <param name="invocationConfiguration">The configuration used to define invocation behaviors.</param>
    /// <remarks>
    ///     This method first calls <see cref="Parse(IReadOnlyList{string}, out RootCommand, ParserConfiguration?)" /> and then calls
    ///     <see cref="ParseResult.Invoke(InvocationConfiguration?)" /> on
    ///     the resulting <see cref="ParseResult" />, passing the provided arguments to each method.<br />
    ///     This overload also produces direct references to the <see cref="RootCommand" /> and <see cref="ParseResult" /> created in the
    ///     process. Generally, you should only call the
    ///     <see
    ///         cref="Invoke(System.Collections.Generic.IReadOnlyList{string},System.CommandLine.ParserConfiguration?,System.CommandLine.InvocationConfiguration?)" />
    ///     method.
    /// </remarks>
    /// <returns>
    ///     The return value from the call to <see cref="ParseResult.Invoke(InvocationConfiguration?)" />.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static int Invoke( IReadOnlyList<string> args, out RootCommand rootCommand, out ParseResult rootCommandParseResult, ParserConfiguration? parserConfiguration = null, InvocationConfiguration? invocationConfiguration = null )
    {
        rootCommandParseResult = Parse ( args, out rootCommand, parserConfiguration );

        return rootCommandParseResult.Invoke ( invocationConfiguration );
    }

    /// <inheritdoc cref="ParseResult.Invoke(InvocationConfiguration?)" />
    /// <param name="args">The string arguments to parse.</param>
    /// <param name="rootCommand">
    ///     Provides an <see langword="out" /> reference to the <see cref="RootCommand" /> returned by the call to
    ///     <see
    ///         cref="Parse(System.Collections.Generic.IReadOnlyList{string},out System.CommandLine.RootCommand,System.CommandLine.ParserConfiguration?)" />
    ///     .
    /// </param>
    /// <param name="rootCommandParseResult">
    ///     Provides an <see langword="out" /> reference to the <see cref="ParseResult" /> returned by the call to
    ///     <see cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" />
    /// </param>
    /// <param name="siazSettings">
    ///     <para>
    ///         An <see langword="out" /> reference to a *new* instance of <see cref="SnapsInAZfsSettings" /> which will have any
    ///         applicable
    ///         overrides from command line elements applied to it.
    ///     </para>
    ///     <para>
    ///         This reference will always be assigned to a *new* instance of <see cref="SnapsInAZfsSettings" />, effectively ignoring
    ///         any pre-initialized references passed to this method.
    ///     </para>
    ///     <para>
    ///         You can declare but should not initialize this parameter in-line in the method call (e.g. do not call
    ///         <c>Invoke(..., out SnapsInAZfsSettings siazSettings = new(), ...)</c>, because the original reference will be lost.
    ///     </para>
    ///     <para>
    ///         The static <see cref="Program.Settings" /> is generally the most logical reference to provide, unless you're creating
    ///         some sort of modal interactive CLI or something like that.
    ///     </para>
    /// </param>
    /// <param name="parserConfiguration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <param name="invocationConfiguration">The configuration used to define invocation behaviors.</param>
    /// <remarks>
    ///     This method first calls <see cref="Parse(IReadOnlyList{string}, out RootCommand, ParserConfiguration?)" /> and then calls
    ///     <see cref="ParseResult.Invoke(InvocationConfiguration?)" /> on
    ///     the resulting <see cref="ParseResult" />, passing the provided arguments to each method.<br />
    ///     This overload also produces direct references to the <see cref="RootCommand" /> and <see cref="ParseResult" /> created in the
    ///     process. Generally, you should only call the
    ///     <see
    ///         cref="Invoke(System.Collections.Generic.IReadOnlyList{string},System.CommandLine.ParserConfiguration?,System.CommandLine.InvocationConfiguration?)" />
    ///     method.
    /// </remarks>
    /// <returns>
    ///     The return value from the call to <see cref="ParseResult.Invoke(InvocationConfiguration?)" />.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static int Invoke( IReadOnlyList<string> args, out RootCommand rootCommand, out ParseResult rootCommandParseResult, out SnapsInAZfsSettings siazSettings, ParserConfiguration? parserConfiguration = null, InvocationConfiguration? invocationConfiguration = null )
    {
        _settings = new ( );

        rootCommandParseResult = Parse ( args, out rootCommand, parserConfiguration );
        int invokeResult = rootCommandParseResult.Invoke ( invocationConfiguration );
        siazSettings = _settings;

        return invokeResult;
    }

    /// <inheritdoc cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" />
    /// <remarks>
    ///     This method first calls <see cref="ConfigureCommandLineTree" /> and then calls
    ///     <see cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" /> on the resulting <see cref="RootCommand" />, using
    ///     the provided arguments.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static ParseResult Parse( IReadOnlyList<string> args, ParserConfiguration? configuration = null )
    {
        return ConfigureCommandLineTree( ).Parse ( args, configuration );
    }

    /// <inheritdoc cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" />
    /// <param name="args">The string arguments to parse.</param>
    /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <param name="rootCommand">
    ///     Provides an <see langword="out" /> reference to the <see cref="RootCommand" /> that was created and parsed.
    /// </param>
    /// <remarks>
    ///     This method first calls <see cref="ConfigureCommandLineTree" /> and then calls
    ///     <see cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" /> on the resulting <see cref="RootCommand" />, using
    ///     the provided arguments.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static ParseResult Parse( IReadOnlyList<string> args, out RootCommand rootCommand, ParserConfiguration? configuration = null )
    {
        rootCommand = ConfigureCommandLineTree( );

        return rootCommand.Parse ( args, configuration );
    }

    /// <summary>
    ///     Parses a boolean from more inputs than <see cref="bool" /> is aware of, taken from the first token of an
    ///     <see cref="ArgumentResult" />.
    /// </summary>
    /// <param name="argumentResult">The argument to parse the token from.</param>
    /// <returns>
    ///     If the token matches any of the values in <see cref="StandardBooleanTrueValuesSearch" />, returns <see langword="true" />.
    ///     <br />
    ///     Returns <see langword="false" /> for all other values.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool ArgumentStandardBooleanValuesParser( ArgumentResult argumentResult )
    {
        return StandardBooleanTrueValuesSearch.Contains ( argumentResult.Tokens [ 0 ].Value );
    }

    private static Task<int> RunSiaz( ParseResult parseResult, CancellationToken cancellation )
    {
        Console.WriteLine ( $"Running siaz with command line {string.Join ( ' ', parseResult.Tokens )}." );
        Console.WriteLine ( "Not yet implemented." );

        return Task.FromResult ( 0 );
    }

    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static int SetDryRun( ParseResult parseResult, TriStateOptionValue dryRun )
    {
        Console.WriteLine ( $"Setting DryRun to {dryRun}." );
        Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

        return 0;
    }

    private static int SetGlobalOption( ParseResult parseResult )
    {
        TriStateOptionValue dryRun = parseResult.GetValue<TriStateOptionValue> ( "state" );

        return SetDryRun ( parseResult, dryRun );
    }

    private static void StartConfigConsole( ParseResult parseResult )
    {
        Console.WriteLine ( parseResult.CommandResult.ToString( ) );
        Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );
    }

    /// <summary>
    ///     Parses a boolean from more inputs than <see cref="bool" /> is aware of, taken from the first token of an
    ///     <see cref="ArgumentResult" />.
    /// </summary>
    /// <param name="argumentResult">The argument to parse the token from.</param>
    /// <returns>
    ///     If the token matches any of the values in <see cref="StandardBooleanTrueValuesSearch" />, returns <see langword="true" />.
    ///     <br />
    ///     Returns <see langword="false" /> for all other values.
    /// </returns>
    private static TriStateOptionValue TriStateArgumentValuesParser( ArgumentResult argumentResult )
    {
        return argumentResult.Tokens [ 0 ].Value is "default" or ""
                   ? TriStateOptionValue.Default
                   : StandardBooleanTrueValuesSearch.Contains ( argumentResult.Tokens [ 0 ].Value )
                       ? TriStateOptionValue.True
                       : TriStateOptionValue.False;
        //return  switch
        //       {
        //           "1"       => true,
        //           "true"    => true,
        //           "True"    => true,
        //           "enable"  => true,
        //           "enabled" => true,
        //           _         => false
        //       };
    }

    private static int ZfsSchemaCheck( ParseResult parseResult )
    {
        Console.WriteLine ( parseResult.CommandResult.ToString( ) );
        Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

        return 0;
    }

    private static int ZfsSchemaClean( ParseResult arg )
    {
        Console.WriteLine ( "Cleaning SIAZ schema from ZFS" );

        return 0;
    }

    private static int ZfsSchemaInitialize( ParseResult parseResult )
    {
        Console.WriteLine ( parseResult.CommandResult.ToString( ) );
        Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

        return 0;
    }
}
