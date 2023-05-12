// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;

using NLog.Config;

using Sanoid.Common.Configuration.Datasets;
using Sanoid.Common.Configuration.Templates;
using Sanoid.Common.Zfs;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Root class for access to configuration settings
/// </summary>
public static class Configuration
{
    // It is ok to disable this warning here, because we explicitly initialize everything in Initialize, which is when this class
    // first gets instantiated.
    // The code is in that method instead of this constructor because it is bad form to do things that can cause exceptions to be
    // thrown in a static constructor, if it can be reasonably avoided. So long as we make sure we've called Initialize() before
    // we touch anything else in this class, that is fine.
#pragma warning disable CS8618
    static Configuration( )
    {
    }
#pragma warning restore CS8618

    /// <summary>
    ///     Gets or sets sanoid's cache path.<br />
    ///     Corresponds to the /CacheDirectory property of Sanoid.json.
    /// </summary>
    /// <remarks>
    ///     Default value is "/var/cache/sanoid"<br />
    /// </remarks>
    /// <value>A <see langword="string" /> indicating the path for sanoid-generated cache files</value>
    public static string CacheDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the absolute path to the directory containing PERL sanoid's configuration files.<br />
    ///     Corresponds to the /ConfigurationPathBase property of Sanoid.json.
    /// </summary>
    /// <remarks>
    ///     Default value is "/etc/sanoid"<br />
    ///     Should not contain a trailing slash.<br />
    ///     Not guaranteed to work with a relative path. Use an absolute path.
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> indicating the absolute path to the directory containing PERL sanoid's configuration
    ///     files
    /// </value>
    public static string ConfigurationPathBase { get; set; }

    /// <summary>
    ///     Gets or sets whether Sanoid.net should take snapshots and prune expired snapshots.
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> indicating whether Sanoid.net will take new snapshots and prune expired snapshots.
    /// </value>
    public static bool Cron
    {
        get => _cron;
        set
        {
            if ( value )
            {
                _takeSnapshots = true;
                _pruneSnapshots = true;
            }

            _cron = value;
        }
    }

    /// <summary>
    ///     Gets a <see cref="Dictionary{TKey,TValue}" /> of <see cref="Dataset" />s, indexed by <see langword="string" />.
    /// </summary>
    /// <remarks>
    ///     First element added should be the virtual root Dataset.
    /// </remarks>
    public static Dictionary<string, Dataset> Datasets { get; } = new( );

    /// <summary>
    ///     Gets or sets the default logging levels to be used by NLog
    /// </summary>
    /// <remarks>
    ///     Getter returns the lowest severity logging level of all configured rules.<br />
    ///     Setter overrides level for all configured rules.
    /// </remarks>
    /// <value>A <see cref="LogLevel" /> indicating the lowest logging level set of any rule.</value>
    public static LogLevel DefaultLoggingLevel
    {
        get
        {
            Log.Debug( "Getting lowest log level of all rules." );
            LogLevel? lowestLogLevel = LogManager.Configuration!.LoggingRules.Min( rule => rule.Levels.Min( ) );
            if ( lowestLogLevel is null )
            {
                Log.Debug( "No logging levels set. Setting to {0}", LogLevel.Info.Name );
                lowestLogLevel = LogLevel.Info;
            }

            Log.Debug( "Lowest logging level is {0}", lowestLogLevel );
            return lowestLogLevel;
        }
        set
        {
            Log.Warn( "Log levels should be changed in Sanoid.nlog.json for normal usage." );
            Log.Debug( "Setting minimum log severity to {0} for ALL rules.", value.Name );
            for ( int ruleIndex = 0; ruleIndex < LogManager.Configuration!.LoggingRules.Count; ruleIndex++ )
            {
                LoggingRule rule = LogManager.Configuration.LoggingRules[ ruleIndex ];
                if ( value == LogLevel.Off )
                {
                    Log.Trace( "Disabling logging for rule {0}", ruleIndex );
                    rule.SetLoggingLevels( LogLevel.Off, LogLevel.Off );
                    Log.Trace( "Disabled logging for rule {0}", ruleIndex );
                }
                else
                {
                    Log.Trace( "Setting log level to {0} for rule {1}", value.Name, ruleIndex );
                    rule.SetLoggingLevels( value, LogLevel.Fatal );
                    Log.Trace( "Log level set to {0} for rule {1}", value.Name, ruleIndex );
                }
            }

            Log.Debug( "Reconfiguring loggers" );
            LogManager.ReconfigExistingLoggers( );
        }
    }

