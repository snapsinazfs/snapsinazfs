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
using System.Runtime.CompilerServices;

/// <summary>
///   Extension methods for <see cref="Argument" />, enabling fluent usage.
/// </summary>
[PublicAPI]
public static class ArgumentExtensions
{
  private static readonly Logger Logger = LogManager.GetCurrentClassLogger ( );

  private static Lazy<SearchValues<char>> _invalidPathCharValues = new ( static ( ) => SearchValues.Create ( Path.GetInvalidPathChars ( ) ), LazyThreadSafetyMode.PublicationOnly );
  private static SearchValues<char>       InvalidPathCharValues => _invalidPathCharValues.Value;

  private static ArgumentResult ValidateCanWriteToPath ( ArgumentResult argumentResult, Token token )
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
      testFile.Close ( );
    }
    catch ( DirectoryNotFoundException directoryNotFoundException )
    {
      string message
        = $"Unable to open file {token.Value} for writing. The parent directory {file.Directory?.FullName ?? "(unknown)"} does not exist or is inaccessible.";
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

  private static ArgumentResult ValidateLegalFilePath ( ArgumentResult argumentResult, Token token )
  {
    int invalidCharacterIndex = token.Value.IndexOfAny ( InvalidPathCharValues );

    if ( invalidCharacterIndex >= 0 )
    {
      argumentResult.AddError ( new ( token.Value [ invalidCharacterIndex ], 1 ) );
    }

    return argumentResult;
  }

  /// <typeparam name="TArgument">The type of the argument value.</typeparam>
  /// <param name="argument">An <see cref="Argument{T}" /> instance.</param>
  extension<TArgument> ( Argument<TArgument> argument )
  {
    /// <inheritdoc cref="ArgumentValidation.AcceptOnlyFromAmong{T}(Argument{T}, string[])" />
    /// <remarks>
    ///   This method is a direct proxy for <see cref="ArgumentValidation.AcceptOnlyFromAmong{T}(Argument{T}, string[])" />, only
    ///   provided here for a more natural fluent grammar.
    /// </remarks>
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Argument<TArgument> AcceptingOnlyValuesIn ( params string[] acceptedValues )
    {
      return argument.AcceptOnlyFromAmong ( acceptedValues );
    }

    /// <summary>
    ///   Sets a custom parser for this <see cref="Argument{T}" /> and returns the same <see cref="Argument{T}" /> reference.
    /// </summary>
    /// <param name="customParser">
    ///   A custom argument parser to assign to this <see cref="Argument{T}" />.<br />
    ///   See <see cref="Argument{T}.CustomParser" />.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Argument{T}" /> instance that this method was called on.
    /// </returns>
    public Argument<TArgument> WithCustomParser ( Func<ArgumentResult, TArgument?>? customParser )
    {
      argument.CustomParser = customParser;

      return argument;
    }

    /// <summary>
    ///   Adds a validator to the current <see cref="Argument{T}" /> instance.
    /// </summary>
    /// <param name="validator">An <see cref="Action{T}" /> delegate that validates the argument value.</param>
    /// <remarks>
    ///   This is a simple proxy for the <see cref="Argument.Validators" />.<see cref="List{T}.Add(T)">Add</see> method.
    /// </remarks>
    [PublicAPI]
    public Argument<TArgument> WithValidator ( Action<ArgumentResult> validator )
    {
      argument.Validators.Add ( validator );

      return argument;
    }
  }
}
