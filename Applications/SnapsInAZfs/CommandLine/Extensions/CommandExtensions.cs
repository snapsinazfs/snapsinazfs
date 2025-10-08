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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

/// <summary>
///   Extension methods for <see cref="Command" />, enabling fluent usage.
/// </summary>
/// <remarks>
///   Note that <see cref="RootCommand" /> is derived from <see cref="Command" />, so most of these methods apply to both types.
///   <br />
///   Generics are used to enable explicit forcing of method resolution if/when necessary.<br />
///   Explicit <see langword="extension" /> blocks are used to group type-specific implementations.
/// </remarks>
[SuppressMessage (
                   "Performance",
                   "CA1822:Mark members as static",
                   Justification = "Extension members are already static. This is an analyzer false positive."
                 )]
[SuppressMessage (
                   "Naming",
                   "CA1708:Identifiers should differ by more than case",
                   Justification = "This is a false positive, due to incomplete support for c# 14 syntax."
                 )]
public static class CommandExtensions
{
  /// <summary>
  ///   Extensions for <see cref="RootCommand" />.
  /// </summary>
  /// <param name="rootCommand">A <see cref="RootCommand" /> to operate on.</param>
  extension ( RootCommand rootCommand )
  {
    /// <inheritdoc cref="CommandExtensions.WithCommand{TCommand}(TCommand, Command)" />
    /// <remarks>
    ///   This method is an alias for <see cref="CommandExtensions.WithCommand{TCommand}(TCommand, Command)" /> that explicitly returns
    ///   a <see cref="RootCommand" /> reference.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public RootCommand With ( Command subCommand )
    {
      return rootCommand.WithCommand ( subCommand );
    }

    /// <summary>
    ///   Adds a single <see cref="Option{TOption}" /> to a <see cref="RootCommand" />.
    /// </summary>
    /// <param name="option">
    ///   An <see cref="Option{TOption}" /> to add to the <see cref="RootCommand" />.
    /// </param>
    /// <typeparam name="T">
    ///   The type parameter of the <see cref="Option{T}" /> provided to <paramref name="option" />.
    /// </typeparam>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public RootCommand WithOption<T> ( Option<T> option )
    {
      rootCommand.Add ( option );

      return rootCommand;
    }
  }

  /// <summary>
  ///   Extensions for <see cref="Command" />.
  /// </summary>
  /// <param name="command"></param>
  extension ( Command command )
  {
    /// <inheritdoc
    ///   cref="WithOption{TOption, TOptionFactoryArgs}(Command, Func{TOptionFactoryArgs?, Option{TOption}?}, TOptionFactoryArgs?, bool)" />
    /// <remarks>
    ///   This method is an alias for
    ///   <see
    ///   cref="WithOption{TOption, TOptionFactoryArgs}(Command, Func{TOptionFactoryArgs?, Option{TOption}?}, TOptionFactoryArgs?, bool)" />
    ///   .
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command With<TOption, TOptionFactoryArgs> (
      Func<TOptionFactoryArgs?, Option<TOption>> optionFactory,
      TOptionFactoryArgs?                        optionFactoryArgs,
      bool                                       skipIfNull = false
    )
      where TOption : notnull
    {
      return command.WithOption ( optionFactory, optionFactoryArgs, skipIfNull );
    }

    /// <summary></summary>
    /// <remarks>
    ///   This method is an alias for <see cref="WithOption{TOption}(Command, string?, Func{string?, Option{TOption}?}, bool)" />.
    /// </remarks>
    /// <param name="name">The name of the option to create and add.</param>
    /// <param name="optionFactory">
    ///   A delegate that accepts a single string parameter and returns an
    ///   <see cref="Option{T}">Option&lt;<typeparamref name="TOption" />&gt;</see>
    /// </param>
    /// <param name="skipIfNull"></param>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command With<TOption> ( string? name, Func<string?, Option<TOption>> optionFactory, bool skipIfNull = false )
    {
      return command.WithOption ( optionFactory, name, skipIfNull );
    }

