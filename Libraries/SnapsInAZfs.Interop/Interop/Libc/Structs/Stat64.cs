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

namespace SnapsInAZfs.Interop.Libc.Structs;

using System.Runtime.InteropServices;
using JetBrains.Annotations;

/// <summary>
/// C stdlib stat64 structure, as it exists in the version of libc used by Linux kernels 6.9 and up.
/// </summary>
[PublicAPI]
[StructLayout ( LayoutKind.Explicit, CharSet = CharSet.Ansi, Size = STRUCT_SIZE )]
public struct Stat64
{
    [FieldOffset ( ST_ATIM_OFFSET )]
    private timespec st_atim; /* Time of last access */

    [FieldOffset ( ST_BLKSIZE_OFFSET )]
    private __blksize_t st_blksize; /* Block size for filesystem I/O */

    [FieldOffset ( ST_BLOCKS_OFFSET )]
    private __blkcnt_t st_blocks; /* Number of 512 B blocks allocated */

    [FieldOffset ( ST_CTIM_OFFSET )]
    private timespec st_ctim; /* Time of last status change */

    [FieldOffset ( ST_DEV_OFFSET )]
    private __dev_t st_dev; /* ID of device containing file */

    [FieldOffset ( ST_GID_OFFSET )]
    private __gid_t st_gid; /* Group ID of owner */

    [FieldOffset ( ST_INO_OFFSET )]
    private __ino_t st_ino; /* Inode number */

    [FieldOffset ( ST_MODE_OFFSET )]
    private __mode_t st_mode; /* File type and mode */

    [FieldOffset ( ST_MTIM_OFFSET )]
    private timespec st_mtim; /* Time of last modification */

    [FieldOffset ( ST_NLINK_OFFSET )]
    private __nlink_t st_nlink; /* Number of hard links */

    [FieldOffset ( ST_RDEV_OFFSET )]
    private __dev_t st_rdev; /* Device ID (if special file) */

    [FieldOffset ( ST_SIZE_OFFSET )]
    private __off_t st_size; /* Total size, in bytes */

    [FieldOffset ( ST_UID_OFFSET )]
    private __uid_t st_uid; /* User ID of owner */

    private const int ST_ATIM_OFFSET    = ST_BLOCKS_OFFSET  + sizeof (__blkcnt_t);
    private const int ST_BLKSIZE_OFFSET = ST_SIZE_OFFSET    + sizeof (__off_t);
    private const int ST_BLOCKS_OFFSET  = ST_BLKSIZE_OFFSET + sizeof (__blksize_t);
    private const int ST_CTIM_OFFSET    = ST_MTIM_OFFSET    + sizeof (timespec);
    private const int ST_DEV_OFFSET     = 0;
    private const int ST_GID_OFFSET     = ST_UID_OFFSET   + sizeof (__uid_t);
    private const int ST_INO_OFFSET     = ST_DEV_OFFSET   + sizeof (__dev_t);
    private const int ST_MODE_OFFSET    = ST_INO_OFFSET   + sizeof (__ino_t);
    private const int ST_MTIM_OFFSET    = ST_ATIM_OFFSET  + sizeof (timespec);
    private const int ST_NLINK_OFFSET   = ST_MODE_OFFSET  + sizeof (__mode_t);
    private const int ST_RDEV_OFFSET    = ST_GID_OFFSET   + sizeof (__gid_t);
    private const int ST_SIZE_OFFSET    = ST_RDEV_OFFSET  + sizeof (__dev_t);
    private const int ST_UID_OFFSET     = ST_NLINK_OFFSET + sizeof (__nlink_t);
    private const int STRUCT_SIZE       = ST_CTIM_OFFSET  + sizeof (timespec);
}
