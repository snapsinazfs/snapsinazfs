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

public abstract class ZfsObjectBase
{
    /// <summary>
    ///     Creates a new <see cref="ZfsObjectBase" /> with the specified name and kind
    /// </summary>
    /// <param name="name">The name of the new <see cref="ZfsObjectBase" /></param>
    /// <param name="kind">The <see cref="ZfsObjectKind" /> of object to create</param>
    /// <param name="poolRoot"></param>
    /// <param name="isKnownPoolRoot">
    ///     Short-circuit if this dataset is known to be a pool root at instantiation, to avoid
    ///     string lookup
    /// </param>
    /// <param name="validateName">
    ///     If true, the constructor will perform name validation for the type of object being created.
    /// </param>
    /// <param name="nameValidatorRegex">
    ///     An optional <see cref="Regex" />. If left null, uses a Regex from <see cref="ZfsIdentifierRegexes" /> based on
    ///     <paramref name="kind" />
    /// </param>
    protected internal ZfsObjectBase( string name, ZfsObjectKind kind, ZfsObjectBase? poolRoot = null, bool isKnownPoolRoot = false, bool validateName = false, Regex? nameValidatorRegex = null )
    {
        Logger.Debug( "Creating new ZfsObjectBase {0} of kind {1}", name, kind );
        ZfsKind = kind;
        IsPoolRoot = isKnownPoolRoot || name.IndexOf( '/' ) == -1;
        if ( IsPoolRoot )
        {
            PoolRoot = this;
        }
        else
        {
            PoolRoot = poolRoot ?? throw new ArgumentNullException( nameof( poolRoot ) );
        }

        NameValidatorRegex = nameValidatorRegex ?? kind switch
        {
            ZfsObjectKind.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            ZfsObjectKind.Unknown => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified for ZfsIdentifierValidator." ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified for ZfsIdentifierValidator." )
        };

        if ( validateName )
        {
            Logger.Debug( "Name validation requested for new ZfsObjectBase" );
            if ( !ValidateName( name ) )
            {
                string errorMessage = $"Invalid name specified for a new {ZfsKind} with validateName=true";
                Logger.Error( errorMessage );
                throw new ArgumentOutOfRangeException( nameof( name ), errorMessage );
            }
        }

