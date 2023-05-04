using System.ComponentModel.DataAnnotations;
using Json.Schema;

namespace Sanoid.Common.Configuration;

/// <summary>
/// Represents errors that occur while validating Sanoid.net's configuration.
/// </summary>
public class ConfigurationValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class with a default error message.
    /// </summary>
    /// <remarks>
    /// Default message: "Error validating Sanoid.json"
    /// </remarks>
    public ConfigurationValidationException( ) : this( "Error validating Sanoid.json" )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public ConfigurationValidationException( string message ) : base( message )
    {
    }

    /// <summary>
    /// Creates a new instance of a <see cref="ConfigurationValidationException"/> with a specified error message and a specified <see cref="EvaluationResults"/> collection
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="details">An <see cref="IEnumerable{T}" /> of <see cref="EvaluationResults"/> to include in the exception.
    /// </param>
    public ConfigurationValidationException( string message, IEnumerable<EvaluationResults> details ) : base( message )
    {
        ValidationDetails = details;
    }
    /// <summary>
    /// Validation details associated with the <see cref="ConfigurationValidationException"/>.<br />
    /// Intended to be used to convey <see cref="EvaluationResults"/> with <see cref="EvaluationResults.IsValid"/> == false to display to the user for troubleshooting.
    /// </summary>
    /// <value>
    /// An <see cref="IEnumerable{T}"/> of <see cref="EvaluationResults"/>
    /// </value>
    public IEnumerable<EvaluationResults>? ValidationDetails { get; set; }
}
