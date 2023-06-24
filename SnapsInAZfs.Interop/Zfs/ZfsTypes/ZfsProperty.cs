// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Text.Json.Serialization;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public readonly record struct ZfsProperty<T>( string Name, T Value, string Source ) : IZfsProperty where T : notnull
{
    /// <summary>
    ///     Gets whether this is a SnapsInAZfs property or not
    /// </summary>
    /// <remarks>Set by constructor, if property name begins with "snapsinazfs.com:"</remarks>
    [JsonIgnore]
    public bool IsSnapsInAZfsProperty { get; } = Name.StartsWith( "snapsinazfs.com:" );

    /// <summary>
    ///     Gets a boolean indicating if this property is a SnapsInAZfs property, is a string, and is equal to "-"
    /// </summary>
    [JsonIgnore]
    public bool IsUndefinedOrDefault => IsSnapsInAZfsProperty && Value is "-";

    /// <summary>
    ///     Gets a string representation of the Value property, in an appropriate form for its type
    /// </summary>
    [JsonIgnore]
    public string ValueString => Value switch
    {
        int intValue => intValue.ToString( ),
        string value => value,
        bool boolValue => boolValue.ToString( ).ToLowerInvariant( ),
        DateTimeOffset dtoValue => dtoValue.ToString( "O" ),
        _ => throw new InvalidOperationException( $"Invalid value type for ZfsProperty {Name} ({typeof( T ).FullName})" )
    };

    [JsonIgnore]
    public bool IsInherited => Source.StartsWith( "inherited" );

    [JsonIgnore]
    public string InheritedFrom => IsInherited ? Source[ 15.. ] : Source;

    [JsonIgnore]
    public bool IsLocal => Source == "local";

    [JsonIgnore]
    public string SetString => $"{Name}={ValueString}";
}
