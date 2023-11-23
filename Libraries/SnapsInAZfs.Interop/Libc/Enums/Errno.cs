// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace SnapsInAZfs.Interop.Libc.Enums;

/// <summary>
///     An <see langword="enum" /> of POSIX-compliant status codes, as defined in
///     <see href="https://raw.githubusercontent.com/torvalds/linux/v6.1/include/uapi/asm-generic/errno-base.h">errno-base.h</see>
///     and <see href="https://github.com/torvalds/linux/raw/v6.1/include/uapi/asm-generic/errno.h">errno.h</see> of the
///     <see href="https://github.com/torvalds/linux/tree/830b3c68c1fb1e9176028d02ef86f3cf76aa2476/include">
///         Linux kernel version 6.1 headers
///     </see>
///     , combined together, as well as extra values for -1 and 0, for better static analysis
/// </summary>
/// <remarks>
///     <list type="table">
///         <listheader>
///             <description>
///                 Note that the following values are not defined in errno-base.h or errno.h and may be unwise
///                 for general use outside of the context of their use in the SnapsInAZfs project:
///             </description>
///         </listheader>
///         <item>
///             <term>
///                 <see cref="GenericError" />
///             </term>
///             <description>
///                 Defined here as -1 (0xFFFFFFFF). Often returned by applications as a generic error, while
///                 also setting LastError
///             </description>
///         </item>
///         <item>
///             <term>
///                 <see cref="EOK" />
///             </term>
///             <description>
///                 Defined here as 0 (0x00000000). POSIX-compliant applications return 0 on success, though some
///                 applications may also set LastError, anyway.
///             </description>
///         </item>
///     </list>
///     While this code is free for use by the terms of <see href="https://www.gnu.org/licenses/gpl-3.0.txt">GPLv3</see>, I
///     certainly don't recommend using it elsewhere without due diligence in ensuring the values returned by an
///     application are known and expected to correspond to these enumeration values, or you can expect
///     <see cref="InvalidCastException" />s and other issues.
/// </remarks>
// General recommendation for types used with P/Invoke is to maintain the native library's naming conventions, so
// let's silence ReSharper against the various ways this type violates recommended C# naming conventions.
// Also, a lot of values aren't used, but I'm leaving them here, so silencing the unused member warning, too.
[SuppressMessage( "ReSharper", "InconsistentNaming", Justification = "Following recommendations for types used with P/Invoke" )]
public enum Errno
{
    /// <summary>
    ///     Unspecified status indicating error. Not actually defined in errno.h, but here for nice output and better
    ///     static analysis.
    /// </summary>
    /// <remarks>
    ///     Generally, a P/Invoke method that returns this has set an error code that should be retrieved by a call to
    ///     <see cref="Marshal.GetLastPInvokeError" /> for further error handling.<br />
    ///     It is entirely possible that some applications may use this status for specific error cases, but that's the
    ///     caller's responsibility to deal with.
    /// </remarks>
    GenericError = -1,

    /// <summary>No error. Not actually defined in errno.h, but here for nice output and better static analysis.</summary>
    /// <remarks>
    ///     Generally, a P/Invoke method that returns this is indicating it executed and exited without a critical error.<br />
    ///     However, it is entirely possible that some applications may have also set LastError, but it is the caller's
    ///     responsibility to deal with that.
    /// </remarks>
    EOK = 0,

    /// <summary>Operation not permitted</summary>
    EPERM = 1,

    /// <summary>No such file or directory</summary>
    ENOENT = 2,

    /// <summary>No such process</summary>
    ESRCH = 3,

    ///<summary>Interrupted system call</summary>
    EINTR = 4,

    ///<summary>I/O error</summary>
    EIO = 5,

    ///<summary>No such device or address</summary>
    ENXIO = 6,

    ///<summary>Arg list too long</summary>
    E2BIG = 7,

    ///<summary>Exec format error</summary>
    ENOEXEC = 8,

    ///<summary>Bad file number</summary>
    EBADF = 9,

    ///<summary>No child processes</summary>
    ECHILD = 10,

    ///<summary>Try again</summary>
    EAGAIN = 11,

    ///<summary>Out of memory</summary>
    ENOMEM = 12,

    ///<summary>Permission denied</summary>
    EACCES = 13,

    ///<summary>Bad address</summary>
    EFAULT = 14,

    ///<summary>Block device required</summary>
    ENOTBLK = 15,

    ///<summary>Device or resource busy</summary>
    EBUSY = 16,

    ///<summary>File exists</summary>
    EEXIST = 17,

    ///<summary>Cross-device link</summary>
    EXDEV = 18,

    ///<summary>No such device</summary>
    ENODEV = 19,

    ///<summary>Not a directory</summary>
    ENOTDIR = 20,

    ///<summary>Is a directory</summary>
    EISDIR = 21,

    ///<summary>Invalid argument</summary>
    EINVAL = 22,

