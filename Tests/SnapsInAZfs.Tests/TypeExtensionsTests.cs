#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace SnapsInAZfs.Tests;

[TestFixture]
[TestOf( typeof( TypeExtensions ) )]
[Parallelizable]
public class TypeExtensionsTests
{
    [Test]
    public void GreatestCommonFactor_OneTerm_ReturnsInput( [Range( 1, 1, 60 )] int term )
    {
        int[] terms = [ term ];
        Assert.That( terms.GreatestCommonFactor( ), Is.EqualTo( term ) );
    }

    [Test]
    [Category( "Exhaustive" )]
    [Parallelizable( ParallelScope.All )]
    [TestCaseSource( nameof( GetThreeTermTestCases ) )]
    public void GreatestCommonFactor_ThreeTerms_ReturnsCorrectResult( int term1, int term2, int term3 )
    {
        int[] terms = [term1, term2, term3];
        int result = terms.GreatestCommonFactor( );
        Assert.Multiple( ( ) =>
        {
            // Mathematically prove that the result is, in fact, the greatest common factor
            // First, check bounds from 1 to 60 and less than or equal to all terms
            Assert.That( result, Is.InRange( 1, 60 ) );
            Assert.That( result, Is.LessThanOrEqualTo( term1 ).And.LessThanOrEqualTo( term2 ).And.LessThanOrEqualTo( term3 ) );

            // Next, check that all terms are evenly divisible by the result
            Assert.Multiple( ( ) =>
            {
                Assert.That( Math.DivRem( term1, result ).Remainder, Is.Zero );
                Assert.That( Math.DivRem( term2, result ).Remainder, Is.Zero );
                Assert.That( Math.DivRem( term3, result ).Remainder, Is.Zero );
            } );

            // Now, prove, by brute force, that all integers greater than result are not factors of at least one of the terms
            for ( int biggerNumber = result + 1; biggerNumber < 60; biggerNumber++ )
            {
                Assert.That( ( term1 % biggerNumber ) + ( term2 % biggerNumber ) + ( term3 % biggerNumber ), Is.Not.Zero );
                int number = biggerNumber;
                Assert.That( terms.Select( t => Math.DivRem( t, number ).Remainder ), Has.Some.Positive );
            }
        } );
    }

    [Test]
    [Category( "Exhaustive" )]
    [Parallelizable( ParallelScope.All )]
    [TestCaseSource( nameof( GetTwoTermTestCases ) )]
    public void GreatestCommonFactor_TwoTerms_ReturnsCorrectResult( int term1, int term2 )
    {
        int[] terms = [term1, term2];
        int result = terms.GreatestCommonFactor( );
        Assert.Multiple( ( ) =>
        {
            Assert.That( result, Is.GreaterThanOrEqualTo( 1 ) );
            Assert.That( result, Is.LessThanOrEqualTo( 60 ) );
            Assert.That( result, Is.LessThanOrEqualTo( term1 ).And.LessThanOrEqualTo( term2 ) );
            Assert.That( term1 % result, Is.Zero );
            Assert.That( term2 % result, Is.Zero );

            // Now, prove, by brute force, that all integers greater than result are not factors of at least one of the terms
            for ( int biggerNumber = result + 1; biggerNumber < 60; biggerNumber++ )
            {
                Assert.That( ( term1 % biggerNumber ) + ( term2 % biggerNumber ), Is.Not.Zero );
                int number = biggerNumber;
                Assert.That( terms.Select( t => t % number ), Has.Some.Positive );
            }
        } );
    }

    private static IEnumerable<int[]> GetThreeTermTestCases( )
    {
        // Not necessary to test all combinations.
        // The two-term test proves the math works.
        // This is now just being extra-careful and proving it works for a third term.
        // So, we'll test a single set of 2 elements against all 60 possible values of the third element.
        // The GCF of the first two elements is 12, which provides 1, 2, 3, 4, 6, and 12 as possible GCF values for generated cases.
        for ( int i = 1; i < 60; i++ )
        {
            yield return [24, 36, i];
        }
    }

    private static IEnumerable<int[]> GetTwoTermTestCases( )
    {
        HashSet<int[]> pairs = new( new IntArrayComparer( ) );
        for ( int term1 = 1; term1 < 60; term1++ )
        {
            for ( int term2 = term1; term2 < 60; term2++ )
            {
                int[] ints = [term1, term2];
                if ( pairs.Add( ints ) )
                {
                    yield return ints;
                }
            }
        }
    }

    private sealed class IntArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals( int[]? x, int[]? y )
        {
            return !x?.Except( y ?? [] ).Any( ) ?? false;
        }

        /// <inheritdoc />
        public int GetHashCode( int[] obj )
        {
            return obj.GetHashCode( );
        }
    }

}
