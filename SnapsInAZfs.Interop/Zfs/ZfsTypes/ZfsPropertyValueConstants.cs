// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyValueConstants
{
    public const string Default = "default";
    public const string None = "-";
    public const string SnapsInAZfs = "siaz";
    public const string ZfsRecursion = "zfs";
    public const string FileSystem = "filesystem";
    public const string Volume = "volume";
    public const string Snapshot = "snapshot";

    public static readonly Dictionary<string, (int Min, int Max)> IntPropertyRanges = new( )
    {
        { ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionDailyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, new( 0, int.MaxValue ) },
        { ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, new( 0, 100 ) }
    };
}
