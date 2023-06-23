// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole;

internal static class ZfsTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static bool SetPropertiesForDataset( bool dryRun, string zfsPath, List<IZfsProperty> modifiedProperties, IZfsCommandRunner commandRunner )
    {
        return commandRunner.SetZfsProperties( dryRun, zfsPath, modifiedProperties );
    }

    internal static async Task<List<ITreeNode>> GetFullZfsConfigurationTreeAsync( ConcurrentDictionary<string, SnapsInAZfsZfsDataset> baseDatasets, ConcurrentDictionary<string, SnapsInAZfsZfsDataset> treeDatasets, ConcurrentDictionary<string, Snapshot> snapshots, IZfsCommandRunner commandRunner )
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
}
