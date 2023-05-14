// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Runtime.InteropServices;

namespace Sanoid.Common.Posix;

[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi )]
public struct StatFs64
{
    public ulong f_type;
    public ulong f_bsize;
    public ulong f_blocks;
    public ulong f_bfree;
    public ulong f_bavail;
    public ulong f_files;
    public ulong f_ffree;

    [MarshalAs( UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 2 )]
    public int[] f_fsid;

    public ulong f_namelen;
    public ulong f_frsize;
    public ulong f_flags;

    [MarshalAs( UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 4 )]
    public ulong[] f_spare;
}
