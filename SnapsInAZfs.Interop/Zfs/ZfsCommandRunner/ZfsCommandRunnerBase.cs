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
}
