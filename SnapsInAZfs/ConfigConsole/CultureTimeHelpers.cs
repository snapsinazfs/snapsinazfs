#region MIT LICENSE
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NStack;

namespace SnapsInAZfs.ConfigConsole;

/// <summary>
///     A static class with some helpers for dealing with dates and times in a culture-aware way
/// </summary>
[ExcludeFromCodeCoverage( Justification = "Just a bunch of proxy methods to framework functionality" )]
public static class CultureTimeHelpers
{
    /// <summary>
    ///     Gets a <see cref="List{T}" /> of string values for all full day names for the current culture of the executing
    ///     thread
    /// </summary>
    public static List<string> DayNamesLong { get; } = DateTimeFormatInfo.CurrentInfo.DayNames.Where( static m => !string.IsNullOrWhiteSpace( m ) ).ToList( );

    /// <summary>
    ///     Gets a <see cref="List{T}" /> of string values for all full and standard abbreviated day names for the current
    ///     culture of the executing thread
    /// </summary>
    public static List<string> DayNamesLongAndAbbreviated { get; } = DateTimeFormatInfo.CurrentInfo.GetLongAndAbbreviatedDayNames( );

    /// <summary>
    ///     Gets a <see cref="List{T}" /> of string values for all full month names for the current culture of the executing
    ///     thread
    /// </summary>
    public static List<string> MonthNamesLong { get; } = DateTimeFormatInfo.CurrentInfo.MonthNames.Where( static m => !string.IsNullOrWhiteSpace( m ) ).ToList( );

    /// <summary>
    ///     Gets a <see cref="List{T}" /> of string values for all full and standard abbreviated month names for the current
    ///     culture of the executing thread
    /// </summary>
    public static List<string> MonthNamesLongAndAbbreviated { get; } = DateTimeFormatInfo.CurrentInfo.GetMonthNames( );

    /// <summary>
    ///     Gets the month number of this <see cref="DateTime" />, for the current culture of the executing thread.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    ///     A 1-based <see langword="int" /> value for the month of <paramref name="value" />, according to the
    ///     <see cref="Calendar" /> of <see cref="CultureInfo.CurrentCulture" />
    /// </returns>
    public static int GetCalendarMonth( this DateTime value )
    {
        return CultureInfo.CurrentCulture.Calendar.GetMonth( value );
    }

    /// <summary>
    ///     Gets the name of the month with the specified <paramref name="value" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    ///     A <see langword="string" /> containing the name of the month corresponding to <paramref name="value" />,
    ///     according to <see cref="DateTimeFormatInfo.CurrentInfo" />
    /// </returns>
    public static string GetCalendarMonth( int value )
    {
        return DateTimeFormatInfo.CurrentInfo.GetMonthName( value );
    }

    /// <summary>
    ///     Gets the month number of the given <see cref="ustring" /> as its index in the
    ///     <see cref="MonthNamesLongAndAbbreviated" /> collection + 1
    /// </summary>
    /// <param name="ustringValue"></param>
    /// <returns>
    ///     An <see langword="int" /> value for the month
    /// </returns>
    public static int GetCalendarMonth( this ustring ustringValue )
    {
        string stringValue = ustringValue.ToString( )!;
        return MonthNamesLongAndAbbreviated.IndexOf( stringValue ) + 1;
    }

    /// <summary>
    ///     Gets the month number of this <see cref="DateTimeOffset" />, for the current culture of the executing thread.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="useLocalTime">
    ///     An optional <see langword="bool" /> indicating whether to use local time (true - default) or
    ///     not (false)
    /// </param>
    /// <returns>
    ///     A 1-based <see langword="int" /> value for the month of <paramref name="value" />, according to the
    ///     <see cref="Calendar" /> of <see cref="CultureInfo.CurrentCulture" />
    /// </returns>
    public static int GetCalendarMonth( this DateTimeOffset value, bool useLocalTime = true )
    {
        return CultureInfo.CurrentCulture.Calendar.GetMonth( useLocalTime ? value.LocalDateTime : value.UtcDateTime );
    }
}
