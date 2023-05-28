// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

public static class DatasetKindExtensions
{
    public static DatasetKind ToDatasetKind( this string value )
    {
        return value switch
        {
            "filesystem" => DatasetKind.FileSystem,
            "volume" => DatasetKind.Volume,
            _ => throw new NotSupportedException( $"String value {value} not supported for conversion to DatasetKind" )
        };
    }
}
