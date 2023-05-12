// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration.Datasets;

namespace Sanoid.Common.Configuration.Templates;

/// <summary>
///     A template of settings to use for processing <see cref="Dataset" /> items.
/// </summary>
/// <remarks>
///     All properties except for <see cref="Name" /> are nullable. If a property is null, its value is inherited from its
///     parent <see cref="Template" /> specified in <see cref="Parent" />.<br />
/// </remarks>
public class Template
{
    private Template( IConfigurationSection configurationSection, string? nameOverride = null, Template? parent = null )
    {
        Logger.Debug( "Creating new template {0}.", nameOverride ?? configurationSection.Key );
        if ( configurationSection.Key == "default" && parent is null )
        {
            Logger.Trace( "Default template creation requested" );
            if ( _defaultTemplate is not null )
            {
                // We already made the default template.
                // Throw an exception.
                // This is a programming error.
                Logger.Fatal( "Default template already exists. Do not call this constructor for default template more than once." );
                throw new InvalidOperationException( "Constructor for default template can only be called once." );
            }

            // Creating the default template.
            Logger.Debug( "Getting configuration for default template. Hard-coded fallback values will be used for missing or invalid values." );
            Name = "default";
            AutoPrune = configurationSection.GetBoolean( "AutoPrune", true );
            AutoSnapshot = configurationSection.GetBoolean( "AutoSnapshot", true );
            Recursive = configurationSection.GetBoolean( "Recursive", false );
            SkipChildren = configurationSection.GetBoolean( "SkipChildren", false );

            Logger.Trace( "Getting SnapshotRetention for default template." );
            IConfigurationSection snapshotRetentionConfigurationSection = configurationSection.GetRequiredSection( "SnapshotRetention" );
            // Get values from configuration or, if any are missing, use hard-coded defaults that match the original Sanoid.json
            SnapshotRetention = new( )
            {
                Daily = snapshotRetentionConfigurationSection.GetInt( "Daily", 90 ),
                Frequent = snapshotRetentionConfigurationSection.GetInt( "Frequent" ),
                FrequentPeriod = snapshotRetentionConfigurationSection.GetInt( "FrequentPeriod", 15 ),
                Hourly = snapshotRetentionConfigurationSection.GetInt( "Hourly", 48 ),
                Monthly = snapshotRetentionConfigurationSection.GetInt( "Monthly", 6 ),
                PruneDeferral = snapshotRetentionConfigurationSection.GetInt( "PruneDeferral" ),
                Weekly = snapshotRetentionConfigurationSection.GetInt( "Weekly" ),
                Yearly = snapshotRetentionConfigurationSection.GetInt( "Yearly" )
            };

            Logger.Trace( "Getting SnapshotTiming for default template." );
            IConfigurationSection snapshotTimingConfigurationSection = configurationSection.GetRequiredSection( "SnapshotTiming" );
            SnapshotTiming = new( )
            {
                DailyTime = snapshotTimingConfigurationSection.GetTimeOnly( "DailyTime", 23, 59 ),
                HourlyMinute = snapshotTimingConfigurationSection.GetInt( "HourlyMinute", 59 ),
                MonthlyDay = snapshotTimingConfigurationSection.GetInt( "MonthlyDay", 31 ),
                MonthlyTime = snapshotTimingConfigurationSection.GetTimeOnly( "MonthlyTime", 23, 59 ),
                UseLocalTime = snapshotTimingConfigurationSection.GetBoolean( "UseLocalTime", true ),
                WeeklyDay = snapshotTimingConfigurationSection[ "WeeklyDay" ] is null ? DayOfWeek.Monday : Enum.Parse<DayOfWeek>( snapshotTimingConfigurationSection[ "WeeklyDay" ]! ),
                WeeklyTime = snapshotTimingConfigurationSection.GetTimeOnly( "WeeklyTime", 23, 59 ),
                YearlyDay = snapshotTimingConfigurationSection.GetInt( "YearlyDay", 31 ),
                YearlyMonth = snapshotTimingConfigurationSection.GetInt( "YearlyMonth", 12 ),
                YearlyTime = snapshotTimingConfigurationSection.GetTimeOnly( "YearlyTime", 23, 59 )
            };

            Logger.Debug( "Default template created." );
            return;
        }

        // Anything but the default...

        if ( parent is null )
        {
            // If parent is still null at this point, throw an exception, because this was called in error.
            Logger.Fatal( "Error creating template {0}. Template constructor MUST be called with a parent template for all descendents of default.", nameOverride ?? configurationSection.Key );
            throw new ArgumentNullException( nameof( parent ), "Template constructor MUST be called with a parent template, for all descendents of default." );
        }

        if ( string.IsNullOrWhiteSpace( configurationSection.Key ) && string.IsNullOrWhiteSpace( nameOverride ) )
        {
            Logger.Fatal( "Error creating child template of {0}. Null, empty, or all-whitespace name entered.", parent.Name );
            throw new InvalidOperationException( "All templates MUST have a non-null, non-whitespace, non-empty-string name." );
        }

        // Initialize values to the same as the parent
        // Inheritance will be handled by a subsequent call to SetConfigurationOverrides
        Logger.Trace( "Cloning settings from template {0} as initial values for template {1}.", parent.Name, nameOverride ?? configurationSection.Key );
        Name = nameOverride ?? configurationSection.Key;
        Parent = parent;
        SnapshotRetention = parent.SnapshotRetention;
        SnapshotTiming = parent.SnapshotTiming;
        AutoPrune = parent.AutoPrune;
        AutoSnapshot = parent.AutoSnapshot;
        Recursive = parent.Recursive;
        SkipChildren = parent.SkipChildren;
        MyConfigurationSection = configurationSection;
        Logger.Debug( "New template {0} created.", nameOverride ?? configurationSection.Key );
    }

