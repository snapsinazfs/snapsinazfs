global using int16_t = System.Int16;
global using int32_t = System.Int32;
global using uint32_t = System.UInt32;
global using uint64_t = System.UInt64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnapsInAZfs.Interop.Zfs.Libzfs_core;

public static partial class libzfs_core
{
    [LibraryImport( "/root/zfs/lib/libzfs_core/libzfs_core_la-libzfs_core.o", EntryPoint = "libzfs_core_init", SetLastError = true )]
    public static partial int libzfs_core_init( );
}

[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4, Size = 24 )]
public struct nvlist_t
{
    int32_t nvl_version;
    uint32_t nvl_nvflag; /* persistent flags */
    uint64_t nvl_priv;   /* ptr to private data if not packed */
    uint32_t nvl_flag;
    int32_t nvl_pad; /* currently not used, for alignment */
}

[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1 )]
public struct nvpair_t
{
    int32_t nvp_size;       /* size of this nvpair */
    int16_t nvp_name_sz;    /* length of name string */
    int16_t nvp_reserve;    /* not used */
    int32_t nvp_value_elem; /* number of elements for array types */
    data_type_t nvp_type;   /* type of value */

    char[] nvp_name; /* name string */
    /* aligned ptr array for string arrays */
    /* aligned array of data for value */
}

public enum data_type_t
{
    DATA_TYPE_DONTCARE = -1,
    DATA_TYPE_UNKNOWN = 0,
    DATA_TYPE_BOOLEAN,
    DATA_TYPE_BYTE,
    DATA_TYPE_INT16,
    DATA_TYPE_UINT16,
    DATA_TYPE_INT32,
    DATA_TYPE_UINT32,
    DATA_TYPE_INT64,
    DATA_TYPE_UINT64,
    DATA_TYPE_STRING,
    DATA_TYPE_BYTE_ARRAY,
    DATA_TYPE_INT16_ARRAY,
    DATA_TYPE_UINT16_ARRAY,
    DATA_TYPE_INT32_ARRAY,
    DATA_TYPE_UINT32_ARRAY,
    DATA_TYPE_INT64_ARRAY,
    DATA_TYPE_UINT64_ARRAY,
    DATA_TYPE_STRING_ARRAY,
    DATA_TYPE_HRTIME,
    DATA_TYPE_NVLIST,
    DATA_TYPE_NVLIST_ARRAY,
    DATA_TYPE_BOOLEAN_VALUE,
    DATA_TYPE_INT8,
    DATA_TYPE_UINT8,
    DATA_TYPE_BOOLEAN_ARRAY,
    DATA_TYPE_INT8_ARRAY,
    DATA_TYPE_UINT8_ARRAY,
    DATA_TYPE_DOUBLE
}
