// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration.Templates;

namespace Sanoid.Common.Zfs;

/// <summary>
/// </summary>
public class ZfsCommandRunner : IZfsCommandRunner
{
    /// <summary>
    ///     Creates a new instance of the standard <see cref="ZfsCommandRunner" /> class, which uses configured platform
    ///     commands to get information from ZFS
    /// </summary>
    /// <param name="platformUtilitiesConfigurationSection">
    ///     An IConfigurationSection referring directly to a
    ///     "PlatformUtilities" section of Sanoid.net's configuration.
    /// </param>
    public ZfsCommandRunner( IConfigurationSection platformUtilitiesConfigurationSection )
    {
        _platformUtilitiesConfigurationSection = platformUtilitiesConfigurationSection;
    }

    private readonly Logger _logger = LogManager.GetCurrentClassLogger( );
    private readonly IConfigurationSection _platformUtilitiesConfigurationSection;

    /// <summary>
    ///     Calls zfs snapshot to create a snapshot of <paramref name="snapshotParent" />.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IZfsObject" /> provided in <paramref name="snapshotParent" /> is expected to be fully configured,
    ///     with all associated <see cref="Template" />s applied and the full tree path of parents back to a
    ///     <see cref="Zpool" />
    ///     already built.
    /// </remarks>
    /// <param name="snapshotParent"></param>
    /// <param name="snapshotName"></param>
    public bool ZfsSnapshot( Configuration.Datasets.Dataset snapshotParent, string snapshotName )
    {
        string zfsCommand = _platformUtilitiesConfigurationSection[ "zfs" ]!;
        string arguments = $"snapshot {snapshotParent.Path}@{snapshotName}";
        _logger.Debug( "Calling `{0} {1}`", zfsCommand, arguments );
        ProcessStartInfo zfsSnapshotStartInfo = new( zfsCommand, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = false
        };
        try
        {
            using ( Process? snapshotProcess = Process.Start( zfsSnapshotStartInfo ) )
            {
                _logger.Debug( "Waiting for {0} {1} to finish", zfsCommand, arguments );
                snapshotProcess?.WaitForExit( );
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
    ///     Gets the output of `zfs list -o name -t ` with the types of objects set in <paramref name="types" /> appended
    /// </summary>
    /// <param name="types">A <see cref="ZfsListObjectTypes" /> with flags set for each desired object type.</param>
    /// <returns>An <see cref="ImmutableSortedSet{T}" /> of <see langword="string" />s containing the output of the command</returns>
    public ImmutableSortedSet<string> ZfsListAll( ZfsListObjectTypes types = ZfsListObjectTypes.FileSystem | ZfsListObjectTypes.Volume )
    {
        ImmutableSortedSet<string>.Builder dataSets = ImmutableSortedSet<string>.Empty.ToBuilder( );
        string typesToList = types.ToStringForCommandLine( );
        _logger.Debug( "Requested listing of all zfs objects of the following types: {0}", typesToList );
        ProcessStartInfo zfsListStartInfo = new( _platformUtilitiesConfigurationSection[ "zfs" ]!, $"list -o name -t {typesToList} -Hr" )
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
