// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

public sealed class NameValidationTestCase( string name, bool valid )
{
    public string Name { get; set; } = name;
    public bool Valid { get; set; } = valid;
}
