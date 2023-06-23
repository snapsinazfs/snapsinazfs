// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.ConfigConsole;

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
