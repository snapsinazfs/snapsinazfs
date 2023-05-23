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

    /// <inheritdoc />
    public ImmutableSortedSet<string> ZfsListAll( )
    {
        ImmutableSortedSet<string>.Builder dataSets = ImmutableSortedSet<string>.Empty.ToBuilder( );
        ProcessStartInfo zfsListStartInfo = new( _platformUtilitiesConfigurationSection[ "zfs" ]!, "list -o name -t filesystem,volume -Hr" )
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
                _logger.Fatal( "Error running zfs list operation. The error returned was {0}", ioex );
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

    /// <summary>
    ///     Calls zfs snapshot to create a snapshot of <paramref name="snapshotParent" />.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IZfsObject" /> provided in <paramref name="snapshotParent" /> is expected to be fully configured,
    ///     with all<br />
    ///     associated <see cref="Template" />s applied and the full tree path of parents back to a <see cref="Zpool" />
    ///     already built.
    /// </remarks>
    /// <param name="snapshotParent"></param>
    /// <param name="snapshotName"></param>
    public void ZfsSnapshot( Configuration.Datasets.Dataset snapshotParent, string snapshotName )
    {
        string zfsCommand = _platformUtilitiesConfigurationSection[ "zfs" ]!;
        string arguments = $"snapshot {snapshotParent.Path}@{snapshotName}";
        _logger.Debug( "Calling `{0} {1}`", zfsCommand, arguments );
        ProcessStartInfo zfsSnapshotStartInfo = new( zfsCommand, arguments )
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
    }
}
