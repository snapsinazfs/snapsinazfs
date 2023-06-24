// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Text.RegularExpressions;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public static partial class ZfsPropertyParseRegexes
{
    /// <summary>
    ///     Get a generated Regex that is designed specifically to get filesystems and volumes, and CANNOT parse snapshot names
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex( "^(?<Name>(?<Dataset>[A-Za-z][A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-](?:/[A-Za-z0-9_.:-][A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]*?)*))\t(?<Property>type|snapsinazfs\\.com:[a-z0-9:_.-]+)\t(?<Value>[a-zA-Z0-9@:. _+-]+)\t(?<Source>(?<UndefinedSource>-)|(?<DefaultSource>default)|(?<LocalSource>local)|inherited from (?<InheritedSource>[A-Za-z0-9_.: @/-]+))" )]
    public static partial Regex DatasetsAllLevelsNoSnapshots( );

    [GeneratedRegex( "^(?<Name>(?<Dataset>[A-Za-z][A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-](?:/[A-Za-z0-9_.:-][A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]*?)*)(?<Snapshot>@[A-Za-z0-9][A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]*?)?)\t(?<Property>type|snapsinazfs\\.com:[a-z0-9:_.-]+)\t(?<Value>[a-zA-Z0-9@:. _+-]+)\t(?<Source>(?<UndefinedSource>-)|(?<DefaultSource>default)|(?<LocalSource>local)|inherited from (?<InheritedSource>[A-Za-z0-9_.: @/-]+))" )]
    public static partial Regex FullFeatured( );

    /// <summary>
    ///     Get a generated Regex that is designed specifically for the case of parsing pool root properties (zfs get all -d 0)
    /// </summary>
    /// <returns>
    ///     Does not parse dataset paths below level 0 (pool roots), snapshots, or inheritance values other than local,
    ///     default, and none (-).
    /// </returns>
    [GeneratedRegex( "^(?<Name>(?<Dataset>[A-Za-z][A-Za-z0-9_.: -]*?[A-Za-z0-9_.:-]))\t(?<Property>type|snapsinazfs\\.com:[a-z0-9:_.-]+)\t(?<Value>[a-zA-Z0-9@:. _+-]+)\t(?<Source>(?<UndefinedSource>-)|(?<DefaultSource>default)|(?<LocalSource>local))" )]
    public static partial Regex PoolRoots( );
}
