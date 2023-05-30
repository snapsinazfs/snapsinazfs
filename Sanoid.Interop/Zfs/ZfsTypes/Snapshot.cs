// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Reflection.Metadata.Ecma335;
using NLog;
using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS snapshot
/// </summary>
public class Snapshot : ZfsObjectBase
{
    private Snapshot( string name, SnapshotPeriod periodKind )
        : base( name, ZfsObjectKind.Snapshot )
    {
    }

    private Snapshot( string name )
        : base( name, ZfsObjectKind.Snapshot )
    {
    }

    public string DatasetName
    {
        get
        {
            if ( !Properties.TryGetValue( SnapshotProperty.NamePropertyName, out ZfsProperty? prop ) )
            {
                throw new InvalidOperationException( "snapshotname property not defined on Snapshot" );
            }

            int sliceEnd = prop.Value.IndexOf( '@' );
            return prop.Value[..sliceEnd] ?? throw new InvalidOperationException( "snapshotname property not defined on Snapshot" );
        }
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public string GetPropertiesAsOptionsString( )
    {
        return $"-o {string.Join( " -o ", Properties.Select( pair => pair.Value.SetString ) )}";
    }
    public string GetPropertiesAsOutputListString( )
    {
        return $"-o {string.Join( ",", Properties.Select( pair => pair.Key ) )}";
    }

    public static Snapshot GetSnapshotForCommandRunner( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings )
    {
        string snapshotName = settings.Templates[ ds.Template ].GenerateFullSnapshotName( ds.Name, period.Kind, timestamp, settings.Formatting );
        Snapshot newSnapshot = new( snapshotName, period );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Name, snapshotName, ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Period, period, ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Timestamp, timestamp.ToString( "O" ), ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Recursion, ds.Recursion, ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Template, ds.Template, ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Prune, ds.PruneSnapshots.ToString( ), ZfsPropertySource.Local ) );
        return newSnapshot;
    }

    /// <summary>
    ///     Gets a new <see cref="Snapshot" /> from a string array in a pre-defined order
    /// </summary>
    /// <param name="lineTokens">
    ///     A string array that must be in the order of the <see cref="SnapshotProperty.KnownSnapshotProperties" /> collection
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
        if ( lineTokens.Length < SnapshotProperty.KnownSnapshotProperties.Count )
        {
            string errorMessage = "Not enough elements in array";
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
            [ SnapshotProperty.PrunePropertyName ] = new( SnapshotProperty.PrunePropertyName, lineTokens[ 0 ], ZfsPropertySource.Local ),
            [ SnapshotProperty.RecursionPropertyName ] = new( SnapshotProperty.RecursionPropertyName, lineTokens[ 1 ], ZfsPropertySource.Local ),
            [ SnapshotProperty.NamePropertyName ] = new( SnapshotProperty.NamePropertyName, lineTokens[ 2 ], ZfsPropertySource.Local ),
            [ SnapshotProperty.PeriodPropertyName ] = new( SnapshotProperty.PeriodPropertyName, lineTokens[ 3 ], ZfsPropertySource.Local ),
            [ SnapshotProperty.TimestampPropertyName ] = new( SnapshotProperty.TimestampPropertyName, lineTokens[ 4 ], ZfsPropertySource.Local ),
            [ SnapshotProperty.TemplatePropertyName ] = new( SnapshotProperty.TemplatePropertyName, lineTokens[ 5 ], ZfsPropertySource.Local )
        };
        return snap;
    }
}
