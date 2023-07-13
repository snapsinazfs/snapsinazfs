// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NStack;
using Terminal.Gui;

namespace SnapsInAZfs.ConfigConsole;

/// <summary>
///     Extension methods
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Gets the last path component of a ZFS path name, starting from the last instance of '/','@',or '#'
    /// </summary>
    /// <param name="path">A path to operate on</param>
    /// <returns>
    ///     The last component of the given ZFS path, as a new <see langword="string" /> instance, with a value equal to the substring of
    ///     <paramref name="path" />, starting from the first character after the last instance of '/','@',or '#', up to the end of the
    ///     string, or the <b>original string reference</b>, if none of those characters are found.
    /// </returns>
    /// <remarks>
    ///     If the given path does not contain '/','@',or '#', the original string reference will be returned.
    /// </remarks>
    public static string GetLastPathElement( this string path )
    {
        int startIndex = 1 + path.LastIndexOfAny( new[] { '/', '@', '#' } );
        // ReSharper disable once HeapView.ObjectAllocation
        return startIndex == 0 ? path : path[ startIndex.. ];
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
    ///     Gets the string value of the label of the currently selected item of a <see cref="RadioGroup" />
    /// </summary>
    /// <param name="group">The <see cref="RadioGroup" /> to get the currently selected label string from</param>
    /// <returns></returns>
    public static bool GetSelectedBooleanFromLabel( this RadioGroup group )
    {
        return bool.Parse( group.RadioLabels[ group.SelectedItem ].ToString( ) ?? throw new InvalidOperationException( "Failed getting radio group boolean value" ) );
    }

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
    ///     Attempts to parse this <see cref="ustring" /> and return the <see langword="int" /> version of it, with the
    ///     specified fallback value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fallbackValue"></param>
    /// <returns>
    ///     If the <paramref name="value" /> is a valid integer, the <see langword="int" /> representation of
    ///     <paramref name="value" /><br />For all other cases, <paramref name="fallbackValue" />
    /// </returns>
    /// <remarks>
    ///     Does not throw exceptions. In case of any exception, returns <paramref name="fallbackValue" />
    /// </remarks>
    [Pure]
    [SuppressMessage( "ReSharper", "CatchAllClause", Justification = "This method intentionally cannot ever throw an exception" )]
    public static int ToInt32( this ustring value, int fallbackValue )
    {
        try
        {
            if ( value.IsEmpty )
            {
                return fallbackValue;
            }

            string stringValue = value.ToString( )!;

            return int.TryParse( stringValue, out int intValue ) ? intValue : fallbackValue;
        }
        catch
        {
            return fallbackValue;
        }
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
            { Length: > 0 } stringValue when stringValue.Trim( ) is { Length: > 0 } trimmedValue && int.TryParse( trimmedValue, out int intValue ) => intValue,
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
            // This case is checking if it's a non-null string at least 5 characters long
            // All other conditions will throw an ArgumentOutOfRangeException for value
            { Length: > 4 } stringValue when stringValue.Trim( ) is { Length: > 4 } trimmedValue && TimeOnly.TryParse( trimmedValue, out TimeOnly timeOnlyValue ) => timeOnlyValue,
            _ => null
        };
    }

    /// <summary>
    ///     A shortcut to calling <see cref="TimeOnly.FromTimeSpan" /> on <paramref name="value" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The time portion of <paramref name="value" /></returns>
    [Pure]
    public static TimeOnly ToTimeOnly( this TimeSpan value )
    {
        return TimeOnly.FromTimeSpan( value );
    }
}
