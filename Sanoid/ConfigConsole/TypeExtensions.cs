// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NStack;
using Terminal.Gui;

namespace Sanoid.ConfigConsole;

/// <summary>
///     Extension methods
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Gets the string value of the label of the currently selected item of a <see cref="RadioGroup" />
    /// </summary>
    /// <param name="group">The <see cref="RadioGroup" /> to get the currently selected label string from</param>
    /// <returns></returns>
    public static string GetSelectedLabelString( this RadioGroup group )
    {
        return group.RadioLabels[ group.SelectedItem ].ToString( ) ?? throw new InvalidOperationException( "Failed getting radio group selected label string" );
    }

    /// <summary>
    ///     Gets the string value of the label of the currently selected item of a <see cref="RadioGroup" />
    /// </summary>
    /// <param name="group">The <see cref="RadioGroup" /> to get the currently selected label string from</param>
    /// <returns></returns>
    public static bool GetSelectedBooleanFromLabel( this RadioGroup group )
    {
        return bool.Parse( group.RadioLabels[ group.SelectedItem ].ToString( ) ?? throw new InvalidOperationException( "Failed getting radio group boolean value" ) );
    }

    /// <summary>
    ///     Gets a list of strings containing day names
    /// </summary>
    /// <param name="value">A <see cref="DateTimeFormatInfo" /> object to get day names for</param>
    /// <returns>
    ///     A <see cref="List{T}" /> of <see langword="string" />s representing all long and standard abbreviated forms
    ///     of the names of the days of the week, in the culture of this <see cref="DateTimeFormatInfo" />,
    ///     in culture-specific order.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" /> or any of its required members are <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the <see cref="DateTimeFormatInfo.DayNames" /> or
    ///     <see cref="DateTimeFormatInfo.AbbreviatedDayNames" /> properties of <paramref name="value" /> are not 7-element
    ///     <see langword="string" /> arrays
    /// </exception>
    [Pure]
    [SuppressMessage( "ReSharper", "ExceptionNotDocumentedOptional", Justification = "The only other exceptions that can be thrown here are for setters that we do not use" )]
    public static List<string> GetLongAndAbbreviatedDayNames( this DateTimeFormatInfo value )
    {
        ArgumentNullException.ThrowIfNull( value );
        if ( value.DayNames is not { Length: 7 } )
        {
            throw new InvalidOperationException( "Invalid DayNames collection in DateTimeFormatInfo object" );
        }

        if ( value.AbbreviatedDayNames is not { Length: 7 } )
        {
            throw new InvalidOperationException( "Invalid AbbreviatedDayNames collection in DateTimeFormatInfo object" );
        }

        List<string> dayNamesList = new( value.DayNames );
        dayNamesList.AddRange( value.AbbreviatedDayNames );
        return dayNamesList;
    }

    /// <summary>
    ///     Gets a list of strings containing day names
    /// </summary>
    /// <param name="value">A <see cref="DateTimeFormatInfo" /> object to get day names for</param>
    /// <returns>
    ///     A <see cref="List{T}" /> of <see langword="string" />s representing all long and standard abbreviated forms
    ///     of the names of the days of the week, in the culture of this <see cref="DateTimeFormatInfo" />,
    ///     in culture-specific order.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" /> or any of its required members are <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the <see cref="DateTimeFormatInfo.DayNames" /> or
    ///     <see cref="DateTimeFormatInfo.AbbreviatedDayNames" /> properties of <paramref name="value" /> are not 7-element
    ///     <see langword="string" /> arrays
    /// </exception>
    [Pure]
    [SuppressMessage( "ReSharper", "ExceptionNotDocumentedOptional", Justification = "The only other exceptions that can be thrown here are for setters that we do not use" )]
    public static List<string> GetMonthNames( this DateTimeFormatInfo value )
    {
        ArgumentNullException.ThrowIfNull( value );

        List<string> monthNamesList = new( value.MonthNames );
        monthNamesList.AddRange( value.AbbreviatedMonthNames );
        monthNamesList.AddRange( value.MonthGenitiveNames );
        monthNamesList.AddRange( value.AbbreviatedMonthGenitiveNames );
        return monthNamesList;
    }

    /// <summary>
    ///     Attempts to parse this <see cref="ustring" /> and return the <see langword="int" /> version of it
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If <paramref name="value" /> is empty</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the value cannot be parsed as an integer</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="value" /> is <see langword="null" /></exception>
    [Pure]
    public static int ToInt32( this ustring value )
    {
        ArgumentNullException.ThrowIfNull( value, nameof( value ) );
        if ( value.IsEmpty )
        {
            throw new ArgumentException( "Empty ustring cannot be converted to int", nameof( value ) );
        }

        return value.ToString( ) switch
        {
            // This case is checking if it's a non-null string, and if it can be parsed as an integer
            // All other conditions will throw an ArgumentOutOfRangeException for value
            { } stringValue when int.TryParse( stringValue, out int intValue ) => intValue,
            _ => throw new ArgumentOutOfRangeException( nameof( value ), "ustring does not represent a valid Int32 value" )
        };
    }

    /// <summary>
    ///     Attempts to parse this <see cref="ustring" /> and return the <see langword="int?" /> version of it
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    ///     For valid integer strings: An <see langword="int" /> value parsed from the string.<br />
    ///     For all other values, including null: A <see langword="null" /> reference.
    /// </returns>
    [Pure]
    public static int? ToNullableInt32( this ustring value )
    {
        if ( value.IsEmpty )
        {
            return null;
        }

        return value.ToString( ) switch
        {
            // This case is checking if it's a non-null string, and if it can be parsed as an integer
            // All other conditions will throw an ArgumentOutOfRangeException for value
            { } stringValue when int.TryParse( stringValue, out int intValue ) => intValue,
            _ => null
        };
    }

    /// <summary>
    ///     Attempts to parse this <see cref="ustring" /> and return a <see cref="TimeOnly" />? version of it
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    ///     For valid time strings: A <see cref="TimeOnly" /> value parsed from the string.<br />
    ///     For all other values, including null: A <see langword="null" /> reference.
    /// </returns>
    [Pure]
    public static TimeOnly? ToNullableTimeOnly( this ustring value )
    {
        if ( value.IsEmpty )
        {
            return null;
        }

        return value.ToString( ) switch
        {
            // This case is checking if it's a non-null string, and if it can be parsed as an integer
            // All other conditions will throw an ArgumentOutOfRangeException for value
            { } stringValue when TimeOnly.TryParse( stringValue, out TimeOnly timeOnlyValue ) => timeOnlyValue,
            _ => null
        };
    }

    /// <summary>
    /// A shortcut to calling <see cref="TimeOnly.FromTimeSpan"/> on <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The time portion of <paramref name="value"/></returns>
    [Pure]
    public static TimeOnly ToTimeOnly( this TimeSpan value )
    {
        return TimeOnly.FromTimeSpan( value );
    }

}
