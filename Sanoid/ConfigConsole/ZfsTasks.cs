// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Text.Json;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui.Trees;

namespace Sanoid.ConfigConsole;

internal static class ZfsTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    internal static async Task<List<ITreeNode>> GetFullZfsConfigurationTreeAsync( ConcurrentDictionary<string, SanoidZfsDataset> baseDatasets, ConcurrentDictionary<string, SanoidZfsDataset> treeDatasets, ConcurrentDictionary<string, Snapshot> snapshots, IZfsCommandRunner commandRunner )
    {
        Logger.Debug( "Getting zfs objects for tree view" );
        try
        {
            return await commandRunner.GetZfsObjectsForConfigConsoleTreeAsync( baseDatasets, treeDatasets ).ConfigureAwait( true );
        }
        catch ( Exception ex )
        {
            Logger.Error( ex );
            throw;
        }
    }

    public static bool SetPropertiesForDataset(bool dryRun, string zfsPath, List<IZfsProperty> modifiedProperties, IZfsCommandRunner commandRunner )
    {
        return commandRunner.SetZfsProperties( dryRun, zfsPath, modifiedProperties );
    }
}
