// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

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

    public readonly string InheritedFrom => IsLocal ? ZfsPropertySourceConstants.Local : Source[ 15.. ];

    [JsonIgnore]
    public readonly string Source =>
        IsLocal switch
        {
            true => ZfsPropertySourceConstants.Local,
            false when Owner is null => ZfsPropertySourceConstants.None,
            false when Owner.ParentDataset[ Name ].IsLocal => $"inherited from {Owner.ParentDataset.Name}",
            false => Owner.ParentDataset[ Name ].Source
        };

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
        DateTimeOffset dtoValue => dtoValue.ToString( "O" ),
        _ => throw new InvalidOperationException( $"Invalid value type for ZfsProperty {Name} ({typeof( T ).FullName})" )
    };

    readonly string IZfsProperty.Source =>
        IsLocal switch
        {
            true => ZfsPropertySourceConstants.Local,
            false when Owner is null => ZfsPropertySourceConstants.None,
            false when Owner.ParentDataset[ Name ].IsLocal => $"inherited from {Owner.ParentDataset.Name}",
            false => Owner.ParentDataset[ Name ].Source
        };

    [JsonIgnore]
    public readonly bool IsInherited => !IsLocal;

    [JsonIgnore]
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

    public readonly bool Equals<TO>( TO other ) where TO : notnull
    {
        return ( Value, other ) switch
        {
            (int v, ZfsProperty<int> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (string v, ZfsProperty<string> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (DateTimeOffset v, ZfsProperty<DateTimeOffset> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (bool v, ZfsProperty<bool> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            _ => false
        };
    }

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
}
