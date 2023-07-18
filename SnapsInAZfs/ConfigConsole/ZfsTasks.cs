// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SnapsInAZfs.ConfigConsole.TreeNodes;
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

    public static async Task<bool> InheritPropertiesForDataset( bool dryRun, string zfsPath, List<IZfsProperty> inheritedProperties, IZfsCommandRunner commandRunner )
    {
        int successfulOperations = 0;
        List<Task> zfsInheritTasks = new( );
        foreach ( IZfsProperty property in inheritedProperties )
        {
            zfsInheritTasks.Add( commandRunner.InheritZfsPropertyAsync( dryRun, zfsPath, property ).ContinueWith( inheritTask => {
                // ReSharper disable once AsyncConverter.AsyncWait
                // ReSharper disable once AsyncApostle.AsyncWait
                if ( inheritTask.Result )
                {
                    Interlocked.Increment( ref successfulOperations );
                }
            } ) );
        }

        await Task.WhenAll( zfsInheritTasks ).ConfigureAwait( true );

        return successfulOperations == inheritedProperties.Count;
    }

    internal static async Task<List<ITreeNode>> GetFullZfsConfigurationTreeAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets, ConcurrentDictionary<string, Snapshot> baseSnapshots, IZfsCommandRunner commandRunner )
    {
        Logger.Debug( "Getting zfs objects for tree view" );
        try
        {
            List<ITreeNode> treeRootNodes = new( );
            await commandRunner.GetDatasetsAndSnapshotsFromZfsAsync( settings, baseDatasets, baseSnapshots ).ConfigureAwait( true );
            ImmutableSortedDictionary<string, ZfsRecord> sortedSetOfPoolRoots = baseDatasets.Where( kvp => kvp.Value.IsPoolRoot ).ToImmutableSortedDictionary( );

            foreach ( ( string dsName, ZfsRecord baseDataset ) in sortedSetOfPoolRoots )
            {
                ZfsObjectConfigurationTreeNode rootNode = new( dsName, baseDataset, baseDataset.DeepCopyClone( ) );
                treeRootNodes.Add( rootNode );
                treeDatasets[ dsName ] = rootNode.TreeDataset;
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
