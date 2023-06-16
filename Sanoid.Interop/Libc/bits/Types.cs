// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

// ReSharper disable RedundantUsingDirective.Global
// ReSharper disable IdentifierTypo
// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

// This file contains what amount to the C# equivalent of the typedefs in bits/types.h in GNU libc
// The basic assumption is made that we are on a 64-bit system
// This is not guaranteed to work (and likely WILL NOT work) on a 32-bit system

global using __u_char = System.Byte;
global using __u_short = System.UInt16;
global using __u_int = System.UInt32;
global using __u_long = System.UInt64;
global using __int8_t = System.SByte;
global using __uint8_t = System.Byte;
global using __int16_t = System.Int16;
global using __uint16_t = System.UInt16;
global using __int32_t = System.Int32;
global using __uint32_t = System.UInt32;
global using __int64_t = System.Int64;
global using __uint64_t = System.UInt64;
global using __int_least8_t = System.Byte;
global using __uint_least8_t = System.Byte;
global using __int_least16_t = System.Int16;
global using __uint_least16_t = System.UInt16;
global using __int_least32_t = System.Int32;
global using __uint_least32_t = System.UInt32;
global using __int_least64_t = System.Int64;
global using __uint_least64_t = System.UInt64;
global using __quad_t = System.Int64;
global using __u_quad_t = System.UInt64;
global using __intmax_t = System.Int64;
global using __uintmax_t = System.UInt64;
global using __S16_TYPE = System.Int16;
global using __U16_TYPE = System.UInt16;
global using __S32_TYPE = System.Int32;
global using __U32_TYPE = System.UInt32;
global using __SLONGWORD_TYPE = System.Int64;
global using __ULONGWORD_TYPE = System.UInt64;
global using __SQUAD_TYPE = System.Int64;
global using __UQUAD_TYPE = System.UInt64;
global using __SWORD_TYPE = System.Int32;
global using __UWORD_TYPE = System.UInt32;
global using __SLONG32_TYPE = System.Int32;
global using __ULONG32_TYPE = System.UInt32;
global using __S64_TYPE = System.Int64;
global using __U64_TYPE = System.UInt64;
global using __dev_t = System.UInt64;
global using __uid_t = System.UInt32;
global using __gid_t = System.UInt32;
global using __ino_t = System.UInt64;
global using __ino64_t = System.UInt64;
global using __mode_t = System.UInt32;
global using __nlink_t = System.UInt64;
global using __off_t = System.Int64;
global using __off64_t = System.Int64;
global using __pid_t = System.Int32;
//global using __fsid_t = __FSID_T_TYPE;
global using __rlim_t = System.UInt64;
global using __rlim64_t = System.UInt64;
global using __id_t = System.UInt32;
global using __clock_t = System.Int64;
global using __time_t = System.Int64;
global using __useconds_t = System.UInt32;
global using __suseconds_t = System.Int64;
global using __suseconds64_t = System.Int64;
global using __daddr_t = System.Int32;
global using __key_t = System.Int32;
global using __clockid_t = System.Int32;
global using __timer_t = System.IntPtr; // void*
global using __blksize_t = System.Int64;
global using __blkcnt_t = System.Int64;
global using __blkcnt64_t = System.Int64;
global using __fsblkcnt_t = System.UInt64;
global using __fsblkcnt64_t = System.UInt64;
global using __fsfilcnt_t = System.UInt64;
global using __fsfilcnt64_t = System.UInt64;
global using __fsword_t = System.Int64;
global using __ssize_t = System.Int32;
global using __syscall_slong_t = System.Int64;
global using __syscall_ulong_t = System.UInt64;
global using __loff_t = System.Int64;
global using __intptr_t = System.Int32;
global using __socklen_t = System.UInt32;
global using __sig_atomic_t = System.Int32;
