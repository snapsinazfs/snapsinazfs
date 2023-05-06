using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Sanoid.Common.Configuration
{
    /// <summary>
    /// Internal representation of configuration for sanoid.<br />
    /// Uses the ini-formatted files that PERL sanoid uses if <see cref="Common.Configuration.Configuration.UseSanoidConfiguration"/> is <see langword="true" />.
    /// Uses values specified in Sanoid.json if <see cref="Common.Configuration.Configuration.UseSanoidConfiguration"/> is <see langword="false" />.
    /// </summary>
    internal static class SanoidIniConfiguration
    {
        private static IConfigurationRoot? _configuration;

        internal static IConfigurationRoot Configuration =>
            _configuration ??= new ConfigurationManager()
                .AddIniFile( Path.Combine( Common.Configuration.Configuration.SanoidConfigurationPathBase, Common.Configuration.Configuration.SanoidConfigurationDefaultsFile ) )
                .AddIniFile( Path.Combine( Common.Configuration.Configuration.SanoidConfigurationPathBase, Common.Configuration.Configuration.SanoidConfigurationLocalFile ) )
                .Build();
    }
}