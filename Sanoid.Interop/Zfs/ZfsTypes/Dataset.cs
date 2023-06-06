// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;
using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsTypes;

/// <summary>
///     A ZFS Dataset object. Can be a filesystem or volume.
/// </summary>
public class Dataset : ZfsObjectBase
{
    /// <summary>
    ///     Creates a new <see cref="Dataset" /> with the specified name and kind, optionally performing name validation
    /// </summary>
    /// <param name="name">The name of the new <see cref="Dataset" /></param>
    /// <param name="kind">The <see cref="DatasetKind" /> of Dataset to create</param>
    /// <param name="poolRoot">
    ///     The root dataset for this dataset. Null for roots.
    /// </param>
    /// <param name="isKnownPoolRoot">
    ///     Short-circuit if this dataset is known to be a pool root at instantiation, to avoid
    ///     string lookup
    /// </param>
    /// <param name="validateName">
    ///     Whether to validate the name of the new <see cref="Dataset" /> (<see langword="true" />) or
    ///     not (<see langword="false" /> - default)
    /// </param>
    /// <param name="validatorRegex">The <see cref="Regex" /> to user for name validation</param>
    public Dataset( string name, DatasetKind kind, Dataset? poolRoot = null, bool isKnownPoolRoot = false, bool validateName = false, Regex? validatorRegex = null )
        : base( name, (ZfsObjectKind)kind, poolRoot, isKnownPoolRoot, validateName, validatorRegex )
    {
    }

    public ConcurrentDictionary<string, Snapshot> AllSnapshots { get; } = new( );
    public List<Snapshot> DailySnapshots { get; } = new( );

    [JsonIgnore]
    public bool Enabled
    {
        get
        {
            string valueString = Properties.TryGetValue( ZfsProperty.EnabledPropertyName, out ZfsProperty? prop ) ? prop.Value : "false";
            return bool.TryParse( valueString, out bool result ) && result;
        }
    }

    public List<Snapshot> FrequentSnapshots { get; } = new( );
    public List<Snapshot> HourlySnapshots { get; } = new( );

    [JsonIgnore]
    public DateTimeOffset LastDailySnapshotTimestamp => Properties.TryGetValue( ZfsProperty.DatasetLastDailySnapshotTimestampPropertyName, out ZfsProperty? prop ) && DateTimeOffset.TryParse( prop.Value, out DateTimeOffset timestamp ) ? timestamp : DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastFrequentSnapshotTimestamp => Properties.TryGetValue( ZfsProperty.DatasetLastFrequentSnapshotTimestampPropertyName, out ZfsProperty? prop ) && DateTimeOffset.TryParse( prop.Value, out DateTimeOffset timestamp ) ? timestamp : DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastHourlySnapshotTimestamp => Properties.TryGetValue( ZfsProperty.DatasetLastHourlySnapshotTimestampPropertyName, out ZfsProperty? prop ) && DateTimeOffset.TryParse( prop.Value, out DateTimeOffset timestamp ) ? timestamp : DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastMonthlySnapshotTimestamp => Properties.TryGetValue( ZfsProperty.DatasetLastMonthlySnapshotTimestampPropertyName, out ZfsProperty? prop ) && DateTimeOffset.TryParse( prop.Value, out DateTimeOffset timestamp ) ? timestamp : DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastWeeklySnapshotTimestamp => Properties.TryGetValue( ZfsProperty.DatasetLastWeeklySnapshotTimestampPropertyName, out ZfsProperty? prop ) && DateTimeOffset.TryParse( prop.Value, out DateTimeOffset timestamp ) ? timestamp : DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastYearlySnapshotTimestamp => Properties.TryGetValue( ZfsProperty.DatasetLastYearlySnapshotTimestampPropertyName, out ZfsProperty? prop ) && DateTimeOffset.TryParse( prop.Value, out DateTimeOffset timestamp ) ? timestamp : DateTimeOffset.UnixEpoch;

    public List<Snapshot> MonthlySnapshots { get; } = new( );

    [JsonIgnore]
    public string Recursion
    {
        get
        {
            string valueString = Properties.TryGetValue( ZfsProperty.RecursionPropertyName, out ZfsProperty? prop ) ? prop.Value : SnapshotRecursionMode.Sanoid;
            return valueString;
        }
    }