    /// <inheritdoc cref="CommandExtensions.WithAction(System.CommandLine.Command,System.Action{System.CommandLine.ParseResult}?)" />
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithAction ( Func<ParseResult, int>? func )
    {
      // Need to explicitly check for null because SetAction throws on null.
      // However, null is valid and results in getting help text unless there were other parse errors
      // encountered, so we still want to be able to null it out if requested.
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
    ///   Sets the <see cref="Command.Action" /> to the provided <see cref="Action{T}" /> <see langword="delegate" />.
    /// </summary>
    /// <remarks>
    ///   This method enables both setting and clearing the <see cref="Command.Action" /> via the same method call. Pass
    ///   <see langword="null" /> to clear.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithAction ( Action<ParseResult>? action )
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

    /// <summary>
    ///   Sets the <see cref="Command.Action" /> to the provided <see cref="Func{T1,T2}" /> <see langword="delegate" />.
    /// </summary>
    /// <remarks>
    ///   This method enables both setting and clearing the <see cref="Command.Action" /> via the same method call. Pass
    ///   <see langword="null" /> to clear.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithAction ( Func<ParseResult, CancellationToken, Task<int>>? func )
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
    ///   Adds an alias to the current <see cref="Command" /> instance and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="alias">
    ///   A <see langword="string" /> to add as an additional alias for the <see cref="Command" />.<br />
    ///   See <see cref="Command.Aliases" />.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///   Aliases are backed by a HashSet of strings, so uniqueness is implicitly guaranteed.<br />
    ///   Aliases must be contiguous non-null strings (i.e., they cannot contain any whitespace at all).
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithAlias ( string alias )
    {
      command.Aliases.Add ( alias );

      return command;
    }

    /// <summary>
    ///   Adds an <see cref="Argument" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="argument">
    ///   A reference to an instance of an <see cref="Argument" /> to add to the <see cref="Command" />.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithArgument<T> ( Argument<T> argument )
    {
      command.Add ( argument );

      return command;
    }

    /// <summary>
    ///   Adds a new <see cref="Command" />, having the supplied <paramref name="name" /> and optional <paramref name="description" />
    ///   and <paramref name="action" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference the
    ///   method was called on.
    /// </summary>
    /// <param name="subCommand">
    ///   A reference to the new <see cref="Command" /> that was added as a sub-command of the current <see cref="Command" />.<br />
    ///   Provided as an <see langword="out" /> reference, for convenience.
    /// </param>
    /// <param name="name">
    ///   The name of the new <see cref="Command" /> to add to the current <see cref="Command" />.
    /// </param>
    /// <param name="description"></param>
    /// <param name="action">
    ///   An optional <see cref="Action{T}" /> where T is <see cref="ParseResult" /> that, if provided and not <see langword="null" />,
    ///   will be assigned to the new <paramref name="subCommand" />.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithCommand ( out Command subCommand, string name, string? description = null, Action<ParseResult>? action = null )
    {
      subCommand = new Command ( name, description ).WithAction ( action );
      command.Add ( subCommand );

      return command;
    }

    /// <summary>
    ///   Adds an <see cref="Option{T}" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="option">
    ///   A reference to an instance of an <see cref="Option{T}" /> to add to the <see cref="Command" />.
    /// </param>
    /// <typeparam name="T">
    ///   A non-null reference to an <see cref="Option{T}" />, where <typeparamref name="T" /> is unbounded.
    /// </typeparam>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithOption<T> ( Option<T> option )
    {
      command.Add ( option );

      return command;
    }

    /// <summary>
    ///   Adds a single <see cref="Option{TOption}" /> returned by the <paramref name="optionFactory" /> delegate to this
    ///   <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="optionFactory">
    ///   A delegate that accepts no arguments and returns a single non-null instance of an <see cref="Option{TOption}" /> to add to
    ///   the <see cref="Command" /> this method was called on.<br />
    ///   If the delegate is null, no action will be taken, but the <see cref="Command" /> will still be returned.
    /// </param>
    /// <param name="skipIfNull">
    ///   If provided and set to <see langword="true" />, performs no action on <paramref name="command" /> if
    ///   <paramref name="optionFactory" /> returns a null reference.<br />
    ///   Otherwise, a <see cref="NullReferenceException" /> will be thrown if <paramref name="optionFactory" /> returns
    ///   <see langword="null" />.
    /// </param>
    /// <typeparam name="TOption">
    ///   The type of <see cref="Option{T}" /> returned by the <paramref name="optionFactory" />, where <typeparamref name="TOption" />
    ///   is unbounded.
    /// </typeparam>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///   <paramref name="optionFactory" /> MUST return a non-null reference to a valid instance of an <see cref="Option{TOption}" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    [SuppressMessage ( "Performance", "CA1822:Mark members as static", Justification = "<Pending>" )]
    public Command WithOption<TOption> ( string? name, Func<string?, Option<TOption>?> optionFactory, bool skipIfNull = false )
    {
      Option<TOption>? option = optionFactory ( name );

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

    /// <summary>
    ///   Adds an <see cref="Option{T}" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="optionFactoryArgs">
    ///   An instance or value of the single argument, of type <typeparamref name="TOptionFactoryArgs" />, that will be passed to the
    ///   <paramref name="optionFactory" /> delegate.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <summary>
    ///   Adds a single <see cref="Option{TOption}" /> returned by the <paramref name="optionFactory" /> delegate to this
    ///   <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="optionFactory">
    ///   A delegate that accepts a single argument of type <typeparamref name="TOptionFactoryArgs" /> and returns a single non-null
    ///   instance of an <see cref="Option{TOption}" /> to add to the <see cref="Command" /> this method was called on.
    /// </param>
    /// <param name="skipIfNull">
    ///   If provided and set to <see langword="true" />, performs no action when <paramref name="optionFactory" /> returns a null
    ///   reference.<br />
    ///   Otherwise, a <see cref="NullReferenceException" /> will be thrown if <paramref name="optionFactory" /> returns
    ///   <see langword="null" />.
    /// </param>
    /// <typeparam name="TOption">
    ///   The type of <see cref="Option{T}" /> returned by the <paramref name="optionFactory" />, where <typeparamref name="TOption" />
    ///   is unbounded.
    /// </typeparam>
    /// <typeparam name="TOptionFactoryArgs">
    ///   The type of the single argument that will be passed to the <paramref name="optionFactory" /> delegate.
    /// </typeparam>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <remarks>
    ///   <paramref name="optionFactory" /> MUST return a non-null reference to a valid instance of an <see cref="Option{TOption}" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public Command WithOption<TOption, TOptionFactoryArgs> (
      Func<TOptionFactoryArgs?, Option<TOption>?> optionFactory,
      TOptionFactoryArgs?                         optionFactoryArgs,
      bool                                        skipIfNull = false
    )
    {
      Option<TOption>? option = optionFactory ( optionFactoryArgs );

      if ( option is not null )
      {
        command.Add ( option );
      }
      else if ( !skipIfNull )
      {
        throw new InvalidOperationException (
                                             $"The {nameof (optionFactory)} invoked with arguments {JsonSerializer.Serialize ( optionFactoryArgs )} produced a null option."
                                            );
      }

      return command;
    }
  }

