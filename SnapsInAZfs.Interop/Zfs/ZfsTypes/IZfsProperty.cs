// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public interface IZfsProperty
{
    string InheritedFrom { get; }
    bool IsInherited { get; }
    bool IsLocal { get; }
    string Name { get; }
    string SetString { get; }
    string Source { get; }
}
