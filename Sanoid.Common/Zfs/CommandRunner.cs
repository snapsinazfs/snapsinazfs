// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics;
using Sanoid.Common.Configuration;

namespace Sanoid.Common.Zfs;

internal static class CommandRunner
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    internal static List<string> ZfsListAll( )
    {
        List<string> dataSets = new( );
        ProcessStartInfo zfsListStartInfo = new( JsonConfigurationSections.PlatformUtilitiesConfiguration[ "zfs" ]!, "list -o name -t filesystem,volume -Hr" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsListProcess = new( ) { StartInfo = zfsListStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", zfsListStartInfo.FileName, zfsListStartInfo.Arguments );
            zfsListProcess.Start( );
            while ( !zfsListProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsListProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "{0}", outputLine );
                dataSets.Add( outputLine! );
            }

            if ( !zfsListProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs list process to exit" );
                zfsListProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs list process finished" );
        }

        return dataSets;
    }
}
