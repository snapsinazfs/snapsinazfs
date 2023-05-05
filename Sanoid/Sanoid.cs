using NLog;
using PowerArgs;
using Sanoid;
using Sanoid.Common.Configuration;

ArgAction<CommandLineArguments> argParseReults = Args.InvokeMain<CommandLineArguments>( args );

Logging.ConfigureLogger();

Logger Logger = NLog.LogManager.GetLogger( "Sanoid" );
Logger.Info( Configuration.UseSanoidConfiguration ? "We are using sanoid.conf" : "We are not using sanoid.conf" );

if ( argParseReults.Cancelled )
{
    return 0;
}

Logger.Error( "Not yet implemented." );
Logger.Error( "Please use the Perl-based sanoid/syncoid for now." );
Logger.Error( "This program will now exit with an error (status -1) to prevent accidental usage in scripts." );

return -1;