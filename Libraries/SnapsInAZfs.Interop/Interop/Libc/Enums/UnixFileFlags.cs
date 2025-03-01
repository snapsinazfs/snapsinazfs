// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

// ReSharper disable InconsistentNaming
// ReSharper disable MissingXmlDoc
// ReSharper disable IdentifierTypo

#pragma warning disable CS1591

namespace SnapsInAZfs.Interop.Libc.Enums;

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
