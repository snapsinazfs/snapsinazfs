// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Libc.Enums;

namespace SnapsInAZfs.Interop.Concurrency;

public static class MutexAcquisitionExtensionMethods
{
    public static Errno ToErrno( this MutexAcquisitionErrno from )
    {
        return (Errno)from;
    }
}
