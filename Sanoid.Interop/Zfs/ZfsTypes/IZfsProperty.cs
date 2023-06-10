// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

public interface IZfsProperty
{
    string Name { get; }
    string Source { get; }
    string ValueString { get; }
    string SetString { get; }
    bool IsInherited { get; }
    string InheritedFrom { get; }
    bool IsLocal { get; }

    /// <summary>
    ///     Gets whether this is a sanoid property or not
    /// </summary>
    /// <remarks>Set by constructor, if property name begins with "sanoid.net:"</remarks>
    bool IsSanoidProperty { get; }
}
