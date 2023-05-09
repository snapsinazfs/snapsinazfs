// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Configuration.Templates;

namespace Sanoid.Common.Configuration.Datasets;

/// <summary>
///     Represents Dataset configuration in Sanoid.json
/// </summary>
public class Dataset
{
    /// <summary>
    ///     Creates a new instance of a Dataset having the specified path.
    /// </summary>
    /// <param name="path">The ZFS path of the dataset</param>
    public Dataset( string path )
    {
        Path = path;
    }

    private Dataset? _parent;

    /// <summary>
    ///     Gets a collection of all child Datasets, indexed by their ZFS paths.
    /// </summary>
    public Dictionary<string, Dataset> Children { get; } = new( );

    internal bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets the parent of this Dataset
    /// </summary>
    /// <value>
    ///     A reference to the parent Dataset or <see langword="null" /> if no parent is configured.
    /// </value>
    public Dataset? Parent
    {
        get => _parent;
        set
        {
            value?.Children.TryAdd( Path, this );

            _parent = value;
        }
    }

    /// <summary>
    ///     Gets or sets the ZFS path of this Dataset
    /// </summary>
    public string Path { get; set; }

    internal Template? Template { get; set; }

    internal Template? TemplateOverrides { get; set; }
}
