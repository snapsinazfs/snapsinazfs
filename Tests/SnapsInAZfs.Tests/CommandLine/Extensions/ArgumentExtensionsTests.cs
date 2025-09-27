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

#pragma warning disable IDE0130
namespace SnapsInAZfs.CommandLine.Extensions.Tests;

using System.CommandLine;
using System.CommandLine.Parsing;

/// <summary>
///     Test fixture for extension methods for <see cref="Argument{T}" />
/// </summary>
/// <remarks>
///     Only basic behavior/sanity is tested, since actual validation correctness is the responsibility of System.CommandLine.
/// </remarks>
[TestFixture]
[TestOf ( typeof( ArgumentExtensions ) )]
[Category ( "Command Line" )]
[Category ( "Type Extensions" )]
public class ArgumentExtensionsTests
{
    private static readonly string[] TestArgValues = [ "val1", "val2", "val3", "val4", "val5" ];

    /// <summary>
    ///     Tests that <see cref="ArgumentExtensions.AcceptingOnlyValuesIn{T}(Argument{T}, string[])" /> can only ever add a validator if
    ///     none existed or create a replacement if there already was one of the same kind.
    /// </summary>
    /// <remarks>
    ///     It's probably overkill to bother with this, because it's just a proxy method, but here it is anyway...
    /// </remarks>
    [Test]
    [Category ( "Validation" )]
    public void AcceptingOnlyValuesIn_CreatesOrUpdatesSingleValidator ( )
    {
        Argument<string> arg = new ( "testArgument" );
        Assume.That ( arg.Validators, Is.Empty );

        arg.AcceptingOnlyValuesIn ( TestArgValues );
        Assert.That ( arg.Validators, Has.Exactly ( 1 ).Items );

        // Grab the validator we just created for later comparison
        Action<ArgumentResult> firstValidator = arg.Validators [ 0 ];

        // Add a new set of values and ensure there's still only one validator.
        arg.AcceptingOnlyValuesIn ( TestArgValues [ ..3 ] );
        Assert.That ( arg.Validators, Has.Exactly ( 1 ).Items );

        // Grab the validator we just set and prove it isn't a reference to the same delegate instance as the original.
        Action<ArgumentResult> secondValidator = arg.Validators [ 0 ];
        Assert.That ( secondValidator, Is.Not.SameAs ( firstValidator ) );
    }

    /// <summary>
    ///     Simple test that just makes sure two validators get added, since all this does is add two validator delegates.
    /// </summary>
    [Test]
    public void OnlyAcceptingLegalExistingWriteableFiles_AddsTwoValidators ( )
    {
        Argument<IEnumerable<string>> arg = new ( "testArgument" );
        Assume.That ( arg.Validators, Is.Empty );
        arg.OnlyAcceptingLegalExistingWriteableFiles( );
        Assert.That ( arg.Validators, Has.Exactly ( 2 ).Items );
    }
}
