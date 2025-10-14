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

namespace SnapsInAZfs;

using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;
using CommandLine;
using Interop.Zfs.ZfsTypes;

internal static class TypeExtensions
{
  private const StringSplitOptions TrimAndRemoveBlanks = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

  /// <summary>
  ///   Gets the greatest common factor of all integers in the set.
  /// </summary>
  /// <param name="terms"></param>
  /// <param name="fallback">Fallback value if the collection is empty</param>
  /// <returns></returns>
  public static int GreatestCommonFactor ( this IList<int> terms, int fallback = 1 )
  {
    int count = terms.Count;

    if ( count <= 1 )
    {
      return terms.FirstOrDefault ( fallback );
    }

    int result = terms [ 0 ];

    for ( int termIndex = 1; termIndex < count; termIndex++ )
    {
      GreatestCommonFactor ( ref result, terms [ termIndex ] );
    }

    return result;
  }

  public static string KeysToCommaSeparatedSingleLineString ( this IEnumerable<KeyValuePair<string, bool>> collection, bool withSpaces )
  {
    return collection.Where ( static kvp => !kvp.Value ).Select ( static kvp => kvp.Key ).ToCommaSeparatedSingleLineString ( withSpaces );
  }

  private static void GreatestCommonFactor ( ref int left, int right )
  {
    while ( left != 0 && right != 0 )
    {
      if ( left > right )
      {
        left %= right;
      }
      else
      {
        right %= left;
      }
    }

    left |= right;
  }

  extension ( string original )
  {
    /// <summary>
    ///   Just a proxy for string.Split with both <see cref="StringSplitOptions.RemoveEmptyEntries" /> and
    ///   <see cref="StringSplitOptions.TrimEntries" /> already specified.
    /// </summary>
    /// <param name="separator">The separator character to split on.</param>
    /// <returns>
    ///   An array of strings from the source string, split by the <paramref name="separator" />.
    /// </returns>
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public string[] SplitAndClean ( char separator )
    {
      return original.Split ( separator, TrimAndRemoveBlanks );
    }
  }

  extension ( TriStateOptionValue value )
  {
    public bool ToBoolean ( bool valueIfDefault = true, bool valueIfUnknown = false )
    {
      return value switch
             {
               TriStateOptionValue.Default => valueIfDefault,
               TriStateOptionValue.False   => false,
               TriStateOptionValue.True    => true,
               _                           => valueIfUnknown
             };
    }
  }
}
