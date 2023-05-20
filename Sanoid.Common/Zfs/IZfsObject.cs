// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Configuration.Templates;

namespace Sanoid.Common.Zfs;

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
    /// <summary>
    /// Gets the full name of the <see cref="IZfsObject"/>
    /// </summary>
    /// <value>A <see langword="string"/> value, containing the fully-qualified ZFS name of the object</value>
    string FullName => this is Zpool ? Name : Path.Combine( Parent!.FullName, Name );

    /// <summary>
    /// Gets the Parent of this <see cref="IZfsObject"/>.
    /// </summary>
    /// <remarks>
    /// If this <see cref="IZfsObject"/> is a <see cref="Zpool"/>, this property will be null.
    /// </remarks>
    /// <value>
    /// If this object is a <see cref="Zpool"/>: null<br/>
    /// Otherwise, a reference to the parent <see cref="IZfsObject"/>
    /// </value>
    IZfsObject? Parent { get; }
}
