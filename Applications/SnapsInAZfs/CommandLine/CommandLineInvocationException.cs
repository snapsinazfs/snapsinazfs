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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapsInAZfs.CommandLine;

using System.Collections;
using System.CommandLine;

/// <summary>
///     The exception that is thrown when SIAZ fails to start due to errors on the command line other than those handled by
///     <c>System.CommandLine</c>.
/// </summary>
/// <param name="message">The exception text.</param>
/// <param name="symbols">(Optional)Symbols that led to this exception.</param>
/// <param name="innerException">
///     (Optional)An exception that caused this exception. This will be passed to the base <see cref="ApplicationException" />
///     constructor.
/// </param>
public sealed class CommandLineInvocationException( string message, Dictionary<string, Symbol>? symbols = null, Exception? innerException = null )
    : ApplicationException ( message, innerException )
{
    /// <summary>When not empty, contains the <see cref="Symbol" />s that led to the exception.</summary>
    public override Dictionary<string, Symbol> Data { get; } = symbols ?? new ( );
}
