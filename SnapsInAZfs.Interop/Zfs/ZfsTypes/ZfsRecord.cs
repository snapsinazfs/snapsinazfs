// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public partial record ZfsRecord : IComparable<ZfsRecord>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public ZfsRecord( string Name, string Kind, bool inheritProperties = true, ZfsRecord? parent = null )
    {
        this.Name = Name;
        IsPoolRoot = parent is null;
        ParentDataset = parent ?? this;
        this.Kind = Kind;
        NameValidatorRegex = Kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new InvalidOperationException( "Unknown type of object specified for ZfsIdentifierValidator." )
        };

        if ( parent is not null && inheritProperties )
        {
            _enabled = parent.Enabled with { IsLocal = false, Owner = this };
            _lastDailySnapshotTimestamp = parent.LastDailySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastFrequentSnapshotTimestamp = parent.LastFrequentSnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastHourlySnapshotTimestamp = parent.LastHourlySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastMonthlySnapshotTimestamp = parent.LastMonthlySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastWeeklySnapshotTimestamp = parent.LastWeeklySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _lastYearlySnapshotTimestamp = parent.LastYearlySnapshotTimestamp with { Value = DateTimeOffset.UnixEpoch, IsLocal = true, Owner = this };
            _pruneSnapshotsField = parent.PruneSnapshots with { IsLocal = false, Owner = this };
            _recursion = parent.Recursion with { IsLocal = false, Owner = this };
            _snapshotRetentionDaily = parent.SnapshotRetentionDaily with { IsLocal = false, Owner = this };
            _snapshotRetentionFrequent = parent.SnapshotRetentionFrequent with { IsLocal = false, Owner = this };
            _snapshotRetentionHourly = parent.SnapshotRetentionHourly with { IsLocal = false, Owner = this };
            _snapshotRetentionMonthly = parent.SnapshotRetentionMonthly with { IsLocal = false, Owner = this };
            _snapshotRetentionPruneDeferral = parent.SnapshotRetentionPruneDeferral with { IsLocal = false, Owner = this };
            _snapshotRetentionWeekly = parent.SnapshotRetentionWeekly with { IsLocal = false, Owner = this };
            _snapshotRetentionYearly = parent.SnapshotRetentionYearly with { IsLocal = false, Owner = this };
            _takeSnapshots = parent.TakeSnapshots with { IsLocal = false, Owner = this };
            _template = parent.Template with { IsLocal = false, Owner = this };
        }
        else
        {
            _enabled = new( this, ZfsPropertyNames.EnabledPropertyName, false );
            _lastDailySnapshotTimestamp = new( this, ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastFrequentSnapshotTimestamp = new( this, ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastHourlySnapshotTimestamp = new( this, ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastMonthlySnapshotTimestamp = new( this, ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastWeeklySnapshotTimestamp = new( this, ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _lastYearlySnapshotTimestamp = new( this, ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch );
            _pruneSnapshotsField = new( this, ZfsPropertyNames.PruneSnapshotsPropertyName, false );
            _recursion = new( this, ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs );
            _snapshotRetentionDaily = new( this, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1 );
            _snapshotRetentionFrequent = new( this, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1 );
            _snapshotRetentionHourly = new( this, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1 );
            _snapshotRetentionMonthly = new( this, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1 );
            _snapshotRetentionPruneDeferral = new( this, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0 );
            _snapshotRetentionWeekly = new( this, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1 );
            _snapshotRetentionYearly = new( this, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1 );
            _takeSnapshots = new( this, ZfsPropertyNames.TakeSnapshotsPropertyName, false );
            _template = new( this, ZfsPropertyNames.TemplatePropertyName, ZfsPropertyValueConstants.Default );
        }
    }

    protected ZfsRecord( string name, string kind, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, long bytesAvailable, long bytesUsed, ZfsRecord? parent = null, bool forcePropertyOwnership = false )
    {
        Name = name;
        IsPoolRoot = parent is null;
        ParentDataset = parent ?? this;
        Kind = kind;
        NameValidatorRegex = kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new InvalidOperationException( "Unknown type of object specified for ZfsIdentifierValidator." )
        };

        if ( !forcePropertyOwnership )
        {
            _enabled = enabled;
            _takeSnapshots = takeSnapshots;
            _pruneSnapshotsField = pruneSnapshots;
            if ( lastFrequentSnapshotTimestamp.IsLocal )
            {
                _lastFrequentSnapshotTimestamp = lastFrequentSnapshotTimestamp;
            }

            if ( lastHourlySnapshotTimestamp.IsLocal )
            {
                _lastHourlySnapshotTimestamp = lastHourlySnapshotTimestamp;
            }

            if ( lastDailySnapshotTimestamp.IsLocal )
            {
                _lastDailySnapshotTimestamp = lastDailySnapshotTimestamp;
            }

            if ( lastWeeklySnapshotTimestamp.IsLocal )
            {
                _lastWeeklySnapshotTimestamp = lastWeeklySnapshotTimestamp;
            }

            if ( lastMonthlySnapshotTimestamp.IsLocal )
            {
                _lastMonthlySnapshotTimestamp = lastMonthlySnapshotTimestamp;
            }

            if ( lastYearlySnapshotTimestamp.IsLocal )
            {
                _lastYearlySnapshotTimestamp = lastYearlySnapshotTimestamp;
            }

            _recursion = recursion;
            _template = template;
            _snapshotRetentionFrequent = retentionFrequent;
            _snapshotRetentionHourly = retentionHourly;
            _snapshotRetentionDaily = retentionDaily;
            _snapshotRetentionWeekly = retentionWeekly;
            _snapshotRetentionMonthly = retentionMonthly;
            _snapshotRetentionYearly = retentionYearly;
            _snapshotRetentionPruneDeferral = retentionPruneDeferral;
            BytesAvailable = bytesAvailable;
            BytesUsed = bytesUsed;
        }
        else
        {
            _enabled = enabled with { Owner = this };
            _takeSnapshots = takeSnapshots with { Owner = this };
            _pruneSnapshotsField = pruneSnapshots with { Owner = this };
            if ( lastFrequentSnapshotTimestamp.IsLocal )
            {
                _lastFrequentSnapshotTimestamp = lastFrequentSnapshotTimestamp with { Owner = this };
            }
            else
            {
                _lastFrequentSnapshotTimestamp = lastFrequentSnapshotTimestamp;
            }

            if ( lastHourlySnapshotTimestamp.IsLocal )
            {
                _lastHourlySnapshotTimestamp = lastHourlySnapshotTimestamp with { Owner = this };
            }

            if ( lastDailySnapshotTimestamp.IsLocal )
            {
                _lastDailySnapshotTimestamp = lastDailySnapshotTimestamp with { Owner = this };
            }

            if ( lastWeeklySnapshotTimestamp.IsLocal )
            {
                _lastWeeklySnapshotTimestamp = lastWeeklySnapshotTimestamp with { Owner = this };
            }

            if ( lastMonthlySnapshotTimestamp.IsLocal )
            {
                _lastMonthlySnapshotTimestamp = lastMonthlySnapshotTimestamp with { Owner = this };
            }

            if ( lastYearlySnapshotTimestamp.IsLocal )
            {
                _lastYearlySnapshotTimestamp = lastYearlySnapshotTimestamp with { Owner = this };
            }

            _recursion = recursion with { Owner = this };
            _template = template with { Owner = this };
            _snapshotRetentionFrequent = retentionFrequent with { Owner = this };
            _snapshotRetentionHourly = retentionHourly with { Owner = this };
            _snapshotRetentionDaily = retentionDaily with { Owner = this };
            _snapshotRetentionWeekly = retentionWeekly with { Owner = this };
            _snapshotRetentionMonthly = retentionMonthly with { Owner = this };
            _snapshotRetentionYearly = retentionYearly with { Owner = this };
            _snapshotRetentionPruneDeferral = retentionPruneDeferral with { Owner = this };
            BytesAvailable = bytesAvailable;
            BytesUsed = bytesUsed;
        }
    }

    public long BytesAvailable { get; }
    public long BytesUsed { get; }

    public int ChildDatasetCount => _childDatasets.Count;
    public bool IsPoolRoot { get; }
    public string Kind { get; }
    public DateTimeOffset LastObservedDailySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedFrequentSnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedHourlySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedMonthlySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedWeeklySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public DateTimeOffset LastObservedYearlySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
    public string Name { get; }

    [JsonIgnore]
    public ZfsRecord ParentDataset { get; init; }

    [JsonIgnore]
    public ZfsRecord PoolRoot => IsPoolRoot ? this : ParentDataset.PoolRoot;

    public int SnapshotCount => Snapshots.Values.Sum( d => d.Count );

    public ConcurrentDictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>> Snapshots { get; } = GetNewSnapshotCollection( );

    [JsonIgnore]
    internal Regex NameValidatorRegex { get; }

    private SortedDictionary<string, ZfsRecord> _childDatasets { get; } = new( );

    [JsonIgnore]
    private long PercentBytesUsed => BytesUsed * 100 / BytesAvailable;

    /// <inheritdoc />
    public int CompareTo( ZfsRecord? other )
    {
        // If the other snapshot is null, consider this record earlier rank
        if ( other is null )
        {
            return -1;
        }

        return string.Compare( Name, other.Name, StringComparison.Ordinal );
    }

    /// <inheritdoc />
    /// <remarks>
    ///     The default generated record equality causes stack overflow due to the self-reference in root datasets,
    ///     so it is excluded from equality comparison.<br />
    ///     This equality method ONLY checks value properties and does not check the content of collections or references to other
    ///     <see cref="ZfsRecord" />s.<br />
    ///     It does, however, check <see cref="SnapshotCount" /> and <see cref="ChildDatasetCount" />.
    /// </remarks>
    public virtual bool Equals( ZfsRecord? other )
    {
        // Returning equality of value properties alphabetically
        return
            other is not null
            && BytesAvailable == other.BytesAvailable
            && BytesUsed == other.BytesUsed
            && Enabled == other.Enabled
            && IsPoolRoot == other.IsPoolRoot
            && Kind == other.Kind
            && LastDailySnapshotTimestamp == other.LastDailySnapshotTimestamp
            && LastFrequentSnapshotTimestamp == other.LastFrequentSnapshotTimestamp
            && LastHourlySnapshotTimestamp == other.LastHourlySnapshotTimestamp
            && LastMonthlySnapshotTimestamp == other.LastMonthlySnapshotTimestamp
            && LastObservedDailySnapshotTimestamp == other.LastObservedDailySnapshotTimestamp
            && LastObservedFrequentSnapshotTimestamp == other.LastObservedFrequentSnapshotTimestamp
            && LastObservedHourlySnapshotTimestamp == other.LastObservedHourlySnapshotTimestamp
            && LastObservedMonthlySnapshotTimestamp == other.LastObservedMonthlySnapshotTimestamp
            && LastObservedWeeklySnapshotTimestamp == other.LastObservedMonthlySnapshotTimestamp
            && LastObservedYearlySnapshotTimestamp == other.LastObservedYearlySnapshotTimestamp
            && LastWeeklySnapshotTimestamp == other.LastWeeklySnapshotTimestamp
            && LastYearlySnapshotTimestamp == other.LastYearlySnapshotTimestamp
            && Name == other.Name
            && PruneSnapshots == other.PruneSnapshots
            && Recursion == other.Recursion
            && SnapshotRetentionDaily == other.SnapshotRetentionDaily
            && SnapshotRetentionFrequent == other.SnapshotRetentionFrequent
            && SnapshotRetentionHourly == other.SnapshotRetentionHourly
            && SnapshotRetentionMonthly == other.SnapshotRetentionMonthly
            && SnapshotRetentionPruneDeferral == other.SnapshotRetentionPruneDeferral
            && SnapshotRetentionWeekly == other.SnapshotRetentionWeekly
            && SnapshotRetentionYearly == other.SnapshotRetentionYearly
            && TakeSnapshots == other.TakeSnapshots
            && Template == other.Template
            && SnapshotCount == other.SnapshotCount
            && ChildDatasetCount == other.ChildDatasetCount;
    }

    /// <exception cref="ArgumentException">
    ///     If the <see cref="ParentDataset" /> property of <paramref name="childDataset" /> does not reference this
    ///     <see cref="ZfsRecord" /> instance.
    /// </exception>
    public void AddDataset( ZfsRecord childDataset )
    {
        if ( !ReferenceEquals( childDataset.ParentDataset, this ) )
        {
            throw new ArgumentException( $"Cannot add child dataset to {Name}. {childDataset.Name} already references parent {childDataset.ParentDataset.Name}.", nameof( childDataset ) );
        }

        SubscribeChildToPropertyEvents( childDataset );
        _childDatasets[ childDataset.Name ] = childDataset;
    }

    public Snapshot AddSnapshot( Snapshot snap )
    {
        Logger.Trace( "Adding snapshot {0} to {1} {2}", snap.Name, Kind, Name );
        SnapshotPeriodKind periodKind = snap.Period.Value.ToSnapshotPeriod( ).Kind;
        Snapshots[ periodKind ][ snap.Name ] = snap;
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
                throw new InvalidOperationException( "Invalid Snapshot Period specified" );
        }

        SubscribeChildToPropertyEvents( snap );
        return snap;
    }

    public Snapshot CreateAndAddSnapshot( string snapName, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, string snapshotName, string periodString, DateTimeOffset snapshotTimestamp, ZfsRecord dataset )
    {
        Logger.Trace( "Creating and adding snapshot {0} to {1} {2}", snapName, Kind, Name );
        Snapshot snap = new( snapName,
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
                             snapshotName,
                             periodString,
                             snapshotTimestamp,
                             dataset );
        AddSnapshot( snap );
        return snap;
    }

    public ZfsRecord CreateChildDataset( string name, string kind, bool inheritProperties = true, long bytesAvailable = -1, long bytesUsed = -1 )
    {
        bool propertiesLocal = !inheritProperties;
        ZfsRecord newDs = inheritProperties
            ? new( name,
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
                   bytesAvailable,
                   bytesUsed,
                   this,
                   true )
            : new( name, kind, false, this );

        AddDataset( newDs );
        return newDs;
    }

    public static ZfsRecord CreateInstanceFromAllProperties( string name, string kind, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, long bytesAvailable, long bytesUsed, ZfsRecord? parent = null )
    {
        return new( name, kind, enabled, takeSnapshots, pruneSnapshots, lastFrequentSnapshotTimestamp, lastHourlySnapshotTimestamp, lastDailySnapshotTimestamp, lastWeeklySnapshotTimestamp, lastMonthlySnapshotTimestamp, lastYearlySnapshotTimestamp, recursion, template, retentionFrequent, retentionHourly, retentionDaily, retentionWeekly, retentionMonthly, retentionYearly, retentionPruneDeferral, bytesAvailable, bytesUsed, parent, true );
    }

    /// <summary>
    ///     Performs a deep copy of this <see cref="ZfsRecord" />
    /// </summary>
    /// <param name="parent">
    ///     A reference to the parent of the new <see cref="ZfsRecord" /> or <see langword="null" />, if the record to be cloned should
    ///     be a pool root
    /// </param>
    /// <remarks>
    ///     The default copy constructor generated by the compiler performs a shallow copy and is unaware of the tree structure, which would mean that the <see cref="Snapshots"/> and <see cref="_childDatasets"/>
    ///     collections would contain references to the original record<br />
    ///     This method loops over both collections, calling <see cref="DeepCopyClone"/> for every element of <see cref="_childDatasets"/> and <see cref="Snapshot.DeepCopyClone"/> for every <see cref="Snapshot"/> in <see cref="Snapshots"/>.<br />
    ///     THIS WILL RESULT IN A CLONE OF THE ENTIRE TREE FROM THIS NODE.
    /// </remarks>
    /// <returns>
    ///     A new instance of a <see cref="ZfsRecord" />, with all properties, both reference and value, cloned to new instances
    /// </returns>
    public virtual ZfsRecord DeepCopyClone( ZfsRecord? parent = null )
    {
        ZfsRecord newRecord = new( new( Name ),
                                   new( Kind ),
                                   Enabled with { },
                                   TakeSnapshots with { },
                                   PruneSnapshots with { },
                                   LastFrequentSnapshotTimestamp with { },
                                   LastHourlySnapshotTimestamp with { },
                                   LastDailySnapshotTimestamp with { },
                                   LastWeeklySnapshotTimestamp with { },
                                   LastMonthlySnapshotTimestamp with { },
                                   LastYearlySnapshotTimestamp with { },
                                   Recursion with { },
                                   Template with { },
                                   SnapshotRetentionFrequent with { },
                                   SnapshotRetentionHourly with { },
                                   SnapshotRetentionDaily with { },
                                   SnapshotRetentionWeekly with { },
                                   SnapshotRetentionMonthly with { },
                                   SnapshotRetentionYearly with { },
                                   SnapshotRetentionPruneDeferral with { },
                                   BytesAvailable,
                                   BytesUsed,
                                   parent,
                                   true )
        {
            LastObservedFrequentSnapshotTimestamp = LastObservedFrequentSnapshotTimestamp,
            LastObservedHourlySnapshotTimestamp = LastObservedHourlySnapshotTimestamp,
            LastObservedDailySnapshotTimestamp = LastObservedDailySnapshotTimestamp,
            LastObservedWeeklySnapshotTimestamp = LastObservedWeeklySnapshotTimestamp,
            LastObservedMonthlySnapshotTimestamp = LastObservedMonthlySnapshotTimestamp,
            LastObservedYearlySnapshotTimestamp = LastObservedYearlySnapshotTimestamp
        };

        foreach ( ( string _, ZfsRecord childDs ) in _childDatasets )
        {
            ZfsRecord clonedChild = childDs.DeepCopyClone( newRecord );
            newRecord.AddDataset( clonedChild );
        }

        foreach ( ( SnapshotPeriodKind period, ConcurrentDictionary<string, Snapshot> periodCollection ) in Snapshots )
        {
            foreach ( ( string _, Snapshot sourceSnap ) in periodCollection )
            {
                newRecord.AddSnapshot( sourceSnap.DeepCopyClone( newRecord ) );
            }
        }

        return newRecord;
    }

    public ImmutableSortedDictionary<string, ZfsRecord> GetChildDatasets( )
    {
        return _childDatasets.ToImmutableSortedDictionary( );
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Uses name and kind only, since they are the only truly immutable properties relevant to this function
    /// </remarks>
    public override int GetHashCode( )
    {
        HashCode hashCode = new( );
        hashCode.Add( Kind, StringComparer.CurrentCulture );
        hashCode.Add( Name, StringComparer.CurrentCulture );
        return hashCode.ToHashCode( );
    }

    /// <summary>
    ///     Gets the collection of <see cref="Snapshot" />s, groups by <see cref="SnapshotPeriodKind" />
    /// </summary>
    /// <remarks>
    ///     Note that this is a reference type, as are the values of this collection.<br />
    ///     Thus, when cloning a <see cref="ZfsRecord" /> using the <see langword="with" /> operator, this collection
    ///     needs to be re-created and all of its values deep-copied manually, if unique references are needed.
    /// </remarks>
    public static ConcurrentDictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>> GetNewSnapshotCollection( )
    {
        return new(
            new Dictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>>
            {
                { SnapshotPeriodKind.Frequent, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Hourly, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Daily, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Weekly, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Monthly, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Yearly, new ConcurrentDictionary<string, Snapshot>( ) }
            } );
    }

    public List<Snapshot> GetSnapshotsToPrune( )
    {
        Logger.Debug( "Getting list of snapshots to prune for dataset {0}", Name );
        if ( !Enabled.Value )
        {
            Logger.Debug( "Dataset {0} is disabled. Skipping pruning", Name );
            return new( );
        }

        Logger.Debug( "Checking prune deferral setting for dataset {0}", Name );
        if ( SnapshotRetentionPruneDeferral.Value != 0 && PercentBytesUsed < SnapshotRetentionPruneDeferral.Value )
        {
            Logger.Info( "Used capacity for {0} ({1}%) is below prune deferral threshold of {2}%. Skipping pruning of {0}", Name, PercentBytesUsed, SnapshotRetentionPruneDeferral.Value );
            return new( );
        }

        if ( SnapshotRetentionPruneDeferral.Value == 0 )
        {
            Logger.Debug( "Prune deferral not enabled for {0}", Name );
        }

        List<Snapshot> snapshotsToPrune = new( );

        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Frequent, SnapshotRetentionFrequent.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Hourly, SnapshotRetentionHourly.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Daily, SnapshotRetentionDaily.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Weekly, SnapshotRetentionWeekly.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Monthly, SnapshotRetentionMonthly.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Yearly, SnapshotRetentionYearly.Value, snapshotsToPrune );

        return snapshotsToPrune;
    }

    /// <summary>
    ///     Gets whether a daily snapshot is needed, according to the <paramref name="timestamp" /> and the properties defined
    ///     on the object
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define daily greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than one day ahead of the last daily
    ///                 snapshot OR the last daily snapshot is not in the same day of the year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsDailySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no dailies
        if ( !SnapshotRetentionDaily.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if daily snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        if ( timestamp <= LastDailySnapshotTimestamp.Value )
        {
            return false;
        }

        TimeSpan timeSinceLastDailySnapshot = timestamp - LastDailySnapshotTimestamp.Value;
        bool atLeastOneDaySinceLastDailySnapshot = timeSinceLastDailySnapshot.TotalDays >= 1d;
        // Check if more than a day ago or if a different day of the year
        bool lastDailyOnDifferentDayOfYear = LastDailySnapshotTimestamp.Value.LocalDateTime.DayOfYear != timestamp.LocalDateTime.DayOfYear;
        bool dailySnapshotNeeded = atLeastOneDaySinceLastDailySnapshot || lastDailyOnDifferentDayOfYear;
        Logger.Debug( "Daily snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, dailySnapshotNeeded ? "" : "not " );
        return dailySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a frequent snapshot is needed, according to the provided <see cref="SnapshotTimingSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="template">
    ///     The <see cref="SnapshotTimingSettings" /> object to check status against.
    /// </param>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Template snapshot retention settings define frequent greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than FrequentPeriod minutes ahead of the last frequent
    ///                 snapshot OR the last frequent snapshot is not in the same period of the hour
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsFrequentSnapshotNeeded( SnapshotTimingSettings template, DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no frequent
        if ( !SnapshotRetentionFrequent.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if frequent snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        if ( timestamp < LastFrequentSnapshotTimestamp.Value )
        {
            return false;
        }

        int currentFrequentPeriodOfHour = template.GetPeriodOfHour( timestamp );
        int lastFrequentSnapshotPeriodOfHour = template.GetPeriodOfHour( LastFrequentSnapshotTimestamp.Value );
        double minutesSinceLastFrequentSnapshot = ( timestamp - LastFrequentSnapshotTimestamp.Value ).TotalMinutes;
        // Check if more than FrequentPeriod ago or if the period of the hour is different.
        bool frequentSnapshotNeeded = minutesSinceLastFrequentSnapshot >= template.FrequentPeriod || lastFrequentSnapshotPeriodOfHour != currentFrequentPeriodOfHour;
        Logger.Debug( "Frequent snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, frequentSnapshotNeeded ? "" : "not " );
        return frequentSnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether an hourly snapshot is needed, according to the provided <paramref name="timestamp" />
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define hourly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than one hour ahead of the last hourly
    ///                 snapshot OR the last hourly snapshot is not in the same hour
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsHourlySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no hourlies
        if ( !SnapshotRetentionHourly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if hourly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        if ( timestamp < LastHourlySnapshotTimestamp.Value )
        {
            return false;
        }

        TimeSpan timeSinceLastHourlySnapshot = timestamp - LastHourlySnapshotTimestamp.Value;
        bool atLeastOneHourSinceLastHourlySnapshot = timeSinceLastHourlySnapshot.TotalHours >= 1d;
        // Check if more than an hour ago or if hour is different
        bool hourlySnapshotNeeded = atLeastOneHourSinceLastHourlySnapshot
                                    || LastHourlySnapshotTimestamp.Value.LocalDateTime.Hour != timestamp.LocalDateTime.Hour;
        Logger.Debug( "Hourly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, hourlySnapshotNeeded ? "" : "not " );
        return hourlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a monthly snapshot is needed
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define monthly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either in a different month than the last monthly snapshot OR the last
    ///                 monthly snapshot is in a different year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of months, using the executing user's culture.
    /// </remarks>
    public bool IsMonthlySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no monthlies
        if ( !SnapshotRetentionMonthly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if monthly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        if ( timestamp < LastMonthlySnapshotTimestamp.Value )
        {
            return false;
        }

        int lastMonthlySnapshotMonth = CultureInfo.CurrentCulture.Calendar.GetMonth( LastMonthlySnapshotTimestamp.Value.LocalDateTime );
        int currentMonth = CultureInfo.CurrentCulture.Calendar.GetMonth( timestamp.LocalDateTime );
        int lastMonthlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastMonthlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last monthly snapshot was in a different month or if same month but different year
        bool lastMonthlySnapshotInDifferentMonth = lastMonthlySnapshotMonth != currentMonth;
        bool lastMonthlySnapshotInDifferentYear = currentYear != lastMonthlySnapshotYear;
        bool monthlySnapshotNeeded = lastMonthlySnapshotInDifferentMonth || lastMonthlySnapshotInDifferentYear;
        Logger.Debug( "Monthly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, monthlySnapshotNeeded ? "" : "not " );
        return monthlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a weekly snapshot is needed, according to the provided <see cref="SnapshotTimingSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="template">
    ///     The <see cref="SnapshotTimingSettings" /> object to check status against.
    /// </param>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define weekly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than 7 days ahead of the last weekly
    ///                 snapshot OR the last weekly snapshot is not in the same week of the year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of week numbers, using the executing user's culture, and treating the day of the
    ///     week specified in settings for weekly snapshots as the "first" day of the week, for week numbering purposes
    /// </remarks>
    public bool IsWeeklySnapshotNeeded( SnapshotTimingSettings template, DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no weeklies
        if ( !SnapshotRetentionWeekly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if weekly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        if ( timestamp < LastWeeklySnapshotTimestamp.Value )
        {
            return false;
        }

        TimeSpan timeSinceLastWeeklySnapshot = timestamp - LastWeeklySnapshotTimestamp.Value;
        bool atLeastOneWeekSinceLastWeeklySnapshot = timeSinceLastWeeklySnapshot.TotalDays >= 7d;
        // Check if more than a week ago or if the week number is different by local rules, using the chosen day as the first day of the week
        int lastWeeklySnapshotWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear( LastWeeklySnapshotTimestamp.Value.LocalDateTime, CalendarWeekRule.FirstDay, template.WeeklyDay );
        int currentWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear( timestamp.LocalDateTime, CalendarWeekRule.FirstDay, template.WeeklyDay );
        bool weeklySnapshotNeeded = atLeastOneWeekSinceLastWeeklySnapshot || currentWeekNumber != lastWeeklySnapshotWeekNumber;
        Logger.Debug( "Weekly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, weeklySnapshotNeeded ? "" : "not " );
        return weeklySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a yearly snapshot is needed, according to the <paramref name="timestamp" /> and properties defined on
    ///     the <see cref="ZfsRecord" />
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether the last yearly snapshot is in the same year as
    ///     <paramref name="timestamp" />
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of years, using the executing user's culture.
    /// </remarks>
    public bool IsYearlySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no monthlies
        if ( !SnapshotRetentionYearly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if yearly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        if ( timestamp < LastYearlySnapshotTimestamp.Value )
        {
            return false;
        }

        int lastYearlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastYearlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last yearly snapshot was in a different year
        bool yearlySnapshotNeeded = lastYearlySnapshotYear < currentYear;
        Logger.Debug( "Yearly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, yearlySnapshotNeeded ? "" : "not " );
        return yearlySnapshotNeeded;
    }

    public static bool ValidateName( string kind, string name, Regex? validatorRegex = null )
    {
        Logger.Debug( "Validating name \"{0}\"", name );
        if ( string.IsNullOrWhiteSpace( name ) )
        {
            throw new ArgumentNullException( nameof( name ), "name must be a non-null, non-empty, non-whitespace string" );
        }

        if ( name.Length > 255 )
        {
            throw new ArgumentOutOfRangeException( nameof( name ), "name must be 255 characters or less" );
        }

        // Sure they are... They're handled by the default case.
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        validatorRegex ??= kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified to ValidateName." )
        };

        MatchCollection matches = validatorRegex.Matches( name );

        if ( matches.Count == 0 )
        {
            return false;
        }

        Logger.Trace( "Checking regex matches for {0}", name );
        // No matter which kind was specified, the pool group should exist and be a match
        for ( int matchIndex = 0; matchIndex < matches.Count; matchIndex++ )
        {
            Match match = matches[ matchIndex ];
            Logger.Trace( "Inspecting match {0}", match.Value );
            if ( match.Success )
            {
                continue;
            }

            Logger.Error( "Name of {0} {1} is invalid", kind, name );
            return false;
        }

        Logger.Debug( "Name of {0} {1} is valid", kind, name );

        return true;
    }

    protected internal bool ValidateName( string name )
    {
        return ValidateName( Kind, name, NameValidatorRegex );
    }

    protected internal bool ValidateName( )
    {
        return ValidateName( Name );
    }

    private void GetSnapshotsToPruneForPeriod( SnapshotPeriod snapshotPeriod, int retentionValue, List<Snapshot> snapshotsToPrune )
    {
        List<Snapshot> snapshotsSetForPruning = Snapshots[ snapshotPeriod.Kind ].Where( kvp => kvp.Value.PruneSnapshots.Value ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "{0} snapshots of {1} available for pruning: {2}", snapshotPeriod, Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        Logger.Trace( "{0} retention is {1:D} for {2} {3}", snapshotPeriod, retentionValue, Kind, Name );
        if ( snapshotsSetForPruning.Count > retentionValue )
        {
            int numberToPrune = snapshotsSetForPruning.Count - retentionValue;
            Logger.Debug( "Need to prune oldest {0} {1} snapshots from {2}", numberToPrune, snapshotPeriod, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot snap = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", snap.Name );
                snapshotsToPrune.Add( snap );
            }
        }
    }

    public bool RemoveSnapshot( Snapshot snapshot )
    {
        Logger.Debug( "Removing snapshot {0} from {1}", snapshot.Name, Name );
        UnsubscribeChildFromPropertyEvents( snapshot );
        return Snapshots[ snapshot.Period.Value.ToSnapshotPeriodKind( ) ].TryRemove( snapshot.Name, out _ );
    }
}
