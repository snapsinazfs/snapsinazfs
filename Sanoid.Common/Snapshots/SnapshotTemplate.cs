// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Snapshots;

/// <summary>
///     Represents a snapshot template
/// </summary>
public class SnapshotTemplate : SnapshotTemplateBase
{
    /// <summary>
    ///     Gets or sets a parent template to apply to this template, for default settings
    /// </summary>
    /// <value>
    ///     The <see langword="string" /> <see cref="SnapshotTemplateBase.Name">Name</see> of the parent template that this
    ///     template inherits from
    /// </value>
    public string? UseTemplate { get; set; }
}