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
using Interop;

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
    Logger.Debug ( "Running siaz with command line {0}", ( ) => string.Join ( ' ', parseResult.Tokens ) );
    Logger.Fatal ( "Not yet implemented." );

    return Task.FromResult ( (int)ExitCode.ECANCELED );
  }

  /// <summary>
  ///   Scans the symbols under the <c>config global</c> command and handles each according to its type.
  /// </summary>
  /// <param name="parseResult"></param>
  /// <returns></returns>
  private static int SetGlobalOptions ( ParseResult parseResult )
  {
    Logger.Debug ( $"Requested to set global configuration options: {parseResult.CommandResult}" );
    Logger.Debug ( $"Command: {parseResult.CommandResult.Command.Name}" );
    Logger.Debug ( $"Command Options: {string.Join ( ',', parseResult.CommandResult.Children.OfType<OptionResult> ( ).Select ( static o => o.Option.Name ) )}" );

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
          Logger.Trace ( "{0} value is {1}", result.Option.Name, value );
        }
          break;

        case nameof (SnapsInAZfsSettings.LocalSystemName):
        {
          string value = result.GetRequiredValue ( (Option<string>)result.Option );
          Logger.Trace ( "{0} value is {1}", result.Option.Name, value );
        }
          break;

        case nameof (SnapsInAZfsSettings.ZfsPath):
        case nameof (SnapsInAZfsSettings.ZpoolPath):
        {
          FileInfo value = result.GetRequiredValue ( (Option<FileInfo>)result.Option );
          Logger.Trace ( "{0} value is {1}", result.Option.Name, value.FullName );
        }
          break;

        case nameof (SnapsInAZfsSettings.DaemonTimerIntervalSeconds):
        {
          uint value = result.GetRequiredValue ( (Option<uint>)result.Option );
          Logger.Trace ( "{0} value is {1}", result.Option.Name, value );
        }
          break;
      }
    }

    return 0;
  }

  private static void StartConfigConsole ( ParseResult parseResult )
  {
    Logger.Fatal ( "{0} not implemented.", parseResult.CommandResult.Command.Name );
  }

  private static int ZfsSchemaCheck ( ParseResult parseResult )
  {
    Console.WriteLine ( parseResult.CommandResult.ToString ( ) );
    Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

    return 0;
  }

  private static int ZfsSchemaClean ( ParseResult arg )
  {
    Console.WriteLine ( "Cleaning SIAZ schema from ZFS" );

    return 0;
  }

  private static int ZfsSchemaInitialize ( ParseResult parseResult )
  {
    Console.WriteLine ( parseResult.CommandResult.ToString ( ) );
    Console.WriteLine ( $"{parseResult.CommandResult.Command.Name} not implemented." );

    return 0;
  }
}
