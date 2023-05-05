using JetBrains.Annotations;
using PowerArgs;

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
        [ArgDescription( "Verbose output logging" )]
        [ArgShortcut( "v" )]
        [ArgShortcut( "--verbose" )]
        public bool Verbose { get; set; }

        [ArgDescription( "Shows this help" )]
        [ArgShortcut( "-h" )]
        [ArgShortcut( "--help" )]
        [HelpHook]
        public bool Help { get; set; }

        [ArgDescription( "Forces loading of PERL sanoid's configuration files" )]
        [ArgDescription( "Debug output logging" )]
        [ArgShortcut( "--debug" )]
        public bool Debug { get; set; }

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
        /// Stub method to allow PowerArgs to run without handing execution over to PowerArgs
        /// </summary>
        public void Main()
        {
        }
    }
}