  /// <summary>
  ///   Extension methods for types deriving from <see cref="Command" />.
  /// </summary>
  extension<TBaseCommand> ( TBaseCommand baseCommand ) where TBaseCommand : Command
  {
    /// <summary>
    ///   Adds a simple validator that adds an error to the result if the <see cref="CommandResult" />'s
    ///   <see cref="CommandResult.Children" /> collection is empty.
    /// </summary>
    /// <returns>
    ///   A reference to the <typeparamref name="TBaseCommand" /> this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public TBaseCommand RequiringOneOrMoreOptions ( )
    {
      baseCommand.Validators.Add ( static result =>
                                   {
                                     if ( !result.Children.Any ( ) )
                                     {
                                       result.AddError ( "One or more options must be specified." );
                                     }
                                   }
                                 );

      return baseCommand;
    }

    /// <summary>
    ///   Adds a simple validator that adds an error to the result if the <see cref="CommandResult" />'s
    ///   <see cref="CommandResult.Children" /> collection is empty.
    /// </summary>
    /// <returns>
    ///   A reference to the <typeparamref name="TBaseCommand" /> this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public TBaseCommand RequiringOneOrMoreOptionsIn ( params IEnumerable<Option> options )
    {
      Option[] optionsArray = [ ..options ];
      baseCommand.WithOptions ( optionsArray );
      baseCommand.Validators.Add ( result =>
                                   {
                                     // If none of the OptionResults in Children are contained in the optionsArray, add an error.
                                     if ( !result.Children.OfType<OptionResult> ( ).Any ( o => optionsArray.Contains ( o.Option ) ) )
                                     {
                                       result.AddError ( "One or more options must be specified." );
                                     }
                                   }
                                 );

      return baseCommand;
    }

