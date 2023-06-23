// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using SnapsInAZfs.Interop.Libc.Enums;

namespace SnapsInAZfs.Interop.Concurrency;

/// <summary>
///     An enumeration type restricted to the specific values that a MutexAcquisitionException can contain.
/// </summary>
/// <remarks>
///     All values correspond to specific POSIX <see cref="Errno" /> values, but have more explicit names for their
///     use cases in this library.
/// </remarks>
public enum MutexAcquisitionErrno
{
    /// <summary>Operation was successful</summary>
    /// <value>Equivalent to <see cref="Errno.EOK" /></value>
    Success = Errno.EOK,

    /// <summary>The operation is still in progress.</summary>
    /// <value>Equivalent to <see cref="Errno.EINPROGRESS" /></value>
    InProgess = Errno.EINPROGRESS,

    /// <summary>An IOException was raised. Usually means the name was invalid.</summary>
    /// <value>Equivalent to <see cref="Errno.EINVAL" /></value>
    IoException = Errno.EINVAL,

    /// <summary>An abandomed mutex was encountered. Caller may try again.</summary>
    /// <value>Equivalent to <see cref="Errno.EAGAIN" /></value>
    AbandonedMutex = Errno.EAGAIN,

    /// <summary>A <see cref="WaitHandleCannotBeOpenedException" /> was encountered. Likely fatal.</summary>
    /// <value>Equivalent to <see cref="Errno.EEXIST" /></value>
    WaitHandleCannotBeOpened = Errno.EEXIST,

    /// <summary>Mutex couldn't be acquired and is probably null.</summary>
    /// <value>Equivalent to <see cref="Errno.ENOLCK" /></value>
    PossiblyNullMutex = Errno.ENOLCK,

    /// <summary>Mutex couldn't be acquired because another process is using it.</summary>
    /// <value>Equivalent to <see cref="Errno.EBUSY" /></value>
    AnotherProcessIsBusy = Errno.EBUSY,

    /// <summary>A null, empty, or whitespace string was supplied as the mutex name.</summary>
    /// <value>Equivalent to <see cref="Errno.EBADR" /></value>
    InvalidMutexNameRequested = Errno.EBADR
}
