// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

public record SanoidZfsDataset( string Name, string Kind, bool IsPoolRoot )
{
    public ZfsProperty<bool> Enabled { get; private set; } = new( ZfsPropertyNames.EnabledPropertyName, false, "local" );

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

    public ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<bool> PruneSnapshots { get; private set; } = new( ZfsPropertyNames.PruneSnapshotsPropertyName, false, "local" );
    public ZfsProperty<string> Recursion { get; private set; } = new( ZfsPropertyNames.RecursionPropertyName, "sanoid", "local" );
    public ZfsProperty<int> SnapshotRetentionDaily { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionFrequent { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionHourly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionMonthly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionPruneDeferral { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, "local" );
    public ZfsProperty<int> SnapshotRetentionWeekly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, "local" );
    public ZfsProperty<int> SnapshotRetentionYearly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, "local" );
    public ZfsProperty<bool> TakeSnapshots { get; private set; } = new( ZfsPropertyNames.TakeSnapshotsPropertyName, false, "local" );
    public ZfsProperty<string> Template { get; private set; } = new( ZfsPropertyNames.TemplatePropertyName, "default", "local" );

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
    public IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.EnabledPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.TakeSnapshotsPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.PruneSnapshotsPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.TemplatePropertyName => Template = Template with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.RecursionPropertyName => Recursion = Recursion with { Value = propertyValue, Source = propertySource },
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

    public ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.EnabledPropertyName => Enabled = Enabled with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.TakeSnapshotsPropertyName => TakeSnapshots = TakeSnapshots with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.PruneSnapshotsPropertyName => PruneSnapshots = PruneSnapshots with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" )
        };
    }

    public ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => LastFrequentSnapshotTimestamp = LastFrequentSnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => LastHourlySnapshotTimestamp = LastHourlySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => LastDailySnapshotTimestamp = LastDailySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => LastWeeklySnapshotTimestamp = LastWeeklySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => LastMonthlySnapshotTimestamp = LastMonthlySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => LastYearlySnapshotTimestamp = LastYearlySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported DateTimeOffset property" )
        };
    }

    public ZfsProperty<int> UpdateProperty( string propertyName, int propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => SnapshotRetentionFrequent = SnapshotRetentionFrequent with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => SnapshotRetentionHourly = SnapshotRetentionHourly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionDailyPropertyName => SnapshotRetentionDaily = SnapshotRetentionDaily with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => SnapshotRetentionWeekly = SnapshotRetentionWeekly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => SnapshotRetentionMonthly = SnapshotRetentionMonthly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => SnapshotRetentionYearly = SnapshotRetentionYearly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => SnapshotRetentionPruneDeferral = SnapshotRetentionPruneDeferral with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" )
        };
    }
}
