// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Diagnostics.CodeAnalysis;
using SnapsInAZfs.Interop.Libc.Enums;

namespace SnapsInAZfs.Interop.Concurrency;

/// <summary>
///     An immutable record returned by mutex acquisition functions
/// </summary>
public sealed class MutexAcquisitionResult : IDisposable
{
    private bool _disposed;
    private readonly Mutex _mutex;

    /// <summary>
    ///     Gets the <see cref="MutexAcquisitionErrno" /> that was set in the result.
    /// </summary>
    public MutexAcquisitionErrno ErrorCode { get; }

    /// <summary>
    ///     Gets any exception that was set when the <see cref="MutexAcquisitionResult" /> was created.
    /// </summary>
    public MutexAcquisitionException Exception { get; }

    /// <summary>
    ///     Gets whether this result represents a successful operation or not
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> computed as <see cref="ErrorCode" /> == <see cref="MutexAcquisitionErrno.Success" />
    /// </value>
    public bool IsSuccessResult => ErrorCode == MutexAcquisitionErrno.Success;

    /// <exception cref="InvalidOperationException" accessor="get">
    ///     with an InnerException of type
    ///     <see cref="MutexAcquisitionException" /> if <see cref="ErrorCode" /> is not quivalent to <see cref="Errno.EOK" />.
    /// </exception>
    public Mutex Mutex
    {
        get
        {
            if ( !IsSuccessResult )
            {
                throw new InvalidOperationException( null, new MutexAcquisitionException( ErrorCode, Exception, "Invalid attempt to get Mutex from failure result." ) );
            }

            return _mutex;
        }
    }

    /// <summary>
    ///     Gets the name originally used to request the mutex
    /// </summary>
    private string MutexName { get; }

    [SuppressMessage( "ReSharper", "ExceptionNotDocumentedOptional", Justification = "The ArgumentNullException is prevented by the string validation before the call to ReleaseMutex" )]
    public void Dispose( )
    {
        if ( !IsSuccessResult || _disposed || string.IsNullOrWhiteSpace( MutexName ) )
        {
            return;
        }

        Mutexes.ReleaseMutex( MutexName );
        _disposed = true;
    }

    public static implicit operator Errno( MutexAcquisitionResult errno )
    {
        return errno.ErrorCode.ToErrno( );
    }

    public static implicit operator MutexAcquisitionResult( Errno errno )
    {
        return new( (MutexAcquisitionErrno)errno );
    }

    public static implicit operator MutexAcquisitionErrno( MutexAcquisitionResult errno )
    {
        return errno.ErrorCode;
    }

    public static implicit operator MutexAcquisitionResult( MutexAcquisitionErrno errno )
    {
        return new( errno );
    }
#pragma warning disable CS8618
    public MutexAcquisitionResult( MutexAcquisitionErrno errno, string mutexName, Mutex mutex )
    {
        _mutex = mutex;
        ErrorCode = errno;
        MutexName = mutexName;
    }

    /// <summary>
    ///     Creates a new <see cref="MutexAcquisitionResult" /> containing the specified <see cref="Mutex" /> and with
    ///     <see cref="ErrorCode" /> set to <see cref="MutexAcquisitionErrno.Success" />
    /// </summary>
    /// <param name="mutexName"></param>
    /// <param name="mutex"></param>
    public MutexAcquisitionResult( string mutexName, Mutex mutex )
    {
        _mutex = mutex;
        ErrorCode = MutexAcquisitionErrno.Success;
        MutexName = mutexName;
    }

    private MutexAcquisitionResult( MutexAcquisitionErrno errno )
    {
        ErrorCode = errno;
    }

    public MutexAcquisitionResult( MutexAcquisitionErrno errno, MutexAcquisitionException e )
    {
        ErrorCode = errno;
        Exception = e;
    }
#pragma warning restore CS8618
}
