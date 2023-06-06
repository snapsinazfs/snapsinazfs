// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;

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
        return true;
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
        return true;
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
    public override IAsyncEnumerable<string> ZfsExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
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
                Logger.Info( "Line is a new dataset" );
                DatasetKind kind = p.Value switch
                {
                    "filesystem" => DatasetKind.FileSystem,
                    "volume" => DatasetKind.Volume,
                    _ => throw new InvalidOperationException( $"Unable to parse DatasetKind from line: {stringToParse}" )
                };
                Logger.Info( "New dataset is a {0:F}", kind );
                string rootPathString = parseResult.parent.GetZfsPathRoot();
                bool isNewRoot = !datasets.ContainsKey( rootPathString );
                Dataset newDs = new( parseResult.parent, kind, isNewRoot ? null : datasets[ rootPathString ], isNewRoot );
                datasets.TryAdd( parseResult.parent, newDs );
                Logger.Info( "New {0:F} {1} created and added to result dictionary", kind, newDs.Name );
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
