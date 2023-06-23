// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using NLog.Config;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui;

#pragma warning disable CA2000

namespace SnapsInAZfs.ConfigConsole;

internal static class ConfigConsole
{
    internal static IZfsCommandRunner? CommandRunner { get; private set; }
    internal static ConcurrentDictionary<string, Snapshot> Snapshots { get; } = new( );
    internal static List<TemplateConfigurationListItem> TemplateListItems { get; } = Program.Settings?.Templates.Select( kvp => new TemplateConfigurationListItem( kvp.Key, kvp.Value with { }, kvp.Value with { } ) ).ToList( ) ?? new( );
    internal static readonly ConcurrentDictionary<string, SnapsInAZfsZfsDataset> BaseDatasets = new( );
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <summary>
    ///     Suspends console logging and runs the <see cref="SnapsInAZfsConfigConsole">Sanoid.net Configuration Console</see>.<br />
    ///     Resumes console logging after shutdown of the TUI.
    /// </summary>
    /// <param name="commandRunner">An <see cref="IZfsCommandRunner" /> to use when performing ZFS operations</param>
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

        Application.Run<SnapsInAZfsConfigConsole>( ErrorHandler );
        Application.Shutdown( );

        if ( consoleRule != null )
        {
            Logger.Info( "Setting \"Console\" logging rule to {0}", minConsoleLogLevel ?? LogLevel.Info );
            consoleRule.EnableLoggingForLevels( minConsoleLogLevel ?? LogLevel.Info, LogLevel.Fatal );
            LogManager.ReconfigExistingLoggers( );
        }

        Logger.Info( "Exited Config Console" );
    }

    /// <summary>
    ///     An error handler function that the <see cref="Application" /> calls for some cases of unhandled exceptions within
    ///     the currently running <see href="https://github.com/gui-cs/Terminal.Gui">Terminal.Gui</see>
    ///     <see href="https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui/Terminal.Gui.Application.html">Application</see>,
    ///     if passed in a call to <see cref="Application.Run(Func{System.Exception,bool})" />
    /// </summary>
    /// <param name="ex"></param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether <see cref="Exception" /> <paramref name="ex" /> should be
    ///     swallowed (<see langword="true" />) or re-thrown (<see langword="false" />)
    /// </returns>
    private static bool ErrorHandler( Exception ex )
    {
        Logger.Error( ex, "Unhandled exception encoutered in configuration console. Please report this" );
        return true;
    }
}
