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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
///     Extension methods for <see cref="Command" />, enabling fluent usage.
/// </summary>
public static class RootCommandExtensions
{
    /// <inheritdoc cref="WithArgument{T}" />
    /// <remarks>
    ///     This method is an alias for <see cref="WithArgument{TArgument}(RootCommand, Argument{TArgument})" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand With<TArgument>( this RootCommand rootCommand, Argument<TArgument> argument )
        => rootCommand.WithArgument ( argument );

    /// <summary>
    ///     Adds <see cref="Command" /> to this <see cref="RootCommand" /> and returns the same <see cref="RootCommand" /> reference the
    ///     method
    ///     was called on.
    /// </summary>
    /// <param name="rootCommand">
    ///     The <see cref="RootCommand" /> to which <paramref name="subCommand" /> will be added.
    /// </param>
    /// <param name="subCommand">
    ///     A reference to an instance of a <see cref="Command" /> to add to the current <see cref="RootCommand" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="RootCommand" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand WithCommand( this RootCommand rootCommand, Command subCommand )
    {
        if ( subCommand is RootCommand )
        {
            throw new ArgumentException ( $"Cannot add RootCommand {subCommand.Name} to RootCommand {rootCommand.Name}." );
        }

        rootCommand.Add ( subCommand );

        return rootCommand;
    }

    [PublicAPI]
    [DoesNotReturn]
    public static RootCommand WithCommand( this RootCommand rootCommand, RootCommand invalid )
    {
        throw new ArgumentException ( $"Cannot add RootCommand {invalid.Name} to RootCommand {rootCommand.Name}." );
    }

    /// <inheritdoc cref="WithCommand(RootCommand, Command)" />
    /// <remarks>This method is an alias for <see cref="WithCommand(RootCommand, Command)" />.</remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand With( this RootCommand rootCommand, Command subCommand )
    {
        return rootCommand.WithCommand ( subCommand );
    }

    /// <inheritdoc cref="WithOption{T}" />
    /// <remarks>This method is an alias for <see cref="WithOption{T}" />.</remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand With<T>( this RootCommand rootCommand, Option<T> option )
    {
        return rootCommand.WithOption ( option );
    }

    /// <summary>
    ///     Adds an <see cref="Option{T}" /> to this <see cref="RootCommand" /> and returns the same <see cref="RootCommand" />
    ///     reference.
    /// </summary>
    /// <param name="optionFactoryArgs">
    ///     An instance or value of the single argument, of type <typeparamref name="TOptionFactoryArgs" />, that will be passed to the
    ///     <paramref name="optionFactory" /> delegate.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="RootCommand" /> instance that this method was called on.
    /// </returns>
    /// <summary>
    ///     Adds a single <see cref="Option{TOption}" /> returned by the <paramref name="optionFactory" /> delegate to this
    ///     <see cref="RootCommand" /> and returns the same <see cref="RootCommand" /> reference.
    /// </summary>
    /// <param name="rootCommand">
    ///     The <see cref="RootCommand" /> to which the <see cref="Option{TOption}" /> returned by <paramref name="optionFactory" /> will
    ///     be added.
    /// </param>
    /// <param name="optionFactory">
    ///     A delegate that accepts a single argument of type <typeparamref name="TOptionFactoryArgs" /> and returns a single non-null
    ///     instance of an <see cref="Option{TOption}" /> to add to the <see cref="RootCommand" /> this method was called on.
    /// </param>
    /// <typeparam name="TOption">
    ///     The type of <see cref="Option{T}" /> returned by the <paramref name="optionFactory" />, where <typeparamref name="TOption" />
    ///     is unbounded.
    /// </typeparam>
    /// <typeparam name="TOptionFactoryArgs">
    ///     The type of the single argument that will be passed to the <paramref name="optionFactory" /> delegate.
    /// </typeparam>
    /// <returns>
    ///     A reference to the same <see cref="RootCommand" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///     <paramref name="optionFactory" /> MUST return a non-null reference to a valid instance of an <see cref="Option{TOption}" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand With<TOption, TOptionFactoryArgs>( this RootCommand rootCommand, Func<TOptionFactoryArgs?, Option<TOption>> optionFactory, TOptionFactoryArgs? optionFactoryArgs )
    {
        rootCommand.Add ( optionFactory ( optionFactoryArgs ) );

        return rootCommand;
    }

    /// <summary>
    ///     Adds a single <see cref="Option{TOption}" /> returned by the <paramref name="optionFactory" /> delegate to this
    ///     <see cref="RootCommand" /> and returns the same <see cref="RootCommand" /> reference.
    /// </summary>
    /// <param name="rootCommand">
    ///     The <see cref="RootCommand" /> to which the <see cref="Option{TOption}" /> returned by <paramref name="optionFactory" /> will
    ///     be added.
    /// </param>
    /// <param name="optionFactory">
    ///     A delegate that accepts no arguments and returns a single non-null instance of an <see cref="Option{TOption}" /> to add to
    ///     the <see cref="RootCommand" /> this method was called on.
    /// </param>
    /// <typeparam name="TOption">
    ///     The type of <see cref="Option{T}" /> returned by the <paramref name="optionFactory" />, where <typeparamref name="TOption" />
    ///     is unbounded.
    /// </typeparam>
    /// <returns>
    ///     A reference to the same <see cref="RootCommand" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///     <paramref name="optionFactory" /> MUST return a non-null reference to a valid instance of an <see cref="Option{TOption}" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand With<TOption>( this RootCommand rootCommand, Func<Option<TOption>> optionFactory )
    {
        rootCommand.Add ( optionFactory( ) );

        return rootCommand;
    }

    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand WithAction( this RootCommand rootCommand, Action<ParseResult>? action )
    {
        // Need to explicitly check for null because SetAction throws on null.
        // However, null is valid and results in getting help text unless there were other parse errors encountered.
        if ( action is not null )
        {
            rootCommand.SetAction ( action );
        }
        else
        {
            rootCommand.Action = null;
        }

        return rootCommand;
    }

    /// <summary>
    ///     Adds an <see cref="Argument" /> to this <see cref="RootCommand" /> and returns the same <see cref="RootCommand" /> reference.
    /// </summary>
    /// <param name="rootCommand">
    ///     The <see cref="RootCommand" /> to which <paramref name="argument" /> will be added.
    /// </param>
    /// <param name="argument">
    ///     A reference to an instance of an <see cref="Argument" /> to add to the <see cref="RootCommand" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="RootCommand" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand WithArgument<TArgument>( this RootCommand rootCommand, Argument<TArgument> argument )
    {
        rootCommand.Add ( argument );

        return rootCommand;
    }

    /// <summary>
    ///     Adds an <see cref="Option{T}" /> to this <see cref="RootCommand" /> and returns the same <see cref="RootCommand" />
    ///     reference.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="RootCommand" /> to which <paramref name="option" /> will be added.
    /// </param>
    /// <param name="option">
    ///     A reference to an instance of an <see cref="Option{T}" /> to add to the <see cref="RootCommand" />.
    /// </param>
    /// <typeparam name="T">
    ///     A non-null reference to an <see cref="Option{T}" />, where <typeparamref name="T" /> is unbounded.
    /// </typeparam>
    /// <returns>
    ///     A reference to the same <see cref="RootCommand" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static RootCommand WithOption<T>( this RootCommand command, Option<T> option )
    {
        command.Add ( option );

        return command;
    }
}
