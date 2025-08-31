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

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MissingXmlDoc.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MissingXmlDoc

namespace SnapsInAZfs.Interop.Libc.Structs;

using System.Runtime.InteropServices;

/// <summary>
///     The data structure returned by a call to <see cref="StatFs64" />
/// </summary>
[StructLayout ( LayoutKind.Sequential, CharSet = CharSet.Ansi )]
public class StatFs64
{
#pragma warning disable CS1591
    public ulong f_type;
    public ulong f_bsize;
    public ulong f_blocks;
    public ulong f_bfree;
    public ulong f_bavail;
    public ulong f_files;
    public ulong f_ffree;

    [MarshalAs ( UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 2 )]
    public int[] f_fsid;

    public ulong f_namelen;
    public ulong f_frsize;
    public ulong f_flags;

    [MarshalAs ( UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 4 )]
    public ulong[] f_spare;
#pragma warning restore CS1591
}
