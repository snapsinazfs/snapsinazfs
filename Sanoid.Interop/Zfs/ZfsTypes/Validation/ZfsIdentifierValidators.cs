// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.RegularExpressions;

namespace Sanoid.Interop.Zfs.ZfsTypes.Validation;

public class ZfsIdentifierValidator : IZfsIdentifierRegexes
{
    /// <inheritdoc />
    public Regex DatasetNameRegex()
    {
        return ZfsIdentifierRegexes.DatasetNameRegex();
    }

    /// <inheritdoc />
    public Regex SnapshotNameRegex()
    {
        return ZfsIdentifierRegexes.SnapshotNameRegex();
    }

    /// <inheritdoc />
    public Regex BookmarkNameRegex()
    {
        return ZfsIdentifierRegexes.BookmarkNameRegex();
    }
}
