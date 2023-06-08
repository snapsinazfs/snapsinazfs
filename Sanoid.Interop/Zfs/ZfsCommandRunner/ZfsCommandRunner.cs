// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Diagnostics;
using NLog;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

/// <summary>
/// </summary>
public class ZfsCommandRunner : ZfsCommandRunnerBase, IZfsCommandRunner
{
    /// <summary>
    ///     Creates a new instance of the standard <see cref="ZfsCommandRunner" /> class, which uses calls zfs at the path
    ///     provided in <paramref name="pathToZfs" />
    /// </summary>
    /// <param name="pathToZfs">
    ///     A fully-qualified path to the zfs executable
    /// </param>
    /// <param name="pathToZpool">
    ///     A fully-qualified path to the zpool executable
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     If either <paramref name="pathToZfs" /> or <paramref name="pathToZpool" /> is
    ///     null, empty or whitespace
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///     If either <paramref name="pathToZfs" /> or <paramref name="pathToZpool" /> do
    ///     not refer to a valid existing file path
    /// </exception>
    public ZfsCommandRunner( string pathToZfs, string pathToZpool )
    {
        if ( string.IsNullOrWhiteSpace( pathToZfs ) )
        {
            throw new ArgumentNullException( nameof( pathToZfs ), "Path to zfs utility cannot be null" );
        }

        if ( !File.Exists( pathToZfs ) )
        {
            throw new FileNotFoundException( "Path to zfs utility must be a valid and accessible path." );
        }

        if ( string.IsNullOrWhiteSpace( pathToZpool ) )
        {
            throw new ArgumentNullException( nameof( pathToZpool ), "Path to zpool utility cannot be null" );
        }

        if ( !File.Exists( pathToZpool ) )
        {
            throw new FileNotFoundException( "Path to zpool utility must be a valid and accessible path." );
        }

        PathToZfsUtility = pathToZfs;
        PathToZpoolUtility = pathToZpool;
    }

    private string PathToZfsUtility { get; }

    private string PathToZpoolUtility { get; }

    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override bool TakeSnapshot( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot )
    {
        Logger.Debug( "{0:G} snapshot requested for dataset {1}", period.Kind, ds.Name );
        snapshot = Snapshot.GetNewSnapshotForCommandRunner( ds, period, timestamp, settings );
        try
        {
            // This exception is only thrown if kind is invalid. We're passing a known good value.
            // ReSharper disable once ExceptionNotDocumentedOptional
            if ( !snapshot.ValidateName( ) )
            {
                Logger.Error( "Snapshot name {0} is invalid. Snapshot not taken", snapshot.Name );
                return false;
            }
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Error( ex, "Snapshot name {0} is invalid. Snapshot not taken", snapshot.Name );
            return false;
        }

        string arguments = $"snapshot {string.Join( ' ', snapshot.Properties.Values.Select( p => $"-o {p.SetString} " ) )} {snapshot.Name}";
        ProcessStartInfo zfsSnapshotStartInfo = new( PathToZfsUtility, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = false
        };
        if ( settings.DryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", PathToZfsUtility, zfsSnapshotStartInfo.Arguments );
            return false;
        }

        Logger.Debug( "Calling `{0} {1}`", PathToZfsUtility, arguments );
        try
        {
            using ( Process? snapshotProcess = Process.Start( zfsSnapshotStartInfo ) )
            {
                Logger.Debug( "Waiting for {0} {1} to finish", PathToZfsUtility, arguments );
                snapshotProcess?.WaitForExit( );
                if ( snapshotProcess?.ExitCode == 0 )
                {
                    return true;
                }

                Logger.Error( "Snapshot creation failed for {0}", snapshot.Name );
            }

            return false;
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error running {0} {1}. Snapshot may not exist", zfsSnapshotStartInfo.FileName, zfsSnapshotStartInfo.Arguments );
            return false;
        }
    }