        Name = name;
        Properties = new( );
    }

    private int _poolUsedCapacity;

    private readonly object _propertiesDictionaryLock = new( );

    public bool IsPoolRoot { get; }

    public ZfsProperty? this[ string key ]
    {
        get
        {
            Logger.Trace( "Trying to get property {0} from {1} {2}", key, ZfsKind, Name );
            bool gotValue = Properties.TryGetValue( key, out ZfsProperty? prop );
            if ( gotValue )
            {
                Logger.Trace( "Got property {0} from {1}", prop, Name );
            }
            else
            {
                Logger.Trace( "Property {0} not found in {1} {2}", key, ZfsKind, Name );
            }

            return prop;
        }
        set
        {
            if ( value is null )
            {
                Logger.Trace( "Removing property {0} from {1} {2}", key, ZfsKind, Name );
                Properties.TryRemove( key, out ZfsProperty? _ );
                return;
            }

            Logger.Trace( "Setting property {0} for {1} {2}", value, ZfsKind, Name );
            Properties[ key ] = value;
        }
    }

    /// <summary>
    ///     Gets or sets the name of the <see cref="ZfsObjectBase" />
    /// </summary>
    /// <value>A <see langword="string" /> containing the name of the object</value>
    public string Name { get; }

    [JsonIgnore]
    internal Regex NameValidatorRegex { get; }

    [JsonIgnore]
    public ZfsObjectBase PoolRoot { get; }

    internal int PoolUsedCapacity
    {
        get => IsPoolRoot ? _poolUsedCapacity : PoolRoot.PoolUsedCapacity;
        set
        {
            if ( IsPoolRoot )
            {
                _poolUsedCapacity = value;
            }
            else
            {
                Logger.Error( "Invalid attempt to set capacity on non-root object {0}", Name );
            }
        }
    }

    /// <summary>
    ///     A dictionary of property names and their values, as strings
    /// </summary>
    public ConcurrentDictionary<string, ZfsProperty> Properties { get; }

    public bool PruneSnapshots
    {
        get
        {
            Logger.Trace( "Trying to get prunesnapshots property for {0}", Name );
            bool gotValue = Properties.TryGetValue( ZfsProperty.PruneSnapshotsPropertyName, out ZfsProperty? prop );
            if ( gotValue )
            {
                Logger.Trace( "Got property {0} from {1}", prop, Name );
            }
            else
            {
                Logger.Trace( "prunesnapshots property not found in {0}", Name );
            }

            if ( prop is null )
            {
                return false;
            }

            if ( string.IsNullOrWhiteSpace( prop.Value ) )
            {
                return false;
            }

            return bool.TryParse( prop.Value, out bool result ) && result;
        }
    }

    public string RootName => Name.GetZfsPathRoot( );

    public ZfsObjectKind ZfsKind { get; }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public bool HasProperty( string propertyName )
    {
        return Properties.ContainsKey( propertyName );
    }

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
        Logger.Debug( "Validating {0} name \"{1}\"", kind.ToString( ), name );
        if ( string.IsNullOrWhiteSpace( name ) )
        {
            throw new ArgumentNullException( nameof( name ), "name must be a non-null, non-empty, non-whitespace string" );
        }

        if ( name.Length > 255 )
        {
            throw new ArgumentOutOfRangeException( nameof( name ), "name must be 255 characters or less" );
        }

        // Sure they are... They're handled by the default case.
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        validatorRegex ??= kind switch
        {
            ZfsObjectKind.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsObjectKind.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified to ValidateName." )
        };

        MatchCollection matches = validatorRegex.Matches( name );

        if ( matches.Count == 0 )
        {
            return false;
        }

        Logger.Trace( "Checking regex matches for {0}", name );
        // No matter which kind was specified, the pool group should exist and be a match
        for ( int matchIndex = 0; matchIndex < matches.Count; matchIndex++ )
        {
            Match match = matches[ matchIndex ];
            Logger.Trace( "Inspecting match {0}", match.Value );
            if ( match.Success )
            {
                continue;
            }

            Logger.Error( "Name of {0} {1} is invalid", kind.ToString( ), name );
            return false;
        }

        Logger.Debug( "Name of {0} {1} is valid", kind, name );

        return true;
    }

    /// <summary>
    ///     Adds the <see cref="ZfsProperty" /> <paramref name="prop" /> to this <see name="ZfsObjectBase" />
    /// </summary>
    /// <param name="prop">The property to add</param>
    public ZfsProperty AddOrUpdateProperty( ZfsProperty prop )
    {
        lock ( _propertiesDictionaryLock )
        {
            return Properties[ prop.Name ] = prop;
        }
    }

    /// <exception cref="ArgumentNullException"><paramref name="propertyName" /> is <see langword="null" />.</exception>
    public ZfsProperty AddOrUpdateProperty( string propertyName, string propertyValue, string propertyValueSource )
    {
        // Unfortunately, the AddOrUpdate method isn't atomic, so we need to enforce a lock ourselves
        // There's no built-in atomic way to perform a check for the key, update if it exists, and insert if not.
        // It can only atomically test and perform one operation
        lock ( _propertiesDictionaryLock )
        {
            return Properties.AddOrUpdate( propertyName, AddValueFactory, UpdateValueFactory );

            ZfsProperty UpdateValueFactory( string key, ZfsProperty oldProperty )
            {
                oldProperty.Value = propertyValue;
                oldProperty.Source = propertyValueSource;
                return oldProperty;
            }

            ZfsProperty AddValueFactory( string arg )
            {
                return new( propertyName, propertyValue, propertyValueSource );
            }
        }
    }
}
