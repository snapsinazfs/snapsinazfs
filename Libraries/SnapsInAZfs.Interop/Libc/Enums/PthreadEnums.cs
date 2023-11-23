// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Runtime.InteropServices;

namespace Sanoid.Interop.Libc.Enums;

public enum PthreadCreateKind
{
    PTHREAD_CREATE_JOINABLE,
    PTHREAD_CREATE_DETACHED
}

public enum PthreadMutexKind
{
    PTHREAD_MUTEX_TIMED_NP,
    PTHREAD_MUTEX_RECURSIVE_NP,
    PTHREAD_MUTEX_ERRORCHECK_NP,
    PTHREAD_MUTEX_ADAPTIVE_NP,
    PTHREAD_MUTEX_NORMAL = PTHREAD_MUTEX_TIMED_NP,
    PTHREAD_MUTEX_RECURSIVE = PTHREAD_MUTEX_RECURSIVE_NP,
    PTHREAD_MUTEX_ERRORCHECK = PTHREAD_MUTEX_ERRORCHECK_NP,
    PTHREAD_MUTEX_DEFAULT = PTHREAD_MUTEX_NORMAL,
    PTHREAD_MUTEX_FAST_NP = PTHREAD_MUTEX_TIMED_NP
}

public enum PthreadMutexRobustKind
{
    PTHREAD_MUTEX_STALLED,
    PTHREAD_MUTEX_STALLED_NP = PTHREAD_MUTEX_STALLED,
    PTHREAD_MUTEX_ROBUST,
    PTHREAD_MUTEX_ROBUST_NP = PTHREAD_MUTEX_ROBUST
}

public enum PthreadMutexPriority
{
    PTHREAD_PRIO_NONE,
    PTHREAD_PRIO_INHERIT,
    PTHREAD_PRIO_PROTECT
}

public enum PthreadRwLockType
{
    PTHREAD_RWLOCK_PREFER_READER_NP,
    PTHREAD_RWLOCK_PREFER_WRITER_NP,
    PTHREAD_RWLOCK_PREFER_WRITER_NONRECURSIVE_NP,
    PTHREAD_RWLOCK_DEFAULT_NP = PTHREAD_RWLOCK_PREFER_READER_NP
}

[StructLayout( LayoutKind.Explicit, Size = 56 )]
public struct pthread_attr_t
{
    [FieldOffset( 0 )]
    [MarshalAs( UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 56 )]
    public byte[] __size;

    [FieldOffset( 0 )]
    [MarshalAs( UnmanagedType.I8 )]
    public long __align;
}
