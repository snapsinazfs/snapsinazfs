// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MissingXmlDoc.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MissingXmlDoc

using System.Runtime.InteropServices;

namespace Sanoid.Interop.Libc.Structs;

/// <summary>
///     The data structure returned by a call to <see cref="NativeMethods.StatFs64" />
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 2)]
    public int[] f_fsid;

    public ulong f_namelen;
    public ulong f_frsize;
    public ulong f_flags;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 4)]
    public ulong[] f_spare;
#pragma warning restore CS1591
}
