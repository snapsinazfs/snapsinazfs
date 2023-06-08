// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Terminal.Gui.Trees;

namespace Sanoid.ConfigConsole;

internal static class ZfsTasks
{
    internal static async Task<List<ITreeNode>> GetFullZfsConfigurationTreeAsync( ConcurrentDictionary<string, SanoidZfsDataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots, IZfsCommandRunner commandRunner )
    {
        List<ITreeNode> nodes = new( );
        await foreach ( string zfsLine in commandRunner.ZfsExecEnumerator( "get", $"type,{string.Join( ',', ZfsProperty.KnownDatasetProperties )} -Hpt filesystem -d 0" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
            datasets.AddOrUpdate( lineTokens[ 0 ], k => new( k, lineTokens[ 2 ] ), ( k, ds ) =>
            {
                ds.UpdateProperty( lineTokens[ 1 ], lineTokens[ 2 ], lineTokens[ 3 ] );
                return ds;
            } );
        }

        await foreach ( string zfsLine in commandRunner.ZfsExecEnumerator( "get", $"type,{string.Join( ',', ZfsProperty.KnownDatasetProperties )} -Hprt filesystem,volume {string.Join( ' ', datasets.Keys )}" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
            datasets.AddOrUpdate( lineTokens[ 0 ], k =>
            {
                int lastSlashIndex = k.LastIndexOf( '/' );
                string parentName = k[ ..lastSlashIndex ];
                SanoidZfsDataset parentDs = datasets[ parentName ];
                SanoidZfsDataset newDs = new( k, lineTokens[ 2 ], parentDs );
                parentDs.Children.Add( newDs );
                return newDs;
            }, ( k, ds ) =>
            {
                if ( ds.IsPoolRoot )
                {
                    return ds;
                }

                ds.UpdateProperty( lineTokens[ 1 ], lineTokens[ 2 ], lineTokens[ 3 ] );
                return ds;
            } );
        }

        return nodes;
    }
}
