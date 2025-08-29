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
using System.CommandLine.Parsing;

/// <summary>
///     Extension methods for <see cref="Argument" />, enabling fluent usage.
/// </summary>
public static class ArgumentExtensions
{
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
}
