// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using NLog.Config;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui;

#pragma warning disable CA2000

namespace Sanoid.ConfigConsole;

internal static class ConfigConsole
{
    internal static IZfsCommandRunner? CommandRunner { get; private set; }
    internal static ConcurrentDictionary<string, SanoidZfsDataset> Datasets { get; } = new( );
    internal static ConcurrentDictionary<string, Snapshot> Snapshots { get; } = new( );
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public static void RunConsoleInterface( IZfsCommandRunner commandRunner )
    {
        Logger.Info( "Config Console requested. \"Console\" logging rule will be suspended until exit" );

        LogManager.Flush( 250 );

        LogLevel? minConsoleLogLevel = null;
        LoggingRule? consoleRule = LogManager.Configuration?.FindRuleByName( "Console" );

        if ( consoleRule != null )
        {
            minConsoleLogLevel = consoleRule.Levels.Min( );
            consoleRule.DisableLoggingForLevels( LogLevel.Trace, LogLevel.Off );
            LogManager.ReconfigExistingLoggers( );
        }

        CommandRunner = commandRunner;

        Application.Run<SanoidConfigConsole>( ErrorHandler );
        Application.Shutdown( );

        if ( consoleRule != null )
        {
            Logger.Info( "Setting \"Console\" logging rule to {0}", minConsoleLogLevel ?? LogLevel.Info );
            consoleRule.EnableLoggingForLevels( minConsoleLogLevel ?? LogLevel.Info, LogLevel.Fatal );
            LogManager.ReconfigExistingLoggers( );
        }

        Logger.Info( "Exited Config Console" );
    }

    private static bool ErrorHandler( Exception arg )
    {
        Logger.Error( arg, "Error encoutered in configuration console" );
        return true;
    }
}
