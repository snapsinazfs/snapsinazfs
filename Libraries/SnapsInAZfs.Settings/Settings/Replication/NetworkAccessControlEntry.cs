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

[PublicAPI]
[JsonSerializable ( typeof (NetworkAccessControlEntry) )]
public sealed record NetworkAccessControlEntry ( string Prefix = "", Guid Id = default, bool Enabled = false ) : ReplicationConfigurationElement ( Id, Enabled )
{
    /// <summary>
    ///     Gets the logical negation of the <see cref="Permit"/> property, indicating if a matching entry should be denied
    ///     (<see langword="true"/>) or permitted (<see langword="false"/>) to proceed with the session.
    /// </summary>
    /// <remarks>
    ///     This is a run-time property for convenience only and is never serialized to or deserialized from configuration by SIAZ.<br/>
    ///     See documentation for <see cref="Permit"/> for details. Behavior of this property is identical to <see cref="Permit"/>,
    ///     except that the result of <see langword="true"/> and <see langword="false"/> values of this property are opposite those of
    ///     <see cref="Permit"/>.
    /// </remarks>
    [JsonIgnore ( Condition = JsonIgnoreCondition.Always )]
    public bool Deny => !Permit;

    /// <summary>
    ///     Arbitrary user-specified string data describing the <see cref="NetworkAccessControlEntry"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         May be displayed in logs and configuration interfaces, so should be kept short and human-readable.
    ///     </para>
    ///     <para>
    ///         This property will be persisted in JSON configuration files if set via the configuration console and will be displayed as
    ///         its raw value.
    ///     </para>
    ///     <para>
    ///         Length limits may be enforced by implementing types or by types or methods consuming the
    ///         <see cref="NetworkAccessControlEntry"/> interface.<br/>
    ///         Values exceeding any such limits may result unexpected behaviors and treatment of such restrictions is undefined for this
    ///         property of the <see cref="NetworkAccessControlEntry"/> interface.
    ///     </para>
    /// </remarks>
    public string? Description { get; }

    /// <summary>
    ///     A <see cref="Boolean"/> value specifying if a remote socket endpoint address matching this
    ///     <see cref="NetworkAccessControlEntry"/> should be permitted to proceed with further replication-related activities for the
    ///     associated session.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A value of <see langword="true"/> means the remote endpoint is permitted to proceed with the session, subject to any
    ///         additional authentication, authorization, or other relevant configuration applicable to the session.<br/>
    ///         A value of <see langword="false"/> means the remote endpoint may not continue, and that the process evaluating that
    ///         incoming session should immediately terminate the session appropriately without further consideration.
    ///     </para>
    ///     <para>
    ///         If the <see cref="IReplicationConfigurationElement.Enabled"/> property is <see langword="false"/>, this
    ///         <see cref="NetworkAccessControlEntry"/> will be skipped regardless of any other property values.
    ///     </para>
    ///     <para>
    ///         If this property is absent/not explicitly defined and is not set to <see langword="true"/> in configuration, it will be
    ///         considered to
    ///         be <see langword="false"/> when configuration is loaded.<br/>
    ///         It is permissible to omit this property in configuration if the desired result of the
    ///         <see cref="NetworkAccessControlEntry"/> is to deny matching sessions.<br/>
    ///         SIAZ will not write this property to configuration files if it is not set to <see langword="true"/> when saving the
    ///         configuration.
    ///     </para>
    /// </remarks>
    [JsonIgnore ( Condition = JsonIgnoreCondition.WhenWritingDefault )]
    public bool Permit { get; set; }

    /// <summary>
    ///     Gets or sets the network prefix (IPv4 or IPv6) of the remote endpoint(s) this <see cref="NetworkAccessControlEntry"/> will
    ///     match.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Whitespace will be ignored but should not be included anyway, to avoid unnecessary additional string manipulation at
    ///         run-time.
    ///     </para>
    ///     <para>
    ///         Values may be IPv6 or IPv4 addresses and are expected in CIDR block notation.<br/>
    ///         IPv4 or IPv6 will be automatically detected by the form of the value specified.
    ///     </para>
    ///     <para>
    ///         Valid forms are Address/PrefixLength, where address is in standard notation for the protocol and PrefixLength is the
    ///         number of **contiguous** bits in the network portion of the address (also called the subnet mask in IPv4).<br/>
    ///         Non-contiguous prefixes are not supported.<br/>
    ///         If no prefix length is specified (ie the /nn is omitted), then /32 (for IPv4) or /128 (for IPv6) will be assumed, which
    ///         is a single address match.<br/>
    ///         IPv6 addresses may be in any valid long or shortened form compliant with RFC 4291.
    ///     </para>
    ///     <para>
    ///         The special value `*` (a single asterisk character), an empty string, or an all-whitespace string are equivalent and will
    ///         match all addresses for both IPv4 and IPv6.<br/>
    ///         When used in an access control list, such an entry will be the last entry considered, if that entry is enabled.
    ///     </para>
    ///     <para>
    ///         Prefixes are for matching only and are not dependent on the prefix length configured on the host being matched.
    ///     </para>
    ///     <para>
    ///         Hostnames and port numbers are not permitted.
    ///     </para>
    ///     <para>
    ///         Only ONE value is valid per <see cref="NetworkAccessControlEntry"/> record.
    ///     </para>
    /// </remarks>
    [JsonRequired]
    public required string Prefix { get; set; } = Prefix;
}