    /// <inheritdoc />
    public override async Task<bool> DestroySnapshotAsync( Snapshot snapshot, SanoidSettings settings )
    {
        Logger.Debug( "Requested to destroy snapshot {0}", snapshot.Name );
        try
        {
            // This exception is only thrown if kind is invalid. We're passing a known good value.
            // ReSharper disable once ExceptionNotDocumentedOptional
            if ( !snapshot.ValidateName( ) )
            {
                Logger.Error( "Snapshot name {0} is invalid. Snapshot not destroyed", snapshot.Name );
                return false;
            }
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Error( ex, "Snapshot name {0} is invalid. Snapshot not destroyed", snapshot.Name );
            return false;
        }

        string arguments = $"destroy -d {snapshot.Name}";
        ProcessStartInfo zfsDestroyStartInfo = new( PathToZfsUtility, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = false
        };
        if ( settings.DryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", PathToZfsUtility, zfsDestroyStartInfo.Arguments );
            return false;
        }

        Logger.Debug( "Calling `{0} {1}`", PathToZfsUtility, arguments );
        try
        {
            using ( Process? zfsDestroyProcess = Process.Start( zfsDestroyStartInfo ) )
            {
                Logger.Debug( "Waiting for {0} {1} to finish", PathToZfsUtility, arguments );
                if ( zfsDestroyProcess is not null )
                {
                    await zfsDestroyProcess.WaitForExitAsync( ).ConfigureAwait( true );
                    if ( zfsDestroyProcess.ExitCode == 0 )
                    {
                        return true;
                    }
                }

                Logger.Error( "Destroy snapshot failed for {0}", snapshot.Name );
            }

            return false;
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error running {0} {1}. Snapshot may still exist", zfsDestroyStartInfo.FileName, zfsDestroyStartInfo.Arguments );
            return false;
        }
    }

