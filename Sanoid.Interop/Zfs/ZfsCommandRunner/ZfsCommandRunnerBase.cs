// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;
using Sanoid.Interop.Zfs.ZfsTypes;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

public abstract class ZfsCommandRunnerBase : IZfsCommandRunner
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public abstract bool TakeSnapshot( string snapshotName );

    /// <inheritdoc />
    public abstract Dictionary<string, ZfsProperty> GetZfsProperties( ZfsObjectKind kind, string zfsObjectName, bool sanoidOnly = true );

    /// <inheritdoc />
    public abstract bool SetZfsProperty( string zfsPath, params ZfsProperty[] properties );

    /// <inheritdoc />
    public abstract Dictionary<string, Dataset> GetZfsDatasetConfiguration( );
}
