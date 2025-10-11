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
using System.Runtime.CompilerServices;

public partial class SiazCommandLine
{
  /// <summary>
  ///   Parses a boolean from more inputs than <see cref="bool" /> is aware of, taken from the first token of an
  ///   <see cref="ArgumentResult" />.
  /// </summary>
  /// <param name="argumentResult">The argument to parse the token from.</param>
  /// <returns>
  ///   If the token matches any of the values in <see cref="StandardBooleanTrueValuesSearch" />, returns <see langword="true" />.
  ///   <br />
  ///   Returns <see langword="false" /> for all other values.
  /// </returns>
  [MethodImpl ( MethodImplOptions.AggressiveInlining )]
  private static bool ArgumentStandardBooleanValuesParser ( ArgumentResult argumentResult )
  {
    return StandardBooleanTrueValuesSearch.Contains ( argumentResult.Tokens [ 0 ].Value );
  }

  /// <summary>
  ///   Parses a boolean from more inputs than <see cref="bool" /> is aware of, taken from the first token of an
  ///   <see cref="ArgumentResult" />.
  /// </summary>
  /// <param name="argumentResult">The argument to parse the token from.</param>
  /// <returns>
  ///   If the token matches any of the values in <see cref="StandardBooleanTrueValuesSearch" />, returns <see langword="true" />.
  ///   <br />
  ///   Returns <see langword="false" /> for all other values.
  /// </returns>
  private static TriStateOptionValue TriStateArgumentValuesParser ( ArgumentResult argumentResult )
  {
    return argumentResult.Tokens [ 0 ].Value is "default" or ""
             ? TriStateOptionValue.Default
             : StandardBooleanTrueValuesSearch.Contains ( argumentResult.Tokens [ 0 ].Value )
               ? TriStateOptionValue.True
               : TriStateOptionValue.False;
  }
}
