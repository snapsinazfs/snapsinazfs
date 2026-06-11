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

using System.Runtime.InteropServices;

namespace SnapsInAZfs.Interop.LibNvPair;

internal static unsafe class NativeMethods
{
  private const string Lib = "nvpair";

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_add_boolean ( nvlist_t* nvl, byte* name );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_add_nvlist ( nvlist_t* nvl, byte* name, nvlist_t* value );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_add_string ( nvlist_t* nvl, byte* name, byte* value );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_add_uint64 ( nvlist_t* nvl, byte* name, ulong value );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_alloc ( nvlist_t** nvl, nvlist_alloc_flags flags, int kmflag );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern void nvlist_free ( nvlist_t* nvl );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_lookup_nvlist ( nvlist_t* nvl, byte* name, nvlist_t** value );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_lookup_string ( nvlist_t* nvl, byte* name, byte** value );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern int nvlist_lookup_uint64 ( nvlist_t* nvl, byte* name, ulong* value );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern nvpair_t* nvlist_next_nvpair ( nvlist_t* nvl, nvpair_t* pair );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern byte* nvpair_name ( nvpair_t* pair );

  [DllImport ( Lib, ExactSpelling = true )]
  public static extern data_type_t nvpair_type ( nvpair_t* pair );
}
