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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ConfigConsole;
using Interop;
using Interop.Zfs.ZfsTypes;
using NLog.Config;
using NLog.Extensions.Logging;

public partial class SiazCommandLine
{
  /// <summary>
  ///   A reference to the <see cref="RootCommand" /> of the command line.
  /// </summary>
  public RootCommand RootCommand { get; private set; }

  private Command RunCommand { get; } = new (
                                             RunCommandName,
                                             """
                                             Run SIAZ, optionally specifying override options.
                                             Use this context when executing one-off operations or for custom service/script-based invocations.
                                             """
                                            );

  internal const string RunCommandName                  = "run";
  private const  string ConfigCommandName               = "config";
  private const  string ConfigConsoleCommandName        = "console";
  private const  string ConfigGlobalCommandName         = "global";
  private const  string ConfigGlobalDryRunCommandName   = "dry-run";
  private const  string KestrelConfigurationSectionName = "Kestrel";
  private const  string ZfsCommandName                  = "zfs";
  private const  string ZfsSchemaCheckCommandName       = "check";
  private const  string ZfsSchemaCleanCommandName       = "clean";
  private const  string ZfsSchemaCommandName            = "schema";
  private const  string ZfsSchemaInitializeCommandName  = "initialize";
}
