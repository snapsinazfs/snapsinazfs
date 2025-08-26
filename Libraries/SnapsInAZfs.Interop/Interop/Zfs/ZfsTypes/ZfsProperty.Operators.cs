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

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

using System.Runtime.CompilerServices;

public readonly partial struct ZfsProperty<T>
{
    /// <inheritdoc />
    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<T> right )
    {
        return left.Equals ( right );
    }

    /// <inheritdoc />
    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<T> right )
    {
        return !left.Equals ( right );
    }

    /// <inheritdoc />
    public bool Equals( bool other )
    {
        return Value is bool v && v == other;
    }

    /// <inheritdoc />
    public bool Equals( DateTimeOffset other )
    {
        return Value is DateTimeOffset v && v == other;
    }

    /// <inheritdoc />
    public bool Equals( int other )
    {
        return Value is int v && v == other;
    }

    /// <inheritdoc />
    public bool Equals( string? other )
    {
        return Value is string v && v == other;
    }

    /// <inheritdoc />
    public bool Equals( ZfsProperty<T> other )
    {
        return EqualityComparer<T>.Default
                                  .Equals ( Value, other.Value )
            && Equals ( Owner, other.Owner )
            && Name == other.Name;
    }

    /// <inheritdoc cref="ZfsProperty{T}.Equals(object?)" />
    public bool Equals( ZfsProperty<bool> other )
    {
        return Value is bool v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc cref="ZfsProperty{T}.Equals(object?)" />
    public bool Equals( ZfsProperty<DateTimeOffset> other )
    {
        return Value is DateTimeOffset v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc cref="ZfsProperty{T}.Equals(object?)" />
    public bool Equals( ZfsProperty<int> other )
    {
        return Value is int v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc cref="ZfsProperty{T}.Equals(object?)" />
    public bool Equals( ZfsProperty<string> other )
    {
        return Value is string v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;
    }

    /// <inheritdoc />
    public override bool Equals( object? obj )
    {
        return obj switch
               {
                   ZfsProperty<int> other            => Equals ( other ),
                   ZfsProperty<bool> other           => Equals ( other ),
                   ZfsProperty<DateTimeOffset> other => Equals ( other ),
                   ZfsProperty<string> other         => Equals ( other ),
                   ZfsProperty<T> other              => Equals ( other ),
                   null                              => false,
                   IZfsProperty other                => LogTypeMismatchAndReturnFalse ( this, other ),
                   _                                 => false
               };

        static bool LogTypeMismatchAndReturnFalse( in ZfsProperty<T> currentProperty, in IZfsProperty other )
        {
            Logger.Warn ( $"Type mismatch comparing equality of {currentProperty} and {other}" );

            return false;
        }
    }

    /// <inheritdoc />
    public override int GetHashCode ( )
    {
        return HashCode.Combine ( Value, Name, IsLocal );
    }

    public static bool operator ==( ZfsProperty<T> left, bool right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, int right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, string right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, DateTimeOffset right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<bool> right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<int> right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<string> right )
    {
        return left.Equals ( right );
    }

    public static bool operator ==( ZfsProperty<T> left, ZfsProperty<DateTimeOffset> right )
    {
        return left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, bool right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, int right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, string right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, DateTimeOffset right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<bool> right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<int> right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<string> right )
    {
        return !left.Equals ( right );
    }

    public static bool operator !=( ZfsProperty<T> left, ZfsProperty<DateTimeOffset> right )
    {
        return !left.Equals ( right );
    }
}
