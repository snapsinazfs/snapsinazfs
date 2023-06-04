// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

internal class DummyZfsCommandRunner : ZfsCommandRunnerBase
{
    /// <inheritdoc />
    public override bool TakeSnapshot( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override bool DestroySnapshot( Dataset ds, Snapshot snapshot, SanoidSettings settings )
    {
        return false;
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
    public override async Task<ConcurrentDictionary<string, Dataset>> GetPoolRootsWithAllRequiredSanoidPropertiesAsync( )
    {
        ConcurrentDictionary<string, Dataset> poolRoots = new( );
        await GetMockZfsOutputFromTextFileAsync( poolRoots, "poolroots-good.txt" ).ConfigureAwait( true );
        return poolRoots;
    }

    /// <param name="datasets"></param>
    /// <inheritdoc />
    public override Dictionary<string, Snapshot> GetZfsSanoidSnapshots( ref Dictionary<string, Dataset> datasets )
    {
        return new( );
    }

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SanoidSettings settings, ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        await GetMockZfsOutputFromTextFileAsync( datasets, "datasets.txt" ).ConfigureAwait( true );
        Logger.Info( "Final dictionary is: {0}", JsonSerializer.Serialize( datasets, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
    }

    private static async Task GetMockZfsOutputFromTextFileAsync( ConcurrentDictionary<string, Dataset> datasets, string filePath )
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

            Logger.Info( "Parsing line {0}", stringToParse );
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
                Dataset newDs = new( parseResult.parent, kind );
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
}
