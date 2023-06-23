// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Runtime.InteropServices;
using SnapsInAZfs.Interop.Libc.Enums;

namespace SnapsInAZfs.Interop.Libc;

/// <summary>
///     Class for access to system calls
/// </summary>
public static partial class NativeMethods
{
    /// <summary>
    ///     The libc canonicalize_file_name function. Takes a path and returns its canonical form.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "canonicalize_file_name", SetLastError = true )]
    public static partial string CanonicalizeFileName( string path );

    /// <summary>
    ///     The libc euidaccess function. Tests the effective access for the calling user against the given file and mode.
    /// </summary>
    /// <param name="pathname"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "euidaccess", SetLastError = true )]
    public static partial int EuidAccess( string pathname, UnixFileTestMode mode );

    /// <summary>
    ///     The libc truncate function. Sets a file to the specified length in bytes.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "truncate", SetLastError = true )]
    public static partial int Truncate( string path, long length );

    /// <summary>
    ///     The libc unlink function. Deletes a file system link, and the file itself, if it is the last remaining link.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "unlink", SetLastError = true )]
    public static partial int Unlink( string path );

    /// <summary>
    ///     The libc open function. Opens a file.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="flags"></param>
    /// <param name="mode"></param>
    /// <returns>On success, returns a file descriptor for the opened file.</returns>
    [LibraryImport( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "open", SetLastError = true )]
    public static partial int Open( string path, UnixFileFlags flags, UnixFileMode mode );

    /// <summary>
    ///     The libc close function. Closes a file descriptor.
    /// </summary>
    /// <param name="fd"></param>
    /// <returns>0 on success</returns>
    [LibraryImport( "libc", EntryPoint = "close", SetLastError = true )]
    public static partial int Close( int fd );
}
