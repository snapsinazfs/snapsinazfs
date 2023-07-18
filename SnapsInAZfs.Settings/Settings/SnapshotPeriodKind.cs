// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     An enumeration of possible snapshot periods
/// </summary>
public enum SnapshotPeriodKind
{
    /// <summary>
    ///     An un-set value. Should be treated as invalid.
    /// </summary>
    /// <value>0</value>
    NotSet = 0,

    /// <summary>
    ///     Snapshots that are taken according to the "frequently" setting
    /// </summary>
    /// <value>1</value>
    Frequent = 1,
    /// <summary>
    ///     Snapshots that are taken according to the "hourly" setting
    /// </summary>
    /// <value>2</value>
    Hourly = 2,

    /// <summary>
    ///     Snapshots that are taken according to the "daily" setting
    /// </summary>
    /// <value>3</value>
    Daily = 3,

    /// <summary>
    ///     Snapshots that are taken according to the "weekly" setting
    /// </summary>
    /// <value>4</value>
    Weekly = 4,

    /// <summary>
    ///     Snapshots that are taken according to the "monthly" setting
    /// </summary>
    /// <value>5</value>
    Monthly = 5,

    /// <summary>
    ///     Snapshots that are taken according to the "yearly" setting
    /// </summary>
    /// <value>6</value>
    Yearly = 6
}
