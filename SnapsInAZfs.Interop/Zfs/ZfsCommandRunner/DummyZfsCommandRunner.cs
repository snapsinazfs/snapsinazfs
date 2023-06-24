// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

internal class DummyZfsCommandRunner : ZfsCommandRunnerBase
{
    /// <inheritdoc />
    public override async Task<bool> DestroySnapshotAsync( SnapshotRecord snapshot, SnapsInAZfsSettings settings )
    {
        return await Task.FromResult( true ).ConfigureAwait( true );
    }

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, SnapshotRecord> snapshots )
    {
        await GetMockZfsDatasetsFromTextFileAsync( datasets, "alldatasets-withproperties.txt" ).ConfigureAwait( true );
        await GetMockZfsSnapshotsFromTextFileAsync( datasets, snapshots, "allsnapshots-withproperties-needspruning.txt" ).ConfigureAwait( true );
    }

    /// <inheritdoc />
    public override async Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, ZfsRecord> datasets )
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
            if ( datasets.TryGetValue( poolName, out ZfsRecord? poolRoot ) && poolRoot is { IsPoolRoot: true } )
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
    public override async Task<ConcurrentDictionary<string, ZfsRecord>> GetPoolRootDatasetsWithAllRequiredSnapsInAZfsPropertiesAsync( )
    {
        ConcurrentDictionary<string, ZfsRecord> poolRoots = new( );
        await GetMockZfsDatasetsFromTextFileAsync( poolRoots, "poolroots-withproperties.txt" ).ConfigureAwait( true );
        return poolRoots;
    }

    /// <inheritdoc />
    public override async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( )
    {
        const string fileName = "poolroots-withproperties.txt";
        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> rootsAndTheirProperties = new( );
        await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync( "get", fileName ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            string poolName = lineTokens[ 0 ];
            string propName = lineTokens[ 1 ];
            string propValue = lineTokens[ 2 ];
            string propSource = lineTokens[ 3 ];
            rootsAndTheirProperties.AddOrUpdate( poolName, AddNewDatasetWithProperty, AddPropertyToExistingDs );

            ConcurrentDictionary<string, bool> AddNewDatasetWithProperty( string key )
            {
                ConcurrentDictionary<string, bool> newDs = new( )
                {
                    [ propName ] = CheckIfPropertyIsValid( propName, propValue, propSource )
                };
                return newDs;
            }

            ConcurrentDictionary<string, bool> AddPropertyToExistingDs( string key, ConcurrentDictionary<string, bool> properties )
            {
                properties[ propName ] = CheckIfPropertyIsValid( propName, propValue, propSource );
                return properties;
            }
        }

        return rootsAndTheirProperties;
    }

    /// <inheritdoc />
    public override async Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets )
    {
        List<ITreeNode> treeRootNodes = new( );
        ConcurrentDictionary<string, TreeNode> allTreeNodes = new( );
        await foreach ( string zfsLine in ZfsExecEnumeratorAsync( "get", "poolroots-withproperties.txt" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
            string dsName = lineTokens[ 0 ];
            string propertyValue = lineTokens[ 2 ];
            baseDatasets.AddOrUpdate( dsName, k =>
            {
                ZfsRecord newRootDsBaseCopy = new( k, propertyValue );
                ZfsRecord newRootDsTreeCopy = newRootDsBaseCopy with { };
                ZfsObjectConfigurationTreeNode node = new( dsName, newRootDsBaseCopy, newRootDsTreeCopy );
                Logger.Debug( "Adding new pool root object {0} to collections", newRootDsBaseCopy.Name );
                treeRootNodes.Add( node );
                allTreeNodes[ dsName ] = node;
                treeDatasets.TryAdd( k, newRootDsTreeCopy );
                return newRootDsBaseCopy;
            }, ( k, ds ) =>
            {
                string propertyName = lineTokens[ 1 ];
                string propertySource = lineTokens[ 3 ];
                ds.UpdateProperty( propertyName, propertyValue, propertySource );
                treeDatasets[ dsName ].UpdateProperty( propertyName, propertyValue, propertySource );
                Logger.Debug( "Updating property {0} for {1} to {2}", propertyName, dsName, propertyValue );
                return ds;
            } );
        }

        await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync( "get", "alldatasets-withproperties.txt" ).ConfigureAwait( true ) )
        {
            Logger.Trace( $"Read line {zfsGetLine}" );
            string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

            string dsName = lineTokens[ 0 ];
            string propertyValue = lineTokens[ 2 ];
            if ( !baseDatasets.ContainsKey( dsName ) )
            {
                Logger.Trace( "{0} is not in dictionary. Creating a new {1}", dsName, propertyValue );
                string parentName = dsName.GetZfsPathParent( );
                bool isPoolRoot = dsName == parentName;
                ZfsRecord parentDsBaseCopy = baseDatasets[ parentName ];
                ZfsRecord parentDsTreeCopy = treeDatasets[ parentName ];
                ZfsRecord newDsBaseCopy = new( dsName, propertyValue, isPoolRoot ? null : parentDsBaseCopy );
                ZfsRecord newDsTreeCopy = newDsBaseCopy with { PoolRoot = parentDsTreeCopy };
                ZfsObjectConfigurationTreeNode node = new( dsName, newDsBaseCopy, newDsTreeCopy, parentDsBaseCopy, parentDsTreeCopy );
                allTreeNodes[ dsName ] = node;
                allTreeNodes[ parentName ].Children.Add( node );
                Logger.Debug( "Adding new {0} {1} to {2}", newDsBaseCopy.Kind, newDsBaseCopy.Name, parentDsBaseCopy.Name );
                baseDatasets.TryAdd( dsName, newDsBaseCopy );
                treeDatasets.TryAdd( dsName, newDsTreeCopy );
            }
            else
            {
                ZfsRecord ds = baseDatasets[ dsName ];
                if ( ds.IsPoolRoot )
                {
                    Logger.Trace( "{0} is a pool root - skipping", dsName );
                    continue;
                }

                string propertyName = lineTokens[ 1 ];
                string propertySource = lineTokens[ 3 ];
                Logger.Debug( "Adding property {0} ({1}) - ({2}) to {3}", propertyName, propertyValue, propertySource, ds.Name );
                ds.UpdateProperty( propertyName, propertyValue, propertySource );
                treeDatasets[ ds.Name ].UpdateProperty( propertyName, propertyValue, propertySource );
            }
        }

        return treeRootNodes;
    }

    /// <inheritdoc />
    public override bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string poolName, string[] propertyArray )
    {
        return !dryRun;
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        return !dryRun;
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        return !dryRun;
    }

    /// <inheritdoc />
    public override bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings template, out SnapshotRecord? snapshot )
    {
        snapshot = SnapshotRecord.GetNewSnapshotObjectForCommandRunner( ds, period, timestamp, template );
        return true;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="verb" /> is <see langword="null" />.</exception>
    public override async IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        ArgumentException.ThrowIfNullOrEmpty( nameof( verb ), "Verb cannot be null or empty" );

        if ( verb is not ("get" or "list") )
        {
            yield break;
        }

        using StreamReader rdr = File.OpenText( args );
        while ( !rdr.EndOfStream )
        {
            yield return await rdr.ReadLineAsync( ).ConfigureAwait( true ) ?? throw new IOException( "Invalid attempt to read when no data present" );
        }
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
    }

    private static async Task GetMockZfsDatasetsFromTextFileAsync( ConcurrentDictionary<string, ZfsRecord> datasets, string filePath )
    {
        using StreamReader rdr = File.OpenText( filePath );

        while ( !rdr.EndOfStream )
        {
            string stringToParse = await rdr.ReadLineAsync( ).ConfigureAwait( true ) ?? string.Empty;
            if ( string.IsNullOrWhiteSpace( stringToParse ) )
            {
                Logger.Error( "Error reading output from zfs. Null or empty line." );
                continue;
            }

            Logger.Info( $"Parsing line {stringToParse}" );
            string[] lineTokens = stringToParse.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

            string dsName = lineTokens[0];
            string propertyName = lineTokens[1];
            string propertyValue = lineTokens[2];
            string propertySource = lineTokens[3];

            Logger.Info( "Parsing successful" );
            if ( propertyName == "type" )
            {
                Logger.Info( "New dataset is a {0}", propertyValue );
                string parentName = dsName.GetZfsPathParent( );
                ZfsRecord newDs = new( dsName, propertyValue, parentName == dsName ? null : datasets[ parentName ] );
                datasets.TryAdd( dsName, newDs );
                Logger.Info( "New {0} {1} created and added to result dictionary", propertyValue, dsName );
            }
            else if ( datasets.TryGetValue( dsName, out ZfsRecord? ds ) )
            {
                Logger.Info( "Line is a property of an existing object" );
                ds.UpdateProperty( propertyName, propertyValue, propertySource );
            }
        }
    }

    private static async Task GetMockZfsSnapshotsFromTextFileAsync( ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, SnapshotRecord> snapshots, string filePath )
    {
        Logger.Info( $"Pretending we ran `zfs list `-t snapshot -H -p -r -o name,{string.Join( ',', IZfsProperty.KnownSnapshotProperties )} pool1" );
        using StreamReader rdr = File.OpenText( filePath );

        while ( !rdr.EndOfStream )
        {
            string? stringToParse = await rdr.ReadLineAsync( ).ConfigureAwait( true );
            string[] zfsListTokens = stringToParse!.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            int propertyCount = IZfsProperty.KnownSnapshotProperties.Count + 1;
            if ( zfsListTokens.Length != propertyCount )
            {
                Logger.Error( "Line not understood. Expected {2} tab-separated tokens. Got {0}: {1}", zfsListTokens.Length, stringToParse, propertyCount );
                continue;
            }

            if ( zfsListTokens[ 2 ] == "-" )
            {
                Logger.Debug( "Line was not a SnapsInAZfs snapshot. Skipping" );
                continue;
            }

            string snapshotName = zfsListTokens[ 0 ];
            string dsName = snapshotName.GetZfsPathParent( );
            SnapshotRecord snap = SnapshotRecord.CreateInstance( snapshotName, datasets[ dsName ] );
            if ( !datasets.ContainsKey( dsName ) )
            {
                Logger.Error( "Parent dataset {0} of snapshot {1} does not exist in the collection. Skipping", dsName, snap.Name );
                continue;
            }

            snapshots[ snapshotName ] = datasets[ dsName ].AddSnapshot( snap );

            Logger.Debug( "Added snapshot {0} to dataset {1}", snapshotName, dsName );
        }
    }
}
