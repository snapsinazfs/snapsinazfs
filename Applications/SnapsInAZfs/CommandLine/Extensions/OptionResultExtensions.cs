// Copyright $CurrentDate.Year Brandon Thetford
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// See https://opensource.org/license/MIT/

namespace SnapsInAZfs.CommandLine.Extensions;

using System.CommandLine;
using System.CommandLine.Parsing;

/// <summary>
///   Extensions for <see cref="OptionResult" />.
/// </summary>
public static class OptionResultExtensions
{
  /// <param name="result">An <see cref="OptionResult" /> to operate on.</param>
  extension ( OptionResult result )
  {
    /// <summary>
    ///   Gets the value of the <see cref="OptionResult.Option" /> in <paramref name="result" /> as an instance of
    ///   <typeparamref name="T" />.<br />
    ///   If the option is not of the requested type or does not exist in the result, <paramref name="defaultValue" /> is returned.
    /// </summary>
    /// <param name="defaultValue">
    ///   The value to return if the option's type differs, or if the option does not exist.
    /// </param>
    /// <typeparam name="T">The type of the value requested.</typeparam>
    /// <remarks>
    ///   This overrides the built-in behavior of <see cref="Option{T}.DefaultValueFactory" /> at the call site.
    /// </remarks>
    /// <returns></returns>
    public T GetValueOrDefault<T> ( T defaultValue )
    {
      if ( result is not { Implicit: false, Option: Option<T> typedOption } )
      {
        return defaultValue;
      }

      return result.GetValue ( typedOption ) ?? defaultValue;
    }
  }
}
