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
using System.Runtime.CompilerServices;

/// <summary>
///     Extension methods for <see cref="Command" />, enabling fluent usage.
/// </summary>
public static class CommandExtensions
{
    /// <inheritdoc cref="WithCommand(Command, Command)" />
    /// <remarks>
    ///     This method is an alias for <see cref="WithCommand(Command, Command)" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command With( this Command rootCommand, Command subCommand )
    {
        return rootCommand.WithCommand ( subCommand );
    }

    /// <summary>
    ///     Adds an <see cref="Option{T}" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="command">The <see cref="Command" /> to which <paramref name="option" /> will be added.</param>
    /// <param name="option">
    ///     A reference to an instance of an <see cref="Option{T}" /> to add to the <see cref="Command" />.
    /// </param>
    /// <typeparam name="T">
    ///     A non-null reference to an <see cref="Option{T}" />, where <typeparamref name="T" /> is unbounded.
    /// </typeparam>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command With<T>( this Command command, Option<T> option )
    {
        command.Add ( option );

        return command;
    }

    /// <summary>
    ///     Adds an <see cref="Option{T}" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="optionFactoryArgs">
    ///     An instance or value of the single argument, of type <typeparamref name="TOptionFactoryArgs" />, that will be passed to the
    ///     <paramref name="optionFactory" /> delegate.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <summary>
    ///     Adds a single <see cref="Option{TOption}" /> returned by the <paramref name="optionFactory" /> delegate to this
    ///     <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="Command" /> to which the <see cref="Option{TOption}" /> returned by <paramref name="optionFactory" /> will be
    ///     added.
    /// </param>
    /// <param name="optionFactory">
    ///     A delegate that accepts a single argument of type <typeparamref name="TOptionFactoryArgs" /> and returns a single non-null
    ///     instance of an <see cref="Option{TOption}" /> to add to the <see cref="Command" /> this method was called on.
    /// </param>
    /// <typeparam name="TOption">
    ///     The type of <see cref="Option{T}" /> returned by the <paramref name="optionFactory" />, where <typeparamref name="TOption" />
    ///     is unbounded.
    /// </typeparam>
    /// <typeparam name="TOptionFactoryArgs">
    ///     The type of the single argument that will be passed to the <paramref name="optionFactory" /> delegate.
    /// </typeparam>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///     <paramref name="optionFactory" /> MUST return a non-null reference to a valid instance of an <see cref="Option{TOption}" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command With<TOption, TOptionFactoryArgs>( this Command command, Func<TOptionFactoryArgs?, Option<TOption>> optionFactory, TOptionFactoryArgs? optionFactoryArgs )
    {
        command.Add ( optionFactory ( optionFactoryArgs ) );

        return command;
    }

    /// <summary>
    ///     Adds a single <see cref="Option{TOption}" /> returned by the <paramref name="optionFactory" /> delegate to this
    ///     <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="Command" /> to which the <see cref="Option{TOption}" /> returned by <paramref name="optionFactory" /> will be
    ///     added.
    /// </param>
    /// <param name="optionFactory">
    ///     A delegate that accepts no arguments and returns a single non-null instance of an <see cref="Option{TOption}" /> to add to
    ///     the <see cref="Command" /> this method was called on.<br />
    ///     If the delegate is null, no action will be taken, but the <see cref="Command" /> will still be returned.
    /// </param>
    /// <param name="skipIfNull">
    ///     If provided and set to <see langword="true" />, performs no action on <paramref name="command" /> if
    ///     <paramref name="optionFactory" /> returns a null reference.<br />
    ///     Otherwise, a <see cref="NullReferenceException" /> will be thrown if <paramref name="optionFactory" /> returns
    ///     <see langword="null" />.
    /// </param>
    /// <typeparam name="TOption">
    ///     The type of <see cref="Option{T}" /> returned by the <paramref name="optionFactory" />, where <typeparamref name="TOption" />
    ///     is unbounded.
    /// </typeparam>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///     <paramref name="optionFactory" /> MUST return a non-null reference to a valid instance of an <see cref="Option{TOption}" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command With<TOption>( this Command command, Func<Option<TOption>?> optionFactory, bool skipIfNull = false )
    {
        Option<TOption>? option = optionFactory( );

        if ( option is not null )
        {
            command.Add ( option );
        }
        else if ( !skipIfNull )
        {
            throw new InvalidOperationException ( $"The {nameof (optionFactory)} produced a null option." );
        }

        return command;
    }

    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command WithAction( this Command command, Action<ParseResult>? action )
    {
        // Need to explicitly check for null because SetAction throws on null.
        // However, null is valid and results in getting help text unless there were other parse errors encountered.
        if ( action is not null )
        {
            command.SetAction ( action );
        }
        else
        {
            command.Action = null;
        }

        return command;
    }

    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command WithAction( this Command command, Func<ParseResult, int>? func )
    {
        // Need to explicitly check for null because SetAction throws on null.
        // However, null is valid and results in getting help text unless there were other parse errors encountered.
        if ( func is not null )
        {
            command.SetAction ( func );
        }
        else
        {
            command.Action = null;
        }

        return command;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command WithAction( this Command command, Func<ParseResult, CancellationToken, Task<int>>? func )
    {
        // Need to explicitly check for null because SetAction throws on null.
        // However, null is valid and results in getting help text unless there were other parse errors encountered.
        if ( func is not null )
        {
            command.SetAction ( func );
        }
        else
        {
            command.Action = null;
        }

        return command;
    }

    /// <summary>
    ///     Adds an <see cref="Argument" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="command">The <see cref="Command" /> to which <paramref name="argument" /> will be added.</param>
    /// <param name="argument">
    ///     A reference to an instance of an <see cref="Argument" /> to add to the <see cref="Command" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    public static Command WithArgument<T>( this Command command, Argument<T> argument )
    {
        command.Add ( argument );

        return command;
    }

    /// <summary>
    ///     Adds <see cref="Command" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference the method
    ///     was called on.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="Command" /> to which <paramref name="subCommand" /> will be added.
    /// </param>
    /// <param name="subCommand">
    ///     A reference to an instance of a <see cref="Command" /> to add to the current <see cref="Command" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command WithCommand( this Command command, Command subCommand )
    {
        command.Add ( subCommand );

        return command;
    }

    /// <summary>
    ///     Adds a new <see cref="Command" />, having the supplied <paramref name="name" /> and optional <paramref name="description" />
    ///     and <paramref name="action" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference the
    ///     method was called on.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="Command" /> to which <paramref name="subCommand" /> will be added.
    /// </param>
    /// <param name="subCommand">
    ///     A reference to the new <see cref="Command" /> that was added as a sub-command of the current <see cref="Command" />.<br />
    ///     Provided as an <see langword="out" /> reference, for convenience.
    /// </param>
    /// <param name="name">
    ///     The name of the new <see cref="Command" /> to add to the current <see cref="Command" />.
    /// </param>
    /// <param name="description"></param>
    /// <param name="action">
    ///     An optional <see cref="Action{T}" /> where T is <see cref="ParseResult" /> that, if provided and not <see langword="null" />,
    ///     will be assigned to the new <paramref name="subCommand" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static Command WithCommand( this Command command, out Command subCommand, string name, string? description = null, Action<ParseResult>? action = null )
    {
        subCommand = new Command ( name, description ).WithAction ( action );
        command.Add ( subCommand );

        return command;
    }
}
