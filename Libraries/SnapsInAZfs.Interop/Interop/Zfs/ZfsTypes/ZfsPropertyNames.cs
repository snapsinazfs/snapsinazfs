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

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static class ZfsPropertyNames
{
  public const   string DatasetLastDailySnapshotTimestamp    = $"{SiazZfsPropNamespace}:lastdailysnapshottimestamp";
  public const   string DatasetLastFrequentSnapshotTimestamp = $"{SiazZfsPropNamespace}:lastfrequentsnapshottimestamp";
  public const   string DatasetLastHourlySnapshotTimestamp   = $"{SiazZfsPropNamespace}:lasthourlysnapshottimestamp";
  public const   string DatasetLastMonthlySnapshotTimestamp  = $"{SiazZfsPropNamespace}:lastmonthlysnapshottimestamp";
  public const   string DatasetLastWeeklySnapshotTimestamp   = $"{SiazZfsPropNamespace}:lastweeklysnapshottimestamp";
  public const   string DatasetLastYearlySnapshotTimestamp   = $"{SiazZfsPropNamespace}:lastyearlysnapshottimestamp";
  public const   string Enabled                              = $"{SiazZfsPropNamespace}:enabled";
  public const   string PruneSnapshots                       = $"{SiazZfsPropNamespace}:prunesnapshots";
  public const   string Recursion                            = $"{SiazZfsPropNamespace}:recursion";
  public const   string ReplicationEnabled                   = $"{SiazReplicationZfsPropNamespace}:enabled";
  public const   string SiazReplicationZfsPropNamespace      = $"{SiazZfsPropNamespace}:replication";
  public const   string SiazRetentionPropNamespace           = $"{SiazZfsPropNamespace}:retention";
  public const   string SiazSnapshotPropNamespace            = $"{SiazZfsPropNamespace}:snapshot";
  public const   string SnapshotPeriod                       = $"{SiazSnapshotPropNamespace}:period";
  public const   string SnapshotRetentionDaily               = $"{SiazRetentionPropNamespace}:daily";
  public const   string SnapshotRetentionFrequent            = $"{SiazRetentionPropNamespace}:frequent";
  public const   string SnapshotRetentionHourly              = $"{SiazRetentionPropNamespace}:hourly";
  public const   string SnapshotRetentionMonthly             = $"{SiazRetentionPropNamespace}:monthly";
  public const   string SnapshotRetentionPruneDeferral       = $"{SiazRetentionPropNamespace}:prunedeferral";
  public const   string SnapshotRetentionWeekly              = $"{SiazRetentionPropNamespace}:weekly";
  public const   string SnapshotRetentionYearly              = $"{SiazRetentionPropNamespace}:yearly";
  public const   string SnapshotTimestamp                    = $"{SiazSnapshotPropNamespace}:timestamp";
  public const   string SourceSystem                         = $"{SiazZfsPropNamespace}:sourcesystem";
  public const   string TakeSnapshots                        = $"{SiazZfsPropNamespace}:takesnapshots";
  public const   string Template                             = $"{SiazZfsPropNamespace}:template";
  internal const string SiazZfsPropNamespace                 = "snapsinazfs.com";
}
