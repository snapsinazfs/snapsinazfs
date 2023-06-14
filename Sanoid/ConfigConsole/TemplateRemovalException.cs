// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.ConfigConsole;

/// <summary>
///     An exception type for failure to remove a template when requested
/// </summary>
public class TemplateRemovalException : ApplicationException
{
    /// <summary>
    ///     Creates a new instance of the <see cref="TemplateRemovalException" /> with the specified
    ///     <paramref name="errorMessage" />
    /// </summary>
    /// <param name="errorMessage">An error message for display to the user</param>
    public TemplateRemovalException( string errorMessage ) : base( errorMessage )
    {
    }
}
