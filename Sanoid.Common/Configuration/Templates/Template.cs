// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration.Datasets;

namespace Sanoid.Common.Configuration.Templates;

/// <summary>
///     A template of settings to use for processing <see cref="Dataset" /> items.
/// </summary>
/// <remarks>
///     All properties except for <see cref="Name" /> are nullable. If a property is null, its value is inherited from its
///     parent <see cref="Template" /> specified in <see cref="UseTemplate" />.<br />
/// </remarks>
public class Template
{
    /// <summary>
    ///     Creates a new instance of a <see cref="Template" /> with the specified name.
    /// </summary>
    /// <param name="templateName">The name, as configured in Sanoid.json, to assign to the <see cref="Name" /> property.</param>
    /// <param name="useTemplateName">The name of a template to use as a parent</param>
    public Template( string templateName, string useTemplateName )
    {
        if ( string.IsNullOrWhiteSpace( templateName ) )
        {
            throw new ArgumentNullException( nameof( templateName ), "All templates MUST have a non-null, non-whitespace, non-empty name" );
        }

        Name = templateName;
        UseTemplateName = useTemplateName;
    }

    private Template( string templateName )
    {
        Name = templateName;
    }

    private bool? _autoPrune;

    private bool? _autoSnapshot;

    private bool? _recursive;
    private bool? _skipChildren;

    private Template? _useTemplate;

    /// <summary>
    ///     Gets or sets whether expired snapshots will be pruned for this template.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether expired snapshots will be pruned for this snapshot.
    /// </value>
    public bool? AutoPrune
    {
        get => _autoPrune ?? UseTemplate?.AutoPrune;
        set => _autoPrune = value;
    }

    /// <summary>
    ///     Gets or sets whether snapshots will be taken for this template.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether snapshots will be taken for this snapshot.
    /// </value>
    public bool? AutoSnapshot
    {
        get => _autoSnapshot ?? UseTemplate?.AutoSnapshot;
        set => _autoSnapshot = value;
    }

    /// <summary>
    ///     Gets a collection of templates that inherit from this template.
    /// </summary>
    public Dictionary<string, Template> Children { get; } = new( );

    /// <summary>
    ///     Gets or sets the name of the template.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> identifying the template, corresponding to a Template item in Sanoid.json.
    /// </value>
    /// <remarks>
    ///     The template MUST be defined in Sanoid.json for initial configuration parsing or an exception will be thrown.
    /// </remarks>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets whether recursive processing will be used for this template and its descendents.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether recursive processing will be used for this template and its
    ///     descendents.
    /// </value>
    public bool? Recursive
    {
        get => _recursive ?? UseTemplate?.Recursive;
        set => _recursive = value;
    }

    /// <summary>
    ///     Gets or sets whether to skip processing of children.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether child datasets will be skipped
    ///     descendents.
    /// </value>
    /// <remarks>
    ///     Can be overridden by <see cref="Dataset.TemplateOverrides" />
    /// </remarks>
    public bool? SkipChildren
    {
        get => _skipChildren ?? UseTemplate?.SkipChildren;
        set => _skipChildren = value;
    }

    /// <summary>
    ///     Gets or sets the snapshot retention policy for this <see cref="Template" />
    /// </summary>
    /// <value>
    ///     A <see cref="SnapshotRetention" /> record specifying the snapshot retention policy for this <see cref="Template" />
    /// </value>
    public SnapshotRetention? SnapshotRetention { get; set; }

    /// <summary>
    ///     Gets or sets the snapshot retention policy for this <see cref="Template" />
    /// </summary>
    /// <value>
    ///     A <see cref="SnapshotRetention" /> record specifying the snapshot retention policy for this <see cref="Template" />
    /// </value>
    public SnapshotTiming? SnapshotTiming { get; set; }

    /// <summary>
    ///     Gets or sets another configured template to inherit settings from.
    /// </summary>
    /// <value>
    ///     A <see cref="Template" /> from which settings are inherited.
    /// </value>
    public Template? UseTemplate
    {
        get => _useTemplate;
        set
        {
            // Add this template as a child of the parent
            value?.Children.TryAdd( Name, this );
            _useTemplate = value;
        }
    }

    internal string UseTemplateName { get; init; }

