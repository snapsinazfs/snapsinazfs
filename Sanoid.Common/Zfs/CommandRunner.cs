// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Diagnostics;
using Sanoid.Common.Configuration;
using Sanoid.Common.Configuration.Datasets;

namespace Sanoid.Common.Zfs;

internal static class CommandRunner
{
    private static ConcurrentDictionary<string, Dataset> _allDatasets = new( );
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    internal static void ZfsListAll( )
    {
        ProcessStartInfo zfsListStartInfo = new( JsonConfigurationSections.PlatformUtilitiesConfiguration[ "zfs" ]!, "list -o name -t filesystem,volume -Hr" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = true
        };
        using ( SemaphoreSlim zfsListWaitSemaphore = new( 1, 1 ) )
        {
            using ( Process? zfsListProcess = new( ) { EnableRaisingEvents = true, StartInfo = zfsListStartInfo } )
            {
                zfsListProcess.OutputDataReceived += ZfsListProcess_OutputDataReceived;
                zfsListProcess.Exited += ( sender, args ) => { zfsListWaitSemaphore.Release( ); };
                zfsListProcess.Start( );
                zfsListWaitSemaphore.Wait( 30000 );
            }
        }
    }

    private static void ZfsListProcess_OutputDataReceived( object sender, DataReceivedEventArgs e )
    {
        Logger.Debug( "{0}", e.Data );
    }
}