    /// <summary>
    ///     Gets or sets if the current run is a dry run, where no changes will be made to zfs
    /// </summary>
    /// <value>
    ///     A A <see langword="bool" /> indicating if no changes will be made to zfs (<see langword="true" />) or if normal
    ///     processing will occur (<see langword="false" /> - default)
    /// </value>
    public static bool DryRun { get; set; }

    /// <summary>
    ///     Gets or sets whether Sanoid.net should prune expired snapshots.
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> indicating whether Sanoid.net will prune expired snapshots.
    /// </value>
    public static bool PruneSnapshots
    {
        get => _pruneSnapshots;
        set
        {
            if ( !value || !_takeSnapshots )
            {
                Cron = false;
            }

            _pruneSnapshots = value;
        }
    }

    /// <summary>
    ///     Gets or sets sanoid's run path.<br />
    ///     Corresponds to the /RunDirectory property of Sanoid.json.
    /// </summary>
    /// <remarks>
    ///     Default value is "/var/run/sanoid"<br />
    /// </remarks>
    /// <value>A <see langword="string" /> indicating the path for sanoid-generated runtime files</value>
    public static string RunDirectory { get; set; }

    /// <summary>
    ///     Gets or sets whether Sanoid.net should take new snapshots.
    /// </summary>
    /// <value>
    ///     A <see langword="bool" /> indicating whether Sanoid.net will take new snapshots.
    /// </value>
    public static bool TakeSnapshots
    {
        get => _takeSnapshots;
        set
        {
            if ( !value || !_pruneSnapshots )
            {
                Cron = false;
            }

            _takeSnapshots = value;
        }
    }

    /// <summary>
    ///     Gets a <see cref="Dictionary{TKey,TValue}" /> of <see cref="Template" />s, indexed
    ///     by <see langword="string" />.
    /// </summary>
    /// <remarks>
    ///     First initialized to an empty dictionary on instantiation of the static <see cref="Configuration" /> class,
    ///     and then any <see cref="Template" />s found in Sanoid.json are added to the
    ///     collection.
    /// </remarks>
    public static Dictionary<string, Template> Templates { get; } = new( );

    private static bool _cron;
    private static bool _pruneSnapshots;
    private static bool _takeSnapshots;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Builds the full dataset path tree and creates datasets as disabled entries.
    /// </summary>
    private static void BuildDatasetHierarchy( Template defaultTemplateForRoot )
    {
        Log.Debug( "Building dataset hiearchy from combination of configured datasets and all datasets on system." );

        // First, add the virtual root dataset, with the provided template
        Datasets.Add( "/", Dataset.GetRoot( defaultTemplateForRoot ) );

        List<string> zfsListResults = CommandRunner.ZfsListAll( );
        // List is returned in the form of a path tree already, so we can just scan the list linearly
        // Pool nodes will be added as children of the dummy root node, and so on down the chain until all datasets exist in the
        // Datasets diciontary
        foreach ( string dsName in zfsListResults )
        {
            Log.Debug( "Processing dataset {0}.", dsName );
            #if WINDOWS
            // Gotta love how Windows changes the forward slashes to backslashes silently, but only on paths more than 1 deep...
            string? parentDsName = $"/{Path.GetDirectoryName( dsName )}".Replace( "\\","/" );
            #else
            string? parentDsName = $"/{Path.GetDirectoryName( dsName )}";
            #endif
            Dataset newDs = new( dsName )
            {
                Enabled = false,
                IsInConfiguration = false,
                Parent = Datasets[ parentDsName ]
            };
            Datasets.TryAdd( newDs.VirtualPath, newDs );
            Log.Debug( "Dataset {0} added to dictionary.", dsName );
        }
    }