    private bool _recursive;
    private bool _skipChildren;

    private readonly IConfigurationSection MyConfigurationSection = null!;

    /// <summary>
    ///     Gets or sets whether expired snapshots will be pruned for this template.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether expired snapshots will be pruned for this snapshot.
    /// </value>
    public bool AutoPrune { get; set; }

    /// <summary>
    ///     Gets or sets whether snapshots will be taken for this template.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether snapshots will be taken for this snapshot.
    /// </value>
    public bool AutoSnapshot { get; private set; }

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
    public string Name { get; }

    /// <summary>
    ///     Gets or sets another configured template to inherit settings from.
    /// </summary>
    /// <value>
    ///     A <see cref="Template" /> from which settings are inherited.
    /// </value>
    public Template Parent { get; } = null!;

    /// <summary>
    ///     Gets or sets whether ZFS native recursive processing will be used for this template and its descendents.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether ZFS native recursive processing will be used for this template and
    ///     its descendents.
    /// </value>
    /// <remarks>
    ///     Requires <see cref="SkipChildren" /> to be <see langword="false" />.
    /// </remarks>
    /// <exception cref="ConfigurationValidationException">
    ///     If both <see cref="Recursive" /> and <see cref="SkipChildren" /> are <see langword="true" />
    /// </exception>
    public bool Recursive
    {
        get => _recursive;
        private set
        {
            if ( value && !SkipChildren )
            {
                // This behavior is different than PERL sanoid's behavior, which is to treat Recursive as authoritative.
                // I believe throwing an exception here and forcing the user to configure it properly is better,
                // as it helps to ensure the user understands the end result of their configuration and doesn't silently
                // succeed when there is a conflict of settings.
                Logger.Fatal( "Recursive cannot be true together with SkipChildren=true for template {0}", Name );
                throw new ConfigurationValidationException( $"Recursive cannot be true together with SkipChildren=true for template {Name}." );
            }

            _recursive = value;
        }
    }

    /// <summary>
    ///     Gets or sets whether to skip processing of children.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether child datasets will be skipped
    ///     descendents.
    /// </value>
    /// <remarks>
    ///     Requires <see cref="Recursive" /> to be <see langword="false" />
    /// </remarks>
    public bool SkipChildren
    {
        get => _skipChildren;
        private set
        {
            if ( Recursive && value )
            {
                // This behavior is different than PERL sanoid's behavior, which is to treat Recursive as authoritative.
                // I believe throwing an exception here and forcing the user to configure it properly is better,
                // as it helps to ensure the user understands the end result of their configuration and doesn't silently
                // succeed when there is a conflict of settings.
                Logger.Fatal( "SkipChildren and Recursive both be true for template {0}.", Name );
                throw new ConfigurationValidationException( $"SkipChildren and Recursive cannot both be true for template {Name}." );
            }

            _skipChildren = value;
        }
    }

    /// <summary>
    ///     Gets or sets the snapshot retention policy for this <see cref="Template" />
    /// </summary>
    /// <value>
    ///     A <see cref="SnapshotRetention" /> record specifying the snapshot retention policy for this <see cref="Template" />
    /// </value>
    public SnapshotRetention SnapshotRetention { get; set; }

    /// <summary>
    ///     Gets or sets the snapshot retention policy for this <see cref="Template" />
    /// </summary>
    /// <value>
    ///     A <see cref="SnapshotRetention" /> record specifying the snapshot retention policy for this <see cref="Template" />
    /// </value>
    public SnapshotTiming SnapshotTiming { get; set; }

