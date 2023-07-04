// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

public class NameValidationTestCase
{
    public NameValidationTestCase(string name, bool valid )
    {
        this.Name = name;
        this.Valid = valid;
    }

    public string Name { get; set; }
    public bool Valid { get; set; }
}
