// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using NLog;
using SnapsInAZfs.Interop.Libc.Enums;

namespace SnapsInAZfs.Interop.Concurrency;

/// <summary>
///     A static class for access to common uses of system-level mutexes
/// </summary>
/// <remarks>
///     By default, lock files used for <see cref="Mutex" />es are stored at /tmp/.dotnet/shm under .net7.0 on Linux<br />
///     The name of a given named Mutex determines its path beneath that root.
/// </remarks>
public sealed class Mutexes : IDisposable
{
    static Mutexes( )
    {
        Logger = LogManager.GetCurrentClassLogger( );
    }

    private Mutexes( )
    {
        Logger = LogManager.GetCurrentClassLogger( );
        Logger.Trace( "Creating mutex manager" );
    }

    private readonly ConcurrentDictionary<string, Mutex?> _allMutexes = new( );
    private bool _disposed;

    public static Mutexes Instance { get; } = new( );

    public Mutex? this[ string name ]
    {
        get
        {
            Mutex? mutex = GetMutex( out Exception? caughtException, name );
            if ( caughtException is not null )
            {
                Logger.Error( caughtException, "Error getting Mutex {0}", name );
            }

            return mutex;
        }
    }

    // ReSharper disable once InconsistentNaming
    private static Logger Logger;

    /// <summary>
    ///     Disposes all remaining held mutexes, and logs warnings for them
    /// </summary>
    public void Dispose( )
    {
        DisposeMutexes( true );
    }

    /// <summary>
    ///     Gets the default mutex for Sanoid.net, named "Global\\Sanoid.net"
    /// </summary>
    /// <param name="caughtException">
    ///     A <see langword="bool" /> indicating whether this method had to catch a fatal exception while attempting
    ///     to acquire the mutex.<br />
    ///     The caller should treat the mutex as invalid and abort.
    /// </param>
    /// <returns>
    ///     A <see cref="Mutex" /> named "Global\\Sanoid.net"
    /// </returns>
    /// <remarks>
    ///     It is possible for the returned <see cref="Mutex" /> to not be valid for use.<br />
    ///     Caller bears responsibility for handling results of this method call.
    /// </remarks>
    [SuppressMessage( "ReSharper", "ExceptionNotDocumented", Justification = "The undocumented exceptions can't be thrown on Linux." )]
    [SuppressMessage( "ReSharper", "ExceptionNotDocumentedOptional", Justification = "The undocumented exceptions can't be thrown on Linux." )]
    public static Mutex? GetSanoidMutex( out Exception? caughtException )
    {
        return GetMutex( out caughtException );
    }

    /// <summary>
    ///     Gets a mutex named <paramref name="name" />
    /// </summary>
    /// <param name="caughtException">
    ///     A <see langword="bool" /> indicating whether this method had to catch a fatal exception while attempting
    ///     to acquire the mutex.<br />
    ///     The caller should treat the mutex as invalid and abort.
    /// </param>
    /// <param name="name">
    ///     The name of the mutex. Must begin with "Global\\" to be valid for inter-process synchronization across all user
    ///     sessions.<br />
    ///     Otherwise, "Local\\" is automatically prefixed by the runtime, which provides a mutex that is only unique for the
    ///     current user session.<br />
    ///     If this parameter is null or omitted, a global Mutex named "Global\\Sanoid.net" will be acquired.
    /// </param>
    /// <returns>
    ///     A <see cref="Mutex" /> with the given name or default name, if <paramref name="name" /> is omitted.
    /// </returns>
    /// <remarks>
    ///     It is possible for the returned <see cref="Mutex" /> to not be valid for use.<br />
    ///     Caller bears responsibility for handling results of this method call.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="name" /> is  <see langword="null" />.</exception>
    [SuppressMessage( "ReSharper", "ExceptionNotDocumented", Justification = "The undocumented exceptions can't be thrown on Linux." )]
    [SuppressMessage( "ReSharper", "ExceptionNotDocumentedOptional", Justification = "The undocumented exceptions can't be thrown on Linux." )]
    public static Mutex? GetMutex( out Exception? caughtException, string name = "Global\\Sanoid.net" )
    {
        Logger.Debug( "Mutex {0} requested", name );
        caughtException = null;
        bool exists = Instance._allMutexes.TryGetValue( name, out Mutex? sanoidMutex );
        if ( exists && sanoidMutex != null )
        {
            Logger.Trace( "Mutex {0} already exists. Returning it", name );
            return sanoidMutex;
        }

        try
        {
            Logger.Debug( "Attempting to acquire new or existing mutex {0}", name );
            sanoidMutex = new( true, name, out bool createdNew );
            Logger.Trace( "Mutex {0} {1}", name, createdNew ? "created" : "already existed" );
            // This exception is not possible. Setter creates the node.
            // ReSharper disable once ExceptionNotDocumentedOptional
            Instance._allMutexes[ name ] = sanoidMutex;
            Instance._disposed = false;
        }
        catch ( IOException ioe )
        {
            Logger.Error( ioe, "Mutex {0} name invalid. Mutex {0} not acquired", name );
            caughtException = ioe;
        }
        catch ( WaitHandleCannotBeOpenedException whcboe )
        {
            Logger.Error( whcboe, "Mutex {0} could not be acquired. Another synchronizatio object of a different type with the same name exists. Mutex {0} not acquired", name );
            caughtException = whcboe;
        }
        catch ( AbandonedMutexException ame )
        {
            Logger.Error( ame, "Mutex {0} exists but was abandoned. Returned mutex is invalid", name );
            caughtException = ame;
        }

        Logger.Trace( "Returning from GetSanoidMutex({0}) with a {1} mutex", name, sanoidMutex is null ? "null" : "not-null" );
        return sanoidMutex;
    }

