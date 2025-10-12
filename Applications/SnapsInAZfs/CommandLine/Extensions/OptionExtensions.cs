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

namespace SnapsInAZfs.CommandLine.Extensions;

using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Numerics;

/// <summary>
///   Extension methods for <see cref="Option{T}" />, enabling fluent usage.
/// </summary>
[PublicAPI]
public static class OptionExtensions
{
  /// <param name="option">
  ///   The <see cref="Option{T}" /> to which the operation applies.
  /// </param>
  /// <typeparam name="T">The type of the option value.</typeparam>
  extension<T> ( Option<T> option )
  {
    /// <summary>
    ///   Adds a source of <see cref="CompletionItem" />s to the <paramref name="option" /> and returns a reference to
    ///   <paramref name="option" />.
    /// </summary>
    /// <param name="completionItemFactory">
    ///   A delegate that accepts a <see cref="CompletionContext" /> and returns an <see cref="IEnumerable{T}" /> of
    ///   <see cref="CompletionItem" />s.
    /// </param>
    /// <returns>The same option that this method was called on.</returns>
    public Option<T> WithCompletionSource ( Func<CompletionContext, IEnumerable<CompletionItem>> completionItemFactory )
    {
      option.CompletionSources.Add ( completionItemFactory );
      return option;
    }

    /// <summary>
    ///   Adds the provided <paramref name="validator" /> delegate to the <see cref="Option.Validators" /> collection for the current
    ///   <see cref="Option{T}" />
    /// </summary>
    /// <param name="validator">
    ///   An <see cref="Action{T}" /> accepting an <see cref="OptionResult" /> and returning nothing, which will be called on every
    ///   value of <paramref name="option" />.
    /// </param>
    /// <returns>
    ///   A reference to <paramref name="option" />, after appending <paramref name="validator" />.
    /// </returns>
    public Option<T> WithValidator ( Action<OptionResult> validator )
    {
      option.Validators.Add ( validator );

      return option;
    }
  }

  /// <param name="option">
  ///   The <see cref="Option{T}" /> where <typeparamref name="T" /> is <see cref="IBinaryInteger{T}" /> of <typeparamref name="T" />
  ///   to which the operation applies.
  /// </param>
  extension<T> ( Option<T> option )
    where T : IBinaryInteger<T>
  {
    /// <summary>
    ///   For a given <see cref="Option{T}" /> of <see langword="int" /> type, adds suggested values, used for tab completion and help
    ///   text, with sorting in numeric order.
    /// </summary>
    /// <param name="values">
    ///   One or more <see langword="int" /> values to provide as suggestions and tab completions.
    /// </param>
    /// <remarks>This method DOES NOT restrict entry to the specified values.</remarks>
    /// <returns>The same option that this method was called on.</returns>
    public Option<T> WithSuggestedCompletionValues ( params int[] values )
    {
      option.CompletionSources
            .Add ( _ =>
                     values
                      .Select
                         ( static i =>
                             new CompletionItem (
                                                 i.ToString ( "D",             CultureInfo.CurrentCulture ),
                                                 sortText: i.ToString ( "D10", CultureInfo.CurrentCulture )
                                                )
                         )
                      .ToArray ( )
                 );
      return option;
    }
  }

  /// <param name="option">
  ///   The <see cref="Option{T}" /> where <typeparamref name="T" /> is an <see langword="unmanaged" /> <see cref="Enum" /> to which
  ///   the operation applies.
  /// </param>
  /// <typeparam name="T">The type of the option value.</typeparam>
  extension<T> ( Option<T> option )
    where T : unmanaged, Enum
  {
    /// <summary>
    ///   For a given <see cref="Option{T}" /> of <see langword="enum" /> type, adds suggested values from the enum names, sorted by
    ///   their values.
    /// </summary>
    /// <returns>The same option that this method was called on.</returns>
    public Option<T> WithValueOrderedEnumHelpText ( )
    {
      option.HelpName = string.Join ( '|', Enum.GetValues<T> ( ).Select ( static e => e.ToString ( "G" ) ) );
      return option;
    }
  }

  /// <param name="option">
  ///   The <see cref="Option{T}" /> where <typeparamref name="T" /> is an <see langword="unmanaged" /> <see cref="Enum" /> with
  ///   underlying type <see cref="IBinaryInteger{T}" /> of <typeparamref name="T" /> to which the operation applies.
  /// </param>
  /// <typeparam name="T">The type of the option value.</typeparam>
  extension<T> ( Option<T> option )
    where T : unmanaged, Enum, IBinaryInteger<T>
  {
    /// <summary>
    ///   For a given <see cref="Option{T}" /> of <see langword="enum" /> type, gets an ordered collection of
    ///   <see cref="CompletionItem" />s, sorted by numeric value.
    /// </summary>
    /// <remarks>
    ///   Values are treated as 20-digit decimal numbers for sorting, to support the maximum possible length of a signed 64-bit value.
    /// </remarks>
    /// <returns>
    ///   An ordered collection of <see cref="CompletionItem" />s, sorted by numeric value.
    /// </returns>
    public IEnumerable<CompletionItem> GetOrderedEnumCompletionItems ( )
    {
      return Enum.GetValues<T> ( ).Select ( static e => new CompletionItem ( e.ToString ( "G" ), sortText: $"{Convert.ToInt64 ( e, NumberFormatInfo.CurrentInfo ):D20}" ) );
    }
  }
}
