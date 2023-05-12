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
public class TemplateTests
{
    [OneTimeSetUp]
    public void OneTimeSetup( )
    {
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
}
