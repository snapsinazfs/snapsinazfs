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

using LogLevel = NLog.LogLevel;

/// <summary>
///   Extensions for the <see cref="LoggingLevel" /> <see langword="enum" />.
/// </summary>
public static class LoggingLevelExtensions
{
  /// <param name="level">
  ///   The logging level to operate on.
  /// </param>
  extension ( LoggingLevel level )
  {
    // False positive
    // TODO: Remove this when fixed version of .net is released.
#pragma warning disable CS1734
    /// <summary>
    ///   Reflection-free conversion of <see cref="LoggingLevel" /> values to their <see langword="string" /> representations.
    /// </summary>
    /// <returns>The <see langword="string" /> representation of <paramref name="level" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="level" /> is set to an undefined value.</exception>
#pragma warning restore CS1734
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

    // False positive
    // TODO: Remove this when fixed version of .net is released.
#pragma warning disable CS1734
    /// <summary>
    ///   Converts a <see cref="LoggingLevel" /> to its equivalent NLog <see cref="LogLevel" />.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="level" /> is set to an undefined value.</exception>
#pragma warning restore CS1734
    public LogLevel ToNLogLevel ( )
    {
      return level switch
             {
               LoggingLevel.Trace => LogLevel.Trace,
               LoggingLevel.Debug => LogLevel.Debug,
               LoggingLevel.Info  => LogLevel.Info,
               LoggingLevel.Warn  => LogLevel.Warn,
               LoggingLevel.Error => LogLevel.Error,
               LoggingLevel.Fatal => LogLevel.Fatal,
               LoggingLevel.Off   => LogLevel.Off,
               _                  => throw new ArgumentOutOfRangeException ( nameof (level), level, $"Value {(int)level} is invalid for {nameof (LoggingLevel)}." )
             };
    }
  }
}
