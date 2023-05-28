// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class ZfsProperty
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ZfsProperty(string propertyNamespace, string propertyName, string propertyValue, string valueSource, string? inheritedFrom = null)
    {
        Namespace = propertyNamespace;
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
        InheritedFrom = inheritedFrom;
    }
    private ZfsProperty(string[] components)
    {
        string[] nameComponents = components[0].Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        switch (nameComponents.Length)
        {
            case 2:
                Namespace = nameComponents[0];
                Name = nameComponents[1];
                break;
            default:
                Namespace = string.Empty;
                Name = components[0];
                break;
        }

        Value = components[1];
        Source = components[2];
        if (components.Length > 3 && components[3].Length >= 16)
        {
            InheritedFrom = components[3][16..];
        }
    }

    public string? InheritedFrom { get; set; }
    public string Source { get; set; }

    public static Dictionary<string, ZfsProperty> DefaultProperties { get; } = new()
    {
        { "sanoid.net:template", new( "sanoid.net","template", "default","sanoid" ) },
        { "sanoid.net:enabled", new("sanoid.net", "enabled", "true","sanoid" ) },
        { "sanoid.net:skipchildren", new("sanoid.net", "skipchildren", "true","sanoid" ) },
        { "sanoid.net:autoprune", new("sanoid.net", "autoprune", "false","sanoid" ) },
        { "sanoid.net:autosnapshot", new("sanoid.net", "autosnapshot", "false","sanoid" ) },
        { "sanoid.net:recursive", new("sanoid.net", "recursive", "false","sanoid" ) },
    };

    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Value { get; set; }

    public static bool TryParse(string value, out ZfsProperty? property)
    {
        property = null;
        try
        {
            property = Parse(value);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return false;
        }
        catch (ArgumentNullException ex)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Gets a <see cref="ZfsProperty" /> parsed from the supplied string
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref propertyName="value" /> is a null, empty, or entirely whitespace string</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the provided property string has less than 3 components separated by a
    ///     tab character.
    /// </exception>
    public static ZfsProperty Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            const string errorString = "Unable to parse ZfsProperty. String must not be null, empty, or whitespace.";
            Logger.Error(errorString);
            throw new ArgumentNullException(nameof(value), errorString);
        }

        string[] components = value.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (components.Length < 3)
        {
            const string errorString = "ZfsProperty value string is invalid.";
            Logger.Error(errorString);
            throw new ArgumentOutOfRangeException(nameof(value), errorString);
        }

        return new(components);
    }
}
