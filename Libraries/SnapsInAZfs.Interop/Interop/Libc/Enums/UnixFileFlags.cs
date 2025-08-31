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

namespace SnapsInAZfs.Interop.Libc.Enums;

using JetBrains.Annotations;

/// <summary>
///     File open flags.
/// </summary>
[Flags]
[PublicAPI]
public enum UnixFileFlags : uint
{
    /// <summary>
    ///     Open for read-only access.
    /// </summary>
    /// <remarks>
    ///     Also often used to test for existence, as there is no such thing as an actual read-only flag, since this
    ///     value is zero and thus cannot actually be set or cleared.
    /// </remarks>
    ReadOnly = 0,

    /// <summary>
    ///     Open for write-only access. Data can be written but not read.
    /// </summary>
    /// <remarks>
    ///     Write-only does not mean forward-only, and a file descriptor opened with this flag MAY still be seekable and thus its
    ///     contents may be overwritten blindly.
    /// </remarks>
    Write = 0x1,

    /// <summary>
    ///     Open for read and write access.
    /// </summary>
    ReadWrite = 0x2,

    /// <summary>
    ///     Create a new file or open an existing file.
    /// </summary>
    Create = 0x40,

    /// <summary>
    ///     Create a new file only. Fail if the file already exists.
    /// </summary>
    CreateNew = 0x80,

    /// <summary>
    ///     Ensures the file does not have a controlling terminal.
    /// </summary>
    PseudoTerminal = 0x100,

    /// <summary>
    ///     Truncate file on open, if it already exists, and position the stream at offset 0.
    /// </summary>
    Truncate = 0x200,

    /// <summary>
    ///     For files opened with write permission, all writes will be performed at the end of the stream, regardless of the location of
    ///     the stream pointer before the write operation.
    /// </summary>
    /// <remarks>
    ///     This behavior is independent of read access and permission and has no effect on reads.
    /// </remarks>
    AppendOnly = 0x400,

    /// <summary>
    ///     Request non-blocking file IO.
    /// </summary>
    NonBlocking = 0x800,

    /// <summary>
    ///     Request that DATA operations are carried out synchronously.
    /// </summary>
    /// <remarks>
    ///     Metadata operations may still be performed asynchronously by the system except those required as a consequence of data IO.
    /// </remarks>
    SynchronousData = 0x1000,

    /// <summary>
    ///     Request asynchronous signal-driven IO, such as for a socket, PTY, or other FIFO.
    /// </summary>
    /// <remarks>
    ///     The file descriptor must have its owner set to the ID of the process, so that it will receive signals.
    /// </remarks>
    Async = 0x2000,

    /// <summary>
    ///     Request that IO bypasses the page cache.
    /// </summary>
    Direct = 0x4000,

    /// <summary>
    ///     Explicitly requests the use of 64-bit offsets.
    /// </summary>
    /// <remarks>This flag is unnecessary on 64-bit platforms.</remarks>
    LargeFile = 0x8000,

    /// <summary>
    ///     Only open if the target is a directory. Fail otherwise.
    /// </summary>
    /// <remarks>
    ///     Cannot be combined with <see cref="CreateNew" /> and will fail when combined with <see cref="Create" />, if the directory
    ///     does not already exist.
    /// </remarks>
    DirectoryOnly = 0x10000,

    /// <summary>
    ///     Fail if the target is a symbolic link. Symbolic links at any non-leaf position will still be resolved.
    /// </summary>
    DoNotFollowSymLinkedLeaf = 0x20000,

    /// <summary>
    ///     Request that atime not be updated for the target on read operations.
    /// </summary>
    /// <remarks>
    ///     The operating system, file system, file permissions, and granted capabilities for the process must all align for this to be
    ///     honored.
    /// </remarks>
    NoATimeUpdate = 0x40000,

    /// <summary>
    ///     Set the close-on-exec flag at file open.
    /// </summary>
    /// <remarks>
    ///     The close-on-exec file descriptor flag (<c>FD_CLOEXEC</c>) causes the file descriptor to be closed if any of the
    ///     exec(le|lp|v|vp|vpe) functions are called.<br />
    ///     These functions replace the running process with another, so this is a security and resource leak protection.<br />
    ///     Setting this flag at open avoids a potential race condition arising from setting <c>FD_CLOEXEC</c> at a later point, after
    ///     the file
    ///     is already open.
    /// </remarks>
    CloseOnExec = 0x80000
}
