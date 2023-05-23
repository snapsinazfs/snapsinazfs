// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using Sanoid.Common.Configuration.Snapshots;
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

    /// <summary>
    ///     Creates a new instance of a Dataset having the specified path and parent Dataset.<br />
    ///     Adds the new dataset to <paramref name="parent" />'s Children collection automatically.
    /// </summary>
    /// <param name="path">The zfs path of the new dataset</param>
    /// <param name="parent">The parent dataset of the new dataset. Must not be null.</param>
    public Dataset( string path, Dataset parent )
    {
        Logger.Debug( "Creating new Dataset {0} with parent {1}", path, parent.VirtualPath );
        Path = path;
        _parent = parent;
        _parent.Children.TryAdd( VirtualPath, this );
    }

    private readonly Dataset? _parent;

    /// <summary>
    ///     Gets a collection of all child Datasets, indexed by their ZFS paths.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.Always )]
    public SortedDictionary<string, Dataset> Children { get; } = new( );

    /// <summary>
    ///     Gets or sets if this Dataset is enabled for explicit processing or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets whether this dataset exists explicitly in the configuration
    /// </summary>
    public bool IsInConfiguration { get; set; }

    public bool IsPool => !IsRoot && _parent!.IsRoot;

    private bool IsRoot { get; init; }

    /// <summary>
    ///     Gets or sets the parent of this Dataset
    /// </summary>
    /// <value>
    ///     A reference to the parent Dataset or <see langword="null" /> if no parent is configured.
    /// </value>
    public Dataset? Parent
    {
        get => _parent;
        init
        {
            value?.Children.TryAdd( VirtualPath, this );

            _parent = value;
        }
    }

    /// <summary>
    ///     Gets or sets the ZFS path of this Dataset
    /// </summary>
    public string Path { get; }

    internal static Dataset? Root { get; private set; }

    /// <summary>
    ///     Gets or sets the <see cref="Templates.Template" /> this <see cref="Dataset" /> will use.
    /// </summary>
    /// <value>
    ///     A <see cref="Templates.Template" /> that this Dataset will use, or <see langword="null" /> if it has not yet been
    ///     set.
    /// </value>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public Template? Template { get; set; }

    /// <summary>
    ///     Gets the <see cref="Path" /> of this Dataset, prepended with a slash, to represent its location in the virtual tree
    /// </summary>
    public string VirtualPath => Path == "/" ? "/" : $"/{Path}";

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Gets the root <see cref="Dataset" />, which is a dummy Dataset that serves as the single root for all ZFS pools.
    /// </summary>
    /// <value>
    ///     A <see cref="Dataset" /> with no parent, no overrides, the default template, and set as disabled.
    /// </value>
    public static Dataset GetRoot( Template defaultTemplate )
    {
        Root ??= new( "/" )
        {
            Template = defaultTemplate,
            Enabled = false,
            Parent = null,
            IsRoot = true
        };
        return Root;
    }

    public bool IsWantedForPeriod( SnapshotPeriod period )
    {
        return Template != null && period switch
        {
            SnapshotPeriod.Temporary => true,
            SnapshotPeriod.Frequent => Template.SnapshotRetention.Frequent > 0,
            SnapshotPeriod.Hourly => Template.SnapshotRetention.Hourly > 0,
            SnapshotPeriod.Daily => Template.SnapshotRetention.Daily > 0,
            SnapshotPeriod.Weekly => Template.SnapshotRetention.Weekly > 0,
            SnapshotPeriod.Monthly => Template.SnapshotRetention.Monthly > 0,
            SnapshotPeriod.Yearly => Template.SnapshotRetention.Yearly > 0,
            SnapshotPeriod.Manual => true,
            _ => throw new ArgumentOutOfRangeException( nameof( period ), period, null )
        };
    }
}
