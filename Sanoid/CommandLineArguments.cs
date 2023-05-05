using JetBrains.Annotations;
using NLog;
using PowerArgs;
using Sanoid.Common.Configuration;

namespace Sanoid
{
    /// <summary>
    /// The command line arguments that sanoid can accept
    /// </summary>
    [ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
    [UsedImplicitly]
    internal class CommandLineArguments
    {
        [ArgDescription( "Verbose (Info level) output logging. Change log level in Sanoid.nlog.json for normal usage." )]
        [ArgShortcut( "v" )]
        [ArgShortcut( "--verbose" )]
        [ArgCantBeCombinedWith( expression: "Debug|Quiet|ReallyQuiet" )]
        public bool Verbose { get; set; }

        [ArgDescription( "Debug level output logging. Change log level in Sanoid.nlog.json for normal usage." )]
        [ArgShortcut( "--debug" )]
        [ArgCantBeCombinedWith( expression: "Verbose|Quiet|ReallyQuiet" )]
        public bool Debug { get; set; }

        [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json for normal usage. WILL WARN BEFORE SETTING IS APPLIED." )]
        [ArgShortcut( "--quiet" )]
        [ArgCantBeCombinedWith( expression: "Debug|Verbose" )]
        public bool Quiet { get; set; }

        [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json to set for normal usage. Will not warn when used." )]
        [ArgShortcut( "--really-quiet" )]
        [ArgCantBeCombinedWith( expression: "Debug|Verbose" )]
        public bool ReallyQuiet { get; set; }

        [ArgDescription( "Shows this help" )]
        [ArgShortcut( "-h" )]
        [ArgShortcut( "--help" )]
        [HelpHook]
        public bool Help { get; set; }

        [ArgDescription( "Forces loading of PERL sanoid's configuration files" )]
        [ArgShortcut( "--use-sanoid-config" )]
        public bool UseSanoidConfig { get; set; }

        [ArgDescription( "Base configuration directory for sanoid" )]
        [ArgShortcut( "--configdir" )]
        [ArgShortcut( "--config-dir" )]
        public string? ConfigDir { get; set; }

        [ArgDescription( "Cache directory for sanoid" )]
        [ArgShortcut( "--cache-dir" )]
        public string? CacheDir { get; set; }

        [ArgDescription( "Runtime directory for sanoid" )]
        [ArgShortcut( "--run-dir" )]
        public string? RunDir { get; set; }

        /// <summary>
        /// Called by main thread to override configured settings with any arguments passed at the command line.
        /// </summary>
        public void Main( )
        {
            if ( Debug )
            {
                Configuration.DefaultLoggingLevel = LogLevel.Debug;
            }
            if ( Verbose )
            {
                Configuration.DefaultLoggingLevel = LogLevel.Info;
            }
            if ( Quiet )
            {
                Configuration.DefaultLoggingLevel = LogLevel.Off;
            }
            if ( ReallyQuiet )
            {
                LogManager.Configuration!.LoggingRules.ForEach( rule => rule.SetLoggingLevels( LogLevel.Off, LogLevel.Off ) );
            }
            if ( UseSanoidConfig )
            {
                Configuration.UseSanoidConfiguration = true;
            }
            if ( ConfigDir is not null )
            {
                Configuration.SanoidConfigurationPathBase = ConfigDir;
            }
            if ( CacheDir is not null )
            {
                Configuration.SanoidConfigurationCacheDirectory = CacheDir;
            }
            if ( RunDir is not null )
            {
                Configuration.SanoidConfigurationRunDirectory = RunDir;
            }
        }
    }
}