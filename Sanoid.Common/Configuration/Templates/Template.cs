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
        string templateName = nameOverride ?? configurationSection.Key;
        string configurationSectionPath = configurationSection.Path;
        Logger.Debug( "Creating new template {templateName}", templateName );
        if ( configurationSection.Key == "default" && parent is null )
        {
            // Creating the default template.
            Logger.Debug( "Getting configuration for default template at {configurationSectionPath}. Hard-coded fallback values will be used for missing or invalid values.", configurationSectionPath );
            Name = "default";
            AutoPrune = configurationSection.GetBoolean( "AutoPrune", true );
            AutoSnapshot = configurationSection.GetBoolean( "AutoSnapshot", true );
            Recursive = configurationSection.GetBoolean( "Recursive", false );
            SkipChildren = configurationSection.GetBoolean( "SkipChildren", false );
            MyConfigurationSection = configurationSection;

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
            Logger.Fatal( "Error creating template {templateName}. Template constructor MUST be called with a parent template for all descendents of default", templateName );
            throw new ArgumentNullException( nameof( parent ), "Template constructor MUST be called with a parent template, for all descendents of default." );
        }

        if ( string.IsNullOrWhiteSpace( configurationSection.Key ) && string.IsNullOrWhiteSpace( nameOverride ) )
        {
            Logger.Fatal( "Error creating child template of {name}. Null, empty, or all-whitespace name entered", parent.Name );
            throw new InvalidOperationException( "All templates MUST have a non-null, non-whitespace, non-empty-string name." );
        }

        // Initialize values to the same as the parent
        // Inheritance will be handled by a subsequent call to SetConfigurationOverrides
        string parentTemplateConfigPath = parent.MyConfigurationSection.Path;
        Logger.Trace( "Cloning settings from template {name} at {parentTemplateConfigPath} as initial values for template {templateName} at {configurationSectionPath}", parent.Name, parentTemplateConfigPath, templateName, configurationSectionPath );
        Name = templateName;
        Parent = parent;
        SnapshotRetention = parent.SnapshotRetention;
        SnapshotTiming = parent.SnapshotTiming;
        AutoPrune = parent.AutoPrune;
        AutoSnapshot = parent.AutoSnapshot;
        Recursive = parent.Recursive;
        SkipChildren = parent.SkipChildren;
        MyConfigurationSection = configurationSection;
        Logger.Debug( message: "New template {templateName} created", templateName );
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
                Logger.Fatal( "Recursive cannot be true together with SkipChildren=true for template {0}", argument: Name );
                throw new ConfigurationValidationException( message: $"Recursive cannot be true together with SkipChildren=true for template {Name}." );
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
                Logger.Fatal( "SkipChildren and Recursive both be true for template {0}", argument: Name );
                throw new ConfigurationValidationException( message: $"SkipChildren and Recursive cannot both be true for template {Name}." );
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

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Creates a child Template, configures it, inherits settings not explicitly set for it, and recursively does the same
    ///     for
    ///     all children of that Template, as well.
    /// </summary>
    /// <param name="childConfigurationSection">The <see cref="IConfigurationSection" /> the child will be created from.</param>
    /// <param name="childTemplateName"><see cref="Template"/> we're creating the child for a <see cref="Dataset" /> with overrides, use this name.</param>
    /// <param name="isDatasetOverride">
    ///     Set to <see langword="true" /> if being called to create override settings from a <see cref="Dataset" />.<br />
    ///     Otherwise, false
    /// </param>
    /// <param name="skipRecursion">Mainly intended for calling from tests. Instructs this function to exit without recursing into sub-templates.</param>
    /// <param name="allTemplates"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Template CreateChild( IConfigurationSection childConfigurationSection, Dictionary<string, Template> allTemplates, string childTemplateName, bool isDatasetOverride = false, bool skipRecursion = false )
    {
        Logger.Trace( message: "Entered CreateChild from template {0}, with requested new template {1}", argument1: Name, argument2: childTemplateName ?? ( string.IsNullOrEmpty( value: childConfigurationSection.Key ) ? "INVALID KEY" : childConfigurationSection.Key ) );
        childTemplateName ??= childConfigurationSection.Key;

        if ( string.IsNullOrWhiteSpace( value: childTemplateName ) )
        {
            throw new ArgumentException( message: "All templates MUST have a non-null, non-whitespace, non-empty name", paramName: nameof( childConfigurationSection ) );
        }

        Logger.Debug( message: "Creating child template {0} of template {1}", argument1: childTemplateName, argument2: Name );

        Template newChildTemplate = new( configurationSection: childConfigurationSection, nameOverride: childTemplateName, parent: this );

        newChildTemplate.SetConfigurationOverrides( templateConfigurationSection: childConfigurationSection );

        // We now have a child template with inherited and overridden properties, as specified in configuration.
        // Add it to this template's children
        Logger.Debug( message: "Adding template {0} to children of template {1}", argument1: newChildTemplate.Name, argument2: Name );
        Children.Add( key: childTemplateName, value: newChildTemplate );

        if ( isDatasetOverride )
        {
            // This is just a dataset override.
            // We can exit now, as child templates here would be meaningless.
            Logger.Debug( message: "Child template {0} of template {1} is a dataset override. Not checking for children", argument1: newChildTemplate.Name, argument2: Name );
            allTemplates.TryAdd( key: newChildTemplate.Name, value: newChildTemplate );
            return newChildTemplate;
        }

        // If we are supposed to skip recursion, exit and return this child now
        if ( skipRecursion )
        {
            Logger.Info( message: "skipRecusion specified while creating template {0}. Returning now", argument: newChildTemplate.Name );
            allTemplates.TryAdd( key: newChildTemplate.Name, value: newChildTemplate );
            return newChildTemplate;
        }

        // Check if there's a Templates section in the child. If not, return the child now.
        // If so, recurse back into this method using its children
        Logger.Trace( message: "Checking for children of template {0}", argument: newChildTemplate.Name );
        IConfigurationSection childTemplatesSection = childConfigurationSection.GetSection( key: "Templates" );
        if ( !childTemplatesSection.Exists( ) )
        {
            Logger.Trace( message: "Template {0} has no Templates section", argument: newChildTemplate.Name );
            allTemplates.TryAdd( key: newChildTemplate.Name, value: newChildTemplate );
            return newChildTemplate;
        }


        Logger.Debug( message: "Template {0} has Templates section. Checking contents of that section", argument: newChildTemplate.Name );

        allTemplates.TryAdd( key: newChildTemplate.Name, value: newChildTemplate );

        foreach ( IConfigurationSection grandChildTemplateConfiguration in childTemplatesSection.GetChildren( ) )
        {
            Logger.Debug( message: "Recursively calling CreateChild on {0} for new template {1}", argument1: newChildTemplate.Name, argument2: grandChildTemplateConfiguration.Key );
            newChildTemplate.CreateChild( childConfigurationSection: grandChildTemplateConfiguration, allTemplates: allTemplates, grandChildTemplateConfiguration.Key );
        }

        Logger.Debug( message: "No more children of {0} remain. Returning template {0} from {1}.CreateChild", argument1: newChildTemplate.Name, argument2: Name );
        // Once we've exhausted the grandchild list (or if it was simply empty), return the child.

        return newChildTemplate;
    }

    private void SetConfigurationOverrides( IConfigurationSection templateConfigurationSection )
    {
        Logger.Debug( message: "Checking for overrides for template {0}, to override parent {1}", argument1: Name, argument2: Parent.Name );

        Logger.Trace( message: "Setting explicit top-level overrides for template {0} and ineriting the rest from {1}", argument1: Name, argument2: Parent.Name );

        AutoPrune = templateConfigurationSection.GetBoolean( settingKey: "AutoPrune", fallbackValue: AutoPrune );
        AutoSnapshot = templateConfigurationSection.GetBoolean( settingKey: "AutoSnapshot", fallbackValue: AutoSnapshot );
        Recursive = templateConfigurationSection.GetBoolean( settingKey: "Recursive", fallbackValue: Recursive );
        SkipChildren = templateConfigurationSection.GetBoolean( settingKey: "SkipChildren", fallbackValue: SkipChildren );

        Logger.Trace( message: "Finished setting explicit top-level overrides for template {0} and ineriting the rest from {1}", argument1: Name, argument2: Parent.Name );

        Logger.Trace( message: "Checking for existence of SnapshotRetention section for template {0}", argument: Name );
        IConfigurationSection retentionOverrides = templateConfigurationSection.GetSection( key: "SnapshotRetention" );
        if ( retentionOverrides.Exists( ) )
        {
            Logger.Debug( message: "SnapshotRetention overrides exist for template {0}. Setting explicit settings and inheriting the rest from {1}", argument1: Name, argument2: Parent.Name );
            SnapshotRetention = new( )
            {
                Daily = retentionOverrides.GetInt( settingKey: "Daily", fallbackValue: SnapshotRetention.Daily ),
                Frequent = retentionOverrides.GetInt( settingKey: "Frequent", fallbackValue: SnapshotRetention.Frequent ),
                FrequentPeriod = retentionOverrides.GetInt( settingKey: "FrequentPeriod", fallbackValue: SnapshotRetention.FrequentPeriod ),
                Hourly = retentionOverrides.GetInt( settingKey: "Hourly", fallbackValue: SnapshotRetention.Hourly ),
                Monthly = retentionOverrides.GetInt( settingKey: "Monthly", fallbackValue: SnapshotRetention.Monthly ),
                PruneDeferral = retentionOverrides.GetInt( settingKey: "PruneDeferral", fallbackValue: SnapshotRetention.PruneDeferral ),
                Weekly = retentionOverrides.GetInt( settingKey: "Weekly", fallbackValue: SnapshotRetention.Weekly ),
                Yearly = retentionOverrides.GetInt( settingKey: "Yearly", fallbackValue: SnapshotRetention.Yearly )
            };
        }

        Logger.Trace( message: "Checking for existence of SnapshotTiming section for template {0}", argument: Name );
        IConfigurationSection timingOverrides = templateConfigurationSection.GetSection( key: "SnapshotTiming" );
        if ( timingOverrides.Exists( ) )
        {
            Logger.Debug( message: "SnapshotTiming overrides exist for template {0}. Setting explicit settings and inheriting the rest from {1}", argument1: Name, argument2: Parent.Name );
            SnapshotTiming = new( )
            {
                DailyTime = timingOverrides[ key: "DailyTime" ] is null ? SnapshotTiming.DailyTime : TimeOnly.Parse( s: timingOverrides[ key: "DailyTime" ]! ),
                HourlyMinute = timingOverrides.GetInt( settingKey: "HourlyMinute", fallbackValue: SnapshotTiming.HourlyMinute ),
                MonthlyDay = timingOverrides.GetInt( settingKey: "MonthlyDay", fallbackValue: SnapshotTiming.MonthlyDay ),
                MonthlyTime = timingOverrides[ key: "MonthlyTime" ] is null ? SnapshotTiming.MonthlyTime : TimeOnly.Parse( s: timingOverrides[ key: "MonthlyTime" ]! ),
                UseLocalTime = timingOverrides.GetBoolean( settingKey: "UseLocalTime", fallbackValue: SnapshotTiming.UseLocalTime ),
                WeeklyDay = timingOverrides[ key: "WeeklyDay" ] is null ? SnapshotTiming.WeeklyDay : Enum.Parse<DayOfWeek>( value: timingOverrides[ key: "WeeklyDay" ]! ),
                WeeklyTime = timingOverrides[ key: "WeeklyTime" ] is null ? SnapshotTiming.WeeklyTime : TimeOnly.Parse( s: timingOverrides[ key: "WeeklyTime" ]! ),
                YearlyDay = timingOverrides.GetInt( settingKey: "YearlyDay", fallbackValue: SnapshotTiming.YearlyDay ),
                YearlyMonth = timingOverrides.GetInt( settingKey: "YearlyMonth", fallbackValue: SnapshotTiming.YearlyMonth ),
                YearlyTime = timingOverrides[ key: "YearlyTime" ] is null ? SnapshotTiming.YearlyTime : TimeOnly.Parse( s: timingOverrides[ key: "YearlyTime" ]! )
            };
        }
    }

    internal Template CloneForDatasetWithOverrides( IConfigurationSection overrides, Dataset targetDataset, Dictionary<string, Template> allTemplates )
    {
        Logger.Debug( message: "Cloning template {0} to apply overrides in dataset {1}", argument1: targetDataset.Template!.Name, argument2: targetDataset.Path );
        Template clone = CreateChild( childConfigurationSection: overrides, allTemplates: allTemplates, childTemplateName: $"{targetDataset.Path}_{Name}_Local",isDatasetOverride:true );
        return clone;
    }

    internal static Template GetDefault( IConfigurationSection defaultTemplateSection, [CallerMemberName] string? caller = "unknown" )
    {
        Logger.Trace( message: "Default template requested by {0}", argument: caller );

        Template defaultTemplate = new( configurationSection: defaultTemplateSection );

        Logger.Debug( message: "Default template created from configuration. Returning to {0}", argument: caller );

        return defaultTemplate;
    }
}
