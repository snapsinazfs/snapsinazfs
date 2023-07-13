// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public partial record ZfsRecord
{
    // ReSharper disable once InconsistentNaming
    protected ZfsProperty<bool> _enabled;

    // ReSharper disable once InconsistentNaming
    protected ZfsProperty<bool> _pruneSnapshotsField;

    // ReSharper disable once InconsistentNaming
    protected ZfsProperty<string> _recursion;

    // ReSharper disable once InconsistentNaming
    protected ZfsProperty<string> _template;
    private ZfsProperty<DateTimeOffset> _lastDailySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastFrequentSnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastHourlySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastMonthlySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastWeeklySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastYearlySnapshotTimestamp;
    private ZfsProperty<int> _snapshotRetentionDaily;
    private ZfsProperty<int> _snapshotRetentionFrequent;
    private ZfsProperty<int> _snapshotRetentionHourly;
    private ZfsProperty<int> _snapshotRetentionMonthly;
    private ZfsProperty<int> _snapshotRetentionPruneDeferral;
    private ZfsProperty<int> _snapshotRetentionWeekly;
    private ZfsProperty<int> _snapshotRetentionYearly;
    private ZfsProperty<bool> _takeSnapshots;

    public ref readonly ZfsProperty<bool> Enabled => ref _enabled;

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

    public ref readonly ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp => ref _lastDailySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp => ref _lastFrequentSnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp => ref _lastHourlySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp => ref _lastMonthlySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp => ref _lastWeeklySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp => ref _lastYearlySnapshotTimestamp;
    public ref readonly ZfsProperty<bool> PruneSnapshots => ref _pruneSnapshotsField;
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
    ///     An <see langword="event" /> fired when any <see cref="ZfsProperty{T}" /> <see langword="bool" /> properties are updated on
    ///     this object
    /// </summary>
    public event BoolPropertyChangedEventHandler? BoolPropertyChanged;

    /// <exception cref="InvalidOperationException">A pool root cannot inherit a property</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unrecognized property name was provided.</exception>
    public ref readonly ZfsProperty<bool> InheritBoolPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException( "A pool root cannot inherit a property" );
        }

        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.Enabled.Value, false );
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.TakeSnapshots.Value, false );
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.PruneSnapshots.Value, false );
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), "Invalid property specified" );
        }
    }

    /// <exception cref="InvalidOperationException">A pool root cannot inherit a property</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unrecognized property name was provided.</exception>
    /// <exception cref="Exception">A delegate callback throws an exception</exception>
    public ref readonly ZfsProperty<string> InheritStringPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException( "A pool root cannot inherit a property" );
        }

        switch ( propertyName )
        {
            case ZfsPropertyNames.RecursionPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.Recursion.Value, false );
            case ZfsPropertyNames.TemplatePropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.Template.Value, false );
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), "Invalid property specified" );
        }
    }

    /// <summary>
    ///     An <see langword="event" /> fired when any <see cref="ZfsProperty{T}" /> <see langword="int" /> properties are updated on
    ///     this object
    /// </summary>
    public event IntPropertyChangedEventHandler? IntPropertyChanged;

    /// <summary>
    ///     An <see langword="event" /> fired when any <see cref="ZfsProperty{T}" /> <see langword="string" /> properties are updated on
    ///     this object
    /// </summary>
    public event StringPropertyChangedEventHandler? StringPropertyChanged;

    /// <exception cref="Exception">A delegate callback throws an exception</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unsupported <paramref name="propertyName" /> was supplied</exception>
    public virtual ref readonly ZfsProperty<string> UpdateProperty( string propertyName, string propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.TemplatePropertyName:
                _template = _template with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke( this, ref _template );
                return ref _template;
            case ZfsPropertyNames.RecursionPropertyName:
                _recursion = _recursion with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke( this, ref _recursion );
                return ref _recursion;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported property" );
        }
    }

    /// <summary>
    ///     Updates a <see cref="bool" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="isLocal">
    ///     Whether this property is defined locally on this <see cref="ZfsRecord" /> or not. Default: <see langword="true" />
    /// </param>
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
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public ref readonly ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
                _enabled = _enabled with { Value = propertyValue, IsLocal = isLocal };
                BoolPropertyChanged?.Invoke( this, ref _enabled );
                return ref _enabled;
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
                _takeSnapshots = _takeSnapshots with { Value = propertyValue, IsLocal = isLocal };
                BoolPropertyChanged?.Invoke( this, ref _takeSnapshots );
                return ref _takeSnapshots;
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
                _pruneSnapshotsField = _pruneSnapshotsField with { Value = propertyValue, IsLocal = isLocal };
                BoolPropertyChanged?.Invoke( this, ref _pruneSnapshotsField );
                return ref _pruneSnapshotsField;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" );
        }
    }

    /// <summary>
    ///     Updates a <see cref="DateTimeOffset" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="isLocal">
    ///     Whether this property is defined locally on this <see cref="ZfsRecord" /> or not. Default: <see langword="true" />
    /// </param>
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
    public virtual ref readonly ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName:
                _lastFrequentSnapshotTimestamp = LastFrequentSnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };
                return ref _lastFrequentSnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName:
                _lastHourlySnapshotTimestamp = LastHourlySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };
                return ref _lastHourlySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName:
                _lastDailySnapshotTimestamp = LastDailySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };
                return ref _lastDailySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName:
                _lastWeeklySnapshotTimestamp = LastWeeklySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };
                return ref _lastWeeklySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName:
                _lastMonthlySnapshotTimestamp = LastMonthlySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };
                return ref _lastMonthlySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName:
                _lastYearlySnapshotTimestamp = LastYearlySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };
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
    /// <param name="isLocal">
    ///     Whether this property is defined locally on this <see cref="ZfsRecord" /> or not. Default: <see langword="true" />
    /// </param>
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
    public ref readonly ZfsProperty<int> UpdateProperty( string propertyName, int propertyValue, bool isLocal = true )
    {
        try
        {
            switch ( propertyName )
            {
                case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
                    _snapshotRetentionFrequent = SnapshotRetentionFrequent with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionFrequent );
                    return ref _snapshotRetentionFrequent;
                case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
                    _snapshotRetentionHourly = SnapshotRetentionHourly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionHourly );
                    return ref _snapshotRetentionHourly;
                case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
                    _snapshotRetentionDaily = SnapshotRetentionDaily with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionDaily );
                    return ref _snapshotRetentionDaily;
                case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
                    _snapshotRetentionWeekly = SnapshotRetentionWeekly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionWeekly );
                    return ref _snapshotRetentionWeekly;
                case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
                    _snapshotRetentionMonthly = SnapshotRetentionMonthly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionMonthly );
                    return ref _snapshotRetentionMonthly;
                case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
                    _snapshotRetentionYearly = SnapshotRetentionYearly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionYearly );
                    return ref _snapshotRetentionYearly;
                case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
                    _snapshotRetentionPruneDeferral = SnapshotRetentionPruneDeferral with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke( this, ref _snapshotRetentionPruneDeferral );
                    return ref _snapshotRetentionPruneDeferral;
                default:
                    throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" );
            }
        }
        catch ( ArgumentOutOfRangeException )
        {
            throw;
        }
        catch ( Exception ex )
        {
            Logger.Error( ex, "Error updating {0} on {1} {2}", propertyName, Kind, Name );
            throw;
        }
    }

    private void OnParentUpdatedBoolProperty( ZfsRecord sender, ref ZfsProperty<bool> updatedProperty )
    {
        Logger.Trace( "{2} received boolean property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
            {
                ZfsPropertyNames.EnabledPropertyName => _enabled.IsInherited,
                ZfsPropertyNames.TakeSnapshotsPropertyName => _takeSnapshots.IsInherited,
                ZfsPropertyNames.PruneSnapshotsPropertyName => _pruneSnapshotsField.IsInherited,
                _ => throw new ArgumentOutOfRangeException( nameof( updatedProperty ), "Unsupported property name when updating boolean property" )
            } )
        {
            UpdateProperty( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    protected virtual void OnParentUpdatedStringProperty( ZfsRecord sender, ref ZfsProperty<string> updatedProperty )
    {
        Logger.Trace( "{2} received boolean property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( this[ updatedProperty.Name ].IsInherited )
        {
            UpdateProperty( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    private void OnParentUpdatedIntProperty( ZfsRecord sender, ref ZfsProperty<int> property )
    {
        Logger.Trace( "{2} received int property change event for {0} from {1}", property.Name, sender.Name, Name );
        if ( this[ property.Name ].IsInherited )
        {
            UpdateProperty( property.Name, property.Value, false );
        }
    }

    private void SubscribeChildToPropertyEvents( ZfsRecord child )
    {
        if ( child.SubscribedToParentPropertyChangeEvents )
        {
            UnsubscribeChildFromPropertyEvents( child );
        }

        IntPropertyChanged += child.OnParentUpdatedIntProperty;
        BoolPropertyChanged += child.OnParentUpdatedBoolProperty;
        StringPropertyChanged += child.OnParentUpdatedStringProperty;
        child.SubscribedToParentPropertyChangeEvents = true;
    }

    private void SubscribeSnapshotToPropertyEvents( Snapshot snap )
    {
        BoolPropertyChanged += snap.OnParentUpdatedBoolProperty;
        StringPropertyChanged += snap.OnParentUpdatedStringProperty;
    }

    private void UnsubscribeChildFromPropertyEvents( ZfsRecord child )
    {
        if ( !child.SubscribedToParentPropertyChangeEvents )
        {
            return;
        }

        IntPropertyChanged -= child.OnParentUpdatedIntProperty;
        BoolPropertyChanged -= child.OnParentUpdatedBoolProperty;
        StringPropertyChanged -= child.OnParentUpdatedStringProperty;
        child.SubscribedToParentPropertyChangeEvents = false;
    }

    private void UnsubscribeSnapshotFromPropertyEvents( Snapshot snap )
    {
        IntPropertyChanged -= snap.OnParentUpdatedIntProperty;
        BoolPropertyChanged -= snap.OnParentUpdatedBoolProperty;
        StringPropertyChanged -= snap.OnParentUpdatedStringProperty;
    }

    public delegate void BoolPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<bool> property );

    public delegate void IntPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<int> property );

    public delegate void StringPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<string> property );
}
