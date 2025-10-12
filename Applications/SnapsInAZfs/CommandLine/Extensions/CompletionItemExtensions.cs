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

using System.CommandLine.Completions;

/// <summary>
///   Extensions for <see cref="CompletionItem" />.
/// </summary>
public static class CompletionItemExtensions
{
  /// <param name="enumMember">
  ///   The <see langword="enum" /> member of type <typeparamref name="TEnum" /> from which to create the <see cref="CompletionItem" />
  ///   .
  /// </param>
  /// <typeparam name="TEnum">An <see langword="enum" /> type.</typeparam>
  extension<TEnum> ( TEnum enumMember )
    where TEnum : unmanaged, Enum
  {
    /// <summary>
    ///   Creates a <see cref="CompletionItem" /> instance from an <see langword="enum" /> member where the
    ///   <see cref="CompletionItem.Label" /> is the name of the <see langword="enum" /> member and the
    ///   <see cref="CompletionItem.SortText" /> is the numeric value of the <see langword="enum" /> member, so that they are sorted by
    ///   value rather than label.
    /// </summary>
    /// <returns>
    ///   A <see cref="CompletionItem" /> with <see cref="CompletionItem.Label" /> set to the name of the <see langword="enum" /> member
    ///   and <see cref="CompletionItem.SortText" /> set to the value of the <see langword="enum" /> member.
    /// </returns>
    public CompletionItem ToOrderableCompletionItem ( )
    {
      return new ( $"{enumMember:G}", sortText: $"{enumMember:D}" );
    }
  }
}
