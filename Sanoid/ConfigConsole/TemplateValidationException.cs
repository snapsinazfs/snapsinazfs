// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Terminal.Gui;

namespace Sanoid.ConfigConsole;

/// <summary>
///     An exception to be thrown when validation of a template from input values fails
/// </summary>
/// <typeparam name="T">Any <see langword="notnull" /> type</typeparam>
public class TemplateValidationException<T> : ApplicationException where T : notnull
{
    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException{T}" /> with the given name, value, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="propertyValue">The value of the property that caused this exception</param>
    /// <param name="message">A message describing the exception</param>
    public TemplateValidationException( string propertyName, T propertyValue, string message ) : base( message )
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }

    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException{T}" /> with the given name, entry field, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="entryField">The <see cref="View" /> that contains the invalid value</param>
    /// <param name="message">A message describing the exception</param>
    public TemplateValidationException( string propertyName, View entryField, string message ) : base( message )
    {
        PropertyName = propertyName;
        EntryField = entryField;
    }

    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException{T}" /> with the given name, value, entry field, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="propertyValue">The value of the property that caused this exception</param>
    /// <param name="entryField">The <see cref="View" /> that contains the invalid value</param>
    /// <param name="message">A message describing the exception</param>
    public TemplateValidationException( string propertyName, T propertyValue, View entryField, string message ) : base( message )
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        EntryField = entryField;
    }

    /// <summary>
    ///     Gets the <see cref="View" /> that contains the value that caused this exception
    /// </summary>
    public View? EntryField { get; init; }

    /// <summary>
    ///     Gets the name of the property that caused this exception
    /// </summary>
    public string PropertyName { get; init; }

    /// <summary>
    ///     Gets the value of the property that caused this exception
    /// </summary>
    public T? PropertyValue { get; init; }
}
