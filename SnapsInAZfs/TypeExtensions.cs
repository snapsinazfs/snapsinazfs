using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapsInAZfs;
internal static class TypeExtensions
{
    /// <summary>
    /// Gets the greatest common factor of all integers in the set
    /// </summary>
    /// <param name="frequentPeriods"></param>
    /// <returns></returns>
    internal static int GreatestCommonFactor(this IEnumerable<int> frequentPeriods)
    {
        return frequentPeriods.Aggregate(( left, right ) =>
        {
            while ( true )
            {
                if ( left == 0 || right == 0 )
                {
                    return left | right;
                }

                left = Math.Min( left, right );
                right = Math.Max( left, right ) % Math.Min( left, right );
            }
        } );
    }

}
