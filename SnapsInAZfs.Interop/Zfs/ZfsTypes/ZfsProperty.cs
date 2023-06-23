// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public class ZfsProperty
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public ZfsProperty( string propertyName, string propertyValue, string valueSource )
    {
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
        IsSnapsInAZfsProperty = propertyName.StartsWith( "snapsinazfs.com:" );
    }

    public static ImmutableDictionary<string, ZfsProperty> DefaultDatasetProperties { get; } = ImmutableDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { ZfsPropertyNames.EnabledPropertyName, new( ZfsPropertyNames.EnabledPropertyName, "false", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.TakeSnapshotsPropertyName, new( ZfsPropertyNames.TakeSnapshotsPropertyName, "false", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.PruneSnapshotsPropertyName, new( ZfsPropertyNames.PruneSnapshotsPropertyName, "false", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.RecursionPropertyName, new( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.TemplatePropertyName, new( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, "0", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, "48", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionDailyPropertyName, new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, "90", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, "0", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, "6", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, "0", ZfsPropertySourceConstants.Local ) },
        { ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, "0", ZfsPropertySourceConstants.Local ) }
    } );

    public static ImmutableSortedDictionary<string, ZfsProperty> DefaultSnapshotProperties { get; } = ImmutableSortedDictionary<string, ZfsProperty>.Empty.AddRange( new Dictionary<string, ZfsProperty>
    {
        { ZfsPropertyNames.SnapshotNamePropertyName, new( ZfsPropertyNames.SnapshotNamePropertyName, ZfsPropertyValueConstants.None, ZfsPropertySourceConstants.SnapsInAZfs ) },
        { ZfsPropertyNames.SnapshotPeriodPropertyName, new( ZfsPropertyNames.SnapshotPeriodPropertyName, ZfsPropertyValueConstants.None, ZfsPropertySourceConstants.SnapsInAZfs ) },
        { ZfsPropertyNames.SnapshotTimestampPropertyName, new( ZfsPropertyNames.SnapshotTimestampPropertyName, UnixEpoch, ZfsPropertySourceConstants.SnapsInAZfs ) }
    } );

    public bool IsSnapsInAZfsProperty { get; }

    [JsonIgnore]
    public bool IsUndefined => IsSnapsInAZfsProperty
                               && ( Value == ZfsPropertyValueConstants.None || Source == ZfsPropertySourceConstants.None );

    public static ImmutableSortedSet<string> KnownDatasetProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        ZfsPropertyNames.EnabledPropertyName,
        ZfsPropertyNames.TakeSnapshotsPropertyName,
        ZfsPropertyNames.PruneSnapshotsPropertyName,
        ZfsPropertyNames.RecursionPropertyName,
        ZfsPropertyNames.TemplatePropertyName,
        ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName,
        ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName,
        ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName,
        ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName,
        ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName,
        ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName,
        ZfsPropertyNames.SnapshotRetentionFrequentPropertyName,
        ZfsPropertyNames.SnapshotRetentionHourlyPropertyName,
        ZfsPropertyNames.SnapshotRetentionDailyPropertyName,
        ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName,
        ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName,
        ZfsPropertyNames.SnapshotRetentionYearlyPropertyName,
        ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName
    } );

    public static ImmutableSortedSet<string> KnownSnapshotProperties { get; } = ImmutableSortedSet<string>.Empty.Union( new[]
    {
        ZfsPropertyNames.SnapshotNamePropertyName,
        ZfsPropertyNames.SnapshotPeriodPropertyName,
        ZfsPropertyNames.SnapshotTimestampPropertyName,
        ZfsPropertyNames.PruneSnapshotsPropertyName
    } );

    public string Name { get; }

    [JsonIgnore]
    public string SetString => $"{Name}={Value}";

    public string Source { get; set; }

    [MaxLength( 8192 )]
    public string Value { get; set; }

    private const string UnixEpoch = "1970-01-01T00:00:00.0000000+00:00";

    public static (bool success, ZfsProperty? prop, string? parent) FromZfsGetLine( string zfsGetLine )
    {
        Logger.Trace( "Using regex to parse new ZfsProperty from {0}", zfsGetLine );
        Regex parseRegex = ZfsPropertyParseRegexes.FullFeatured( );
        MatchCollection matches = parseRegex.Matches( zfsGetLine );
        Match firstMatch = matches[ 0 ];
        GroupCollection groups = firstMatch.Groups;

        // If the regex matched exactly once, and got all 4 of the expected capture groups, this is a good parse.
        if ( firstMatch.Success
             && groups[ "Name" ].Success
             && groups[ "Property" ].Success
             && groups[ "Value" ].Success
             && groups[ "Source" ].Success )
        {
            Logger.Trace( "Match succeeded. Returning new ZfsProperty" );
            return new( true, new( groups[ "Property" ].Value, groups[ "Value" ].Value, groups[ "Source" ].Value ), groups[ "Name" ].Value );
        }

        return ( false, null, null );
    }
}

public readonly record struct ZfsProperty<T>( string Name, T Value, string Source ) : IZfsProperty where T : notnull
{
    /// <summary>
    ///     Gets whether this is a SnapsInAZfs property or not
    /// </summary>
    /// <remarks>Set by constructor, if property name begins with "snapsinazfs.com:"</remarks>
    [JsonIgnore]
    public bool IsSnapsInAZfsProperty { get; } = Name.StartsWith( "snapsinazfs.com:" );

    /// <summary>
    ///     Gets a boolean indicating if this property is a SnapsInAZfs property, is a string, and is equal to "-"
    /// </summary>
    [JsonIgnore]
    public bool IsUndefinedOrDefault => IsSnapsInAZfsProperty && Value is "-";

    /// <summary>
    ///     Gets a string representation of the Value property, in an appropriate form for its type
    /// </summary>
    [JsonIgnore]
    public string ValueString => Value switch
    {
        int intValue => intValue.ToString( ),
        string value => value,
        bool boolValue => boolValue.ToString( ).ToLowerInvariant( ),
        DateTimeOffset dtoValue => dtoValue.ToString( "O" ),
        _ => throw new InvalidOperationException( $"Invalid value type for ZfsProperty {Name} ({typeof( T ).FullName})" )
    };

    [JsonIgnore]
    public bool IsInherited => Source.StartsWith( "inherited" );

    [JsonIgnore]
    public string InheritedFrom => IsInherited ? Source[ 15.. ] : Source;

    [JsonIgnore]
    public bool IsLocal => Source == "local";

    [JsonIgnore]
    public string SetString => $"{Name}={ValueString}";
}
