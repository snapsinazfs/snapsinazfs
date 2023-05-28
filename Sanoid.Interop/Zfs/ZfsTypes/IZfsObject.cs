// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
/// An interface for objects implementing basic common properties for ZFS objects
/// </summary>
public interface IZfsObject
{
    /// <summary>
    /// Gets or sets the name of the <see cref="IZfsObject"/>
    /// </summary>
    /// <value>A <see langword="string"/> value, containing the final component of the name of the object</value>
    string Name { get; }

    ZfsObjectKind ZfsKind { get; }

    /// <summary>
    /// A dcitionary of property names and their values, as strings
    /// </summary>
    ConcurrentDictionary<string, string> Properties { get; }
}
