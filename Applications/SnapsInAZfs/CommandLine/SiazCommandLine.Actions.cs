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
using System.Globalization;
using Interop;
using Interop.Zfs.ZfsTypes;

public partial class SiazCommandLine
{
  /// <summary>
  ///   Event raised when the global configuration is changed from the command line.
  /// </summary>
  public event EventHandler<GlobalConfigChangeEventArgs>? GlobalConfigurationChangeRequested;

  /// <summary>
  ///   Event raised when the action for <see cref="RunCommand" /> is invoked by System.CommandLine.
  /// </summary>
  public event EventHandler<RunSiazActionEventArgs>? RunSiazInvoked;

  /// <summary>
  ///   Event raised when the action for <see cref="ZfsSchemaCheckCommand" /> is invoked by System.CommandLine.
  /// </summary>
  public event EventHandler<ZfsSchemaActionEventArgs>? ZfsSchemaCheckInvoked;

  /// <summary>
  ///   Event raised when the action for <see cref="ZfsSchemaCleanCommand" /> is invoked by System.CommandLine.
  /// </summary>
  public event EventHandler<ZfsSchemaChangeEventArgs>? ZfsSchemaCleanInvoked;

  /// <summary>
  ///   Event raised when the action for <see cref="ZfsSchemaInitializeCommand" /> is invoked by System.CommandLine.
  /// </summary>
  public event EventHandler<ZfsSchemaChangeEventArgs>? ZfsSchemaInitializeInvoked;

  private Task<int> RunSiaz ( ParseResult parseResult, CancellationToken cancellation )
  {
    _logger.Debug ( "Running siaz with command line {0}", ( ) => string.Join ( ' ', parseResult.Tokens ) );

    RunSiazInvoked?.Invoke ( this, new ( ) );

    return Task.FromResult ( (int)ExitCode.EOK );
  }

  /// <summary>
  ///   Scans the symbols under the <c>config global</c> command and handles each according to its type.
  /// </summary>
  /// <param name="parseResult"></param>
  /// <returns></returns>
  private int SetGlobalOptions ( ParseResult parseResult )
  {
    _logger.Debug (
                   "Requested to set global configuration options: {0}",
                   parseResult
                    .CommandResult
                    .Children
                    .OfType<OptionResult> ( )
                    .Select ( static o => $"{o.IdentifierToken?.Value}={o.Tokens [ 0 ].Value}" )
                    .ToSpaceSeparatedSingleLineString ( )
                  );

    Dictionary<string, string?> settings = [ ];
    ConfigurationBuilder        builder  = new ( );

    foreach ( SymbolResult t in parseResult.CommandResult.Children )
    {
      if ( t is not OptionResult result )
      {
        continue;
      }

      switch ( result.Option.Name )
      {
        case nameof (SnapsInAZfsSettings.DryRun):
        case nameof (SnapsInAZfsSettings.Daemonize):
        case nameof (SnapsInAZfsSettings.PruneSnapshots):
        case nameof (SnapsInAZfsSettings.TakeSnapshots):
        {
          TriStateOptionValue value = result.GetRequiredValue ( (Option<TriStateOptionValue>)result.Option );
          settings [ result.Option.Name ] = value.ToString ( "G" );
          _logger.Trace ( "{0} value is {1}", result.Option.Name, value );
        }
          break;

        case nameof (SnapsInAZfsSettings.LocalSystemName):
        {
          string value = result.GetRequiredValue ( (Option<string>)result.Option );
          settings [ result.Option.Name ] = value;
          _logger.Trace ( "{0} value is {1}", result.Option.Name, value );
        }
          break;

        case nameof (SnapsInAZfsSettings.ZfsPath):
        case nameof (SnapsInAZfsSettings.ZpoolPath):
        {
          FileInfo value = result.GetRequiredValue ( (Option<FileInfo>)result.Option );
          settings [ result.Option.Name ] = value.FullName;
          _logger.Trace ( "{0} value is {1}", result.Option.Name, value.FullName );
        }
          break;

        case nameof (SnapsInAZfsSettings.DaemonTimerIntervalSeconds):
        {
          uint value = result.GetRequiredValue ( (Option<uint>)result.Option );
          settings [ result.Option.Name ] = value.ToString ( NumberFormatInfo.CurrentInfo );
          _logger.Trace ( "{0} value is {1}", result.Option.Name, value );
        }
          break;
      }
    }

    builder.AddInMemoryCollection ( settings );
    GlobalConfigChangeEventArgs eventArgs = new ( builder.Build ( ) );

    _logger.ConditionalDebug (
                              $"""
                               Configuration parsed from command line:
                               {eventArgs.ModifiedConfiguration.GetDebugView ( )}
                               """
                             );

    GlobalConfigurationChangeRequested?.Invoke ( this, eventArgs );

    return 0;
  }

  private static void StartConfigConsole ( ParseResult parseResult )
  {
    _logger.Fatal ( "{0} not implemented.", parseResult.CommandResult.Command.Name );
  }

  private int ZfsSchemaCheck ( ParseResult parseResult )
  {
    string[] poolsArgumentResult = parseResult.CommandResult.GetValue ( PoolsArgument ) ?? [ ];

    ZfsSchemaCheckInvoked?.Invoke ( this, new ( poolsArgumentResult ) );

    return 0;
  }

  private int ZfsSchemaClean ( ParseResult parseResult )
  {
    bool confirmOptionPresentAndTrue = parseResult.CommandResult.GetResult ( ZfsSchemaChangeCommands_ConfirmImpactOption )
                                         is { Implicit: false } confirmOptionResult
                                    && confirmOptionResult.GetValueOrDefault<bool> ( );
    bool reallyConfirmOptionPresentAndTrue = parseResult.CommandResult.GetResult ( ZfsSchemaChangeCommands_ReallyConfirmImpactOption )
                                               is { Implicit: false } reallyConfirmOptionResult
                                          && reallyConfirmOptionResult.GetValueOrDefault<bool> ( );

    string[] poolsArgumentResult = parseResult.CommandResult.GetValue ( PoolsArgument ) ?? [ ];

    ZfsSchemaCleanInvoked?.Invoke ( this, new ( poolsArgumentResult, confirmOptionPresentAndTrue, reallyConfirmOptionPresentAndTrue ) );

    return 0;
  }

  private int ZfsSchemaInitialize ( ParseResult parseResult )
  {
    bool confirmOptionPresentAndTrue = parseResult.CommandResult.GetResult ( ZfsSchemaChangeCommands_ConfirmImpactOption )
                                         is { Implicit: false } confirmOptionResult
                                    && confirmOptionResult.GetValueOrDefault<bool> ( );
    bool reallyConfirmOptionPresentAndTrue = parseResult.CommandResult.GetResult ( ZfsSchemaChangeCommands_ReallyConfirmImpactOption )
                                               is { Implicit: false } reallyConfirmOptionResult
                                          && reallyConfirmOptionResult.GetValueOrDefault<bool> ( );

    string[] poolsArgumentResult = parseResult.CommandResult.GetValue ( PoolsArgument ) ?? [ ];

    ZfsSchemaInitializeInvoked?.Invoke ( this, new ( poolsArgumentResult, confirmOptionPresentAndTrue, reallyConfirmOptionPresentAndTrue ) );

    return 0;
  }
}
