// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Zfs;
using Dataset = Sanoid.Common.Configuration.Datasets.Dataset;

namespace Sanoid;

/// <summary>
///     A dummy implementation of <see cref="IZfsCommandRunner" /> for use in development or if run from an unsupported
///     platform
/// </summary>
internal class DummyZfsCommandRunner : IZfsCommandRunner
{
    /// <summary>
    ///     Creates a new instance of a dummy command runner that never actually runs ZFS commands.
    /// </summary>
    public DummyZfsCommandRunner( )
    {
        Logger.Warn( "USING DUMMY ZFS COMMAND RUNNER. NO REAL ZFS COMMANDS WILL BE EXECUTED." );
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public bool ZfsSnapshot( Dataset snapshotParent, string snapshotName )
    {
        return true;
    }
}
