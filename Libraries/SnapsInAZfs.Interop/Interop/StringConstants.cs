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

namespace SnapsInAZfs.Interop;

/// <summary>
///     Internal class for common string constants.
/// </summary>
/// <remarks>
///     In future .net versions, most or all of this will likely go away with improvements to `nameof` to work with namespaces.
/// </remarks>
internal static class StringConstants
{
    internal const string InteropNamespace     = $"{SnapsInAZfsNamespace}.Interop";
    internal const string SnapsInAZfsNamespace = "SnapsInAZfs";
    internal const string ZfsNamespace         = $"{InteropNamespace}.Zfs";
    internal const string ZfsTypesNamespace    = $"{ZfsNamespace}.ZfsTypes";
}
