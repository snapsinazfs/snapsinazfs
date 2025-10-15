#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

using System.Runtime.CompilerServices;

/// <summary>
///   Extension methods for various types
/// </summary>
public static class TypeExtensions
{
  /// <summary>
  ///   Gets an integer index for radio button groups assuming the order of true, false from this
  ///   <see cref="ZfsProperty{T}" />
  /// </summary>
  /// <param name="property">
  ///   The <see cref="ZfsProperty{T}" /> to convert to an integer index for radio button groups
  /// </param>
  /// <returns>
  ///   An <see langword="int" /> representing the index in a radio button group for this property's source<br />
  ///   0: true<br />
  ///   1: false<br />
  /// </returns>
  public static int AsTrueFalseRadioIndex ( this ZfsProperty<bool> property )
  {
    return property.Value ? 0 : 1;
  }

  public static string GetMostRecentSnapshotZfsPropertyName ( this SnapshotPeriod period )
  {
    return period.Kind switch
           {
             SnapshotPeriodKind.Frequent => ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp,
             SnapshotPeriodKind.Hourly   => ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,
             SnapshotPeriodKind.Daily    => ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,
             SnapshotPeriodKind.Weekly   => ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,
             SnapshotPeriodKind.Monthly  => ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,
             SnapshotPeriodKind.Yearly   => ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,
             SnapshotPeriodKind.NotSet   => throw new ArgumentOutOfRangeException ( nameof (period) ),
             _                           => throw new FormatException ( "Unrecognized SnapshotPeriod value" )
           };
  }

  public static string GetZfsPathParent ( this string value )
  {
    int endIndex = value.LastIndexOfAny ( [ '/', '@', '#' ] );

    return endIndex == -1
             ?

             // This is a pool root.
             // Returned value is the same as input
             value
             :

             // This is a non-root dataset, snapshot, or bookmark
             // Return its parent dataset name
             // ReSharper disable once HeapView.ObjectAllocation
             value [ ..endIndex ];
  }

  /// <summary>
  ///   Totally unnecessary convenience proxy method for
  ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" />
  /// </summary>
  /// <exception cref="OutOfMemoryException">
  ///   The length of the resulting string overflows the maximum allowed length (
  ///   <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
  /// </exception>
  public static string ToCommaSeparatedSingleLineString<T> ( this IEnumerable<T> fsInfos, bool withSpaces = false )
    where T : FileSystemInfo
  {
    return ToCommaSeparatedSingleLineString ( fsInfos.Select ( static f => f.FullName ).Order ( ), withSpaces );
  }

  /// <summary>
  ///   Totally unnecessary convenience proxy method for
  ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" />
  /// </summary>
  /// <exception cref="OutOfMemoryException">
  ///   The length of the resulting string overflows the maximum allowed length (
  ///   <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
  /// </exception>
  public static string ToCommaSeparatedSingleLineString ( this IEnumerable<ZfsRecord> records, bool withSpaces = false )
  {
    return ToCommaSeparatedSingleLineString ( records.Order ( ).Select ( static r => r.Name ), withSpaces );
  }

  /// <summary>
  ///   Reflection-free conversion of string to <see cref="SnapshotPeriodKind" />.
  /// </summary>
  public static SnapshotPeriodKind ToSnapshotPeriodKind ( this string input )
  {
    return SnapshotPeriod.StringToSnapshotPeriodKind ( input );
  }

  /// <summary>
  ///   Totally unnecessary convenience proxy method for
  ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" />
  /// </summary>
  public static string ToStringForZfsSet ( this IEnumerable<IZfsProperty> properties )
  {
    return properties.Select ( static p => p.SetString ).ToSpaceSeparatedSingleLineString ( );
  }

