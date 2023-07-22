// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

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

    private static string PathToZfsUtility { get; set; } = "/usr/sbin/zfs";
    private static string PathToZpoolUtility { get; set; } = "/usr/sbin/zpool";

    /// <inheritdoc />
    public override ZfsCommandRunnerOperationStatus TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot )
    {
        bool zfsRecursionWanted = ds.Recursion.Value == ZfsPropertyValueConstants.ZfsRecursion;
        Logger.Debug( "{0} {2}snapshot requested for dataset {1}", period, ds.Name, zfsRecursionWanted ? "recursive " : "" );
        snapshot = ds.CreateSnapshot( in period, in timestamp, in datasetTemplate );
        try
        {
            // This exception is only thrown if kind is invalid. We're passing a known good value.
            // ReSharper disable once ExceptionNotDocumentedOptional
            if ( !snapshot.ValidateName( ) )
            {
                Logger.Error( "Snapshot name {0} is invalid. Snapshot not taken", snapshot.Name );
                return ZfsCommandRunnerOperationStatus.NameValidationFailed;
            }
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Error( ex, "Snapshot name {0} is invalid. Snapshot not taken", snapshot.Name );
            return ZfsCommandRunnerOperationStatus.NameValidationFailed;
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
            return ZfsCommandRunnerOperationStatus.DryRun;
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
                    return ZfsCommandRunnerOperationStatus.Success;
                }

                Logger.Error( "Snapshot creation failed for {0}", snapshot.Name );
            }

            return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error running {0} {1}. Snapshot may not exist", zfsSnapshotStartInfo.FileName, zfsSnapshotStartInfo.Arguments );
            return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
        }
    }

    /// <inheritdoc />
    public override async Task<ZfsCommandRunnerOperationStatus> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings )
    {
        Logger.Debug( "Requested to destroy snapshot {0}", snapshot.Name );
        try
        {
            // This exception is only thrown if kind is invalid. We're passing a known good value.
            // ReSharper disable once ExceptionNotDocumentedOptional
            if ( !snapshot.ValidateName( ) )
            {
                Logger.Error( "Snapshot name {0} is invalid. Snapshot not destroyed", snapshot.Name );
                return ZfsCommandRunnerOperationStatus.NameValidationFailed;
            }
        }
        catch ( ArgumentNullException ex )
        {
            Logger.Error( ex, "Snapshot name {0} is invalid. Snapshot not destroyed", snapshot.Name );
            return ZfsCommandRunnerOperationStatus.NameValidationFailed;
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
            return ZfsCommandRunnerOperationStatus.DryRun;
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
                        return ZfsCommandRunnerOperationStatus.Success;
                    }
                }

                Logger.Error( "Destroy snapshot failed for {0}", snapshot.Name );
            }

            return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error running {0} {1}. Snapshot may still exist", zfsDestroyStartInfo.FileName, zfsDestroyStartInfo.Arguments );
            return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
        }
    }

    /// <inheritdoc />
    public override async Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        // Ignoring the ArgumentOutOfRangeException that this throws because it's not possible here
        // ReSharper disable once ExceptionNotDocumentedOptional
        if ( !ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, zfsPath ) )
        {
            return ZfsCommandRunnerOperationStatus.NameValidationFailed;
        }

        return await PrivateSetZfsPropertyAsync( dryRun, zfsPath, properties ).ConfigureAwait( true );
    }

    /// <inheritdoc />
    public override async Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        if ( properties.Count == 0 )
        {
            Logger.Debug( "Asked to set properties for {0} but no properties provided", zfsPath );
            return ZfsCommandRunnerOperationStatus.ZeroLengthRequest;
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
                return ZfsCommandRunnerOperationStatus.DryRun;
            }

            Logger.Debug( "Calling {0} {1}", zfsSetStartInfo.FileName, zfsSetStartInfo.Arguments );
            try
            {
                zfsSetProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Error( ioex, "Error running zfs set operation. Exit status: {0}", Marshal.GetLastSystemError( ) );
                return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
            }

            if ( !zfsSetProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs set process to exit" );
                await zfsSetProcess.WaitForExitAsync( ).ConfigureAwait( true );
            }

            Logger.Trace( "zfs set process finished" );
            return ZfsCommandRunnerOperationStatus.Success;
        }
    }

    /// <inheritdoc />
    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string propertiesString = IZfsProperty.KnownDatasetProperties.Union( IZfsProperty.KnownSnapshotProperties ).ToCommaSeparatedSingleLineString( );
        ConfiguredCancelableAsyncEnumerable<string> lineProvider = ZfsExecEnumeratorAsync( "get", $"type,{propertiesString},available,used -H -p -r -t filesystem,volume,snapshot" ).ConfigureAwait( true );
        SortedDictionary<string, RawZfsObject> rawObjects = new( );
        await GetRawZfsObjectsAsync( lineProvider, rawObjects ).ConfigureAwait( true );
        ProcessRawObjects( rawObjects, datasets, snapshots );
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
    public override async Task<ZfsCommandRunnerOperationStatus> InheritZfsPropertyAsync( bool dryRun, string zfsPath, IZfsProperty propertyToInherit )
    {
        if ( propertyToInherit.Owner is null )
        {
            Logger.Error( "Property has no reference to the dataset it belongs to. Cannot inherit." );
            return ZfsCommandRunnerOperationStatus.Failure;
        }
        Logger.Trace( "Attempting to inherit property {0} on {1} from {2}", propertyToInherit.Name, zfsPath, propertyToInherit.Owner.ParentDataset.Name );
        ProcessStartInfo zfsInheritStartInfo = new( PathToZfsUtility, $"inherit {propertyToInherit.Name} {zfsPath}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        if ( dryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", zfsInheritStartInfo.FileName, zfsInheritStartInfo.Arguments );
            return ZfsCommandRunnerOperationStatus.DryRun;
        }

        using ( Process zfsInheritProcess = new( ) { StartInfo = zfsInheritStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", zfsInheritStartInfo.FileName, zfsInheritStartInfo.Arguments );
            try
            {
                zfsInheritProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Error( ioex, "Error running zfs inherit operation for {0} on {1}", propertyToInherit.Name, propertyToInherit.Owner.Name );
                return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
            }

            if ( !zfsInheritProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs inherit process to exit" );
                using CancellationTokenSource tokenSource = new( 5000 );
                ConfiguredTaskAwaitable waitForExitAsync = zfsInheritProcess.WaitForExitAsync( tokenSource.Token ).ConfigureAwait( false );
                await waitForExitAsync;
            }

            Logger.Trace( "zfs inherit process finished" );
            return ZfsCommandRunnerOperationStatus.Success;
        }
    }

    /// <inheritdoc />
    public override bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string dsName, string[] properties )
    {
        if ( properties.Length == 0 )
        {
            Logger.Warn( "Asked to set properties for {0} but no properties provided", dsName );
            return false;
        }

        string propertiesSetString = properties.Select( propName => IZfsProperty.DefaultDatasetProperties[ propName ].SetString ).ToList( ).ToSpaceSeparatedSingleLineString( );
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

    /// <inheritdoc cref="SetZfsPropertiesAsync(bool,string,SnapsInAZfs.Interop.Zfs.ZfsTypes.IZfsProperty[])" />
    /// <remarks>
    ///     Does not perform name validation
    /// </remarks>
    private static async Task<ZfsCommandRunnerOperationStatus> PrivateSetZfsPropertyAsync( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        if ( properties.Length == 0 )
        {
            Logger.Trace( "No properties to set" );
            return ZfsCommandRunnerOperationStatus.ZeroLengthRequest;
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
            return ZfsCommandRunnerOperationStatus.DryRun;
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
                Logger.Error( ioex, "Error running zfs set operation. Exit status was {0}", Marshal.GetLastSystemError() );
                return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
            }

            if ( !zfsSetProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs set process to exit" );
                try
                {
                    await zfsSetProcess.WaitForExitAsync( ).ConfigureAwait( false );
                }
                catch ( Exception ex )
                {
                    Logger.Error( ex, "Error running zfs set operation. Exit status was {0}", Marshal.GetLastSystemError() );
                    return ZfsCommandRunnerOperationStatus.ZfsProcessFailure;
                }
            }

            Logger.Trace( "zfs set process finished" );
            return ZfsCommandRunnerOperationStatus.Success;
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
