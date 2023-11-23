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

public struct ZfsProperty<T> : IZfsProperty, IEquatable<int>, IEquatable<string>, IEquatable<bool>, IEquatable<DateTimeOffset>, IEquatable<ZfsProperty<int>>, IEquatable<ZfsProperty<bool>>, IEquatable<ZfsProperty<string>>, IEquatable<ZfsProperty<DateTimeOffset>> where T : notnull
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private ZfsProperty( string name, in T value, bool isLocal = true )
    {
        Name = name;
        Value = value;
        IsLocal = isLocal;
    }

    public ZfsProperty( ZfsRecord owner, string name, in T value, bool isLocal = true )
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
    public ZfsRecord? Owner { get; set; }

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

    public static bool TryParseDatasetPropertiesFromRawZfsObject( string dsName, RawZfsObject rawZfsObject, [NotNullWhen( true )] out ZfsProperty<bool>? enabled, [NotNullWhen( true )] out ZfsProperty<bool>? takeSnapshots, [NotNullWhen( true )] out ZfsProperty<bool>? pruneSnapshots, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, [NotNullWhen( true )] out ZfsProperty<int>? retentionFrequent, [NotNullWhen( true )] out ZfsProperty<int>? retentionHourly, [NotNullWhen( true )] out ZfsProperty<int>? retentionDaily, [NotNullWhen( true )] out ZfsProperty<int>? retentionWeekly, [NotNullWhen( true )] out ZfsProperty<int>? retentionMonthly, [NotNullWhen( true )] out ZfsProperty<int>? retentionYearly, [NotNullWhen( true )] out ZfsProperty<int>? retentionPruneDeferral, out ZfsProperty<string> sourceSystem, out long bytesAvailable, out long bytesUsed )
    {
        bytesAvailable = 0;
        bytesUsed = 0;

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.EnabledPropertyName ], out enabled ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.EnabledPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.EnabledPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.TakeSnapshotsPropertyName ], out takeSnapshots ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.TakeSnapshotsPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.TakeSnapshotsPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.PruneSnapshotsPropertyName ], out pruneSnapshots ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.PruneSnapshotsPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.PruneSnapshotsPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ], out lastFrequentSnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ], out lastHourlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ], out lastDailySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ], out lastWeeklySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ], out lastMonthlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ], out lastYearlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        recursion = ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.RecursionPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.RecursionPropertyName ].Value, rawZfsObject.Properties[ ZfsPropertyNames.RecursionPropertyName ].Source == ZfsPropertySourceConstants.Local );
        sourceSystem = ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.SourceSystem, rawZfsObject.Properties[ ZfsPropertyNames.SourceSystem ].Value, rawZfsObject.Properties[ ZfsPropertyNames.SourceSystem ].Source == ZfsPropertySourceConstants.Local );
        template = ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.TemplatePropertyName, rawZfsObject.Properties[ ZfsPropertyNames.TemplatePropertyName ].Value, rawZfsObject.Properties[ ZfsPropertyNames.TemplatePropertyName ].Source == ZfsPropertySourceConstants.Local );

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ], out retentionFrequent ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ], out retentionHourly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ], out retentionDaily ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionDailyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ], out retentionWeekly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ], out retentionMonthly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ], out retentionYearly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ], out retentionPruneDeferral ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( rawZfsObject.Kind != ZfsPropertyValueConstants.Snapshot && !long.TryParse( rawZfsObject.Properties[ ZfsNativePropertyNames.Available ].Value, out bytesAvailable ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsNativePropertyNames.Available, rawZfsObject.Properties[ ZfsNativePropertyNames.Available ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out _, out bytesUsed );
            bytesAvailable = 0;
            return false;
        }

        // Keeping this one as-is for consistency with the rest of the method
        // ReSharper disable once InvertIf
        if ( rawZfsObject.Kind != ZfsPropertyValueConstants.Snapshot && !long.TryParse( rawZfsObject.Properties[ ZfsNativePropertyNames.Used ].Value, out bytesUsed ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsNativePropertyNames.Used, rawZfsObject.Properties[ ZfsNativePropertyNames.Used ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out enabled, out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out sourceSystem, out bytesAvailable, out _ );
            bytesUsed = 0;
            return false;
        }

        return true;

        static void SetAllOutParametersNull( out ZfsProperty<bool>? enabled, out ZfsProperty<bool>? takeSnapshots, out ZfsProperty<bool>? pruneSnapshots, out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, out ZfsProperty<int>? retentionFrequent, out ZfsProperty<int>? retentionHourly, out ZfsProperty<int>? retentionDaily, out ZfsProperty<int>? retentionWeekly, out ZfsProperty<int>? retentionMonthly, out ZfsProperty<int>? retentionYearly, out ZfsProperty<int>? retentionPruneDeferral, out ZfsProperty<string> sourceSystem, out long bytesAvailable, out long bytesUsed )
        {
            enabled = null;
            takeSnapshots = null;
            pruneSnapshots = null;
            lastFrequentSnapshotTimestamp = null;
            lastHourlySnapshotTimestamp = null;
            lastDailySnapshotTimestamp = null;
            lastWeeklySnapshotTimestamp = null;
            lastMonthlySnapshotTimestamp = null;
            lastYearlySnapshotTimestamp = null;
            recursion = default;
            template = default;
            retentionFrequent = null;
            retentionHourly = null;
            retentionDaily = null;
            retentionWeekly = null;
            retentionMonthly = null;
            retentionYearly = null;
            retentionPruneDeferral = null;
            sourceSystem = default;
            bytesAvailable = 0L;
            bytesUsed = 0L;
        }
    }
}
