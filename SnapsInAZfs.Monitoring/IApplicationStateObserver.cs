#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

namespace SnapsInAZfs.Monitoring;

/// <summary>
///     Interface defining minimum implementation of an object that intends to observe an <see cref="IApplicationStateObservable" />,
///     via direct polling and by event subscription
/// </summary>
public interface IApplicationStateObserver
{
    /// <summary>
    ///     Gets a string representation of the monitored <see cref="IApplicationStateObservable" />'s current state.
    /// </summary>
    /// <returns>
    ///     A string representing the current state of the monitored observable object.
    /// </returns>
    string GetApplicationState( );

    /// <summary>
    ///     Registers an instance of an <see cref="IApplicationStateObservable" /> with this <see cref="IApplicationStateObserver" />
    /// </summary>
    /// <remarks>
    ///     Implementations should typically subscribe the observer object to the observable object's
    ///     <see cref="IApplicationStateObservable.ApplicationStateChanged" /> <see langword="event" />
    /// </remarks>
    public void RegisterApplicationStateObservable( IApplicationStateObservable observableObject, bool subscribeToEvents = true );
}