    private static void LoadDatasetConfigurations( )
    {
        //TODO: This can probably be inlined when loading datasets
        Log.Debug( "Setting dataset options from configuration" );
        // Scan the datasets collection
        // If an entry exists in configuration, set its settings, following inheritance rules.
        foreach ( (_, Dataset? ds) in Datasets )
        {
            Log.Debug( "Processing dataset {0}", ds.Path );
            if ( ds.Path == "/" )
            {
                //Skip the root dataset, as it is already configured for defaults.
                continue;
            }

            IConfigurationSection section = JsonConfigurationSections.DatasetsConfiguration.GetSection( ds.Path );
            if ( section.Exists( ) )
            {
                // Dataset exists in configuration. Set configured settings and inherit everything else
                ds.IsInConfiguration = true;
                ds.Enabled = section.GetBoolean( "Enabled", true );
                string? templateName = section[ "Template" ];
                ds.Template = templateName is null ? ds.Parent!.Template : Templates[ templateName ];

                IConfigurationSection overrides = section.GetSection( "TemplateOverrides" );
                if ( overrides.Exists( ) )
                {
                    Log.Debug( "Template overrides exist for Dataset {0}. Creating override Template with settings inherited from Template {1}.", section.Key, templateName );
                    ds.Template = ds.Template!.CloneForDatasetWithOverrides( overrides, ds );
                }
            }
            else
            {
                // Dataset is not explicitly configured. Inherit relevant properties from parent only.
                ds.Enabled = ds.Parent!.Enabled;
                ds.Template = ds.Parent.Template;
            }
        }

        Log.Debug( "Dataset options configured." );
    }

    private static void BuildTemplateHierarchy( IConfigurationSection defaultTemplateConfigurationSection )
    {
        // We have enforced a tree structure for templates in the configuration, so we can recursively descend down the configuration
        // tree to get template configuration. We will add them to a flat dictionary, as well, for easy access by key.

        Log.Debug( "Creating Template objects from configuration" );

        // First, add the default template
        Template defaultTemplate = Template.GetDefault( defaultTemplateConfigurationSection );
        Templates.TryAdd( "default", defaultTemplate );

        // Now, for all children, if any, call recursive function to descend down the tree, respecting inheritance as we go
        // This is the "Templates" section of the "default" template. It can exist and be empty, exist and have entries, or not exist.
        // All cases are ok, as the Exists() function handles that for us.
        IConfigurationSection defaultTemplateChildTemplatesConfigurationSection = defaultTemplateConfigurationSection.GetSection( "Templates" );

        if ( defaultTemplateChildTemplatesConfigurationSection.Exists( ) )
        {
            foreach ( IConfigurationSection childConfigurationSection in defaultTemplateChildTemplatesConfigurationSection.GetChildren( ) )
            {
                defaultTemplate.CreateChild( childConfigurationSection );
            }
        }

        Log.Debug( "Templates loaded." );
    }

    /// <summary>
    ///     Method used to force instantiation of this static class and load Sanoid.net's configuration from various sources.
    /// </summary>
    public static void Initialize( )
    {
        // Global configuration initialization
        GetBaseConfiguration( JsonConfigurationSections.RootConfiguration );

        // Template configuration initialization
        Log.Debug( "Initializing template configuration from Sanoid.json#/Templates" );
        // First, find the default template
        JsonConfigurationSections.RootConfiguration.CheckTemplateSectionExists( "default", out IConfigurationSection defaultTemplateSection );
        defaultTemplateSection.CheckTemplateSnapshotRetentionSectionExists( out IConfigurationSection defaultTemplateSnapshotRetentionSection );
        defaultTemplateSection.CheckDefaultTemplateSnapshotTimingSectionExists( out IConfigurationSection defaultTemplateSnapshotTimingSection );

        BuildTemplateHierarchy( defaultTemplateSection );
        Log.Debug( "Template configuration complete." );

        // Diverging from PERL sanoid a bit, here.
        // We can much more efficiently call zfs list once for everything and just process the strings internally, rather
        // than invoking multiple zfs list processes.
        BuildDatasetHierarchy( Templates[ "default" ] );
        LoadDatasetConfigurations( );
    }

    internal static void GetBaseConfiguration( IConfigurationRoot rootConfiguration )
    {
        Log.Debug( "Initializing root-level configuration from Sanoid.Json#/" );
        CacheDirectory = rootConfiguration[ "SanoidConfigurationCacheDirectory" ] ?? "/var/cache/sanoid";
        ConfigurationPathBase = rootConfiguration[ "SanoidConfigurationPathBase" ] ?? "/etc/sanoid";
        RunDirectory = rootConfiguration[ "SanoidConfigurationRunDirectory" ] ?? "/var/run/sanoid";
        TakeSnapshots = rootConfiguration.GetBoolean( "TakeSnapshots" );
        PruneSnapshots = rootConfiguration.GetBoolean( "PruneSnapshots" );
        Log.Debug( "Root level configuration initialized." );
    }
}
