#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace SnapsInAZfs.Settings.Settings;

/// <summary>
///     Class that implements most of the public properties of <see cref="KestrelServerOptions" />, except for those that cannot be
///     serialized by the <see cref="JsonSerializer" />
/// </summary>
public sealed class KestrelConfiguration
{
    public bool? AddServerHeader { get; set; }
    public bool? AllowAlternateSchemes { get; set; }
    public string? AllowedHosts { get; set; }
    public bool? AllowResponseHeaderCompression { get; set; }
    public bool? AllowSynchronousIO { get; set; }
    public bool? DisableStringReuse { get; set; }
    public Dictionary<string, KestrelEndpointConfiguration>? Endpoints { get; set; }
    public KestrelServerLimits? Limits { get; set; } = new( );
}
