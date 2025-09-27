#region MIT LICENSE
// Copyright 2025 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

namespace SnapsInAZfs.CommandLine.Extensions;

using System.Buffers;
using System.CommandLine;
using System.CommandLine.Parsing;

/// <summary>
///     Extension methods for <see cref="Argument" />, enabling fluent usage.
/// </summary>
public static class ArgumentExtensions
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc cref="ArgumentValidation.AcceptOnlyFromAmong{T}(Argument{T}, string[])" />
    /// <remarks>
    ///     This method is a direct proxy for <see cref="ArgumentValidation.AcceptOnlyFromAmong{T}(Argument{T}, string[])" />, only
    ///     provided here for a more natural fluent grammar.
    /// </remarks>
    public static Argument<T> AcceptingOnlyValuesIn<T>( this Argument<T> argument, params string[] acceptedValues )
    {
        return argument.AcceptOnlyFromAmong ( acceptedValues );
    }

    /// <summary>
    ///     Validates that the tokens provided for the argument represent normal files that exist and are writeable by the process.
    /// </summary>
    /// <param name="argument">The <see cref="Argument{T}" /> to validate.</param>
    /// <returns>An <see cref="ArgumentResult" /> with any validation errors appended.</returns>
    /// <remarks>
    ///     <para>
    ///         This method combines the functionality of
    ///         <see cref="ArgumentValidation.AcceptLegalFileNamesOnly{T}(Argument{T})" /> with an optimized implementation of
    ///         <see cref="ArgumentValidation.AcceptExistingOnly{T}(Argument{T})" /> that uses a cached <see cref="SearchValues{T}" />
    ///         for more efficient matching on longer or multiple inputs.<br />
    ///         Short or single-item inputs generally achieve comparable performance as the built-in validators as well.
    ///     </para>
    ///     <para>
    ///         The tests for legality of the path and existence of/access to the file are added as separate validators, with path name
    ///         validation being before existence/access in the validation pipeline.
    ///     </para>
    ///     <para>
    ///         This implementation tests access via actually attempting to open the target file(s) for more reliable results.<br />
    ///         If this is too costly or is otherwise undesirable for your application, use the built-in validators instead.
    ///     </para>
    /// </remarks>
    public static Argument<IEnumerable<string>> OnlyAcceptingLegalExistingWriteableFiles( this Argument<IEnumerable<string>> argument )
    {
        argument.Validators.Add ( result => _ = result.Tokens.Aggregate ( result, ValidateLegalFilePath ) );
        argument.Validators.Add ( result => _ = result.Tokens.Aggregate ( result, ValidateCanWriteToPath ) );

        return argument;
    }

    /// <summary>
    ///     Sets a custom parser for this <see cref="Argument{T}" /> and returns the same <see cref="Argument{T}" /> reference.
    /// </summary>
    /// <param name="argument">
    ///     The <see cref="Argument{T}" /> whose <see cref="Argument{T}.CustomParser" /> property will be set to
    ///     <paramref name="customParser" />.
    /// </param>
    /// <param name="customParser">
    ///     A custom argument parser to assign to this <see cref="Argument{T}" />.<br />
    ///     See <see cref="Argument{T}.CustomParser" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="Argument{T}" /> instance that this method was called on.
    /// </returns>
    public static Argument<T> WithCustomParser<T>( this Argument<T> argument, Func<ArgumentResult, T?>? customParser )
    {
        argument.CustomParser = customParser;

        return argument;
    }

    public static Argument<TArgument> WithValidator<TArgument>( this Argument<TArgument> argument, Action<ArgumentResult> validator )
    {
        argument.Validators.Add ( validator );

        return argument;
    }

    private static ArgumentResult ValidateCanWriteToPath( ArgumentResult argumentResult, Token token )
    {
        FileInfo file = new ( Path.GetFullPath ( token.Value ) );
        try
        {
            using FileStream testFile = file.Open (
                                                   new FileStreamOptions
                                                   {
                                                       Access = FileAccess.ReadWrite,
                                                       Mode   = FileMode.Open,
                                                       Share  = FileShare.Read | FileShare.Inheritable
                                                   }
                                                  );
            testFile.Close( );
        }
        catch ( DirectoryNotFoundException directoryNotFoundException )
        {
            string message = $"Unable to open file {token.Value} for writing. The parent directory {file.Directory?.FullName ?? "(unknown)"} does not exist or is inaccessible.";
            Logger.Warn ( directoryNotFoundException, message );
            argumentResult.AddError ( $"{message} See log for detailed exception data." );
        }
        catch ( FileNotFoundException fileNotFoundException )
        {
            string message = $"The file {token.Value} does not exist.";
            Logger.Warn ( fileNotFoundException, message );
            argumentResult.AddError ( $"{message} See log for detailed exception data." );
        }
        catch ( UnauthorizedAccessException unauthorizedAccessException )
        {
            string message = $"Unable to open file {token.Value} for writing. Access is denied.";
            Logger.Warn ( unauthorizedAccessException, message );
            argumentResult.AddError ( $"{message} See log for detailed exception data." );
        }
        catch ( IOException ioException )
        {
            string message = $"Unable to open file {token.Value} for writing. The result was {ioException.HResult}.";
            Logger.Warn ( ioException, message );
            argumentResult.AddError ( $"{message} See log for detailed exception data." );
        }

        return argumentResult;
    }

    private static ArgumentResult ValidateLegalFilePath( ArgumentResult argumentResult, Token token )
    {
        SearchValues<char> invalidPathCharValues = SearchValues.Create ( Path.GetInvalidPathChars( ) );

        int invalidCharacterIndex = token.Value.IndexOfAny ( invalidPathCharValues );

        if ( invalidCharacterIndex >= 0 )
        {
            argumentResult.AddError ( new ( token.Value [ invalidCharacterIndex ], 1 ) );
        }

        return argumentResult;
    }
}
