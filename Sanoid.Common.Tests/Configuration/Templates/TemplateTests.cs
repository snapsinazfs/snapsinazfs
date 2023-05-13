// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration.Templates;

#pragma warning disable CS8618

namespace Sanoid.Common.Tests.Configuration.Templates;

[TestFixture]
[Order( 2 )]
public class TemplateTests
{
    [OneTimeSetUp]
    public void OneTimeSetup( )
    {
        // Since this test suite is forced to run after the configuration tests, we know configuration is valid,
        // so we will use file configuration from here on out, with supplements if necessary.
        _configurationRoot = new ConfigurationBuilder( )
                             .AddJsonFile( "Sanoid.json" )
                             .AddJsonFile( "Sanoid.local.json" )
                             .Build( );
        _rootTemplatesConfigurationSection = _configurationRoot.GetRequiredSection( "Templates" );
        _rootTemplatesDefaultConfigurationSection = _rootTemplatesConfigurationSection.GetRequiredSection( "default" );
    }

    private static IConfigurationRoot _configurationRoot;
    private static IConfigurationSection _rootTemplatesConfigurationSection;
    private static IConfigurationSection _rootTemplatesDefaultConfigurationSection;
    private static Template _defaultTemplate;

    [Test]
    [Order( 1 )]
    public void CanCreateDefaultTemplate( )
    {
        _defaultTemplate = Template.GetDefault( _rootTemplatesDefaultConfigurationSection );
        Assert.That( _defaultTemplate, Is.Not.Null );
    }

    [Test]
    [Order( 2 )]
    public void CreateChildTest_NoRecursion( )
    {
        Assert.Fail( "Test not implemented" );
        //// Get the Templates:default:Templates section
        //IConfigurationSection defaultTemplateTemplatesSection = _rootTemplatesDefaultConfigurationSection.GetRequiredSection( "Templates" );

        //// Get the Templates:default:Templates:production template
        //IConfigurationSection productionTemplateSection = defaultTemplateTemplatesSection.GetRequiredSection( "production" );

        //// Create a child template, but skip recursion. Only interested in testing building a single template.
        //Template newChildTemplate = _defaultTemplate.CreateChild( productionTemplateSection, null, nameOverride: false, true );

        //Assert.Multiple( ( ) =>
        //{
        //    // Check that our root template has the new child key in its Children dictionary
        //    Assert.That( _defaultTemplate.Children, Contains.Key( "production" ) );

        //    // Check that the object returned by CreateChild is the same object reference as the object in the Children dictionary
        //    Assert.That( _defaultTemplate.Children[ "production" ], Is.SameAs( newChildTemplate ) );

        //    // Now check that all the properties are set and have the expected values
        //    Assert.That( newChildTemplate, Has.Property( "Name" ).EqualTo( "production" ) );
        //    Assert.That( newChildTemplate, Has.Property( "AutoPrune" ).True );
        //    Assert.That( newChildTemplate, Has.Property( "AutoSnapshot" ).True );
        //    Assert.That( newChildTemplate, Has.Property( "Recursive" ).False );
        //    Assert.That( newChildTemplate, Has.Property( "SkipChildren" ).False );
        //    Assert.That( newChildTemplate, Has.Property( "SnapshotRetention" ) );
        //} );
    }
}
