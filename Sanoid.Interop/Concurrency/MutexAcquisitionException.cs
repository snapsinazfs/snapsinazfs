// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Interop.Libc.Enums;

namespace Sanoid.Interop.Concurrency;

/// <summary>
///     An exception type for errors acquiring mutexes
/// </summary>
/// <remarks>
///     Inherits from <see cref="InvalidOperationException" /> and includes a mandator POSIX <see cref="Errno" /> for
///     use in exception handling.
/// </remarks>
/// <seealso cref="InvalidOperationException" />
/// <seealso cref="Mutexes" />
public class MutexAcquisitionException : InvalidOperationException
{
    /// <summary>
    ///     Creates a new instance of a <see cref="MutexAcquisitionException" />
    /// </summary>
    /// <param name="statusCode">The POSIX <see cref="Errno" /> to include in the exception</param>
    /// <param name="innerException">The inner exception that caused this exception to be thrown</param>
    /// <param name="message">An optional message describing the exception</param>
    public MutexAcquisitionException( MutexAcquisitionErrno statusCode, Exception innerException, string? message = "Mutex acquisition failed." )
        : base( message, innerException )
    {
        StatusCode = statusCode;
    }

    /// <summary>
    ///     Creates a new instance of a <see cref="MutexAcquisitionException" />
    /// </summary>
    /// <param name="statusCode">The POSIX <see cref="Errno" /> to include in the exception</param>
    /// <param name="message">An optional message describing the exception</param>
    public MutexAcquisitionException( MutexAcquisitionErrno statusCode, string? message = "Mutex acquisition failed." )
        : base( message )
    {
        StatusCode = statusCode;
    }

    /// <summary>
    ///     Status set by the thrower of the exception
    /// </summary>
    public MutexAcquisitionErrno StatusCode { get; init; }
}
