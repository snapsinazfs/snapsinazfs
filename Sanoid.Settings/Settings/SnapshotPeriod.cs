// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Settings.Settings;

public class SnapshotPeriod
{
    private SnapshotPeriod( SnapshotPeriodKind kind)
    {
        Kind = kind;
    }
    public SnapshotPeriodKind Kind { get; set; }

    public static SnapshotPeriod Temporary { get; } = new ( SnapshotPeriodKind.Temporary );

    public static SnapshotPeriod Frequent { get; } = new( SnapshotPeriodKind.Frequent );

    public static SnapshotPeriod Hourly { get; } = new ( SnapshotPeriodKind.Hourly);
    public static SnapshotPeriod Daily { get; } = new( SnapshotPeriodKind.Daily);
    public static SnapshotPeriod Weekly { get; } = new( SnapshotPeriodKind.Weekly);
    public static SnapshotPeriod Monthly { get; } = new( SnapshotPeriodKind.Monthly);
    public static SnapshotPeriod Yearly { get; } = new( SnapshotPeriodKind.Yearly);
    public static SnapshotPeriod Manual { get; } = new( SnapshotPeriodKind.Manual);

    public static implicit operator string( SnapshotPeriod self )
    {
        return self.Kind switch
        {
            SnapshotPeriodKind.Temporary => "temporary",
            SnapshotPeriodKind.Frequent => "frequently",
            SnapshotPeriodKind.Hourly => "hourly",
            SnapshotPeriodKind.Daily => "daily",
            SnapshotPeriodKind.Weekly => "weekly",
            SnapshotPeriodKind.Monthly => "monthly",
            SnapshotPeriodKind.Yearly => "yearly",
            SnapshotPeriodKind.Manual => "manual",
        };
    }

    public static explicit operator SnapshotPeriod( string value )
    {
        if ( !Enum.TryParse( value, out SnapshotPeriodKind kind ) )
            throw new InvalidCastException( "Invalid SnapshotPeriod string" );
        return new( kind );
    }
}
