// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public record struct ZfsProperty<T> : IZfsProperty, IEquatable<T> where T : notnull
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private ZfsProperty( string Name, in T Value, bool IsLocal = true )
    {
        this.Name = Name;
        this.Value = Value;
        this.IsLocal = IsLocal;
        IsSnapsInAZfsProperty = Name.StartsWith( "snapsinazfs.com:" );
    }

    public ZfsProperty( ZfsRecord Owner, string Name, in T Value, bool IsLocal = true )
    {
        this.Owner = Owner;
        this.Name = Name;
        this.Value = Value;
        this.IsLocal = IsLocal;
        IsSnapsInAZfsProperty = Name.StartsWith( "snapsinazfs.com:" );
    }

    /// <summary>
    ///     Gets whether this is a SnapsInAZfs property or not
    /// </summary>
    /// <remarks>Set by constructor, if property name begins with "snapsinazfs.com:"</remarks>
    [JsonIgnore]
    public bool IsSnapsInAZfsProperty { get; }

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
    ///     Compares equality of the <see cref="Value" /> property of this <see cref="ZfsProperty{T}" /> and <paramref name="other" />
    ///     using the default <see cref="EqualityComparer{T}" /> for <typeparamref name="T" />
    /// </summary>
    public readonly bool Equals( T? other )
    {
        return EqualityComparer<T>.Default.Equals( Value, other );
    }

    /// <summary>
    ///     Compares equality of this <see cref="ZfsProperty{T}" /> and <paramref name="other" />, using the <see cref="Name" /> and
    ///     <see cref="Value" /> properties ONLY.
    /// </summary>
    /// <remarks>
    ///     Type <typeparamref name="T" /> of <paramref name="other" /> must be the same as this <see cref="ZfsProperty{T}" />
    /// </remarks>
    public readonly bool Equals( ZfsProperty<T> other )
    {
        return Name == other.Name && EqualityComparer<T>.Default.Equals( Value, other.Value ) && IsLocal == other.IsLocal;
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
