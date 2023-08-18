// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the �Software�), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1051

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
    private ZfsProperty<bool> _pruneSnapshotsField;
    private ZfsProperty<string> _recursion;
    private ZfsProperty<int> _snapshotRetentionDaily;
    private ZfsProperty<int> _snapshotRetentionFrequent;
    private ZfsProperty<int> _snapshotRetentionHourly;
    private ZfsProperty<int> _snapshotRetentionMonthly;
    private ZfsProperty<int> _snapshotRetentionPruneDeferral;
    private ZfsProperty<int> _snapshotRetentionWeekly;
    private ZfsProperty<int> _snapshotRetentionYearly;
    private ZfsProperty<string> _sourceSystem;
    private ZfsProperty<bool> _takeSnapshots;
    private ZfsProperty<string> _template;

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
                ZfsPropertyNames.SourceSystem => SourceSystem,
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
    public ref readonly ZfsProperty<string> SourceSystem => ref _sourceSystem;
    public ref readonly ZfsProperty<bool> TakeSnapshots => ref _takeSnapshots;
    public ref readonly ZfsProperty<string> Template => ref _template;

    /// <summary>
    ///     An <see langword="event" /> fired when any <see cref="ZfsProperty{T}" /> <see langword="bool" /> properties are updated on
    ///     this object
    /// </summary>
    public event BoolPropertyChangedEventHandler? BoolPropertyChanged;

    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="propertyName" /> is not a supported int property</exception>
    [SuppressMessage( "ReSharper", "ConvertSwitchStatementToSwitchExpression", Justification = "Switch expressions cannot be ref return" )]
    public ref readonly ZfsProperty<int> GetIntProperty( string propertyName )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
                return ref _snapshotRetentionFrequent;
            case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
                return ref _snapshotRetentionHourly;
            case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
                return ref _snapshotRetentionDaily;
            case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
                return ref _snapshotRetentionWeekly;
            case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
                return ref _snapshotRetentionMonthly;
            case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
                return ref _snapshotRetentionYearly;
            case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
                return ref _snapshotRetentionPruneDeferral;
            default:
                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" );
        }
    }

    /// <exception cref="InvalidOperationException">A pool root cannot inherit a property</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unrecognized property name was provided.</exception>
    public ref readonly ZfsProperty<bool> InheritBoolPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException( "A pool root cannot inherit a property" );
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
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
    public ref readonly ZfsProperty<int> InheritIntPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException( "A pool root cannot inherit a property" );
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionFrequent.Value, false );
            case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionHourly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionDaily.Value, false );
            case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionWeekly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionMonthly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionYearly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
                return ref UpdateProperty( propertyName, ParentDataset.SnapshotRetentionPruneDeferral.Value, false );
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

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
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

    public bool TryGetBoolProperty( string propertyName, out ZfsProperty<bool> boolProperty )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
                boolProperty = _enabled;
                return true;
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
                boolProperty = _takeSnapshots;
                return true;
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
                boolProperty = _pruneSnapshotsField;
                return true;
            default:
                boolProperty = ZfsProperty<bool>.DefaultProperty( );
                return false;
        }
    }

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
            case ZfsPropertyNames.SourceSystem:
                _sourceSystem = _sourceSystem with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke( this, ref _sourceSystem );
                return ref _sourceSystem;
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
    public virtual ref readonly ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, in DateTimeOffset propertyValue, bool isLocal = true )
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

    protected virtual void OnParentUpdatedStringProperty( ZfsRecord sender, ref ZfsProperty<string> updatedProperty )
    {
        Logger.Trace( "{2} received string property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
            {
                ZfsPropertyNames.RecursionPropertyName => _recursion.IsInherited,
                ZfsPropertyNames.TemplatePropertyName => _template.IsInherited,
                _ => throw new ArgumentOutOfRangeException( nameof( updatedProperty ), "Unsupported property name {0} when updating string property", updatedProperty.Name )
            } )
        {
            UpdateProperty( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    internal void UnsubscribeChildFromPropertyEvents( ZfsRecord child )
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

    internal void UnsubscribeSnapshotFromPropertyEvents( Snapshot snap )
    {
        IntPropertyChanged -= snap.OnParentUpdatedIntProperty;
        BoolPropertyChanged -= snap.OnParentUpdatedBoolProperty;
        StringPropertyChanged -= snap.OnParentUpdatedStringProperty;
    }

    private void OnParentUpdatedBoolProperty( ZfsRecord sender, ref ZfsProperty<bool> updatedProperty )
    {
        Logger.Trace( "{2} received boolean property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
            {
                ZfsPropertyNames.EnabledPropertyName => _enabled.IsInherited,
                ZfsPropertyNames.TakeSnapshotsPropertyName => _takeSnapshots.IsInherited,
                ZfsPropertyNames.PruneSnapshotsPropertyName => _pruneSnapshotsField.IsInherited,
                _ => throw new ArgumentOutOfRangeException( nameof( updatedProperty ), "Unsupported property name {0} when updating boolean property", updatedProperty.Name )
            } )
        {
            UpdateProperty( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    private void OnParentUpdatedIntProperty( ZfsRecord sender, ref ZfsProperty<int> updatedProperty )
    {
        Logger.Trace( "{2} received int property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
            {
                ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => _snapshotRetentionFrequent.IsInherited,
                ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => _snapshotRetentionHourly.IsInherited,
                ZfsPropertyNames.SnapshotRetentionDailyPropertyName => _snapshotRetentionDaily.IsInherited,
                ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => _snapshotRetentionWeekly.IsInherited,
                ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => _snapshotRetentionMonthly.IsInherited,
                ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => _snapshotRetentionYearly.IsInherited,
                ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => _snapshotRetentionPruneDeferral.IsInherited,
                _ => throw new ArgumentOutOfRangeException( nameof( updatedProperty ), "Unsupported property name {0} when updating int property", updatedProperty.Name )
            } )
        {
            UpdateProperty( updatedProperty.Name, updatedProperty.Value, false );
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

    public delegate void BoolPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<bool> property );

    public delegate void IntPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<int> property );

    public delegate void StringPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<string> property );
}
