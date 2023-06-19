// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using NLog;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui.Trees;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

public abstract class ZfsCommandRunnerBase : IZfsCommandRunner
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public abstract bool TakeSnapshot( Dataset ds, SnapshotPeriod period, DateTimeOffset timestamp, SanoidSettings sanoidSettings, TemplateSettings template, out Snapshot snapshot );

    /// <inheritdoc />
    public abstract Task<bool> DestroySnapshotAsync( Snapshot snapshot, SanoidSettings settings );

    /// <inheritdoc />
    public abstract Task<bool> GetPoolCapacitiesAsync( ConcurrentDictionary<string, Dataset> datasets );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties );

    /// <inheritdoc />
    public abstract bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties );

    /// <inheritdoc />
    public abstract Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" );

    /// <inheritdoc />
    public abstract Task<ConcurrentDictionary<string, Dataset>> GetPoolRootDatasetsWithAllRequiredSanoidPropertiesAsync( );

    /// <inheritdoc />
    public abstract Task GetDatasetsAndSnapshotsFromZfsAsync( ConcurrentDictionary<string, Dataset> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

    public abstract IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args );

    /// <inheritdoc />
    public abstract IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args );

    public abstract Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, SanoidZfsDataset> baseDatasets, ConcurrentDictionary<string, SanoidZfsDataset> treeDatasets );

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
            ZfsPropertyNames.RecursionPropertyName => !string.IsNullOrWhiteSpace( value ) && value is "sanoid" or "zfs",
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
