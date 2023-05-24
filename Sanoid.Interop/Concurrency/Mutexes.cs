// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Security.AccessControl;
using NLog;
using Sanoid.Interop.Libc.Enums;

namespace Sanoid.Interop.Concurrency;

public static class Mutexes
{
    private static bool _disposed;
    private static Mutex? _sanoidMutex;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    /// </summary>
    /// <param name="createdNew">
    ///     When this method returns, contains a <see langword="bool" /> that is <see langword="true" /> if a local mutex was
    ///     created (that is, if name is null or an empty string) or if the specified named system mutex was created;<br />
    ///     <see langword="false" /> if the specified named system mutex already existed. Any initial value of this parameter
    ///     is ignored and overwritten by the framework.
    /// </param>
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
    /// <exception cref="UnauthorizedAccessException">Windows-only: The named mutex exists and has access control security, but the user does not have <see cref="System.Security.AccessControl.MutexRights.FullControl" />.</exception>
    /// <exception cref="ArgumentException">.NET Framework only: <paramref name="name" /> is longer than MAX_PATH (260 characters).</exception>
    public static Mutex? GetSanoidMutex( out bool createdNew, out Exception? caughtException, string name = "Global\\Sanoid.net" )
    {
        caughtException = null;
        createdNew = false;
        if ( _sanoidMutex != null )
        {
            return _sanoidMutex;
        }

        try
        {
            _sanoidMutex = new( true, name, out createdNew );
            _disposed = false;
        }
        catch ( IOException ioe )
        {
            Logger.Error( ioe, "Mutex {0} name invalid. Mutex {0} not acquired.", name );
            caughtException = ioe;
        }
        catch ( WaitHandleCannotBeOpenedException whcboe )
        {
            Logger.Error( whcboe, "Mutex {0} could not be acquired. Another synchronizatio object of a different type with the same name exists. Mutex {0} not acquired.", name );
            caughtException = whcboe;
        }
        catch ( AbandonedMutexException ame )
        {
            Logger.Error( ame, "Mutex {0} exists but was abandoned. Returned mutex is invalid.", name );
            caughtException = ame;
        }

        return _sanoidMutex;
    }

    public static void ReleaseSanoidMutex( )
    {
        if ( _disposed )
        {
            return;
        }

        try
        {
            _sanoidMutex?.ReleaseMutex( );
        }
        catch ( ApplicationException ex )
        {
            //This means we didn't own the mutex. We can discard this and carry on with life.
            Logger.Warn( "Attempted to release mutex when we didn't own it.", ex );
        }
        catch ( ObjectDisposedException ex )
        {
            // If the mutex has somehow already been disposed elsewhere, we can discard this and carry on.
            Logger.Warn( "Attempted to release mutex that has already been disposed.", ex );
        }
    }

    public static void Dispose( )
    {
        if ( _disposed )
        {
            return;
        }

        Logger.Debug( "Disposing all mutexes" );

        ReleaseSanoidMutex( );
        _sanoidMutex?.Dispose( );
        _sanoidMutex = null;
        _disposed = true;
    }
}
