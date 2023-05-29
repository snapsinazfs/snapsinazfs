using System.Text.RegularExpressions;

namespace Sanoid.Interop.Zfs.ZfsTypes.Validation;

public class WindowsZfsIdentifierValidator : IZfsIdentifierRegexes
{
    /// <inheritdoc />
    public Regex DatasetNameRegex()
    {
        return WindowsZfsIdentifierRegexes.DatasetNameRegex();
    }

    /// <inheritdoc />
    public Regex SnapshotNameRegex()
    {
        return WindowsZfsIdentifierRegexes.SnapshotNameRegex();
    }

    /// <inheritdoc />
    public Regex BookmarkNameRegex()
    {
        return WindowsZfsIdentifierRegexes.BookmarkNameRegex();
    }
}
