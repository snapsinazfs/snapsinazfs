// Copyright $CurrentDate.Year Brandon Thetford
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// See https://opensource.org/license/MIT/

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace SnapsInAZfs;

internal static class StringFormattingConstants
{
  /// <summary>
  ///   End bold
  /// </summary>
  internal const string _B = "\e[22m";

  /// <summary>
  ///   End colored text
  /// </summary>
  internal const string _FGCOLOR = "\e[39m";

  /// <summary>
  ///   End underline
  /// </summary>
  internal const string _U = "\e[24m";

  /// <summary>
  ///   Bold
  /// </summary>
  internal const string B = "\e[1m";

  /// <summary>
  ///   Foreground color red
  /// </summary>
  internal const string FGRED = "\e[91m";

  /// <summary>
  ///   Foreground color yellow
  /// </summary>
  internal const string FGYELLOW = "\e[93m";

  /// <summary>
  ///   Negative/Inverted foreground and background colors.
  /// </summary>
  internal const string N = "\e[7m";

  /// <summary>
  ///   Reset to non-inverted foreground and background colors.
  /// </summary>
  internal const string P = "\e[27m";

  /// <summary>
  ///   Underline
  /// </summary>
  internal const string U = "\e[4m";
}
