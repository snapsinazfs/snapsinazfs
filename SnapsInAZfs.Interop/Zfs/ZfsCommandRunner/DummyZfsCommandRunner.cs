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
            ParseAndValidatePoolRootZfsGetLine( lineTokens, rootsAndTheirProperties );
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

            ParsePoolRootDatasetZfsGetLineForConfigConsoleTree( baseDatasets, treeDatasets, lineTokens, treeRootNodes, allTreeNodes );
        }

        await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync( "get", "alldatasets-withproperties.txt" ).ConfigureAwait( true ) )
        {
            Logger.Trace( $"Read line {zfsGetLine}" );
            string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

            ParseDatasetZfsGetLineForConfigConsoleTree( baseDatasets, treeDatasets, lineTokens, allTreeNodes );
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
            ParseDatasetZfsGetLine( stringToParse, datasets );
        }
    }

    private static async Task GetMockZfsSnapshotsFromTextFileAsync( ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, SnapshotRecord> snapshots, string filePath )
    {
        Logger.Info( $"Pretending we ran `zfs list `-t snapshot -H -p -r -o name,{string.Join( ',', IZfsProperty.KnownSnapshotProperties )} pool1" );
        using StreamReader rdr = File.OpenText( filePath );

        while ( !rdr.EndOfStream )
        {
            string stringToParse = await rdr.ReadLineAsync( ).ConfigureAwait( true ) ?? string.Empty;
            ParseSnapshotZfsGetLine( datasets, stringToParse, snapshots );
        }
    }
}
