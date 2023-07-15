// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public struct ZfsProperty<T> : IZfsProperty, IEquatable<T>, IEquatable<ZfsProperty<T>> where T : notnull
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

    /// <summary>
    ///     Compares equality of the <see cref="Value" /> property of this <see cref="ZfsProperty{T}" /> and <typeparamref name="T"/> <paramref name="other" />
    ///     using the default <see cref="EqualityComparer{T}" /> for <typeparamref name="T" />
    /// </summary>
    readonly bool IEquatable<T>.Equals( T? other )
    {
        return EqualityComparer<T>.Default.Equals( Value, other );
    }

    /// <summary>
    ///     Compares equality of this <see cref="ZfsProperty{T}" /> and <paramref name="other" />, using the <see cref="Name" />,
    ///     <see cref="Value" />, and <see cref="IsLocal"/> properties ONLY.
    /// </summary>
    /// <remarks>
    ///     Type <typeparamref name="T" /> of <paramref name="other" /> must be the same as this <see cref="ZfsProperty{T}" />
    /// </remarks>
    public readonly bool Equals( ZfsProperty<T> other )
    {
        return Name == other.Name && IsLocal == other.IsLocal && ( Value, other.Value ) switch
        {
            (int v, int o) => v == o,
            (string v, string o) => v == o,
            (DateTimeOffset v, DateTimeOffset o) => v == o,
            (bool v, bool o) => v == o,
            _ => false
        };
    }

    public readonly bool Equals<TO>( TO other ) where TO : notnull
    {
        return ( Value, other ) switch
        {
            (int v, int o) => v == o,
            (string v, string o) => v == o,
            (DateTimeOffset v, DateTimeOffset o) => v == o,
            (bool v, bool o) => v == o,
            (int v, ZfsProperty<int> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (string v, ZfsProperty<string> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (DateTimeOffset v, ZfsProperty<DateTimeOffset> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (bool v, ZfsProperty<bool> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            _ => false
        };
    }

    /// <inheritdoc />
    public readonly override bool Equals( object? obj )
    {
        if ( obj == null )
        {
            return false;
        }

        return ( Value, obj ) switch
        {
            (int v, int o) => v == o,
            (string v, string o) => v == o,
            (DateTimeOffset v, DateTimeOffset o) => v == o,
            (bool v, bool o) => v == o,
            (int v, ZfsProperty<int> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (string v, ZfsProperty<string> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (DateTimeOffset v, ZfsProperty<DateTimeOffset> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            (bool v, ZfsProperty<bool> o) => Name == o.Name && v == o.Value && IsLocal == o.IsLocal,
            _ => false
        };
    }

    public static bool operator ==( ZfsProperty<T> left, object right )
    {
        return left.Equals( right );
    }
    public static bool operator !=( ZfsProperty<T> left, object right )
    {
        return !left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<T> left, T right )
    {
        return left.Equals( right );
    }
    public static bool operator !=( ZfsProperty<T> left, T right )
    {
        return !left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<int> left, ZfsProperty<T> right )
    {
        return left.Equals( right );
    }
    public static bool operator !=( ZfsProperty<int> left, ZfsProperty<T> right )
    {
        return !left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<DateTimeOffset> left, ZfsProperty<T> right )
    {
        return left.Equals( right );
    }
    public static bool operator !=( ZfsProperty<DateTimeOffset> left, ZfsProperty<T> right )
    {
        return !left.Equals( right );
    }

    public static bool operator ==( ZfsProperty<bool> left, ZfsProperty<T> right )
    {
        return left.Equals( right );
    }
    public static bool operator !=( ZfsProperty<bool> left, ZfsProperty<T> right )
    {
        return !left.Equals( right );
    }

    //public static bool operator ==( ZfsProperty<T> left, ZfsProperty<string> right )
    //{
    //    return left.Equals( right );
    //}
    //public static bool operator !=( ZfsProperty<T> left, ZfsProperty<string> right )
    //{
    //    return !left.Equals( right );
    //}

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<T> right )
    {
        return left.Equals( right );
    }
    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<T> right )
    {
        return !left.Equals( right );
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
        SnapshotPeriod periodValue => periodValue.ToString( ),
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
    public readonly string InheritedFrom => IsLocal ? ZfsPropertySourceConstants.Local : Source[ 15.. ];

    public ZfsRecord ChangeParentReference( ZfsRecord newParent )
    {
        return Owner = newParent;
    }

    public static ZfsProperty<bool> CreateWithoutParent( string name, in bool value, bool isLocal = true )
    {
        Logger.Trace("Creating ZfsProperty<bool> {0} without parent dataset", name  );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<int> CreateWithoutParent( string name, in int value, bool isLocal = true )
    {
        Logger.Trace("Creating ZfsProperty<int> {0} without parent dataset", name  );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<string> CreateWithoutParent( string name, string value, bool isLocal = true )
    {
        Logger.Trace("Creating ZfsProperty<string> {0} without parent dataset", name  );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<T> CreateWithoutParent( string name, T value, in bool isLocal = true )
    {
        Logger.Trace("Creating ZfsProperty<T> {0} without parent dataset", name  );
        return new( name, in value, isLocal );
    }

    public static ZfsProperty<DateTimeOffset> CreateWithoutParent( string name, in DateTimeOffset value, bool isLocal = true )
    {
        Logger.Trace("Creating ZfsProperty<DateTimeOffset> {0} without parent dataset", name  );
        return new( name, in value, isLocal );
    }

    // ReSharper disable InconsistentNaming
    // ReSharper disable ParameterHidesMember
    public readonly void Deconstruct( out ZfsRecord? Parent, out string Name, out T Value, out bool IsLocal )
    {
        Parent = Owner;
        Name = this.Name;
        Value = this.Value;
        IsLocal = this.IsLocal;
    }
    // ReSharper enable InconsistentNaming
    // ReSharper enable ParameterHidesMember

    /// <inheritdoc />
    public readonly override int GetHashCode( )
    {
        return HashCode.Combine( Value, Name, IsLocal );
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
