// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
        { ZfsPropertyNames.SnapshotPeriodPropertyName, ZfsProperty<string>.CreateWithoutParent( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.NotSet ) },
        { ZfsPropertyNames.SnapshotTimestampPropertyName, ZfsProperty<DateTimeOffset>.CreateWithoutParent( ZfsPropertyNames.SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch ) }
    } );
    public bool IsLocal { get; init; }

    public ZfsRecord? Owner { get; set; }
    public static ImmutableSortedSet<string> KnownDatasetProperties { get; }

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; }

    string Name { get; }
    string SetString { get; }
    string Source { get; }
    string ValueString { get; }
}
