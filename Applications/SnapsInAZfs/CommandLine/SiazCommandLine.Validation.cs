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

namespace SnapsInAZfs.CommandLine;

using System.Buffers;
using System.CommandLine.Parsing;
using System.Globalization;

public partial class SiazCommandLine
{
  // ReSharper disable once MemberCanBePrivate.Global
  internal SearchValues<char> InvalidPathCharValues = SearchValues.Create ( Path.GetInvalidPathChars ( ) );

  // ReSharper disable once MemberCanBePrivate.Global
  internal static readonly string[] StandardBooleanFalseStrings =
  [
    "0",
    bool.FalseString,
    CultureInfo.CurrentUICulture.TextInfo.ToLower ( bool.FalseString ),
    "disable",
    "disabled",
    "no",
    "off"
  ];

  // ReSharper disable once MemberCanBePrivate.Global
  internal static readonly string[] StandardBooleanTrueStrings =
  [
    "1",
    bool.TrueString,
    CultureInfo.CurrentUICulture.TextInfo.ToLower ( bool.TrueString ),
    "enable",
    "enabled",
    "yes",
    "on"
  ];

  internal static readonly string[] StandardBooleanFormsSet =
  [
    ..StandardBooleanTrueStrings,
    ..StandardBooleanFalseStrings
  ];

  // ReSharper disable once MemberCanBePrivate.Global

  // ReSharper disable once MemberCanBePrivate.Global
  internal static readonly SearchValues<string> StandardBooleanTrueValuesSearch
    = SearchValues.Create ( StandardBooleanTrueStrings.AsSpan ( ), StringComparison.OrdinalIgnoreCase );

  private OptionResult ValidateCanWriteToPath ( OptionResult optionResult, Token token )
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
      _logger.Warn ( directoryNotFoundException, message );
      optionResult.AddError ( $"{message} See log for detailed exception data." );
    }
    catch ( FileNotFoundException fileNotFoundException )
    {
      string message = $"The file {token.Value} does not exist.";
      _logger.Warn ( fileNotFoundException, message );
      optionResult.AddError ( $"{message} See log for detailed exception data." );
    }
    catch ( UnauthorizedAccessException unauthorizedAccessException )
    {
      string message = $"Unable to open file {token.Value} for writing. Access is denied.";
      _logger.Warn ( unauthorizedAccessException, message );
      optionResult.AddError ( $"{message} See log for detailed exception data." );
    }
    catch ( IOException ioException )
    {
      string message = $"Unable to open file {token.Value} for writing. The result was {ioException.HResult}.";
      _logger.Warn ( ioException, message );
      optionResult.AddError ( $"{message} See log for detailed exception data." );
    }

    return optionResult;
  }

  private void ValidateFileExistsAndIsWriteable ( OptionResult option )
  {
    option.Option.Validators.Add ( result => _ = result.Tokens.Aggregate ( result, ValidateLegalFilePath ) );
    option.Option.Validators.Add ( result => _ = result.Tokens.Aggregate ( result, ValidateCanWriteToPath ) );
  }

  private OptionResult ValidateLegalFilePath ( OptionResult optionResult, Token token )
  {
    int invalidCharacterIndex = token.Value.IndexOfAny ( InvalidPathCharValues );

    if ( invalidCharacterIndex >= 0 )
    {
      optionResult.AddError ( new ( token.Value [ invalidCharacterIndex ], 1 ) );
    }

    return optionResult;
  }

  internal static readonly System.CommandLine.ParserConfiguration? ParserConfiguration = new ( )
                                                                                         {
                                                                                           EnablePosixBundling = true
                                                                                         };
}
