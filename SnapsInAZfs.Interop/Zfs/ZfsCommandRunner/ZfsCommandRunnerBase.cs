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
    public abstract bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings template, out SnapshotRecord? snapshot );

    /// <inheritdoc />
    public abstract Task<bool> DestroySnapshotAsync( SnapshotRecord snapshot, SnapsInAZfsSettings settings );

    /// <inheritdoc />
    public abstract Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, ZfsRecord> datasets );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, params IZfsProperty[] properties );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, ZfsRecord>> GetPoolRootDatasetsWithAllRequiredSnapsInAZfsPropertiesAsync( );

    /// <inheritdoc />
    public abstract Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, SnapshotRecord> snapshots );

    public abstract IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );

    /// <inheritdoc />
    public abstract IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args );

    public abstract Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( );

    /// <inheritdoc />
    public abstract bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string poolName, string[] propertyArray );

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
            ZfsRecord newDsTreeCopy = newDsBaseCopy with { PoolRoot = parentDsTreeCopy };
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
}