    ///<summary>File table overflow</summary>
    ENFILE = 23,

    ///<summary>Too many open files</summary>
    EMFILE = 24,

    ///<summary>Not a typewriter</summary>
    ENOTTY = 25,

    ///<summary>Text file busy</summary>
    ETXTBSY = 26,

    ///<summary>No space left on device</summary>
    ENOSPC = 28,

    ///<summary>Illegal seek</summary>
    ESPIPE = 29,

    ///<summary>Read-only file system</summary>
    EROFS = 30,

    ///<summary>Too many links</summary>
    EMLINK = 31,

    ///<summary>Broken pipe</summary>
    EPIPE = 32,

    ///<summary>Math argument out of domain of func</summary>
    EDOM = 33,

    ///<summary>Math result not representable</summary>
    ERANGE = 34,

    ///<summary>Resource deadlock would occur</summary>
    EDEADLK = 35,

    ///<summary>File name too long</summary>
    ENAMETOOLONG = 36,

    ///<summary>No record locks available</summary>
    ENOLCK = 37,

    /// <summary>
    ///     Function not implemented
    /// </summary>
    ENOSYS = 38,

    ///<summary>Directory not empty</summary>
    ENOTEMPTY = 39,

    ///<summary>Too many symbolic links encountered</summary>
    ELOOP = 40,
    EWOULDBLOCK = EAGAIN, // Operation would block

    ///<summary>No message of desired type</summary>
    ENOMSG = 42,

    ///<summary>Identifier removed</summary>
    EIDRM = 43,

    ///<summary>Channel number out of range</summary>
    ECHRNG = 44,

    ///<summary>Level 2 not synchronized</summary>
    EL2NSYNC = 45,

    ///<summary>Level 3 halted</summary>
    EL3HLT = 46,

    ///<summary>Level 3 reset</summary>
    EL3RST = 47,

    ///<summary>Link number out of range</summary>
    ELNRNG = 48,

    ///<summary>Protocol driver not attached</summary>
    EUNATCH = 49,

    ///<summary>No CSI structure available</summary>
    ENOCSI = 50,

    ///<summary>Level 2 halted</summary>
    EL2HLT = 51,

    ///<summary>Invalid exchange</summary>
    EBADE = 52,

    ///<summary>Invalid request descriptor</summary>
    EBADR = 53,

    ///<summary>Exchange full</summary>
    EXFULL = 54,

    ///<summary>No anode</summary>
    ENOANO = 55,

    ///<summary>Invalid request code</summary>
    EBADRQC = 56,

    ///<summary>Invalid slot</summary>
    EBADSLT = 57,
    EDEADLOCK = EDEADLK,

    ///<summary>Bad font file format</summary>
    EBFONT = 59,

    ///<summary>Device not a stream</summary>
    ENOSTR = 60,

    ///<summary>No data available</summary>
    ENODATA = 61,

    ///<summary>Timer expired</summary>
    ETIME = 62,

    ///<summary>Out of streams resources</summary>
    ENOSR = 63,

    ///<summary>Machine is not on the network</summary>
    ENONET = 64,

    ///<summary>Package not installed</summary>
    ENOPKG = 65,

    ///<summary>Object is remote</summary>
    EREMOTE = 66,

    ///<summary>Link has been severed</summary>
    ENOLINK = 67,

    ///<summary>Advertise error</summary>
    EADV = 68,

    ///<summary>Srmount error</summary>
    ESRMNT = 69,

    ///<summary>Communication error on send</summary>
    ECOMM = 70,

    ///<summary>Protocol error</summary>
    EPROTO = 71,

    ///<summary>Multihop attempted</summary>
    EMULTIHOP = 72,

    ///<summary>RFS specific error</summary>
    EDOTDOT = 73,

    ///<summary>Not a data message</summary>
    EBADMSG = 74,

    ///<summary>Value too large for defined data type</summary>
    EOVERFLOW = 75,

    ///<summary>Name not unique on network</summary>
    ENOTUNIQ = 76,

    ///<summary>File descriptor in bad state</summary>
    EBADFD = 77,

    ///<summary>Remote address changed</summary>
    EREMCHG = 78,

    ///<summary>Can not access a needed shared library</summary>
    ELIBACC = 79,

    ///<summary>Accessing a corrupted shared library</summary>
    ELIBBAD = 80,

    ///<summary>.lib section in a.out corrupted</summary>
    ELIBSCN = 81,

    ///<summary>Attempting to link in too many shared libraries</summary>
    ELIBMAX = 82,

    ///<summary>Cannot exec a shared library directly</summary>
    ELIBEXEC = 83,

    ///<summary>Illegal byte sequence</summary>
    EILSEQ = 84,

    ///<summary>Interrupted system call should be restarted</summary>
    ERESTART = 85,

    ///<summary>Streams pipe error</summary>
    ESTRPIPE = 86,

    ///<summary>Too many users</summary>
    EUSERS = 87,

