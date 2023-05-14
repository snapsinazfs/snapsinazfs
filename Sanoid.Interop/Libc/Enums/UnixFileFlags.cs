// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

// ReSharper disable InconsistentNaming
// ReSharper disable MissingXmlDoc
#pragma warning disable CS1591


namespace Sanoid.Interop.Libc.Enums;

/// <summary>
///     File open flags
/// </summary>
[Flags]
public enum UnixFileFlags
{
    O_WRONLY = 0x1,
    O_CREAT = 0x40,
    O_TRUNC = 0x200,
    O_DIRECTORY = 0x10000
}
