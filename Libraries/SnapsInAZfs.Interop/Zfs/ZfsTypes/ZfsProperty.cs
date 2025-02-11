// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

using System.Runtime.CompilerServices;

public readonly struct ZfsProperty<T> : IZfsProperty, IEquatable<int>, IEquatable<string>, IEquatable<bool>, IEquatable<DateTimeOffset>, IEquatable<ZfsProperty<int>>, IEquatable<ZfsProperty<bool>>, IEquatable<ZfsProperty<string>>, IEquatable<ZfsProperty<DateTimeOffset>> where T : notnull
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private ZfsProperty ( string name, in T value, bool isLocal = true )
    {
        Name = name;
        Value = value;
        IsLocal = isLocal;
    }

    public ZfsProperty ( ZfsRecord owner, string name, in T value, bool isLocal = true )
    {
        Owner = owner;
        Name = name;
        Value = value;
        IsLocal = isLocal;
    }

    // ReSharper disable once HeapView.ObjectAllocation
    public readonly string InheritedFrom => IsLocal ? ZfsPropertySourceConstants.Local : Source[ 15.. ];

    [JsonIgnore]
    public readonly bool IsInherited => !IsLocal;

    public T Value { get; init; }

    /// <inheritdoc />
    public readonly bool Equals( bool other )
    {
        return Value is bool v && v == other;
    }

    /// <inheritdoc />
    public readonly bool Equals( DateTimeOffset other )
    {
        return Value is DateTimeOffset v && v == other;
    }

    /// <inheritdoc />
    public readonly bool Equals( int other )
    {
        return Value is int v && v == other;
    }

    /// <inheritdoc />
    public readonly bool Equals( string? other )
    {
        return Value is string v && v == other;
    }

    /// <inheritdoc />
    public readonly bool Equals( ZfsProperty<bool> other )
    {
        return Value is bool v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc />
    public readonly bool Equals( ZfsProperty<DateTimeOffset> other )
    {
        return Value is DateTimeOffset v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc />
    public readonly bool Equals( ZfsProperty<int> other )
    {
        return Value is int v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc />
    public readonly bool Equals( ZfsProperty<string> other )
    {
        return Value is string v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    [JsonIgnore]
    public readonly string Source => IsLocal switch
    {
        true => ZfsPropertySourceConstants.Local,
        false when Owner is null => ZfsPropertySourceConstants.None,
        // ReSharper disable once HeapView.ObjectAllocation
        false when Owner.ParentDataset[ Name ].IsLocal => $"inherited from {Owner.ParentDataset.Name}",
        false => Owner.ParentDataset[ Name ].Source
    };

    [JsonIgnore]
    public ZfsRecord? Owner { get; init; }

    /// <summary>
    ///     Gets a string representation of the Value property, in an appropriate form for its type
    /// </summary>
    [JsonIgnore]
    public readonly string ValueString => Value switch
    {
        int intValue => intValue.ToString( ),
        string value => value,
        bool boolValue => boolValue.ToString( ).ToLowerInvariant( ),
        DateTimeOffset dtoValue => dtoValue.ToString( "O" )
    };

    [JsonIgnore]
    // ReSharper disable once HeapView.ObjectAllocation
    public readonly string SetString => $"{Name}={ValueString}";

    public string Name { get; init; }
    public bool IsLocal { get; init; }

    public static ZfsProperty<bool> CreateWithoutParent( string name, in bool value, bool isLocal = true )
    {
        Logger.Trace( "Creating ZfsProperty<bool> {0} without parent dataset", name );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<int> CreateWithoutParent( string name, in int value, bool isLocal = true )
    {
        Logger.Trace( "Creating ZfsProperty<int> {0} without parent dataset", name );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<string> CreateWithoutParent( string name, string value, bool isLocal = true )
    {
        Logger.Trace( "Creating ZfsProperty<string> {0} without parent dataset", name );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<DateTimeOffset> CreateWithoutParent( string name, in DateTimeOffset value, bool isLocal = true )
    {
        Logger.Trace( "Creating ZfsProperty<DateTimeOffset> {0} without parent dataset", name );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<T> DefaultProperty( ) => new( );

    /// <inheritdoc />
    public readonly override int GetHashCode( )
    {
        return HashCode.Combine( Value, Name, IsLocal );
    }

    public static bool operator ==( ZfsProperty<T> left, bool right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, int right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, string right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, DateTimeOffset right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<bool> right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<int> right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<string> right )
    {
        return left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<DateTimeOffset> right )
    {
        return left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, bool right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, int right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, string right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, DateTimeOffset right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<bool> right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<int> right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<string> right )
    {
        return !left.Equals( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<DateTimeOffset> right )
    {
        return !left.Equals( right );
    }

    /// <summary>
    ///     Attempts to parse a <see cref="RawProperty" /> as its <see cref="ZfsProperty{T}" /> (<see langword="bool" />) equivalent
    /// </summary>
    /// <param name="input">The <see cref="RawProperty" /> to parse</param>
    /// <param name="property">
    ///     The parsed <see cref="ZfsProperty{T}" /> (<see langword="bool" />), if successful
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="input" /> was parsed successfully; otherwise <see langword="false" />
    /// </returns>
    /// <remarks>
    ///     <paramref name="property" /> is never null when this method returns <see langword="true" />; otherwise,
    ///     <paramref name="property" /> is always <see langword="null" />
    /// </remarks>
    public static bool TryParse( RawProperty input, [NotNullWhen( true )] out ZfsProperty<bool>? property )
    {
        property = null;

        // ReSharper disable once InvertIf
        if ( bool.TryParse( input.Value, out bool result ) )
        {
            property = ZfsProperty<bool>.CreateWithoutParent( input.Name, result, input.Source == ZfsPropertySourceConstants.Local );
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Attempts to parse a <see cref="RawProperty" /> as its <see cref="ZfsProperty{T}" /> (<see langword="int" />) equivalent
    /// </summary>
    /// <param name="input">The <see cref="RawProperty" /> to parse</param>
    /// <param name="property">
    ///     The parsed <see cref="ZfsProperty{T}" /> (<see langword="int" />), if successful
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="input" /> was parsed successfully; otherwise <see langword="false" />
    /// </returns>
    /// <remarks>
    ///     <paramref name="property" /> is never null when this method returns <see langword="true" />; otherwise,
    ///     <paramref name="property" /> is always <see langword="null" />
    /// </remarks>
    public static bool TryParse( RawProperty input, [NotNullWhen( true )] out ZfsProperty<int>? property )
    {
        if ( int.TryParse( input.Value, out int result ) )
        {
            property = ZfsProperty<int>.CreateWithoutParent( input.Name, result, input.Source == ZfsPropertySourceConstants.Local );
            return true;
        }

        property = null;
        return false;
    }

    /// <summary>
    ///     Attempts to parse a <see cref="RawProperty" /> as its <see cref="ZfsProperty{T}" /> (<see cref="DateTimeOffset" />)
    ///     equivalent
    /// </summary>
    /// <param name="input">The <see cref="RawProperty" /> to parse</param>
    /// <param name="property">
    ///     The parsed <see cref="ZfsProperty{T}" /> (<see cref="DateTimeOffset" />), if successful
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="input" /> was parsed successfully; otherwise <see langword="false" />
    /// </returns>
    /// <remarks>
    ///     <paramref name="property" /> is never null when this method returns <see langword="true" />; otherwise,
    ///     <paramref name="property" /> is always <see langword="null" />
    /// </remarks>
    public static bool TryParse( RawProperty input, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? property )
    {
        if ( DateTimeOffset.TryParse( input.Value, out DateTimeOffset result ) )
        {
            property = ZfsProperty<DateTimeOffset>.CreateWithoutParent( input.Name, result, input.Source == ZfsPropertySourceConstants.Local );
            return true;
        }

        property = null;
        return false;
    }

    public static bool TryParseDatasetPropertiesFromRawZfsObject (
        string                                                  dsName,
        RawZfsObject                                            rawZfsObject,
        [NotNullWhen ( true )] out ZfsProperty<bool>?           enabled,
        [NotNullWhen ( true )] out ZfsProperty<bool>?           takeSnapshots,
        [NotNullWhen ( true )] out ZfsProperty<bool>?           pruneSnapshots,
        [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp,
        [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp,
        [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp,
        [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp,
        [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp,
        [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp,
        [NotNullWhen ( true )] out ZfsProperty<string>?         recursion,
        [NotNullWhen ( true )] out ZfsProperty<string>?         template,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionFrequent,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionHourly,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionDaily,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionWeekly,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionMonthly,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionYearly,
        [NotNullWhen ( true )] out ZfsProperty<int>?            retentionPruneDeferral,
        [NotNullWhen ( true )] out ZfsProperty<string>?         sourceSystem,
        out                        long                         bytesAvailable,
        out                        long                         bytesUsed
    )
    {
        bytesAvailable = 0;
        bytesUsed = 0;
        Unsafe.SkipInit ( out enabled );
        Unsafe.SkipInit ( out takeSnapshots );
        Unsafe.SkipInit ( out pruneSnapshots );
        Unsafe.SkipInit ( out lastFrequentSnapshotTimestamp );
        Unsafe.SkipInit ( out lastHourlySnapshotTimestamp );
        Unsafe.SkipInit ( out lastDailySnapshotTimestamp );
        Unsafe.SkipInit ( out lastWeeklySnapshotTimestamp );
        Unsafe.SkipInit ( out lastMonthlySnapshotTimestamp );
        Unsafe.SkipInit ( out lastYearlySnapshotTimestamp );
        Unsafe.SkipInit ( out recursion );
        Unsafe.SkipInit ( out template );
        Unsafe.SkipInit ( out retentionFrequent );
        Unsafe.SkipInit ( out retentionHourly );
        Unsafe.SkipInit ( out retentionDaily );
        Unsafe.SkipInit ( out retentionWeekly );
        Unsafe.SkipInit ( out retentionMonthly );
        Unsafe.SkipInit ( out retentionYearly );
        Unsafe.SkipInit ( out retentionPruneDeferral );
        Unsafe.SkipInit ( out sourceSystem );

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.EnabledPropertyName, rawZfsObject, out enabled ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.TakeSnapshotsPropertyName, rawZfsObject, out takeSnapshots ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.PruneSnapshotsPropertyName, rawZfsObject, out pruneSnapshots ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, rawZfsObject, out lastFrequentSnapshotTimestamp ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, rawZfsObject, out lastHourlySnapshotTimestamp ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, rawZfsObject, out lastDailySnapshotTimestamp ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, rawZfsObject, out lastWeeklySnapshotTimestamp ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, rawZfsObject, out lastMonthlySnapshotTimestamp ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, rawZfsObject, out lastYearlySnapshotTimestamp ) )
        {
            return false;
        }

        recursion    = ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.RecursionPropertyName, rawZfsObject.Properties [ ZfsPropertyNames.RecursionPropertyName ].Value, rawZfsObject.Properties [ ZfsPropertyNames.RecursionPropertyName ].Source == ZfsPropertySourceConstants.Local );
        sourceSystem = ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.SourceSystem,          rawZfsObject.Properties [ ZfsPropertyNames.SourceSystem ].Value,          rawZfsObject.Properties [ ZfsPropertyNames.SourceSystem ].Source          == ZfsPropertySourceConstants.Local );
        template     = ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.TemplatePropertyName,  rawZfsObject.Properties [ ZfsPropertyNames.TemplatePropertyName ].Value,  rawZfsObject.Properties [ ZfsPropertyNames.TemplatePropertyName ].Source  == ZfsPropertySourceConstants.Local );

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, rawZfsObject, out retentionFrequent ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, rawZfsObject, out retentionHourly ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, rawZfsObject, out retentionDaily ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, rawZfsObject, out retentionWeekly ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, rawZfsObject, out retentionMonthly ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, rawZfsObject, out retentionYearly ) )
        {
            return false;
        }

        if ( !TryParsePropertyByName ( dsName, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, rawZfsObject, out retentionPruneDeferral ) )
        {
            return false;
        }

        if ( rawZfsObject.Kind != ZfsPropertyValueConstants.Snapshot && !long.TryParse ( rawZfsObject.Properties [ ZfsNativePropertyNames.Available ].Value, out bytesAvailable ) )
        {
            Logger.Debug ( "{0} value {1} not valid for {2} {3} - skipping object", ZfsNativePropertyNames.Available, rawZfsObject.Properties [ ZfsNativePropertyNames.Available ].Value, rawZfsObject.Kind, dsName );

            bytesAvailable = 0;

            return false;
        }

        // Keeping this one as-is for consistency with the rest of the method
        // ReSharper disable once InvertIf
        if ( rawZfsObject.Kind != ZfsPropertyValueConstants.Snapshot && !long.TryParse ( rawZfsObject.Properties [ ZfsNativePropertyNames.Used ].Value, out bytesUsed ) )
        {
            Logger.Debug ( "{0} value {1} not valid for {2} {3} - skipping object", ZfsNativePropertyNames.Used, rawZfsObject.Properties [ ZfsNativePropertyNames.Used ].Value, rawZfsObject.Kind, dsName );

            bytesUsed = 0;

            return false;
        }

        return true;
    }

    private static bool TryParsePropertyByName ( string objectName, string propertyName, RawZfsObject rawZfsObject, [NotNullWhen ( true )] out ZfsProperty<bool>? parsedProperty )
    {
        Unsafe.SkipInit ( out parsedProperty );

        if ( rawZfsObject.Properties.TryGetValue ( propertyName, out RawProperty rawProp ) )
        {
            if ( ZfsProperty<bool>.TryParse ( rawProp, out parsedProperty ) )
            {
                return true;
            }

            Logger.Debug ( "{0} value {1} not valid for {2} {3} - skipping object", propertyName, rawProp.Value, rawZfsObject.Kind, objectName );
        }

        Logger.Debug ( "Property {0} does not exist for {1} {2} - skipping object", propertyName, rawZfsObject.Kind, objectName );

        return false;
    }

    private static bool TryParsePropertyByName ( string objectName, string propertyName, RawZfsObject rawZfsObject, [NotNullWhen ( true )] out ZfsProperty<DateTimeOffset>? parsedProperty )
    {
        Unsafe.SkipInit ( out parsedProperty );

        if ( rawZfsObject.Properties.TryGetValue ( propertyName, out RawProperty rawProp ) )
        {
            if ( ZfsProperty<DateTimeOffset>.TryParse ( rawProp, out parsedProperty ) )
            {
                return true;
            }

            Logger.Debug ( "{0} value {1} not valid for {2} {3} - skipping object", propertyName, rawProp.Value, rawZfsObject.Kind, objectName );
        }

        Logger.Debug ( "Property {0} does not exist for {1} {2} - skipping object", propertyName, rawZfsObject.Kind, objectName );

        return false;
    }

    private static bool TryParsePropertyByName ( string objectName, string propertyName, RawZfsObject rawZfsObject, [NotNullWhen ( true )] out ZfsProperty<int>? parsedProperty )
    {
        Unsafe.SkipInit ( out parsedProperty );

        if ( rawZfsObject.Properties.TryGetValue ( propertyName, out RawProperty rawProp ) )
        {
            if ( ZfsProperty<int>.TryParse ( rawProp, out parsedProperty ) )
            {
                return true;
            }

            Logger.Debug ( "{0} value {1} not valid for {2} {3} - skipping object", propertyName, rawProp.Value, rawZfsObject.Kind, objectName );
        }

        Logger.Debug ( "Property {0} does not exist for {1} {2} - skipping object", propertyName, rawZfsObject.Kind, objectName );

        return false;
    }
}
