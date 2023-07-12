// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Immutable;

using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public interface IZfsProperty
{
    /// <summary>
    ///     Gets the union of <see cref="KnownDatasetProperties" /> and <see cref="KnownSnapshotProperties" />
    /// </summary>
    public static ImmutableSortedSet<string> AllKnownProperties { get; }

    static IZfsProperty( )
    {
        KnownDatasetProperties = ImmutableSortedSet<string>.Empty.Union( new[]
        {
            ZfsPropertyNames.EnabledPropertyName,
            ZfsPropertyNames.TakeSnapshotsPropertyName,
            ZfsPropertyNames.PruneSnapshotsPropertyName,
            ZfsPropertyNames.RecursionPropertyName,
            ZfsPropertyNames.TemplatePropertyName,
            ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName,
            ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName,
            ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName,
            ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName,
            ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName,
            ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName,
            ZfsPropertyNames.SnapshotRetentionFrequentPropertyName,
            ZfsPropertyNames.SnapshotRetentionHourlyPropertyName,
            ZfsPropertyNames.SnapshotRetentionDailyPropertyName,
            ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName,
            ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName,
            ZfsPropertyNames.SnapshotRetentionYearlyPropertyName,
            ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName
        } );

        KnownSnapshotProperties = ImmutableSortedSet<string>.Empty.Union( new[]
        {
            ZfsPropertyNames.SnapshotNamePropertyName,
            ZfsPropertyNames.SnapshotPeriodPropertyName,
            ZfsPropertyNames.SnapshotTimestampPropertyName,
            ZfsPropertyNames.PruneSnapshotsPropertyName
        } );

        AllKnownProperties = KnownDatasetProperties.Union( KnownSnapshotProperties );
    }

    public static ImmutableDictionary<string, IZfsProperty> DefaultDatasetProperties { get; } = ImmutableDictionary<string, IZfsProperty>.Empty.AddRange( new Dictionary<string, IZfsProperty>
    {
        { ZfsPropertyNames.EnabledPropertyName, ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.EnabledPropertyName, false ) },
        { ZfsPropertyNames.TakeSnapshotsPropertyName, ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.TakeSnapshotsPropertyName, false ) },
        { ZfsPropertyNames.PruneSnapshotsPropertyName, ZfsProperty<bool>.CreateWithoutParent( ZfsPropertyNames.PruneSnapshotsPropertyName, false ) },
        { ZfsPropertyNames.RecursionPropertyName, ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs ) },
        { ZfsPropertyNames.TemplatePropertyName, ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.TemplatePropertyName, "default" ) },
        { ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) },
        { ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) },
        { ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) },
        { ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) },
        { ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) },
        { ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) },
        { ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 0 ) },
        { ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, 48 ) },
        { ZfsPropertyNames.SnapshotRetentionDailyPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, 90 ) },
        { ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, 0 ) },
        { ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, 6 ) },
        { ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, 0 ) },
        { ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, ZfsProperty<int>.CreateWithoutParent( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 ) }
    } );

    public static ImmutableSortedDictionary<string, IZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, IZfsProperty>.Empty.AddRange( new Dictionary<string, IZfsProperty>
    {
        { ZfsPropertyNames.SnapshotNamePropertyName, ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.SnapshotNamePropertyName, ZfsPropertyValueConstants.None ) },
        { ZfsPropertyNames.SnapshotPeriodPropertyName, ZfsProperty<SnapshotPeriod>.CreateWithoutParent( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.NotSet ) },
        { ZfsPropertyNames.SnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) }
    } );

    bool IsInherited => !IsLocal;
    public bool IsLocal { get; init; }

    public ZfsRecord? Owner { get; set; }
    public static ImmutableSortedSet<string> KnownDatasetProperties { get; }

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; }

    string Name { get; }
    string SetString { get; }
    string Source { get; }
    string ValueString { get; }
}
