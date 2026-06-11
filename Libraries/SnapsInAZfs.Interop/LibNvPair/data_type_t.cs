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

namespace SnapsInAZfs.Interop.LibNvPair;

public enum data_type_t
{
  DATA_TYPE_UNKNOWN = 0,
  DATA_TYPE_BOOLEAN = 1,
  DATA_TYPE_BYTE = 2,
  DATA_TYPE_INT16 = 3,
  DATA_TYPE_UINT16 = 4,
  DATA_TYPE_INT32 = 5,
  DATA_TYPE_UINT32 = 6,
  DATA_TYPE_INT64 = 7,
  DATA_TYPE_UINT64 = 8,
  DATA_TYPE_STRING = 9,
  DATA_TYPE_BYTE_ARRAY = 10,
  DATA_TYPE_INT16_ARRAY = 11,
  DATA_TYPE_UINT16_ARRAY = 12,
  DATA_TYPE_INT32_ARRAY = 13,
  DATA_TYPE_UINT32_ARRAY = 14,
  DATA_TYPE_INT64_ARRAY = 15,
  DATA_TYPE_UINT64_ARRAY = 16,
  DATA_TYPE_STRING_ARRAY = 17,
  DATA_TYPE_HRTIME = 18,
  DATA_TYPE_NVLIST = 19,
  DATA_TYPE_NVLIST_ARRAY = 20,
  DATA_TYPE_BOOLEAN_VALUE = 21,
  DATA_TYPE_INT8 = 22,
  DATA_TYPE_UINT8 = 23,
  DATA_TYPE_BOOLEAN_ARRAY = 24,
  DATA_TYPE_INT8_ARRAY = 25,
  DATA_TYPE_UINT8_ARRAY = 26
}
