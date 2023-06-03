// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sanoid.Interop.Libc.Enums;
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
    public override ConcurrentDictionary<string, Dataset> GetPoolRootsWithAllRequiredSanoidProperties( )
    {
        ConcurrentDictionary<string, Dataset> poolRoots = new( );
        GetMockZfsOutputFromTextFile(poolRoots,"poolroots-good.txt");
        return poolRoots;
    }

    /// <param name="datasets"></param>
    /// <inheritdoc />
    public override Dictionary<string, Snapshot> GetZfsSanoidSnapshots( ref Dictionary<string, Dataset> datasets )
    {
        return new( );
    }

    /// <inheritdoc />
    public override (Errno status, ConcurrentDictionary<string, Dataset> datasets) GetFullDatasetConfiguration( SanoidSettings settings )
    {
        ConcurrentDictionary<string, Dataset> datasets = new( );
        GetMockZfsOutputFromTextFile( datasets, "datasets.txt" );
        Logger.Info( "Final dictionary is: {0}", JsonSerializer.Serialize( datasets, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
        return ( Errno.ECANCELED, datasets );
    }

    private static void GetMockZfsOutputFromTextFile( ConcurrentDictionary<string, Dataset> datasets, string filePath )
    {
        using StreamReader rdr = File.OpenText( filePath );

        while ( !rdr.EndOfStream )
        {
            string stringToParse = rdr.ReadLine( );
            Logger.Info( "Parsing line {0}", stringToParse );
            (bool success, ZfsProperty? prop, string? parent) parseResult = ZfsProperty.FromZfsGetLine( stringToParse );
            if ( parseResult is { success: true, prop: not null, parent: not null } )
            {
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
                else if ( datasets.ContainsKey( parseResult.parent ) )
                {
                    Logger.Info( "Line is a property of an existing object" );
                    datasets[ parseResult.parent ].AddProperty( parseResult.prop );
                }
            }
        }
    }
}
