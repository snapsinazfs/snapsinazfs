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

using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.SnapshotTests;

internal static class SnapshotTestHelpers
{
    internal static Snapshot GetStandardTestSnapshot( SnapshotPeriod period, DateTimeOffset timestamp, string parentName = "testRoot" )
    {
        ZfsRecord parent = ZfsRecordTestHelpers.GetNewTestRootFileSystem( parentName );
        return GetStandardTestSnapshotForParent( period, timestamp, parent );
    }

    internal static Snapshot GetStandardTestSnapshotForParent( SnapshotPeriod period, DateTimeOffset timestamp, ZfsRecord parent )
    {
#pragma warning disable CA2000
        return parent.AddSnapshot( new( $"{parent.Name}@autosnap_{timestamp:s}_{period}", in period.Kind, in parent.SourceSystem, in timestamp, parent ) );
#pragma warning restore CA2000
    }
}
