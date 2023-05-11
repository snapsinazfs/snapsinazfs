// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Microsoft.Extensions.Configuration;
using NLog;
using PowerArgs;
using Sanoid;
using Sanoid.Common.Configuration;
using Sanoid.Common.Posix;

Logging.ConfigureLogger();

// Note that logging will be at whatever level is defined in Sanoid.nlog.json until configuration is initialized, regardless of command-line parameters.
Logger logger = LogManager.GetCurrentClassLogger();

ArgAction<CommandLineArguments> argParseReults = Args.InvokeMain<CommandLineArguments>( args );

//If PowerArgs cancelled the parser invocation, either it handled the help method or it encountered an error.
//Either way, exit now.
if ( argParseReults.Cancelled )
{
    return 0;
}

//Version output handled by the PowerArgs parser invocation, to avoid unnecessary further setup.
//Can just exit now without doing anything else.
if ( argParseReults.Args.Version )
{
    return 0;
}

logger.Fatal( "Not yet implemented." );
logger.Fatal( "Please use the Perl-based sanoid/syncoid for now." );
logger.Fatal( "This program will now exit with an error (status 38 - ENOSYS) to prevent accidental usage in scripts." );

return (int)Errno.ENOSYS;
