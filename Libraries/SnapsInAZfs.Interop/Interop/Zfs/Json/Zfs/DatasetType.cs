namespace SnapsInAZfs.Interop.Zfs.Json.Zfs;

using System.Diagnostics.CodeAnalysis;

[Flags]
[SuppressMessage ( "ReSharper", "InconsistentNaming", Justification = "Same casing as actual output, to make source gen happy." )]
public enum DatasetType
{
  FILESYSTEM = 1,
  VOLUME     = 2,
  SNAPSHOT   = 4,
  BOOKMARK   = 8,
  ALL        = FILESYSTEM | VOLUME | SNAPSHOT | BOOKMARK
}