    internal Template CloneForDatasetWithOverrides( Dataset targetDataset, IConfigurationSection overrides )
    {
        IConfigurationSection retentionOverrides = overrides.GetSection( "SnapshotRetention" );
        IConfigurationSection timingOverrides = overrides.GetSection( "SnapshotTiming" );
        return new Template( $"{targetDataset.Path}_{Name}_Local" )
        {
            SnapshotRetention = retentionOverrides.Exists( )
                ? new SnapshotRetention
                {
                    Daily = retentionOverrides.GetInt( "Daily", SnapshotRetention!.Value.Daily ),
                    Frequent = retentionOverrides.GetInt( "Frequent", SnapshotRetention.Value.Frequent ),
                    FrequentPeriod = retentionOverrides.GetInt( "FrequentPeriod", SnapshotRetention.Value.FrequentPeriod ),
                    Hourly = retentionOverrides.GetInt( "Hourly", SnapshotRetention.Value.Hourly ),
                    Monthly = retentionOverrides.GetInt( "Monthly", SnapshotRetention.Value.Monthly ),
                    PruneDeferral = retentionOverrides.GetInt( "PruneDeferral", SnapshotRetention.Value.PruneDeferral ),
                    Weekly = retentionOverrides.GetInt( "Weekly", SnapshotRetention.Value.Weekly ),
                    Yearly = retentionOverrides.GetInt( "Yearly", SnapshotRetention.Value.Yearly )
                }
                : SnapshotRetention!.Value,
            SnapshotTiming = timingOverrides.Exists( )
                ? new SnapshotTiming
                {
                    DailyTime = timingOverrides[ "DailyTime" ] is null ? SnapshotTiming!.Value.DailyTime : TimeOnly.Parse( timingOverrides[ "DailyTime" ]! ),
                    HourlyMinute = timingOverrides.GetInt( "HourlyMinute", SnapshotTiming!.Value.HourlyMinute ),
                    MonthlyDay = timingOverrides.GetInt( "MonthlyDay", SnapshotTiming!.Value.MonthlyDay ),
                    MonthlyTime = timingOverrides[ "MonthlyTime" ] is null ? SnapshotTiming!.Value.MonthlyTime : TimeOnly.Parse( timingOverrides[ "MonthlyTime" ]! ),
                    UseLocalTime = timingOverrides.GetBoolean( "UseLocalTime", SnapshotTiming!.Value.UseLocalTime ),
                    WeeklyDay = timingOverrides[ "WeeklyDay" ] is null ? SnapshotTiming!.Value.WeeklyDay : Enum.Parse<DayOfWeek>( timingOverrides[ "WeeklyDay" ]! ),
                    WeeklyTime = timingOverrides[ "WeeklyTime" ] is null ? SnapshotTiming!.Value.WeeklyTime : TimeOnly.Parse( timingOverrides[ "WeeklyTime" ]! ),
                    YearlyDay = timingOverrides.GetInt( "YearlyDay", SnapshotTiming!.Value.YearlyDay ),
                    YearlyMonth = timingOverrides.GetInt( "YearlyMonth", SnapshotTiming!.Value.YearlyMonth ),
                    YearlyTime = timingOverrides[ "YearlyTime" ] is null ? SnapshotTiming!.Value.YearlyTime : TimeOnly.Parse( timingOverrides[ "YearlyTime" ]! )
                }
                : SnapshotTiming!.Value,
            AutoPrune = overrides.GetBoolean( "AutoPrune", AutoPrune ),
            AutoSnapshot = overrides.GetBoolean( "AutoSnapshot", AutoSnapshot ),
            Recursive = overrides.GetBoolean( "Recursive", Recursive ),
            SkipChildren = overrides.GetBoolean( "SkipChildren", SkipChildren ),
            UseTemplate = _useTemplate
        };
    }

