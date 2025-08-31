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

// ReSharper disable RedundantUsingDirective.Global
// ReSharper disable IdentifierTypo

// This file contains what amount to the C# equivalent of the typedefs in bits/types.h in GNU libc
// The basic assumption is made that we are on a 64-bit system
// This is not guaranteed to work (and likely WILL NOT work) on a 32-bit system.

global using __u_char = byte;
global using __u_short = ushort;
global using __u_int = uint;
global using __u_long = ulong;
global using __int8_t = sbyte;
global using __uint8_t = byte;
global using __int16_t = short;
global using __uint16_t = ushort;
global using __int32_t = int;
global using __uint32_t = uint;
global using __int64_t = long;
global using __uint64_t = ulong;
global using __int_least8_t = byte;
global using __uint_least8_t = byte;
global using __int_least16_t = short;
global using __uint_least16_t = ushort;
global using __int_least32_t = int;
global using __uint_least32_t = uint;
global using __int_least64_t = long;
global using __uint_least64_t = ulong;
global using __quad_t = long;
global using __u_quad_t = ulong;
global using __intmax_t = long;
global using __uintmax_t = ulong;
global using __S16_TYPE = short;
global using __U16_TYPE = ushort;
global using __S32_TYPE = int;
global using __U32_TYPE = uint;
global using __SLONGWORD_TYPE = long;
global using __ULONGWORD_TYPE = ulong;
global using __SQUAD_TYPE = long;
global using __UQUAD_TYPE = ulong;
global using __SWORD_TYPE = int;
global using __UWORD_TYPE = uint;
global using __SLONG32_TYPE = int;
global using __ULONG32_TYPE = uint;
global using __S64_TYPE = long;
global using __U64_TYPE = ulong;
global using __pid_t = int;
//global using __fsid_t = __FSID_T_TYPE;
global using __rlim_t = ulong;
global using __rlim64_t = ulong;
global using __id_t = uint;
global using __clock_t = long;
global using __useconds_t = uint;
global using __suseconds_t = long;
global using __suseconds64_t = long;
global using __daddr_t = int;
global using __key_t = int;
global using __clockid_t = int;
global using __timer_t = nint; // void*
global using __fsblkcnt_t = ulong;
global using __fsblkcnt64_t = ulong;
global using __fsfilcnt_t = ulong;
global using __fsfilcnt64_t = ulong;
global using __fsword_t = long;
global using __ssize_t = int;
global using __syscall_slong_t = long;
global using __syscall_ulong_t = ulong;
global using __loff_t = long;
global using __intptr_t = int;
global using __socklen_t = uint;
global using __sig_atomic_t = int;
