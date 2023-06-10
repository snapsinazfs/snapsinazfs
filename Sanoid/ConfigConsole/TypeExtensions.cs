// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Interop.Zfs.ZfsTypes;

namespace Sanoid.ConfigConsole;

/// <summary>
///     Extension methods
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Gets an integer index for radio button groups assuming the order of true,false,inherited from this
    ///     <see cref="ZfsProperty{T}" />
    /// </summary>
    /// <param name="property">The <see cref="ZfsProperty{T}" /> to convert to an integer index for radio button groups</param>
    /// <returns>
    ///     An <see langword="int" /> representing the index in a radio button group for this property's source<br />
    ///     0: true<br />
    ///     1: false<br />
    ///     2: inherited
    /// </returns>
    public static int AsTrueFalseRadioIndex( this ZfsProperty<bool> property )
    {
        return property.Value ? 0 : 1;
    }
}
