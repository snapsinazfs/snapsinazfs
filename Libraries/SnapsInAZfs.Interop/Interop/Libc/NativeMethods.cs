#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace SnapsInAZfs.Interop.Libc;

using System.Runtime.InteropServices;
using Enums;
using JetBrains.Annotations;

/// <summary>
///   Class for access to system calls
/// </summary>
[PublicAPI]
public static partial class NativeMethods
{
  /// <summary>
  ///   The libc canonicalize_file_name function. Takes a path and returns its canonical form.
  /// </summary>
  /// <param name="path"></param>
  /// <returns></returns>
  [LibraryImport ( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "canonicalize_file_name", SetLastError = true )]
  public static partial string CanonicalizeFileName ( string path );

  /// <summary>
  ///   The libc close function. Closes a file descriptor.
  /// </summary>
  /// <param name="fd"></param>
  /// <returns>0 on success</returns>
  [LibraryImport ( "libc", EntryPoint = "close", SetLastError = true )]
  public static partial int Close ( int fd );

  /// <summary>
  ///   The libc euidaccess function. Tests the effective access for the calling user against the given file and mode mask.
  /// </summary>
  /// <param name="pathname">Path to evaluate for access.</param>
  /// <param name="mode">
  ///   A standard <see cref="UnixFileMode" /> to use as a mask for requested permissions to the target path.<br />
  ///   The current path will be resolved and its effective access for the executing uid and gid will be checked against this mask.
  ///   <br />
  ///   If any bits in this mask are missing in the result, -1 will be returned and <c>LastError</c> will be set to a specific POSIX
  ///   exit code.
  /// </param>
  /// <returns>
  ///   0, if all bits in <paramref name="mode" /> are granted in the effective permissions of the file;<br />
  ///   -1 otherwise.
  /// </returns>
  /// <remarks>
  ///   Generally, it is better to attempt to open the file in the desired mode, instead, as test-then-open introduces a race
  ///   condition.
  /// </remarks>
  // ReSharper disable once StringLiteralTypo
  [LibraryImport ( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "euidaccess", SetLastError = true )]
  public static partial int EuidAccess ( string pathname, __mode_t mode );

  // ReSharper disable once StringLiteralTypo
  [LibraryImport ( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "gethostname", SetLastError = true )]
  public static partial int GetHostName ( string name, uint len );

  /// <summary>
  ///   The libc open function. Opens a file.
  /// </summary>
  /// <param name="path">Path to the file.</param>
  /// <param name="flags"></param>
  /// <param name="mode"></param>
  /// <returns>On success, returns a file descriptor for the opened file.</returns>
  [LibraryImport ( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "open", SetLastError = true )]
  public static partial int Open ( string path, UnixFileFlags flags, UnixFileMode mode );

  /// <summary>
  ///   The libc truncate function. Sets a file to the specified length in bytes.
  /// </summary>
  /// <param name="path"></param>
  /// <param name="length"></param>
  /// <returns></returns>
  [LibraryImport ( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "truncate", SetLastError = true )]
  public static partial int Truncate ( string path, long length );

  /// <summary>
  ///   The libc unlink function. Deletes a file system link, and the file itself, if it is the last remaining link.
  /// </summary>
  /// <param name="path"></param>
  /// <returns></returns>
  [LibraryImport ( "libc", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "unlink", SetLastError = true )]
  public static partial int Unlink ( string path );
}
