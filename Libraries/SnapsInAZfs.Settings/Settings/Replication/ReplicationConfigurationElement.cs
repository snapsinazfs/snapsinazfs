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

namespace SnapsInAZfs.Settings.Replication;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///     Base type for replication configuration elements.
/// </summary>
[JsonSourceGenerationOptions (
                                 JsonSerializerDefaults.General,
                                 AllowTrailingCommas = true,
                                 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                                 GenerationMode = JsonSourceGenerationMode.Default,
                                 IgnoreReadOnlyProperties = false,
                                 PropertyNameCaseInsensitive = true,
                                 PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                                 WriteIndented = true )]
[JsonSerializable ( typeof (ReplicationConfigurationElement) )]
public abstract record ReplicationConfigurationElement : IReplicationConfigurationElement
{
    /// <summary>
    ///     Creates a new instance of a <see cref="ReplicationConfigurationElement"/> with the provided <paramref name="Id"/> and
    ///     <paramref name="Enabled"/> values.
    /// </summary>
    /// <param name="Id">
    ///     A unique identifier for the <see cref="ReplicationConfigurationElement"/> or a new random value if unspecified or equal to
    ///     <see cref="Guid.Empty"/> (all-zeros).
    /// </param>
    /// <param name="Enabled">
    ///     Whether this <see cref="ReplicationConfigurationElement"/> should be enabled for processing or not. Default is
    ///     <see langword="false"/> if omitted.
    /// </param>
    /// <remarks>
    ///     Derived types are strongly encouraged to supply an explicit and valid value for <paramref name="Id"/>, if possible, when
    ///     constructing new elements, rather than relying on default behavior, to guard against potential future changes.
    /// </remarks>
    protected ReplicationConfigurationElement ( Guid Id = default, bool Enabled = false )
    {
        this.Enabled = Enabled;
        this.Id      = Id == Guid.Empty ? Guid.NewGuid ( ) : Id;
    }

    /// <inheritdoc/>
    public bool Enabled { get; set; }

    /// <inheritdoc/>
    [JsonRequired]
    public required Guid Id { get; init; }

    [SuppressMessage ( "ReSharper", "ParameterHidesMember", Justification = "This is a deconstructor." )]
    [SuppressMessage ( "ReSharper", "InconsistentNaming",   Justification = "This is a deconstructor." )]
    public void Deconstruct ( out bool Enabled, out Guid Id )
    {
        Enabled = this.Enabled;
        Id      = this.Id;
    }
}
