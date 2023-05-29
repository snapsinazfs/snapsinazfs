// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.RegularExpressions;

namespace Sanoid.Interop.Zfs.ZfsTypes.Validation;

/// <summary>
///     Pre-generated regular expressions which follow the naming rules outlined in the zfs(8) man page as defined in
///     version 2.1.11 of ZFS
/// </summary>
public static partial class ZfsIdentifierRegexes
{
    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)(?<Dataset>/[A-Za-z0-9_.:-]*[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]+)*$", RegexOptions.Compiled )]
    public static partial Regex DatasetNameRegex( );

    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)(?<Dataset>/[A-Za-z0-9_.:-]*[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]+)*@(?<Snapshot>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)$", RegexOptions.Compiled )]
    public static partial Regex SnapshotNameRegex( );

    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)(?<Dataset>/[A-Za-z0-9_.:-]*[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]+)*#(?<Bookmark>[A-Za-z0-9_.:-]*?[A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]+)$", RegexOptions.Compiled )]
    public static partial Regex BookmarkNameRegex( );
}
