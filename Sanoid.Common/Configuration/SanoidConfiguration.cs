using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration
{
    /// <summary>
    /// Internal representation of configuration for sanoid.<br />
    /// Uses the ini-formatted files that PERL sanoid uses if <see cref="Configuration.UseSanoidConfiguration"/> is <see langword="true" />.
    /// Uses values specified in Sanoid.json if <see cref="Configuration.UseSanoidConfiguration"/> is <see langword="false" />.
    /// </summary>
    public class SanoidConfiguration
    {
        private IConfigurationRoot? _configuration;
        internal IConfigurationRoot Configuration =>
            _configuration ??= new ConfigurationManager( )
                .AddIniFile( Path.Combine( Common.Configuration.Configuration.SanoidConfigurationPathBase, Common.Configuration.Configuration.SanoidConfigurationDefaultsFile ) )
                .AddIniFile( Path.Combine( Common.Configuration.Configuration.SanoidConfigurationPathBase, Common.Configuration.Configuration.SanoidConfigurationLocalFile ) )
                .Build( );
    }
}
