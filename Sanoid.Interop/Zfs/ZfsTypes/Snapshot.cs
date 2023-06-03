// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;
using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS snapshot
/// </summary>
public class Snapshot : ZfsObjectBase, IComparable<Snapshot>, IEquatable<Snapshot>
{
    private Snapshot( string name )
        : base( name, ZfsObjectKind.Snapshot )
    {
    }

    public string DatasetName
    {
        get
        {
            if ( !Properties.TryGetValue( ZfsProperty.SnapshotNamePropertyName, out ZfsProperty? prop ) )
            {
                throw new InvalidOperationException( "snapshotname property not defined on Snapshot" );
            }

            int sliceEnd = prop.Value.IndexOf( '@' );
            return prop.Value[ ..sliceEnd ] ?? throw new InvalidOperationException( "snapshotname property not defined on Snapshot" );
        }
    }

    public SnapshotPeriod Period
    {
        get
        {
            if ( !Properties.TryGetValue( ZfsProperty.SnapshotPeriodPropertyName, out ZfsProperty? prop ) )
            {
                throw new InvalidOperationException( "snapshotperiod property not defined on Snapshot" );
            }

            return (SnapshotPeriod)prop.Value;
        }
    }

    public DateTimeOffset? Timestamp
    {
        get
        {
            if ( !Properties.TryGetValue( ZfsProperty.SnapshotTimestampPropertyName, out ZfsProperty? prop ) )
            {
                throw new InvalidOperationException( "snapshottimestamp property not defined on Snapshot" );
            }

            if ( !DateTimeOffset.TryParse( prop.Value, out DateTimeOffset result ) )
            {
                return null;
            }

            return result;
        }
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Compares the current instance with another <see cref="Snapshot" /> and returns an integer that indicates
    ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
    ///     <see cref="Snapshot" />.
    /// </summary>
    /// <param name="other">Another <see cref="Snapshot" /> to compare with this instance.</param>
    /// <returns>
    ///     A value that indicates the relative order of the <see cref="Snapshot" />s being compared. The return value has
    ///     these meanings:
    ///     <list type="table">
    ///         <listheader>
    ///             <term> Value</term><description> Meaning</description>
    ///         </listheader>
    ///         <item>
    ///             <term> Less than zero</term>
    ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term> Zero</term>
    ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
    ///         </item>
    ///         <item>
    ///             <term> Greater than zero</term>
    ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Sort order is as follows:
    ///     <list type="number">
    ///         <listheader>
    ///             <term>Condition</term><description>Result</description>
    ///         </listheader>
    ///         <item>
    ///             <term>Other <see cref="Snapshot" /> is null or has a null <see cref="Timestamp" /></term>
    ///             <description>This <see cref="Snapshot" /> precedes <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of this <see cref="Snapshot" /> is null</term>
    ///             <description>This <see cref="Snapshot" /> follows <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of each <see cref="Snapshot" /> is different</term>
    ///             <description>
    ///                 Sort by <see cref="Timestamp" />, using system rules for the <see cref="DateTimeOffset" />
    ///                 type
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period" /> of each <see cref="Snapshot" /> is different</term>
    ///             <description>Delegate sort order to <see cref="SnapshotPeriod" />, using <see cref="Period" /> of each</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period" />s of both <see cref="Snapshot" />s are equal</term>
    ///             <description>Sort by <see cref="ZfsObjectBase.Name" /></description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public int CompareTo( Snapshot? other )
    {
        // If the other snapshot is null or has a null timestamp, consider this snapshot earlier rank
        if ( other?.Timestamp is null )
        {
            return -1;
        }

        // If our timestamp is null, and the other isn't, consider this snapshot later rank
        if ( Timestamp is null )
        {
            return 1;
        }

        // If timestamps are different, sort on timestamps
        if ( other.Timestamp != Timestamp )
        {
            return Timestamp.Value.CompareTo( other.Timestamp.Value );
        }

        // If timestamps are different, sort on period
        if ( Period != other.Period )
        {
            return Period.CompareTo( other.Period );
        }

        // If periods are the same, sort by name
        return Name.CompareTo( other.Name );
    }

    /// <inheritdoc />
    public bool Equals( Snapshot? other )
    {
        // First, check for reference equality
        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        // If the other is null, obviously we're not equal
        if ( other == null )
        {
            return false;
        }

        // If our names are the same, we're the same snapshot
        return Name == other.Name;
    }

    public static Snapshot GetSnapshotForCommandRunner( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings )
    {
        string snapshotName = settings.Templates[ ds.Template ].GenerateFullSnapshotName( ds.Name, period.Kind, timestamp, settings.Formatting );
        Snapshot newSnapshot = new( snapshotName );
        newSnapshot.AddProperty( new( ZfsProperty.SnapshotNamePropertyName, snapshotName, ZfsPropertySourceConstants.Local ) );
        newSnapshot.AddProperty( new( ZfsProperty.SnapshotPeriodPropertyName, period, ZfsPropertySourceConstants.Local ) );
        newSnapshot.AddProperty( new( ZfsProperty.SnapshotTimestampPropertyName, timestamp.ToString( "O" ), ZfsPropertySourceConstants.Local ) );
        newSnapshot.AddProperty( new( ZfsProperty.RecursionPropertyName, ds.Recursion, ZfsPropertySourceConstants.Local ) );
        newSnapshot.AddProperty( new( ZfsProperty.TemplatePropertyName, ds.Template, ZfsPropertySourceConstants.Local ) );
        newSnapshot.AddProperty( new( ZfsProperty.PruneSnapshotsPropertyName, ds.PruneSnapshots.ToString( ).ToLowerInvariant( ), ZfsPropertySourceConstants.Local ) );
        return newSnapshot;
    }

    /// <summary>
    ///     Gets a new <see cref="Snapshot" /> from a string array in a pre-defined order
    /// </summary>
    /// <param name="lineTokens">
    ///     A string array that must be in the order of the <see cref="ZfsProperty.KnownSnapshotProperties" /> collection
    /// </param>
    /// <returns>
    ///     A new <see cref="Snapshot" /> from the input array
    /// </returns>
    /// <remarks>
    ///     <paramref name="lineTokens" /> must be in this order:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>0</term>
    ///             <description>sanoid.net:prunesnapshots</description>
    ///         </item>
    ///         <item>
    ///             <term>1</term>
    ///             <description>sanoid.net:recursion</description>
    ///         </item>
    ///         <item>
    ///             <term>2</term>
    ///             <description>sanoid.net:snapshotname</description>
    ///         </item>
    ///         <item>
    ///             <term>3</term>
    ///             <description>sanoid.net:snapshotperiod</description>
    ///         </item>
    ///         <item>
    ///             <term>4</term>
    ///             <description>sanoid.net:snapshottimestamp</description>
    ///         </item>
    ///         <item>
    ///             <term>5</term>
    ///             <description>sanoid.net:template</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static Snapshot FromListSnapshots( string[] lineTokens )
    {
        if ( lineTokens.Length < ZfsProperty.KnownSnapshotProperties.Count )
        {
            const string errorMessage = "Not enough elements in array";
            Logger.Error( errorMessage );
            throw new ArgumentException( errorMessage, nameof( lineTokens ) );
        }

        if ( !ValidateName( ZfsObjectKind.Snapshot, lineTokens[ 2 ] ) )
        {
            string errorMessage = $"Snapshot name {lineTokens[ 2 ]} is invalid";
            Logger.Error( errorMessage );
            throw new ArgumentException( errorMessage, nameof( lineTokens ) );
        }

        Snapshot snap = new( lineTokens[ 2 ] )
        {
            [ ZfsProperty.PruneSnapshotsPropertyName ] = new( ZfsProperty.PruneSnapshotsPropertyName, lineTokens[ 0 ], ZfsPropertySourceConstants.Local ),
            [ ZfsProperty.RecursionPropertyName ] = new( ZfsProperty.RecursionPropertyName, lineTokens[ 1 ], ZfsPropertySourceConstants.Local ),
            [ ZfsProperty.SnapshotNamePropertyName ] = new( ZfsProperty.SnapshotNamePropertyName, lineTokens[ 2 ], ZfsPropertySourceConstants.Local ),
            [ ZfsProperty.SnapshotPeriodPropertyName ] = new( ZfsProperty.SnapshotPeriodPropertyName, lineTokens[ 3 ], ZfsPropertySourceConstants.Local ),
            [ ZfsProperty.SnapshotTimestampPropertyName ] = new( ZfsProperty.SnapshotTimestampPropertyName, lineTokens[ 4 ], ZfsPropertySourceConstants.Local ),
            [ ZfsProperty.TemplatePropertyName ] = new( ZfsProperty.TemplatePropertyName, lineTokens[ 5 ], ZfsPropertySourceConstants.Local )
        };
        return snap;
    }

    public class SnapshotReverseComparer : IComparer<Snapshot>
    {
        /// <inheritdoc />
        public int Compare( Snapshot? x, Snapshot? y )
        {
            if ( x == null && y == null )
            {
                return 0;
            }

            if ( x != null && y == null )
            {
                return 1;
            }

            if ( x == null && y != null )
            {
                return -1;
            }

            return 0 - x!.CompareTo( y );
        }
    }
}
