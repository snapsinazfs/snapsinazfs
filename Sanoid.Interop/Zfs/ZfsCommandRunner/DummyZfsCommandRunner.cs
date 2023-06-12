// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Xml.Linq;

using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

internal class DummyZfsCommandRunner : ZfsCommandRunnerBase
{
    /// <inheritdoc />
    public override bool TakeSnapshot( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot )
    {
        snapshot = Snapshot.GetNewSnapshotForCommandRunner( ds, period, timestamp, settings );
        return true;
    }

    /// <inheritdoc />
    public override async Task<bool> DestroySnapshotAsync( Snapshot snapshot, SanoidSettings settings )
    {
        return await Task.FromResult( true ).ConfigureAwait( true );
    }

    /// <inheritdoc />
    public override async Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, Dataset> datasets )
    {
        using StreamReader rdr = File.OpenText( "poolroots-capacities.txt" );
        bool errorsEncountered = false;
        while ( !rdr.EndOfStream )
        {
            string? stringToParse = await rdr.ReadLineAsync( ).ConfigureAwait( true );
            if ( string.IsNullOrWhiteSpace( stringToParse ) )
            {
                Logger.Error( "Error reading output from zfs. Null or empty line." );
                continue;
            }

            Logger.Info( $"Parsing line {stringToParse}" );

            string[] lineTokens = stringToParse.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            string poolName = lineTokens[ 0 ];
            string poolCapacityString = lineTokens[ 1 ];
            Logger.Debug( "Pool {0} capacity is {1}", poolName, poolCapacityString );
            if ( datasets.TryGetValue( poolName, out Dataset? poolRoot ) && poolRoot is { IsPoolRoot: true } )
            {
                if ( int.TryParse( poolCapacityString, out int usedCapacity ) )
                {
                    Logger.Debug( "Setting dataset object {0} pool used capacity to {1}", poolName, usedCapacity );
                    poolRoot.PoolUsedCapacity = usedCapacity;
                }
                else
                {
                    Logger.Error( "Failed to parse capacity for pool {0}. Prune deferral setting may be incorrect", poolName );
                    errorsEncountered = true;
                }
            }
            else if ( !datasets.ContainsKey( poolName ) )
            {
                Logger.Error( "Pool root {0} does not exist in current program state. Prune deferral setting may be incorrect", poolName );
                errorsEncountered = true;
            }
        }

        return errorsEncountered;
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties )
    {
        return !dryRun;
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        return !dryRun;
    }

    /// <inheritdoc />
    public override Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" )
    {
        return new( );
    }

    /// <inheritdoc />
    public override async Task<ConcurrentDictionary<string, Dataset>> GetPoolRootDatasetsWithAllRequiredSanoidPropertiesAsync( )
    {
        ConcurrentDictionary<string, Dataset> poolRoots = new( );
        await GetMockZfsDatasetsFromTextFileAsync( poolRoots, "poolroots-withproperties.txt" ).ConfigureAwait( true );
        return poolRoots;
    }

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        await GetMockZfsDatasetsFromTextFileAsync( datasets, "alldatasets-withproperties.txt" ).ConfigureAwait( true );
        await GetMockZfsSnapshotsFromTextFileAsync( datasets, snapshots, "allsnapshots-withproperties-needspruning.txt" ).ConfigureAwait( true );
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="verb" /> is <see langword="null" />.</exception>
    public override async IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        ArgumentException.ThrowIfNullOrEmpty( nameof( verb ), "Verb cannot be null or empty" );

        if ( verb is "get" or "list" )
        {
            using StreamReader rdr = File.OpenText( args );
            while ( !rdr.EndOfStream )
            {
                yield return await rdr.ReadLineAsync( ).ConfigureAwait( true ) ?? throw new IOException( "Invalid attempt to read when no data present" );
            }
        }
    }

    /// <inheritdoc />
    public override async Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, SanoidZfsDataset> baseDatasets, ConcurrentDictionary<string, SanoidZfsDataset> treeDatasets )
    {
        List<ITreeNode> treeRootNodes = new( );
        Dictionary<string, TreeNode> allTreeNodes = new( );
        await foreach ( string zfsLine in ZfsExecEnumeratorAsync( "get", "poolroots-withproperties.txt" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
            string dsName = lineTokens[ 0 ];
            baseDatasets.AddOrUpdate( dsName, k =>
            {
                SanoidZfsDataset newRootDsBaseCopy = new( k, lineTokens[ 2 ], true );
                SanoidZfsDataset newRootDsTreeCopy = newRootDsBaseCopy with { };
                ZfsObjectConfigurationTreeNode node = new( dsName, newRootDsBaseCopy, newRootDsTreeCopy );
                Logger.Debug( "Adding new pool root object {0} to collections", newRootDsBaseCopy.Name );
                treeRootNodes.Add( node );
                allTreeNodes[ dsName ] = node;
                treeDatasets.TryAdd( k, newRootDsTreeCopy );
                return newRootDsBaseCopy;
            }, ( k, ds ) =>
            {
                ds.UpdateProperty( lineTokens[ 1 ], lineTokens[ 2 ], lineTokens[ 3 ] );
                treeDatasets[ dsName ].UpdateProperty( lineTokens[ 1 ], lineTokens[ 2 ], lineTokens[ 3 ] );
                Logger.Debug( "Updating property {0} for {1} to {2}", lineTokens[ 1 ], dsName, lineTokens[ 2 ] );
                return ds;
            } );
        }

        await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync( "get", "alldatasets-withproperties.txt" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

            string dsName = lineTokens[ 0 ];
            if ( !baseDatasets.ContainsKey( dsName ) )
            {
                int lastSlashIndex = dsName.LastIndexOf( '/' );
                string parentName = dsName[ ..lastSlashIndex ];
                SanoidZfsDataset parentDsBaseCopy = baseDatasets[ parentName ];
                SanoidZfsDataset parentDsTreeCopy = treeDatasets[ parentName ];
                SanoidZfsDataset newDsBaseCopy = new( dsName, lineTokens[ 2 ], false );
                SanoidZfsDataset newDsTreeCopy = newDsBaseCopy with { };
                ZfsObjectConfigurationTreeNode node = new(dsName, newDsBaseCopy, newDsTreeCopy, parentDsBaseCopy, parentDsTreeCopy );
                allTreeNodes[ dsName ] = node;
                allTreeNodes[ parentName ].Children.Add( node );
                Logger.Debug( "Adding new {0} {1} to {2}", newDsBaseCopy.Kind, newDsBaseCopy.Name, parentDsBaseCopy.Name );
                baseDatasets.TryAdd( dsName, newDsBaseCopy );
                treeDatasets.TryAdd( dsName, newDsTreeCopy );
            }
            else
            {
                SanoidZfsDataset ds = baseDatasets[ dsName ];
                if ( ds.IsPoolRoot )
                {
                    continue;
                }

                string propertyName = lineTokens[ 1 ];
                string propertyValue = lineTokens[ 2 ];
                string propertySource = lineTokens[ 3 ];
                Logger.Debug( "Adding property {0} ({1}) - ({2}) to {3}", propertyName, propertyValue, propertySource, ds.Name );
                ds.UpdateProperty( propertyName, propertyValue, propertySource );
                treeDatasets[ ds.Name ].UpdateProperty( propertyName, propertyValue, propertySource );
            }
        }

        return treeRootNodes;
    }

    private static async Task GetMockZfsDatasetsFromTextFileAsync( ConcurrentDictionary<string, Dataset> datasets, string filePath )
    {
        using StreamReader rdr = File.OpenText( filePath );

        while ( !rdr.EndOfStream )
        {

            string? stringToParse = await rdr.ReadLineAsync( ).ConfigureAwait( true );
            if ( string.IsNullOrWhiteSpace( stringToParse ) )
            {
                Logger.Error( "Error reading output from zfs. Null or empty line." );
                continue;
            }

            Logger.Info( $"Parsing line {stringToParse}" );
            (bool success, ZfsProperty? prop, string? parent) parseResult = ZfsProperty.FromZfsGetLine( stringToParse );
            if ( parseResult is not { success: true, prop: not null, parent: not null } )
            {
                continue;
            }

            Logger.Info( "Parsing successful" );
            ZfsProperty p = parseResult.prop;
            if ( p.Name == "type" )
            {
                Logger.Info( "New dataset is a {0:F}", p.Value );
                string rootPathString = parseResult.parent.GetZfsPathRoot();
                bool isNewRoot = !datasets.ContainsKey( rootPathString );
                Dataset newDs = new( parseResult.parent, p.Value, isNewRoot ? null : datasets[ rootPathString ], isNewRoot );
                datasets.TryAdd( parseResult.parent, newDs );
                Logger.Info( "New {0:F} {1} created and added to result dictionary", p.Value, newDs.Name );
            }
            else if ( datasets.TryGetValue( parseResult.parent, out Dataset? ds ) )
            {
                Logger.Info( "Line is a property of an existing object" );
                ds.AddOrUpdateProperty( parseResult.prop );
            }
        }
    }
    private static async Task GetMockZfsSnapshotsFromTextFileAsync( ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots, string filePath )
    {
        Logger.Info( $"Pretending we ran `zfs list `-t snapshot -H -p -r -o name,{string.Join( ',', ZfsProperty.KnownSnapshotProperties )} pool1" );
        using StreamReader rdr = File.OpenText( filePath );

        while ( !rdr.EndOfStream )
        {
            string? stringToParse = await rdr.ReadLineAsync( ).ConfigureAwait( true );
            string[] zfsListTokens = stringToParse!.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            int propertyCount = ZfsProperty.KnownSnapshotProperties.Count + 1;
            if ( zfsListTokens.Length != propertyCount )
            {
                Logger.Error( "Line not understood. Expected {2} tab-separated tokens. Got {0}: {1}", zfsListTokens.Length, stringToParse, propertyCount );
                continue;
            }

            if ( zfsListTokens[ 2 ] == "-" )
            {
                Logger.Debug( "Line was not a sanoid.net snapshot. Skipping" );
                continue;
            }

            Snapshot snap = Snapshot.FromListSnapshots( zfsListTokens, datasets );
            string snapDatasetName = snap.DatasetName;
            if ( !datasets.ContainsKey( snapDatasetName ) )
            {
                Logger.Error( "Parent dataset {0} of snapshot {1} does not exist in the collection. Skipping", snapDatasetName, snap.Name );
                continue;
            }

            snapshots[ zfsListTokens[ 0 ] ] = datasets[ snapDatasetName ].AddSnapshot( snap );

            Logger.Debug( "Added snapshot {0} to dataset {1}", zfsListTokens[ 0 ], snapDatasetName );
        }
    }
}
