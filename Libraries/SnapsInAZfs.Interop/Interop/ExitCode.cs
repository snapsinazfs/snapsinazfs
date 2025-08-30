#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

using System.Runtime.InteropServices;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace SnapsInAZfs.Interop;

/// <summary>
///     An <see langword="enum" /> of POSIX-compliant status codes used directly in SIAZ, as well as extra values for -1 and 0, for
///     better static analysis.
/// </summary>
/// <remarks>
///     <para>
///         Not all of these values (notably -1 and 0 at this time) are defined by POSIX, but 0 is used here by the overwhelming
///         convention of meaning no error, and -1 is intended to be a generic unspecified error.
///     </para>
///     <list type="table">
///         <listheader>
///             <description>
///                 Note that the following values are not defined in errno-base.h or errno.h and may be unwise
///                 for general use outside the context of their use in the SnapsInAZfs project:
///             </description>
///         </listheader>
///         <item>
///             <term>
///                 <see cref="GenericError" />
///             </term>
///             <description>
///                 Defined here as -1 (0xFFFFFFFF). Often returned by applications as a generic error, while
///                 also setting LastError.
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
/// </remarks>
public enum ExitCode
{
    /// <summary>
    ///     🤷‍<br />
    ///     Unspecified general non-success result.<br />
    ///     Not actually defined by POSIX.
    /// </summary>
    /// <remarks>
    ///     Generally, a P/Invoke method that returns this has set an error code that should be retrieved by a call to
    ///     <see cref="Marshal.GetLastPInvokeError" /> for further error handling.<br />
    ///     It is entirely possible that some applications may use this status for specific error cases, but that's the
    ///     caller's responsibility to deal with.
    /// </remarks>
    GenericError = -1,

    /// <summary>
    ///     No error. Not actually defined by POSIX , but here for nice output and better static analysis.
    /// </summary>
    /// <remarks>
    ///     Generally, a P/Invoke method that returns this is indicating it executed and exited without a critical error.<br />
    ///     However, it is entirely possible that some applications may have also set LastError, but it is the caller's
    ///     responsibility to deal with that.
    /// </remarks>
    EOK = 0,

    /// <summary>Operation not permitted, access denied, or any number of possible meanings depending on who returned it.</summary>
    /// <remarks>Also used as a generic error result by System.CommandLine.</remarks>
    EPERM = 1,

    /// <summary>An operation was canceled.</summary>
    ECANCELED = 125,

    ///<summary>Inappropriate file type or format</summary>
    EFTYPE = 1079,

    ///<summary>Attribute not found</summary>
    ENOATTR = 1093
}
