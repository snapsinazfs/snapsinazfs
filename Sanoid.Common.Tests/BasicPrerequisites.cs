// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Sanoid.Common.Tests;

[TestFixture( Description = "These tests are for basic system-level prerequisites before we even get into bothering with anything specific to Sanoid.net" )]
[Order( 1 )]
[Parallelizable( ParallelScope.Children )]
public class BasicPrerequisiteTests
{
    [Test]
    [Parallelizable( ParallelScope.None )]
    [Order( 1 )]
    public void CheckPathEnvironmentVariableIsDefined( )
    {
        string? pathVariable = Environment.GetEnvironmentVariable( "PATH" );
        Assert.That( pathVariable, Is.Not.Null );
    }

    [Test]
    [Parallelizable( ParallelScope.None )]
    [Order( 2 )]
    public void CheckPathEnvironmentVariableIsNotEmpty( )
    {
        string pathVariable = Environment.GetEnvironmentVariable( "PATH" )!;
        Assert.That( pathVariable, Is.Not.Empty );
    }

    private string _dotnetInfoOutput;
    private readonly Version _minimumSupportedDotnetVersion = new( 7, 0, 0 );

    [Test]
    [Parallelizable( ParallelScope.None )]
    [Order( 3 )]
    public void CheckDotnetInfo( )
    {
        // OK, this looks really janky, but that's because there's literally not a way to get the executing runtime
        // version accurately. Even on .net 7.0.105 on linux (what I have on an Ubuntu 22.10 box), the runtime identifies itself as v3.1 (???)
        // So, we're going to slice up the output from `dotnet --info` and look for the .NET SDK section and
        // grab the version string from there to check if it's ok...
        ProcessStartInfo psi = new( "dotnet", "--info" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        Process process = Process.Start( psi );
        _dotnetInfoOutput = process.StandardOutput.ReadToEnd( );
        if ( !process.HasExited )
        {
            process.WaitForExit( 2000 );
        }

        process.Dispose( );

        // Let's go ahead and dump it out just so the user can see...
        Console.WriteLine( _dotnetInfoOutput );

        // First, we'll at least check that it's a real string, before we bother continuing
        // The following test will dissect the string and make appropriate assertions on it
        Assert.Multiple( ( ) =>
        {
            Assert.That( _dotnetInfoOutput, Is.Not.Null );
            Assert.That( _dotnetInfoOutput, Is.Not.Empty );
        } );
    }

    [Test]
    [Parallelizable( ParallelScope.Self )]
    [Order( 4 )]
    public void CheckDotnetSdkVersionIsSupported( )
    {
        // This regular expression breaks down as follows:
        //   Matches but does not capture:
        //     ".NET SDK:" + (any number of newline characters) + "Version:" + (any amount of whitespace)
        //   Matches and captures as the named group "versionString":
        //     (1 instance of 7 or 8)  + "." + (one or more digits) + "." + (one or more digits)
        // We expect it to return a collection of Match objects containing exactly one Match,
        // and that Match is expected to contain the named group "versionString" with a non-null,
        // non-empty string that we can then parse as a Version object for comparison.
        Regex netSdkSectionRegex = new( @"(?:\.NET SDK:(?:\r|\r\n|\n)*\s*Version:\s+)(?<versionString>[78]{1}\.\d+\.\d+)" );
        MatchCollection matches = netSdkSectionRegex.Matches( _dotnetInfoOutput );

        Assert.Multiple( ( ) =>
        {
            // A few things to assert here.
            // First, that we only got one match. Otherwise I can't guarantee this regex matched something expected.
            Assert.That( matches.Count, Is.EqualTo( 1 ) );

            // If that passed, let's make sure the named group is there and that it's valid
            Assert.Multiple( ( ) =>
            {
                Assert.That( matches[ 0 ].Groups, Does.ContainKey( "versionString" ) );
                Assert.That( matches[ 0 ].Groups[ "versionString" ].Success, Is.True );
                Assert.That( matches[ 0 ].Groups[ "versionString" ].Value, Is.Not.Null );
                Assert.That( matches[ 0 ].Groups[ "versionString" ].Value, Is.Not.Empty );
            } );

            // Now let's validate the string as a version and do the actual version comparison
            Version sdkVersion = Version.Parse( matches[ 0 ].Groups[ "versionString" ].Value );
            Assert.That( sdkVersion, Is.InstanceOf<Version>( ) );
            Assert.That( sdkVersion, Is.GreaterThanOrEqualTo( _minimumSupportedDotnetVersion ) );
        } );
    }

    [Test]
    [Order( 4 )]
    public void CheckDotnetRuntimeVersionIsSupported( )
    {
        // This regular expression matches the entire ".NET runtimes installed:" section, and specifically
        // captures named groups that should capture as many lines as there are in the entire section
        // We'll use collection asserts to concisely check for a supported version
        Regex netRuntimeSectionRegex = new(
            "(?<header>\\.NET runtimes installed:(\\r\\n|\\r|\\n){1})(?<RuntimeLine>(?: +)(?<RuntimeName>(?<NetCore>Microsoft\\.NETCore\\.App (?<versionString>[0-9]{1}\\.\\d+\\.\\d+))|(Microsoft\\.[A-Za-z.]+ \\d+\\.\\d+\\.\\d+))(?: \\[.*\\]\\r\\n|\\r|\\n){1})*",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
        );
        MatchCollection matches = netRuntimeSectionRegex.Matches( _dotnetInfoOutput );

        Assert.Multiple( ( ) =>
        {
            // Like the SDK test, we expect this regex to match exactly once, but have named groups
            Assert.That( matches.Count, Is.EqualTo( 1 ) );

            // If that passed, let's make sure the named group is there and that it's valid
            GroupCollection matchedGroups = matches[ 0 ].Groups;
            Assert.That( matchedGroups, Does.ContainKey( "versionString" ) );
            Group versionGroup = matchedGroups[ "versionString" ];
            Assert.That( versionGroup.Success, Is.True );
            // This collection contains ONLY the version number from lines that matched with the name Microsoft.NETCore.App #.#.#
            CaptureCollection versionGroupCaptures = versionGroup.Captures;
            // Let's make sure there's something in the collection
            Assert.That( versionGroupCaptures.Count, Is.GreaterThanOrEqualTo( 1 ) );
            Version[] netCoreAppVersions = versionGroupCaptures.Select( c => new Version( c.Value ) ).ToArray( );
            Assert.That( netCoreAppVersions, Is.Not.Null );
            Assert.That( netCoreAppVersions, Has.Some.GreaterThanOrEqualTo( _minimumSupportedDotnetVersion ) );
        } );
    }
}
