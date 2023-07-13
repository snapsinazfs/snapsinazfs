// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

/// <summary>
///     Extension methods for various types
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Gets an integer index for radio button groups assuming the order of true, false from this
    ///     <see cref="ZfsProperty{T}" />
    /// </summary>
    /// <param name="property">
    ///     The <see cref="ZfsProperty{T}" /> to convert to an integer index for radio button groups
    /// </param>
    /// <returns>
    ///     An <see langword="int" /> representing the index in a radio button group for this property's source<br />
    ///     0: true<br />
    ///     1: false<br />
    /// </returns>
    public static int AsTrueFalseRadioIndex( this ZfsProperty<bool> property )
    {
        return property.Value ? 0 : 1;
    }

    /// <summary>
    ///     Gets an integer index for radio button groups assuming the order of true,false from this
    ///     <see langword="bool" /> value
    /// </summary>
    /// <param name="value">
    ///     The <see langword="bool" /> to convert to an integer index for radio button groups
    /// </param>
    /// <returns>
    ///     An <see langword="int" /> representing the index in a radio button group for this property's source<br />
    ///     0: true<br />
    ///     1: false<br />
    /// </returns>
    public static int AsTrueFalseRadioIndex( this bool value )
    {
        return value ? 0 : 1;
    }

    public static string GetMostRecentSnapshotZfsPropertyName( this SnapshotPeriod period )
    {
        return period.Kind switch
        {
            SnapshotPeriodKind.Frequent => ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName,
            SnapshotPeriodKind.Hourly => ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName,
            SnapshotPeriodKind.Daily => ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName,
            SnapshotPeriodKind.Weekly => ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName,
            SnapshotPeriodKind.Monthly => ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName,
            SnapshotPeriodKind.Yearly => ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName,
            SnapshotPeriodKind.NotSet => throw new ArgumentOutOfRangeException( nameof( period ) ),
            _ => throw new FormatException( "Unrecognized SnapshotPeriod value" )
        };
    }

    public static string GetZfsPathParent( this string value )
    {
        int endIndex = value.LastIndexOfAny( new[] { '/', '@', '#' } );

        return endIndex == -1
            ?
            // This is a pool root.
            // Returned value is the same as input
            value
            :
            // This is a non-root dataset, snapshot, or bookmark
            // Return its parent dataset name
            value[ ..endIndex ];
    }

    public static string GetZfsPathRoot( this string value )
    {
        int endIndex = value.IndexOfAny( new[] { '/', '@', '#' }, 1 );

        return endIndex == -1 ? value : value[ ..endIndex ];
    }

    public static bool IsNotWanted( this ZfsProperty<int> retentionProperty )
    {
        return retentionProperty.Value == 0;
    }

    public static bool IsWanted( this ZfsProperty<int> retentionProperty )
    {
        return retentionProperty.Value != 0;
    }

    /// <exception cref="OutOfMemoryException">
    ///     The length of the resulting string overflows the maximum allowed length (
    ///     <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public static string ToCommaSeparatedSingleLineString( this IEnumerable<string> strings )
    {
        return string.Join( ',', strings );
    }

    /// <exception cref="OutOfMemoryException">
    ///     The length of the resulting string overflows the maximum allowed length (
    ///     <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public static string ToNewlineSeparatedString( this IEnumerable<string> strings )
    {
        return string.Join( '\n', strings );
    }

    public static SnapshotPeriod ToSnapshotPeriod( this string input )
    {
        return (SnapshotPeriod)input;
    }

    public static SnapshotPeriodKind ToSnapshotPeriodKind( this string input )
    {
        return SnapshotPeriod.StringToSnapshotPeriodKind( input );
    }

    /// <exception cref="OutOfMemoryException">
    ///     The length of the resulting string overflows the maximum allowed length (
    ///     <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public static string ToSpaceSeparatedSingleLineString( this IEnumerable<string> strings )
    {
        return string.Join( ' ', strings );
    }

    public static string ToStringForZfsSet( this IEnumerable<IZfsProperty> properties )
    {
        return properties.Select( p => p.SetString ).ToSpaceSeparatedSingleLineString( );
    }

    public static string ToStringForZfsSet( this IDictionary<string, IZfsProperty> properties )
    {
        return properties.Select( kvp => kvp.Value.SetString ).ToSpaceSeparatedSingleLineString( );
    }

    /// <summary>
    ///     Gets a string of all <see cref="IZfsProperty.SetString" /> values, separated by spaces, to be used in zfs set
    ///     operations
    /// </summary>
    /// <param name="properties">
    ///     An <see cref="IEnumerable{T}" /> of <see cref="IZfsProperty" /> objects to get a set string
    ///     for
    /// </param>
    /// <returns></returns>
    public static string ToStringForZfsSet( this List<IZfsProperty> properties )
    {
        ArgumentNullException.ThrowIfNull( properties, nameof( properties ) );

        if ( !properties.Any( ) )
        {
            throw new ArgumentException( "Empty collection provided", nameof( properties ) );
        }

        return properties.Select( p => p.SetString ).ToSpaceSeparatedSingleLineString( );
    }
}
