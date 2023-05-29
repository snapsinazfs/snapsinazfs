// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS snapshot
/// </summary>
public class Snapshot : ZfsObjectBase
{
    /// <summary>
    ///     Creates a new <see cref="Snapshot" /> with the given name and an empty <see cref="ZfsObjectBase.Properties" />
    ///     collection
    /// </summary>
    /// <param name="name">The final component of the name of the Snapshot</param>
    public Snapshot( string name )
        : base( name, ZfsObjectKind.Snapshot )
    {
    }

    private Snapshot( string parentPath, string name )
        : base( name, ZfsObjectKind.Snapshot )
    {
        ParentPath = parentPath;
    }

    public static Snapshot Invalid => new( "@@INVALID@@" )
    {
        Properties =
        {
            [ "sanoid.net:invalid" ] = new( "sanoid.net:", "invalid", "true", "sanoid" )
        }
    };

    /// <summary>
    ///     Gets or sets the path of the parent object, without trailing '/'
    /// </summary>
    /// <remarks>
    ///     Is not validated
    /// </remarks>
    /// <value>
    ///     <see langword="null" /> or a string representing the path of the <see cref="Dataset" /> the snapshot belongs to
    /// </value>
    public string? ParentPath { get; set; }
}
