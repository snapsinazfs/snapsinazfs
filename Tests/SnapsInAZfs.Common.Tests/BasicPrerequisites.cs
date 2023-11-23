using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SnapsInAZfs.Common.Tests;

[TestFixture( Description = "These tests are for basic system-level prerequisites before we even get into bothering with anything specific to SnapsInAZfs" )]
[Order( 1 )]
[Category( "General" )]
[Category( "Prerequisites" )]
public class BasicPrerequisiteTests
{
    [Test]
    [Order( 1 )]
    public void IsOperatingSystemSupported( )
    {
        Console.Write( $"Checking if operating system is supported: {(Environment.OSVersion.Platform == PlatformID.Unix ? "yes" : $"no")}" );
        Warn.If( Environment.OSVersion.Platform, Is.Not.EqualTo( PlatformID.Unix ), ( ) => "Not on supported platform." );
    }

    [Test]
    [Order( 1 )]
    public void CheckPathEnvironmentVariableIsDefined( )
    {
        Console.Write( "Checking PATH environment variable not null: " );
        string? pathVariable = Environment.GetEnvironmentVariable( "PATH" );
        Console.Write( pathVariable is not null );
        Assert.That( pathVariable, Is.Not.Null );
    }

    [Test]
    [Order( 2 )]
    public void CheckPathEnvironmentVariableIsNotEmpty( )
    {
        Console.Write( "Checking PATH environment variable not empty: " );
        string pathVariable = Environment.GetEnvironmentVariable( "PATH" )!;
        Console.Write( pathVariable );
        Assert.That( pathVariable, Is.Not.Empty );
    }

    private string? _dotnetInfoOutput;
    private readonly Version _minimumSupportedDotnetVersion = new( 7, 0, 0 );

    [Test]
    [NonParallelizable]
    [Order( 3 )]
    [Category( ".NET" )]
    public void CheckDotnetInfo( )
    {
        // OK, this looks really janky, but that's because there's literally not a way to get the executing runtime
        // version accurately. Even on .net 7.0.105 on linux (what I have on an Ubuntu 22.10 box), the runtime identifies itself as v3.1 (???)
        // So, we're going to slice up the output from `dotnet --info` and look for the .NET SDK section and
        // grab the version string from there to check if it's ok...
        Console.Write( "Checking dotnet --info..." );
        Console.Out.Flush( );
        ProcessStartInfo psi = new( "dotnet", "--info" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process? process = Process.Start( psi ) )
        {
            _dotnetInfoOutput = process!.StandardOutput.ReadToEnd( );
            process?.WaitForExit( 2000 );
        }

        // First, we'll at least check that it's a real string, before we bother continuing
        // The following test will dissect the string and make appropriate assertions on it
        Assert.Multiple( ( ) =>
        {
            Assert.That( _dotnetInfoOutput, Is.Not.Null );
            Assert.That( _dotnetInfoOutput, Is.Not.Empty );
        } );
    }

    [Test]
    [Order( 4 )]
    [NonParallelizable]
    [Category( ".NET" )]
    public void CheckDotnetSdkVersionIsSupported( )
    {
        Console.Write( "Checking that dotnet SDK version is supported (7.0 or higher): " );
        Assert.That( _dotnetInfoOutput, Is.Not.Null );
        // This regular expression grabs the ".NET SDKs installed:" section from dotnet --info
        // We expect it to return a collection of Match objects containing exactly one Match,
        // and that Match is expected to contain the named group "versionString" with a non-null,
        // non-empty string that we can then parse as a Version object for comparison.
        Regex netSdkSectionRegex = Regexes.DotnetSdkSectionRegex( );
        MatchCollection matches = netSdkSectionRegex.Matches( _dotnetInfoOutput! );

        Assert.Multiple( ( ) =>
        {
            // A few things to assert here.
            // First, that we only got one match. Otherwise I can't guarantee this regex matched something expected.
            Assert.That( matches, Has.Count.EqualTo( 1 ) );

            // If that passed, let's make sure the named group is there and that it's valid
            Assert.Multiple( ( ) =>
            {
                Assert.That( matches[ 0 ].Groups, Does.ContainKey( "versionString" ) );
                Assert.That( matches[ 0 ].Groups[ "versionString" ].Success, Is.True );
                Assert.That( matches[ 0 ].Groups[ "versionString" ].Value, Is.Not.Null );
                Assert.That( matches[ 0 ].Groups[ "versionString" ].Value, Is.Not.Empty );
            } );

            // If that passed, let's make sure the named group is there and that it's valid
            GroupCollection matchedGroups = matches[ 0 ].Groups;
            Assert.That( matchedGroups, Does.ContainKey( "versionString" ) );
            Group versionGroup = matchedGroups[ "versionString" ];
            Assert.That( versionGroup.Success, Is.True );
            // This collection contains ONLY the version number from lines that matched with the name Microsoft.NETCore.App #.#.#
            CaptureCollection versionGroupCaptures = versionGroup.Captures;
            // Let's make sure there's something in the collection
            Assert.That( versionGroupCaptures, Has.Count.GreaterThanOrEqualTo( 1 ) );
            Version[] netCoreAppVersions = versionGroupCaptures.Select( c => new Version( c.Value ) ).ToArray( );
            Assert.That( netCoreAppVersions, Is.Not.Null );
            Assert.That( netCoreAppVersions, Has.Some.GreaterThanOrEqualTo( _minimumSupportedDotnetVersion ) );
        } );
        Console.Write( "Yes" );
    }

    [Test]
    [Order( 5 )]
    [NonParallelizable]
    [Category( ".NET" )]
    public void CheckDotnetRuntimeVersionIsSupported( )
    {
        Console.Write( "Checking that dotnet runtime version is supported (7.0 or higher): " );
        Assert.That( _dotnetInfoOutput, Is.Not.Null );
        // This regular expression matches the entire ".NET runtimes installed:" section, and specifically
        // captures named groups that should capture as many lines as there are in the entire section
        // We'll use collection asserts to concisely check for a supported version
        Regex netRuntimeSectionRegex = Regexes.DotnetRuntimeSectionRegex( );
        MatchCollection matches = netRuntimeSectionRegex.Matches( _dotnetInfoOutput! );

        Assert.Multiple( ( ) =>
        {
            // Like the SDK test, we expect this regex to match exactly once, but have named groups
            Assert.That( matches, Has.Count.EqualTo( 1 ) );

            // If that passed, let's make sure the named group is there and that it's valid
            GroupCollection matchedGroups = matches[ 0 ].Groups;
            Assert.That( matchedGroups, Does.ContainKey( "versionString" ) );
            Group versionGroup = matchedGroups[ "versionString" ];
            Assert.That( versionGroup.Success, Is.True );
            // This collection contains ONLY the version number from lines that matched with the name Microsoft.NETCore.App #.#.#
            CaptureCollection versionGroupCaptures = versionGroup.Captures;
            // Let's make sure there's something in the collection
            Assert.That( versionGroupCaptures, Has.Count.GreaterThanOrEqualTo( 1 ) );
            Version[] netCoreAppVersions = versionGroupCaptures.Select( c => new Version( c.Value ) ).ToArray( );
            Assert.That( netCoreAppVersions, Is.Not.Null );
            Assert.That( netCoreAppVersions, Has.Some.GreaterThanOrEqualTo( _minimumSupportedDotnetVersion ) );
        } );
        Console.Write( "Yes" );
    }

    [Test]
    [Order( 6 )]
    public void CheckPlatformIs64Bit( )
    {
        Assert.That( Environment.Is64BitOperatingSystem, Is.True );
    }
}
