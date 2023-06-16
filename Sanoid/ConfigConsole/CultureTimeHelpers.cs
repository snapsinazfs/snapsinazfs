// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Globalization;
using NStack;

namespace Sanoid.ConfigConsole;

/// <summary>
///     A static class with some helpers for dealing with dates and times in a culture-aware way
/// </summary>
public static class CultureTimeHelpers
{
    /// <summary>
    ///     Gets a <see cref="List{T}" /> of string values for all full and standard abbreviated day names for the current
    ///     culture of the executing thread
    /// </summary>
    public static List<string> DayNamesLongAndAbbreviated { get; } = DateTimeFormatInfo.CurrentInfo.GetLongAndAbbreviatedDayNames( );

    /// <summary>
    ///     Gets a <see cref="List{T}" /> of string values for all full and standard abbreviated month names for the current
    ///     culture of the executing thread
    /// </summary>
    public static List<string> MonthNamesLongAndAbbreviated { get; } = DateTimeFormatInfo.CurrentInfo.GetMonthNames( );

    public static List<string> MonthNamesLong { get; } = DateTimeFormatInfo.CurrentInfo.MonthNames.Where( m => !string.IsNullOrWhiteSpace( m ) ).ToList( );

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
