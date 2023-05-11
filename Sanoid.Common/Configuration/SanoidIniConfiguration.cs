// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Json.More;
using System.Text.Json.Serialization;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Sanoid.Common.Posix;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Internal representation of configuration for sanoid.<br />
///     Uses the ini-formatted files that PERL sanoid uses if
///     <see cref="Common.Configuration.Configuration.UseSanoidConfiguration" /> is <see langword="true" />.
///     Uses values specified in Sanoid.json if <see cref="Common.Configuration.Configuration.UseSanoidConfiguration" /> is
///     <see langword="false" />.
/// </summary>
internal static class SanoidIniConfiguration
{
    private static Logger Logger = LogManager.GetCurrentClassLogger();
    internal static IConfigurationRoot IniConfiguration =>
#pragma warning disable CA2000
        _configuration ??= new ConfigurationManager( )
                           .AddIniFile( Path.Combine( Common.Configuration.Configuration.ConfigurationPathBase, Common.Configuration.Configuration.SanoidConfigurationDefaultsFile ) )
                           .AddIniFile( Path.Combine( Common.Configuration.Configuration.ConfigurationPathBase, Common.Configuration.Configuration.SanoidConfigurationLocalFile ) )
                           .Build( );
#pragma warning restore CA2000

    private static IConfigurationRoot? _configuration;

}
