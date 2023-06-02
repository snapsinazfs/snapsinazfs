// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
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
    public ZfsCommandRunner( string pathToZfs )
    {
        ZfsPath = pathToZfs;
    }

    private string ZfsPath { get; }

    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override bool TakeSnapshot( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot )
    {
        Logger.Debug( "Snapshot requested for dataset {0}", ds.Name );
        snapshot = Snapshot.GetSnapshotForCommandRunner( ds, period, timestamp, settings );
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
        ProcessStartInfo zfsSnapshotStartInfo = new( ZfsPath, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = false
        };
        if ( settings.DryRun )
        {
            Logger.Info( "DRY RUN: Would execute `{0} {1}`", ZfsPath, zfsSnapshotStartInfo.Arguments );
            return true;
        }

        Logger.Debug( "Calling `{0} {1}`", ZfsPath, arguments );
        try
        {
            using ( Process? snapshotProcess = Process.Start( zfsSnapshotStartInfo ) )
            {
                Logger.Debug( "Waiting for {0} {1} to finish", ZfsPath, arguments );
                snapshotProcess?.WaitForExit( );
                if ( snapshotProcess?.ExitCode == 0 )
                {
                    return true;
                }

                Logger.Error( "Snapshot creation failed for {0}", snapshot.Name );
            }

            return true;
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error running {0} {1}. Snapshot may not exist", zfsSnapshotStartInfo.FileName, zfsSnapshotStartInfo.Arguments );
            return false;
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, ZfsProperty> GetZfsProperties( ZfsObjectKind kind, string zfsObjectName, bool sanoidOnly = true )
    {
        if ( !ZfsObjectBase.ValidateName( kind, zfsObjectName ) )
        {
            throw new ArgumentException( $"Unable to get properties for {zfsObjectName}. PropertyName is invalid.", nameof( zfsObjectName ) );
        }

        ProcessStartInfo zfsGetStartInfo = new( ZfsPath, $"get all -o property,value,source -H {zfsObjectName}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        Dictionary<string, ZfsProperty> properties = new( );
        using ( Process zfsGetProcess = new( ) { StartInfo = zfsGetStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", (object)zfsGetStartInfo.FileName, (object)zfsGetStartInfo.Arguments );
            try
            {
                zfsGetProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Fatal( ioex, "Error running zfs get operation. The error returned was {0}" );
                throw;
            }

            while ( !zfsGetProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsGetProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "{0}", outputLine );
                if ( ZfsProperty.TryParse( outputLine, out ZfsProperty? property ) )
                {
                    properties.Add( property!.FullName, property );
                }
            }

            if ( !zfsGetProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs get process to exit" );
                zfsGetProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs get process finished" );
            return properties;
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">If name validation fails for <paramref name="zfsPath" /></exception>
    public override bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties )
    {
        // Ignoring the ArgumentOutOfRangeException that this throws because it's not possible here
        // ReSharper disable once ExceptionNotDocumentedOptional
        if ( !ZfsObjectBase.ValidateName( ZfsObjectKind.FileSystem, zfsPath ) )
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
    /// <exception cref="ArgumentNullException">If a <see langword="null" /> string is somehow provided to ContainsKey when looking for existing entries in the dictionary to return.</exception>
    /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer when parsing zfs output.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="Dictionary`2" />.</exception>
    public override Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" )
    {
        Dictionary<string, Dataset> datasets = new( );

        Logger.Debug( "Getting ZFS dataset configurations" );
        ProcessStartInfo zfsGetStartInfo = new( ZfsPath, $"get all{args} -t filesystem,volume -H -p -o name,property,value,source" )
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
                Logger.Error( ioex, "Error running zfs list operation. The error returned was {0}" );
                throw;
            }

            while ( !zfsGetProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsGetProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "Read line {0} from zfs get", outputLine );
                string[] lineTokens = outputLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
                if ( lineTokens.Length < 4 )
                {
                    Logger.Error( "Line {0} not understood", outputLine );
                    throw new InvalidOperationException( $"Unable to parse dataset configuration. Expected 4 tokens in output. Got {lineTokens.Length}: [{outputLine}]" );
                }

                if ( !datasets.ContainsKey( lineTokens[ 0 ] ) )
                {
                    if ( lineTokens[ 1 ] == "type" )
                    {
                        Logger.Debug( "Adding new Dataset {0} to collection", lineTokens[ 0 ] );
                        DatasetKind newDsKind = lineTokens[ 2 ] switch
                        {
                            "filesystem" => DatasetKind.FileSystem,
                            "volume" => DatasetKind.Volume,
                            _ => throw new InvalidOperationException( "Type of object from zfs get was unrecognized" )
                        };
                        Logger.Debug( "Dataset {0} will be a {1}", lineTokens[ 0 ], newDsKind );

                        Dataset dataset = new( lineTokens[ 0 ], newDsKind );
                        datasets.Add( lineTokens[ 0 ], dataset );
                    }
                }
                else
                {
                    Logger.Trace( "Checking if property {0} is wanted by sanoid", lineTokens[ 1 ] );
                    if ( ZfsProperty.KnownDatasetProperties.Contains( lineTokens[ 1 ] ) || lineTokens[ 1 ] == "snapshot_limit" || lineTokens[ 1 ] == "snapshot_count" )
                    {
                        Logger.Trace( "Property {0} is wanted by sanoid. Adding new property {0} to Dataset {1}", lineTokens[ 1 ], lineTokens[ 0 ] );
                        datasets[ lineTokens[ 0 ] ].AddProperty( ZfsProperty.Parse( lineTokens[ 1.. ] ) );
                    }
                    else
                    {
                        Logger.Trace( "Property {0} is not wanted by sanoid. Ignoring", lineTokens[ 1 ] );
                    }
                }

                Logger.Trace( "Finished with line {0} from zfs get", outputLine );
            }

            if ( !zfsGetProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs list process to exit" );
                zfsGetProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs list process finished" );
            return datasets;
        }
    }

    /// <inheritdoc />
    public override Dictionary<string, Dataset> GetZfsPoolRoots( )
    {
        Logger.Debug( "Requested pool root configuration" );
        Dictionary<string, Dataset> poolRoots = GetZfsDatasetConfiguration( " -d 0" );
        Logger.Debug( "Pool root configuration retrieved" );
        return poolRoots;
    }

    /// <param name="datasets"></param>
    /// <inheritdoc />
    public override Dictionary<string, Snapshot> GetZfsSanoidSnapshots( ref Dictionary<string, Dataset> datasets )
    {
        Dictionary<string, Snapshot> snapshots = new( );

        Logger.Debug( "Getting ZFS snapshot configurations" );
        ProcessStartInfo zfsListStartInfo = new( ZfsPath, $"list -r -t snapshot -H -p -o {string.Join( ',', SnapshotProperty.KnownSnapshotProperties )}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsListProcess = new( ) { StartInfo = zfsListStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", (object)zfsListStartInfo.FileName, (object)zfsListStartInfo.Arguments );
            try
            {
                zfsListProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Error( ioex, "Error running zfs list operation. The error returned was {0}" );
                throw;
            }

            if ( zfsListProcess is { HasExited: true, ExitCode: 2 } )
            {
                const string errorMessage = "Missing snapshot properties. Cannot get snapshots from ZFS";
                Logger.Error( errorMessage );
                throw new InvalidOperationException( errorMessage );
            }

            while ( !zfsListProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsListProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "Read line {0} from zfs list", outputLine );
                string[] lineTokens = outputLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );
                if ( lineTokens.Length < SnapshotProperty.KnownSnapshotProperties.Count )
                {
                    Logger.Error( "Line {0} not understood", outputLine );
                    throw new InvalidOperationException( $"Unable to parse snapshot output. Expected {SnapshotProperty.KnownSnapshotProperties.Count} tokens in output. Got {lineTokens.Length}: [{outputLine}]" );
                }

                if ( lineTokens[ 2 ] == "-" )
                {
                    Logger.Trace( "Output line is not a sanoid.net snapshot - skipping" );
                    continue;
                }

                Snapshot snap = Snapshot.FromListSnapshots( lineTokens );
                snapshots.TryAdd( snap.Name, snap );
                datasets[ snap.DatasetName ].AddSnapshot( snap );

                Logger.Trace( "Finished with line {0} from zfs list", outputLine );
            }

            if ( !zfsListProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs list process to exit" );
                zfsListProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs list process finished" );
            return snapshots;
        }
    }

    /// <summary>
    ///     Gets the output of `zfs list -o name -t ` with the kind of objects set in <paramref name="kind" /> appended
    /// </summary>
    /// <param name="kind">A <see cref="ZfsObjectKind" /> with flags set for each desired object type.</param>
    /// <returns>An <see cref="ImmutableSortedSet{T}" /> of <see langword="string" />s containing the output of the command</returns>
    public ImmutableSortedSet<string> ZfsListAll( ZfsObjectKind kind = ZfsObjectKind.FileSystem | ZfsObjectKind.Volume )
    {
        ImmutableSortedSet<string>.Builder dataSets = ImmutableSortedSet<string>.Empty.ToBuilder( );
        string typesToList = kind.ToStringForCommandLine( );
        Logger.Debug( "Requested listing of all zfs objects of the following kind: {0}", typesToList );
        ProcessStartInfo zfsListStartInfo = new( ZfsPath, $"list -o name -t {typesToList} -Hr" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsListProcess = new( ) { StartInfo = zfsListStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", (object)zfsListStartInfo.FileName, (object)zfsListStartInfo.Arguments );
            try
            {
                zfsListProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Fatal( ioex, "Error running zfs list operation. The error returned was {0}" );
                throw;
            }

            while ( !zfsListProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsListProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "{0}", outputLine );
                dataSets.Add( outputLine );
            }

            if ( !zfsListProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs list process to exit" );
                zfsListProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs list process finished" );
        }

        return dataSets.ToImmutable( );
    }

    /// <exception cref="ArgumentNullException">If <paramref name="zfsPath" /> is null, empty, or only whitespace</exception>
    public UpdateZfsPropertySchemaResult UpdateZfsPropertySchema( string zfsPath, bool dryRun )
    {
        // Ignoring the ArgumentOutOfRangeException that this throws because it's not possible here
        // ReSharper disable once ExceptionNotDocumentedOptional
        if ( !ZfsObjectBase.ValidateName( ZfsObjectKind.FileSystem, zfsPath ) )
        {
            throw new ArgumentException( $"Unable to update schema for {zfsPath}. PropertyName is invalid.", nameof( zfsPath ) );
        }

        UpdateZfsPropertySchemaResult result = new( )
        {
            ExistingProperties = GetZfsProperties( ZfsObjectKind.FileSystem, zfsPath )
        };

        Dictionary<string, ZfsProperty> propertiesToAdd = new( );

        foreach ( ( string key, ZfsProperty prop ) in ZfsProperty.DefaultDatasetProperties )
        {
            if ( result.ExistingProperties.ContainsKey( key ) )
            {
                continue;
            }

            propertiesToAdd.Add( key, prop );
        }

        PrivateSetZfsProperty( dryRun, zfsPath, propertiesToAdd.Values.ToArray( ) );
        result.AddedProperties = propertiesToAdd;
        return result;
    }

    /// <inheritdoc cref="SetZfsProperties" />
    /// <remarks>
    ///     Does not perform name validation
    /// </remarks>
    private bool PrivateSetZfsProperty( bool dryRun, string zfsPath, params ZfsProperty[] properties )
    {
        string propertiesToSet = string.Join( ' ', properties.Select( p => p.SetString ) );
        Logger.Trace( "Attempting to set properties on {0}: {1}", zfsPath, propertiesToSet );
        ProcessStartInfo zfsSetStartInfo = new( ZfsPath, $"set {propertiesToSet} {zfsPath}" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsSetProcess = new( ) { StartInfo = zfsSetStartInfo } )
        {
            if ( dryRun )
            {
                Logger.Info( "DRY RUN: Would execute `{0} {1}`", zfsPath, zfsSetStartInfo.Arguments );
                return true;
            }

            Logger.Debug( "Calling {0} {1}", (object)zfsSetStartInfo.FileName, (object)zfsSetStartInfo.Arguments );
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

    public List<string> GetZfsRootNames( )
    {
        List<string> names = new( );

        Logger.Debug( "Getting all ZFS root datasets" );
        ProcessStartInfo zfsListStartInfo = new( ZfsPath, "list -d 0 -H -o name" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsListProcess = new( ) { StartInfo = zfsListStartInfo } )
        {
            Logger.Debug( "Calling {0} {1}", (object)zfsListStartInfo.FileName, (object)zfsListStartInfo.Arguments );
            try
            {
                zfsListProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                Logger.Error( ioex, "Error running zfs list operation. The error returned was {0}" );
                throw;
            }

            while ( !zfsListProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsListProcess.StandardOutput.ReadLine( )!;
                Logger.Trace( "{0}", outputLine );
                names.Add( outputLine.Trim( ) );
            }

            if ( !zfsListProcess.HasExited )
            {
                Logger.Trace( "Waiting for zfs list process to exit" );
                zfsListProcess.WaitForExit( 3000 );
            }

            Logger.Debug( "zfs list process finished" );
            return names;
        }
    }
}
