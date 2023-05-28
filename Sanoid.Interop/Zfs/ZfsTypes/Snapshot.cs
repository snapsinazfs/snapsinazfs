// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS snapshot
/// </summary>
public class Snapshot : IZfsObject
{
    /// <summary>
    ///     Creates a new <see cref="Snapshot" /> with the given name
    /// </summary>
    /// <param name="name">The final component of the name of the Snapshot</param>
    public Snapshot(string name)
    {
        Name = name;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ZfsObjectKind ZfsKind { get; }

    /// <inheritdoc />
    public ConcurrentDictionary<string, ZfsProperty> Properties { get; private set; }
}
