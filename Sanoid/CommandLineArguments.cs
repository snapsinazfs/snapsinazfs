// /*
// *  LICENSE:
// *
// *  This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// *  from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// *  project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.
// */

using JetBrains.Annotations;
using NLog;
using PowerArgs;
using Sanoid.Common.Configuration;

namespace Sanoid;

/// <summary>
///     The command line arguments that sanoid can accept
/// </summary>
[ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )]
[UsedImplicitly]
internal class CommandLineArguments
{
    [ArgDescription( "Cache directory for sanoid" )]
    [ArgShortcut( "--cache-dir" )]
    public string? CacheDir { get; set; }

    [ArgDescription( "Base configuration directory for sanoid" )]
    [ArgShortcut( "--configdir" )]
    [ArgShortcut( "--config-dir" )]
    public string? ConfigDir { get; set; }

    [ArgDescription( "Create snapshots and prune expired snapshots. Equivalent to --take-snapshots --prune-snapshots" )]
    [ArgShortcut( "--cron" )]
    public bool Cron { get; set; }

    [ArgDescription( "Debug level output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--debug" )]
    [ArgCantBeCombinedWith( "Verbose|Quiet|ReallyQuiet" )]
    public bool Debug { get; set; }

    [ArgDescription( "Shows this help" )]
    [ArgShortcut( "-h" )]
    [ArgShortcut( "--help" )]
    [HelpHook]
    public bool Help { get; set; }

    [ArgDescription( "Prunes expired snapshots." )]
    [ArgShortcut( "--prune-snapshots" )]
    public bool PruneSnapshots { get; set; }

    [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json for normal usage. WILL WARN BEFORE SETTING IS APPLIED." )]
    [ArgShortcut( "--quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose" )]
    public bool Quiet { get; set; }

    [ArgDescription( "No output logging. Change log level to Off in Sanoid.nlog.json to set for normal usage. Will not warn when used." )]
    [ArgShortcut( "--really-quiet" )]
    [ArgCantBeCombinedWith( "Debug|Verbose" )]
    public bool ReallyQuiet { get; set; }

    [ArgDescription( "Runtime directory for sanoid" )]
    [ArgShortcut( "--run-dir" )]
    public string? RunDir { get; set; }

    [ArgDescription( "Will make sanoid take snapshots, but will not prune unless --prune-snapshots is also specified." )]
    [ArgShortcut( "--take-snapshots" )]
    [ArgCantBeCombinedWith( "ReadOnly" )]
    public bool TakeSnapshots { get; set; }

    [ArgDescription( "Trace level output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "--trace" )]
    [ArgCantBeCombinedWith( "Verbose|Debug|Quiet|ReallyQuiet" )]
    public bool Trace { get; set; }

    [ArgDescription( "Forces loading of PERL sanoid's configuration files" )]
    [ArgShortcut( "--use-sanoid-config" )]
    public bool UseSanoidConfig { get; set; }

    [ArgDescription( "Verbose (Info level) output logging. Change log level in Sanoid.nlog.json for normal usage." )]
    [ArgShortcut( "v" )]
    [ArgShortcut( "--verbose" )]
    [ArgCantBeCombinedWith( "Debug|Quiet|ReallyQuiet" )]
    public bool Verbose { get; set; }

    /// <summary>
    ///     Called by main thread to override configured settings with any arguments passed at the command line.
    /// </summary>
    public void Main()
    {
        if ( Debug )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Debug;
        }

        if ( Verbose )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Info;
        }

        if ( Trace )
        {
            Configuration.DefaultLoggingLevel = LogLevel.Trace;
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
