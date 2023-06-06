// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     Snapshot retention policy
/// </summary>
public sealed class SnapshotRetentionSettings
{
    public SnapshotRetentionSettings( )
    {
    }

    /// <summary>
    ///     Gets or sets how many daily snapshots will be retained
    /// </summary>
    public int Daily { get; set; }
    //{
    //    get
    //    {
    //        if ( !_owner.HasProperty( ZfsProperty.SnapshotRetentionDailyPropertyName ) )
    //            throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionDailyPropertyName} not defined for {_owner.Name}" );
    //        return int.Parse( _owner[ ZfsProperty.SnapshotRetentionDailyPropertyName ]?.Value ?? throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionDailyPropertyName} value invalid for {_owner.Name}" ) );
    //    }
    //}

    /// <summary>
    ///     Gets or sets how many frequent snapshots will be retained
    /// </summary>
    public int Frequent { get; set; }
    //{
    //    get
    //    {
    //        if ( !_owner.HasProperty( ZfsProperty.SnapshotRetentionFrequentPropertyName ) )
    //            throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionFrequentPropertyName} not defined for {_owner.Name}" );
    //        return int.Parse( _owner[ ZfsProperty.SnapshotRetentionFrequentPropertyName ]?.Value ?? throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionFrequentPropertyName} value invalid for {_owner.Name}" ) );
    //    }
    //}

    /// <summary>
    ///     Gets or sets how many hourly snapshots will be retained
    /// </summary>
    public int Hourly { get; set; }
    //{
    //    get
    //    {
    //        if ( !_owner.HasProperty( ZfsProperty.SnapshotRetentionHourlyPropertyName ) )
    //            throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionHourlyPropertyName} not defined for {_owner.Name}" );
    //        return int.Parse( _owner[ ZfsProperty.SnapshotRetentionHourlyPropertyName ]?.Value ?? throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionHourlyPropertyName} value invalid for {_owner.Name}" ) );
    //    }
    //}

    [JsonIgnore]
    public bool IsDailyWanted => Daily > 0;

    [JsonIgnore]
    public bool IsFrequentWanted => Frequent > 0;

    [JsonIgnore]
    public bool IsHourlyWanted => Hourly > 0;

    [JsonIgnore]
    public bool IsMonthlyWanted => Monthly > 0;

    [JsonIgnore]
    public bool IsWeeklyWanted => Weekly > 0;

    [JsonIgnore]
    public bool IsYearlyWanted => Yearly > 0;

    /// <summary>
    ///     Gets or sets how many monthly snapshots will be retained
    /// </summary>
    public int Monthly { get; set; }
    //{
    //    get
    //    {
    //        if ( !_owner.HasProperty( ZfsProperty.SnapshotRetentionMonthlyPropertyName ) )
    //            throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionMonthlyPropertyName} not defined for {_owner.Name}" );
    //        return int.Parse( _owner[ ZfsProperty.SnapshotRetentionMonthlyPropertyName ]?.Value ?? throw new InvalidOperationException( $"{ZfsProperty.SnapshotRetentionMonthlyPropertyName} value invalid for {_owner.Name}" ) );
    //    }
    //}


    /// <summary>
    ///     Gets or sets what percentage of remaining pool capacity must be reached before snapshots will be pruned by this
    ///     policy
    /// </summary>
    public int PruneDeferral { get; set; }

    /// <summary>
    ///     Gets or sets how many weekly snapshots will be retained
    /// </summary>
    public int Weekly { get; set; }

    /// <summary>
    ///     Gets or sets how many yearly snapshots will be retained
    /// </summary>
    public int Yearly { get; set; }
}
