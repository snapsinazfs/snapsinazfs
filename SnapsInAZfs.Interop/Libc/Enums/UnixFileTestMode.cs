// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace SnapsInAZfs.Interop.Libc.Enums;

/// <summary>
///     An enumeration of file modes, in standard unix octal form, for a specific component of a file mode
/// </summary>
/// <remarks>Used by functions such as <see cref="NativeMethods.EuidAccess" /></remarks>
[Flags]
public enum UnixFileTestMode
{
    /// <summary>The file exists</summary>
    /// <remarks>Equivalent to F_OK in unistd.h</remarks>
    Exists = 0,

    /// <summary>Allowed to execute the file</summary>
    /// <remarks>Equivalent to X_OK in unistd.h</remarks>
    Execute = 1,

    /// <summary>Allowed to write to the file</summary>
    /// <remarks>Equivalent to W_OK in unistd.h</remarks>
    Write = 2,

    /// <summary>Allowed to read the file</summary>
    /// <remarks>Equivalent to R_OK in unistd.h</remarks>
    Read = 4
}
