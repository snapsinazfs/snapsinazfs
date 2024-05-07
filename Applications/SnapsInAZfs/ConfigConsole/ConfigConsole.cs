#region MIT LICENSE
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

using System.Collections.Concurrent;
using NLog.Config;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui;
using LogLevel = NLog.LogLevel;
using TemplateConfigurationListItem = SnapsInAZfs.ConfigConsole.TreeNodes.TemplateConfigurationListItem;

#pragma warning disable CA2000

namespace SnapsInAZfs.ConfigConsole;

internal static class ConfigConsole
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    internal static IZfsCommandRunner? CommandRunner { get; private set; }
    internal static ConcurrentDictionary<string, Snapshot> Snapshots { get; } = [];
    // ReSharper disable HeapView.ObjectAllocation
    internal static List<TemplateConfigurationListItem> TemplateListItems { get; } = Program.Settings?.Templates.Select( static kvp => new TemplateConfigurationListItem( kvp.Key, kvp.Value with { }, kvp.Value with { } ) ).ToList( ) ?? [];
    // ReSharper restore HeapView.ObjectAllocation
    internal static readonly ConcurrentDictionary<string, ZfsRecord> BaseDatasets = [];

    /// <summary>
    ///     Suspends console logging and runs the <see cref="SnapsInAZfsConfigConsole">SnapsInAZfs Configuration Console</see>.
    ///     <br />
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

        Application.Init();
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
