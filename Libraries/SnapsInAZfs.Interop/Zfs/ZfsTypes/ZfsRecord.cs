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

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public partial record ZfsRecord : IComparable<ZfsRecord>, IEqualityOperators<ZfsRecord, ZfsRecord, bool>
{
    private static readonly Logger Logger = LogManager.GetLogger ( $"{StringConstants.ZfsTypesNamespace}.{nameof (ZfsRecord)}" )!;

    /// <summary>
    ///     Represents a node in the ZFS hierarchy.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="sourceSystem"/> is <see langword="null"/>, empty, or only whitespace
    /// </exception>
    public ZfsRecord ( string Name, string Kind, string sourceSystem = "", bool inheritProperties = true, ZfsRecord? parent = null )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace ( sourceSystem, nameof (sourceSystem) );

        this.Name     = Name;
        IsPoolRoot    = parent is null;
        ParentDataset = parent ?? this;
        this.Kind     = Kind;

        NameValidatorRegex = Kind switch
                             {
                                 ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex ( ),
                                 ZfsPropertyValueConstants.Volume     => ZfsIdentifierRegexes.DatasetNameRegex ( ),
                                 ZfsPropertyValueConstants.Snapshot   => ZfsIdentifierRegexes.SnapshotNameRegex ( ),
                                 _                                    => throw new InvalidOperationException ( "Unknown type of object specified for ZfsIdentifierValidator." )
                             };

        if ( parent is { } && inheritProperties )
        {
            _enabled                        = parent.Enabled with { IsLocal = false, Owner = this };
            _lastDailySnapshotTimestamp     = parent.LastDailySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastFrequentSnapshotTimestamp  = parent.LastFrequentSnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastHourlySnapshotTimestamp    = parent.LastHourlySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastMonthlySnapshotTimestamp   = parent.LastMonthlySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastWeeklySnapshotTimestamp    = parent.LastWeeklySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastYearlySnapshotTimestamp    = parent.LastYearlySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _pruneSnapshotsField            = parent.PruneSnapshots with { IsLocal = false, Owner = this };
            _recursion                      = parent.Recursion with { IsLocal = false, Owner = this };
            _snapshotRetentionDaily         = parent.SnapshotRetentionDaily with { IsLocal = false, Owner = this };
            _snapshotRetentionFrequent      = parent.SnapshotRetentionFrequent with { IsLocal = false, Owner = this };
            _snapshotRetentionHourly        = parent.SnapshotRetentionHourly with { IsLocal = false, Owner = this };
            _snapshotRetentionMonthly       = parent.SnapshotRetentionMonthly with { IsLocal = false, Owner = this };
            _snapshotRetentionPruneDeferral = parent.SnapshotRetentionPruneDeferral with { IsLocal = false, Owner = this };
            _snapshotRetentionWeekly        = parent.SnapshotRetentionWeekly with { IsLocal = false, Owner = this };
            _snapshotRetentionYearly        = parent.SnapshotRetentionYearly with { IsLocal = false, Owner = this };
            _sourceSystem                   = parent.SourceSystem with { IsLocal = false, Value = sourceSystem, Owner = this };
            _takeSnapshots                  = parent.TakeSnapshots with { IsLocal = false, Owner = this };
            _template                       = parent.Template with { IsLocal = false, Owner = this };
        }
        else
        {
            _enabled                        = new ( this, ZfsPropertyNames.EnabledPropertyName, false );
            _lastDailySnapshotTimestamp     = new ( this, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastFrequentSnapshotTimestamp  = new ( this, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastHourlySnapshotTimestamp    = new ( this, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastMonthlySnapshotTimestamp   = new ( this, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastWeeklySnapshotTimestamp    = new ( this, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastYearlySnapshotTimestamp    = new ( this, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _pruneSnapshotsField            = new ( this, ZfsPropertyNames.PruneSnapshotsPropertyName, false );
            _recursion                      = new ( this, ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs );
            _snapshotRetentionDaily         = new ( this, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1 );
            _snapshotRetentionFrequent      = new ( this, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1 );
            _snapshotRetentionHourly        = new ( this, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1 );
            _snapshotRetentionMonthly       = new ( this, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1 );
            _snapshotRetentionPruneDeferral = new ( this, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 );
            _snapshotRetentionWeekly        = new ( this, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1 );
            _snapshotRetentionYearly        = new ( this, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1 );
            _sourceSystem                   = new ( this, ZfsPropertyNames.SourceSystem, sourceSystem );
            _takeSnapshots                  = new ( this, ZfsPropertyNames.TakeSnapshotsPropertyName, false );
            _template                       = new ( this, ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default );
        }
    }

    /// <summary>
    ///     (Protected) Creates a new instance of a <see cref="ZfsRecord"/> from all supplied properties.
    /// </summary>
    /// <exception cref="ArgumentException">sourceSystem must have a non-null, non-whitespace-only Value</exception>
    protected ZfsRecord (
        string                         name,
        string                         kind,
        in ZfsProperty<bool>           enabled,
        in ZfsProperty<bool>           takeSnapshots,
        in ZfsProperty<bool>           pruneSnapshots,
        in ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp,
        in ZfsProperty<string>         recursion,
        in ZfsProperty<string>         template,
        in ZfsProperty<int>            retentionFrequent,
        in ZfsProperty<int>            retentionHourly,
        in ZfsProperty<int>            retentionDaily,
        in ZfsProperty<int>            retentionWeekly,
        in ZfsProperty<int>            retentionMonthly,
        in ZfsProperty<int>            retentionYearly,
        in ZfsProperty<int>            retentionPruneDeferral,
        in ZfsProperty<string>         sourceSystem,
        in long                        bytesAvailable,
        in long                        bytesUsed,
        ZfsRecord?                     parent                 = null,
        bool                           forcePropertyOwnership = false
    )
    {
        if ( string.IsNullOrWhiteSpace ( sourceSystem.Value ) )
        {
            throw new ArgumentException ( "sourceSystem must have a non-null, non-whitespace-only Value", nameof (sourceSystem) );
        }

        Name          = name;
        IsPoolRoot    = parent is null;
        ParentDataset = parent ?? this;
        Kind          = kind;

        NameValidatorRegex = kind switch
                             {
                                 ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex ( ),
                                 ZfsPropertyValueConstants.Volume     => ZfsIdentifierRegexes.DatasetNameRegex ( ),
                                 ZfsPropertyValueConstants.Snapshot   => ZfsIdentifierRegexes.SnapshotNameRegex ( ),
                                 _                                    => throw new InvalidOperationException ( "Unknown type of object specified for ZfsIdentifierValidator." )
                             };
        bool notASnapshot = Kind != ZfsPropertyValueConstants.Snapshot;
        bool isASnapshot  = parent is { } && Kind == ZfsPropertyValueConstants.Snapshot;

        if ( forcePropertyOwnership || isASnapshot )
        {
            _enabled             = enabled with { Owner = this, IsLocal = notASnapshot       && enabled.IsLocal };
            _takeSnapshots       = takeSnapshots with { Owner = this, IsLocal = notASnapshot && takeSnapshots.IsLocal };
            _pruneSnapshotsField = pruneSnapshots with { Owner = this };

            _lastFrequentSnapshotTimestamp = notASnapshot && lastFrequentSnapshotTimestamp.IsLocal
                                                 ? lastFrequentSnapshotTimestamp with { Owner = this }
                                                 : new ( this, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _lastHourlySnapshotTimestamp = notASnapshot && lastHourlySnapshotTimestamp.IsLocal
                                               ? lastHourlySnapshotTimestamp with { Owner = this }
                                               : new ( this, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _lastDailySnapshotTimestamp = notASnapshot && lastDailySnapshotTimestamp.IsLocal
                                              ? lastDailySnapshotTimestamp with { Owner = this }
                                              : new ( this, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _lastWeeklySnapshotTimestamp = notASnapshot && lastWeeklySnapshotTimestamp.IsLocal
                                               ? lastWeeklySnapshotTimestamp with { Owner = this }
                                               : new ( this, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _lastMonthlySnapshotTimestamp = notASnapshot && lastMonthlySnapshotTimestamp.IsLocal
                                                ? lastMonthlySnapshotTimestamp with { Owner = this }
                                                : new ( this, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _lastYearlySnapshotTimestamp = notASnapshot && lastYearlySnapshotTimestamp.IsLocal
                                               ? lastYearlySnapshotTimestamp with { Owner = this }
                                               : new ( this, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _recursion    = recursion with { Owner = this, IsLocal = isASnapshot || recursion.IsLocal };
            _template     = template with { Owner = this, IsLocal = isASnapshot  || template.IsLocal };
            _sourceSystem = sourceSystem with { Owner = this };

            _snapshotRetentionFrequent      = retentionFrequent with { Owner = this, IsLocal = notASnapshot      && retentionFrequent.IsLocal };
            _snapshotRetentionHourly        = retentionHourly with { Owner = this, IsLocal = notASnapshot        && retentionHourly.IsLocal };
            _snapshotRetentionDaily         = retentionDaily with { Owner = this, IsLocal = notASnapshot         && retentionDaily.IsLocal };
            _snapshotRetentionWeekly        = retentionWeekly with { Owner = this, IsLocal = notASnapshot        && retentionWeekly.IsLocal };
            _snapshotRetentionMonthly       = retentionMonthly with { Owner = this, IsLocal = notASnapshot       && retentionMonthly.IsLocal };
            _snapshotRetentionYearly        = retentionYearly with { Owner = this, IsLocal = notASnapshot        && retentionYearly.IsLocal };
            _snapshotRetentionPruneDeferral = retentionPruneDeferral with { Owner = this, IsLocal = notASnapshot && retentionPruneDeferral.IsLocal };

            BytesAvailable = isASnapshot ? parent!.BytesAvailable : bytesAvailable;
            BytesUsed      = isASnapshot ? parent!.BytesUsed : bytesUsed;
        }
        else
        {
            _enabled             = enabled;
            _takeSnapshots       = takeSnapshots;
            _pruneSnapshotsField = pruneSnapshots;

            // If local, take what we were given. Otherwise, construct a default property, as this should NEVER be inherited
            _lastFrequentSnapshotTimestamp = lastFrequentSnapshotTimestamp.IsLocal ? lastFrequentSnapshotTimestamp : new ( this, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastHourlySnapshotTimestamp   = lastHourlySnapshotTimestamp.IsLocal ? lastHourlySnapshotTimestamp : new ( this, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastDailySnapshotTimestamp    = lastDailySnapshotTimestamp.IsLocal ? lastDailySnapshotTimestamp : new ( this, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastWeeklySnapshotTimestamp   = lastWeeklySnapshotTimestamp.IsLocal ? lastWeeklySnapshotTimestamp : new ( this, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastMonthlySnapshotTimestamp  = lastMonthlySnapshotTimestamp.IsLocal ? lastMonthlySnapshotTimestamp : new ( this, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastYearlySnapshotTimestamp   = lastYearlySnapshotTimestamp.IsLocal ? lastYearlySnapshotTimestamp : new ( this, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );

            _recursion    = recursion;
            _template     = template;
            _sourceSystem = sourceSystem;

            _snapshotRetentionFrequent      = retentionFrequent;
            _snapshotRetentionHourly        = retentionHourly;
            _snapshotRetentionDaily         = retentionDaily;
            _snapshotRetentionWeekly        = retentionWeekly;
            _snapshotRetentionMonthly       = retentionMonthly;
            _snapshotRetentionYearly        = retentionYearly;
            _snapshotRetentionPruneDeferral = retentionPruneDeferral;
            BytesAvailable                  = bytesAvailable;
            BytesUsed                       = bytesUsed;
        }
    }

    private ImmutableSortedDictionary<string, ZfsRecord>? _sortedChildDatasets;

    public long BytesAvailable { get; init; }
    public long BytesUsed      { get; init; }

    public int            ChildDatasetCount                     => _childDatasets.Count;
    public bool           IsPoolRoot                            { get; }
    public string         Kind                                  { get; }
    public DateTimeOffset LastObservedDailySnapshotTimestamp    { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedFrequentSnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedHourlySnapshotTimestamp   { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedMonthlySnapshotTimestamp  { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedWeeklySnapshotTimestamp   { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedYearlySnapshotTimestamp   { get; private set; } = DateTimeOffset.UnixEpoch;
    public string         Name                                  { get; }

    [JsonIgnore]
    public ZfsRecord ParentDataset { get; }

    public int SnapshotCount => Snapshots.Values.Sum ( static d => d.Count );

    public ConcurrentDictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>> Snapshots { get; } = GetNewSnapshotCollection ( );

    public bool SubscribedToParentPropertyChangeEvents { get; private set; }

    [JsonIgnore]
    internal Regex NameValidatorRegex { get; }

    private SortedDictionary<string, ZfsRecord> _childDatasets { get; } = new ( );

    [JsonIgnore]
    private long PercentBytesUsed => BytesUsed * 100 / BytesAvailable;

    /// <inheritdoc/>
    public int CompareTo ( ZfsRecord? other )
    {
        // If the other snapshot is null, consider this record earlier rank
        if ( other is null )
        {
            return -1;
        }

        return string.Compare ( Name, other.Name, StringComparison.Ordinal );
    }

    /// <inheritdoc/>
    /// <remarks>
    ///     The default generated record equality causes stack overflow due to the self-reference in root datasets,
    ///     so it is excluded from equality comparison.<br/>
    ///     This equality method ONLY checks value properties and does not check the content of collections or references to other
    ///     <see cref="ZfsRecord"/>s.<br/>
    ///     It does, however, check <see cref="SnapshotCount"/> and <see cref="ChildDatasetCount"/>.
    /// </remarks>
    public virtual bool Equals ( ZfsRecord? other ) =>

        // Returning equality of value properties alphabetically
        other is { }
     && BytesAvailable                        == other.BytesAvailable
     && BytesUsed                             == other.BytesUsed
     && Enabled                               == other.Enabled
     && IsPoolRoot                            == other.IsPoolRoot
     && Kind                                  == other.Kind
     && LastDailySnapshotTimestamp            == other.LastDailySnapshotTimestamp
     && LastFrequentSnapshotTimestamp         == other.LastFrequentSnapshotTimestamp
     && LastHourlySnapshotTimestamp           == other.LastHourlySnapshotTimestamp
     && LastMonthlySnapshotTimestamp          == other.LastMonthlySnapshotTimestamp
     && LastObservedDailySnapshotTimestamp    == other.LastObservedDailySnapshotTimestamp
     && LastObservedFrequentSnapshotTimestamp == other.LastObservedFrequentSnapshotTimestamp
     && LastObservedHourlySnapshotTimestamp   == other.LastObservedHourlySnapshotTimestamp
     && LastObservedWeeklySnapshotTimestamp   == other.LastObservedWeeklySnapshotTimestamp
     && LastObservedMonthlySnapshotTimestamp  == other.LastObservedMonthlySnapshotTimestamp
     && LastObservedYearlySnapshotTimestamp   == other.LastObservedYearlySnapshotTimestamp
     && LastWeeklySnapshotTimestamp           == other.LastWeeklySnapshotTimestamp
     && LastYearlySnapshotTimestamp           == other.LastYearlySnapshotTimestamp
     && Name                                  == other.Name
     && PruneSnapshots                        == other.PruneSnapshots
     && Recursion                             == other.Recursion
     && SnapshotRetentionDaily                == other.SnapshotRetentionDaily
     && SnapshotRetentionFrequent             == other.SnapshotRetentionFrequent
     && SnapshotRetentionHourly               == other.SnapshotRetentionHourly
     && SnapshotRetentionMonthly              == other.SnapshotRetentionMonthly
     && SnapshotRetentionPruneDeferral        == other.SnapshotRetentionPruneDeferral
     && SnapshotRetentionWeekly               == other.SnapshotRetentionWeekly
     && SnapshotRetentionYearly               == other.SnapshotRetentionYearly
     && SourceSystem                          == other.SourceSystem
     && TakeSnapshots                         == other.TakeSnapshots
     && Template                              == other.Template
     && SnapshotCount                         == other.SnapshotCount
     && ChildDatasetCount                     == other.ChildDatasetCount;

    /// <summary>
    ///     Adds a <see cref="ZfsRecord"/> as an immediate descendant of the current <see cref="ZfsRecord"/> and subscribes the
    ///     descendant to events published by the current <see cref="ZfsRecord"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     If the <see cref="ParentDataset"/> property of <paramref name="childDataset"/> does not reference this
    ///     <see cref="ZfsRecord"/> instance.
    /// </exception>
    public void AddDataset ( ZfsRecord childDataset )
    {
        if ( !ReferenceEquals ( childDataset.ParentDataset, this ) )
        {
            throw new ArgumentException ( $"Cannot add child dataset to {Name}. {childDataset.Name} already references parent {childDataset.ParentDataset.Name}.", nameof (childDataset) );
        }

        SubscribeChildToPropertyEvents ( childDataset );
        _childDatasets [ childDataset.Name ] = childDataset;
    }

    /// <summary>
    ///     Adds a <see cref="Snapshot"/> as an immediate descendant of the current <see cref="ZfsRecord"/>, subscribes the
    ///     descendant to events published by the current <see cref="ZfsRecord"/>, and updates the corresponding latest snapshot period
    ///     timestamp on the current object if it is newer than the current value.
    /// </summary>
    /// <returns>
    ///     The same <see cref="Snapshot"/> reference that was supplied to this method in <paramref name="snap"/>.
    /// </returns>
    public Snapshot AddSnapshot ( Snapshot snap )
    {
        Logger.Trace ( "Adding snapshot {0} to {1} {2}", snap.Name, Kind, Name );
        SnapshotPeriodKind periodKind = snap.Period.Value.ToSnapshotPeriodKind ( );
        Snapshots [ periodKind ] [ snap.Name ] = snap;

        switch ( periodKind )
        {
            case SnapshotPeriodKind.Frequent:
                if ( LastObservedFrequentSnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedFrequentSnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( LastObservedHourlySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedHourlySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( LastObservedDailySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedDailySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( LastObservedWeeklySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedWeeklySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( LastObservedMonthlySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedMonthlySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( LastObservedYearlySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedYearlySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.NotSet:
            default:
                throw new InvalidOperationException ( "Invalid Snapshot Period specified" );
        }

        SubscribeSnapshotToPropertyEvents ( snap );

        return snap;
    }

    /// <summary>
    ///     Creates a new <see cref="Snapshot"/> from the supplied parameters, adds it as a descendant of the current
    ///     <see cref="ZfsRecord"/> via <see cref="AddSnapshot"/>, and returns a reference to the new <see cref="Snapshot"/> instance.
    /// </summary>
    /// <exception cref="ArgumentException">sourceSystem must have a non-null, non-whitespace-only Value</exception>
    public Snapshot CreateAndAddSnapshot (
        string                         snapName,
        in ZfsProperty<bool>           enabled,
        in ZfsProperty<bool>           takeSnapshots,
        in ZfsProperty<bool>           pruneSnapshots,
        in ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp,
        in ZfsProperty<string>         recursion,
        in ZfsProperty<string>         template,
        in ZfsProperty<int>            retentionFrequent,
        in ZfsProperty<int>            retentionHourly,
        in ZfsProperty<int>            retentionDaily,
        in ZfsProperty<int>            retentionWeekly,
        in ZfsProperty<int>            retentionMonthly,
        in ZfsProperty<int>            retentionYearly,
        in ZfsProperty<int>            retentionPruneDeferral,
        string                         periodString,
        in ZfsProperty<string>         sourceSystem,
        in DateTimeOffset              snapshotTimestamp,
        ZfsRecord                      dataset
    )
    {
        Logger.Trace ( "Creating and adding snapshot {0} to {1} {2}", snapName, Kind, Name );

        if ( string.IsNullOrWhiteSpace ( sourceSystem.Value ) )
        {
            throw new ArgumentException ( "sourceSystem must have a non-null, non-whitespace-only Value", nameof (sourceSystem) );
        }

        Snapshot snap = new (
                             snapName,
                             enabled,
                             takeSnapshots,
                             pruneSnapshots,
                             lastFrequentSnapshotTimestamp,
                             lastHourlySnapshotTimestamp,
                             lastDailySnapshotTimestamp,
                             lastWeeklySnapshotTimestamp,
                             lastMonthlySnapshotTimestamp,
                             lastYearlySnapshotTimestamp,
                             recursion,
                             template,
                             retentionFrequent,
                             retentionHourly,
                             retentionDaily,
                             retentionWeekly,
                             retentionMonthly,
                             retentionYearly,
                             retentionPruneDeferral,
                             sourceSystem,
                             periodString,
                             snapshotTimestamp,
                             dataset );
        AddSnapshot ( snap );

        return snap;
    }

    /// <summary>
    ///     Convenience method that creates a descendant <see cref="ZfsRecord"/> from the provided parameters and calls
    ///     <see cref="AddDataset"/> to add it to the current <see cref="ZfsRecord"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="sourceSystem"/> is <see langword="null"/>, empty, or only whitespace
    /// </exception>
    public ZfsRecord CreateChildDataset ( string name, string kind, string sourceSystem = "", bool inheritProperties = true, long bytesAvailable = -1, long bytesUsed = -1 )
    {
        if ( string.IsNullOrWhiteSpace ( sourceSystem ) )
        {
            throw new ArgumentNullException ( nameof (sourceSystem) );
        }

        bool propertiesLocal = !inheritProperties;

        ZfsRecord newDs = inheritProperties
                              ? new (
                                     name,
                                     kind,
                                     Enabled with { IsLocal = propertiesLocal },
                                     TakeSnapshots with { IsLocal = propertiesLocal },
                                     PruneSnapshots with { IsLocal = propertiesLocal },
                                     LastFrequentSnapshotTimestamp with { IsLocal = true, Value = DateTimeOffset.UnixEpoch },
                                     LastHourlySnapshotTimestamp with { IsLocal = true, Value = DateTimeOffset.UnixEpoch },
                                     LastDailySnapshotTimestamp with { IsLocal = true, Value = DateTimeOffset.UnixEpoch },
                                     LastWeeklySnapshotTimestamp with { IsLocal = true, Value = DateTimeOffset.UnixEpoch },
                                     LastMonthlySnapshotTimestamp with { IsLocal = true, Value = DateTimeOffset.UnixEpoch },
                                     LastYearlySnapshotTimestamp with { IsLocal = true, Value = DateTimeOffset.UnixEpoch },
                                     Recursion with { IsLocal = propertiesLocal },
                                     Template with { IsLocal = propertiesLocal },
                                     SnapshotRetentionFrequent with { IsLocal = propertiesLocal },
                                     SnapshotRetentionHourly with { IsLocal = propertiesLocal },
                                     SnapshotRetentionDaily with { IsLocal = propertiesLocal },
                                     SnapshotRetentionWeekly with { IsLocal = propertiesLocal },
                                     SnapshotRetentionMonthly with { IsLocal = propertiesLocal },
                                     SnapshotRetentionYearly with { IsLocal = propertiesLocal },
                                     SnapshotRetentionPruneDeferral with { IsLocal = propertiesLocal },
                                     SourceSystem with { IsLocal = true, Value = sourceSystem },
                                     bytesAvailable,
                                     bytesUsed,
                                     this,
                                     true )
                              : new ( name, kind, sourceSystem, false, this )
                                {
                                    BytesAvailable = bytesAvailable,
                                    BytesUsed      = bytesUsed
                                };

        AddDataset ( newDs );

        return newDs;
    }

    /// <summary>
    ///     Factory method that creates a new <see cref="ZfsRecord"/> via the protected constructor.
    /// </summary>
    /// <exception cref="ArgumentException">sourceSystem must have a non-null, non-whitespace-only Value</exception>
    public static ZfsRecord CreateInstanceFromAllProperties (
        string                         name,
        string                         kind,
        in ZfsProperty<bool>           enabled,
        in ZfsProperty<bool>           takeSnapshots,
        in ZfsProperty<bool>           pruneSnapshots,
        in ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp,
        in ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp,
        in ZfsProperty<string>         recursion,
        in ZfsProperty<string>         template,
        in ZfsProperty<int>            retentionFrequent,
        in ZfsProperty<int>            retentionHourly,
        in ZfsProperty<int>            retentionDaily,
        in ZfsProperty<int>            retentionWeekly,
        in ZfsProperty<int>            retentionMonthly,
        in ZfsProperty<int>            retentionYearly,
        in ZfsProperty<int>            retentionPruneDeferral,
        in ZfsProperty<string>         sourceSystem,
        in long                        bytesAvailable,
        in long                        bytesUsed,
        ZfsRecord?                     parent = null
    )
    {
        if ( string.IsNullOrWhiteSpace ( sourceSystem.Value ) )
        {
            throw new ArgumentException ( "sourceSystem must have a non-null, non-whitespace-only Value", nameof (sourceSystem) );
        }

        return new (
                    name,
                    kind,
                    in enabled,
                    in takeSnapshots,
                    in pruneSnapshots,
                    in lastFrequentSnapshotTimestamp,
                    in lastHourlySnapshotTimestamp,
                    in lastDailySnapshotTimestamp,
                    in lastWeeklySnapshotTimestamp,
                    in lastMonthlySnapshotTimestamp,
                    in lastYearlySnapshotTimestamp,
                    in recursion,
                    in template,
                    in retentionFrequent,
                    in retentionHourly,
                    in retentionDaily,
                    in retentionWeekly,
                    in retentionMonthly,
                    in retentionYearly,
                    in retentionPruneDeferral,
                    in sourceSystem,
                    in bytesAvailable,
                    in bytesUsed,
                    parent,
                    true );
    }

    /// <summary>
    ///     Creates a new <see cref="Snapshot"/> from the given <paramref name="period"/> and <paramref name="timestamp"/>,
    ///     using <paramref name="formattingSettings"/> to generate its name.
    /// </summary>
    /// <param name="period">The period for the new <see cref="Snapshot"/></param>
    /// <param name="timestamp">The timestamp for the new <see cref="Snapshot"/></param>
    /// <param name="formattingSettings"></param>
    /// <param name="sourceSystem"></param>
    /// <returns>
    ///     A reference to the created <see cref="Snapshot"/>
    /// </returns>
    public Snapshot CreateSnapshot ( in SnapshotPeriod period, in DateTimeOffset timestamp, in FormattingSettings formattingSettings, in ZfsProperty<string> sourceSystem )
    {
        Logger.Trace ( "Creating {0} snapshot for {1} {2}", period, Kind, Name );

        if ( string.IsNullOrWhiteSpace ( sourceSystem.Value ) )
        {
            throw new ArgumentException ( "sourceSystem must have a non-null, non-whitespace-only Value", nameof (sourceSystem) );
        }

        string   snapName = formattingSettings.GenerateFullSnapshotName ( Name, in period.Kind, in timestamp );
        Snapshot snap     = new ( snapName, in period.Kind, in sourceSystem, in timestamp, this );

        return snap;
    }

    /// <summary>
    ///     Performs a deep copy of this <see cref="ZfsRecord"/>
    /// </summary>
    /// <param name="parent">
    ///     A reference to the parent of the new <see cref="ZfsRecord"/> or <see langword="null"/>, if the record to be cloned should
    ///     be a pool root
    /// </param>
    /// <remarks>
    ///     The default copy constructor generated by the compiler performs a shallow copy and is unaware of the tree structure, which
    ///     would mean that the <see cref="Snapshots"/> and <see cref="_childDatasets"/>
    ///     collections would contain references to the original record<br/>
    ///     This method loops over both collections, calling <see cref="DeepCopyClone"/> for every element of
    ///     <see cref="_childDatasets"/> and <see cref="Snapshot.DeepCopyClone"/> for every <see cref="Snapshot"/> in
    ///     <see cref="Snapshots"/>.<br/>
    ///     THIS WILL RESULT IN A CLONE OF THE ENTIRE TREE FROM THIS NODE, INCLUDING ALL EVENT SUBSCRIPTIONS.
    /// </remarks>
    /// <returns>
    ///     A new instance of a <see cref="ZfsRecord"/>, with all properties, both reference and value, cloned to new instances
    /// </returns>
    public virtual ZfsRecord DeepCopyClone ( ZfsRecord? parent = null )
    {
        ZfsRecord newRecord = new (
                                   new ( Name ),
                                   new ( Kind ),
                                   Enabled,
                                   TakeSnapshots,
                                   PruneSnapshots,
                                   LastFrequentSnapshotTimestamp,
                                   LastHourlySnapshotTimestamp,
                                   LastDailySnapshotTimestamp,
                                   LastWeeklySnapshotTimestamp,
                                   LastMonthlySnapshotTimestamp,
                                   LastYearlySnapshotTimestamp,
                                   Recursion,
                                   Template,
                                   SnapshotRetentionFrequent,
                                   SnapshotRetentionHourly,
                                   SnapshotRetentionDaily,
                                   SnapshotRetentionWeekly,
                                   SnapshotRetentionMonthly,
                                   SnapshotRetentionYearly,
                                   SnapshotRetentionPruneDeferral,
                                   SourceSystem,
                                   BytesAvailable,
                                   BytesUsed,
                                   parent,
                                   true )
                              {
                                  LastObservedFrequentSnapshotTimestamp = LastObservedFrequentSnapshotTimestamp,
                                  LastObservedHourlySnapshotTimestamp   = LastObservedHourlySnapshotTimestamp,
                                  LastObservedDailySnapshotTimestamp    = LastObservedDailySnapshotTimestamp,
                                  LastObservedWeeklySnapshotTimestamp   = LastObservedWeeklySnapshotTimestamp,
                                  LastObservedMonthlySnapshotTimestamp  = LastObservedMonthlySnapshotTimestamp,
                                  LastObservedYearlySnapshotTimestamp   = LastObservedYearlySnapshotTimestamp
                              };

        foreach ( ( string _, ZfsRecord childDs ) in _childDatasets )
        {
            newRecord.AddDataset ( childDs.DeepCopyClone ( newRecord ) );
        }

        foreach ( ( SnapshotPeriodKind _, ConcurrentDictionary<string, Snapshot> periodCollection ) in Snapshots )
        {
            foreach ( ( string _, Snapshot sourceSnap ) in periodCollection )
            {
                newRecord.AddSnapshot ( sourceSnap.DeepCopyClone ( newRecord ) );
            }
        }

        return newRecord;
    }

    /// <summary>
    ///     Gets a child <see cref="ZfsRecord"/> from this <see cref="ZfsRecord"/>, by name, or <see langword="null"/>, if no such
    ///     child exists.
    /// </summary>
    /// <param name="childName">The fully-qualified name of the <see cref="ZfsRecord"/> to retrieve</param>
    /// <param name="child">
    ///     An <see langword="out"/> reference to the child <see cref="ZfsRecord"/>, if found
    /// </param>
    /// <returns>
    ///     If <paramref name="childName"/> is found, <see langword="true"/>; Otherwise, <see langword="false"/>
    /// </returns>
    public bool GetChild ( string childName, [NotNullWhen ( true )] out ZfsRecord? child ) => _childDatasets.TryGetValue ( childName, out child );

    /// <inheritdoc/>
    /// <remarks>
    ///     Uses name and kind only, since they are the only truly immutable properties relevant to this function
    /// </remarks>
    public override int GetHashCode ( )
    {
        HashCode hashCode = new ( );
        hashCode.Add ( Kind, StringComparer.CurrentCulture );
        hashCode.Add ( Name, StringComparer.CurrentCulture );

        return hashCode.ToHashCode ( );
    }

    public List<Snapshot> GetSnapshotsToPrune ( )
    {
        Logger.Debug ( "Getting list of snapshots to prune for dataset {0}", Name );

        if ( !Enabled.Value )
        {
            Logger.Debug ( "Dataset {0} is disabled. Skipping pruning", Name );

            return [ ];
        }

        if ( !_pruneSnapshotsField.Value )
        {
            Logger.Debug ( "Dataset {0} is not enabled for pruning", Name );

            return [ ];
        }

        Logger.Debug ( "Checking prune deferral setting for dataset {0}", Name );

        if ( SnapshotRetentionPruneDeferral.Value != 0 && PercentBytesUsed < SnapshotRetentionPruneDeferral.Value )
        {
            Logger.Info ( "Used capacity for {0} ({1}%) is below prune deferral threshold of {2}%. Skipping pruning of {0}", Name, PercentBytesUsed, SnapshotRetentionPruneDeferral.Value );

            return [ ];
        }

        if ( SnapshotRetentionPruneDeferral.Value == 0 )
        {
            Logger.Debug ( "Prune deferral not enabled for {0}", Name );
        }

        List<Snapshot> snapshotsToPrune = [ ];

        GetSnapshotsToPruneForPeriod ( SnapshotPeriod.Frequent, SnapshotRetentionFrequent.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod ( SnapshotPeriod.Hourly,   SnapshotRetentionHourly.Value,   snapshotsToPrune );
        GetSnapshotsToPruneForPeriod ( SnapshotPeriod.Daily,    SnapshotRetentionDaily.Value,    snapshotsToPrune );
        GetSnapshotsToPruneForPeriod ( SnapshotPeriod.Weekly,   SnapshotRetentionWeekly.Value,   snapshotsToPrune );
        GetSnapshotsToPruneForPeriod ( SnapshotPeriod.Monthly,  SnapshotRetentionMonthly.Value,  snapshotsToPrune );
        GetSnapshotsToPruneForPeriod ( SnapshotPeriod.Yearly,   SnapshotRetentionYearly.Value,   snapshotsToPrune );

        return snapshotsToPrune;
    }

    /// <summary>
    ///     Gets and caches a sorted dictionary of the child datasets of this <see cref="ZfsRecord"/>
    /// </summary>
    /// <returns></returns>
    public ImmutableSortedDictionary<string, ZfsRecord> GetSortedChildDatasets ( ) { return _sortedChildDatasets ??= _childDatasets.ToImmutableSortedDictionary ( ); }

    /// <summary>
    ///     Gets whether a daily snapshot is needed, according to the <paramref name="timestamp"/> and the properties defined
    ///     on the object
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset"/> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool"/> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define daily greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp"/> is either more than one day ahead of the last daily
    ///                 snapshot OR the last daily snapshot is not in the same day of the year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsDailySnapshotNeeded ( in DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no dailies
        if ( !SnapshotRetentionDaily.IsWanted ( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace ( "Checking if daily snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );

        if ( timestamp <= LastDailySnapshotTimestamp.Value )
        {
            return false;
        }

        TimeSpan timeSinceLastDailySnapshot          = timestamp - LastDailySnapshotTimestamp.Value;
        bool     atLeastOneDaySinceLastDailySnapshot = timeSinceLastDailySnapshot.TotalDays >= 1d;

        // Check if more than a day ago or if a different day of the year
        bool lastDailyOnDifferentDayOfYear = LastDailySnapshotTimestamp.Value.LocalDateTime.DayOfYear != timestamp.LocalDateTime.DayOfYear;
        bool dailySnapshotNeeded           = atLeastOneDaySinceLastDailySnapshot || lastDailyOnDifferentDayOfYear;
        Logger.Debug ( "Daily snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, dailySnapshotNeeded ? "" : "not " );

        return dailySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a frequent snapshot is needed, according to the provided <see cref="SnapshotTimingSettings"/> and
    ///     <paramref name="timestamp"/>
    /// </summary>
    /// <param name="template">
    ///     The <see cref="SnapshotTimingSettings"/> object to check status against.
    /// </param>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset"/> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool"/> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Template snapshot retention settings define frequent greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp"/> is either more than FrequentPeriod minutes ahead of the last frequent
    ///                 snapshot OR the last frequent snapshot is not in the same period of the hour
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsFrequentSnapshotNeeded ( SnapshotTimingSettings template, in DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no frequent
        if ( !SnapshotRetentionFrequent.IsWanted ( ) )
        {
            return false;
        }

        Logger.Trace ( "Checking if frequent snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );

        if ( timestamp < LastFrequentSnapshotTimestamp.Value )
        {
            return false;
        }

        int    currentFrequentPeriodOfHour      = template.GetPeriodOfHour ( timestamp );
        int    lastFrequentSnapshotPeriodOfHour = template.GetPeriodOfHour ( LastFrequentSnapshotTimestamp.Value );
        double minutesSinceLastFrequentSnapshot = ( timestamp - LastFrequentSnapshotTimestamp.Value ).TotalMinutes;

        // Check if more than FrequentPeriod ago or if the period of the hour is different.
        bool frequentSnapshotNeeded = minutesSinceLastFrequentSnapshot >= template.FrequentPeriod || lastFrequentSnapshotPeriodOfHour != currentFrequentPeriodOfHour;
        Logger.Debug ( "Frequent snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, frequentSnapshotNeeded ? "" : "not " );

        return frequentSnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether an hourly snapshot is needed, according to the provided <paramref name="timestamp"/>
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset"/> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool"/> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define hourly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp"/> is either more than one hour ahead of the last hourly
    ///                 snapshot OR the last hourly snapshot is not in the same hour
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsHourlySnapshotNeeded ( in DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no hourlies
        if ( !SnapshotRetentionHourly.IsWanted ( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace ( "Checking if hourly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );

        if ( timestamp < LastHourlySnapshotTimestamp.Value )
        {
            return false;
        }

        TimeSpan timeSinceLastHourlySnapshot           = timestamp - LastHourlySnapshotTimestamp.Value;
        bool     atLeastOneHourSinceLastHourlySnapshot = timeSinceLastHourlySnapshot.TotalHours >= 1d;

        // Check if more than an hour ago or if hour is different
        bool hourlySnapshotNeeded = atLeastOneHourSinceLastHourlySnapshot
                                 || LastHourlySnapshotTimestamp.Value.LocalDateTime.Hour != timestamp.LocalDateTime.Hour;
        Logger.Debug ( "Hourly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, hourlySnapshotNeeded ? "" : "not " );

        return hourlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a monthly snapshot is needed
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset"/> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool"/> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define monthly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp"/> is either in a different month than the last monthly snapshot OR the last
    ///                 monthly snapshot is in a different year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of months, using the executing user's culture.
    /// </remarks>
    public bool IsMonthlySnapshotNeeded ( in DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no monthlies
        if ( !SnapshotRetentionMonthly.IsWanted ( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace ( "Checking if monthly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );

        if ( timestamp < LastMonthlySnapshotTimestamp.Value )
        {
            return false;
        }

        int lastMonthlySnapshotMonth = CultureInfo.CurrentCulture.Calendar.GetMonth ( LastMonthlySnapshotTimestamp.Value.LocalDateTime );
        int currentMonth             = CultureInfo.CurrentCulture.Calendar.GetMonth ( timestamp.LocalDateTime );
        int lastMonthlySnapshotYear  = CultureInfo.CurrentCulture.Calendar.GetYear ( LastMonthlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear              = CultureInfo.CurrentCulture.Calendar.GetYear ( timestamp.LocalDateTime );

        // Check if the last monthly snapshot was in a different month or if same month but different year
        bool lastMonthlySnapshotInDifferentMonth = lastMonthlySnapshotMonth != currentMonth;
        bool lastMonthlySnapshotInDifferentYear  = currentYear              != lastMonthlySnapshotYear;
        bool monthlySnapshotNeeded               = lastMonthlySnapshotInDifferentMonth || lastMonthlySnapshotInDifferentYear;
        Logger.Debug ( "Monthly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, monthlySnapshotNeeded ? "" : "not " );

        return monthlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a weekly snapshot is needed, according to the provided <see cref="SnapshotTimingSettings"/> and
    ///     <paramref name="timestamp"/>
    /// </summary>
    /// <param name="template">
    ///     The <see cref="SnapshotTimingSettings"/> object to check status against.
    /// </param>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset"/> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool"/> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define weekly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp"/> is either more than 7 days ahead of the last weekly
    ///                 snapshot OR the last weekly snapshot is not in the same week of the year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of week numbers, using the executing user's culture, and treating the day of the
    ///     week specified in settings for weekly snapshots as the "first" day of the week, for week numbering purposes
    /// </remarks>
    public bool IsWeeklySnapshotNeeded ( SnapshotTimingSettings template, in DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no weeklies
        if ( !SnapshotRetentionWeekly.IsWanted ( ) )
        {
            Logger.Debug ( "Weekly snapshot not wanted for {0} {1}", Kind, Name );

            return false;
        }

        Logger.Trace ( "Checking if weekly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );

        if ( timestamp < LastWeeklySnapshotTimestamp.Value )
        {
            Logger.Debug ( "Weekly snapshot not needed for {0} {1}", Kind, Name );

            return false;
        }

        TimeSpan timeSinceLastWeeklySnapshot           = timestamp - LastWeeklySnapshotTimestamp.Value;
        bool     atLeastOneWeekSinceLastWeeklySnapshot = timeSinceLastWeeklySnapshot.TotalDays >= 7d;

        // Check if more than a week ago or if the week number is different by local rules, using the chosen day as the first day of the week
        int  lastWeeklySnapshotWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear ( LastWeeklySnapshotTimestamp.Value.LocalDateTime, CalendarWeekRule.FirstDay, template.WeeklyDay );
        int  currentWeekNumber            = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear ( timestamp.LocalDateTime,                         CalendarWeekRule.FirstDay, template.WeeklyDay );
        bool weeklySnapshotNeeded         = atLeastOneWeekSinceLastWeeklySnapshot || currentWeekNumber != lastWeeklySnapshotWeekNumber;
        Logger.Debug ( "Weekly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, weeklySnapshotNeeded ? "" : "not " );

        return weeklySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a yearly snapshot is needed, according to the <paramref name="timestamp"/> and properties defined on
    ///     the <see cref="ZfsRecord"/>
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset"/> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool"/> indicating whether the last yearly snapshot is in the same year as
    ///     <paramref name="timestamp"/>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of years, using the executing user's culture.
    /// </remarks>
    public bool IsYearlySnapshotNeeded ( in DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no monthlies
        if ( !SnapshotRetentionYearly.IsWanted ( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace ( "Checking if yearly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );

        if ( timestamp < LastYearlySnapshotTimestamp.Value )
        {
            return false;
        }

        int lastYearlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear ( LastYearlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear            = CultureInfo.CurrentCulture.Calendar.GetYear ( timestamp.LocalDateTime );

        // Check if the last yearly snapshot was in a different year
        bool yearlySnapshotNeeded = lastYearlySnapshotYear < currentYear;
        Logger.Debug ( "Yearly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, yearlySnapshotNeeded ? "" : "not " );

        return yearlySnapshotNeeded;
    }

    public bool RemoveSnapshot ( Snapshot snapshot )
    {
        Logger.Debug ( "Removing snapshot {0} from {1}", snapshot.Name, Name );
        UnsubscribeSnapshotFromPropertyEvents ( snapshot );

        return Snapshots [ snapshot.Period.Value.ToSnapshotPeriodKind ( ) ].TryRemove ( snapshot.Name, out _ );
    }

    /// <summary>
    ///     Validates a supplied <see cref="String"/> value against naming rules for ZFS objects supported by <see cref="ZfsRecord"/>.
    /// </summary>
    /// <remarks>
    ///     <paramref name="name"/> is validated as non-null, non-whitespace, no longer than 255 codepoints, and matching a
    ///     <see cref="Regex"/> according to the supplied <paramref name="kind"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     name must be a non-null, non-empty, non-whitespace string <paramref name="name"/>
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="name"/> is longer than 255 characters (ZFS limit)</exception>
    public static bool ValidateName ( string kind, string name, Regex? validatorRegex = null )
    {
        Logger.Debug ( "Validating name \"{0}\"", name );

        if ( string.IsNullOrWhiteSpace ( name ) )
        {
            throw new ArgumentNullException ( nameof (name), "name must be a non-null, non-empty, non-whitespace string" );
        }

        if ( name.Length > 255 )
        {
            throw new ArgumentOutOfRangeException ( nameof (name), "name must be 255 characters or less" );
        }

        // Sure they are... They're handled by the default case.
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        validatorRegex ??= kind switch
                           {
                               ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex ( ),
                               ZfsPropertyValueConstants.Volume     => ZfsIdentifierRegexes.DatasetNameRegex ( ),
                               ZfsPropertyValueConstants.Snapshot   => ZfsIdentifierRegexes.SnapshotNameRegex ( ),
                               _                                    => throw new ArgumentOutOfRangeException ( nameof (kind), "Unknown type of object specified to ValidateName." )
                           };

        MatchCollection matches = validatorRegex.Matches ( name );

        if ( matches.Count == 0 )
        {
            return false;
        }

        Logger.Trace ( "Checking regex matches for {0}", name );

        // No matter which kind was specified, the pool group should exist and be a match
        for ( int matchIndex = 0; matchIndex < matches.Count; matchIndex++ )
        {
            Match match = matches [ matchIndex ];
            Logger.Trace ( "Inspecting match {0}", match.Value );

            if ( match.Success )
            {
                Logger.Trace ( "Match successful" );

                continue;
            }

            Logger.Error ( "Name of {0} {1} is invalid", kind, name );

            return false;
        }

        Logger.Debug ( "Name of {0} {1} is valid", kind, name );

        return true;
    }

    protected internal bool ValidateName ( string name ) => ValidateName ( Kind, name, NameValidatorRegex );

    protected internal bool ValidateName ( ) => ValidateName ( Name );

    /// <summary>
    ///     Gets the collection of <see cref="Snapshot"/>s, groups by <see cref="SnapshotPeriodKind"/>
    /// </summary>
    /// <remarks>
    ///     Note that this is a reference type, as are the values of this collection.<br/>
    ///     Thus, when cloning a <see cref="ZfsRecord"/> using the <see langword="with"/> operator, this collection
    ///     needs to be re-created and all of its values deep-copied manually, if unique references are needed.
    /// </remarks>
    private static ConcurrentDictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>> GetNewSnapshotCollection ( ) =>
        new (
             new Dictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>>
             {
                 { SnapshotPeriodKind.Frequent, [ ] },
                 { SnapshotPeriodKind.Hourly, [ ] },
                 { SnapshotPeriodKind.Daily, [ ] },
                 { SnapshotPeriodKind.Weekly, [ ] },
                 { SnapshotPeriodKind.Monthly, [ ] },
                 { SnapshotPeriodKind.Yearly, [ ] }
             } );

    private void GetSnapshotsToPruneForPeriod ( SnapshotPeriod snapshotPeriod, int retentionValue, List<Snapshot> snapshotsToPrune )
    {
        List<Snapshot> snapshotsSetForPruning = Snapshots [ snapshotPeriod.Kind ].Where ( static kvp => kvp.Value.PruneSnapshots.Value ).Select ( static kvp => kvp.Value ).ToList ( );
        Logger.Trace ( "{0} snapshots of {1} configured for pruning: {2}", snapshotPeriod, Name, snapshotsSetForPruning.ToCommaSeparatedSingleLineString ( true ) );

        if ( snapshotsSetForPruning.Count <= retentionValue )
        {
            Logger.Trace ( "Number of pruning-enabled {0} snapshots for {1} ({2}) does not exceed retention setting ({3})", snapshotPeriod.ToString ( ), Name, snapshotsSetForPruning.Count.ToString ( ), retentionValue.ToString ( ) );

            return;
        }

        int numberToPrune = snapshotsSetForPruning.Count - retentionValue;
        Logger.Debug ( "Need to prune oldest {0} {1} snapshots from {2}", numberToPrune, snapshotPeriod, Name );
        snapshotsSetForPruning.Sort ( );

        for ( int i = 0; i < numberToPrune; i++ )
        {
            Snapshot snap = snapshotsSetForPruning [ i ];
            Logger.Trace ( "Adding snapshot {0} to prune list", snap.Name );
            snapshotsToPrune.Add ( snap );
        }
    }
}
