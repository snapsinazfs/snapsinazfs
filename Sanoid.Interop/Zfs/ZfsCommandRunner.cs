// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using System.Diagnostics;
using NLog;

namespace Sanoid.Interop.Zfs;

/// <summary>
/// </summary>
public class ZfsCommandRunner : IZfsCommandRunner
{
    /// <summary>
    ///     Creates a new instance of the standard <see cref="ZfsCommandRunner" /> class, which uses calls zfs at the path provided in <paramref name="pathToZfs"/>
    /// </summary>
    /// <param name="pathToZfs">
    ///     A fully-qualified path to the zfs executable
    /// </param>
    public ZfsCommandRunner( string pathToZfs )
    {
        ZfsPath = pathToZfs;
    }

    private string ZfsPath { get; }

    private readonly Logger _logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public bool ZfsSnapshot( string snapshotName )
    {
        string arguments = $"snapshot {snapshotName}";
        _logger.Debug( "Calling `{0} {1}`", ZfsPath, arguments );
        ProcessStartInfo zfsSnapshotStartInfo = new( ZfsPath, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = false
        };
        try
        {
            using ( Process? snapshotProcess = Process.Start( zfsSnapshotStartInfo ) )
            {
                _logger.Debug( "Waiting for {0} {1} to finish", ZfsPath, arguments );
                snapshotProcess?.WaitForExit( );
                if ( snapshotProcess?.ExitCode == 0 )
                {
                    return true;
                }

                _logger.Error( "Snapshot creation failed for {0}", snapshotName );
            }

            return true;
        }
        catch ( Exception e )
        {
            _logger.Error( e, "Error running {0} {1}. Snapshot may not exist", zfsSnapshotStartInfo.FileName, zfsSnapshotStartInfo.Arguments );
            return false;
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
        _logger.Debug( "Requested listing of all zfs objects of the following kind: {0}", typesToList );
        ProcessStartInfo zfsListStartInfo = new( ZfsPath, $"list -o name -t {typesToList} -Hr" )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using ( Process zfsListProcess = new( ) { StartInfo = zfsListStartInfo } )
        {
            _logger.Debug( "Calling {0} {1}", (object)zfsListStartInfo.FileName, (object)zfsListStartInfo.Arguments );
            try
            {
                zfsListProcess.Start( );
            }
            catch ( InvalidOperationException ioex )
            {
                _logger.Fatal( ioex, "Error running zfs list operation. The error returned was {0}" );
                throw;
            }

            while ( !zfsListProcess.StandardOutput.EndOfStream )
            {
                string outputLine = zfsListProcess.StandardOutput.ReadLine( )!;
                _logger.Trace( "{0}", outputLine );
                dataSets.Add( outputLine );
            }

            if ( !zfsListProcess.HasExited )
            {
                _logger.Trace( "Waiting for zfs list process to exit" );
                zfsListProcess.WaitForExit( 3000 );
            }

            _logger.Debug( "zfs list process finished" );
        }

        return dataSets.ToImmutable( );
    }
}
