using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapsInAZfs.Interop
{
    /// <summary>
    /// Internal class for common string constants.
    /// </summary>
    /// <remarks>In future .net versions, most or all of this will likely go away with improvements to `nameof` to work with namespaces.</remarks>
    internal static class StringConstants
    {
        internal const string SnapsInAZfsNamespace = "SnapsInAZfs";
        internal const string InteropNamespace     = $"{SnapsInAZfsNamespace}.Interop";
        internal const string ZfsNamespace         = $"{InteropNamespace}.Zfs";
        internal const string ZfsTypesNamespace    = $"{ZfsNamespace}.ZfsTypes";
    }
}
