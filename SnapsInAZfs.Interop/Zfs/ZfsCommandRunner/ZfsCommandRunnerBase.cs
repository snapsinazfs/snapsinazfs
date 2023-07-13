// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

public abstract class ZfsCommandRunnerBase : IZfsCommandRunner
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public abstract bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot );

    /// <inheritdoc />
    public abstract Task<bool> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, params IZfsProperty[] properties );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <inheritdoc />
    public abstract Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

    public abstract IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );

    /// <inheritdoc />
    public abstract IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( );

    /// <inheritdoc />
    public abstract Task<bool> InheritZfsPropertyAsync( bool dryRun, string zfsPath, IZfsProperty propertyToInherit );

    /// <inheritdoc />
    public abstract bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string poolName, string[] propertyArray );

    protected void CheckAndUpdateLastSnapshotTimesForDatasets( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets )
    {
        Logger.Trace( "Checking all dataset last snapshot times" );
        Parallel.ForEach( datasets.Values, new( ) { MaxDegreeOfParallelism = 4 }, ds =>
        {
            List<IZfsProperty> propertiesToSet = new( );
            if ( ds.LastFrequentSnapshotTimestamp.Value != ds.LastObservedFrequentSnapshotTimestamp )
            {
                propertiesToSet.Add( ds.UpdateProperty( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ds.LastObservedFrequentSnapshotTimestamp ) );
            }

            if ( ds.LastHourlySnapshotTimestamp.Value != ds.LastObservedHourlySnapshotTimestamp )
            {
                propertiesToSet.Add( ds.UpdateProperty( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ds.LastObservedHourlySnapshotTimestamp ) );
            }

            if ( ds.LastDailySnapshotTimestamp.Value != ds.LastObservedDailySnapshotTimestamp )
            {
                propertiesToSet.Add( ds.UpdateProperty( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ds.LastObservedDailySnapshotTimestamp ) );
            }

            if ( ds.LastWeeklySnapshotTimestamp.Value != ds.LastObservedWeeklySnapshotTimestamp )
            {
                propertiesToSet.Add( ds.UpdateProperty( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ds.LastObservedWeeklySnapshotTimestamp ) );
            }

            if ( ds.LastMonthlySnapshotTimestamp.Value != ds.LastObservedMonthlySnapshotTimestamp )
            {
                propertiesToSet.Add( ds.UpdateProperty( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ds.LastObservedMonthlySnapshotTimestamp ) );
            }

            if ( ds.LastYearlySnapshotTimestamp.Value != ds.LastObservedYearlySnapshotTimestamp )
            {
                propertiesToSet.Add( ds.UpdateProperty( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, ds.LastObservedYearlySnapshotTimestamp ) );
            }

            // ReSharper disable once InvertIf
            if ( propertiesToSet.Count > 0 )
            {
                Logger.Debug( "Timestamps are out of sync for {0} - updating properties", ds.Name );
                SetZfsProperties( settings.DryRun, ds.Name, propertiesToSet );
            }
        } );
    }

    protected static bool CheckIfPropertyIsValid( string name, string value, string source )
    {
        if ( source == "-" )
        {
            return false;
        }

        return name switch
        {
            "type" => !string.IsNullOrWhiteSpace( value ) && value == "filesystem",
            ZfsPropertyNames.EnabledPropertyName => bool.TryParse( value, out _ ),
            ZfsPropertyNames.TakeSnapshotsPropertyName => bool.TryParse( value, out _ ),
            ZfsPropertyNames.PruneSnapshotsPropertyName => bool.TryParse( value, out _ ),
            ZfsPropertyNames.RecursionPropertyName => !string.IsNullOrWhiteSpace( value ) && value is ZfsPropertyValueConstants.SnapsInAZfs or ZfsPropertyValueConstants.ZfsRecursion,
            ZfsPropertyNames.TemplatePropertyName => !string.IsNullOrWhiteSpace( value ),
            ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => int.TryParse( value, out int intValue ) && intValue >= 0,
            ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => int.TryParse( value, out int intValue ) && intValue >= 0,
            ZfsPropertyNames.SnapshotRetentionDailyPropertyName => int.TryParse( value, out int intValue ) && intValue >= 0,
            ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => int.TryParse( value, out int intValue ) && intValue >= 0,
            ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => int.TryParse( value, out int intValue ) && intValue >= 0,
            ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => int.TryParse( value, out int intValue ) && intValue >= 0,
            ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => int.TryParse( value, out int intValue ) && intValue is >= 0 and <= 100,
            ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => DateTimeOffset.TryParse( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
            ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => DateTimeOffset.TryParse( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
            ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => DateTimeOffset.TryParse( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
            ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => DateTimeOffset.TryParse( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
            ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => DateTimeOffset.TryParse( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
            ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => DateTimeOffset.TryParse( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
            _ => throw new ArgumentOutOfRangeException( nameof( name ) )
        };
    }

    /// <summary>
    ///     Iterates over <paramref name="lineProvider" /> and builds a collection of raw objects from the provided values
    /// </summary>
    /// <param name="lineProvider">
    ///     A <see cref="ConfiguredCancelableAsyncEnumerable{T}" /> (<see langword="string" />) that provides text output in the same
    ///     format as <c>zfs get all -Hpr</c>
    /// </param>
    /// <param name="rawObjects">
    ///     The collection of <see cref="RawZfsObject" />s, indexed and sorted by name, this method will build from the output provided
    ///     by
    ///     <paramref name="lineProvider" />
    /// </param>
    protected static async Task GetRawZfsObjectsAsync( ConfiguredCancelableAsyncEnumerable<string> lineProvider, SortedDictionary<string, RawZfsObject> rawObjects )
    {
        await foreach ( string zfsGetLine in lineProvider )
        {
            string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

            if ( !rawObjects.TryGetValue( lineTokens[ 0 ], out RawZfsObject? obj ) )
            {
                rawObjects.Add( lineTokens[ 0 ], new( lineTokens[ 0 ], lineTokens[ 2 ] ) );
                continue;
            }

            obj.Properties.Add( lineTokens[ 1 ], new( lineTokens[ 1 ], lineTokens[ 2 ], lineTokens[ 3 ] ) );
        }
    }

    protected static void ParseAndValidatePoolRootZfsGetLine( string[] lineTokens, ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> rootsAndTheirProperties )
    {
        string poolName = lineTokens[ 0 ];
        string propName = lineTokens[ 1 ];
        string propValue = lineTokens[ 2 ];
        string propSource = lineTokens[ 3 ];
        rootsAndTheirProperties.AddOrUpdate( poolName, AddNewDatasetWithProperty, AddPropertyToExistingDs );

        ConcurrentDictionary<string, bool> AddNewDatasetWithProperty( string key )
        {
            ConcurrentDictionary<string, bool> newDs = new( )
            {
                [ propName ] = CheckIfPropertyIsValid( propName, propValue, propSource )
            };
            return newDs;
        }

        ConcurrentDictionary<string, bool> AddPropertyToExistingDs( string key, ConcurrentDictionary<string, bool> properties )
        {
            properties[ propName ] = CheckIfPropertyIsValid( propName, propValue, propSource );
            return properties;
        }
    }

    protected static void ProcessRawObjects( SortedDictionary<string, RawZfsObject> rawObjects, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        foreach ( ( string objName, RawZfsObject obj ) in rawObjects )
        {
            switch ( obj.Kind )
            {
                case ZfsPropertyValueConstants.FileSystem:
                case ZfsPropertyValueConstants.Volume:
                    CreateAndAddDatasetFromRawObject( objName, obj, datasets );
                    break;
                case ZfsPropertyValueConstants.Snapshot:
                    CreateAndAddSnapshotFromRawObject( objName, obj, datasets, snapshots );
                    break;
            }
        }
    }

    private static bool TryParseDatasetProperties( string dsName, RawZfsObject rawZfsObject, [NotNullWhen( true )] out ZfsProperty<bool>? enabled, [NotNullWhen( true )] out ZfsProperty<bool>? takeSnapshots, [NotNullWhen( true )] out ZfsProperty<bool>? pruneSnapshots, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, [NotNullWhen( true )] out ZfsProperty<int>? retentionFrequent, [NotNullWhen( true )] out ZfsProperty<int>? retentionHourly, [NotNullWhen( true )] out ZfsProperty<int>? retentionDaily, [NotNullWhen( true )] out ZfsProperty<int>? retentionWeekly, [NotNullWhen( true )] out ZfsProperty<int>? retentionMonthly, [NotNullWhen( true )] out ZfsProperty<int>? retentionYearly, [NotNullWhen( true )] out ZfsProperty<int>? retentionPruneDeferral, out long bytesAvailable, out long bytesUsed )
    {
        bytesAvailable = 0;
        bytesUsed = 0;

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.EnabledPropertyName ], out enabled ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.EnabledPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.EnabledPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.TakeSnapshotsPropertyName ], out takeSnapshots ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.TakeSnapshotsPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.TakeSnapshotsPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.PruneSnapshotsPropertyName ], out pruneSnapshots ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.PruneSnapshotsPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.PruneSnapshotsPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ], out lastFrequentSnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ], out lastHourlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ], out lastDailySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ], out lastWeeklySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ], out lastMonthlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ], out lastYearlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        recursion = ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.RecursionPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.RecursionPropertyName ].Value, rawZfsObject.Properties[ ZfsPropertyNames.RecursionPropertyName ].Source == ZfsPropertySourceConstants.Local );
        template = ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.TemplatePropertyName, rawZfsObject.Properties[ ZfsPropertyNames.TemplatePropertyName ].Value, rawZfsObject.Properties[ ZfsPropertyNames.TemplatePropertyName ].Source == ZfsPropertySourceConstants.Local );

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ], out retentionFrequent ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ], out retentionHourly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ], out retentionDaily ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionDailyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ], out retentionWeekly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ], out retentionMonthly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ], out retentionYearly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ], out retentionPruneDeferral ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out bytesUsed );
            return false;
        }

        if ( rawZfsObject.Kind != ZfsPropertyValueConstants.Snapshot && !long.TryParse( rawZfsObject.Properties[ ZfsNativePropertyNames.Available ].Value, out bytesAvailable ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsNativePropertyNames.Available, rawZfsObject.Properties[ ZfsNativePropertyNames.Available ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out _, out bytesUsed );
            bytesAvailable = 0;
            return false;
        }

        if ( rawZfsObject.Kind != ZfsPropertyValueConstants.Snapshot && !long.TryParse( rawZfsObject.Properties[ ZfsNativePropertyNames.Used ].Value, out bytesUsed ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsNativePropertyNames.Used, rawZfsObject.Properties[ ZfsNativePropertyNames.Used ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral, out bytesAvailable, out _ );
            bytesUsed = 0;
            return false;
        }

        return true;

        static void SetAllOutParametersNull( out ZfsProperty<bool>? takeSnapshots, out ZfsProperty<bool>? pruneSnapshots, out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, out ZfsProperty<int>? retentionFrequent, out ZfsProperty<int>? retentionHourly, out ZfsProperty<int>? retentionDaily, out ZfsProperty<int>? retentionWeekly, out ZfsProperty<int>? retentionMonthly, out ZfsProperty<int>? retentionYearly, out ZfsProperty<int>? retentionPruneDeferral, out long bytesAvailable, out long bytesUsed )
        {
            takeSnapshots = null;
            pruneSnapshots = null;
            lastFrequentSnapshotTimestamp = null;
            lastHourlySnapshotTimestamp = null;
            lastDailySnapshotTimestamp = null;
            lastWeeklySnapshotTimestamp = null;
            lastMonthlySnapshotTimestamp = null;
            lastYearlySnapshotTimestamp = null;
            recursion = default;
            template = default;
            retentionFrequent = null;
            retentionHourly = null;
            retentionDaily = null;
            retentionWeekly = null;
            retentionMonthly = null;
            retentionYearly = null;
            retentionPruneDeferral = null;
            bytesAvailable = 0L;
            bytesUsed = 0L;
        }
    }

    private static void CreateAndAddDatasetFromRawObject( string dsName, RawZfsObject rawZfsObject, ConcurrentDictionary<string, ZfsRecord> datasets )
    {
        Logger.Trace( "Parsing property values for {0} {1}", rawZfsObject.Kind, dsName );

        if ( !TryParseDatasetProperties( dsName, rawZfsObject, out ZfsProperty<bool>? enabled, out ZfsProperty<bool>? takeSnapshots, out ZfsProperty<bool>? pruneSnapshots, out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, out ZfsProperty<int>? retentionFrequent, out ZfsProperty<int>? retentionHourly, out ZfsProperty<int>? retentionDaily, out ZfsProperty<int>? retentionWeekly, out ZfsProperty<int>? retentionMonthly, out ZfsProperty<int>? retentionYearly, out ZfsProperty<int>? retentionPruneDeferral, out long bytesAvailable, out long bytesUsed ) )
        {
            Logger.Error( "Failed parsing object {0} from ZFS", dsName );
            return;
        }

        string parentName = dsName.GetZfsPathParent( );
        bool isRootDs = dsName == parentName;
        ZfsRecord newDs = ZfsRecord.CreateInstanceFromAllProperties(dsName,
                                                                    rawZfsObject.Kind,
                                                                    enabled.Value,
                                                                    takeSnapshots.Value,
                                                                    pruneSnapshots.Value,
                                                                    lastFrequentSnapshotTimestamp.Value,
                                                                    lastHourlySnapshotTimestamp.Value,
                                                                    lastDailySnapshotTimestamp.Value,
                                                                    lastWeeklySnapshotTimestamp.Value,
                                                                    lastMonthlySnapshotTimestamp.Value,
                                                                    lastYearlySnapshotTimestamp.Value,
                                                                    recursion,
                                                                    template,
                                                                    retentionFrequent.Value,
                                                                    retentionHourly.Value,
                                                                    retentionDaily.Value,
                                                                    retentionWeekly.Value,
                                                                    retentionMonthly.Value,
                                                                    retentionYearly.Value,
                                                                    retentionPruneDeferral.Value,
                                                                    bytesAvailable,
                                                                    bytesUsed,
                                                                    isRootDs ? null : datasets[ parentName ] );
        datasets[ dsName ] = newDs;
        if ( !isRootDs )
        {
            datasets[ parentName ].AddDataset( newDs );
        }
    }

    private static void CreateAndAddSnapshotFromRawObject( string snapName, RawZfsObject rawZfsObject, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        Logger.Trace( "Parsing property values for snapshot {0}", snapName );

        if ( !TryParseDatasetProperties( snapName, rawZfsObject, out ZfsProperty<bool>? enabled, out ZfsProperty<bool>? takeSnapshots, out ZfsProperty<bool>? pruneSnapshots, out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, out ZfsProperty<int>? retentionFrequent, out ZfsProperty<int>? retentionHourly, out ZfsProperty<int>? retentionDaily, out ZfsProperty<int>? retentionWeekly, out ZfsProperty<int>? retentionMonthly, out ZfsProperty<int>? retentionYearly, out ZfsProperty<int>? retentionPruneDeferral, out long _, out long _ ) )
        {
            return;
        }

        if ( !DateTimeOffset.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotTimestampPropertyName ].Value, out DateTimeOffset snapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for snapshot {2} - skipping object", ZfsPropertyNames.SnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotTimestampPropertyName ].Value, snapName );
            return;
        }

        string parentName = snapName.GetZfsPathParent( );
        ZfsRecord dataset = datasets[ parentName ];
        snapshots[ snapName ] = dataset.CreateAndAddSnapshot( snapName,
                                                        enabled.Value,
                                                        takeSnapshots.Value,
                                                        pruneSnapshots.Value,
                                                        lastFrequentSnapshotTimestamp.Value,
                                                        lastHourlySnapshotTimestamp.Value,
                                                        lastDailySnapshotTimestamp.Value,
                                                        lastWeeklySnapshotTimestamp.Value,
                                                        lastMonthlySnapshotTimestamp.Value,
                                                        lastYearlySnapshotTimestamp.Value,
                                                        recursion,
                                                        template,
                                                        retentionFrequent.Value,
                                                        retentionHourly.Value,
                                                        retentionDaily.Value,
                                                        retentionWeekly.Value,
                                                        retentionMonthly.Value,
                                                        retentionYearly.Value,
                                                        retentionPruneDeferral.Value,
                                                        rawZfsObject.Properties[ ZfsPropertyNames.SnapshotNamePropertyName ].Value,
                                                        rawZfsObject.Properties[ ZfsPropertyNames.SnapshotPeriodPropertyName ].Value,
                                                        snapshotTimestamp,
                                                        dataset );
        Logger.Trace( "Snapshot object {0} added to {1} collection and parent {2}", snapName, nameof( snapshots ), parentName );
    }
}
