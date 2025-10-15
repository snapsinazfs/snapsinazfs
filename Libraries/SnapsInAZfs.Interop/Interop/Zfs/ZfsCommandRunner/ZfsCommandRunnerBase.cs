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

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZfsTypes;


public abstract record ZfsCommandRunnerBase : IZfsCommandRunner
{
  [method: SetsRequiredMembers]
  protected ZfsCommandRunnerBase ( [Required] string ZfsPath, [Required] string ZpoolPath )
  {
    ArgumentException.ThrowIfNullOrWhiteSpace ( ZfsPath );
    ArgumentException.ThrowIfNullOrWhiteSpace ( ZpoolPath );

    this.ZfsPath   = ZfsPath;
    this.ZpoolPath = ZpoolPath;

    Logger.Trace ( "DummyZfsCommandRunner created with fake ZFS utilities at {0} and {1}", ZfsPath, ZpoolPath );
  }

  [SetsRequiredMembers]
  protected ZfsCommandRunnerBase ( ) : this ( "", "" ){}

  private static readonly Logger Logger = LogManager.GetCurrentClassLogger ( );
  public required         string ZfsPath   { get; init; }
  public required         string ZpoolPath { get; init; }

  /// <inheritdoc />
  public abstract ZfsCommandRunnerOperationStatus TakeSnapshot ( ZfsRecord ds, SnapshotPeriod period, in DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, FormattingSettings datasetFormattingSettings, out Snapshot? snapshot );

  /// <inheritdoc />
  public abstract Task<ZfsCommandRunnerOperationStatus> DestroySnapshotAsync ( Snapshot snapshot, SnapsInAZfsSettings settings );

  /// <inheritdoc />
  public abstract Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync ( bool dryRun, string zfsPath, SemaphoreSlim taskSemaphore, params IZfsProperty[] properties );

  /// <inheritdoc />
  public abstract Task<ZfsCommandRunnerOperationStatus> SetZfsPropertiesAsync ( bool dryRun, string zfsPath, List<IZfsProperty> properties );

  /// <inheritdoc />
  public abstract Task GetDatasetsAndSnapshotsFromZfsAsync ( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots );

  /// <inheritdoc />
  public abstract IAsyncEnumerable<string> ZpoolExecEnumerator ( string verb, string args );

  /// <inheritdoc />
  public abstract IAsyncEnumerable<string> ZfsExecEnumeratorAsync ( string verb, string args );

  /// <inheritdoc />
  public abstract Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync ( );

  /// <inheritdoc />
  public abstract Task<ZfsCommandRunnerOperationStatus> InheritZfsPropertyAsync ( bool dryRun, string zfsPath, IZfsProperty propertyToInherit );

  /// <inheritdoc />
  public abstract bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync ( SnapsInAZfsSettings settings, string poolName, string[] propertyArray );

  protected async Task CheckAndUpdateLastSnapshotTimesForDatasets ( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets )
  {
    Logger.Trace ( "Checking all dataset last snapshot times" );

    // Do the worst-case allocation, and take it from the pool, to avoid the otherwise significant amount of reallocation building this list often will have.
    // It's a pointer array, so it isn't a big allocation even if there are hundreds of datasets.
    IZfsProperty[] propertiesToSet          = ArrayPool<IZfsProperty>.Shared.Rent ( datasets.Count * 6 );
    int            endIndex            = 0;
    using SemaphoreSlim  propertySetTaskSemaphore = new ( datasets.Count );

    foreach ( ZfsRecord ds in datasets.Values )
    {
      int startIndex      = endIndex;
      int propertiesCount = 0;

      if ( ds.LastFrequentSnapshotTimestamp.Value != ds.LastObservedFrequentSnapshotTimestamp )
      {
        propertiesToSet [ endIndex++ ] = ds.UpdateProperty ( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp, ds.LastObservedFrequentSnapshotTimestamp );
        propertiesCount++;
      }

      if ( ds.LastHourlySnapshotTimestamp.Value != ds.LastObservedHourlySnapshotTimestamp )
      {
        propertiesToSet [ endIndex++ ] = ds.UpdateProperty ( ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp, ds.LastObservedHourlySnapshotTimestamp );
        propertiesCount++;
      }

      if ( ds.LastDailySnapshotTimestamp.Value != ds.LastObservedDailySnapshotTimestamp )
      {
        propertiesToSet [ endIndex++ ] = ds.UpdateProperty ( ZfsPropertyNames.DatasetLastDailySnapshotTimestamp, ds.LastObservedDailySnapshotTimestamp );
        propertiesCount++;
      }

      if ( ds.LastWeeklySnapshotTimestamp.Value != ds.LastObservedWeeklySnapshotTimestamp )
      {
        propertiesToSet [endIndex++] = ds.UpdateProperty (ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp, ds.LastObservedWeeklySnapshotTimestamp);
        propertiesCount++;
      }

      if ( ds.LastMonthlySnapshotTimestamp.Value != ds.LastObservedMonthlySnapshotTimestamp )
      {
        propertiesToSet [endIndex++] = ds.UpdateProperty (ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp, ds.LastObservedMonthlySnapshotTimestamp);
        propertiesCount++;
      }

      if ( ds.LastYearlySnapshotTimestamp.Value != ds.LastObservedYearlySnapshotTimestamp )
      {
        propertiesToSet [endIndex++] = ds.UpdateProperty (ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp, ds.LastObservedYearlySnapshotTimestamp);
        propertiesCount++;
      }

      // ReSharper disable once InvertIf
      if ( propertiesCount > 0 )
      {
        Logger.Debug ( "Timestamps older than latest snapshot for {0} - updating properties", ds.Name );
        _ = SetZfsPropertiesAsync ( settings.DryRun, ds.Name, propertySetTaskSemaphore, propertiesToSet [ startIndex..endIndex ] );
      }
    }

    // Enter the semaphore and wait 100ms at a time until only this line holds the semaphore before we return the array back to the pool.
    await propertySetTaskSemaphore.WaitAsync ( );

    while ( propertySetTaskSemaphore.CurrentCount > 1 )
    {
      await Task.Delay ( 100 );
    }

    propertySetTaskSemaphore.Release ( );
    ArrayPool<IZfsProperty>.Shared.Return ( propertiesToSet );
  }

