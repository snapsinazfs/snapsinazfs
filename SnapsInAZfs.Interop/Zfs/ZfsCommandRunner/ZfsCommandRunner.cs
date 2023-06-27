// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

/// <summary>
/// </summary>
public class ZfsCommandRunner : ZfsCommandRunnerBase, IZfsCommandRunner
{
    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

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

    /// <inheritdoc />
    public override bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot )
    {
        bool zfsRecursionWanted = ds.Recursion.Value == ZfsPropertyValueConstants.ZfsRecursion;
        Logger.Debug( "{0} {2}snapshot requested for dataset {1}", period, ds.Name, zfsRecursionWanted ? "recursive " : "" );
        string snapName = datasetTemplate.GenerateFullSnapshotName( ds.Name, period.Kind, timestamp );
        snapshot = new( snapName, ds.PruneSnapshots.Value, period, timestamp, ds );
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

        string arguments = $"snapshot {( zfsRecursionWanted ? "-r " : "" )}{snapshot.GetSnapshotOptionsStringForZfsSnapshot( )} {snapshot.Name}";
        ProcessStartInfo zfsSnapshotStartInfo = new( PathToZfsUtility, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = false
        };
        if ( snapsInAZfsSettings.DryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", PathToZfsUtility, zfsSnapshotStartInfo.Arguments );
            return false;
        }

        Logger.Debug( "Calling `{0} {1}`", PathToZfsUtility, zfsSnapshotStartInfo.Arguments );
        try
        {
            using ( Process? snapshotProcess = Process.Start( zfsSnapshotStartInfo ) )
            {
                Logger.Debug( "Waiting for {0} {1} to finish", PathToZfsUtility, zfsSnapshotStartInfo.Arguments );
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
    public override async Task<bool> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings )
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
    public override async Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, ZfsRecord> datasets )
    {
        Logger.Debug( "Requested pool root capacities" );
        bool errorsEncountered = false;
        await foreach ( string zpoolListLine in ZpoolExecEnumerator( "list", $"-Hpo name,{ZpoolProperty.ZfsPoolCapacityPropertyName} {string.Join( ' ', datasets.Keys )}" ) )
        {
            string[] lineTokens = zpoolListLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
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
    /// <exception cref="ArgumentException">If name validation fails for <paramref name="zfsPath" /></exception>
    public override bool SetZfsProperties( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        // Ignoring the ArgumentOutOfRangeException that this throws because it's not possible here
        // ReSharper disable once ExceptionNotDocumentedOptional
        if ( !ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, zfsPath ) )
        {
            throw new ArgumentException( $"Unable to update schema for {zfsPath}. PropertyName is invalid.", nameof( zfsPath ) );
        }

        return PrivateSetZfsProperty( dryRun, zfsPath, properties );
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        if ( properties.Count == 0 )
        {
            Logger.Debug( "Asked to set properties for {0} but no properties provided", zfsPath );
            return false;
        }

        string propertiesSetString = properties.ToStringForZfsSet( );
        Logger.Trace( "Attempting to set properties on {0}: {1}", zfsPath, propertiesSetString );
        ProcessStartInfo zfsSetStartInfo = new( PathToZfsUtility, $"set {propertiesSetString} {zfsPath}" )
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

    /// <inheritdoc />
    public override async Task<ConcurrentDictionary<string, ZfsRecord>> GetPoolRootDatasetsWithAllRequiredSnapsInAZfsPropertiesAsync( )
    {
        ConcurrentDictionary<string, ZfsRecord> result = new( );
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
        // for SnapsInAZfs to depend on.
        // The user can, of course, break things, if they want to, but that's their own fault.
        // That's why SnapsInAZfs will do at least this check on every startup.
        // The run-time cost is minimal, so it's better to be safe, even if a cached configuration is likely to
        // be correct the majority of the time.
        // Comments, corrections, rude commentary, etc. are welcome (but not too rude - this is a labor of love😅).
        await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync( "get", $"{string.Join( ',', IZfsProperty.DefaultDatasetProperties.Keys )} -t filesystem -s local,default,none -Hpd 0" ).ConfigureAwait( true ) )
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
                result[ dsName ] = new( dsName, ZfsPropertyValueConstants.FileSystem );
            }

            Logger.Debug( "Adding property {0}({1} , {2}) to {3}", propertyName, propertyValue, propertyValueSource, dsName );
            result[ dsName ].UpdateProperty( propertyName, propertyValue, propertyValueSource );
        }

        Logger.Debug( "Pool root configuration retrieved" );
        return result;
    }

    /// <inheritdoc />
    /// <exception cref="OverflowException">rawObjects contains too many elements.</exception>
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string propertiesString = IZfsProperty.KnownDatasetProperties.Union(IZfsProperty.KnownSnapshotProperties).ToCommaSeparatedSingleLineString( );
        ConfiguredCancelableAsyncEnumerable<string> lineProvider = ZfsExecEnumeratorAsync( "get", $"type,{propertiesString},available,used -H -p -r -t filesystem,volume,snapshot" ).ConfigureAwait( true );
        ConcurrentDictionary<string, RawZfsObject> rawObjects = new( );
        await GetRawZfsObjectsAsync( lineProvider, rawObjects ).ConfigureAwait( true );
        ProcessRawObjects( rawObjects, datasets, snapshots);
        CheckAndUpdateLastSnapshotTimesForDatasets( settings, datasets );
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
    /// <exception cref="ArgumentNullException"><paramref name="verb" /> is <see langword="null" />.</exception>
    public override async IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        ArgumentException.ThrowIfNullOrEmpty( nameof( verb ), "Verb cannot be null or empty" );
        ValidateCommonExecArguments( verb, args );

        if ( verb is not "get" and not "list" )
        {
            throw new ArgumentOutOfRangeException( nameof( verb ), "Only get and list verbs are permitted for zfs enumerator operations" );
        }

        ProcessStartInfo zfsProcessStartInfo = new( PathToZfsUtility, $"{verb} {args}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        Logger.Trace( "Preparing to execute `{0} {1} {2}` and yield an enumerator for output", PathToZfsUtility, verb, args );
        using ( Process zfsProcess = new( ) { StartInfo = zfsProcessStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", zfsProcessStartInfo.FileName, zfsProcessStartInfo.Arguments );
            try
            {
                zfsProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                // Log this, but re-throw, because this is likely fatal, depending on call site
                Logger.Error( ioex, "Error running zfs get operation. The error returned was {0}", ioex.Message );
                throw;
            }

            while ( !zfsProcess.StandardOutput.EndOfStream )
            {
                yield return await zfsProcess.StandardOutput.ReadLineAsync( ).ConfigureAwait( true ) ?? throw new IOException( "Invalid attempt to read when no data present" );
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

    public override async Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets )
    {
        List<ITreeNode> treeRootNodes = new( );
        ConcurrentDictionary<string, TreeNode> allTreeNodes = new( );
        await foreach ( string zfsLine in ZfsExecEnumeratorAsync( "get", $"type,{IZfsProperty.KnownDatasetProperties.ToCommaSeparatedSingleLineString( )} -Hpt filesystem -d 0" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

            ParsePoolRootDatasetZfsGetLineForConfigConsoleTree( baseDatasets, treeDatasets, lineTokens, treeRootNodes, allTreeNodes );
        }

        await foreach ( string zfsLine in ZfsExecEnumeratorAsync( "get", $"type,{IZfsProperty.KnownDatasetProperties.ToCommaSeparatedSingleLineString( )} -Hprt filesystem,volume {string.Join( ' ', baseDatasets.Keys )}" ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

            ParseDatasetZfsGetLineForConfigConsoleTree( baseDatasets, treeDatasets, lineTokens, allTreeNodes );
        }

        return treeRootNodes;
    }

    /// <inheritdoc />
    public override async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( )
    {
        string zfsGetArgs = $"{IZfsProperty.KnownDatasetProperties.ToCommaSeparatedSingleLineString( )} -Hpt filesystem -d 0";
        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> rootsAndTheirProperties = new( );
        await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync( "get", zfsGetArgs ).ConfigureAwait( true ) )
        {
            string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            ParseAndValidatePoolRootZfsGetLine( lineTokens, rootsAndTheirProperties );
        }

        return rootsAndTheirProperties;
    }

    /// <inheritdoc />
    public override bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string dsName, string[] properties )
    {
        if ( properties.Length == 0 )
        {
            Logger.Warn( "Asked to set properties for {0} but no properties provided", dsName );
            return false;
        }

        string propertiesSetString = string.Join( ' ', properties.Select( propName => IZfsProperty.DefaultDatasetProperties[ propName ].SetString ).ToList( ) );
        Logger.Trace( "Attempting to set properties on {0}: {1}", dsName, propertiesSetString );
        ProcessStartInfo zfsSetStartInfo = new( PathToZfsUtility, $"set {propertiesSetString} {dsName}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        if ( dryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", zfsSetStartInfo.FileName, zfsSetStartInfo.Arguments );
            return false;
        }

        using ( Process zfsSetProcess = new( ) { StartInfo = zfsSetStartInfo } )
        {
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

    /// <inheritdoc cref="SetZfsProperties(bool,string,SnapsInAZfs.Interop.Zfs.ZfsTypes.IZfsProperty[])" />
    /// <remarks>
    ///     Does not perform name validation
    /// </remarks>
    private bool PrivateSetZfsProperty( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        if ( properties.Length == 0 )
        {
            Logger.Trace( "No properties to set" );
            return false;
        }

        string propertiesToSet = properties.ToStringForZfsSet( );
        Logger.Trace( "Attempting to set properties on {0}: {1}", zfsPath, propertiesToSet );
        ProcessStartInfo zfsSetStartInfo = new( PathToZfsUtility, $"set {propertiesToSet} {zfsPath}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        if ( dryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", zfsSetStartInfo.FileName, zfsSetStartInfo.Arguments );
            return false;
        }

        using ( Process zfsSetProcess = new( ) { StartInfo = zfsSetStartInfo } )
        {
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
}
