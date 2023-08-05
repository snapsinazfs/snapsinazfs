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

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using BitConverter = System.BitConverter;

namespace SnapsInAZfs.Tests;

[TestFixture]
[TestOf( typeof( TypeExtensions ) )]
public class TypeExtensionsTests
{
    private static int[][]? ArrayOfThreeIntegerArrays { get; set; }

    [Test]
    public void GreatestCommonFactor_OneTerm_ReturnsInput( [Range( 1, 1, 60 )] int term )
    {
        int[] terms = { term };
        Assert.That( terms.GreatestCommonFactor( ), Is.EqualTo( term ) );
    }

    [Test]
    [Parallelizable( ParallelScope.All )]
    [Category( "Exhaustive" )]
    [TestCaseSource( nameof( ArrayOfThreeIntegerArrays ) )]
    [Pairwise]
    public void GreatestCommonFactor_ThreeTerms_ReturnsCorrectResult( int term1, int term2, int term3 )
    {
        int[] terms = { term1, term2, term3 };
        int result = terms.GreatestCommonFactor( );
        Assert.Multiple( ( ) =>
        {
            // Mathematically prove that the result is, in fact, the greatest common factor
            // First, check bounds from 1 to 60 and less than or equal to all terms
            Assert.That( result, Is.InRange( 1, 60 ) );
            Assert.That( result, Is.LessThanOrEqualTo( term1 ).And.LessThanOrEqualTo( term2 ).And.LessThanOrEqualTo( term3 ) );

            // Next, check that all terms are evenly divisible by the result
            Assert.That( term1 % result, Is.Zero );
            Assert.That( term2 % result, Is.Zero );
            Assert.That( term3 % result, Is.Zero );

            // Now, prove, by brute force, that all integers greater than result are not factors of at least one of the terms
            for ( int biggerNumber = result + 1; biggerNumber < 60; biggerNumber++ )
            {
                Assert.That( ( term1 % biggerNumber ) + ( term2 % biggerNumber ) + ( term3 % biggerNumber ), Is.Not.Zero );
                Assert.That( terms.Select( t => t % biggerNumber ), Has.Some.Positive );
            }
        } );
    }

    [Test]
    [Category( "Exhaustive" )]
    [Parallelizable( ParallelScope.All )]
    [TestCaseSource( nameof( GetTwoTermTestCases ) )]
    public void GreatestCommonFactor_TwoTerms_ReturnsCorrectResult( int term1, int term2 )
    {
        int[] terms = { term1, term2 };
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
                Assert.That( terms.Select( t => t % biggerNumber ), Has.Some.Positive );
            }
        } );
    }

    [OneTimeSetUp]
    public void SetUpTestCases( )
    {
        ArrayOfThreeIntegerArrays = GetThreeTermTestCases( ).ToArray( );
    }

    /// <summary>
    ///     This method generates all possible unique combinations (not permutations) of 3-element integer arrays with element values
    ///     from 1 to 60, and writes them to a file as their raw 4-byte representations
    /// </summary>
    /// <remarks>
    /// </remarks>
    [UsedImplicitly]
    private static void GenerateThreeIntGcfTestFile( )
    {
        using FileStream f = File.Create( "GreatestCommonFactor_ThreeTermTestCaseInput.dat", 12 * 1024 );
        HashSet<int[]> trios = new( new IntArrayComparer( ) );
        for ( int term1 = 1; term1 < 60; term1++ )
        {
            for ( int term2 = term1; term2 < 60; term2++ )
            {
                for ( int term3 = term2 + 1; term3 < 60; term3++ )
                {
                    int[] arr = { term1, term2, term3 };
                    if ( trios.Add( arr ) )
                    {
                        f.Write( MemoryMarshal.Cast<int, byte>( arr.AsSpan( ) ) );
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Reads the pre-generated input file and yields an enumerator to the caller for each 12 bytes of the input file, interpreted as
    ///     3 int values
    /// </summary>
    /// <remarks>
    ///     This file contains every unique combination of 3 integers from 1 to 60, where unique combination means that no two arrays in
    ///     the set contain the same 3 integer values, in any order.<br />
    ///     For example, 1,1,2 is in the set, but 1,2,1 and 2,1,1 are not in the set, because they are equivalent sets.<br />
    ///     Generating these for every test run is of course possible, but it makes the visual studio test runner extremely slow when
    ///     enumerating all 34220 individual test cases.<br />
    ///     When it gets it by reading from this file, it is MUCH faster.
    /// </remarks>
    /// >
    private static IEnumerable<int[]> GetThreeTermTestCases( )
    {
        // See the GenerateThreeIntGcfTestFile() method for how this file was generated
        using FileStream f = File.OpenRead( "GreatestCommonFactor_ThreeTermTestCaseInput.dat" );
        byte[] s = new byte[12];
        while ( f.Position < f.Length )
        {
            f.ReadExactly( s );
            yield return MemoryMarshal.Cast<byte, int>( s ).ToArray( );
        }
        f.Close();
    }

    private static IEnumerable<int[]> GetTwoTermTestCases( )
    {
        HashSet<int[]> pairs = new( new IntArrayComparer( ) );
        for ( int term1 = 1; term1 < 60; term1++ )
        {
            for ( int term2 = term1; term2 < 60; term2++ )
            {
                int[] ints = { term1, term2 };
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
            return !x?.Except( y ?? Array.Empty<int>( ) ).Any( ) ?? false;
        }

        /// <inheritdoc />
        public int GetHashCode( int[] obj )
        {
            return obj.GetHashCode( );
        }
    }
}
