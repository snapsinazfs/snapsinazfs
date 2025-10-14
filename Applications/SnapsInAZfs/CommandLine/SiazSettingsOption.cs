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

/// <inheritdoc cref="Option{T}" />
/// <remarks>This type adds a <see cref="SettingsKey" /> property, for use in configuration.</remarks>
/// <typeparam name="T">The <see cref="Type" /> of the <see cref="Option{T}" />.</typeparam>
public sealed class SiazSettingsOption<T> : Option<T>, ISiazSettingsKeyedOption
{
  /// <inheritdoc />
  /// <param name="name">The name of the option. This is used during parsing and is displayed in help.</param>
  /// <param name="settingsKey">
  ///   The .net configuration key for the associated setting in <see cref="SnapsInAZfsSettings" />.
  /// </param>
  /// <param name="aliases">Optional aliases by which the option can be specified on the command line.</param>
  public SiazSettingsOption ( string name, string settingsKey, params string[] aliases ) : base ( name, aliases )
  {
    SettingsKey = settingsKey;
  }

  /// <summary>
  ///   Gets or sets the .net configuration key for the associated setting in <see cref="SnapsInAZfsSettings" />.
  /// </summary>
  public string SettingsKey { get; set; }
}
