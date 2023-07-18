// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using NLog.LayoutRenderers.Wrappers;

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
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Monthly" />
    /// </summary>
    public static SnapshotPeriod Monthly { get; } = new( SnapshotPeriodKind.Monthly );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.NotSet" />
    /// </summary>
    public static SnapshotPeriod NotSet { get; } = new( SnapshotPeriodKind.NotSet );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Weekly" />
    /// </summary>
    public static SnapshotPeriod Weekly { get; } = new( SnapshotPeriodKind.Weekly );

    /// <summary>
    ///     Gets a <see cref="SnapshotPeriod" /> with <see cref="Kind" /> pre-set to <see cref="SnapshotPeriodKind.Yearly" />
    /// </summary>
    public static SnapshotPeriod Yearly { get; } = new( SnapshotPeriodKind.Yearly );

    public const string DailyString = "daily";
    public const string FrequentString = "frequently";
    public const string HourlyString = "hourly";
    public const string MonthlyString = "monthly";
    public const string NotSetString = "-";
    public const string WeeklyString = "weekly";
    public const string YearlyString = "yearly";

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

    public static int Compare( SnapshotPeriod? x, SnapshotPeriod? y )
    {
        return x switch
        {
            null when y is null => 0,
            null => -1,
            _ => x.CompareTo( y )
        };
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
            NotSetString => NotSet,
            _ => throw new FormatException( $"{value} is not a valid SnapshotPeriod value" )
        };
    }

    public static SnapshotPeriodKind StringToSnapshotPeriodKind( string value )
    {
        return value switch
        {
            FrequentString => SnapshotPeriodKind.Frequent,
            HourlyString => SnapshotPeriodKind.Hourly,
            DailyString => SnapshotPeriodKind.Daily,
            WeeklyString => SnapshotPeriodKind.Weekly,
            MonthlyString => SnapshotPeriodKind.Monthly,
            YearlyString => SnapshotPeriodKind.Yearly,
            NotSetString => SnapshotPeriodKind.NotSet,
            _ => throw new FormatException( $"{value} is not a valid SnapshotPeriodKind value" )
        };
    }

    /// <summary>
    ///     Implicit or explicit conversion to <see langword="string" />, for the given <see cref="SnapshotPeriod" /> object
    /// </summary>
    /// <param name="self"></param>
#pragma warning disable CS8524
    public static implicit operator string( SnapshotPeriod self )
    {
        return self.Kind switch
        {
            SnapshotPeriodKind.Frequent => FrequentString,
            SnapshotPeriodKind.Hourly => HourlyString,
            SnapshotPeriodKind.Daily => DailyString,
            SnapshotPeriodKind.Weekly => WeeklyString,
            SnapshotPeriodKind.Monthly => MonthlyString,
            SnapshotPeriodKind.Yearly => YearlyString,
            SnapshotPeriodKind.NotSet => NotSetString,
        };
    }

    public static explicit operator SnapshotPeriodKind( SnapshotPeriod self )
    {
        return self.Kind;
    }

    public static implicit operator SnapshotPeriod( SnapshotPeriodKind kind )
    {
        return kind switch
        {
            SnapshotPeriodKind.Frequent => Frequent,
            SnapshotPeriodKind.Hourly => Hourly,
            SnapshotPeriodKind.Daily => Daily,
            SnapshotPeriodKind.Weekly => Weekly,
            SnapshotPeriodKind.Monthly => Monthly,
            SnapshotPeriodKind.Yearly => Yearly,
            SnapshotPeriodKind.NotSet => NotSet,
        };
    }
#pragma warning restore CS8524

    /// <inheritdoc />
    public override string ToString( )
    {
        return this;
    }
}
