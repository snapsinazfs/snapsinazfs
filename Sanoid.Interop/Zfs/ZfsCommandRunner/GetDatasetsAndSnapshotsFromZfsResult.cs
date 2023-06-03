// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using Sanoid.Interop.Libc.Enums;
using Sanoid.Interop.Zfs.ZfsTypes;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

public sealed class GetDatasetsAndSnapshotsFromZfsResult
{
    public Errno Status { get; set; }
    public ConcurrentDictionary<string, Dataset> Datasets { get; } = new( );
    public ConcurrentDictionary<string, Snapshot> Snapshots { get; } = new( );
}
