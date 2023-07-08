// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Collections.Immutable;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole;

internal static class ZfsTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static bool SetPropertiesForDataset( bool dryRun, string zfsPath, List<IZfsProperty> modifiedProperties, IZfsCommandRunner commandRunner )
    {
        return commandRunner.SetZfsProperties( dryRun, zfsPath, modifiedProperties );
    }

    internal static async Task<List<ITreeNode>> GetFullZfsConfigurationTreeAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets, ConcurrentDictionary<string, Snapshot> baseSnapshots, IZfsCommandRunner commandRunner )
    {
        Logger.Debug( "Getting zfs objects for tree view" );
        try
        {
            List<ITreeNode> treeRootNodes = new( );
            ConcurrentDictionary<string, TreeNode> allTreeNodes = new( );
            await commandRunner.GetDatasetsAndSnapshotsFromZfsAsync( settings, baseDatasets, baseSnapshots ).ConfigureAwait( true );
            ImmutableSortedDictionary<string, ZfsRecord> sortedDatasetDictionary = baseDatasets.ToImmutableSortedDictionary( );
            foreach ( ( string dsName, ZfsRecord baseDataset ) in sortedDatasetDictionary )
            {
                ZfsRecord treeDataset = baseDataset.DeepCopyClone( baseDataset.IsPoolRoot ? null : treeDatasets[ baseDataset.ParentDataset.Name ] );
                treeDatasets[ dsName ] = treeDataset;
                ZfsObjectConfigurationTreeNode node = new( dsName, baseDataset, treeDataset );
                allTreeNodes[ dsName ] = node;
                if ( baseDataset.IsPoolRoot )
                {
                    treeRootNodes.Add( node );
                }
                else
                {
                    allTreeNodes[ baseDataset.ParentDataset.Name ].Children.Add( node );
                }
            }

            return treeRootNodes;
        }
        catch ( Exception ex )
        {
            Logger.Error( ex );
            throw;
        }
    }
}