    private static Template? _defaultTemplate;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Creates a child Template, configures it, inherits settings not explicitly set for it, and recursively does the same
    ///     for
    ///     all children of that Template, as well.
    /// </summary>
    /// <param name="childConfigurationSection">The <see cref="IConfigurationSection" /> the child will be created from.</param>
    /// <param name="nameOverride">If we're creating the child for a <see cref="Dataset" /> with overrides, use this name.</param>
    /// <param name="isDatasetOverride">
    ///     Set to <see langword="true" /> if being called to create override settings from a <see cref="Dataset" />.<br />
    ///     Otherwise, false
    /// </param>
    /// <param name="skipRecursion">Mainly intended for calling from tests. Instructs this function to exit without recursing into sub-templates.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Template CreateChild( IConfigurationSection childConfigurationSection, string? nameOverride = null, bool isDatasetOverride = false, bool skipRecursion = false )
    {
        Logger.Trace( "Entered CreateChild from template {0}, with requested new template {1}", Name, nameOverride ?? ( string.IsNullOrEmpty( childConfigurationSection.Key ) ? "INVALID KEY" : childConfigurationSection.Key ) );
        string childTemplateName = nameOverride ?? childConfigurationSection.Key;

        if ( string.IsNullOrWhiteSpace( childTemplateName ) )
        {
            throw new ArgumentException( "All templates MUST have a non-null, non-whitespace, non-empty name", nameof( childConfigurationSection ) );
        }

        Logger.Debug( "Creating child template {0} of template {1}.", childTemplateName, Name );

        Template newChildTemplate = new( childConfigurationSection, childTemplateName, this );

        newChildTemplate.SetConfigurationOverrides( childConfigurationSection );

        // We now have a child template with inherited and overridden properties, as specified in configuration.
        // Add it to this template's children
        Logger.Debug( "Adding template {0} to children of template {1}", newChildTemplate.Name, Name );
        Children.Add( childTemplateName, newChildTemplate );

        if ( isDatasetOverride )
        {
            // This is just a dataset override.
            // We can exit now, as child templates here would be meaningless.
            Logger.Debug( "Child template {0} of template {1} is a dataset override. Not checking for children.", newChildTemplate.Name, Name );
            Configuration.Templates.TryAdd( newChildTemplate.Name, newChildTemplate );
            return newChildTemplate;
        }

        // Check if there's a Templates section in the child. If not, return the child now.
        // If so, recurse back into this method using its children
        Logger.Trace( "Checking for children of template {0}", newChildTemplate.Name );
        IConfigurationSection childTemplatesSection = childConfigurationSection.GetSection( "Templates" );
        if ( !childTemplatesSection.Exists( ) )
        {
            Logger.Trace( "Template {0} has no Templates section.", newChildTemplate.Name );
            Configuration.Templates.TryAdd( newChildTemplate.Name, newChildTemplate );
            return newChildTemplate;
        }

        if ( skipRecursion )
        {
            Logger.Info( "skipRecusion specified while creating template {0}. Returning now.", newChildTemplate.Name );
            Configuration.Templates.TryAdd( newChildTemplate.Name, newChildTemplate );
            return newChildTemplate;
        }

        Logger.Debug( "Template {0} has Templates section. Checking contents of that section.", newChildTemplate.Name );

        Configuration.Templates.TryAdd( newChildTemplate.Name, newChildTemplate );

        foreach ( IConfigurationSection grandChildTemplateConfiguration in childTemplatesSection.GetChildren( ) )
        {
            Logger.Debug( "Recursively calling CreateChild on {0} for new template {1}", newChildTemplate.Name, grandChildTemplateConfiguration.Key );
            newChildTemplate.CreateChild( grandChildTemplateConfiguration );
        }

        Logger.Debug( "No more children of {0} remain. Returning template {0} from {1}.CreateChild", newChildTemplate.Name, Name );
        // Once we've exhausted the grandchild list (or if it was simply empty), return the child.

        return newChildTemplate;
    }

