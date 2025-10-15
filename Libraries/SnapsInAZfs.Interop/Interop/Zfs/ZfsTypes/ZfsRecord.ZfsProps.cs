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

using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1051

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public partial record ZfsRecord
{
    private ZfsProperty<bool>           _enabled;
    private ZfsProperty<DateTimeOffset> _lastDailySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastFrequentSnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastHourlySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastMonthlySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastWeeklySnapshotTimestamp;
    private ZfsProperty<DateTimeOffset> _lastYearlySnapshotTimestamp;
    private ZfsProperty<bool>           _pruneSnapshotsField;
    private ZfsProperty<string>         _recursion;
    private ZfsProperty<int>            _snapshotRetentionDaily;
    private ZfsProperty<int>            _snapshotRetentionFrequent;
    private ZfsProperty<int>            _snapshotRetentionHourly;
    private ZfsProperty<int>            _snapshotRetentionMonthly;
    private ZfsProperty<int>            _snapshotRetentionPruneDeferral;
    private ZfsProperty<int>            _snapshotRetentionWeekly;
    private ZfsProperty<int>            _snapshotRetentionYearly;
    private ZfsProperty<string>         _sourceSystem;
    private ZfsProperty<bool>           _takeSnapshots;
    private ZfsProperty<string>         _template;

    public ref readonly ZfsProperty<bool> Enabled => ref _enabled;

    public IZfsProperty this[ string propName ]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty ( propName );

            return propName switch
                   {
                       ZfsPropertyNames.Enabled                              => Enabled,
                       ZfsPropertyNames.TakeSnapshots                        => TakeSnapshots,
                       ZfsPropertyNames.PruneSnapshots                       => PruneSnapshots,
                       ZfsPropertyNames.Recursion                            => Recursion,
                       ZfsPropertyNames.Template                             => Template,
                       ZfsPropertyNames.SnapshotRetentionFrequent            => SnapshotRetentionFrequent,
                       ZfsPropertyNames.SnapshotRetentionHourly              => SnapshotRetentionHourly,
                       ZfsPropertyNames.SnapshotRetentionDaily               => SnapshotRetentionDaily,
                       ZfsPropertyNames.SnapshotRetentionWeekly              => SnapshotRetentionWeekly,
                       ZfsPropertyNames.SnapshotRetentionMonthly             => SnapshotRetentionMonthly,
                       ZfsPropertyNames.SnapshotRetentionYearly              => SnapshotRetentionYearly,
                       ZfsPropertyNames.SnapshotRetentionPruneDeferral       => SnapshotRetentionPruneDeferral,
                       ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp => LastFrequentSnapshotTimestamp,
                       ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp   => LastHourlySnapshotTimestamp,
                       ZfsPropertyNames.DatasetLastDailySnapshotTimestamp    => LastDailySnapshotTimestamp,
                       ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp   => LastWeeklySnapshotTimestamp,
                       ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp  => LastMonthlySnapshotTimestamp,
                       ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp   => LastYearlySnapshotTimestamp,
                       ZfsPropertyNames.SourceSystem                                     => SourceSystem,
                       _                                                                 => throw new ArgumentOutOfRangeException ( nameof (propName) )
                   };
        }
    }

    public ref readonly ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp     => ref _lastDailySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp  => ref _lastFrequentSnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp    => ref _lastHourlySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp   => ref _lastMonthlySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp    => ref _lastWeeklySnapshotTimestamp;
    public ref readonly ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp    => ref _lastYearlySnapshotTimestamp;
    public ref readonly ZfsProperty<bool>           PruneSnapshots                 => ref _pruneSnapshotsField;
    public ref readonly ZfsProperty<string>         Recursion                      => ref _recursion;
    public ref readonly ZfsProperty<int>            SnapshotRetentionDaily         => ref _snapshotRetentionDaily;
    public ref readonly ZfsProperty<int>            SnapshotRetentionFrequent      => ref _snapshotRetentionFrequent;
    public ref readonly ZfsProperty<int>            SnapshotRetentionHourly        => ref _snapshotRetentionHourly;
    public ref readonly ZfsProperty<int>            SnapshotRetentionMonthly       => ref _snapshotRetentionMonthly;
    public ref readonly ZfsProperty<int>            SnapshotRetentionPruneDeferral => ref _snapshotRetentionPruneDeferral;
    public ref readonly ZfsProperty<int>            SnapshotRetentionWeekly        => ref _snapshotRetentionWeekly;
    public ref readonly ZfsProperty<int>            SnapshotRetentionYearly        => ref _snapshotRetentionYearly;
    public ref readonly ZfsProperty<string>         SourceSystem                   => ref _sourceSystem;
    public ref readonly ZfsProperty<bool>           TakeSnapshots                  => ref _takeSnapshots;
    public ref readonly ZfsProperty<string>         Template                       => ref _template;

    /// <summary>
    ///     An <see langword="event" /> fired when any <see cref="ZfsProperty{T}" /> <see langword="bool" /> properties are updated on
    ///     this object
    /// </summary>
    public event BoolPropertyChangedEventHandler? BoolPropertyChanged;

    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="propertyName" /> is not a supported int property</exception>
    [SuppressMessage ( "ReSharper", "ConvertSwitchStatementToSwitchExpression", Justification = "Switch expressions cannot be ref return" )]
    public ref readonly ZfsProperty<int> GetIntProperty( string propertyName )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotRetentionFrequent:
                return ref _snapshotRetentionFrequent;
            case ZfsPropertyNames.SnapshotRetentionHourly:
                return ref _snapshotRetentionHourly;
            case ZfsPropertyNames.SnapshotRetentionDaily:
                return ref _snapshotRetentionDaily;
            case ZfsPropertyNames.SnapshotRetentionWeekly:
                return ref _snapshotRetentionWeekly;
            case ZfsPropertyNames.SnapshotRetentionMonthly:
                return ref _snapshotRetentionMonthly;
            case ZfsPropertyNames.SnapshotRetentionYearly:
                return ref _snapshotRetentionYearly;
            case ZfsPropertyNames.SnapshotRetentionPruneDeferral:
                return ref _snapshotRetentionPruneDeferral;
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), $"{propertyName} is not a supported int property" );
        }
    }

    /// <exception cref="InvalidOperationException">A pool root cannot inherit a property</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unrecognized property name was provided.</exception>
    public ref readonly ZfsProperty<bool> InheritBoolPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException ( "A pool root cannot inherit a property" );
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch ( propertyName )
        {
            case ZfsPropertyNames.Enabled:
                return ref UpdateProperty ( propertyName, ParentDataset.Enabled.Value, false );
            case ZfsPropertyNames.TakeSnapshots:
                return ref UpdateProperty ( propertyName, ParentDataset.TakeSnapshots.Value, false );
            case ZfsPropertyNames.PruneSnapshots:
                return ref UpdateProperty ( propertyName, ParentDataset.PruneSnapshots.Value, false );
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), "Invalid property specified" );
        }
    }

    /// <exception cref="InvalidOperationException">A pool root cannot inherit a property</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unrecognized property name was provided.</exception>
    public ref readonly ZfsProperty<int> InheritIntPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException ( "A pool root cannot inherit a property" );
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch ( propertyName )
        {
            case ZfsPropertyNames.SnapshotRetentionFrequent:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionFrequent.Value, false );
            case ZfsPropertyNames.SnapshotRetentionHourly:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionHourly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionDaily:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionDaily.Value, false );
            case ZfsPropertyNames.SnapshotRetentionWeekly:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionWeekly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionMonthly:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionMonthly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionYearly:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionYearly.Value, false );
            case ZfsPropertyNames.SnapshotRetentionPruneDeferral:
                return ref UpdateProperty ( propertyName, ParentDataset.SnapshotRetentionPruneDeferral.Value, false );
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), "Invalid property specified" );
        }
    }

    /// <exception cref="InvalidOperationException">A pool root cannot inherit a property</exception>
    /// <exception cref="ArgumentOutOfRangeException">An unrecognized property name was provided.</exception>
    /// <exception cref="Exception">A delegate callback throws an exception</exception>
    public ref readonly ZfsProperty<string> InheritStringPropertyFromParent( string propertyName )
    {
        if ( IsPoolRoot )
        {
            throw new InvalidOperationException ( "A pool root cannot inherit a property" );
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch ( propertyName )
        {
            case ZfsPropertyNames.Recursion:
                return ref UpdateProperty ( propertyName, ParentDataset.Recursion.Value, false );
            case ZfsPropertyNames.Template:
                return ref UpdateProperty ( propertyName, ParentDataset.Template.Value, false );
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), "Invalid property specified" );
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
            case ZfsPropertyNames.Enabled:
                boolProperty = _enabled;

                return true;
            case ZfsPropertyNames.TakeSnapshots:
                boolProperty = _takeSnapshots;

                return true;
            case ZfsPropertyNames.PruneSnapshots:
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
            case ZfsPropertyNames.Template:
                _template = _template with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke ( this, ref _template );

                return ref _template;
            case ZfsPropertyNames.Recursion:
                _recursion = _recursion with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke ( this, ref _recursion );

                return ref _recursion;
            case ZfsPropertyNames.SourceSystem:
                _sourceSystem = _sourceSystem with { Value = propertyValue, IsLocal = isLocal };
                StringPropertyChanged?.Invoke ( this, ref _sourceSystem );

                return ref _sourceSystem;
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), $"{propertyName} is not a supported property" );
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
    ///                 <see cref="ZfsPropertyNames.Enabled" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TakeSnapshots" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.PruneSnapshots" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public ref readonly ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.Enabled:
                _enabled = _enabled with { Value = propertyValue, IsLocal = isLocal };
                BoolPropertyChanged?.Invoke ( this, ref _enabled );

                return ref _enabled;
            case ZfsPropertyNames.TakeSnapshots:
                _takeSnapshots = _takeSnapshots with { Value = propertyValue, IsLocal = isLocal };
                BoolPropertyChanged?.Invoke ( this, ref _takeSnapshots );

                return ref _takeSnapshots;
            case ZfsPropertyNames.PruneSnapshots:
                _pruneSnapshotsField = _pruneSnapshotsField with { Value = propertyValue, IsLocal = isLocal };
                BoolPropertyChanged?.Invoke ( this, ref _pruneSnapshotsField );

                return ref _pruneSnapshotsField;
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), $"{propertyName} is not a supported boolean property" );
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
    ///                 <see cref="ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastDailySnapshotTimestamp" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public virtual ref readonly ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, in DateTimeOffset propertyValue, bool isLocal = true )
    {
        switch ( propertyName )
        {
            case ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp:
                _lastFrequentSnapshotTimestamp = LastFrequentSnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };

                return ref _lastFrequentSnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp:
                _lastHourlySnapshotTimestamp = LastHourlySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };

                return ref _lastHourlySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastDailySnapshotTimestamp:
                _lastDailySnapshotTimestamp = LastDailySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };

                return ref _lastDailySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp:
                _lastWeeklySnapshotTimestamp = LastWeeklySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };

                return ref _lastWeeklySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp:
                _lastMonthlySnapshotTimestamp = LastMonthlySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };

                return ref _lastMonthlySnapshotTimestamp;
            case ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp:
                _lastYearlySnapshotTimestamp = LastYearlySnapshotTimestamp with { Value = propertyValue, IsLocal = isLocal };

                return ref _lastYearlySnapshotTimestamp;
            default:
                throw new ArgumentOutOfRangeException ( nameof (propertyName), $"{propertyName} is not a supported DateTimeOffset property" );
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
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionFrequent" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionHourly" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionDaily" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionWeekly" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionMonthly" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionYearly" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionPruneDeferral" />
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
                case ZfsPropertyNames.SnapshotRetentionFrequent:
                    _snapshotRetentionFrequent = SnapshotRetentionFrequent with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionFrequent );

                    return ref _snapshotRetentionFrequent;
                case ZfsPropertyNames.SnapshotRetentionHourly:
                    _snapshotRetentionHourly = SnapshotRetentionHourly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionHourly );

                    return ref _snapshotRetentionHourly;
                case ZfsPropertyNames.SnapshotRetentionDaily:
                    _snapshotRetentionDaily = SnapshotRetentionDaily with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionDaily );

                    return ref _snapshotRetentionDaily;
                case ZfsPropertyNames.SnapshotRetentionWeekly:
                    _snapshotRetentionWeekly = SnapshotRetentionWeekly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionWeekly );

                    return ref _snapshotRetentionWeekly;
                case ZfsPropertyNames.SnapshotRetentionMonthly:
                    _snapshotRetentionMonthly = SnapshotRetentionMonthly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionMonthly );

                    return ref _snapshotRetentionMonthly;
                case ZfsPropertyNames.SnapshotRetentionYearly:
                    _snapshotRetentionYearly = SnapshotRetentionYearly with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionYearly );

                    return ref _snapshotRetentionYearly;
                case ZfsPropertyNames.SnapshotRetentionPruneDeferral:
                    _snapshotRetentionPruneDeferral = SnapshotRetentionPruneDeferral with { Value = propertyValue, IsLocal = isLocal };
                    IntPropertyChanged?.Invoke ( this, ref _snapshotRetentionPruneDeferral );

                    return ref _snapshotRetentionPruneDeferral;
                default:
                    throw new ArgumentOutOfRangeException ( nameof (propertyName), $"{propertyName} is not a supported int property" );
            }
        }
        catch ( ArgumentOutOfRangeException )
        {
            throw;
        }
        catch ( Exception ex )
        {
            Logger.Error ( ex, "Error updating {0} on {1} {2}", propertyName, Kind, Name );

            throw;
        }
    }

    protected virtual void OnParentUpdatedStringProperty( ZfsRecord sender, ref ZfsProperty<string> updatedProperty )
    {
        Logger.ConditionalTrace ( "{2} received string property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
             {
                 ZfsPropertyNames.Recursion => _recursion.IsInherited,
                 ZfsPropertyNames.Template  => _template.IsInherited,
                 _                                      => throw new ArgumentOutOfRangeException ( nameof (updatedProperty), "Unsupported property name {0} when updating string property", updatedProperty.Name )
             } )
        {
            UpdateProperty ( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    internal void UnsubscribeChildFromPropertyEvents( ZfsRecord child )
    {
        if ( !child.SubscribedToParentPropertyChangeEvents )
        {
            return;
        }

        IntPropertyChanged                           -= child.OnParentUpdatedIntProperty;
        BoolPropertyChanged                          -= child.OnParentUpdatedBoolProperty;
        StringPropertyChanged                        -= child.OnParentUpdatedStringProperty;
        child.SubscribedToParentPropertyChangeEvents =  false;
    }

    internal void UnsubscribeSnapshotFromPropertyEvents( Snapshot snap )
    {
        IntPropertyChanged    -= snap.OnParentUpdatedIntProperty;
        BoolPropertyChanged   -= snap.OnParentUpdatedBoolProperty;
        StringPropertyChanged -= snap.OnParentUpdatedStringProperty;
    }

    private void OnParentUpdatedBoolProperty( ZfsRecord sender, ref ZfsProperty<bool> updatedProperty )
    {
        Logger.ConditionalTrace ( "{2} received boolean property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
             {
                 ZfsPropertyNames.Enabled        => _enabled.IsInherited,
                 ZfsPropertyNames.TakeSnapshots  => _takeSnapshots.IsInherited,
                 ZfsPropertyNames.PruneSnapshots => _pruneSnapshotsField.IsInherited,
                 _                                           => throw new ArgumentOutOfRangeException ( nameof (updatedProperty), "Unsupported property name {0} when updating boolean property", updatedProperty.Name )
             } )
        {
            UpdateProperty ( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    private void OnParentUpdatedIntProperty( ZfsRecord sender, ref ZfsProperty<int> updatedProperty )
    {
        Logger.ConditionalTrace ( "{2} received int property change event for {0} from {1}", updatedProperty.Name, sender.Name, Name );
        if ( updatedProperty.Name switch
             {
                 ZfsPropertyNames.SnapshotRetentionFrequent      => _snapshotRetentionFrequent.IsInherited,
                 ZfsPropertyNames.SnapshotRetentionHourly        => _snapshotRetentionHourly.IsInherited,
                 ZfsPropertyNames.SnapshotRetentionDaily         => _snapshotRetentionDaily.IsInherited,
                 ZfsPropertyNames.SnapshotRetentionWeekly        => _snapshotRetentionWeekly.IsInherited,
                 ZfsPropertyNames.SnapshotRetentionMonthly       => _snapshotRetentionMonthly.IsInherited,
                 ZfsPropertyNames.SnapshotRetentionYearly        => _snapshotRetentionYearly.IsInherited,
                 ZfsPropertyNames.SnapshotRetentionPruneDeferral => _snapshotRetentionPruneDeferral.IsInherited,
                 _                                                           => throw new ArgumentOutOfRangeException ( nameof (updatedProperty), "Unsupported property name {0} when updating int property", updatedProperty.Name )
             } )
        {
            UpdateProperty ( updatedProperty.Name, updatedProperty.Value, false );
        }
    }

    private void SubscribeChildToPropertyEvents( ZfsRecord child )
    {
        if ( child.SubscribedToParentPropertyChangeEvents )
        {
            UnsubscribeChildFromPropertyEvents ( child );
        }

        IntPropertyChanged                           += child.OnParentUpdatedIntProperty;
        BoolPropertyChanged                          += child.OnParentUpdatedBoolProperty;
        StringPropertyChanged                        += child.OnParentUpdatedStringProperty;
        child.SubscribedToParentPropertyChangeEvents =  true;
    }

    private void SubscribeSnapshotToPropertyEvents( Snapshot snap )
    {
        BoolPropertyChanged   += snap.OnParentUpdatedBoolProperty;
        StringPropertyChanged += snap.OnParentUpdatedStringProperty;
    }

    public delegate void BoolPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<bool> property );

    public delegate void IntPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<int> property );

    public delegate void StringPropertyChangedEventHandler( ZfsRecord sender, ref ZfsProperty<string> property );
}