    /// <inheritdoc cref="WithCommand{TCommand}(TCommand, Command)" />
    /// <remarks>
    ///   This method is an alias for <see cref="WithCommand{TCommand}(TCommand, Command)" />.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public TBaseCommand With<TSubCommand> ( TSubCommand subCommand )
      where TSubCommand : Command
    {
      return baseCommand.WithCommand ( subCommand );
    }

    /// <summary>
    ///   Adds <see cref="Command" /> to this <see cref="Command" /> and returns the same <see cref="Command" /> reference the method
    ///   was called on.
    /// </summary>
    /// <param name="subCommand">
    ///   A reference to an instance of a <see cref="Command" /> to add to the current <see cref="Command" />.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    /// <exception cref="ArgumentException">If <paramref name="subCommand" /> is an instance of <see cref="RootCommand" />.</exception>
    /// <remarks>
    ///   This method explicitly checks that you are not attempting to add a <see cref="RootCommand" /> to any other type of
    ///   <see cref="Command" /> and throws <see cref="InvalidOperationException" /> if you do.
    /// </remarks>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public TBaseCommand WithCommand ( Command subCommand )
    {
      return ( baseCommand, subCommand ) switch
             {
               (not null, not RootCommand) => AddSubCommandAndReturn ( baseCommand, subCommand ),
               (RootCommand, RootCommand) => throw new ArgumentException (
                                                                          $"Cannot add {nameof (RootCommand)} {subCommand.Name} to {nameof (RootCommand)} {baseCommand.Name}."
                                                                         ),
               (not null, RootCommand) => throw new ArgumentException (
                                                                       $"Cannot add {nameof (RootCommand)} {subCommand.Name} to {nameof (Command)} {baseCommand.Name}."
                                                                      ),
               _ => throw new InvalidOperationException ( $"Invalid {nameof (Command)}(s) provided." )
             };

      static TBaseCommand AddSubCommandAndReturn ( TBaseCommand cmd, Command sub )
      {
        cmd.Add ( sub );

        return cmd;
      }
    }

    /// <summary>
    ///   Adds one or more <see cref="Option" />s to this <see cref="Command" /> and returns the same <see cref="Command" /> reference.
    /// </summary>
    /// <param name="options">
    ///   A collection of one or more instances of <see cref="Option" />s to add to the <see cref="Command" />.
    /// </param>
    /// <returns>
    ///   A reference to the same <see cref="Command" /> instance that this method was called on.
    /// </returns>
    [PublicAPI]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public TBaseCommand WithOptions ( params IEnumerable<Option> options )
    {
      foreach ( Option option in options )
      {
        baseCommand.Add ( option );
      }

      return baseCommand;
    }
  }
}
