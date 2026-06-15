namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

public sealed class NameValidationTestCase( string name, bool valid )
{
    public string Name { get; set; } = name;
    public bool Valid { get; set; } = valid;
}
