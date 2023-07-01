// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     A class for extra convenience when dealing with the <see cref="SnapshotPeriodKind" /> enum
/// </summary>
public class SnapshotPeriod : IComparable<SnapshotPeriodKind>, IComparable<SnapshotPeriod>
{
    private SnapshotPeriod( SnapshotPeriodKind kind )
    {
        Kind = kind;
    }

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Daily" />
    /// </summary>
    public static SnapshotPeriod Daily { get; } = new( SnapshotPeriodKind.Daily );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Frequent" />
    /// </summary>
    public static SnapshotPeriod Frequent { get; } = new( SnapshotPeriodKind.Frequent );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Hourly" />
    /// </summary>
    public static SnapshotPeriod Hourly { get; } = new( SnapshotPeriodKind.Hourly );

    /// <summary>
    ///     Gets the <see cref="SnapshotPeriodKind" /> value for this object
    /// </summary>
    public SnapshotPeriodKind Kind { get; }

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Manual" />
    /// </summary>
    public static SnapshotPeriod Manual { get; } = new( SnapshotPeriodKind.Manual );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Monthly" />
    /// </summary>
    public static SnapshotPeriod Monthly { get; } = new( SnapshotPeriodKind.Monthly );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to
    ///     <see cref="SnapshotPeriodKind.Temporary" />
    /// </summary>
    public static SnapshotPeriod Temporary { get; } = new( SnapshotPeriodKind.Temporary );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Weekly" />
    /// </summary>
    public static SnapshotPeriod Weekly { get; } = new( SnapshotPeriodKind.Weekly );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Yearly" />
    /// </summary>
    public static SnapshotPeriod Yearly { get; } = new( SnapshotPeriodKind.Yearly );

    private const string DailyString = "daily";
    private const string FrequentString = "frequently";
    private const string HourlyString = "hourly";
    private const string ManualString = "manual";
    private const string MonthlyString = "monthly";
    private const string TemporaryString = "temporary";
    private const string UnsetString = "-";
    private const string WeeklyString = "weekly";
    private const string YearlyString = "yearly";

    /// <inheritdoc />
    /// <exception cref="ArgumentException"><paramref name="other" /> and this instance are not the same type.</exception>
    /// <exception cref="InvalidOperationException">
    ///     This instance is not type <see cref="SByte" />, <see cref="Int16" />,
    ///     <see cref="Int32" />, <see cref="Int64" />, <see cref="Byte" />, <see cref="UInt16" />, <see cref="UInt32" />, or
    ///     <see cref="UInt64" />.
    /// </exception>
    public int CompareTo( SnapshotPeriod? other )
    {
        return other is null ? -1 : Kind.CompareTo( other.Kind );
    }

    /// <inheritdoc />
    public int CompareTo( SnapshotPeriodKind other )
    {
        return Kind.CompareTo( other );
    }

    /// <inheritdoc />
    public override bool Equals( object? obj )
    {
        if ( obj is not SnapshotPeriod other )
            return false;
        if ( ReferenceEquals( this, obj ) )
            return true;

        return Kind == other.Kind;
    }

    public bool Equals( SnapshotPeriod other )
    {
        return Kind == other.Kind;
    }

    /// <inheritdoc />
    public override int GetHashCode( )
    {
        return (int)Kind;
    }

    /// <summary>
    ///     Explicit conversion from <see langword="string" /> to <see cref="SnapshotPeriod" />
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator SnapshotPeriod( string value )
    {
        return value switch
        {
            FrequentString => Frequent,
            HourlyString => Hourly,
            DailyString => Daily,
            WeeklyString => Weekly,
            MonthlyString => Monthly,
            YearlyString => Yearly,
            ManualString => Manual,
            _ => Temporary
        };
    }

    /// <summary>
    ///     Implicit or explicit conversion to <see langword="string" />, for the given <see cref="SnapshotPeriod" /> object
    /// </summary>
    /// <param name="self"></param>
    public static implicit operator string( SnapshotPeriod self )
    {
        return self.Kind switch
        {
            SnapshotPeriodKind.Temporary => TemporaryString,
            SnapshotPeriodKind.Frequent => FrequentString,
            SnapshotPeriodKind.Hourly => HourlyString,
            SnapshotPeriodKind.Daily => DailyString,
            SnapshotPeriodKind.Weekly => WeeklyString,
            SnapshotPeriodKind.Monthly => MonthlyString,
            SnapshotPeriodKind.Yearly => YearlyString,
            SnapshotPeriodKind.Manual => ManualString,
            _ => UnsetString
        };
    }

    public static implicit operator SnapshotPeriodKind( SnapshotPeriod self )
    {
        return self.Kind;
    }

    public static implicit operator SnapshotPeriod( SnapshotPeriodKind kind )
    {
        return new ( kind );
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return this;
    }
}
