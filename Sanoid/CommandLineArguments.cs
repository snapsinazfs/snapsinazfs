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
        [ArgDefaultValue( false )]
        [ArgDescription( "Verbose output logging" )]
        [ArgShortcut( "v" )]
        [ArgShortcut( "--verbose" )]
        public bool Verbose { get; set; }

        [ArgDescription( "Shows this help" )]
        [ArgShortcut( "-h" )]
        [ArgShortcut( "--help" )]
        [HelpHook]
        public bool Help { get; set; }

        [ArgDefaultValue( false )]
        [ArgDescription( "Debug output logging" )]
        [ArgShortcut( "--debug" )]
        public bool Debug { get; set; }

        [ArgDefaultValue( "/etc/sanoid" )]
        [ArgDescription( "Base configuration directory for sanoid" )]
        [ArgShortcut( "--configdir" )]
        public string ConfigDir { get; set; } = "/etc/sanoid";

        /// <summary>
        /// Stub method to allow PowerArgs to run without handing execution over to PowerArgs
        /// </summary>
        public void Main()
        {
        }
    }
}