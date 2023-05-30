// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;
using Sanoid.Interop.Zfs.ZfsTypes.Validation;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public abstract class ZfsObjectBase : IZfsObject
{
    /// <summary>
    ///     Creates a new <see cref="ZfsObjectBase" /> with the specified name and kind
    /// </summary>
    /// <param name="name">The name of the new <see cref="ZfsObjectBase" /></param>
    /// <param name="kind">The <see cref="ZfsObjectKind" /> of object to create</param>
    /// <param name="validateName">
    ///     If true, the constructor will perform name validation for the type of object being created.
    /// </param>
    /// <param name="nameValidatorRegex">
    ///     An optional <see cref="Regex" />. If left null, uses a Regex from <see cref="ZfsIdentifierRegexes" /> based on
    ///     <paramref name="kind" />
    /// </param>
    protected internal ZfsObjectBase( string name, ZfsObjectKind kind, Regex? nameValidatorRegex = null, bool validateName = false )
    {
        Logger.Debug( "Creating new ZfsObjectBase {0} of kind {1}", name, kind );
        ZfsKind = kind;
        NameValidatorRegex = nameValidatorRegex ?? kind switch
        {
            ZfsObjectKind.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified for ZfsIdentifierValidator." )
        };

        if ( validateName )
        {
            Logger.Debug( "Name validation requested for new ZfsObjectBase" );
            if ( !ValidateName( name ) )
            {
                string? errorMessage = $"Invalid name specified for a new {ZfsKind} with validateName=true";
                Logger.Error( errorMessage );
                throw new ArgumentOutOfRangeException( nameof( name ), errorMessage );
            }
        }

        Name = name;
        Properties = new( );
    }

    public ZfsProperty? this[ string key ]
    {
        get
        {
            Logger.Debug("Trying to get property {0} from {1} {2}",key, ZfsKind, Name);
            bool gotValue = Properties.TryGetValue( key, out ZfsProperty? prop );
            if ( gotValue )
            {
                Logger.Debug( "Got property {0}({3}) from {1} {2}", key, ZfsKind, Name, prop!.Value );
            }
            else
            {
                Logger.Debug( "Property {0} not found in {1} {2}", key, ZfsKind, Name );
            }
            return prop;
        }
        set
        {
            if ( value is null )
            {
                Logger.Debug( "Removing property {0} from {1} {2}", key, ZfsKind, Name );
                Properties.TryRemove( key, out ZfsProperty? prop );
                return;
            }

            Logger.Debug( "Setting property {0}({3}) from {1} {2}", key, ZfsKind, Name, value.Value );
            Properties[ key ] = value;
        }
    }

    [JsonIgnore]
    internal Regex NameValidatorRegex { get; }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ZfsObjectKind ZfsKind { get; }

    /// <inheritdoc />
    public bool HasProperty( string propertyName )
    {
        return Properties.ContainsKey( propertyName );
    }

    /// <inheritdoc />
    public ConcurrentDictionary<string, ZfsProperty> Properties { get; }

    protected internal bool ValidateName( string name )
    {
        return ValidateName( ZfsKind, name, NameValidatorRegex );
    }

    protected internal bool ValidateName( )
    {
        return ValidateName( Name );
    }

    /// <exception cref="ArgumentOutOfRangeException">
    ///     If an invalid or uninitialized value is provided for
    ///     <paramref name="kind" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name" /> is null, empty, or only whitespace</exception>
    public static bool ValidateName( ZfsObjectKind kind, string name, Regex? validatorRegex = null )
    {
        if ( string.IsNullOrWhiteSpace( name ) )
        {
            throw new ArgumentNullException( nameof( name ), "name must be a non-null, non-empty, non-whitespace string" );
        }

        if ( name.Length > 255 )
        {
            throw new ArgumentOutOfRangeException( nameof( name ), "name must be 255 characters or less" );
        }

        validatorRegex ??= kind switch
        {
            ZfsObjectKind.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified to ValidateName." )
        };

        // ReSharper disable once ExceptionNotDocumentedOptional
        MatchCollection matches = validatorRegex.Matches( name );

        if ( matches.Count == 0 )
        {
            return false;
        }

        Logger.Debug( "Checking regex matches for {0}", name );
        // No matter which kind was specified, the pool group should exist and be a match
        foreach ( Match match in matches )
        {
            Logger.Debug( "Inspecting match {0}", match.Value );
        }

        Logger.Debug( "Name of {0} {1} is valid", kind, name );

        return true;
    }

    /// <summary>
    ///     Adds the <see cref="ZfsProperty" /> <paramref name="prop" /> to this <see name="ZfsObjectBase" />
    /// </summary>
    /// <param name="prop">The property to add</param>
    public void AddProperty( ZfsProperty prop )
    {
        Logger.Debug( "Adding property {0} to {1}", prop, Name );
        Properties[ prop.FullName ] = prop;
    }
}
