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
    public static string ToSnapshotDateTimeString(this DateTimeOffset dt)
    {
        return dt.ToString(Sanoid.Common.Configuration.SnapshotNaming.TimestampFormatString);
    }
}