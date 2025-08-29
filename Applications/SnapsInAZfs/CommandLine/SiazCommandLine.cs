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
using System.Globalization;
using Extensions;

#pragma warning disable CS1591
/// <summary>
/// </summary>
public static class SiazCommandLine
{
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
    ///     Builds and parses the command line.
    /// </summary>
    /// <param name="args">The raw arguments array to parse.</param>
    /// <remarks>
    ///     <para>
    ///         This method is organized hierarchically using extension methods defined for the System.CommandLine types, so the code
    ///         forms a tree that matches the CLI layout.
    ///     </para>
    ///     <para>
    ///         Configuration files are loaded first, regardless of order of arguments.<br />
    ///         If no configuration file path override is specified, the default locations will be searched and applied as
    ///         specified in the documentation.
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
               .WithCommand
                    (
                     new Command (
                                  "config",
                                  "Perform configuration operations on SIAZ and managed pools/datasets directly or via the configuration console."
                                 )
                        .With
                             (
                              new Command (
                                           "global",
                                           "Modify global settings in the root of the JSON configuration files."
                                          )
                                 .With
                                      (
                                       new Command (
                                                    "dry-run",
                                                    "Set the DryRun option, which controls whether SIAZ can make changes (false) or not (true)."
                                                   )
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
                                          .WithAction ( SetDryRun )
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
    /// <remarks>
    ///     This method first calls <see cref="Parse" /> and then calls <see cref="ParseResult.Invoke(InvocationConfiguration?)" /> on
    ///     the resulting <see cref="ParseResult" />, passing the provided arguments to each method.
    /// </remarks>
    public static int Invoke( IReadOnlyList<string> arguments, ParserConfiguration? parserConfiguration = null, InvocationConfiguration? invocationConfiguration = null )
    {
        return Parse ( arguments, parserConfiguration ).Invoke ( invocationConfiguration );
    }

    /// <inheritdoc cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" />
    /// <remarks>
    ///     This method first calls <see cref="ConfigureCommandLineTree" /> and then calls
    ///     <see cref="Command.Parse(IReadOnlyList{string}, ParserConfiguration?)" /> on the resulting <see cref="RootCommand" />, using
    ///     the provided arguments.
    /// </remarks>
    public static ParseResult Parse( IReadOnlyList<string> arguments, ParserConfiguration? parserConfiguration = null )
    {
        return ConfigureCommandLineTree( ).Parse ( arguments, parserConfiguration );
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

    private static void SetDryRun( ParseResult parseResult )
    {
        TriStateOptionValue dryRun = parseResult.GetValue<TriStateOptionValue> ( "state" );
        SetDryRun ( parseResult, dryRun );
    }

    private static int SetDryRun( ParseResult parseResult, TriStateOptionValue dryRun )
    {
        Console.WriteLine ( $"Setting DryRun to {dryRun}." );
        Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

        return 0;
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

    public sealed record CommandLineCommands
    {
        public CommandLineCommands ( )
        {
            Version = new ( "version", "Outputs SnapsInAZfs version to configured logging targets and exits, making no changes." );
            Version.Aliases.Add ( "-V" );
            Version.Aliases.Add ( "--version" );

            Siaz = new ( "SnapsInAZfs" );
        }

        public RootCommand Siaz    { get; set; }
        public Command     Version { get; set; }
    }

    public sealed record CommandLineOptions
    {
        public Option<bool> Verbose { get; set; } = new ( "--verbose", "-v" )
                                                    {
                                                        Description = "Verbose (Info level) output logging. Change log level in SnapsInAZfs.nlog.json for normal usage."
                                                    };
    }
}