  /// <summary>
  ///   Performs some basic parse and range checks on properties that are required to be defined on pool roots.
  /// </summary>
  /// <param name="name">The name of the property</param>
  /// <param name="value">The raw string value of the property</param>
  /// <param name="source">The source string of the property</param>
  /// <returns>
  ///   A boolean value indicating if the property passed basic parse and range checks, and was defined as local or inherited.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">If the provided property name is not one of the expected values.</exception>
  protected static bool CheckIfPropertyIsValid ( string name, string value, string source )
  {
    if ( source == "-" )
    {
      return false;
    }

    return name switch
           {
             "type"                                                            => !string.IsNullOrWhiteSpace ( value ) && value is ZfsPropertyValueConstants.FileSystem or ZfsPropertyValueConstants.Volume,
             ZfsPropertyNames.Enabled                              => bool.TryParse ( value, out _ ),
             ZfsPropertyNames.TakeSnapshots                        => bool.TryParse ( value, out _ ),
             ZfsPropertyNames.PruneSnapshots                       => bool.TryParse ( value, out _ ),
             ZfsPropertyNames.Recursion                            => !string.IsNullOrWhiteSpace ( value ) && value is ZfsPropertyValueConstants.SnapsInAZfs or ZfsPropertyValueConstants.ZfsRecursion,
             ZfsPropertyNames.Template                             => !string.IsNullOrWhiteSpace ( value ),
             ZfsPropertyNames.SourceSystem                                     => !string.IsNullOrWhiteSpace ( value ),
             ZfsPropertyNames.SnapshotRetentionFrequent            => int.TryParse ( value, out int intValue )                       && intValue >= 0,
             ZfsPropertyNames.SnapshotRetentionHourly              => int.TryParse ( value, out int intValue )                       && intValue >= 0,
             ZfsPropertyNames.SnapshotRetentionDaily               => int.TryParse ( value, out int intValue )                       && intValue >= 0,
             ZfsPropertyNames.SnapshotRetentionWeekly              => int.TryParse ( value, out int intValue )                       && intValue >= 0,
             ZfsPropertyNames.SnapshotRetentionMonthly             => int.TryParse ( value, out int intValue )                       && intValue >= 0,
             ZfsPropertyNames.SnapshotRetentionYearly              => int.TryParse ( value, out int intValue )                       && intValue >= 0,
             ZfsPropertyNames.SnapshotRetentionPruneDeferral       => int.TryParse ( value, out int intValue )                       && intValue is >= 0 and <= 100,
             ZfsPropertyNames.DatasetLastFrequentSnapshotTimestamp => DateTimeOffset.TryParse ( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
             ZfsPropertyNames.DatasetLastHourlySnapshotTimestamp   => DateTimeOffset.TryParse ( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
             ZfsPropertyNames.DatasetLastDailySnapshotTimestamp    => DateTimeOffset.TryParse ( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
             ZfsPropertyNames.DatasetLastWeeklySnapshotTimestamp   => DateTimeOffset.TryParse ( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
             ZfsPropertyNames.DatasetLastMonthlySnapshotTimestamp  => DateTimeOffset.TryParse ( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
             ZfsPropertyNames.DatasetLastYearlySnapshotTimestamp   => DateTimeOffset.TryParse ( value, out DateTimeOffset dtoValue ) && dtoValue >= DateTimeOffset.UnixEpoch,
             "used"                                                            => long.TryParse ( value, out _ ),
             "available"                                                       => long.TryParse ( value, out _ ),
             _                                                                 => throw new ArgumentOutOfRangeException ( nameof (name) )
           };
  }

  protected async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync ( string zfsGetArgs )
  {
    ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> rootsAndTheirProperties = new ( );

    await foreach ( string zfsGetLine in ZfsExecEnumeratorAsync ( "get", zfsGetArgs ).ConfigureAwait ( true ) )
    {
      string[] lineTokens = zfsGetLine.Split ( '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
      ParseAndValidatePoolRootZfsGetLine ( lineTokens, ref rootsAndTheirProperties );
    }

    return rootsAndTheirProperties;
  }

  /// <summary>
  ///   Iterates over <paramref name="lineProvider" /> and builds a collection of raw objects from the provided values
  /// </summary>
  /// <param name="lineProvider">
  ///   A <see cref="ConfiguredCancelableAsyncEnumerable{T}" /> (<see langword="string" />) that provides text output in the same
  ///   format as <c>zfs get all -Hpr</c>
  /// </param>
  /// <param name="rawObjects">
  ///   The collection of <see cref="RawZfsObject" />s, indexed and sorted by name, this method will build from the output provided
  ///   by
  ///   <paramref name="lineProvider" />
  /// </param>
  protected static async Task GetRawZfsObjectsAsync ( ConfiguredCancelableAsyncEnumerable<string> lineProvider, SortedDictionary<string, RawZfsObject> rawObjects )
  {
    await foreach ( string zfsGetLine in lineProvider )
    {
      string[] lineTokens = zfsGetLine.Split ( '\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries );

      string objectName    = lineTokens [ 0 ];
      string propertyValue = lineTokens [ 2 ];

      if ( !rawObjects.TryGetValue ( objectName, out RawZfsObject? obj ) )
      {
        rawObjects.Add ( objectName, new ( propertyValue ) );
        rawObjects [ objectName ].AddRawProperty ( in lineTokens [ 1 ], in propertyValue, in lineTokens [ 3 ] );

        continue;
      }

      obj.AddRawProperty ( in lineTokens [ 1 ], in propertyValue, in lineTokens [ 3 ] );
    }
  }

  protected static bool ParseAndValidatePoolRootZfsGetLine ( string[] lineTokens, ref ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> rootsAndTheirProperties )
  {
    if ( lineTokens.Length < 4 )
    {
      return false;
    }

    ref string poolName   = ref lineTokens [ 0 ];
    string     propName   = lineTokens [ 1 ];
    string     propValue  = lineTokens [ 2 ];
    string     propSource = lineTokens [ 3 ];
    rootsAndTheirProperties.AddOrUpdate ( poolName, AddNewDatasetWithProperty, AddPropertyToExistingDs );

    return true;

    ConcurrentDictionary<string, bool> AddNewDatasetWithProperty ( string key )
    {
      ConcurrentDictionary<string, bool> newDs = new ( )
                                                 {
                                                   [ propName ] = CheckIfPropertyIsValid ( propName, propValue, propSource )
                                                 };

      return newDs;
    }

    ConcurrentDictionary<string, bool> AddPropertyToExistingDs ( string key, ConcurrentDictionary<string, bool> properties )
    {
      properties [ propName ] = CheckIfPropertyIsValid ( propName, propValue, propSource );

      return properties;
    }
  }

  protected static void ProcessRawObjects ( SortedDictionary<string, RawZfsObject> rawObjects, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
  {
    foreach ( ( string objName, RawZfsObject obj ) in rawObjects )
    {
      switch ( obj.Kind )
      {
        case ZfsPropertyValueConstants.FileSystem:
        case ZfsPropertyValueConstants.Volume:
          obj.ConvertToDatasetAndAddToCollection ( objName, datasets );

          break;

        case ZfsPropertyValueConstants.Snapshot:
          obj.ConvertToSnapshotAndAddToCollections ( objName, datasets, snapshots );

          break;
      }
    }
  }

  public void Deconstruct ( out string ZfsPath, out string ZpoolPath )
  {
    ZfsPath   = this.ZfsPath;
    ZpoolPath = this.ZpoolPath;
  }
}
