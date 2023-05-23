// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Sanoid.Common.Configuration.Snapshots;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Corresponds to the /Formatting/SnapshotNaming section of Sanoid.json
/// </summary>
public class SnapshotNaming
{
    private readonly IConfigurationSection _snapshotNamingConfigurationSection;

    /// <summary>
    /// Creates a new instance of a SnapshotNaming object, for control of Snapshot naming rules, from the specified configuration section
    /// </summary>
    /// <param name="snapshotNamingConfigurationSection">An <see cref="IConfigurationSection"/> containing the items specified in the SnapshotNaming schema definition.</param>
    public SnapshotNaming( IConfigurationSection snapshotNamingConfigurationSection )
    {
        _snapshotNamingConfigurationSection = snapshotNamingConfigurationSection;
    }

    /// <summary>
    ///     Gets the string used to separate components of a <see cref="Snapshot" /> name.
    /// </summary>
    /// <remarks>Default value is "_"</remarks>
    /// <value>A <see langword="string" /> value for use in <see cref="Snapshot" /> names, between components.</value>
    [JsonPropertyName( "ComponentSeparator" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string ComponentSeparator => _snapshotNamingConfigurationSection[ "ComponentSeparator" ] ?? "_";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Daily">daily</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "daily"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Daily" />.
    /// </value>
    [JsonPropertyName( "DailySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string DailySuffix => _snapshotNamingConfigurationSection[ "DailySuffix" ] ?? "daily";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Frequent">frequent</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "frequently"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Frequent" />.
    /// </value>
    [JsonPropertyName( "FrequentSuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string FrequentSuffix => _snapshotNamingConfigurationSection[ "FrequentSuffix" ] ?? "frequently";

    /// <summary>
    ///     Gets the string used as the suffix component of an <see cref="SnapshotPeriod.Hourly">hourly</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "hourly"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Hourly" />.
    /// </value>
    [JsonPropertyName( "HourlySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string HourlySuffix => _snapshotNamingConfigurationSection[ "HourlySuffix" ] ?? "hourly";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Manual">manual</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "yearly"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Manual" />.
    /// </value>
    [JsonPropertyName( "ManualSuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string ManualSuffix => _snapshotNamingConfigurationSection[ "ManualSuffix" ] ?? "manual";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Monthly">monthly</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "monthly"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Monthly" />.
    /// </value>
    [JsonPropertyName( "MonthlySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string MonthlySuffix => _snapshotNamingConfigurationSection[ "MonthlySuffix" ] ?? "monthly";

    /// <summary>
    ///     Gets the string used as the first component of a snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "autosnap"
    /// </remarks>
    /// <value>A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the first component of the name.</value>
    [JsonPropertyName( "Prefix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string Prefix => _snapshotNamingConfigurationSection[ "Prefix" ] ?? "autosnap";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Temporary">temporary</see> snapshot
    ///     name.
    /// </summary>
    /// <remarks>
    ///     Default value is "yearly"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Temporary" />.
    /// </value>
    [JsonPropertyName( "TemporarySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string TemporarySuffix => _snapshotNamingConfigurationSection[ "TemporarySuffix" ] ?? "temporary";

    /// <summary>
    ///     Gets the format string used to create the timestamp component of a snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "yyyy-MM-dd_HH\:mm\:ss"<br />
    ///     See <see cref="DateTimeOffset.ToString(string)" /> documentation for format string details.
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, used as the argument to
    ///     <see cref="DateTimeOffset.ToString(string?)" />.
    /// </value>
    /// <seealso cref="DateTimeOffset.ToString(string)"
    ///     href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#table-of-format-specifiers" />
    [JsonPropertyName( "TimestampFormatString" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string TimestampFormatString => _snapshotNamingConfigurationSection[ "TimestampFormatString" ] ?? "yyyy-MM-dd_HH\\:mm\\:ss";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Weekly">weekly</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "weekly"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Weekly" />.
    /// </value>
    [JsonPropertyName( "WeeklySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string WeeklySuffix => _snapshotNamingConfigurationSection[ "WeeklySuffix" ] ?? "weekly";

    /// <summary>
    ///     Gets the string used as the suffix component of a <see cref="SnapshotPeriod.Yearly">yearly</see> snapshot name.
    /// </summary>
    /// <remarks>
    ///     Default value is "yearly"
    /// </remarks>
    /// <value>
    ///     A <see langword="string" /> value for use in <see cref="Snapshot" /> names, as the final component of the name
    ///     for Snapshots with <see cref="Snapshot.Period" /> = <see cref="SnapshotPeriod.Yearly" />.
    /// </value>
    [JsonPropertyName( "YearlySuffix" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.Never )]
    [JsonRequired]
    public string YearlySuffix => _snapshotNamingConfigurationSection[ "YearlySuffix" ] ?? "yearly";

    /// <summary>
    /// Gets a snapshot name, following the rules in this instance of <see cref="SnapshotNaming"/>
    /// </summary>
    /// <param name="period">Which kind of snapshot we are taking</param>
    /// <param name="timestamp">The timestamp that will be passed to the formatter</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public string GetSnapshotName( SnapshotPeriod period,DateTimeOffset timestamp )
    {
        string everythingButSuffix = $"{Prefix}{ComponentSeparator}{timestamp.ToString( TimestampFormatString )}{ComponentSeparator}";
        return period switch
        {
            SnapshotPeriod.Daily => $"{everythingButSuffix}{DailySuffix}",
            SnapshotPeriod.Monthly => $"{everythingButSuffix}{MonthlySuffix}",
            SnapshotPeriod.Temporary => $"{everythingButSuffix}{TemporarySuffix}",
            SnapshotPeriod.Frequent => $"{everythingButSuffix}{FrequentSuffix}",
            SnapshotPeriod.Hourly => $"{everythingButSuffix}{HourlySuffix}",
            SnapshotPeriod.Weekly => $"{everythingButSuffix}{WeeklySuffix}",
            SnapshotPeriod.Yearly => $"{everythingButSuffix}{YearlySuffix}",
            SnapshotPeriod.Manual => $"{everythingButSuffix}{ManualSuffix}",
            _ => throw new ArgumentOutOfRangeException( nameof( period ), period, null )
        };
    }
}