    /// <inheritdoc />
    public override async Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, Dataset> datasets )
    {
        Logger.Debug( "Requested pool root capacities" );
        bool errorsEncountered = false;
        await foreach ( string zpoolListLine in ZpoolExecEnumerator( "list", $"-Hpo name,{ZpoolProperty.ZfsPoolCapacityPropertyName} {string.Join( ' ', datasets.Keys )}" ) )
        {
            string[] lineTokens = zpoolListLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
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
    /// <exception cref="ArgumentException">If name validation fails for <paramref name="zfsPath" /></exception>
    public override bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties )
    {
        // Ignoring the ArgumentOutOfRangeException that this throws because it's not possible here
        // ReSharper disable once ExceptionNotDocumentedOptional
        if ( !ZfsObjectBase.ValidateName( "filesystem", zfsPath ) )
        {
            throw new ArgumentException( $"Unable to update schema for {zfsPath}. PropertyName is invalid.", nameof( zfsPath ) );
        }

        return PrivateSetZfsProperty( dryRun, zfsPath, properties );
    }

    /// <summary>Gets properties for datasets, either recursively (default) or using supplied arguments</summary>
    /// <returns>
    ///     A  <see cref="Dictionary{TKey,TValue}" /> of <see langword="string" /> to <see cref="Dataset" />
    ///     of all datasets in zfs, with sanoid.net properties populated
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     <list type="bullet">
    ///         <listheader>
    ///             <description>
    ///                 Thrown for the following reasons:
    ///             </description>
    ///         </listheader>
    ///         <item>
    ///             <description>
    ///                 If an exception is thrown when executing the zfs process
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 If a parse error occurs
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     If a <see langword="null" /> string is somehow provided to ContainsKey when
    ///     looking for existing entries in the dictionary to return.
    /// </exception>
    /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer when parsing zfs output.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    public override Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" )
    {
        Dictionary<string, Dataset> datasets = new( );

        Logger.Debug( "Getting ZFS dataset configurations" );
        ProcessStartInfo zfsGetStartInfo = new( PathToZfsUtility, $"get all{args} -t filesystem,volume,snapshot -H -p -o name,property,value,source" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsGetProcess = new( ) { StartInfo = zfsGetStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", (object)zfsGetStartInfo.FileName, (object)zfsGetStartInfo.Arguments );
            try
            {
                zfsGetProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                // Log this, but re-throw, because this is likely fatal, depending on call site
                Logger.Error( ioex, "Error running zfs get operation. The error returned was {0}" );
                throw;
            }

            while ( !zfsGetProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsGetProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "Read line {0} from zfs get", outputLine );
                string[] lineTokens = outputLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
                if ( lineTokens.Length < 4 )
                {
                    // If there aren't the expected number of tokens, something is badly wrong.
                    // Log the error and throw to caller.
                    Logger.Error( "Line {0} not understood", outputLine );
                    throw new InvalidOperationException( $"Unable to parse dataset configuration. Expected 4 tokens in output. Got {lineTokens.Length}: [{outputLine}]" );
                }

                // If datasets doesn't already contain this token, it's a new dataset. Add it.
                string dsName = lineTokens[ 0 ];
                string dsPropertyName = lineTokens[ 1 ];
                if ( !datasets.ContainsKey( dsName ) )
                {
                    if ( dsPropertyName == "type" )
                    {
                        Logger.Debug( "Adding new Dataset {0} to collection", dsName );
                        string newDsType = lineTokens[ 2 ];
                        if ( newDsType is not "filesystem" and not "volume" )
                        {
                            throw new InvalidOperationException( "Type of object from zfs get was unrecognized" );
                        }

                        Logger.Debug( "Dataset {0} will be a {1}", dsName, newDsType );

                        Dataset dataset = new( dsName, newDsType );
                        if ( !datasets.TryAdd( dsName, dataset ) )
                        {
                            // Log if we somehow try to add a duplicate, but continue processing
                            // Likely not fatal, but needs to be reported if it does happen
                            Logger.Error( "Attempted to add a duplicate dataset ({0}) to the collection", dsName );
                        }
                    }
                }
                else
                {
                    // Dataset is already in the collection
                    // This line is a property line
                    // Parse it and add it to the dataset, if it is one of the wanted keys
                    Logger.Trace( "Checking if property {0} is wanted by sanoid", dsPropertyName );
                    if ( ZfsProperty.KnownDatasetProperties.Contains( dsPropertyName ) )
                    {
                        Logger.Trace( "Property {0} is wanted by sanoid. Adding new property {0} to Dataset {1}", dsPropertyName, dsName );

                        // Parse the array starting from the second element (first was dataset name)
                        // The slice does allocate a new array, but it's trivial
                        (bool success, ZfsProperty? prop, string? parent) parseResult = ZfsProperty.FromZfsGetLine( outputLine );

                        if ( parseResult is { success: true, prop: not null, parent: not null } )
                        {
                            datasets[ parseResult.parent ].AddOrUpdateProperty( parseResult.prop );
                        }
                    }
                    else
                    {
                        // Property name wasn't a key in the set of wanted keys, so we can just ignore it and move on
                        Logger.Trace( "Property {0} is not wanted by sanoid. Ignoring", dsPropertyName );
                    }
                }

                Logger.Trace( "Finished with line {0} from zfs get", outputLine );
            }

            if ( !zfsGetProcess.HasExited )
            {
                // If the process hasn't exited, log it at debug level,
                // wait 3 more seconds, and abandon if if the wait times out.
                // It has nothing else useful to say to us
                Logger.Debug( "Waiting for zfs list process to exit" );
                if ( !zfsGetProcess.WaitForExit( 3000 ) )
                {
                    Logger.Warn( "zfs get process abandoned after 3 seconds. Please report this warning if you receive it often." );
                }
            }

            Logger.Debug( "Finished getting datasets" );
            return datasets;
        }
    }

    /// <inheritdoc />
    public override async Task<ConcurrentDictionary<string, Dataset>> GetPoolRootDatasetsWithAllRequiredSanoidPropertiesAsync( )
    {
        ConcurrentDictionary<string, Dataset> result = new( );
        Logger.Debug( "Requested pool root configuration" );
        // ZFS is interesting here... If a property doesn't exist, it'll return it in the output anyway, but just
        // show its value and source as none, denoted by "-"
        // We can just use that behavior to tell us which properties are defined and which aren't, by whether
        // they have a source of "local" or not.
        // Format of line output from zfs get, without -o argument, is:
        // {datasetName}\t{propertyName}\t{propertyValue}\t{propertySource}\n
        // The line feed is swallowed by the ReadLine method in the iterator, so we don't need to worry about it.
        // Pool roots are the schema root, and must have all required properties defined locally.
        // ZFS User Properties are always inherited, so this guarantees the minimum required schema will be there
        // for Sanoid.net to depend on.
        // The user can, of course, break things, if they want to, but that's their own fault.
        // That's why Sanoid.net will do at least this check on every startup.
        // The run-time cost is minimal, so it's better to be safe, even if a cached configuration is likely to
        // be correct the majority of the time.
        // Comments, corrections, rude commentary, etc. are welcome (but not too rude - this is a labor of love😅).
        await foreach ( string zfsGetLine in ZfsExecEnumerator( "get", $"{string.Join( ',', ZfsProperty.DefaultDatasetProperties.Keys )} -t filesystem -s local,default,none -Hpd 0" ) )
        {
            string[] elements = zfsGetLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            if ( elements.Length != 4 )
            {
                Logger.Error( "Expected exactly 4 elements from zfs get command. Got {0}. Line from ZFS: {1}", elements.Length, zfsGetLine );
                continue;
            }

            // Root datasets are always filesystems, so we can just charge right on ahead
            // Also, just grabbing these strings with names for readability.
            // The compiler will optimize this away, in release builds, so it's nothing to worry about
            string dsName = elements[ 0 ];
            string propertyName = elements[ 1 ];
            string propertyValue = elements[ 2 ];
            string propertyValueSource = elements[ 3 ];

            // Get or add the dataset. If it's already in the dictionary, the existing Dataset will we returned.
            // IF it isn't the new one will be constructed.
            if ( !result.ContainsKey( dsName ) )
            {
                Logger.Debug( "Key not in result. Creating new dataset {0}", dsName );
                result[ dsName ] = new( dsName, "filesystem", null, true );
            }

            Logger.Debug( "Adding property {0}({1} , {2}) to {3}", propertyName, propertyValue, propertyValueSource, dsName );
            result[ dsName ].AddOrUpdateProperty( propertyName, propertyValue, propertyValueSource );
        }

        Logger.Debug( "Pool root configuration retrieved" );
        return result;
    }

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string[] poolRootNames = datasets.Keys.ToArray( );
        string datasetPropertiesString = string.Join( ',', ZfsProperty.KnownDatasetProperties );
        Logger.Debug( "Getting remaining dataset configuration from ZFS" );
        Task[] zfsGetDatasetTasks =
            ( from poolName
                  in poolRootNames
              select Task.Run( async ( ) => await GetDatasets( poolName ).ConfigureAwait( true ) ) ).ToArray( );

        Logger.Debug( "Waiting for all zfs get processes to finish." );
        await Task.WhenAll( zfsGetDatasetTasks ).ConfigureAwait( true );

        Logger.Debug( "Getting all snapshots" );
        Task[] zfsGetSnapshotTasks =
            ( from poolName
                  in poolRootNames
              select Task.Run( async ( ) => await GetSnapshots( poolName ).ConfigureAwait( true ) ) ).ToArray( );
        await Task.WhenAll( zfsGetSnapshotTasks ).ConfigureAwait( true );

        // Local function to get datasets starting from the specified path
        async Task GetDatasets( string poolRootName )
        {
            Logger.Debug( "Getting and parsing filesystem and volume descendents of {0}", poolRootName );
            await foreach ( string zfsGetLine in ZfsExecEnumerator( "get", $"type,{datasetPropertiesString} -H -p -r -t filesystem,volume {poolRootName}" ) )
            {
                Logger.Debug( "Attempting to parse line {0} from zfs", zfsGetLine );
                string[] zfsListTokens = zfsGetLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
                // zfs get operations without an -o argument return 4 values per line
                if ( zfsListTokens.Length != 4 )
                {
                    Logger.Error( "Line not understood. Expected 4 tab-separated tokens. Got {0}: {1}", zfsListTokens.Length, zfsGetLine );
                    continue;
                }

                string dsName = zfsListTokens[ 0 ];
                string propertyName = zfsListTokens[ 1 ];
                string propertyValue = zfsListTokens[ 2 ];
                Logger.Debug( "Checking for existence of dataset {0} in collection", dsName );
                if ( !datasets.ContainsKey( dsName ) )
                {
                    Logger.Debug( "Dataset {0} not in collection. Attempting to add using Name: {0}, Kind: {1}", dsName, propertyValue );
                    if ( datasets.TryAdd( dsName, new( dsName, propertyValue, datasets[ poolRootName ] ) ) )
                    {
                        Logger.Debug( "Added Dataset {0} to collection", dsName );
                        continue;
                    }

                    Logger.Error( "Failed adding dataset {0} to dictionary. Taking and pruning of snapshots for this Dataset may not be performed", dsName );
                    continue;
                }

                // We can ignore and continue if this is the type property
                // This case happens for the pool roots, since they've already been created and the first
                // encountered property will always be type
                if ( propertyName == "type" )
                {
                    continue;
                }

                Logger.Debug( "Adding property {0} to dataset {1}", propertyName, dsName );
                datasets[ dsName ].AddOrUpdateProperty( propertyName, propertyValue, zfsListTokens[ 3 ] );
            }

            Logger.Debug( "Finished adding dataset children of {0}", poolRootName );
        }

        // Local function to get snapshots, starting from the specified path
        async Task GetSnapshots( string poolRootName )
        {
            Logger.Debug( "Getting and parsing snapshot descendents of {0}", poolRootName );
            // This one can use a list operation, because we don't care about inheritance source for snapshots - just the values
            await foreach ( string zfsGetLine in ZfsExecEnumerator( "list", $"-t snapshot -H -p -r -o name,{string.Join( ',', ZfsProperty.KnownSnapshotProperties )} {poolRootName}" ) )
            {
                string[] zfsListTokens = zfsGetLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
                int propertyCount = ZfsProperty.KnownSnapshotProperties.Count + 1;
                if ( zfsListTokens.Length != propertyCount )
                {
                    Logger.Error( "Line not understood. Expected {2} tab-separated tokens. Got {0}: {1}", zfsListTokens.Length, zfsGetLine, propertyCount );
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

                string snapName = zfsListTokens[ 0 ];
                snapshots[ snapName ] = datasets[ snapDatasetName ].AddSnapshot( snap );

                Logger.Debug( "Added snapshot {0} to dataset {1}", snapName, snapDatasetName );
            }

            Logger.Debug( "Finished adding snapshot children of {0}", poolRootName );
        }
    }

    /// <summary>
    ///     Raw call to the zfs utility with any supplied verb and parameters that yields a string enumerator, which provides a
    ///     line-by-line
    ///     enumeration of the output of the command.
    /// </summary>
    /// <param name="verb"></param>
    /// <param name="args"></param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see langword="string" />s, iterating over the output of the zfs operation
    /// </returns>
    public override async IAsyncEnumerable<string> ZfsExecEnumerator( string verb, string args )
    {
        ValidateCommonExecArguments( verb, args );

        if ( verb is not "get" and not "list" )
        {
            throw new ArgumentOutOfRangeException( nameof( verb ), "Only get and list verbs are permitted for zfs enumerator operations" );
        }

        ProcessStartInfo zfsGetStartInfo = new( PathToZfsUtility, $"{verb} {args}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        Logger.Debug( "Preparing to execute `{0} {1} {2}` and yield an enumerator for output", PathToZfsUtility, verb, args );
        using ( Process zfsGetProcess = new( ) { StartInfo = zfsGetStartInfo } )
        {
            Logger.Debug( "Calling {0} {1} {2}", zfsGetStartInfo.FileName, verb, args );
            try
            {
                zfsGetProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                // Log this, but re-throw, because this is likely fatal, depending on call site
                Logger.Error( ioex, "Error running zfs get operation. The error returned was {0}", ioex.Message );
                throw;
            }

            while ( !zfsGetProcess.StandardOutput.EndOfStream )
            {
                yield return ( await zfsGetProcess.StandardOutput.ReadLineAsync( ).ConfigureAwait( true ) )!;
            }
        }
    }

    /// <summary>
    ///     Raw call to the zpool utility with any supplied <paramref name="verb" /> and <paramref name="args" /> that yields a
    ///     string enumerator, which provides a line-by-line
    ///     enumeration of the output of the command.
    /// </summary>
    /// <param name="verb">The verb (list or get) to use</param>
    /// <param name="args">The arguments to supply after the verb</param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see langword="string" />s, iterating over the output of the zpool operation
    /// </returns>
    public override async IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        ValidateCommonExecArguments( verb, args );

        if ( verb is not "get" and not "list" )
        {
            throw new ArgumentOutOfRangeException( nameof( verb ), "Only get and list verbs are permitted for zpool enumerator operations" );
        }

        ProcessStartInfo zpoolExecStartInfo = new( PathToZpoolUtility, $"{verb} {args}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        Logger.Debug( "Preparing to execute `{0} {1} {2}` and yield an enumerator for output", PathToZpoolUtility, verb, args );
        using ( Process zpoolExecProcess = new( ) { StartInfo = zpoolExecStartInfo } )
        {
            Logger.Debug( "Calling {0} {1} {2}", zpoolExecStartInfo.FileName, verb, args );
            try
            {
                zpoolExecProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                // Log this, but re-throw, because this is likely fatal, depending on call site
                Logger.Error( ioex, "Error running zpool {0} operation. The error returned was {1}", verb, ioex.Message );
                throw;
            }

            while ( !zpoolExecProcess.StandardOutput.EndOfStream )
            {
                yield return ( await zpoolExecProcess.StandardOutput.ReadLineAsync( ).ConfigureAwait( true ) )!;
            }
        }
    }

    private static void ValidateCommonExecArguments( string verb, string args )
    {
        if ( string.IsNullOrWhiteSpace( verb ) )
        {
            throw new ArgumentNullException( nameof( verb ), "verb cannot be null" );
        }

        if ( string.IsNullOrWhiteSpace( args ) )
        {
            throw new ArgumentNullException( nameof( args ), "Arguments are required for zfs/zpool exec operations" );
        }
    }

    /// <inheritdoc cref="SetZfsProperties" />
    /// <remarks>
    ///     Does not perform name validation
    /// </remarks>
    private bool PrivateSetZfsProperty( bool dryRun, string zfsPath, params ZfsProperty[] properties )
    {
        if ( properties.Length == 0 )
        {
            Logger.Trace( "No properties to set" );
            return false;
        }

        string propertiesToSet = string.Join( ' ', properties.Select( p => p.SetString ) );
        Logger.Trace( "Attempting to set properties on {0}: {1}", zfsPath, propertiesToSet );
        ProcessStartInfo zfsSetStartInfo = new( PathToZfsUtility, $"set {propertiesToSet} {zfsPath}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsSetProcess = new( ) { StartInfo = zfsSetStartInfo } )
        {
            if ( dryRun )
            {
                Logger.Info( "DRY RUN: Would execute `{0} {1}`", zfsSetStartInfo.FileName, zfsSetStartInfo.Arguments );
                return false;
            }

            Logger.Debug( "Calling {0} {1}", zfsSetStartInfo.FileName, zfsSetStartInfo.Arguments );
            try
            {
                zfsSetProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Error( ioex, "Error running zfs set operation. The error returned was {0}" );
                return false;
            }

            if ( !zfsSetProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs set process to exit" );
                zfsSetProcess.WaitForExit( 3000 );
            }

            Logger.Trace( "zfs set process finished" );
            return true;
        }
    }
}
