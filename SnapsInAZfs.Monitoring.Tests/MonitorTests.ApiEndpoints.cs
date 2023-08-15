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

using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;

#pragma warning disable CA2007

namespace SnapsInAZfs.Monitoring.Tests;

public partial class MonitorTests
{
    [Test]
    [TestCaseSource( nameof( Endpoint_ReturnsOk_IfApplicationObservableRegistered_Cases ) )]
    public async Task Endpoint_ReturnsOk_IfApplicationObservableRegistered<T>( string methodName, T resultType )
    {
        ApplicationStateObservableMock observable = new( );
        Monitor testMonitor = new( );
        testMonitor.RegisterApplicationStateObservable( observable );
        MethodInfo method = typeof( IApplicationStateObserver ).GetMethod( methodName )!;
        Task<Results<Ok<T>, StatusCodeHttpResult>> task = ( method.Invoke( testMonitor, null ) as Task<Results<Ok<T>, StatusCodeHttpResult>> )!;
        Results<Ok<T>, StatusCodeHttpResult> results = await task;
        Assert.That( results.Result, Is.InstanceOf<Ok<T>>( ) );
    }

    [Test]
    [TestCaseSource( nameof( Endpoint_ReturnsOk_IfSnapshotOperationObservableRegistered_Cases ) )]
    public async Task Endpoint_ReturnsOk_IfSnapshotOperationObservableRegistered<T>( string methodName, T resultType )
    {
        SnapshotOperationsObservableMock observable = new( );
        Monitor testMonitor = new( );
        testMonitor.RegisterSnapshotOperationsObservable( observable );
        MethodInfo method = typeof( ISnapshotOperationsObserver ).GetMethod( methodName )!;
        Task<Results<Ok<T>, StatusCodeHttpResult>> task = ( method.Invoke( testMonitor, null ) as Task<Results<Ok<T>, StatusCodeHttpResult>> )!;
        Results<Ok<T>, StatusCodeHttpResult> results = await task;
        Assert.That( results.Result, Is.InstanceOf<Ok<T>>( ) );
    }

    [Test]
    [TestCaseSource( nameof( Endpoint_ReturnsServiceUnavailable_IfApplicationObservableNotRegistered_Cases ) )]
    public async Task Endpoint_ReturnsServiceUnavailable_IfApplicationObservableNotRegistered<T>( string methodName, T resultType )
    {
        Monitor testMonitor = new( );
        MethodInfo method = typeof( IApplicationStateObserver ).GetMethod( methodName )!;
        Task<Results<Ok<T>, StatusCodeHttpResult>> task = ( method.Invoke( testMonitor, null ) as Task<Results<Ok<T>, StatusCodeHttpResult>> )!;
        Results<Ok<T>, StatusCodeHttpResult> results = await task;
        Assert.That( results.Result, Is.InstanceOf<StatusCodeHttpResult>( ) );
        StatusCodeHttpResult statusCodeResult = (StatusCodeHttpResult)results.Result;
        Assert.That( statusCodeResult.StatusCode, Is.EqualTo( (int)HttpStatusCode.ServiceUnavailable ) );
    }

    [Test]
    [TestCaseSource( nameof( Endpoint_ReturnsServiceUnavailable_IfSnapshotOperationObservableNotRegistered_Cases ) )]
    public async Task Endpoint_ReturnsServiceUnavailable_IfSnapshotOperationObservableNotRegistered<T>( string methodName, T resultType )
    {
        Monitor testMonitor = new( );
        MethodInfo method = typeof( ISnapshotOperationsObserver ).GetMethod( methodName )!;
        Task<Results<Ok<T>, StatusCodeHttpResult>> task = ( method.Invoke( testMonitor, null ) as Task<Results<Ok<T>, StatusCodeHttpResult>> )!;
        Results<Ok<T>, StatusCodeHttpResult> results = await task;
        Assert.That( results.Result, Is.InstanceOf<StatusCodeHttpResult>( ) );
        StatusCodeHttpResult statusCodeResult = (StatusCodeHttpResult)results.Result;
        Assert.That( statusCodeResult.StatusCode, Is.EqualTo( (int)HttpStatusCode.ServiceUnavailable ) );
    }

    [Test]
    public async Task GetWorkingSetAsync_ReturnsOkWithPositiveValue( )
    {
        Monitor testMonitor = new( );
        Results<Ok<long>, StatusCodeHttpResult> s = await testMonitor.GetWorkingSetAsync( );
        Ok<long> result = (Ok<long>)s.Result;
        Assert.That( result.Value, Is.Positive );
    }

    private static IEnumerable<TestCaseData> Endpoint_ReturnsOk_IfApplicationObservableRegistered_Cases( )
    {
        yield return new( "GetApplicationStateAsync", string.Empty );
        yield return new( "GetFullApplicationStateAsync", ApplicationStateMetrics.Empty );
        yield return new( "GetNextRunTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetServiceStartTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetVersionAsync", string.Empty );
        yield return new( "GetWorkingSetAsync", 0L );
    }

    private static IEnumerable<TestCaseData> Endpoint_ReturnsOk_IfSnapshotOperationObservableRegistered_Cases( )
    {
        yield return new( "GetAllSnapshotCountsAsync", new SnapshotCountMetrics( ) );
        yield return new( "GetLastSnapshotPrunedTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetLastSnapshotTakenTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetSnapshotsPrunedFailedLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsPrunedFailedLastRunNamesAsync", new List<string>( ) );
        yield return new( "GetSnapshotsPrunedFailedSinceStartCountAsync", 0U );
        yield return new( "GetSnapshotsPrunedSucceededLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsPrunedSucceededSinceStartCountAsync", 0U );
        yield return new( "GetSnapshotsTakenFailedLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsTakenFailedLastRunNamesAsync", new List<string>( ) );
        yield return new( "GetSnapshotsTakenFailedSinceStartCountAsync", 0U );
        yield return new( "GetSnapshotsTakenSucceededLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsTakenSucceededSinceStartCountAsync", 0U );
    }

    private static IEnumerable<TestCaseData> Endpoint_ReturnsServiceUnavailable_IfApplicationObservableNotRegistered_Cases( )
    {
        yield return new( "GetApplicationStateAsync", string.Empty );
        yield return new( "GetFullApplicationStateAsync", ApplicationStateMetrics.Empty );
        yield return new( "GetNextRunTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetServiceStartTimeAsync", DateTimeOffset.UnixEpoch );
    }

    private static IEnumerable<TestCaseData> Endpoint_ReturnsServiceUnavailable_IfSnapshotOperationObservableNotRegistered_Cases( )
    {
        yield return new( "GetAllSnapshotCountsAsync", new SnapshotCountMetrics( ) );
        yield return new( "GetLastSnapshotPrunedTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetLastSnapshotTakenTimeAsync", DateTimeOffset.UnixEpoch );
        yield return new( "GetSnapshotsPrunedFailedLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsPrunedFailedLastRunNamesAsync", new List<string>( ) );
        yield return new( "GetSnapshotsPrunedFailedSinceStartCountAsync", 0U );
        yield return new( "GetSnapshotsPrunedSucceededLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsPrunedSucceededSinceStartCountAsync", 0U );
        yield return new( "GetSnapshotsTakenFailedLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsTakenFailedLastRunNamesAsync", new List<string>( ) );
        yield return new( "GetSnapshotsTakenFailedSinceStartCountAsync", 0U );
        yield return new( "GetSnapshotsTakenSucceededLastRunCountAsync", 0U );
        yield return new( "GetSnapshotsTakenSucceededSinceStartCountAsync", 0U );
    }
}
