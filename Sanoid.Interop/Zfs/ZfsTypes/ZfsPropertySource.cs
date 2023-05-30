﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Interop.Zfs.ZfsTypes;

public sealed class ZfsPropertySource
{
    private ZfsPropertySource( )
    {
    }

    private ZfsPropertySource( ZfsPropertySourceKind kind )
    {
        _kind = kind;
    }

    private readonly ZfsPropertySourceKind _kind;

    public static ZfsPropertySource Default { get; } = new( ZfsPropertySourceKind.Default );
    public static ZfsPropertySource Inherited { get; } = new( ZfsPropertySourceKind.Inherited );
    public static ZfsPropertySource Local { get; } = new( ZfsPropertySourceKind.Local );
    public static ZfsPropertySource Native { get; } = new( ZfsPropertySourceKind.Native );

    public static implicit operator string( ZfsPropertySource obj )
    {
        return obj.ToString( );
    }

    /// <summary>
    ///     Implicit conversion from string to <see cref="ZfsPropertySource" />
    /// </summary>
    /// <param name="value">The string to convert to a <see cref="ZfsPropertySource" /></param>
    /// <remarks>
    ///     Assumes that any string that isn't explicitly defined indicates the property is inherited.
    /// </remarks>
    public static implicit operator ZfsPropertySource( string value )
    {
        return value.ToLowerInvariant( ) switch
        {
            "local" => Local,
            "default" => Default,
            "-" => Native,
            _ => Inherited
        };
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return _kind switch
        {
            ZfsPropertySourceKind.Default => "default",
            ZfsPropertySourceKind.Local => "local",
            ZfsPropertySourceKind.Native => "native",
            ZfsPropertySourceKind.Inherited => "inherited"
        };
    }
}