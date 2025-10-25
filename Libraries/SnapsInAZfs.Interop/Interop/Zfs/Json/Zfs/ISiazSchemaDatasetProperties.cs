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

namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

/// <summary>
///   High-level minimal API surface expected for all types representing a ZFS dataset, insofar as SIAZ cares.
/// </summary>
public interface ISiazSchemaDatasetProperties
{
  /// <summary>
  ///   Gets or sets whether the dataset is enabled for processing by SIAZ.
  /// </summary>
  bool Enabled { get; set; }

  DatasetProperty<DateTimeOffset>? LastDailySnapshotTimestamp    { get; set; }
  DatasetProperty<DateTimeOffset>? LastFrequentSnapshotTimestamp { get; set; }
  DatasetProperty<DateTimeOffset>? LastHourlySnapshotTimestamp   { get; set; }
  DatasetProperty<DateTimeOffset>? LastMonthlySnapshotTimestamp  { get; set; }
  DatasetProperty<DateTimeOffset>? LastWeeklySnapshotTimestamp   { get; set; }
  DatasetProperty<DateTimeOffset>? LastYearlySnapshotTimestamp   { get; set; }
  bool                             PruneSnapshots                { get; set; }
  DatasetProperty<string>?         Recursion                     { get; set; }
  DatasetProperty<int>?            RetentionDaily                { get; set; }
  DatasetProperty<int>?            RetentionFrequent             { get; set; }
  DatasetProperty<int>?            RetentionHourly               { get; set; }
  DatasetProperty<int>?            RetentionMonthly              { get; set; }
  DatasetProperty<int>?            RetentionPruneDeferral        { get; set; }
  DatasetProperty<int>?            RetentionWeekly               { get; set; }
  DatasetProperty<int>?            RetentionYearly               { get; set; }
  DateTimeOffset                   SnapshotsChanged              { get; set; }
  DatasetProperty<string>?         SourceSystem                  { get; set; }
  bool                             TakeSnapshots                 { get; set; }
  DatasetProperty<string>?         Template                      { get; set; }
}