  /// <summary>
  ///   Gets a string of all <see cref="IZfsProperty.SetString" /> values, separated by spaces, to be used in zfs set
  ///   operations.
  /// </summary>
  /// <param name="properties">
  ///   An <see cref="IEnumerable{T}" /> of <see cref="IZfsProperty" /> objects to get a set string for.
  /// </param>
  public static string ToStringForZfsSet ( this List<IZfsProperty> properties )
  {
    ArgumentNullException.ThrowIfNull ( properties );

    if ( properties.Count == 0 )
    {
      throw new ArgumentException ( "Empty collection provided", nameof (properties) );
    }

    return properties.Select ( static p => p.SetString ).ToSpaceSeparatedSingleLineString ( );
  }

  extension ( IEnumerable<string> strings )
  {
    /// <summary>
    ///   Totally unnecessary convenience proxy method for
    ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" />
    /// </summary>
    /// <exception cref="OutOfMemoryException">
    ///   The length of the resulting string overflows the maximum allowed length (
    ///   <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public string ToCommaSeparatedSingleLineString ( bool withSpaces = false )
    {
      return withSpaces ? string.Join ( ", ", strings ) : string.Join ( ',', strings );
    }

    /// <summary>
    ///   Totally unnecessary convenience proxy method for
    ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" />
    /// </summary>
    /// <exception cref="OutOfMemoryException">
    ///   The length of the resulting string overflows the maximum allowed length (
    ///   <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public string ToNewlineSeparatedString ( )
    {
      return strings.ToString ( true );
    }

    /// <summary>
    ///   Totally unnecessary convenience proxy method for
    ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" />
    /// </summary>
    /// <exception cref="OutOfMemoryException">
    ///   The length of the resulting string overflows the maximum allowed length (
    ///   <see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public string ToSpaceSeparatedSingleLineString ( )
    {
      return strings.ToString ( withSpaces: true );
    }

    /// <summary>
    ///   Totally unnecessary convenience proxy method for
    ///   <see cref="string.Join(string?,System.Collections.Generic.IEnumerable{string?})" /> that provides canned forms for consistency.
    /// </summary>
    /// <param name="withNewlines">
    ///   If <see langword="true" />, include a newline character as the last component of the separator.
    /// </param>
    /// <param name="withCommas">
    ///   If <see langword="true" />, include a comma as the first component of the separator.
    /// </param>
    /// <param name="withSpaces">
    ///   If <see langword="true" />, include a space as the last component of the separator.<br />
    ///   Spaces will not be included if <paramref name="withNewlines" /> is also <see langword="true" />.
    /// </param>
    /// <exception cref="OutOfMemoryException">
    ///   The length of the resulting string overflows the maximum allowed length
    ///   (<see cref="System.Int32.MaxValue">Int32.MaxValue</see>).
    /// </exception>
    public string ToString ( bool withNewlines = false, bool withCommas = false, bool withSpaces = false )
    {
      return ( withNewlines, withCommas, withSpaces ) switch
             {
               (false, false, false) => string.Join ( ", ",         strings ),
               (false, false, true)  => string.Join ( ',',          strings ),
               (false, true, false)  => string.Join ( ' ',          strings ),
               (false, true, true)   => string.Join ( string.Empty, strings ),
               // These two will ignore spaces since they make no sense with a newline
               (true, false, _) => string.Join ( "\n",  strings ),
               (true, true, _)  => string.Join ( ",\n", strings )
             };
    }
  }

  extension ( ZfsProperty<int> retentionProperty )
  {
    /// <summary>
    ///   Returns a boolean indicating whether <paramref name="retentionProperty" /> is NOT wanted, by checking if its value is 0.
    /// </summary>
    /// <remarks>
    ///   This method is an extension because it is only intended to apply for specific types of ZfsProperties.
    /// </remarks>
    public bool IsNotWanted => retentionProperty.Value == 0;

    /// <summary>
    ///   Returns a boolean indicating whether <paramref name="retentionProperty" /> IS wanted, by checking if its value is not 0.
    /// </summary>
    public bool IsWanted => retentionProperty.Value != 0;
  }
}
