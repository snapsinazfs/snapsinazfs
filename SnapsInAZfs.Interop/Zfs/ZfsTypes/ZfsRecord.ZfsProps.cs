// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public partial record ZfsRecord
{
    private ZfsProperty<bool> _enabled;
    private ZfsProperty<DateTimeOffset> _lastDailySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastFrequentSnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastHourlySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastMonthlySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastWeeklySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastYearlySnapshotTimestamp;
    private ZfsProperty<bool> _pruneSnapshots;
    private ZfsProperty<string> _recursion;
    private ZfsProperty<int> _snapshotRetentionDaily;
    private ZfsProperty<int> _snapshotRetentionFrequent;
    private ZfsProperty<int> _snapshotRetentionHourly;
    private ZfsProperty<int> _snapshotRetentionMonthly;
    private ZfsProperty<int> _snapshotRetentionPruneDeferral;
    private ZfsProperty<int> _snapshotRetentionWeekly;
    private ZfsProperty<int> _snapshotRetentionYearly;
    private ZfsProperty<bool> _takeSnapshots;
    private ZfsProperty<string> _template;

    public ref readonly ZfsProperty<bool> Enabled => ref _enabled;
    public ref readonly ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp => ref _lastDailySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp => ref _lastFrequentSnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp => ref _lastHourlySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp => ref _lastMonthlySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp => ref _lastWeeklySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp => ref _lastYearlySnapshotTimestamp;
    public ref readonly ZfsProperty<bool> PruneSnapshots => ref _pruneSnapshots;
    public ref readonly ZfsProperty<string> Recursion => ref _recursion;
    public ref readonly ZfsProperty<int> SnapshotRetentionDaily => ref _snapshotRetentionDaily;
    public ref readonly ZfsProperty<int> SnapshotRetentionFrequent => ref _snapshotRetentionFrequent;
    public ref readonly ZfsProperty<int> SnapshotRetentionHourly => ref _snapshotRetentionHourly;
    public ref readonly ZfsProperty<int> SnapshotRetentionMonthly => ref _snapshotRetentionMonthly;
    public ref readonly ZfsProperty<int> SnapshotRetentionPruneDeferral => ref _snapshotRetentionPruneDeferral;
    public ref readonly ZfsProperty<int> SnapshotRetentionWeekly => ref _snapshotRetentionWeekly;
    public ref readonly ZfsProperty<int> SnapshotRetentionYearly => ref _snapshotRetentionYearly;
    public ref readonly ZfsProperty<bool> TakeSnapshots => ref _takeSnapshots;
    public ref readonly ZfsProperty<string> Template => ref _template;

    /// <summary>
    ///     Updates a property for this <see cref="ZfsRecord" /> object and returns the new property boxed as an
    ///     <see cref="IZfsProperty" /> instance
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>
    ///     The new property created by this method, boxed as an <see cref="IZfsProperty" /> instance
    /// </returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="FormatException">
    ///     <paramref name="propertyValue" /> is not a valid string representation of the target
    ///     property value type.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="propertyValue" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     For <see cref="DateTimeOffset" /> properties, the offset is greater than 14 hours
    ///     or less than -14 hours.
    /// </exception>
    /// <exception cref="OverflowException">
    ///     For <see langword="int" /> properties, <paramref name="propertyValue" /> represents
    ///     a number less than <see cref="int.MinValue" /> or greater than <see cref="int.MaxValue" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.EnabledPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TakeSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.PruneSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.RecursionPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TemplatePropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionFrequentPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionHourlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionDailyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionYearlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public virtual IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        return propertyName switch
        {
            ZfsPropertyNames.EnabledPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.TakeSnapshotsPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.PruneSnapshotsPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.TemplatePropertyName => _template = _template with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.RecursionPropertyName => _recursion = _recursion with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionDailyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported property" )
        };
    }

    /// <summary>
    ///     Updates a <see cref="bool" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>The new property created by this method</returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.EnabledPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TakeSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.PruneSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public ref readonly ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
                _enabled = _enabled with { Value = propertyValue, Source = propertySource };
                return ref _enabled;
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
                _takeSnapshots = _takeSnapshots with { Value = propertyValue, Source = propertySource };
                return ref _takeSnapshots;
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
                _pruneSnapshots = _pruneSnapshots with { Value = propertyValue, Source = propertySource };
                return ref _pruneSnapshots;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" );
        }
    }

    /// <summary>
    ///     Updates a <see cref="DateTimeOffset" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>The new property created by this method</returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public virtual ref readonly ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName:
                _lastFrequentSnapshotTimestamp = LastFrequentSnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return ref _lastFrequentSnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName:
                _lastHourlySnapshotTimestamp = LastHourlySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return ref _lastHourlySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName:
                _lastDailySnapshotTimestamp = LastDailySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return ref _lastDailySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName:
                _lastWeeklySnapshotTimestamp = LastWeeklySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return ref _lastWeeklySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName:
                _lastMonthlySnapshotTimestamp = LastMonthlySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return ref _lastMonthlySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName:
                _lastYearlySnapshotTimestamp = LastYearlySnapshotTimestamp with { Value = propertyValue, Source = propertySource };
                return ref _lastYearlySnapshotTimestamp;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported DateTimeOffset property" );
        }
    }

    /// <summary>
    ///     Updates an <see cref="int" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>The new property created by this method</returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionFrequentPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionHourlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionDailyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionYearlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public ref readonly ZfsProperty<int> UpdateProperty( string propertyName, int propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
                _snapshotRetentionFrequent = SnapshotRetentionFrequent with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionFrequent;
            case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
                _snapshotRetentionHourly = SnapshotRetentionHourly with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionHourly;
            case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
                _snapshotRetentionDaily = SnapshotRetentionDaily with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionDaily;
            case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
                _snapshotRetentionWeekly = SnapshotRetentionWeekly with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionWeekly;
            case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
                _snapshotRetentionMonthly = SnapshotRetentionMonthly with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionMonthly;
            case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
                _snapshotRetentionYearly = SnapshotRetentionYearly with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionYearly;
            case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
                _snapshotRetentionPruneDeferral = SnapshotRetentionPruneDeferral with { Value = propertyValue, Source = propertySource };
                return ref _snapshotRetentionPruneDeferral;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" );
        }
    }

    public IZfsProperty this[ string propName ]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty( propName );
            return propName switch
            {
                ZfsPropertyNames.EnabledPropertyName => Enabled,
                ZfsPropertyNames.TakeSnapshotsPropertyName => TakeSnapshots,
                ZfsPropertyNames.PruneSnapshotsPropertyName => PruneSnapshots,
                ZfsPropertyNames.RecursionPropertyName => Recursion,
                ZfsPropertyNames.TemplatePropertyName => Template,
                ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => SnapshotRetentionFrequent,
                ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => SnapshotRetentionHourly,
                ZfsPropertyNames.SnapshotRetentionDailyPropertyName => SnapshotRetentionDaily,
                ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => SnapshotRetentionWeekly,
                ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => SnapshotRetentionMonthly,
                ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => SnapshotRetentionYearly,
                ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => SnapshotRetentionPruneDeferral,
                ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => LastFrequentSnapshotTimestamp,
                ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => LastHourlySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => LastDailySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => LastWeeklySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => LastMonthlySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => LastYearlySnapshotTimestamp,
                _ => throw new ArgumentOutOfRangeException( nameof( propName ) )
            };
        }
    }
}
