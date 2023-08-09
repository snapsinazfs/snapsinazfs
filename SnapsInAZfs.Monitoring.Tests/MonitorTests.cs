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

using System.Reflection;

namespace SnapsInAZfs.Monitoring.Tests;

[TestFixture]
[TestOf( typeof( Monitor ) )]
public class MonitorTests
{
    [Test]
    public void AllDateTimeOffsetPropertiesUnixEpochByDefault( )
    {
        Monitor testMonitor = new( );
        foreach ( PropertyInfo propertyInfo in testMonitor.GetType( ).GetProperties( ).Where( propertyInfo => propertyInfo.PropertyType == typeof( DateTimeOffset ) ) )
        {
            Assert.That( propertyInfo.GetValue( testMonitor ), Is.EqualTo( DateTimeOffset.UnixEpoch ) );
        }
    }

    [Test]
    public void AllUintPropertiesZeroByDefault( )
    {
        Monitor testMonitor = new( );
        foreach ( PropertyInfo propertyInfo in testMonitor.GetType( ).GetProperties( ).Where( propertyInfo => propertyInfo.PropertyType == typeof( uint ) ) )
        {
            Assert.That( propertyInfo.GetValue( testMonitor ), Is.Zero );
        }
    }

    [Test]
    public void BeginEventsResetCounters( )
    {
        Monitor testMonitor = new( );
        SnapshotOperationsObservableMock observable = new( );

        // Use reflection to set the fields to non-zero values, since we don't want to expose setters on the properties
        FieldInfo snapshotsPrunedSucceededLastRunFieldInfo = typeof( Monitor ).GetField( "_snapshotsPrunedSucceededLastRun", BindingFlags.Instance | BindingFlags.NonPublic )!;
        snapshotsPrunedSucceededLastRunFieldInfo.SetValue( testMonitor, 1u );
        FieldInfo snapshotsPrunedFailedLastRunFieldInfo = typeof( Monitor ).GetField( "_snapshotsPrunedFailedLastRun", BindingFlags.Instance | BindingFlags.NonPublic )!;
        snapshotsPrunedFailedLastRunFieldInfo.SetValue( testMonitor, 1u );
        FieldInfo snapshotsTakenSucceededLastRunFieldInfo = typeof( Monitor ).GetField( "_snapshotsTakenSucceededLastRun", BindingFlags.Instance | BindingFlags.NonPublic )!;
        snapshotsTakenSucceededLastRunFieldInfo.SetValue( testMonitor, 1u );
        FieldInfo snapshotsTakenFailedLastRunFieldInfo = typeof( Monitor ).GetField( "_snapshotsTakenFailedLastRun", BindingFlags.Instance | BindingFlags.NonPublic )!;
        snapshotsTakenFailedLastRunFieldInfo.SetValue( testMonitor, 1u );

        Assume.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.EqualTo( 1u ) );
        Assume.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.EqualTo( 1u ) );
        Assume.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.EqualTo( 1u ) );
        Assume.That( testMonitor.SnapshotsTakenFailedLastRun, Is.EqualTo( 1u ) );

        testMonitor.RegisterSnapshotOperationsObservable( observable );

        Assume.That( observable.IsBeginPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsBeginTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotSucceededRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotSucceededRegistered, Is.True );

        observable.RaiseBeginPruningSnapshotsEvent( );
        Assert.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.Zero );
        Assert.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.Zero );

        observable.RaiseBeginTakingSnapshotsEvent( );
        Assert.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.Zero );
        Assert.That( testMonitor.SnapshotsTakenFailedLastRun, Is.Zero );
    }

    [Test]
    public void EndEventsUpdateLastEndedTimestamps( )
    {
        Monitor testMonitor = new( );
        SnapshotOperationsObservableMock observable = new( );

        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset oneMinuteAgo = DateTimeOffset.Now.AddMinutes( -1 );

        testMonitor.SnapshotsPrunedLastEnded = oneMinuteAgo;
        testMonitor.SnapshotsTakenLastEnded = oneMinuteAgo;

        Assume.That( testMonitor.SnapshotsPrunedLastEnded, Is.EqualTo( oneMinuteAgo ) );
        Assume.That( testMonitor.SnapshotsTakenLastEnded, Is.EqualTo( oneMinuteAgo ) );

        testMonitor.RegisterSnapshotOperationsObservable( observable );

        Assume.That( observable.IsBeginPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsBeginTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotSucceededRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotSucceededRegistered, Is.True );

        observable.RaiseEndPruningSnapshotsEvent( );
        Assert.That( testMonitor.SnapshotsPrunedLastEnded, Is.EqualTo( now ).Within( 5 ).Seconds );

        observable.RaiseEndTakingSnapshotsEvent( );
        Assert.That( testMonitor.SnapshotsTakenLastEnded, Is.EqualTo( now ).Within( 5 ).Seconds );
    }

    [Test]
    public void RegisterApplicationStateObservable_NotSubscribedIfSecondParameterFalse( [Values] bool isSubscribedInitially )
    {
        ApplicationStateObservableMock observable = new( );
        Monitor testMonitor = new( );
        Assume.That( observable.IsApplicationStateChangedSubscribed, Is.False );
        testMonitor.RegisterApplicationStateObservable( observable, isSubscribedInitially );
        Assert.That( observable.IsApplicationStateChangedSubscribed, Is.EqualTo( isSubscribedInitially ) );
        testMonitor.RegisterApplicationStateObservable( observable, false );
        Assert.That( observable.IsApplicationStateChangedSubscribed, Is.False );
    }

    [Test]
    public void RegisterApplicationStateObservable_OneParameter_SubscribesToEvent( )
    {
        ApplicationStateObservableMock observable = new( );
        Monitor testMonitor = new( );
        Assume.That( observable.IsApplicationStateChangedSubscribed, Is.False );
        testMonitor.RegisterApplicationStateObservable( observable );
        Assert.That( observable.IsApplicationStateChangedSubscribed, Is.True );
    }

    [Test]
    public void RegisterApplicationStateObservable_ReturnsObservableStateIfSecondParameterFalse( )
    {
        ApplicationStateObservableMock observable = new( )
        {
            State = ApplicationState.Executing
        };
        Monitor testMonitor = new( );
        Assume.That( testMonitor.GetApplicationState( ), Is.EqualTo( "Not Registered" ) );
        testMonitor.RegisterApplicationStateObservable( observable, false );
        Assert.That( testMonitor.GetApplicationState( ), Is.EqualTo( observable.State.ToString( "G" ) ) );
        observable.State = ApplicationState.Idle;
        Assert.That( testMonitor.GetApplicationState( ), Is.EqualTo( observable.State.ToString( "G" ) ) );
    }

    [Test]
    public void RegisterApplicationStateObservable_ReturnsObservableStateIfSecondParameterFalseAfterReRegister( )
    {
        ApplicationStateObservableMock observable = new( )
        {
            State = ApplicationState.Executing
        };
        Monitor testMonitor = new( );
        Assume.That( testMonitor.GetApplicationState( ), Is.EqualTo( "Not Registered" ) );
        testMonitor.RegisterApplicationStateObservable( observable );
        Assert.That( testMonitor.GetApplicationState( ), Is.Not.EqualTo( observable.State.ToString( "G" ) ) );
        testMonitor.RegisterApplicationStateObservable( observable, false );
        Assert.That( testMonitor.GetApplicationState( ), Is.EqualTo( observable.State.ToString( "G" ) ) );
    }

    [Test]
    public void RegisterApplicationStateObservable_ThrowsOnAttemptToRegisterDifferentObservable( )
    {
        ApplicationStateObservableMock firstObservable = new( );
        ApplicationStateObservableMock secondObservable = new( );
        Monitor testMonitor = new( );
        testMonitor.RegisterApplicationStateObservable( firstObservable );
        Assert.That( ( ) => testMonitor.RegisterApplicationStateObservable( secondObservable ), Throws.InvalidOperationException );
    }

    [Test]
    public void RegisterSnapshotOperationsObservable_NoChangeIfRegisteringSameObservableAgain( )
    {
        SnapshotOperationsObservableMock observable = new( );
        Monitor testMonitor = new( );
        Assume.That( observable.IsBeginPruningSnapshotsRegistered, Is.False );
        Assume.That( observable.IsBeginTakingSnapshotsRegistered, Is.False );
        Assume.That( observable.IsEndPruningSnapshotsRegistered, Is.False );
        Assume.That( observable.IsEndTakingSnapshotsRegistered, Is.False );
        Assume.That( observable.IsPruneSnapshotFailedRegistered, Is.False );
        Assume.That( observable.IsPruneSnapshotSucceededRegistered, Is.False );
        Assume.That( observable.IsTakeSnapshotFailedRegistered, Is.False );
        Assume.That( observable.IsTakeSnapshotSucceededRegistered, Is.False );
        testMonitor.RegisterSnapshotOperationsObservable( observable );
        Assume.That( observable.IsBeginPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsBeginTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotSucceededRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotSucceededRegistered, Is.True );
        testMonitor.RegisterSnapshotOperationsObservable( observable );
        Assert.Multiple( ( ) =>
        {
            Assert.That( observable.DidBeginPruningSnapshotsRegisterMultiple, Is.False );
            Assert.That( observable.DidBeginTakingSnapshotsRegisterMultiple, Is.False );
            Assert.That( observable.DidEndPruningSnapshotsRegisterMultiple, Is.False );
            Assert.That( observable.DidEndTakingSnapshotsRegisterMultiple, Is.False );
            Assert.That( observable.DidPruneSnapshotFailedRegisterMultiple, Is.False );
            Assert.That( observable.DidPruneSnapshotSucceededRegisterMultiple, Is.False );
            Assert.That( observable.DidTakeSnapshotFailedRegisterMultiple, Is.False );
            Assert.That( observable.DidTakeSnapshotSucceededRegisterMultiple, Is.False );
        } );
    }

    [Test]
    public void RegisterSnapshotOperationsObservable_SubscribesToEvents( )
    {
        SnapshotOperationsObservableMock observable = new( );
        Monitor testMonitor = new( );
        Assume.That( observable.IsBeginPruningSnapshotsRegistered, Is.False );
        Assume.That( observable.IsBeginTakingSnapshotsRegistered, Is.False );
        Assume.That( observable.IsEndPruningSnapshotsRegistered, Is.False );
        Assume.That( observable.IsEndTakingSnapshotsRegistered, Is.False );
        Assume.That( observable.IsPruneSnapshotFailedRegistered, Is.False );
        Assume.That( observable.IsPruneSnapshotSucceededRegistered, Is.False );
        Assume.That( observable.IsTakeSnapshotFailedRegistered, Is.False );
        Assume.That( observable.IsTakeSnapshotSucceededRegistered, Is.False );
        testMonitor.RegisterSnapshotOperationsObservable( observable );
        Assert.Multiple( ( ) =>
        {
            Assert.That( observable.IsBeginPruningSnapshotsRegistered, Is.True );
            Assert.That( observable.IsBeginTakingSnapshotsRegistered, Is.True );
            Assert.That( observable.IsEndPruningSnapshotsRegistered, Is.True );
            Assert.That( observable.IsEndTakingSnapshotsRegistered, Is.True );
            Assert.That( observable.IsPruneSnapshotFailedRegistered, Is.True );
            Assert.That( observable.IsPruneSnapshotSucceededRegistered, Is.True );
            Assert.That( observable.IsTakeSnapshotFailedRegistered, Is.True );
            Assert.That( observable.IsTakeSnapshotSucceededRegistered, Is.True );
        } );
    }

    [Test]
    public void RegisterSnapshotOperationsObservable_ThrowsOnAttemptToRegisterDifferentObservable( )
    {
        SnapshotOperationsObservableMock firstObservable = new( );
        SnapshotOperationsObservableMock secondObservable = new( );
        Monitor testMonitor = new( );
        testMonitor.RegisterSnapshotOperationsObservable( firstObservable );
        Assert.That( ( ) => testMonitor.RegisterSnapshotOperationsObservable( secondObservable ), Throws.InvalidOperationException );
    }

    [Test]
    public void SuccessAndFailureEventsIncrementCounters( )
    {
        Monitor testMonitor = new( );
        SnapshotOperationsObservableMock observable = new( );

        Assume.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.Zero );
        Assume.That( testMonitor.SnapshotsPrunedFailedSinceStart, Is.Zero );
        Assume.That( testMonitor.SnapshotsTakenFailedLastRun, Is.Zero );
        Assume.That( testMonitor.SnapshotsTakenFailedSinceStart, Is.Zero );
        Assume.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.Zero );
        Assume.That( testMonitor.SnapshotsPrunedSucceededSinceStart, Is.Zero );
        Assume.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.Zero );
        Assume.That( testMonitor.SnapshotsTakenSucceededSinceStart, Is.Zero );

        testMonitor.RegisterSnapshotOperationsObservable( observable );

        Assume.That( observable.IsBeginPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsBeginTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndPruningSnapshotsRegistered, Is.True );
        Assume.That( observable.IsEndTakingSnapshotsRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsPruneSnapshotSucceededRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotFailedRegistered, Is.True );
        Assume.That( observable.IsTakeSnapshotSucceededRegistered, Is.True );

        DateTimeOffset now = DateTimeOffset.Now;
        observable.RaisePruneSnapshotFailedEvent( "snapshot name", in now );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedLastRun, Is.Zero );
            Assert.That( testMonitor.SnapshotsTakenFailedSinceStart, Is.Zero );
            Assert.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.Zero );
            Assert.That( testMonitor.SnapshotsPrunedSucceededSinceStart, Is.Zero );
            Assert.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.Zero );
            Assert.That( testMonitor.SnapshotsTakenSucceededSinceStart, Is.Zero );
        } );

        observable.RaiseTakeSnapshotFailedEvent( "snapshot name", in now );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.Zero );
            Assert.That( testMonitor.SnapshotsPrunedSucceededSinceStart, Is.Zero );
            Assert.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.Zero );
            Assert.That( testMonitor.SnapshotsTakenSucceededSinceStart, Is.Zero );
        } );

        observable.RaisePruneSnapshotSucceededEvent( "snapshot name", in now );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedSucceededSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.Zero );
            Assert.That( testMonitor.SnapshotsTakenSucceededSinceStart, Is.Zero );
        } );

        observable.RaiseTakeSnapshotSucceededEvent( "snapshot name", in now );
        Assert.Multiple( ( ) =>
        {
            Assert.That( testMonitor.SnapshotsPrunedFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenFailedSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedSucceededLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsPrunedSucceededSinceStart, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenSucceededLastRun, Is.EqualTo( 1 ) );
            Assert.That( testMonitor.SnapshotsTakenSucceededSinceStart, Is.EqualTo( 1 ) );
        } );
    }

    private static IEnumerable<EventInfo> getSnapshotOperationEventArgsEvents( )
    {
        return typeof( Monitor ).GetEvents( ).Where( ei => ei.EventHandlerType == typeof( EventHandler<SnapshotOperationEventArgs> ) );
    }

    private class ApplicationStateObservableMock : IApplicationStateObservable
    {
        private ApplicationState _state;

        public bool IsApplicationStateChangedSubscribed { get; private set; }

        public ApplicationState State
        {
            get => _state;
            set
            {
                if ( _state != value )
                {
                    _applicationStateChanged?.Invoke( this, new( _state, value ) );
                }

                _state = value;
            }
        }

        /// <inheritdoc />
        public DateTimeOffset ServiceStartTime { get; } = DateTimeOffset.Now;

        public event EventHandler<ApplicationStateChangedEventArgs>? ApplicationStateChanged
        {
            add
            {
                IsApplicationStateChangedSubscribed = true;
                _applicationStateChanged += value;
            }
            remove
            {
                IsApplicationStateChangedSubscribed = false;
                _applicationStateChanged -= value;
            }
        }

        private event EventHandler<ApplicationStateChangedEventArgs>? _applicationStateChanged;
    }

    private class SnapshotOperationsObservableMock : ISnapshotOperationsObservable
    {
        public bool DidBeginPruningSnapshotsRegisterMultiple { get; private set; }

        public bool DidBeginTakingSnapshotsRegisterMultiple { get; private set; }

        public bool DidEndPruningSnapshotsRegisterMultiple { get; private set; }
        public bool DidEndTakingSnapshotsRegisterMultiple { get; private set; }
        public bool DidPruneSnapshotFailedRegisterMultiple { get; private set; }
        public bool DidPruneSnapshotSucceededRegisterMultiple { get; private set; }
        public bool DidTakeSnapshotFailedRegisterMultiple { get; private set; }
        public bool DidTakeSnapshotSucceededRegisterMultiple { get; private set; }
        public bool IsBeginPruningSnapshotsRegistered { get; private set; }
        public bool IsBeginTakingSnapshotsRegistered { get; private set; }
        public bool IsEndPruningSnapshotsRegistered { get; private set; }
        public bool IsEndTakingSnapshotsRegistered { get; private set; }
        public bool IsPruneSnapshotFailedRegistered { get; private set; }
        public bool IsPruneSnapshotSucceededRegistered { get; private set; }
        public bool IsTakeSnapshotFailedRegistered { get; private set; }
        public bool IsTakeSnapshotSucceededRegistered { get; private set; }

        /// <inheritdoc />
        public event EventHandler<DateTimeOffset>? BeginPruningSnapshots
        {
            add
            {
                DidBeginPruningSnapshotsRegisterMultiple = IsBeginPruningSnapshotsRegistered;
                IsBeginPruningSnapshotsRegistered = true;
                _beginPruningSnapshots += value;
            }
            remove
            {
                IsBeginPruningSnapshotsRegistered = false;
                _beginPruningSnapshots -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<DateTimeOffset>? BeginTakingSnapshots
        {
            add
            {
                DidBeginTakingSnapshotsRegisterMultiple = IsBeginTakingSnapshotsRegistered;
                IsBeginTakingSnapshotsRegistered = true;
                _beginTakingSnapshots += value;
            }
            remove
            {
                IsBeginTakingSnapshotsRegistered = false;
                _beginTakingSnapshots -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<DateTimeOffset>? EndPruningSnapshots
        {
            add
            {
                DidEndPruningSnapshotsRegisterMultiple = IsEndPruningSnapshotsRegistered;
                IsEndPruningSnapshotsRegistered = true;
                _endPruningSnapshots += value;
            }
            remove
            {
                IsEndPruningSnapshotsRegistered = false;
                _endPruningSnapshots -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<DateTimeOffset>? EndTakingSnapshots
        {
            add
            {
                DidEndTakingSnapshotsRegisterMultiple = IsEndTakingSnapshotsRegistered;
                IsEndTakingSnapshotsRegistered = true;
                _endTakingSnapshots += value;
            }
            remove
            {
                IsEndTakingSnapshotsRegistered = false;
                _endTakingSnapshots -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<SnapshotOperationEventArgs>? PruneSnapshotFailed
        {
            add
            {
                DidPruneSnapshotFailedRegisterMultiple = IsPruneSnapshotFailedRegistered;
                IsPruneSnapshotFailedRegistered = true;
                _pruneSnapshotFailed += value;
            }
            remove
            {
                IsPruneSnapshotFailedRegistered = false;
                _pruneSnapshotFailed -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<SnapshotOperationEventArgs>? TakeSnapshotFailed
        {
            add
            {
                DidTakeSnapshotFailedRegisterMultiple = IsTakeSnapshotFailedRegistered;
                IsTakeSnapshotFailedRegistered = true;
                _takeSnapshotFailed += value;
            }
            remove
            {
                IsTakeSnapshotFailedRegistered = false;
                _takeSnapshotFailed -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<SnapshotOperationEventArgs>? PruneSnapshotSucceeded
        {
            add
            {
                DidPruneSnapshotSucceededRegisterMultiple = IsPruneSnapshotSucceededRegistered;
                IsPruneSnapshotSucceededRegistered = true;
                _pruneSnapshotSucceeded += value;
            }
            remove
            {
                IsPruneSnapshotSucceededRegistered = false;
                _pruneSnapshotSucceeded -= value;
            }
        }

        /// <inheritdoc />
        public event EventHandler<SnapshotOperationEventArgs>? TakeSnapshotSucceeded
        {
            add
            {
                DidTakeSnapshotSucceededRegisterMultiple = IsPruneSnapshotSucceededRegistered;
                IsTakeSnapshotSucceededRegistered = true;
                _takeSnapshotSucceeded += value;
            }
            remove
            {
                IsTakeSnapshotSucceededRegistered = false;
                _takeSnapshotSucceeded -= value;
            }
        }

        public void RaiseBeginPruningSnapshotsEvent( )
        {
            _beginPruningSnapshots?.Invoke( this, DateTimeOffset.Now );
        }

        public void RaiseBeginTakingSnapshotsEvent( )
        {
            _beginTakingSnapshots?.Invoke( this, DateTimeOffset.Now );
        }

        public void RaiseEndPruningSnapshotsEvent( )
        {
            _endPruningSnapshots?.Invoke( this, DateTimeOffset.Now );
        }

        public void RaiseEndTakingSnapshotsEvent( )
        {
            _endTakingSnapshots?.Invoke( this, DateTimeOffset.Now );
        }

        public void RaisePruneSnapshotFailedEvent( string name, in DateTimeOffset timestamp )
        {
            _pruneSnapshotFailed?.Invoke( this, new( name, in timestamp ) );
        }

        public void RaisePruneSnapshotSucceededEvent( string name, in DateTimeOffset timestamp )
        {
            _pruneSnapshotSucceeded?.Invoke( this, new( name, in timestamp ) );
        }

        public void RaiseTakeSnapshotFailedEvent( string name, in DateTimeOffset timestamp )
        {
            _takeSnapshotFailed?.Invoke( this, new( name, in timestamp ) );
        }

        public void RaiseTakeSnapshotSucceededEvent( string name, in DateTimeOffset timestamp )
        {
            _takeSnapshotSucceeded?.Invoke( this, new( name, in timestamp ) );
        }

        private event EventHandler<DateTimeOffset>? _beginPruningSnapshots;

        private event EventHandler<DateTimeOffset>? _beginTakingSnapshots;

        private event EventHandler<DateTimeOffset>? _endPruningSnapshots;

        private event EventHandler<DateTimeOffset>? _endTakingSnapshots;

        private event EventHandler<SnapshotOperationEventArgs>? _pruneSnapshotFailed;

        private event EventHandler<SnapshotOperationEventArgs>? _pruneSnapshotSucceeded;

        private event EventHandler<SnapshotOperationEventArgs>? _takeSnapshotFailed;

        private event EventHandler<SnapshotOperationEventArgs>? _takeSnapshotSucceeded;
    }
}
