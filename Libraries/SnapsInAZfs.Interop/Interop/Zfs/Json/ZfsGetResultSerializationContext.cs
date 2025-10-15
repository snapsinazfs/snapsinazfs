namespace SnapsInAZfs.Interop.Zfs.Json;

using System.Text.Json.Serialization;

/// <summary>
///   Source generation context type for zfs get command JSON output.
/// </summary>
[JsonSerializable ( typeof( ZfsGetResult ) )]
[JsonSerializable ( typeof( Dictionary<string, Dataset> ) )]
[JsonSerializable ( typeof( Dictionary<string, DatasetProperty> ) )]
[JsonSerializable ( typeof( DatasetType ) )]
[JsonSerializable ( typeof( SourceType ) )]
[JsonSourceGenerationOptions (
                               NumberHandling = JsonNumberHandling.AllowReadingFromString,
                               WriteIndented = true,
                               IndentCharacter = ' ',
                               IndentSize = 2,
                               UseStringEnumConverter = true
                             )]
public partial class ZfsGetResultSerializationContext : JsonSerializerContext
{
}
