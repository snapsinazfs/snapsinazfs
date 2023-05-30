// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS snapshot
/// </summary>
public class Snapshot : ZfsObjectBase
{
    private Snapshot( string name, SnapshotPeriodKind periodKind )
        : base( name, ZfsObjectKind.Snapshot )
    {
    }

    public string GetPropertiesSetStringForCommandLine( )
    {
        return $"-o {string.Join( ", -o ", Properties.Select( pair => pair.Value.SetString ) )}";
    }

    public static Snapshot GetSnapshotForCommandRunner( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings settings )
    {
        string snapshotName = settings.Templates[ ds.Template ].GenerateFullSnapshotName( ds.Name, period.Kind, timestamp, settings.Formatting );
        Snapshot newSnapshot = new( snapshotName, period.Kind );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Name, snapshotName, ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Period, period, ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Timestamp, timestamp.ToString( "O" ), ZfsPropertySource.Local ) );
        newSnapshot.AddProperty( SnapshotProperty.GetNewSnapshotProperty( SnapshotProperty.SnapshotPropertyKind.Recursion, ds.Recursion, ZfsPropertySource.Local ) );
        return newSnapshot;
    }
}