    private void SetConfigurationOverrides( IConfigurationSection templateConfigurationSection )
    {
        Logger.Debug( "Checking for overrides for template {0}, to override parent {1}", Name, Parent.Name );

        Logger.Trace( "Setting explicit top-level overrides for template {0} and ineriting the rest from {1}", Name, Parent.Name );

        AutoPrune = templateConfigurationSection.GetBoolean( "AutoPrune", AutoPrune );
        AutoSnapshot = templateConfigurationSection.GetBoolean( "AutoSnapshot", AutoSnapshot );
        Recursive = templateConfigurationSection.GetBoolean( "Recursive", Recursive );
        SkipChildren = templateConfigurationSection.GetBoolean( "SkipChildren", SkipChildren );

        Logger.Trace( "Finished setting explicit top-level overrides for template {0} and ineriting the rest from {1}", Name, Parent.Name );

        Logger.Trace( "Checking for existence of SnapshotRetention section for template {0}", Name );
        IConfigurationSection retentionOverrides = templateConfigurationSection.GetSection( "SnapshotRetention" );
        if ( retentionOverrides.Exists( ) )
        {
            Logger.Debug( "SnapshotRetention overrides exist for template {0}. Setting explicit settings and inheriting the rest from {1}.", Name, Parent.Name );
            SnapshotRetention = new( )
            {
                Daily = retentionOverrides.GetInt( "Daily", SnapshotRetention.Daily ),
                Frequent = retentionOverrides.GetInt( "Frequent", SnapshotRetention.Frequent ),
                FrequentPeriod = retentionOverrides.GetInt( "FrequentPeriod", SnapshotRetention.FrequentPeriod ),
                Hourly = retentionOverrides.GetInt( "Hourly", SnapshotRetention.Hourly ),
                Monthly = retentionOverrides.GetInt( "Monthly", SnapshotRetention.Monthly ),
                PruneDeferral = retentionOverrides.GetInt( "PruneDeferral", SnapshotRetention.PruneDeferral ),
                Weekly = retentionOverrides.GetInt( "Weekly", SnapshotRetention.Weekly ),
                Yearly = retentionOverrides.GetInt( "Yearly", SnapshotRetention.Yearly )
            };
        }

        Logger.Trace( "Checking for existence of SnapshotTiming section for template {0}", Name );
        IConfigurationSection timingOverrides = templateConfigurationSection.GetSection( "SnapshotTiming" );
        if ( timingOverrides.Exists( ) )
        {
            Logger.Debug( "SnapshotTiming overrides exist for template {0}. Setting explicit settings and inheriting the rest from {1}.", Name, Parent.Name );
            SnapshotTiming = new( )
            {
                DailyTime = timingOverrides[ "DailyTime" ] is null ? SnapshotTiming.DailyTime : TimeOnly.Parse( timingOverrides[ "DailyTime" ]! ),
                HourlyMinute = timingOverrides.GetInt( "HourlyMinute", SnapshotTiming.HourlyMinute ),
                MonthlyDay = timingOverrides.GetInt( "MonthlyDay", SnapshotTiming.MonthlyDay ),
                MonthlyTime = timingOverrides[ "MonthlyTime" ] is null ? SnapshotTiming.MonthlyTime : TimeOnly.Parse( timingOverrides[ "MonthlyTime" ]! ),
                UseLocalTime = timingOverrides.GetBoolean( "UseLocalTime", SnapshotTiming.UseLocalTime ),
                WeeklyDay = timingOverrides[ "WeeklyDay" ] is null ? SnapshotTiming.WeeklyDay : Enum.Parse<DayOfWeek>( timingOverrides[ "WeeklyDay" ]! ),
                WeeklyTime = timingOverrides[ "WeeklyTime" ] is null ? SnapshotTiming.WeeklyTime : TimeOnly.Parse( timingOverrides[ "WeeklyTime" ]! ),
                YearlyDay = timingOverrides.GetInt( "YearlyDay", SnapshotTiming.YearlyDay ),
                YearlyMonth = timingOverrides.GetInt( "YearlyMonth", SnapshotTiming.YearlyMonth ),
                YearlyTime = timingOverrides[ "YearlyTime" ] is null ? SnapshotTiming.YearlyTime : TimeOnly.Parse( timingOverrides[ "YearlyTime" ]! )
            };
        }
    }

    internal Template CloneForDatasetWithOverrides( IConfigurationSection overrides, Dataset targetDataset )
    {
        Logger.Debug( "Cloning template {0} to apply overrides in dataset {1}", targetDataset.Template!.Name, targetDataset.Path );
        Template clone = CreateChild( overrides, $"{targetDataset.Path}_{Name}_Local" );
        return clone;
    }

    internal static Template GetDefault( IConfigurationSection defaultTemplateSection, [CallerMemberName] string? caller = "unknown" )
    {
        Logger.Trace( "Default template requested by {0}", caller );
        if ( _defaultTemplate is not null )
        {
            Logger.Trace( "Default template already exists. Returning existing template to {0}.", caller );
            return _defaultTemplate;
        }

        Logger.Debug( "Default template not yet configured. Attempting to build from configuration." );

        _defaultTemplate = new( defaultTemplateSection );

        Logger.Debug( "Default template created from configuration. Returning to {0}.", caller );

        return _defaultTemplate;
    }
}
