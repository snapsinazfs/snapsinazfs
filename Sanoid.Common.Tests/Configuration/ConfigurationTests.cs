// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration;
using Sanoid.Common.Zfs;
using Dataset = Sanoid.Common.Configuration.Datasets.Dataset;

namespace Sanoid.Common.Tests.Configuration;

[TestFixture]
[Order( 1 )]
[Category("General")]
[Category("Configuration")]
public class ConfigurationTests
{
    [OneTimeSetUp]
    public void Setup( )
    {
        _fileBaseConfiguration = new ConfigurationBuilder( )
                                 .AddJsonFile( "Sanoid.json" )
                                 .Build( );
        _fileBaseConfigDictionary = _fileBaseConfiguration.AsEnumerable( ).ToDictionary( pair => pair.Key, pair => pair.Value );

        _fileLocalConfiguration = new ConfigurationBuilder( )
                                  .AddJsonFile( "Sanoid.json" )
                                  .AddJsonFile( "Sanoid.local.json" )
                                  .Build( );

        _mockBaseConfiguration = new ConfigurationBuilder( ).AddInMemoryCollection( CommonStatics.MockBaseConfigDictionary ).Build( );
    }

    private IConfigurationRoot _fileBaseConfiguration;
    private IConfigurationRoot _fileLocalConfiguration;
    private IConfigurationRoot _mockBaseConfiguration;
    private Dictionary<string, string?>? _fileBaseConfigDictionary;
    private Common.Configuration.Configuration? _sanoidConfiguration;

    [Test]
    [Order( 1 )]
    public void BaseConfigurationNotModified( )
    {
        // This test is for making sure that the base configuration hasn't been changed
        // Helps ensure changes to base config don't unintentionally get committed

        Assert.That( _fileBaseConfigDictionary, Is.Not.Null );

        foreach ( ( string key, string? value ) in CommonStatics.MockBaseConfigDictionary )
        {
            if ( _fileBaseConfigDictionary!.TryGetValue( key, out string? fileConfigElementValue ) )
            {
                Assert.That( fileConfigElementValue, Is.EqualTo( value ) );
            }
            else
            {
                Assert.Fail( $"{key} does not exist in Sanoid.json" );
            }
        }
    }

    [Test]
    [Order( 2 )]
    public void ConfigurationDefinesAtLeastOneDataset( )
    {
        // Check if there's at least one dataset defined by checking there is at least one non-null value in the section
        IConfigurationSection datasetsSection = _fileLocalConfiguration.GetRequiredSection( "Datasets" );
        Dictionary<string, string?> datasetsDictionary = datasetsSection.AsEnumerable( ).ToDictionary( pair => pair.Key, pair => pair.Value );
        Assert.That( datasetsDictionary.Any( kvp => kvp.Value is not null ) );
    }

    [Test]
    [Order( 3 )]
    public void CanAccessEnvironmentVariables( )
    {
        // Check if we can access environment variables and that they properly override configuration in files
        // Original value of the TakeSnapshots setting in Sanoid.json is False
        // This sets the environment variable Sanoid.net:TakeSnapshots to True and checks for its existence in configuration and that it is correctly overridden.
        Environment.SetEnvironmentVariable( "Sanoid.net:TakeSnapshots", "True" );
        IConfigurationRoot configurationWithEnvironmentVariables = new ConfigurationBuilder( )
                                                                   .AddJsonFile( "Sanoid.json" )
                                                                   .AddEnvironmentVariables( "Sanoid.net:" )
                                                                   .Build( );
        Dictionary<string, string?> datasetsDictionary = configurationWithEnvironmentVariables.AsEnumerable( ).ToDictionary( pair => pair.Key, pair => pair.Value );
        Assert.That( datasetsDictionary, Contains.Key( "TakeSnapshots" ) );
        Assert.That( datasetsDictionary[ "TakeSnapshots" ], Is.EqualTo( "True" ) );
    }

    private class MockZfsCommandRunner : IZfsCommandRunner
    {
        public void ZfsSnapshot( IConfigurationSection config, IZfsObject snapshotParent )
        {
            throw new NotImplementedException( );
        }

        public bool ZfsSnapshot( Dataset snapshotParent, string snapshotName )
        {
            return true;
        }
    }

    [Test]
    [Order( 4 )]
    public void CanConstructConfigurationObject( )
    {
        _sanoidConfiguration = new( _fileLocalConfiguration, new MockZfsCommandRunner( ) );

        Assert.Multiple( ( ) =>
        {
            Assert.That( _sanoidConfiguration, Is.InstanceOf<Common.Configuration.Configuration>( ) );
            Assert.That( _sanoidConfiguration, Is.Not.Null );
        } );
    }

    [Test]
    [Order( 5 )]
    public void ExpectedValuesExistInConfigurationAfterLoadingFromIConfiguration( )
    {
        _sanoidConfiguration!.LoadConfigurationFromIConfiguration( );

        Assert.Multiple( ( ) =>
        {
            Assert.That( _sanoidConfiguration.CacheDirectory, Is.EqualTo( "/var/cache/sanoid" ) );
            Assert.That( _sanoidConfiguration.ConfigurationPathBase, Is.EqualTo( "/etc/sanoid" ) );
        } );
    }
}
