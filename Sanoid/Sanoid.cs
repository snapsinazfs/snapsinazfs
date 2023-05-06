// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;
using PowerArgs;
using Sanoid;
using Sanoid.Common.Configuration;

Logging.ConfigureLogger();

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

logger.Error( "Not yet implemented." );
logger.Error( "Please use the Perl-based sanoid/syncoid for now." );
logger.Error( "This program will now exit with an error (status -1) to prevent accidental usage in scripts." );

return -1;
