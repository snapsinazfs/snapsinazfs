// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;
#if WINDOWS
/// <summary>
///     Pre-generated regular expressions which follow the naming rules outlined in the zfs(8) man page as defined in
///     version 2.1.11 of ZFS, but using backslashes as the path separator, for use on Windows
/// </summary>
public static partial class WindowsZfsIdentifierRegexes
{
    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)(?<Dataset>\\[A-Za-z0-9_.:-]*[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]+)*$", RegexOptions.Compiled )]
    public static partial Regex DatasetNameRegex( );

    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)(?<Dataset>\\[A-Za-z0-9_.:-]*[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]+)*@(?<Snapshot>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)$", RegexOptions.Compiled )]
    public static partial Regex SnapshotNameRegex( );

    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)(?<Dataset>\\[A-Za-z0-9_.:-]*[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]+)*#(?<Bookmark>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)$", RegexOptions.Compiled )]
    public static partial Regex BookmarkNameRegex( );
}
#endif
