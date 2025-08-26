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

using System.Collections.Concurrent;
using System.Text.Json.Serialization;

[PublicAPI]
[JsonSerializable ( typeof( ReplicationSettings ) )]
public sealed record ReplicationSettings
{
    private IList<NetworkAccessControlEntry>? _networkAccessControlList;

    /// <summary>
    ///     Gets or sets a <see cref="Boolean" /> value controlling whether replication functionality is enabled or disabled at
    ///     application startup.
    ///     If <see langword="false" />, all replication activity is disabled and the values of <see cref="EnableLocal" />,
    ///     <see cref="EnableInbound" />, and <see cref="EnableOutbound" /> are ignored.
    /// </summary>
    /// <remarks>
    ///     All enablement flags default to <see langword="false" /> if not set.
    /// </remarks>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets a <see cref="Boolean" /> value controlling whether the inbound socket will listen for incoming replication
    ///     connections from remote systems.<br />
    ///     If <see langword="false" />, the socket will not be listening and inbound replication will not be available.
    ///     application startup.
    /// </summary>
    /// <remarks>
    ///     All enablement flags default to <see langword="false" /> if not set.
    /// </remarks>
    public bool EnableInbound { get; set; }

    /// <summary>
    ///     Gets or sets a <see cref="Boolean" /> value controlling whether scheduled replication tasks having both source and
    ///     destination
    ///     set to local ZFS locations will be launched as scheduled.<br />
    ///     If <see langword="false" />, local scheduled ZFS-to-ZFS replication operations will not run.<br />
    ///     Manual invocation of local replication tasks is always allowed, regardless of this setting.
    /// </summary>
    /// <remarks>
    ///     All enablement flags default to <see langword="false" /> if not set.
    /// </remarks>
    public bool EnableLocal { get; set; }

    /// <summary>
    ///     Gets or sets a <see cref="Boolean" /> value controlling whether scheduled replication tasks with remote destinations will be
    ///     launched as scheduled.<br />
    ///     If <see langword="false" />, scheduled ZFS-to-remote endpoint replication operations will not run.<br />
    ///     Manual invocation of outbound replication tasks is always allowed, regardless of this setting.
    /// </summary>
    /// <remarks>
    ///     All enablement flags default to <see langword="false" /> if not set.
    /// </remarks>
    public bool EnableOutbound { get; set; }

    /// <summary>
    ///     A priority-ordered collection of <see cref="NetworkAccessControlEntry" /> items which define the remote network addresses
    ///     allowed to send replication streams to a SIAZ process running on the local system.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entries are processed top-down, as they appear in the collection, before any replication activities are started.
    ///     </para>
    ///     <para>
    ///         Entries with <see cref="NetworkAccessControlEntry.Enabled" /> set to <see langword="false" /> are skipped without being
    ///         considered, continuing with the next entry as if the disabled entry were not in the list at all.
    ///     </para>
    ///     <para>
    ///         The first enabled entry that matches stops the search and the session will be permitted or denied continuation of the
    ///         session according to the value of <see cref="NetworkAccessControlEntry.Permit" />.
    ///     </para>
    ///     <para>
    ///         If no match is found, the session will be terminated as if there were an implicit match-all entry at the end of the list
    ///         with <see cref="NetworkAccessControlEntry.Permit" /> == <see langword="false" />.
    ///     </para>
    /// </remarks>
    public IList<NetworkAccessControlEntry>? NetworkAccessControlList
    {
        [NotNull]
        get { return _networkAccessControlList ??= [ ]; }

        init => _networkAccessControlList = value ?? [ ];
    }
}
