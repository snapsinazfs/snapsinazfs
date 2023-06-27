// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

/// <summary>
///     Immutable value type representing the raw string values of a ZFS property's name, value, and source attributes
/// </summary>
/// <param name="Name">The string corresponding to the 'property' attribute of a ZFS property</param>
/// <param name="Value">The string corresponding to the 'value' attribute of a ZFS property</param>
/// <param name="Source">The string corresponding to the 'source' attribute of a ZFS property</param>
public readonly record struct RawProperty( string Name, string Value, string Source );
