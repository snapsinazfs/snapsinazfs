// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.RegularExpressions;
using NLog;

namespace Sanoid.Interop.Zfs;

public abstract class ZfsCommandRunnerBase : IZfsCommandRunner
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <exception cref="ArgumentOutOfRangeException">
    ///     If an invalid or uninitialized value is provided for
    ///     <paramref name="kind" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name" /> is null, empty, or only whitespace</exception>
    public static bool ValidateName( ZfsObjectKind kind, string name )
    {
        if ( string.IsNullOrWhiteSpace( name ) )
        {
            throw new ArgumentNullException( nameof( name ), "name must be a non-null, non-empty, non-whitespace string" );
        }

        if ( name.Length > 255 )
        {
            throw new ArgumentOutOfRangeException( nameof( name ), "name must be 255 characters or less" );
        }

        Regex validatorRegex = kind switch
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

    /// <inheritdoc />
    public abstract Dictionary<string, ZfsProperty> GetZfsProperties( ZfsObjectKind kind, string zfsObjectName, bool sanoidOnly = true );
}
