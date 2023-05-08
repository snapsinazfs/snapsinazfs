using Microsoft.Extensions.Configuration;

namespace Sanoid.Common;

/// <summary>
/// Extension methods for base classes.
/// </summary>
public static class BaseClassExtensions
{
    /// <summary>
    /// Gets the string representation of this <see cref="DateTimeOffset"/>, formatted according to configuration in Sanoid.json.
    /// </summary>
    /// <param name="dt">The <see cref="DateTimeOffset"/> being formatted as a string.</param>
    /// <returns>A string representation of <paramref name="dt"/>, formatted according to configuration in Sanoid.json.</returns>
    public static string ToSnapshotDateTimeString( this DateTimeOffset dt )
    {
        return dt.ToString( Sanoid.Common.Configuration.SnapshotNaming.TimestampFormatString );
    }

    /// <summary>
    /// Attempts to convert the current string to a <see cref="bool"/> value.<br />
    /// Validation left up to the <see cref="bool.Parse(string)"/> method.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool AsBoolean( this string value )
    {
        return bool.Parse( value );
    }

    /// <summary>
    /// Attempts to get a boolean value with the specified key from any type implementing <see cref="IConfiguration"/>
    /// </summary>
    /// <typeparam name="T">Any type implementing <see cref="IConfiguration"/></typeparam>
    /// <param name="configurationSection">The current <see cref="IConfiguration"/> object to get the value from</param>
    /// <param name="settingKey">The key of the value in <paramref name="configurationSection"/> to return as a boolean value</param>
    /// <returns>A boolean parsed from the value with the specicified <paramref name="settingKey"/></returns>
    /// <remarks>Validation of the value retrieved from configuration is delegated to the base class library</remarks>
    public static bool GetBoolean<T>( this T configurationSection, string settingKey ) where T : IConfiguration
    {
        if ( string.IsNullOrWhiteSpace( settingKey ) )
            throw new ArgumentException( "settingKey must be a non-null, non-empty string.", settingKey );
        return (configurationSection[settingKey] ?? "False").AsBoolean( );
    }
}