    ///<summary>Socket operation on non-socket</summary>
    ENOTSOCK = 88,

    ///<summary>Destination address required</summary>
    EDESTADDRREQ = 89,

    ///<summary>Message too long</summary>
    EMSGSIZE = 90,

    ///<summary>Protocol wrong type for socket</summary>
    EPROTOTYPE = 91,

    ///<summary>Protocol not available</summary>
    ENOPROTOOPT = 92,

    ///<summary>Protocol not supported</summary>
    EPROTONOSUPPORT = 93,

    ///<summary>Socket type not supported</summary>
    ESOCKTNOSUPPORT = 94,

    ///<summary>Operation not supported on transport endpoint</summary>
    EOPNOTSUPP = 95,

    ///<summary>Protocol family not supported</summary>
    EPFNOSUPPORT = 96,

    ///<summary>Address family not supported by protocol</summary>
    EAFNOSUPPORT = 97,

    ///<summary>Address already in use</summary>
    EADDRINUSE = 98,

    ///<summary>Cannot assign requested address</summary>
    EADDRNOTAVAIL = 99,

    ///<summary>Network is down</summary>
    ENETDOWN = 100,

    ///<summary>Network is unreachable</summary>
    ENETUNREACH = 101,

    ///<summary>Network dropped connection because of reset</summary>
    ENETRESET = 102,

    ///<summary>Software caused connection abort</summary>
    ECONNABORTED = 103,

    ///<summary>Connection reset by peer</summary>
    ECONNRESET = 104,

    ///<summary>No buffer space available</summary>
    ENOBUFS = 105,

    ///<summary>Transport endpoint is already connected</summary>
    EISCONN = 106,

    ///<summary>Transport endpoint is not connected</summary>
    ENOTCONN = 107,

    ///<summary>Cannot send after transport endpoint shutdown</summary>
    ESHUTDOWN = 108,

    ///<summary>Too many references: cannot splice</summary>
    ETOOMANYREFS = 109,

    ///<summary>Connection timed out</summary>
    ETIMEDOUT = 110,

    ///<summary>Connection refused</summary>
    ECONNREFUSED = 111,

    ///<summary>Host is down</summary>
    EHOSTDOWN = 112,

    ///<summary>No route to host</summary>
    EHOSTUNREACH = 113,

    ///<summary>Operation already in progress</summary>
    EALREADY = 114,

    ///<summary>Operation now in progress</summary>
    EINPROGRESS = 115,

    ///<summary>Stale NFS file handle</summary>
    ESTALE = 116,

    ///<summary>Structure needs cleaning</summary>
    EUCLEAN = 117,

    ///<summary>Not a XENIX named type file</summary>
    ENOTNAM = 118,

    ///<summary>No XENIX semaphores available</summary>
    ENAVAIL = 119,

    ///<summary>Is a named type file</summary>
    EISNAM = 120,

    ///<summary>Remote I/O error</summary>
    EREMOTEIO = 121,

    ///<summary>Quota exceeded</summary>
    EDQUOT = 122,

    ///<summary>No medium found</summary>
    ENOMEDIUM = 123,

    ///<summary>Wrong medium type</summary>
    EMEDIUMTYPE = 124,
    ECANCELED = 125,
    ENOKEY = 126,
    EKEYEXPIRED = 127,
    EKEYREVOKED = 128,
    EKEYREJECTED = 129,
    EOWNERDEAD = 130,

    ///<summary>OS X-specific values: OS X value + 1000</summary>
    ENOTRECOVERABLE = 131,

    ///<summary>Too many processes</summary>
    EPROCLIM = 1067,

    ///<summary>RPC struct is bad</summary>
    EBADRPC = 1072,

    ///<summary>RPC version wrong</summary>
    ERPCMISMATCH = 1073,

    ///<summary>RPC prog. not avail</summary>
    EPROGUNAVAIL = 1074,

    ///<summary>Program version wrong</summary>
    EPROGMISMATCH = 1075,

    ///<summary>Bad procedure for program</summary>
    EPROCUNAVAIL = 1076,

    ///<summary>Inappropriate file type or format</summary>
    EFTYPE = 1079,

    ///<summary>Authentication error</summary>
    EAUTH = 1080,

    ///<summary>Need authenticator</summary>
    ENEEDAUTH = 1081,

    ///<summary>Device power is off</summary>
    EPWROFF = 1082,

    ///<summary>Device error, e.g. paper out</summary>
    EDEVERR = 1083,

    ///<summary>Bad executable</summary>
    EBADEXEC = 1085,

    ///<summary>Bad CPU type in executable</summary>
    EBADARCH = 1086,

    ///<summary>Shared library version mismatch</summary>
    ESHLIBVERS = 1087,

    ///<summary>Malformed Macho file</summary>
    EBADMACHO = 1088,

    ///<summary>Attribute not found</summary>
    ENOATTR = 1093,
    ENOPOLICY = 1103 // No such policy registered
}
