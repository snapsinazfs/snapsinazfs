// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

public abstract class ZfsCommandRunnerBase : IZfsCommandRunner
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public abstract bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot );

    /// <inheritdoc />
    public abstract Task<bool> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings );

    /// <inheritdoc />
    public abstract Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, ZfsRecord> datasets );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, params IZfsProperty[] properties );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, ZfsRecord>> GetPoolRootDatasetsWithAllRequiredSnapsInAZfsPropertiesAsync( );

    /// <inheritdoc />
    public abstract Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

    public abstract IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );

    /// <inheritdoc />
    public abstract IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args );

    public abstract Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( );

    /// <inheritdoc />
    public abstract bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string poolName, string[] propertyArray );

    public static void ParsePoolRootDatasetZfsGetLineForConfigConsoleTree( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets, string[] lineTokens, List<ITreeNode> treeRootNodes, ConcurrentDictionary<string, TreeNode> allTreeNodes )
    {
        string propertyValue = lineTokens[ 2 ];
        string dsName = lineTokens[ 0 ];
        baseDatasets.AddOrUpdate( dsName, k =>
        {
            ZfsRecord newRootDsBaseCopy = new( k, propertyValue );
            ZfsRecord newRootDsTreeCopy = newRootDsBaseCopy with { };
            ZfsObjectConfigurationTreeNode node = new( dsName, newRootDsBaseCopy, newRootDsTreeCopy );
            Logger.Debug( "Adding new pool root object {0} to collections", newRootDsBaseCopy.Name );
            treeRootNodes.Add( node );
            allTreeNodes[ dsName ] = node;
            treeDatasets.TryAdd( k, newRootDsTreeCopy );
            return newRootDsBaseCopy;
        }, ( _, ds ) =>
        {
            string propertyName = lineTokens[ 1 ];
            string propertySource = lineTokens[ 3 ];
            ds.UpdateProperty( propertyName, propertyValue, propertySource );
            treeDatasets[ dsName ].UpdateProperty( propertyName, propertyValue, propertySource );
            Logger.Debug( "Updating property {0} for {1} to {2}", propertyName, dsName, propertyValue );
            return ds;
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

    protected static void ParseDatasetZfsGetLine( string zfsGetLine, ConcurrentDictionary<string, ZfsRecord> allDatasets )
    {
        Logger.Debug( "Attempting to parse line {0} from zfs", zfsGetLine );
        if ( string.IsNullOrWhiteSpace( zfsGetLine ) )
        {
            Logger.Error( "Error reading output from zfs. Null or empty line." );
            return;
        }

        string[] lineTokens = zfsGetLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
        // zfs get operations without an -o argument return 4 values per line
        if ( lineTokens.Length != 4 )
        {
            Logger.Error( "Line not understood. Expected 4 tab-separated tokens. Got {0}: '{1}'", lineTokens.Length, zfsGetLine );
            return;
        }

        string dsName = lineTokens[ 0 ];
        string propertyName = lineTokens[ 1 ];
        string propertyValue = lineTokens[ 2 ];
        string propertySource = lineTokens[ 3 ];
        Logger.Debug( "Checking for existence of dataset {0} in collection", dsName );
        if ( !allDatasets.ContainsKey( dsName ) )
        {
            string dsKind = lineTokens[ 2 ];
            string parentDsName = dsName.GetZfsPathParent( );
            if ( dsName == parentDsName )
            {
                Logger.Debug( "Dataset {0} is a pool root filesystem", dsName );
                if ( allDatasets.TryAdd( dsName, new( dsName, dsKind ) ) )
                {
                    Logger.Debug( "Added pool root filesystem {0} to collection", dsName );
                    return;
                }

                Logger.Error( "Failed adding pool root filesystem {0} to dictionary. Taking and pruning of snapshots for this Dataset and descendents may not be performed", dsName );
                return;
            }

            Logger.Debug( "{1} {0} not in dictionary. Attempting to add as child of {2}", dsName, dsKind, parentDsName );
            if ( allDatasets.TryAdd( dsName, new( dsName, dsKind, allDatasets[ parentDsName ] ) ) )
            {
                Logger.Debug( "Added {1} {0} to dictionary", dsName, dsKind );
                return;
            }

            Logger.Error( "Failed adding {1} {0} to dictionary. Taking and pruning of snapshots for this {1} and descendents may not be performed", dsName, dsKind );
            return;
        }

        Logger.Debug( "Adding property {0}({1}) to {2}", propertyName, propertyValue, dsName );
        allDatasets[ dsName ].UpdateProperty( propertyName, propertyValue, propertySource );
    }

    protected static void ParseDatasetZfsGetLineForConfigConsoleTree( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets, string[] lineTokens, ConcurrentDictionary<string, TreeNode> allTreeNodes )
    {
        string dsName = lineTokens[ 0 ];
        string propertyValue = lineTokens[ 2 ];
        if ( !baseDatasets.ContainsKey( dsName ) )
        {
            Logger.Trace( "{0} is not in dictionary. Creating a new {1}", dsName, propertyValue );
            string parentName = dsName.GetZfsPathParent( );
            bool isPoolRoot = dsName == parentName;
            ZfsRecord parentDsBaseCopy = baseDatasets[ parentName ];
            ZfsRecord parentDsTreeCopy = treeDatasets[ parentName ];
            ZfsRecord newDsBaseCopy = new( dsName, propertyValue, isPoolRoot ? null : parentDsBaseCopy );
            ZfsRecord newDsTreeCopy = newDsBaseCopy with { ParentDataset = parentDsTreeCopy };
            ZfsObjectConfigurationTreeNode node = new( dsName, newDsBaseCopy, newDsTreeCopy, parentDsBaseCopy, parentDsTreeCopy );
            allTreeNodes[ dsName ] = node;
            allTreeNodes[ parentName ].Children.Add( node );
            Logger.Debug( "Adding new {0} {1} to {2}", newDsBaseCopy.Kind, newDsBaseCopy.Name, parentDsBaseCopy.Name );
            baseDatasets.TryAdd( dsName, newDsBaseCopy );
            treeDatasets.TryAdd( dsName, newDsTreeCopy );
        }
        else
        {
            ZfsRecord ds = baseDatasets[ dsName ];
            if ( ds.IsPoolRoot )
            {
                Logger.Trace( "{0} is a pool root - skipping", dsName );
                return;
            }

            string propertyName = lineTokens[ 1 ];
            string propertySource = lineTokens[ 3 ];
            Logger.Debug( "Adding property {0} ({1}) - ({2}) to {3}", propertyName, propertyValue, propertySource, ds.Name );
            ds.UpdateProperty( propertyName, propertyValue, propertySource );
            treeDatasets[ ds.Name ].UpdateProperty( propertyName, propertyValue, propertySource );
        }
    }

    protected static void ParseSnapshotZfsListLine( string zfsListLine, SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> allSnapshots )
    {
        Logger.Trace( "Attempting to parse zfs list line {0}", zfsListLine );
        try
        {
            string[] zfsListTokens = zfsListLine.Split( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
            int propertyCount = IZfsProperty.KnownSnapshotProperties.Count + 1;
            if ( zfsListTokens.Length != propertyCount )
            {
                Logger.Error( "Line not understood. Expected {2} tab-separated tokens. Got {0}: {1}", zfsListTokens.Length, zfsListLine, propertyCount );
                return;
            }

            string snapName = zfsListTokens[ 2 ];
            Logger.Trace( "Checking if {0} is a SnapsInAZfs snapshot", snapName );
            if ( snapName == "-" )
            {
                Logger.Debug( "{0} is not a SnapsInAZfs snapshot. Skipping", zfsListTokens[ 0 ] );
                return;
            }

            string snapDatasetName = snapName.GetZfsPathParent( );
            Logger.Trace( "Parent dataset of {0} is {1}", snapName, snapDatasetName );
            if ( !datasets.ContainsKey( snapDatasetName ) )
            {
                Logger.Error( "Parent dataset {0} of snapshot {1} does not exist in the collection. Skipping", snapDatasetName, snapName );
                return;
            }

            Logger.Trace( "Getting parent dataset {0} of snapshot {1}", snapDatasetName, snapName );
            ZfsRecord parentDataset = datasets[ snapDatasetName ];
            Logger.Trace( "Creating new snapshot instance {0} with parent {1} {2}", snapName, parentDataset.Kind, parentDataset.Name );
            Snapshot snap = new( snapName, bool.Parse( zfsListTokens[ 1 ] ), (SnapshotPeriod)zfsListTokens[ 3 ], DateTimeOffset.Parse( zfsListTokens[ 4 ] ), parentDataset );
            allSnapshots[ snapName ] = snap.ParentDataset.AddSnapshot( snap );

            Logger.Debug( "Added snapshot {0} to {1} {2}", snapName, parentDataset.Kind, parentDataset.Name );
        }
        catch ( Exception ex )
        {
            Logger.Error( ex, "Error while creating snapshot instance from {0}", zfsListLine );
        }
    }
    protected static bool TryParseDatasetProperties( string dsName, RawZfsObject rawZfsObject, [NotNullWhen( true )] out ZfsProperty<bool>? enabled, [NotNullWhen( true )] out ZfsProperty<bool>? takeSnapshots, [NotNullWhen( true )] out ZfsProperty<bool>? pruneSnapshots, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, [NotNullWhen( true )] out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, [NotNullWhen( true )] out ZfsProperty<int>? retentionFrequent, [NotNullWhen( true )] out ZfsProperty<int>? retentionHourly, [NotNullWhen( true )] out ZfsProperty<int>? retentionDaily, [NotNullWhen( true )] out ZfsProperty<int>? retentionWeekly, [NotNullWhen( true )] out ZfsProperty<int>? retentionMonthly, [NotNullWhen( true )] out ZfsProperty<int>? retentionYearly, [NotNullWhen( true )] out ZfsProperty<int>? retentionPruneDeferral )
    {
        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.EnabledPropertyName ], out enabled ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.EnabledPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.EnabledPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.TakeSnapshotsPropertyName ], out takeSnapshots ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.TakeSnapshotsPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.TakeSnapshotsPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<bool>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.PruneSnapshotsPropertyName ], out pruneSnapshots ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.PruneSnapshotsPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.PruneSnapshotsPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ], out lastFrequentSnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ], out lastHourlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ], out lastDailySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ], out lastWeeklySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ], out lastMonthlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<DateTimeOffset>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ], out lastYearlySnapshotTimestamp ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        recursion = new( ZfsPropertyNames.RecursionPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.RecursionPropertyName ].Value, rawZfsObject.Properties[ ZfsPropertyNames.RecursionPropertyName ].Source );
        template = new( ZfsPropertyNames.TemplatePropertyName, rawZfsObject.Properties[ ZfsPropertyNames.TemplatePropertyName ].Value, rawZfsObject.Properties[ ZfsPropertyNames.TemplatePropertyName ].Source );

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ], out retentionFrequent ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ], out retentionHourly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ], out retentionDaily ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionDailyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ], out retentionWeekly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ], out retentionMonthly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ], out retentionYearly ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        if ( !ZfsProperty<int>.TryParse( rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ], out retentionPruneDeferral ) )
        {
            Logger.Debug( "{0} value {1} not valid for {2} {3} - skipping object", ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, rawZfsObject.Properties[ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ].Value, rawZfsObject.Kind, dsName );
            SetAllOutParametersNull( out takeSnapshots, out pruneSnapshots, out lastFrequentSnapshotTimestamp, out lastHourlySnapshotTimestamp, out lastDailySnapshotTimestamp, out lastWeeklySnapshotTimestamp, out lastMonthlySnapshotTimestamp, out lastYearlySnapshotTimestamp, out recursion, out template, out retentionFrequent, out retentionHourly, out retentionDaily, out retentionWeekly, out retentionMonthly, out retentionYearly, out retentionPruneDeferral );
            return false;
        }

        return true;

        static void SetAllOutParametersNull( out ZfsProperty<bool>? takeSnapshots, out ZfsProperty<bool>? pruneSnapshots, out ZfsProperty<DateTimeOffset>? lastFrequentSnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastHourlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastDailySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastWeeklySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastMonthlySnapshotTimestamp, out ZfsProperty<DateTimeOffset>? lastYearlySnapshotTimestamp, out ZfsProperty<string> recursion, out ZfsProperty<string> template, out ZfsProperty<int>? retentionFrequent, out ZfsProperty<int>? retentionHourly, out ZfsProperty<int>? retentionDaily, out ZfsProperty<int>? retentionWeekly, out ZfsProperty<int>? retentionMonthly, out ZfsProperty<int>? retentionYearly, out ZfsProperty<int>? retentionPruneDeferral )
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
        }
    }
}
