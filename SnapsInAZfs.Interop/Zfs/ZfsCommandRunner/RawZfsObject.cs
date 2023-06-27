// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

/// <summary>
///     Immutable reference type representing all of the string values of a ZFS object and its properties
/// </summary>
/// <param name="Name">The string corresponding to the 'name' attribute of a ZFS object</param>
/// <param name="Kind">The string corresponding to the 'type' attribute of a ZFS object</param>
/// <param name="Properties">
///     A collection of <see cref="RawProperty" /> values for a ZFS object, indexed and sorted by their 'property' name values
/// </param>
public record RawZfsObject( string Name, string Kind, SortedList<string, RawProperty> Properties )
{
    public RawZfsObject( string Name, string Kind ) : this( Name, Kind, new( ) )
    {
    }
}
