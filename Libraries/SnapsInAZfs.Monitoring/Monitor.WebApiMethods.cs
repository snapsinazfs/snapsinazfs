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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SnapsInAZfs.Monitoring;

public sealed partial class Monitor
{
    /// <inheritdoc />
    public async Task<Results<Ok<string>, StatusCodeHttpResult>> GetApplicationStateAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( _applicationStateObservable switch
            {
                not null => TypedResults.Ok( GetApplicationState( ) ),
                _ => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application state" );
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<ApplicationStateMetrics>, StatusCodeHttpResult>> GetFullApplicationStateAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<ApplicationStateMetrics>, StatusCodeHttpResult>>( _applicationStateObservable switch
            {
                not null => TypedResults.Ok( new ApplicationStateMetrics( GetApplicationState( ), ServiceStartTime, NextRunTime, Environment.WorkingSet, Version ?? "Unknown" ) ),
                _ => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application state" );
            return await Task.FromResult<Results<Ok<ApplicationStateMetrics>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetServiceStartTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( _applicationStateObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( ServiceStartTime )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting service start time" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<string>, StatusCodeHttpResult>> GetVersionAsync( )
    {
        try
        {
            // ReSharper disable once ExceptionNotDocumented
            string? informationalVersion = Version;
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( informationalVersion switch
            {
                null or "" => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( informationalVersion )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application version" );
            return await Task.FromResult<Results<Ok<string>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<long>, StatusCodeHttpResult>> GetWorkingSetAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<long>, StatusCodeHttpResult>>( TypedResults.Ok( Environment.WorkingSet ) ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting application working set" );
            return await Task.FromResult<Results<Ok<long>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenSucceededLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenSucceededLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenSucceededSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenSucceededSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedSucceededLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedSucceededLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedSucceededSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedSucceededSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenFailedLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenFailedLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsTakenFailedSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsTakenFailedSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    public async Task<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>> GetAllSnapshotCountsAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( new SnapshotCountMetrics( in _snapshotsPrunedFailedLastRun, in _snapshotsPrunedFailedSinceStart, in _snapshotsPrunedSucceededLastRun, in _snapshotsPrunedSucceededSinceStart, in _snapshotsTakenFailedLastRun, in _snapshotsTakenFailedSinceStart, in _snapshotsTakenSucceededLastRun, in _snapshotsTakenSucceededSinceStart ) )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<SnapshotCountMetrics>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedLastRunCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedFailedLastRun )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<uint>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedSinceStartCountAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( _snapshotsPrunedFailedSinceStart )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, ErrorGettingSnapshotCount );
            return await Task.FromResult<Results<Ok<uint>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public Task<Results<Ok<List<string>>, StatusCodeHttpResult>> GetSnapshotsTakenFailedLastRunNamesAsync( )
    {
        if ( _snapshotOperationsObservable is null )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ) );
        }

        lock ( _snapshotsTakenFailedLastRunNamesLock )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.Ok( _snapshotsTakenFailedLastRunNames.ToList( ) ) );
        }
    }

    /// <inheritdoc />
    public Task<Results<Ok<List<string>>, StatusCodeHttpResult>> GetSnapshotsPrunedFailedLastRunNamesAsync( )
    {
        if ( _snapshotOperationsObservable is null )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ) );
        }

        lock ( _snapshotsPrunedFailedLastRunNamesLock )
        {
            return Task.FromResult<Results<Ok<List<string>>, StatusCodeHttpResult>>( TypedResults.Ok( _snapshotsPrunedFailedLastRunNames.ToList( ) ) );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetLastSnapshotTakenTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( SnapshotsTakenLastEnded )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting timestamp" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetLastSnapshotPrunedTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( _snapshotOperationsObservable switch
            {
                null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                _ => TypedResults.Ok( SnapshotsPrunedLastEnded )
            } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting timestamp" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }

    /// <inheritdoc />
    public async Task<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>> GetNextRunTimeAsync( )
    {
        try
        {
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>(
                _applicationStateObservable switch
                {
                    null => TypedResults.StatusCode( (int)HttpStatusCode.ServiceUnavailable ),
                    _ => TypedResults.Ok( DateTimeOffset.FromUnixTimeMilliseconds( Interlocked.Read( ref _nextRunTime ) ) )
                } ).ConfigureAwait( false );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting timestamp" );
            return await Task.FromResult<Results<Ok<DateTimeOffset>, StatusCodeHttpResult>>( TypedResults.StatusCode( (int)HttpStatusCode.InternalServerError ) ).ConfigureAwait( false );
        }
    }
}
