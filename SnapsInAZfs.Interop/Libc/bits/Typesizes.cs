// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

// This file contains what amount to the C# equivalent of the typedefs in bits/types.h in GNU libc
// The basic assumption is made that we are on a 64-bit system
// This is not guaranteed to work (and likely WILL NOT work) on a 32-bit system

global using __SYSCALL_SLONG_TYPE = System.Int64;
global using __SYSCALL_ULONG_TYPE = System.UInt64;
global using __DEV_T_TYPE = System.UInt64;
global using __UID_T_TYPE = System.UInt32;
global using __GID_T_TYPE = System.UInt32;
global using __INO_T_TYPE = System.UInt64;
global using __INO64_T_TYPE = System.UInt64;
global using __MODE_T_TYPE = System.UInt32;
global using __NLINK_T_TYPE = System.UInt64;
global using __FSWORD_T_TYPE = System.Int64;
global using __OFF_T_TYPE = System.Int64;
global using __OFF64_T_TYPE = System.Int64;
global using __PID_T_TYPE = System.Int32;
global using __RLIM_T_TYPE = System.UInt64;
global using __RLIM64_T_TYPE = System.UInt64;
global using __BLKCNT_T_TYPE = System.Int64;
global using __BLKCNT64_T_TYPE = System.Int64;
global using __FSBLKCNT_T_TYPE = System.UInt64;
global using __FSBLKCNT64_T_TYPE = System.UInt64;
global using __FSFILCNT_T_TYPE = System.UInt64;
global using __FSFILCNT64_T_TYPE = System.UInt64;
global using __ID_T_TYPE = System.UInt32;
global using __CLOCK_T_TYPE = System.Int64;
global using __TIME_T_TYPE = System.Int64;
global using __USECONDS_T_TYPE = System.UInt32;
global using __SUSECONDS_T_TYPE = System.Int64;
global using __SUSECONDS64_T_TYPE = System.Int64;
global using __DADDR_T_TYPE = System.Int32;
global using __KEY_T_TYPE = System.Int32;
global using __CLOCKID_T_TYPE = System.Int32;
global using __TIMER_T_TYPE = System.IntPtr; // void*
global using __BLKSIZE_T_TYPE = System.Int64;
global using __SSIZE_T_TYPE = System.Int32;
global using __CPU_MASK_TYPE = System.UInt64;
using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential, Size = 8 )]
internal struct __FSID_T_TYPE
{
    [MarshalAs( UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 2 )]
    internal int[] __val;
}
