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
public class TemplateValidationException : ApplicationException
{
    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException" /> with the given name, value, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="propertyValue">The value of the property that caused this exception</param>
    /// <param name="message">
    ///     <inheritdoc cref="ApplicationException(string)" path="/param[@name='message']" />
    /// </param>
    public TemplateValidationException( string? propertyName, string? propertyValue, string message ) : base( message )
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        InnerExceptions = new( );
    }

    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException" /> with the given name, entry field, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="entryField">The <see cref="View" /> that contains the invalid value</param>
    /// <param name="message">
    ///     <inheritdoc cref="ApplicationException(string)" path="/param[@name='message']" />
    /// </param>
    public TemplateValidationException( string? propertyName, View entryField, string message ) : base( message )
    {
        PropertyName = propertyName;
        EntryField = entryField;
        InnerExceptions = new( );
    }

    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException" /> with the given name, value, entry field, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="propertyValue">The value of the property that caused this exception</param>
    /// <param name="entryField">The <see cref="View" /> that contains the invalid value</param>
    /// <param name="message">
    ///     <inheritdoc cref="ApplicationException(string)" path="/param[@name='message']" />
    /// </param>
    public TemplateValidationException( string? propertyName, string? propertyValue, View? entryField, string message ) : base( message )
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        EntryField = entryField;
        InnerExceptions = new( );
    }

    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException" /> with the given name, value, entry field, and message
    /// </summary>
    /// <param name="propertyName">The name of the property that caused this exception</param>
    /// <param name="propertyValue">The value of the property that caused this exception</param>
    /// <param name="entryField">The <see cref="View" /> that contains the invalid value</param>
    /// <param name="message">
    ///     <inheritdoc cref="ApplicationException(string,Exception)" path="/param[@name='message']" />
    /// </param>
    /// <param name="innerException">
    ///     <inheritdoc cref="ApplicationException(string,Exception)" path="/param[@name='innerException']" />
    /// </param>
    public TemplateValidationException( string? propertyName, string? propertyValue, View entryField, string message, Exception innerException ) : base( message, innerException )
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        EntryField = entryField;
        InnerExceptions = new( );
    }

    /// <summary>
    ///     Creates a new <see cref="TemplateValidationException" /> message and inner exceptions
    /// </summary>
    /// <param name="message">
    ///     <inheritdoc cref="ApplicationException(string)" path="/param[@name='message']" />
    /// </param>
    /// <param name="innerExceptions">A list of exceptions to use, if this is an aggregated exception</param>
    public TemplateValidationException( string message, List<TemplateValidationException> innerExceptions ) : base( message )
    {
        InnerExceptions = innerExceptions;
    }

    /// <summary>
    ///     Gets the <see cref="View" /> that contains the value that caused this exception
    /// </summary>
    public View? EntryField { get; init; }

    /// <summary>
    ///     Gets a list of inner exceptions
    /// </summary>
    public List<TemplateValidationException> InnerExceptions { get; init; }

    /// <summary>
    ///     Gets the name of the property that caused this exception
    /// </summary>
    public string? PropertyName { get; init; }

    /// <summary>
    ///     Gets the value of the property that caused this exception
    /// </summary>
    public string? PropertyValue { get; init; }
}
