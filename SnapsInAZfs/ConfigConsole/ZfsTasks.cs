#region MIT LICENSE
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

using System.Collections.Concurrent;
using System.Collections.Immutable;
using SnapsInAZfs.ConfigConsole.TreeNodes;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole;

internal static class ZfsTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static Task<ZfsCommandRunnerOperationStatus> SetPropertiesForDatasetAsync( bool dryRun, string zfsPath, List<IZfsProperty> modifiedProperties, IZfsCommandRunner commandRunner )
    {
        return commandRunner.SetZfsPropertiesAsync( dryRun, zfsPath, modifiedProperties );
    }

    public static async Task<ZfsCommandRunnerOperationStatus> InheritPropertiesForDatasetAsync( bool dryRun, string zfsPath, List<IZfsProperty> inheritedProperties, IZfsCommandRunner commandRunner )
    {
        int successfulOperations = 0;
        List<Task> zfsInheritTasks = new( );
        foreach ( IZfsProperty property in inheritedProperties )
        {
            zfsInheritTasks.Add( commandRunner.InheritZfsPropertyAsync( dryRun, zfsPath, property ).ContinueWith( async inheritTask =>
            {
                Logger.Trace( "ZFS inherit operation continuation received" );
                ZfsCommandRunnerOperationStatus inheritResult = await inheritTask.ConfigureAwait( false );
                switch ( inheritResult )
                {
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Trace( "ZFS inherit operation succeeded" );
                        goto Increment;
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Trace( "DRY RUN: Pretenting ZFS inherit operation succeeded" );
                    Increment:
                        Interlocked.Increment( ref successfulOperations );
                        break;
                    case ZfsCommandRunnerOperationStatus.NameValidationFailed:
                    case ZfsCommandRunnerOperationStatus.ZeroLengthRequest:
                    case ZfsCommandRunnerOperationStatus.OneOrMoreOperationsFailed:
                    case ZfsCommandRunnerOperationStatus.Failure:
                        Logger.Trace( "Failure result received from ZFS inherit operation" );
                        goto default;
                    case ZfsCommandRunnerOperationStatus.ZfsProcessFailure:
                        Logger.Trace( "ZfsProcessFailure result received from ZFS inherit operation" );
                        goto default;
                    default:
                        Logger.Error( "Error inheriting property" );
                        break;
                }
            } ) );
        }

        await Task.WhenAll( zfsInheritTasks ).ConfigureAwait( true );
        if ( successfulOperations == inheritedProperties.Count )
        {
            if ( dryRun )
            {
                Logger.Trace( "DRY RUN: Pretending all requested properties were inherited successfully for {0}", zfsPath );
                return ZfsCommandRunnerOperationStatus.DryRun;
            }

            Logger.Trace( "All requested properties were inherited successfully for {0}", zfsPath );
            return ZfsCommandRunnerOperationStatus.Success;
        }

        Logger.Error( "One or more operations failed while inheriting requested properties for {0}", zfsPath );
        return ZfsCommandRunnerOperationStatus.OneOrMoreOperationsFailed;
    }

    internal static async Task<List<ITreeNode>> GetFullZfsConfigurationTreeAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets, ConcurrentDictionary<string, Snapshot> baseSnapshots, IZfsCommandRunner commandRunner )
    {
        Logger.Debug( "Getting zfs objects for tree view" );
        try
        {
            List<ITreeNode> treeRootNodes = new( );
            await commandRunner.GetDatasetsAndSnapshotsFromZfsAsync( settings, baseDatasets, baseSnapshots ).ConfigureAwait( true );
            ImmutableSortedDictionary<string, ZfsRecord> sortedSetOfPoolRoots = baseDatasets.Where( static kvp => kvp.Value.IsPoolRoot ).ToImmutableSortedDictionary( );

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
