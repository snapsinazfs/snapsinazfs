// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Snapshots;

/// <summary>
///     Base type for snapshot templates
/// </summary>
public abstract class SnapshotTemplateBase
{
    /// <summary>
    ///     Gets or sets the template name
    /// </summary>
    /// <value>
    ///     A <see langword="string" /> value that corresponds to a template's name, which are represented as sections in
    ///     ini configuration files.
    /// </value>
    public string Name { get; set; } = "default";

    /// <summary>
    ///     Gets or sets the ZFS path
    /// </summary>
    /// <value>A <see langword="string" /> value of the ZFS path this template refers to.</value>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether this template processes children only
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> value indicating whether this template applies to the <see cref="Path">Path</see> (
    ///     <see langword="false" />) or only to its children (<see langword="true" />)
    /// </value>
    public bool ProcessChildrenOnly { get; set; }

    /// <summary>
    ///     Gets or sets whether this template uses recursive processing
    /// </summary>
    /// <value>A <see langword="bool" /> indicating whether this template uses recursing processing</value>
    public bool Recursive { get; set; }

    /// <summary>
    ///     Gets or sets whether this template skips children
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> value indicating whether this template skips children of the
    ///     <see cref="Path">Path</see> (<see langword="true" />) or not (<see langword="false" />)
    /// </value>
    public bool SkipChildren { get; set; }
}