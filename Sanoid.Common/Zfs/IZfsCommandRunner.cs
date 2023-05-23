// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using System.Text.Json;

namespace Sanoid.Common.Zfs;

/// <summary>
///     Interface for classes that call native ZFS utilities from the system.
/// </summary>
/// <remarks>
///     Default implementations of command functions return mocked values.
/// </remarks>
public interface IZfsCommandRunner
{
    /// <summary>
    ///     Gets a list of ZFS datasets (filesystems and volumes)
    /// </summary>
    /// <returns>
    ///     A <see cref="List{T}" /> of <see langword="string" />s, each representing the ZFS path of a dataset on the system.
    /// </returns>
    /// <remarks>
    ///     The default implementation does not actually call any external methods and simply returns a pre-defined list of
    ///     fake dataset<br />
    ///     strings, for use in unit tests or to provide a basic, non-destructive operation when implementing
    ///     <see cref="IZfsCommandRunner" /><br />
    ///     in new classes.
    /// </remarks>
    ImmutableSortedSet<string> ZfsListAll( )
    {
        ImmutableSortedSet<string> dataSets = ImmutableSortedSet<string>.Empty.Union( new[] { "pool1", "pool1/dataset1", "pool1/dataset1/leaf", "pool1/dataset2", "pool1/dataset3", "pool1/zvol1" } );
        LogManager.GetCurrentClassLogger( ).Warn( "Running on windows. Returning fake datasets: {0}", JsonSerializer.Serialize( dataSets ) );
        return dataSets;
    }

    /// <summary>
    ///     Calls ZFS snapshot, using the provided configuration and parameters
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    bool ZfsSnapshot( Configuration.Datasets.Dataset snapshotParent, string snapshotName );
}
