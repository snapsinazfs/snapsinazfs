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

/// <summary>
///   Logging levels matching the values defined by NLog, for command line autocompletion.
/// </summary>
[PublicAPI]
[UsedImplicitly ( ImplicitUseTargetFlags.WithMembers )]
public enum LoggingLevel
{
  /// <inheritdoc cref="NLog.LogLevel.Trace" />
  Trace = 0,

  /// <inheritdoc cref="NLog.LogLevel.Debug" />
  Debug = 1,

  /// <inheritdoc cref="NLog.LogLevel.Info" />
  Info = 2,

  /// <inheritdoc cref="NLog.LogLevel.Warn" />
  Warn = 3,

  /// <inheritdoc cref="NLog.LogLevel.Error" />
  Error = 4,

  /// <inheritdoc cref="NLog.LogLevel.Fatal" />
  Fatal = 5,

  /// <inheritdoc cref="NLog.LogLevel.Off" />
  Off = 6
}
