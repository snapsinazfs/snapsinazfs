// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.RegularExpressions;

namespace Sanoid.Common.Tests;

public static partial class Regexes
{
    [GeneratedRegex( "(?<header>\\.NET runtimes installed:\\p{C}+)(?<RuntimeLine>(?: +)(?<RuntimeName>(?<NetCore>Microsoft\\.NETCore\\.App (?<versionString>[0-9]{1}\\.\\d+\\.\\d+))|(Microsoft\\.[A-Za-z.]+ \\d+\\.\\d+\\.\\d+)) +(?<pathString>\\[[a-zA-Z0-9:_/\\\\\\. -]+\\])(?:\\p{C}+))*", RegexOptions.CultureInvariant | RegexOptions.Compiled )]
    public static partial Regex DotnetRuntimeSectionRegex( );

    [GeneratedRegex( "(?<header>\\.NET SDKs installed:(\\r\\n|\\r|\\n){1})(?<RuntimeName>(?: +)(?<versionString>[0-9]{1}\\.\\d+\\.\\d+)(?: +\\[.*\\](?:\\r\\n|\\r|\\n){1}))*", RegexOptions.CultureInvariant | RegexOptions.Compiled )]
    public static partial Regex DotnetSdkSectionRegex( );
}
