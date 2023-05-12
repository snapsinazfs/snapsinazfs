// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;

namespace Sanoid.Common.Zfs;

internal static class CommandRunner
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    internal static List<string> ZfsListAll( )
    {
    #if WINDOWS
        List<string> dataSets = new( ) { "pool1", "pool1/dataset1", "pool1/dataset1/leaf", "pool1/dataset2", "pool1/dataset3", "pool1/zvol1" };
        Logger.Warn( "Running on windows. Returning fake datasets: {0}",JsonSerializer.Serialize( dataSets ) );
    #else
        List<string> dataSets = new();
        ProcessStartInfo zfsListStartInfo = new( JsonConfigurationSections.PlatformUtilitiesConfiguration[ "zfs" ]!, "list -o name -t filesystem,volume -Hr" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsListProcess = new( ) { StartInfo = zfsListStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", zfsListStartInfo.FileName, zfsListStartInfo.Arguments );
            try
            {
                zfsListProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Fatal( "Error running zfs list operation. The error returned was {0}", ioex );
                throw;
            }

            while ( !zfsListProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsListProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "{0}", outputLine );
                dataSets.Add( outputLine );
            }

            if ( !zfsListProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs list process to exit" );
                zfsListProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs list process finished" );
        }
    #endif

        return dataSets;
    }
}
