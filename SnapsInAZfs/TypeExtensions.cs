#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

namespace SnapsInAZfs;

internal static class TypeExtensions
{
    /// <summary>
    ///     Gets the greatest common factor of all integers in the set
    /// </summary>
    /// <param name="terms"></param>
    /// <param name="fallback">Fallback value if the collection is empty</param>
    /// <returns></returns>
    internal static int GreatestCommonFactor( this IList<int> terms, int fallback = 1 )
    {
        int count = terms.Count;
        if ( count <= 1 )
        {
            return terms.FirstOrDefault( fallback );
        }

        int result = terms[ 0 ];
        for ( int termIndex = 1; termIndex < count; termIndex++ )
        {
            GreatestCommonFactor( ref result, terms[ termIndex ] );
        }

        return result;
        //return terms.Aggregate( GreatestCommonFactor );
    }

    private static void GreatestCommonFactor( ref int left, int right )
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
}
