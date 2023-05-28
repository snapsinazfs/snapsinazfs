// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Security;
using System.Text.Json.Serialization;
using Sanoid.Common.Configuration.Snapshots;
using Template = Sanoid.Common.Configuration.Templates.Template;

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
        Logger.Trace( "Creating new Dataset {0} with parent {1}", path, parent.VirtualPath );
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
    /// Scans all processes in /proc for any running calls to zfs send or receive that involve this Dataset
    /// </summary>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="PathTooLongException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="DirectoryNotFoundException">
    ///     The path encapsulated in the <see cref="DirectoryInfo" /> object is
    ///     invalid (for example, it is on an unmapped drive).
    /// </exception>
    /// <exception cref="ArgumentException" />
    /// <exception cref="UnauthorizedAccessException" />
    /// <exception cref="NotSupportedException" />
    /// <exception cref="IOException" />
    /// <exception cref="FileNotFoundException" />
    /// <exception cref="OverflowException" />
    /// 
    public bool IsAlreadyInvolvedInSendOrReceive
    {
        get
        {
            DirectoryInfo procDir = new( "/proc" );
            foreach ( DirectoryInfo p in procDir.EnumerateDirectories( ) )
            {
                // Process directories are purely numeric, so skip anything that isn't int parseable
                if ( !int.TryParse( p.Name, out _ ) )
                {
                    continue;
                }

                // Look for the exe node in /proc/[id]/
                FileInfo f = new( System.IO.Path.Combine( p.FullName, "exe" ) );

                // Verify it is actually there, is a ReparsePoint, is a valid string value, and ends with "/zfs"
                if ( f.Exists && f.Attributes.BinaryHasFlags( FileAttributes.ReparsePoint ) & !string.IsNullOrWhiteSpace(f.LinkTarget) && f.LinkTarget!.EndsWith( "/zfs" ) )
                {
                    continue;
                }

                // Get the cmdline special file, and check if it is long enough to care and is send or receive operation
                string[] commandLine = File.ReadAllText( System.IO.Path.Combine( p.FullName, "cmdline" ) ).Split( '\0', (StringSplitOptions)3 );
                if ( commandLine.Length < 3 || !new[] { "send", "receive", "recv" }.Contains( commandLine[ 1 ] ) )
                {
                    continue;
                }

                // Scan all arguments past the second one and check if any are explicitly our dataset (not children)
                for ( int i = commandLine.Length-1; i>2;i--)
                {
                    if ( System.IO.Path.GetDirectoryName(commandLine[i]) == Path )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    ///     Gets or sets whether this dataset exists explicitly in the configuration
    /// </summary>
    public bool IsInConfiguration { get; set; }

    /// <summary>
    ///     Gets whether this Dataset is a zpool
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> computed from !<see cref="IsRoot" /> &amp;&amp; <see cref="_parent" />!.
    ///     <see cref="IsRoot" />
    /// </value>
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

    private static Dataset? Root { get; set; }

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

    internal void TrimUnwantedChildren( SortedDictionary<string, Dataset> allDatasets )
    {
        if ( Children.Count == 0 )
        {
            return;
        }

        Logger.Debug( "Pruning unwanted children of Dataset {0}", Path );
        string[] dsNames = Children.Keys.ToArray( );
        foreach ( string childKey in dsNames )
        {
            if ( !allDatasets.TryGetValue( childKey, out _ ) )
            {
                continue;
            }

            Dataset child = allDatasets[ childKey ];
            child.TrimUnwantedChildren( allDatasets );
            if ( !child.Enabled )
            {
                Logger.Trace( "Dataset {0} is not wanted. Attempting to remove from parent.", child.Path );
                Children.Remove( childKey );
                Logger.Trace( "Dataset {0} is not wanted. Attempting to remove from global collection.", child.Path );
                if ( !allDatasets.Remove( childKey, out _ ) )
                {
                    Logger.Error( "Dataset {0} could not be removed from the global collection.", child.Path );
                }

                continue;
            }

            Logger.Debug( "Dataset {0} is still wanted.", child.Path );
        }

        Logger.Debug( "Finished pruning unwanted children of Dataset {0}", Path );
    }

    /// <summary>
    ///     Gets whether or not this Dataset is wanted for the given period, according to its Template.
    /// </summary>
    /// <param name="period">The period to check against.</param>
    /// <returns>
    ///     A boolean indicating that the dataset's template specifies a value greater than 0 for the given period.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If an unexpected value is passed for <paramref name="period" />
    /// </exception>
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
