// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Drawing;
using Sanoid.Interop.Zfs.ZfsCommandRunner;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui;

#pragma warning disable CA2000

namespace Sanoid.ConfigConsole;

internal class ConfigConsole
{
    internal static ConcurrentDictionary<string, Dataset> Datasets { get; set; } = new( );
    internal static ConcurrentDictionary<string, Snapshot> Snapshots { get; set; } = new( );
    internal static IZfsCommandRunner CommandRunner { get; set; }
    internal static SanoidSettings Settings { get; set; }

    public static void RunConsoleInterface( SanoidSettings settings, IZfsCommandRunner commandRunner )
    {
        using ( LogManager.SuspendLogging( ) )
        {
            Settings = settings;
            CommandRunner = commandRunner;

            Application.Run<SanoidConfigConsole>( );
            Application.Shutdown();
        }
    }
}
