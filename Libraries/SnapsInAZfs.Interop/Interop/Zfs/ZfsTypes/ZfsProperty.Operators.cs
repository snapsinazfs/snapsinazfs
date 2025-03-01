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

public readonly partial struct ZfsProperty<T>
{
    /// <inheritdoc/>
    public static bool operator == ( ZfsProperty<T> left, ZfsProperty<T> right ) => left.Equals ( right );

    /// <inheritdoc/>
    public static bool operator != ( ZfsProperty<T> left, ZfsProperty<T> right ) => !left.Equals ( right );

    /// <inheritdoc/>
    public bool Equals ( bool other ) => Value is bool v && v == other;

    /// <inheritdoc/>
    public bool Equals ( DateTimeOffset other ) => Value is DateTimeOffset v && v == other;

    /// <inheritdoc/>
    public bool Equals ( int other ) => Value is int v && v == other;

    /// <inheritdoc/>
    public bool Equals ( string? other ) => Value is string v && v == other;

    /// <inheritdoc/>
    public bool Equals ( ZfsProperty<bool> other ) => Value is bool v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;

    /// <inheritdoc/>
    public bool Equals ( ZfsProperty<DateTimeOffset> other ) => Value is DateTimeOffset v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;

    /// <inheritdoc/>
    public bool Equals ( ZfsProperty<int> other ) => Value is int v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;

    /// <inheritdoc/>
    public bool Equals ( ZfsProperty<string> other ) => Value is string v && Name == other.Name && v == other.Value && IsLocal == other.IsLocal;

    /// <inheritdoc/>
    public bool Equals ( ZfsProperty<T> other ) =>
        EqualityComparer<T>.Default
                           .Equals ( Value, other.Value )
     && Equals ( Owner, other.Owner )
     && Name == other.Name;

    /// <inheritdoc/>
    public override bool Equals ( object? obj )
    {
        return obj switch
               {
                   ZfsProperty<int> other            => Equals ( other ),
                   ZfsProperty<bool> other           => Equals ( other ),
                   ZfsProperty<DateTimeOffset> other => Equals ( other ),
                   ZfsProperty<string> other         => Equals ( other ),
                   ZfsProperty<T> other              => Equals ( other ),
                   null                              => false,
                   IZfsProperty other                => other.Equals ( this ),
                   _                                 => false
               };
    }

    /// <inheritdoc/>
    public override int GetHashCode ( ) => HashCode.Combine ( Value, Name, IsLocal );

    public static bool operator == ( ZfsProperty<T> left, bool                        right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, int                         right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, string                      right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, DateTimeOffset              right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, ZfsProperty<bool>           right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, ZfsProperty<int>            right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, ZfsProperty<string>         right ) => left.Equals ( right );
    public static bool operator == ( ZfsProperty<T> left, ZfsProperty<DateTimeOffset> right ) => left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, bool                        right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, int                         right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, string                      right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, DateTimeOffset              right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, ZfsProperty<bool>           right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, ZfsProperty<int>            right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, ZfsProperty<string>         right ) => !left.Equals ( right );
    public static bool operator != ( ZfsProperty<T> left, ZfsProperty<DateTimeOffset> right ) => !left.Equals ( right );
}
