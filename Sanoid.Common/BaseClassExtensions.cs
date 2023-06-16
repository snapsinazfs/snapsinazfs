// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Sanoid.Common;

/// <summary>
///     Extension methods for base classes.
/// </summary>
public static class BaseClassExtensions
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Attempts to convert the current string to a <see cref="bool" /> value.<br />
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fallback">A fallback value to return if string parsing fails.</param>
    /// <returns></returns>
    public static bool AsBoolean( this string? value, bool fallback )
    {
        return !bool.TryParse( value, out bool returnValue ) ? fallback : returnValue;
    }

    /// <summary>
    ///     Attempts to convert the current string to an <see cref="int" /> value.<br />
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fallback">A fallback value to return if string parsing fails.</param>
    /// <returns></returns>
    public static int AsInt( this string? value, int fallback )
    {
        return !int.TryParse( value, out int returnValue ) ? fallback : returnValue;
    }

    /// <summary>
    ///     Attempts to get a boolean value with the specified key from any type implementing <see cref="IConfiguration" />
    /// </summary>
    /// <typeparam name="T">Any type implementing <see cref="IConfiguration" /></typeparam>
    /// <param name="configurationSection">The current <see cref="IConfiguration" /> object to get the value from</param>
    /// <param name="settingKey">The key of the value in <paramref name="configurationSection" /> to return as a boolean value</param>
    /// <returns>A boolean parsed from the value with the specicified <paramref name="settingKey" /></returns>
    /// <remarks>Validation of the value retrieved from configuration is delegated to the base class library</remarks>
    public static bool GetBoolean<T>( this T configurationSection, string settingKey ) where T : IConfiguration
    {
        if ( string.IsNullOrWhiteSpace( settingKey ) )
        {
            throw new ArgumentException( "settingKey must be a non-null, non-empty string.", settingKey );
        }

        return configurationSection[ settingKey ].AsBoolean( false );
    }

    /// <summary>
    ///     Attempts to get a boolean value with the specified key from any type implementing <see cref="IConfiguration" />,
    ///     with specified fallback value.
    /// </summary>
    /// <typeparam name="T">Any type implementing <see cref="IConfiguration" /></typeparam>
    /// <param name="configurationSection">The current <see cref="IConfiguration" /> object to get the value from</param>
    /// <param name="settingKey">The key of the value in <paramref name="configurationSection" /> to return as a boolean value</param>
    /// <param name="fallbackValue">The value to return if the configured value does not exist or is unparseable.</param>
    /// <returns>A boolean parsed from the value with the specicified <paramref name="settingKey" /></returns>
    /// <remarks>Validation of the value retrieved from configuration is delegated to the base class library</remarks>
    public static bool GetBoolean<T>( this T configurationSection, string settingKey, bool fallbackValue ) where T : IConfiguration
    {
        if ( string.IsNullOrWhiteSpace( settingKey ) )
        {
            throw new ArgumentException( "settingKey must be a non-null, non-empty string.", settingKey );
        }

        return configurationSection[ settingKey ].AsBoolean( fallbackValue );
    }

    /// <summary>
    ///     Attempts to get a <see langword="bool?" /> value with the specified key from any type implementing
    ///     <see cref="IConfiguration" />, with specified fallback value.
    /// </summary>
    /// <typeparam name="T">Any type implementing <see cref="IConfiguration" /></typeparam>
    /// <param name="configurationSection">The current <see cref="IConfiguration" /> object to get the value from</param>
    /// <param name="settingKey">The key of the value in <paramref name="configurationSection" /> to return as a boolean value</param>
    /// <param name="fallbackValue">The value to return if the configured value does not exist or is unparseable.</param>
    /// <returns>A boolean parsed from the value with the specicified <paramref name="settingKey" /></returns>
    /// <remarks>Validation of the value retrieved from configuration is delegated to the base class library</remarks>
    public static bool? GetBoolean<T>( this T configurationSection, string settingKey, bool? fallbackValue = null ) where T : IConfiguration
    {
        if ( string.IsNullOrWhiteSpace( settingKey ) )
        {
            throw new ArgumentException( "settingKey must be a non-null, non-empty string.", settingKey );
        }

        if ( configurationSection[ settingKey ] is null )
        {
            return null;
        }

        string? tempQualifier = configurationSection[ settingKey ];
        return !bool.TryParse( tempQualifier, out bool returnValue ) ? fallbackValue : returnValue;
    }

    /// <summary>
    ///     Attempts to get a <see cref="TimeOnly" /> value with the specified key from any type implementing
    ///     <see cref="IConfiguration" />, with specified fallback value.
    /// </summary>
    /// <typeparam name="T">Any type implementing <see cref="IConfiguration" /></typeparam>
    /// <param name="configurationSection">The current <see cref="IConfiguration" /> object to get the value from</param>
    /// <param name="settingKey">
    ///     The key of the value in <paramref name="configurationSection" /> to return as a
    ///     <see cref="TimeOnly" /> value
    /// </param>
    /// <param name="fallbackHours">Value to use for hours as fallback, if value does not exist or is not parseable</param>
    /// <param name="fallbackMinutes">Value to use for minutes as fallback, if value does not exist or is not parseable</param>
    /// <param name="fallbackSeconds">Value to use for seconds as fallback, if value does not exist or is not parseable</param>
    /// <returns>A <see cref="TimeOnly" /> parsed from the value with the specicified <paramref name="settingKey" /></returns>
    /// <remarks>Validation of the value retrieved from configuration is delegated to the base class library</remarks>
    public static TimeOnly GetTimeOnly<T>( this T configurationSection, string settingKey, int fallbackHours, int fallbackMinutes, int fallbackSeconds = 0 ) where T : IConfiguration
    {
        if ( string.IsNullOrWhiteSpace( settingKey ) )
        {
            throw new ArgumentException( "settingKey must be a non-null, non-empty string.", settingKey );
        }

        if ( configurationSection[ settingKey ] is null )
        {
            return new( fallbackHours, fallbackMinutes, fallbackSeconds );
        }

        string? timeString = configurationSection[ settingKey ];
        return TimeOnly.TryParse( timeString ?? "00:00", out TimeOnly returnValue ) ? returnValue : new( fallbackHours, fallbackMinutes, fallbackSeconds );
    }

    /// <summary>
    ///     Attempts to get an <see langword="int" /> value with the specified key from any type implementing
    ///     <see cref="IConfiguration" />, with specified fallback value.
    /// </summary>
    /// <typeparam name="T">Any type implementing <see cref="IConfiguration" /></typeparam>
    /// <param name="configurationSection">The current <see cref="IConfiguration" /> object to get the value from</param>
    /// <param name="settingKey">
    ///     The key of the value in <paramref name="configurationSection" /> to return as an
    ///     <see langword="int" /> value
    /// </param>
    /// <param name="fallbackValue">The value to return if the configured value does not exist or is unparseable.</param>
    /// <returns>An <see langword="int" /> parsed from the value with the specicified <paramref name="settingKey" /></returns>
    /// <remarks>Validation of the value retrieved from configuration is delegated to the base class library</remarks>
    public static int GetInt<T>( this T configurationSection, string settingKey, int fallbackValue = 0 ) where T : IConfiguration
    {
        if ( string.IsNullOrWhiteSpace( settingKey ) )
        {
            throw new ArgumentException( "settingKey must be a non-null, non-empty string.", settingKey );
        }

        return configurationSection[ settingKey ].AsInt( fallbackValue );
    }

    /// <inheritdoc cref="Enum.HasFlag" />
    /// <remarks>
    ///     Valid for 32-bit enums ONLY. Will return unpredictable results or potentially memory access violations for shorter
    ///     types.<br />
    ///     Works by taking the pointer to your enum and the flag value, re-casting them as int pointers, and then bitwise
    ///     AND-ing the two values.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
    public static unsafe bool BinaryHasFlags<T>( this T value, T flag ) where T : unmanaged, Enum
    {
        int* valuePointer = (int*)&value;
        int* flagPointer = (int*)&flag;
        return ( *valuePointer & *flagPointer ) == *valuePointer;
    }
}
