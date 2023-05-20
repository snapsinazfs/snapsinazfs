// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Configuration.Templates;

namespace Sanoid.Common.Zfs;

/// <summary>
///     A ZFS snapshot
/// </summary>
public class Snapshot : IZfsObject
{
    /// <summary>
    ///     Creates a new <see cref="Snapshot" /> with the given name and parent <see cref="IZfsObject" />
    /// </summary>
    /// <param name="name">The final component of the name of the Snapshot</param>
    /// <param name="parent">The parent <see cref="IZfsObject" /> the Snapshot belongs to</param>
    public Snapshot( string name, IZfsObject parent )
    {
        Name = name;
        Parent = parent;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IZfsObject? Parent { get; }

    public Template? Template { get; set; }
}
