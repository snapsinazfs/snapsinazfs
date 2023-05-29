using System.Text.RegularExpressions;

namespace Sanoid.Interop.Zfs.ZfsTypes.Validation;

public interface IZfsIdentifierRegexes
{
    /// <summary>
    /// Gets a <see cref="Regex"/> for validation of <see cref="Dataset"/> names
    /// </summary>
    Regex DatasetNameRegex();
    /// <summary>
    /// Gets a <see cref="Regex"/> for validation of <see cref="Snapshot"/> names
    /// </summary>
    Regex SnapshotNameRegex();
    /// <summary>
    /// Gets a <see cref="Regex"/> for validation of Bookmark names
    /// </summary>
    Regex BookmarkNameRegex();
}
