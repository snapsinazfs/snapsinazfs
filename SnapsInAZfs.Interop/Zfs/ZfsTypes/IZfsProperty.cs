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
        { ZfsPropertyNames.EnabledPropertyName, new ZfsProperty<bool>( ZfsPropertyNames.EnabledPropertyName, false, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.TakeSnapshotsPropertyName, new ZfsProperty<bool>( ZfsPropertyNames.TakeSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.PruneSnapshotsPropertyName, new ZfsProperty<bool>( ZfsPropertyNames.PruneSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.RecursionPropertyName, new ZfsProperty<string>( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.TemplatePropertyName, new ZfsProperty<string>( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, 0, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, 48, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionDailyPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, 90, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, 0, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, 6, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, 0, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local ) }
    } );

    public static ImmutableSortedDictionary<string, IZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, IZfsProperty>.Empty.AddRange( new Dictionary<string, IZfsProperty>
    {
        { ZfsPropertyNames.SnapshotNamePropertyName, new ZfsProperty<string>( ZfsPropertyNames.SnapshotNamePropertyName, ZfsPropertyValueConstants.None, ZfsPropertySourceConstants.SnapsInAZfs ) },
        { ZfsPropertyNames.SnapshotPeriodPropertyName, new ZfsProperty<SnapshotPeriod>( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.NotSet, ZfsPropertySourceConstants.SnapsInAZfs ) },
        { ZfsPropertyNames.SnapshotTimestampPropertyName, new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.SnapsInAZfs ) }
    } );

    string InheritedFrom { get; }
    bool IsInherited { get; }
    bool IsLocal { get; }

    public static ImmutableSortedSet<string> KnownDatasetProperties { get; }

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; }

    string Name { get; }
    string SetString { get; }
    string Source { get; }
    string ValueString { get; }
}
