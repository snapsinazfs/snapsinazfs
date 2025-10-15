#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

using System.Collections.Frozen;
using System.Collections.Immutable;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public interface IZfsProperty
{
    static IZfsProperty ( )
    {
        KnownDatasetProperties =
            ImmutableSortedSet<string>.Empty
                                      .Union (
                                              [
                                                  ZfsPropertyNames.Enabled,
                                                  ZfsPropertyNames.TakeSnapshots,
                                                  ZfsPropertyNames.PruneSnapshots,
                                                  ZfsPropertyNames.Recursion,
                                                  ZfsPropertyNames.SourceSystem,
                                                  ZfsPropertyNames.Template,
                                                  ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp,
                                                  ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,
                                                  ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,
                                                  ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,
                                                  ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,
                                                  ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,
                                                  ZfsPropertyNames.SnapshotRetentionFrequent,
                                                  ZfsPropertyNames.SnapshotRetentionHourly,
                                                  ZfsPropertyNames.SnapshotRetentionDaily,
                                                  ZfsPropertyNames.SnapshotRetentionWeekly,
                                                  ZfsPropertyNames.SnapshotRetentionMonthly,
                                                  ZfsPropertyNames.SnapshotRetentionYearly,
                                                  ZfsPropertyNames.SnapshotRetentionPruneDeferral
                                              ]
                                             );

        KnownSnapshotProperties =
            ImmutableSortedSet<string>.Empty
                                      .Union (
                                              [
                                                  ZfsPropertyNames.SnapshotPeriod,
                                                  ZfsPropertyNames.SnapshotTimestamp,
                                                  ZfsPropertyNames.PruneSnapshots
                                              ]
                                             );

        AllKnownProperties = KnownDatasetProperties.Union ( KnownSnapshotProperties, StringComparer.OrdinalIgnoreCase ).ToImmutableSortedSet( );
    }

    /// <summary>
    ///     Gets the union of <see cref="KnownDatasetProperties" /> and <see cref="KnownSnapshotProperties" />
    /// </summary>
    public static ImmutableSortedSet<string> AllKnownProperties { get; }

    // PERFORMANCE: This could be improved by just making it a big struct and eliminating the boxing due to the interface in the dictionary.
    public static FrozenDictionary<string, IZfsProperty> DefaultDatasetProperties { get; }
        = new Dictionary<string, IZfsProperty>
          {
              { ZfsPropertyNames.Enabled, ZfsProperty<bool>.CreateWithoutParent ( ZfsPropertyNames.Enabled,               false ) },
              { ZfsPropertyNames.TakeSnapshots, ZfsProperty<bool>.CreateWithoutParent ( ZfsPropertyNames.TakeSnapshots,   false ) },
              { ZfsPropertyNames.PruneSnapshots, ZfsProperty<bool>.CreateWithoutParent ( ZfsPropertyNames.PruneSnapshots, false ) },
              { ZfsPropertyNames.Recursion, ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.Recursion, ZfsPropertyValueConstants.SnapsInAZfs ) },
              { ZfsPropertyNames.SourceSystem, ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.SourceSystem,                   ZfsPropertyValueConstants.StandaloneSiazSystem ) },
              { ZfsPropertyNames.Template, ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.Template,   "default" ) },
              { ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, DateTimeOffset.UnixEpoch ) },
              { ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp,     DateTimeOffset.UnixEpoch ) },
              { ZfsPropertyNames.DatasetLastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp,       DateTimeOffset.UnixEpoch ) },
              { ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp,     DateTimeOffset.UnixEpoch ) },
              { ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp,   DateTimeOffset.UnixEpoch ) },
              { ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp,     DateTimeOffset.UnixEpoch ) },
              { ZfsPropertyNames.SnapshotRetentionFrequent, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionFrequent,           0 ) },
              { ZfsPropertyNames.SnapshotRetentionHourly, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionHourly,               48 ) },
              { ZfsPropertyNames.SnapshotRetentionDaily, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionDaily,                 90 ) },
              { ZfsPropertyNames.SnapshotRetentionWeekly, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionWeekly,               0 ) },
              { ZfsPropertyNames.SnapshotRetentionMonthly, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionMonthly,             6 ) },
              { ZfsPropertyNames.SnapshotRetentionYearly, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionYearly,               0 ) },
              { ZfsPropertyNames.SnapshotRetentionPruneDeferral, ZfsProperty<int>.CreateWithoutParent ( ZfsPropertyNames.SnapshotRetentionPruneDeferral, 0 ) }
          }.ToFrozenDictionary( );

    public static ImmutableSortedDictionary<string, IZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, IZfsProperty>.Empty.AddRange (
                                                                                                                                                                       new Dictionary<string, IZfsProperty>
                                                                                                                                                                       {
                                                                                                                                                                           { ZfsPropertyNames.SnapshotPeriod, ZfsProperty<string>.CreateWithoutParent ( ZfsPropertyNames.SnapshotPeriod, SnapshotPeriod.NotSet ) },
                                                                                                                                                                           { ZfsPropertyNames.SnapshotTimestamp, ZfsProperty<DateTimeOffset>.CreateWithoutParent ( ZfsPropertyNames.SnapshotTimestamp, DateTimeOffset.UnixEpoch ) }
                                                                                                                                                                       }
                                                                                                                                                                      );

    bool                                     IsLocal                 { get; init; }
    public static ImmutableSortedSet<string> KnownDatasetProperties  { get; }
    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; }
    string                                   Name                    { get; }
    public ZfsRecord?                        Owner                   { get; init; }
    string                                   SetString               { get; }
    string                                   Source                  { get; }
    string                                   ValueString             { get; }
}
