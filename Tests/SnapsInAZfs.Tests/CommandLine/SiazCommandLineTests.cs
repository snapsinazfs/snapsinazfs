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

// Copyright $CurrentDate.Year Brandon Thetford
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// See https://opensource.org/license/MIT/
// ReSharper disable CheckNamespace
#pragma warning disable IDE0130

namespace SnapsInAZfs.CommandLine.Tests;

using System.CommandLine;
using System.CommandLine.Parsing;

[TestFixture]
[TestOf ( typeof( SiazCommandLine ) )]
[Category ( "Settings" )]
[Category ( "Command Line" )]
public class SiazCommandLineTests
{
  private static TextWriter ErrorStream  => TestContext.CurrentContext.Test.Properties.ContainsKey ( "Use stderr" ) ? TestContext.Error : TextWriter.Null;
  private static TextWriter OutputStream => TestContext.CurrentContext.Test.Properties.ContainsKey ( "Use stdout" ) ? TestContext.Out : TextWriter.Null;

  [Test]
  public void ConfigureCommandLineTree_CreatesNewRootCommand ( )
  {
    SiazCommandLine siazCli             = new ( );
    RootCommand     originalRootCommand = siazCli.RootCommand;
    Assume.That ( siazCli.RootCommand, Is.Not.Null.And.InstanceOf<RootCommand> ( ) );

    siazCli.ConfigureCommandLineTree ( );

    Assert.That ( siazCli.RootCommand, Is.Not.Null.And.Not.SameAs ( originalRootCommand ) );
  }

  [Test]
  [Category ( "Exceptions" )]
  public void ConfigureCommandLineTree_DoesNotThrow ( )
  {
    SiazCommandLine siazCli = new ( );
    Assert.DoesNotThrow ( ( ) => siazCli.ConfigureCommandLineTree ( ) );
  }

  [Test]
  [Category ( "Null Handling" )]
  public void Constructor_SetsRootCommand_InstanceOf_SCLRootCommand ( )
  {
    SiazCommandLine siazCli = new ( );
    Assert.That ( siazCli.RootCommand, Is.Not.Null );
    Assert.That ( siazCli.RootCommand, Is.InstanceOf<RootCommand> ( ) );
  }

  [Test]
  [Category ( "Validation" )]
  public void Invoke_NoArgs_HasExactlyOneParseError ( )
  {
    SiazCommandLine siazCli = new ( );
    Assume.That ( siazCli, Is.Not.Null.And.InstanceOf<SiazCommandLine> ( ) );

    string[] emptyArgs = [ ];

    ParseResult result = siazCli.Parse ( emptyArgs );
    _ = siazCli.Invoke ( OutputStream, ErrorStream );

    Assert.That ( result,        Is.SameAs ( siazCli.RootCommandParseResult ) );
    Assert.That ( result.Errors, Has.Exactly ( 1 ).TypeOf<ParseError> ( ) );
  }
}
