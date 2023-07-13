// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Text.RegularExpressions;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;

/// <summary>
///     Pre-generated regular expressions which follow the naming rules outlined in the zfs(8) man page as defined in
///     version 2.1.11 of ZFS
/// </summary>
public static partial class ZfsIdentifierRegexes
{
    [GeneratedRegex( @"^(?<Pool>[A-Za-z]+[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})(?<Dataset>/[A-Za-z0-9_.:-]?[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})*$", RegexOptions.Compiled )]
    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]?[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})(?<Dataset>/[A-Za-z0-9_.:-]?[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})*$", RegexOptions.Compiled )]
    public static partial Regex DatasetNameRegex( );

    [GeneratedRegex( @"^(?<Pool>[A-Za-z0-9_.:-]?[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})(?<Dataset>/[A-Za-z0-9_.:-]?[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})*@(?<Snapshot>[A-Za-z0-9_.:-]?[A-Za-z0-9_.: -]*[A-Za-z0-9_.:-]{1})$", RegexOptions.Compiled )]
    public static partial Regex SnapshotNameRegex( );
}
