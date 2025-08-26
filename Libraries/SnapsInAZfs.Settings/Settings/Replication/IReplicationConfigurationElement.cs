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

using System.Text.Json.Serialization;

/// <summary>
///     High-level interface implemented by any replication type that needs an identifier and the ability to enable/disable.
/// </summary>
[PublicAPI]
public interface IReplicationConfigurationElement
{
    /// <summary>
    ///     Whether this <see cref="IReplicationConfigurationElement" /> is enabled for future replication activities.
    /// </summary>
    /// <para>
    ///     May not be serialized in configuration files by all implementing types and, if absent, will be <see langword="false" />.
    /// </para>
    [JsonIgnore ( Condition = JsonIgnoreCondition.WhenWritingDefault )]
    bool Enabled { get; set; }

    /// <summary>
    ///     An immutable unique identifier for the <see cref="IReplicationConfigurationElement" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values having the first 4 bytes set to all-zeros (00000000-XXXX-XXXX-XXXX-XXXXXXXXXXXX) are reserved for internal use by
    ///         SIAZ as identifiers for common objects that are always expected to exist and for unknown objects.<br />
    ///         Values having the first 4 bytes set to all-ones (FFFFFFFF-XXXX-XXXX-XXXX-XXXXXXXXXXXX) are reserved for user-defined
    ///         purposes, with all remaining bytes available for any arbitrary definition (suggestions: WWNs, MAC addresses, etc).<br />
    ///         SIAZ will discard randomly-generated GUID values that have the first 4 bytes set to all-ones or all-zeros to ensure that
    ///         those ranges are protected against the unlikely case of random collisions.
    ///     </para>
    ///     <para>
    ///         Values in the reserved internal range configured as identifiers for user-specified objects will be rejected and result in
    ///         an appropriate exception.
    ///     </para>
    ///     <para>
    ///         The all-zeros value, as it is the default value of a GUID, shall always be interpreted internally by SIAZ as
    ///         unknown/unspecified, and may result in a random GUID being generated, when necessary.
    ///     </para>
    /// </remarks>
    Guid Id { get; init; }
}
