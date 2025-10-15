namespace SnapsInAZfs.Interop.Zfs.Json;

/// <summary>
/// A dataset, as output by <c>zfs get -j</c> commands.
/// </summary>
public class Dataset
{
  public string                              name       { get; set; }
  public DatasetType                         type       { get; set; }
  public string                              pool       { get; set; }
  public ulong                               createtxg  { get; set; }
  public Dictionary<string, DatasetProperty> properties { get; set; }
}
