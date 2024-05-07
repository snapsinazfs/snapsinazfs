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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.Json.Nodes;
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
        int startIndex = 1 + path.LastIndexOfAny( ['/', '@', '#'] );
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

        List<string> dayNamesList = [..value.DayNames];
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

        List<string> monthNamesList = [..value.MonthNames];
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

    public static JsonNode? SerializeToJson( this IConfiguration configSection )
    {
        JsonObject obj = new( );

        IConfigurationSection[] nodeChildren = configSection.GetChildren( ).ToArray( );
        foreach ( IConfigurationSection childSection in nodeChildren )
        {
            if ( childSection.Path.EndsWith( ":0" ) )
            {
                JsonArray arrayNode = [];

                foreach ( IConfigurationSection arrayNodeChild in nodeChildren )
                {
                    arrayNode.Add( SerializeToJson( arrayNodeChild ) );
                }

                return arrayNode;
            }

            obj.Add( childSection.Key, SerializeToJson( childSection ) );
        }

        if ( obj.Any( ) || configSection is not IConfigurationSection section )
        {
            return obj;
        }

        if ( bool.TryParse( section.Value, out bool boolean ) )
        {
            return JsonValue.Create( boolean );
        }

        if ( decimal.TryParse( section.Value, out decimal real ) )
        {
            return JsonValue.Create( real );
        }

        if ( long.TryParse( section.Value, out long integer ) )
        {
            return JsonValue.Create( integer );
        }

        return JsonValue.Create( section.Value );
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
    public static int ToInt32( this ustring value, in int fallbackValue )
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
    ///     A shortcut to calling <see cref="TimeOnly.FromTimeSpan" /> on <paramref name="value" />
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The time portion of <paramref name="value" /></returns>
    [Pure]
    [ExcludeFromCodeCoverage( Justification = "Just a proxy method to framework functionality" )]
    public static TimeOnly ToTimeOnly( this TimeSpan value )
    {
        return TimeOnly.FromTimeSpan( value );
    }
}
