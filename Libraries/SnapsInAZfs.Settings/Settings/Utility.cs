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

namespace SnapsInAZfs.Settings;

using System.Net.NetworkInformation;

/// <summary>
///     Extension methods and utility methods relevant to functionality in the SnapsInAZfs.Settings library.
/// </summary>
[PublicAPI]
public static class Utility
{
    /// <summary>
    ///     Gets the fully-qualified domain name of the <paramref name="host"/> system.
    /// </summary>
    /// <param name="host">An <see cref="IPGlobalProperties"/> instance for which to return the FQDN.</param>
    /// <returns>
    ///     A <see cref="string"/> containing the fully-qualified domain name of the <paramref name="host"/>, as seen by the runtime.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         If the <paramref name="host"/> does not have a domain name configured, the hostname followed by a dot will be returned.
    ///         <br/>
    ///         This is a valid FQDN but may not be resolvable from other hosts, depending on the environment.
    ///     </para>
    ///     <para>This method is not supported by .net on Illumos or Solaris platforms.</para>
    /// </remarks>
    [UnsupportedOSPlatform ( "illumos" )]
    [UnsupportedOSPlatform ( "solaris" )]
    [Pure]
    public static string GetFullyQualifiedDomainName ( this IPGlobalProperties host ) => $"{host.HostName}.{host.DomainName}";

    /// <summary>
    ///     Gets the fully-qualified domain name of the local system.
    /// </summary>
    /// <returns>
    ///     A <see cref="string"/> containing the fully-qualified domain name of the local system, as seen by the runtime.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         If the local system does not have a domain name configured, the hostname followed by a dot will be returned.
    ///         <br/>
    ///         This is a valid FQDN but may not be resolvable from other hosts, depending on the environment.
    ///     </para>
    ///     <para>This method is not supported by .net on IllumOS or Solaris platforms.</para>
    /// </remarks>
    [UnsupportedOSPlatform ( "illumos" )]
    [UnsupportedOSPlatform ( "solaris" )]
    public static string GetFullyQualifiedDomainName ( ) => IPGlobalProperties.GetIPGlobalProperties ( ).GetFullyQualifiedDomainName ( );

    /// <summary>
    ///     Searches locations in the PATH environment variable for <paramref name="program"/>, if whitelisted, and returns a
    ///     fully-qualified path, if found.
    /// </summary>
    /// <param name="program">
    ///     A program name to attempt to resolve. Restricted to a whitelisted set of values of programs used by SIAZ.
    /// </param>
    /// <returns>
    ///     The fully-qualified and resolved path to a normal file for <paramref name="program"/>, if found, or throws
    ///     <see cref="FileNotFoundException"/> if not found.
    /// </returns>
    /// <exception cref="ApplicationException">If the PATH environment variable is undefined or empty.</exception>
    /// <exception cref="ArgumentException">If <paramref name="program"/> is an empty string or all-whitespace.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="program"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="program"/> is not one of the whitelisted values.</exception>
    /// <exception cref="FileNotFoundException">If <paramref name="program"/> was not found in any locations in PATH.</exception>
    /// <remarks>
    ///     <para>
    ///         If <paramref name="program"/> is found in PATH and is a symbolic link, the link target will be resolved to ensure the
    ///         target exists. If a symbolic link target does not exist, the result is the same as if <paramref name="program"/> were not
    ///         found.
    ///     </para>
    ///     <para>
    ///         Allowed values for <paramref name="program"/> are currently `zfs` and `zpool`.
    ///     </para>
    /// </remarks>
    public static string Which ( string program )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace ( program, nameof (program) );

        string? pathVar = Environment.GetEnvironmentVariable ( "PATH", EnvironmentVariableTarget.Process );

        if ( string.IsNullOrWhiteSpace ( pathVar ) )
        {
            throw new ApplicationException ( "PATH environment variable is empty or is not defined for the current process." );
        }

        ReadOnlySpan<char> pathVarSpan = pathVar.AsSpan ( );

        char pathVarElementSeparator = OperatingSystem.IsWindows ( ) ? ';' : ':';

        switch ( program )
        {
            case "zfs":
            case "zpool":
            {
                // PATH is defined - look for allowedProgram in each element
                foreach ( Range basePathRange in pathVarSpan.Split ( pathVarElementSeparator ) )
                {
                    if ( basePathRange.Start.Equals ( basePathRange.End ) )
                    {
                        // Zero length is always invalid, and is most likely from multiple adjacent separators in PATH.
                        // In any case, we can skip immediately.
                        continue;
                    }

                    string programPathCandidate = Path.Combine ( new ( pathVarSpan [ basePathRange ] ), program );

                    // Check if path plus allowedProgram exists
                    if ( !File.Exists ( programPathCandidate ) )
                    {
                        continue;
                    }

                    // The path exists. Return it if it's a file or resolve and check before returning target if it is a symlink.
                    switch ( File.ResolveLinkTarget ( programPathCandidate, true ) )
                    {
                        case { Exists: true } goodLink:
                            // It was a symlink to a valid file target. Return the fully-resolved target.
                            return goodLink.FullName;
                        case { Exists: false }:
                            // It was a symlink, but the target does not exist.
                            continue;
                        default:
                            // It was a normal file. Return the path candidate
                            return programPathCandidate;
                    }
                }

                throw new FileNotFoundException ( $"Unable to locate {program} on the local system or it is a broken symbolic link. Check that {program} is installed and that the PATH environment variable for the principal executing SIAZ contains the parent directory of {program}. PATH was: {new ( pathVarSpan )}" );
            }

            default:
                throw new ArgumentOutOfRangeException ( nameof (program), $"Specified program `{program}` is not allowed." );
        }
    }
}
