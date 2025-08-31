// Copyright $CurrentDate.Year Brandon Thetford
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// See https://opensource.org/license/MIT/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapsInAZfs.CommandLine.Extensions;

using System.Buffers;
using System.CommandLine;
using System.CommandLine.Parsing;

public static class OptionExtensions
{
    public static Option<string> ValidatePathIsWriteable( this Option<string> option )
    {
        option.Validators.Add( ValidatePathIsWriteable );

        void ValidatePathIsWriteable( OptionResult result )
        {
            string path = Path.GetFullPath ( result.GetValueOrDefault<string>( ) );
            result.AddError("Specified path is not writeable.");
        }

        return option;
    }


    internal static readonly char[]             invalidPathChars      = Path.GetInvalidPathChars( );
    internal static readonly SearchValues<char> invalidPathCharValues = SearchValues.Create ( Path.GetInvalidPathChars( ) );

    public static Argument<T> OnlyAcceptingLegalAndWriteableFilePaths<T>( this Argument<T> argument )
    {
        argument.Validators.Add (static result =>
                                  {
                                      _ = result.Tokens.Aggregate ( result, ValidateFilePathAndFileMode );

                                      return;

                                      static ArgumentResult ValidateFilePathAndFileMode( ArgumentResult argumentResult, Token token)
                                      {
                                          int invalidCharacterIndex = token.Value.IndexOfAny ( invalidPathCharValues );

                                          if ( invalidCharacterIndex >= 0 )
                                          {
                                              argumentResult.AddError ( new ( token.Value [ invalidCharacterIndex ], 1 ) );

                                              return argumentResult;
                                          }

                                          System.IO.
                                          
                                          FileInfo file = new ( token.Value );
                                          switch ( file )
                                          {
                                              case { Exists: true, IsReadOnly: false } when (file.UnixFileMode & mode) == mode:
                                          }
                                          if ( file.Exists )
                                          {
                                              file.UnixFileMode & mode
                                          }
                                          if(File.Exists (  ))

                                          return argumentResult;
                                      }

                                  }
                                );

        return argument;
    }

    public static Argument<T> AcceptLegalFilePathsOnlyLoop<T>(this Argument<T> argument)
        {
            argument.Validators.Add(static result =>
                                    {
                                        var invalidPathChars = Path.GetInvalidPathChars();

                                        for (var i = 0; i < result.Tokens.Count; i++)
                                        {
                                            var token = result.Tokens[i];

                                            // File class no longer check invalid character
                                            // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                                            var invalidCharactersIndex = token.Value.IndexOfAny(invalidPathChars);

                                            if (invalidCharactersIndex >= 0)
                                            {
                                                result.AddError ( new ( token.Value [ invalidCharactersIndex ], 1 ) );
                                            }
                                        }
                                    });

            return argument;
        }
}