    [JsonIgnore]
    public bool TakeSnapshots
    {
        get
        {
            string valueString = Properties.TryGetValue( ZfsProperty.TakeSnapshotsPropertyName, out ZfsProperty? prop ) ? prop.Value : "false";
            return bool.TryParse( valueString, out bool result ) && result;
        }
    }

    [JsonIgnore]
    public string Template => Properties.TryGetValue( ZfsProperty.TemplatePropertyName, out ZfsProperty? prop ) ? prop.Value : "default";

    public List<Snapshot> WeeklySnapshots { get; } = new( );
    public List<Snapshot> YearlySnapshots { get; } = new( );

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public (bool MissingAny, List<string>? MissingPropertyNames) GetMissingMandatoryProperties( )
    {
        List<string> missing = ZfsProperty.DefaultDatasetProperties.Keys.Except( Properties.Keys ).ToList( );
        return missing.Any( ) ? new( false, missing ) : new( true, null );
    }

    public List<Snapshot> GetSnapshotsToPrune( )
    {
        Logger.Debug( "Getting list of snapshots to prune for dataset {0}", Name );
        if ( !Enabled )
        {
            Logger.Debug( "Dataset {0} is disabled. Skipping pruning", Name );
            return new( );
        }

        Logger.Debug( "Checking prune deferral setting for dataset {0}", Name );
        if ( PruneDeferral != 0 && PoolUsedCapacity < PruneDeferral )
        {
            Logger.Info( "Pool used capacity for {0} ({1}%) is below prune deferral threshold of {2}%. Skipping pruning of {0}", Name, PoolUsedCapacity, PruneDeferral );
            return new( );
        }

        if ( PruneDeferral == 0 )
        {
            Logger.Debug( "Prune deferral not enabled for {0}", Name );
        }

        List<Snapshot> snapshotsToPrune = new( );
        List<Snapshot> snapshotsSetForPruning = FrequentSnapshots.Where( s => s.PruneSnapshots ).ToList( );
        Logger.Debug( "Frequent snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        int numberToPrune;
        if ( int.TryParse( Properties[ ZfsProperty.SnapshotRetentionFrequentPropertyName ].Value, out int numberToKeep ) && snapshotsSetForPruning.Count > numberToKeep )
        {
            numberToPrune = snapshotsSetForPruning.Count - numberToKeep;
            Logger.Debug( "Need to prune oldest {0} frequent snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot frequentSnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", frequentSnapshot.Name );
                snapshotsToPrune.Add( frequentSnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = HourlySnapshots.Where( s => s.PruneSnapshots ).ToList( );
        Logger.Debug( "Hourly snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        if ( int.TryParse( Properties[ ZfsProperty.SnapshotRetentionHourlyPropertyName ].Value, out numberToKeep ) && snapshotsSetForPruning.Count > numberToKeep )
        {
            numberToPrune = snapshotsSetForPruning.Count - numberToKeep;
            Logger.Debug( "Need to prune oldest {0} hourly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot hourlySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", hourlySnapshot.Name );
                snapshotsToPrune.Add( hourlySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = DailySnapshots.Where( s => s.PruneSnapshots ).ToList( );
        Logger.Debug( "Daily snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        if ( int.TryParse( Properties[ ZfsProperty.SnapshotRetentionDailyPropertyName ].Value, out numberToKeep ) && snapshotsSetForPruning.Count > numberToKeep )
        {
            numberToPrune = snapshotsSetForPruning.Count - numberToKeep;
            Logger.Debug( "Need to prune oldest {0} daily snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot dailySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", dailySnapshot.Name );
                snapshotsToPrune.Add( dailySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = WeeklySnapshots.Where( s => s.PruneSnapshots ).ToList( );
        Logger.Debug( "Weekly snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        if ( int.TryParse( Properties[ ZfsProperty.SnapshotRetentionWeeklyPropertyName ].Value, out numberToKeep ) && snapshotsSetForPruning.Count > numberToKeep )
        {
            numberToPrune = snapshotsSetForPruning.Count - numberToKeep;
            Logger.Debug( "Need to prune oldest {0} weekly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot weeklySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", weeklySnapshot.Name );
                snapshotsToPrune.Add( weeklySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = MonthlySnapshots.Where( s => s.PruneSnapshots ).ToList( );
        Logger.Debug( "Monthly snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        if ( int.TryParse( Properties[ ZfsProperty.SnapshotRetentionMonthlyPropertyName ].Value, out numberToKeep ) && snapshotsSetForPruning.Count > numberToKeep )
        {
            numberToPrune = snapshotsSetForPruning.Count - numberToKeep;
            Logger.Debug( "Need to prune oldest {0} monthly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot monthlySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", monthlySnapshot.Name );
                snapshotsToPrune.Add( monthlySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = YearlySnapshots.Where( s => s.PruneSnapshots ).ToList( );
        Logger.Debug( "Yearly snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        // Don't do this, so these all look the same
        // ReSharper disable once InvertIf
        if ( int.TryParse( Properties[ ZfsProperty.SnapshotRetentionYearlyPropertyName ].Value, out numberToKeep ) && snapshotsSetForPruning.Count > numberToKeep )
        {
            numberToPrune = snapshotsSetForPruning.Count - numberToKeep;
            Logger.Debug( "Need to prune oldest {0} yearly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot yearlySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", yearlySnapshot.Name );
                snapshotsToPrune.Add( yearlySnapshot );
            }
        }

        return snapshotsToPrune;
    }

    /// <summary>
    ///     Gets whether a frequent snapshot is needed, according to the provided <see cref="TemplateSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="template">
    ///     The <see cref="TemplateSettings" /> object to check status against. Must have the
    ///     <see cref="TemplateSettings.SnapshotTiming" /> property defined.
    /// </param>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
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
    public bool IsFrequentSnapshotNeeded( TemplateSettings template, DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no frequent
        if ( !RetentionSettings.IsFrequentWanted )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if frequent snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        int currentFrequentPeriodOfHour = template.SnapshotTiming.GetPeriodOfHour( timestamp );
        int lastFrequentSnapshotPeriodOfHour = template.SnapshotTiming.GetPeriodOfHour( LastFrequentSnapshotTimestamp );
        double minutesSinceLastFrequentSnapshot = ( timestamp - LastFrequentSnapshotTimestamp ).TotalMinutes;
        // Check if more than FrequentPeriod ago or if the period of the hour is different.
        bool frequentSnapshotNeeded = minutesSinceLastFrequentSnapshot >= template.SnapshotTiming.FrequentPeriod || lastFrequentSnapshotPeriodOfHour != currentFrequentPeriodOfHour;
        Logger.Debug( "Frequent snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, frequentSnapshotNeeded ? "" : "not " );
        return frequentSnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether an hourly snapshot is needed, according to the provided <paramref name="timestamp" />
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
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
        if ( !RetentionSettings.IsHourlyWanted )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if hourly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        TimeSpan timeSinceLastHourlySnapshot = timestamp - LastHourlySnapshotTimestamp;
        bool atLeastOneHourSinceLastHourlySnapshot = timeSinceLastHourlySnapshot.TotalHours >= 1d;
        // Check if more than an hour ago or if hour is different
        bool hourlySnapshotNeeded = atLeastOneHourSinceLastHourlySnapshot
                                    || LastHourlySnapshotTimestamp.LocalDateTime.Hour != timestamp.LocalDateTime.Hour;
        Logger.Debug( "Hourly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, hourlySnapshotNeeded ? "" : "not " );
        return hourlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a daily snapshot is needed, according to the provided <see cref="SnapshotRetentionSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
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
        if ( !RetentionSettings.IsDailyWanted )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if daily snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        TimeSpan timeSinceLastDailySnapshot = timestamp - LastDailySnapshotTimestamp;
        bool atLeastOneDaySinceLastDailySnapshot = timeSinceLastDailySnapshot.TotalDays >= 1d;
        // Check if more than a day ago or if a different day of the year
        bool lastDailyOnDifferentDayOfYear = LastDailySnapshotTimestamp.LocalDateTime.DayOfYear != timestamp.LocalDateTime.DayOfYear;
        bool dailySnapshotNeeded = atLeastOneDaySinceLastDailySnapshot || lastDailyOnDifferentDayOfYear;
        Logger.Debug( "Daily snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, dailySnapshotNeeded ? "" : "not " );
        return dailySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a weekly snapshot is needed, according to the provided <see cref="TemplateSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="template">
    ///     The <see cref="TemplateSettings" /> object to check status against. Must have the
    ///     <see cref="TemplateSettings.SnapshotTiming" /> property defined.
    /// </param>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
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
    public bool IsWeeklySnapshotNeeded( TemplateSettings template, DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no weeklies
        if ( !RetentionSettings.IsWeeklyWanted )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if weekly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        TimeSpan timeSinceLastWeeklySnapshot = timestamp - LastWeeklySnapshotTimestamp;
        bool atLeastOneWeekSinceLastWeeklySnapshot = timeSinceLastWeeklySnapshot.TotalDays >= 7d;
        // Check if more than a week ago or if the week number is different by local rules, using the chosen day as the first day of the week
        int lastWeeklySnapshotWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear( LastWeeklySnapshotTimestamp.LocalDateTime, CalendarWeekRule.FirstDay, template.SnapshotTiming.WeeklyDay );
        int currentWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear( timestamp.LocalDateTime, CalendarWeekRule.FirstDay, template.SnapshotTiming.WeeklyDay );
        bool weeklySnapshotNeeded = atLeastOneWeekSinceLastWeeklySnapshot || currentWeekNumber != lastWeeklySnapshotWeekNumber;
        Logger.Debug( "Weekly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, weeklySnapshotNeeded ? "" : "not " );
        return weeklySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a monthly snapshot is needed, according to the provided <see cref="TemplateSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
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
        if ( !RetentionSettings.IsMonthlyWanted )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if monthly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        int lastMonthlySnapshotMonth = CultureInfo.CurrentCulture.Calendar.GetMonth( LastMonthlySnapshotTimestamp.LocalDateTime );
        int currentMonth = CultureInfo.CurrentCulture.Calendar.GetMonth( timestamp.LocalDateTime );
        int lastMonthlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastMonthlySnapshotTimestamp.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last monthly snapshot was in a different month or if same month but different year
        bool lastMonthlySnapshotInDifferentMonth = lastMonthlySnapshotMonth != currentMonth;
        bool lastMonthlySnapshotInDifferentYear = currentYear != lastMonthlySnapshotYear;
        bool monthlySnapshotNeeded = lastMonthlySnapshotInDifferentMonth || lastMonthlySnapshotInDifferentYear;
        Logger.Debug( "Monthly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, monthlySnapshotNeeded ? "" : "not " );
        return monthlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a yearly snapshot is needed, according to the provided <see cref="SnapshotRetentionSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
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
        if ( !RetentionSettings.IsYearlyWanted )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if yearly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        int lastYearlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastYearlySnapshotTimestamp.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last yearly snapshot was in a different year
        bool yearlySnapshotNeeded = lastYearlySnapshotYear != currentYear;
        Logger.Debug( "Yearly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, yearlySnapshotNeeded ? "" : "not " );
        return yearlySnapshotNeeded;
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return JsonSerializer.Serialize( this );
    }

    public Snapshot AddSnapshot( Snapshot snap )
    {
        Logger.Trace( "Adding snapshot {0} to dataset object {1}", snap.Name, Name );
        AllSnapshots[ snap.Name ] = snap;
        switch ( snap.Period.Kind )
        {
            case SnapshotPeriodKind.Frequent:
                FrequentSnapshots.Add( snap );
                break;
            case SnapshotPeriodKind.Hourly:
                HourlySnapshots.Add( snap );
                break;
            case SnapshotPeriodKind.Daily:
                DailySnapshots.Add( snap );
                break;
            case SnapshotPeriodKind.Weekly:
                WeeklySnapshots.Add( snap );
                break;
            case SnapshotPeriodKind.Monthly:
                MonthlySnapshots.Add( snap );
                break;
            case SnapshotPeriodKind.Yearly:
                YearlySnapshots.Add( snap );
                break;
        }

        return snap;
    }
}
