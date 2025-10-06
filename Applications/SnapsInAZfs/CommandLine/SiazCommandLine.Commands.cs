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

public partial class SiazCommandLine
{
  private Command _configCommand = new (
                                        ConfigCommandName,
                                        "Perform configuration operations on SIAZ and managed pools/datasets directly or via the configuration console."
                                       );

  private Command _configConsoleCommand = new (
                                               ConfigConsoleCommandName,
                                               "Launches the configuration console TUI."
                                              );

  private Command _configGlobalCommand = new Command (
                                                      ConfigGlobalCommandName,
                                                      $"""
                                                      Modify global settings in the root of the JSON configuration files.
                                                      If no --output-file option is specified, resulting changes will be written to the last configuration file loaded, including any specified on the command line.
                                                      """
                                                     );

  private Command _zfsCommand = new (
                                     ZfsCommandName,
                                     "Perform operations on ZFS pools and datasets managed by SIAZ."
                                    );

  private Command _zfsSchemaCheckCommand = new (
                                                ZfsSchemaCheckCommandName,
                                                "Checks the property schema for SnapsInAZfs in ZFS and reports any missing properties for pool roots. Checks all pools by default."
                                               );

  private Command _zfsSchemaCleanCommand = new (
                                                ZfsSchemaCleanCommandName,
                                                "Completely removes all pool and dataset properties that came from SIAZ."
                                               );

  private Command _zfsSchemaCommand = new (
                                           ZfsSchemaCommandName,
                                           "Perform operations on properties of ZFS pools and datasets used by SIAZ."
                                          );

  private Command _zfsSchemaInitializeCommand = new (
                                                     ZfsSchemaInitializeCommandName,
                                                     "Updates the property schema for SnapsInAZfs in ZFS, using default values. Will not overwrite StandardBooleanOptions that are already set."
                                                    );

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
