// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the �Software�), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

/// <summary>
/// Dummy command runner used for testing when running on windows or wherever zfs isn't installed
/// </summary>
public class DummyZfsCommandRunner : ZfsCommandRunnerBase
{
    // ReSharper disable RedundantAwait
    // ReSharper disable AsyncConverter.AsyncAwaitMayBeElidedHighlighting
    /// <inheritdoc />
    public override async Task<bool> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings )
    {
        return await Task.FromResult( true ).ConfigureAwait( true );
    }
    // ReSharper restore AsyncConverter.AsyncAwaitMayBeElidedHighlighting
    // ReSharper restore RedundantAwait

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string propertiesString = IZfsProperty.KnownDatasetProperties.Union( IZfsProperty.KnownSnapshotProperties ).ToCommaSeparatedSingleLineString( );
        Logger.Debug( "Pretending to run zfs get type,{0},available,used -H -p -r -t filesystem,volume,snapshot", propertiesString );
        ConfiguredCancelableAsyncEnumerable<string> lineProvider = ZfsExecEnumeratorAsync( "get", "fullZfsGet.txt" ).ConfigureAwait( true );
        SortedDictionary<string, RawZfsObject> rawObjects = new( );
        await GetRawZfsObjectsAsync( lineProvider, rawObjects ).ConfigureAwait( true );
        ProcessRawObjects( rawObjects, datasets, snapshots );
        CheckAndUpdateLastSnapshotTimesForDatasets( settings, datasets );
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
    public override Task<bool> InheritZfsPropertyAsync( bool dryRun, string zfsPath, IZfsProperty propertyToInherit )
    {
        return Task.FromResult( !dryRun );
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
    public override bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot )
    {
        bool zfsRecursionWanted = ds.Recursion.Value == ZfsPropertyValueConstants.ZfsRecursion;
        Logger.Debug( "{0:G} {2}snapshot requested for dataset {1}", period.Kind, ds.Name, zfsRecursionWanted ? "recursive " : "" );
        string snapName = datasetTemplate.GenerateFullSnapshotName( ds.Name, period.Kind, timestamp );
        snapshot = new( snapName, period.Kind, timestamp, ds );
        return true;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="verb" /> is <see langword="null" />.</exception>
    /// <exception cref="IOException">Invalid attempt to read when no data present</exception>
    public override async IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        ArgumentException.ThrowIfNullOrEmpty( nameof( verb ), "Verb cannot be null or empty" );

        if ( verb is not ("get" or "list") )
        {
            yield break;
        }

        Logger.Trace( "Preparing to execute `{0} {1} {2}` and yield an enumerator for output", "zfs", verb, args );
        Logger.Debug( "Calling zfs {0} {1}", verb, args );
        using StreamReader zfsProcess = File.OpenText( args );

        while ( !zfsProcess.EndOfStream )
        {
            yield return await zfsProcess.ReadLineAsync( ).ConfigureAwait( true ) ?? throw new IOException( "Invalid attempt to read when no data present" );
        }
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
    }
}
