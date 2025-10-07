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

namespace SnapsInAZfs.Settings;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

/// <summary>
///   Settings definitions for templates
/// </summary>
[method: SetsRequiredMembers]
public sealed record TemplateSettings ( FormattingSettings? Formatting = null, SnapshotTimingSettings? SnapshotTiming = null )
{
  /// <summary>
  ///   Gets or sets the Formatting subsection for this <see cref="TemplateSettings" /> object.
  /// </summary>
  [Required]
  public required FormattingSettings Formatting { get; set; } = Formatting ?? FormattingSettings.GetDefault ( );

  /// <summary>
  ///   Gets or sets the snapshot timing settings subsection.
  /// </summary>
  [Required]
  public required SnapshotTimingSettings SnapshotTiming { get; set; } = SnapshotTiming ?? SnapshotTimingSettings.GetDefault ( );

  /// <summary>
  ///   The section name for this type in configuration files.
  /// </summary>
  [UsedImplicitly ( Reason = "Used by the ConfigurationBinder source generator." )]
  public const string ConfigurationSectionName = "Templates";

  public static TemplateSettings GetDefaultTemplate ( )
  {
    return new ( FormattingSettings.GetDefault ( ), SnapshotTimingSettings.GetDefault ( ) );
  }
}
