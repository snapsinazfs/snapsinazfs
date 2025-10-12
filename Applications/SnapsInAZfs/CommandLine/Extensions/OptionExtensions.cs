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

using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Reflection.Emit;

/// <summary>
///   Extension methods for <see cref="Option{T}" />, enabling fluent usage.
/// </summary>
[PublicAPI]
public static class OptionExtensions
{
  /// <summary>
  ///   Adds the provided <paramref name="validator" /> delegate to the <see cref="Option.Validators" /> collection for the current
  ///   <see cref="Option{T}" />
  /// </summary>
  /// <typeparam name="TOption">The type of the option.</typeparam>
  /// <param name="option">The option to which <paramref name="validator" /> will be added.</param>
  /// <param name="validator">
  ///   An <see cref="Action{T}" /> accepting an <see cref="OptionResult" /> and returning nothing, which will be called on every
  ///   value of <paramref name="option" />.
  /// </param>
  /// <returns>
  ///   A reference to <paramref name="option" />, after appending <paramref name="validator" />.
  /// </returns>
  public static Option<TOption> WithValidator<TOption> ( this Option<TOption> option, Action<OptionResult> validator )
  {
    option.Validators.Add ( validator );

    return option;
  }

  extension<T> ( Option<T> option )
  {
    public Option<T> WithCompletionSource ( Func<CompletionContext,IEnumerable<CompletionItem>> completionItemFactory )
    {
      option.CompletionSources.Add( completionItemFactory );
      return option;
    }
  }


  extension<T> ( Option<T> option )
  where T : IBinaryInteger<T>
  {
    public Option<T> WithSuggestedCompletionValues ( params int[] values )
    {
      option.CompletionSources.Add ( c =>
                                     {
                                       return values.Select ( static i => new CompletionItem ( label: i.ToString ( "D", CultureInfo.CurrentCulture ), sortText: i.ToString ( "D10", CultureInfo.CurrentCulture ) ) ).ToArray ( );
                                     }
                                   );
      return option;
    }
  }
  
  extension<T> ( Option<T> option )
  where T : unmanaged, Enum
  {
    public Option<T> WithValueOrderedEnumHelpText ( )
    {
      option.HelpName = string.Join ( '|', Enum.GetValues<T> ( ).Select ( static e => e.ToString ( "G" ) ) );
      return option;
    }
  }
  
  extension<T> ( T enumType )
  where T : struct, Enum
  {
    public IEnumerable<CompletionItem> GetOrderedEnumCompletionItems ( )
    {
      
      return Enum.GetValues<T> ( ).Select ( static e => new CompletionItem ( e.ToString ( "G" ), sortText: e.ToString ( "D" ) ) );
    }
  }

  extension ( LoggingLevel level )
  {
    public string ToNameString ( )
    {
      return level switch
             {
               LoggingLevel.Trace => "Trace",
               LoggingLevel.Debug => "Debug",
               LoggingLevel.Info  => "Info",
               LoggingLevel.Warn  => "Warn",
               LoggingLevel.Error => "Error",
               LoggingLevel.Fatal => "Fatal",
               LoggingLevel.Off   => "Off",
               _                  => throw new ArgumentOutOfRangeException ( nameof (level), level, $"Value {(int)level} is invalid for {nameof (LoggingLevel)}." )
             };
    }
  }
  
}