    /// <summary>
    ///     Attempts to acquire the named mutex, and returns a standard POSIX <see cref="Errno" /> based on the result.
    /// </summary>
    /// <param name="mutexName">The name of the mutex to attempt to acquire</param>
    /// <param name="timeout">Number of milliseconds to wait for an in-use mutex, or 5000 if omitted</param>
    /// <returns>
    ///     A <see cref="MutexAcquisitionResult" /> containing the status of the operation as well as:<br />
    ///     On success: The acquired Mutex<br />
    ///     On failure: A <see cref="MutexAcquisitionException" /> describing the nature of the failure.
    /// </returns>
    /// <exception cref="MutexAcquisitionException"></exception>
    public static MutexAcquisitionResult GetAndWaitMutex( string mutexName, int timeout = 5000 )
    {
        if ( string.IsNullOrWhiteSpace( mutexName ) )
        {
            const string errorMessage = "Requested mutex name cannot be null, empty string, or whitespace.";
            return new( MutexAcquisitionErrno.InvalidMutexNameRequested, new( MutexAcquisitionErrno.InvalidMutexNameRequested, new ArgumentNullException( nameof( mutexName ), errorMessage ), errorMessage ) );
        }

        Mutex? possiblyNullMutex = GetMutex( out Exception? caughtException, mutexName );
        switch ( caughtException )
        {
            case IOException ioe:
            {
                string errorMessage = $"Failed taking snapshots due to IOException: {ioe.Message}";
                Logger.Error( ioe, errorMessage );
                return new( MutexAcquisitionErrno.IoException, new( MutexAcquisitionErrno.IoException, ioe ) );
            }
            case AbandonedMutexException ame:
            {
                const string errorMessage = "Failed taking snapshots. A previous instance of Sanoid.net exited without properly releasing the snapshot mutex.";
                Logger.Error( ame, errorMessage );
                return new( MutexAcquisitionErrno.AbandonedMutex, new( MutexAcquisitionErrno.AbandonedMutex, ame ) );
            }
            case WaitHandleCannotBeOpenedException whcboe:
            {
                const string errorMessage = "Unable to acquire snapshot mutex. See InnerException for details.";
                Logger.Error( whcboe, errorMessage );
                return new( MutexAcquisitionErrno.WaitHandleCannotBeOpened, new( MutexAcquisitionErrno.WaitHandleCannotBeOpened, whcboe ) );
            }
            case null:
                break;
            default:
                throw new MutexAcquisitionException( MutexAcquisitionErrno.PossiblyNullMutex, caughtException );
        }

        if ( possiblyNullMutex is null )
        {
            const string errorMessage = "Unable to acquire snapshot mutex.";
            Logger.Error( caughtException, errorMessage );
            return new( MutexAcquisitionErrno.PossiblyNullMutex, new( MutexAcquisitionErrno.PossiblyNullMutex, new NullReferenceException( "A null mutex object was encountered" ), errorMessage ) );
        }

        // The exceptions this could throw will have already happened in the call to GetMutex above,
        // so we can silence Resharper's warnings about them here.
        // If one somehow happens at run-time, it will be addressed, but it should theoretically not be possible.
        // ReSharper disable ExceptionNotDocumented
        // ReSharper disable ExceptionNotDocumentedOptional
        if ( possiblyNullMutex.WaitOne( timeout ) )
        {
            Logger.Debug( "Mutex {0} successfully acquired", mutexName );
            return new( mutexName, possiblyNullMutex );
        }
        // ReSharper restore ExceptionNotDocumentedOptional
        // ReSharper restore ExceptionNotDocumented

        Logger.Error( "Timed out waiting for another process to release the mutex. This process will not take snapshots." );
        return new( MutexAcquisitionErrno.AnotherProcessIsBusy, mutexName, possiblyNullMutex );
    }

    /// <summary>
    ///     Attempts to release the specified mutex.
    /// </summary>
    /// <param name="name">The name of the mutex to release.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name" /> is  <see langword="null" />.</exception>
    public static void ReleaseMutex( string name = "Global\\Sanoid.net" )
    {
        if ( string.IsNullOrWhiteSpace( name ) )
        {
            throw new ArgumentNullException( nameof( name ), "Mutex name cannot be null or an empty string." );
        }

        Logger.Debug( "Requested to release mutex {0}", name );
        if ( Instance._disposed )
        {
            return;
        }

        try
        {
            if ( Instance._allMutexes.TryGetValue( name, out Mutex? mutex ) )
            {
                mutex?.ReleaseMutex( );
                Instance._allMutexes.TryRemove( name, out _ );
            }

            Logger.Debug( "Mutex {0} released", name );
        }
        catch ( ApplicationException ex )
        {
            //This means we didn't own the mutex. We can discard this and carry on with life.
            Logger.Warn( ex, "Attempted to release mutex when we didn't own it." );
        }
        catch ( ObjectDisposedException ex )
        {
            // If the mutex has somehow already been disposed elsewhere, we can discard this and carry on.
            // Log as an error, though, because this needs to be handled in code.
            Logger.Error( ex, "Attempted to release mutex that has already been disposed." );
        }
    }

    public static void DisposeMutexes( bool warnOnStillHeld = false )
    {
        if ( Instance._disposed )
        {
            return;
        }

        Logger.Debug( "Disposing all mutexes" );

        foreach ( ( string? name, Mutex? mutex ) in Instance._allMutexes )
        {
            if ( warnOnStillHeld )
            {
                Logger.Warn( "Mutex {0} still held", name );
            }

            mutex?.Dispose( );
        }

        Instance._allMutexes.Clear( );
        Instance._disposed = true;
    }
}
