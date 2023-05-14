// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Runtime.InteropServices;

namespace Sanoid.Common.Tests;

public static class NativeFunctions
{
    [DllImport( "libc", CharSet = CharSet.Auto )]
    public static extern string canonicalize_file_name( string path );

    [DllImport( "libc", CharSet = CharSet.Auto, SetLastError = true )]
    public static extern int euidaccess( string pathname, UnixFileTestMode mode );

    [DllImport( "libc", CharSet = CharSet.Auto, SetLastError = true )]
    public static extern int truncate( string path, long length );

    [DllImport( "libc", CharSet = CharSet.Auto, SetLastError = true )]
    public static extern int unlink( string path );

    [DllImport( "libc", CharSet = CharSet.Auto, SetLastError = true )]
    public static extern int open( string path, UnixFileFlags flags, UnixFileMode mode );

    [DllImport( "libc", CharSet = CharSet.Auto, SetLastError = true )]
    public static extern int close( int fd );
}
