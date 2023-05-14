// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Runtime.InteropServices;
using Sanoid.Common.Posix;

namespace Sanoid.Common.Tests;

public static partial class NativeFunctions
{
    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "canonicalize_file_name", SetLastError = true)]
    public static partial string CanonicalizeFileName( string path );

    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "euidaccess", SetLastError = true)]
    public static partial int EuidAccess( string pathname, UnixFileTestMode mode );

    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "truncate", SetLastError = true)]
    public static partial int Truncate( string path, long length );

    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "unlink", SetLastError = true)]
    public static partial int Unlink( string path );

    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "open", SetLastError = true)]
    public static partial int Open( string path, UnixFileFlags flags, UnixFileMode mode );

    [LibraryImport( "libc",EntryPoint = "close", SetLastError = true)]
    public static partial int Close( int fd );
}
