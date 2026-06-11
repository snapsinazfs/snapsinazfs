#region MIT LICENSE

// Copyright 2026 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace SnapsInAZfs.Interop.LibNvPair;

internal static unsafe class NativeMemoryHelpers
{
  extension ( ReadOnlySpan<char> value )
  {
    /// <summary>
    ///   Copies the provided <see cref="ReadOnlySpan{T}" /> of <see langword="char" />s to an unmanaged heap buffer of
    ///   <see langword="byte" />s in UTF-8 encoding, adds a terminating zero, and returns the pointer to that buffer.
    /// </summary>
    /// <remarks>
    ///   Don't forget to free this using the <see cref="NativeMemoryHelpers.Free" /> method after it is no longer needed.
    /// </remarks>
    [MustUseReturnValue]
    public byte* GetUnmanagedNativeUtf8Bytes ( )
    {
      int   byteCount = Encoding.UTF8.GetByteCount ( value );
      byte* buffer    = (byte*)NativeMemory.Alloc ( (nuint)( byteCount + 1 ) );

      Span<byte> bufferSpan = new ( buffer, byteCount + 1 );
      Encoding.UTF8.GetBytes ( value, bufferSpan );
      buffer [ byteCount ] = 0;

      return buffer;
    }
  }

  extension ( ReadOnlySpan<byte> value )
  {
    /// <summary>
    ///   Copies the provided <see cref="ReadOnlySpan{T}" /> of <see langword="byte" />s to an unmanaged heap buffer, optionally adds a
    ///   terminating zero (default), and returns the pointer to that buffer.
    /// </summary>
    /// <remarks>
    ///   Don't forget to free this using the <see cref="NativeMemoryHelpers.Free" /> method after it is no longer needed.
    /// </remarks>
    [MustUseReturnValue]
    public byte* GetUnmanagedNativeBytes ( bool omitTerminatingZero = false )
    {
      int        valueByteCount  = value.Length;
      int        bufferByteCount = omitTerminatingZero ? valueByteCount : valueByteCount + 1;
      byte*      buffer          = (byte*)NativeMemory.Alloc ( (nuint)bufferByteCount );
      Span<byte> bufferSpan      = new ( buffer, bufferByteCount );

      Unsafe.CopyBlock ( ref bufferSpan [ 0 ], in value [ 0 ], (uint)valueByteCount );
      if ( !omitTerminatingZero )
      {
        bufferSpan [ valueByteCount ] = 0;
      }

      return buffer;
    }
  }

  public static void Free ( byte* ptr )
  {
    NativeMemory.Free ( ptr );
  }
}