    internal void InheritSnapshotRetentionAndTimingSettings( )
    {
        Logger log = LogManager.GetCurrentClassLogger( );
        log.Debug( "Inheriting retention and timing settings from Template {0} to children.", Name );
        foreach ( ( _, Template? value ) in Children )
        {
            log.Trace( "Getting configuration section for Template {0}", value.Name );
            IConfigurationSection childConfigSection = JsonConfigurationSections.TemplatesConfiguration.GetSection( value.Name );
            IConfigurationSection childRetentionSettings = childConfigSection.GetSection( "SnapshotRetention" );
            if ( !childRetentionSettings.Exists( ) )
            {
                log.Trace( "No SnapshotRetention overrides specified for Template {0}. Copying all SnapshotRetention settings from parent {1}", value.Name, Name );
                value.SnapshotRetention = SnapshotRetention!.Value;
            }
            else
            {
                log.Trace( "SnapshotRetention overrides found for Template {0}. Overriding SnapshotRetention settings from parent {1} as configured.", value.Name, Name );
                value.SnapshotRetention = new SnapshotRetention
                {
                    FrequentPeriod = childRetentionSettings.GetInt( "FrequentPeriod", SnapshotRetention!.Value.FrequentPeriod ),
                    Frequent = childRetentionSettings.GetInt( "Frequent", SnapshotRetention!.Value.Frequent ),
                    Hourly = childRetentionSettings.GetInt( "Hourly", SnapshotRetention!.Value.Hourly ),
                    Daily = childRetentionSettings.GetInt( "Daily", SnapshotRetention!.Value.Daily ),
                    Weekly = childRetentionSettings.GetInt( "Weekly", SnapshotRetention!.Value.Weekly ),
                    Monthly = childRetentionSettings.GetInt( "Monthly", SnapshotRetention!.Value.Monthly ),
                    Yearly = childRetentionSettings.GetInt( "Yearly", SnapshotRetention!.Value.Yearly ),
                    PruneDeferral = childRetentionSettings.GetInt( "PruneDeferral", SnapshotRetention!.Value.PruneDeferral )
                };
            }

            IConfigurationSection childTimingSettings = childConfigSection.GetSection( "SnapshotTiming" );
            if ( !childTimingSettings.Exists( ) )
            {
                log.Trace( "No SnapshotTiming overrides specified for Template {0}. Copying all SnapshotTiming settings from parent {1}", value.Name, Name );
                value.SnapshotTiming = SnapshotTiming!.Value;
            }
            else
            {
                log.Trace( "SnapshotTiming overrides found for Template {0}. Overriding SnapshotTiming settings from parent {1} as configured.", value.Name, Name );
                value.SnapshotTiming = new SnapshotTiming
                {
                    DailyTime = childTimingSettings[ "DailyTime" ] is null ? SnapshotTiming!.Value.DailyTime : TimeOnly.Parse( childTimingSettings[ "DailyTime" ]! ),
                    HourlyMinute = childTimingSettings.GetInt( "HourlyMinute", SnapshotTiming!.Value.HourlyMinute ),
                    MonthlyDay = childTimingSettings.GetInt( "MonthlyDay", SnapshotTiming!.Value.MonthlyDay ),
                    MonthlyTime = childTimingSettings[ "MonthlyTime" ] is null ? SnapshotTiming!.Value.MonthlyTime : TimeOnly.Parse( childTimingSettings[ "MonthlyTime" ]! ),
                    UseLocalTime = childTimingSettings.GetBoolean( "UseLocalTime", SnapshotTiming!.Value.UseLocalTime ),
                    WeeklyDay = childTimingSettings[ "WeeklyDay" ] is null ? SnapshotTiming!.Value.WeeklyDay : Enum.Parse<DayOfWeek>( childTimingSettings[ "WeeklyDay" ]! ),
                    WeeklyTime = childTimingSettings[ "WeeklyTime" ] is null ? SnapshotTiming!.Value.WeeklyTime : TimeOnly.Parse( childTimingSettings[ "WeeklyTime" ]! ),
                    YearlyDay = childTimingSettings.GetInt( "YearlyDay", SnapshotTiming!.Value.YearlyDay ),
                    YearlyMonth = childTimingSettings.GetInt( "YearlyMonth", SnapshotTiming!.Value.YearlyMonth ),
                    YearlyTime = childTimingSettings[ "YearlyTime" ] is null ? SnapshotTiming!.Value.YearlyTime : TimeOnly.Parse( childTimingSettings[ "YearlyTime" ]! )
                };
            }

            log.Trace( "Retention and timing settings loaded for Template {0}.", value.Name );
            if ( Children.Count > 0 )
            {
                log.Trace( "Processing child templates of {0}.", value.Name );
                value.InheritSnapshotRetentionAndTimingSettings( );
                log.Trace( "Finished processing child templates of {0}.", value.Name );
            }
        }

        log.Debug( "Inheritance complete for all children of Template {0}.", Name );
    }
}
