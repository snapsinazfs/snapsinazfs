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
using Extensions;

public partial class SiazCommandLine
{
  private Argument<string[]> _zfsSchemaInitializeCommand_PoolsArgument = new ( PoolsArgumentName )
                                                                         {
                                                                           Arity               = ArgumentArity.ZeroOrMore,
                                                                           Description         = "If specified, limits the initialization of the schema to the named pools.",
                                                                           DefaultValueFactory = static _ => [ ]
                                                                         };

  /// <summary>
  ///   Common argument used for options with binary true/false values with some common aliases for those values, plus a third
  ///   "default" value for explicit reversion to defaults.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Allowed values for this argument are defined in <see cref="StandardBooleanFormsSet" />.<br />
  ///     All "truthy" values are equivalent to <see langword="true" />, and all "falsy" values are equivalent to
  ///     <see langword="false" />.
  ///   </para>
  ///   <para>
  ///     The value `default` should be handled according to the definition of the default of the target value, as defined in the
  ///     JSON configuration schema.
  ///   </para>
  ///   <para>
  ///     This argument is not intended for use outside a configuration context.
  ///   </para>
  /// </remarks>
  private Argument<TriStateOptionValue> ConfigStateArgument { get; }
    = new Argument<TriStateOptionValue> ( ConfigStateArgumentName )
      {
        Arity       = ArgumentArity.ExactlyOne,
        Description = "Set to true to set SIAZ to dry run mode (no changes made), false to disable dry run mode (normal operation). Default: false"
      }
     .WithCustomParser ( TriStateArgumentValuesParser )
     .AcceptingOnlyValuesIn ( [ ..StandardBooleanFormsSet, "default" ] );

  /// <summary>
  ///   Common argument used when the user is expected to input the name(s) of one or more ZFS pools.
  /// </summary>
  /// <remarks>
  ///   This argument is not explicitly required in this definition, as it is used as a filter. The absence of this argument is
  ///   interpreted as no filter - i.e., all pools.
  /// </remarks>
  // IDEA: It would be cool and user-friendly to have this call `zpool list` in a completion source to enable rich tab-completion for pool names. Any such functionality would need to fail gracefully if the `zpool` command isn't in the PATH environment variable, to avoid a dependency on building the full configuration before doing it. Perhaps caching the results of it in a file could help with responsiveness?
  private Argument<string[]> PoolsArgument { get; }
    = new
      ( PoolsArgumentName )
      {
        Arity               = ArgumentArity.ZeroOrMore,
        Description         = "If specified, limits the operation to the named pools.",
        DefaultValueFactory = static _ => [ ],
        HelpName = PoolsArgumentName
      };

  private const string ConfigStateArgumentName = "state";
  private const string PoolsArgumentName       = "pools";
}
