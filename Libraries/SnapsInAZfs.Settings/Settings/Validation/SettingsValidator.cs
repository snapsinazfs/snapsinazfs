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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
///     Record type for results of run-time configuration validation.
/// </summary>
/// <remarks>
///     This type is not versioned and may not be binary-compatible between different SIAZ versions. It should not be serialized
///     except for logging.<br/>
///     Note that the default state of this type has <see cref="ValidationErrors"/> = 1 and <see cref="IsSettingsObjectNull"/> =
///     <see langword="true"/>, which are handled appropriately by normal logic once validation is performed against a non-null
///     instance of <see cref="SnapsInAZfsSettings"/>.
/// </remarks>
[ComponentGuarantees ( ComponentGuaranteesOptions.None )]
public partial record SettingsValidator
{
    private static readonly Logger Logger = LogManager.GetLogger ( $"SnapsInAZfs.Settings.{nameof (SettingsValidator)}" )!;

    // Initial state has 1 error, which is null settings object, which will be cleared the first time
    // validation is performed against a non-null settings object.
    private bool _isLocalSystemNameInvalid;
    private bool _isSettingsObjectNull = true;
    private bool _isZfsPathInvalid;
    private bool _isZpoolPathInvalid;
    private int  _validationErrorCount = 1;

    /// <summary>
    ///     Gets or sets a <see cref="Boolean"/> value indicating if the value of the <see cref="SnapsInAZfsSettings.LocalSystemName"/>
    ///     setting was set to '*', indicating that auto-detection and configuration of <see cref="SnapsInAZfsSettings.LocalSystemName"/>
    ///     based on the local system's fully qualified domain name has been requested.
    /// </summary>
    /// <remarks>
    ///     If this property is <see langword="true"/> after validation, the
    ///     <see cref="SnapsInAZfsSettings.AutoDetectAndSetLocalSystemName"/> method should be called on the settings object and then
    ///     validation should be performed again before using the <see cref="SnapsInAZfsSettings.LocalSystemName"/> value for normal
    ///     operation.
    /// </remarks>
    public bool IsAutoConfigureLocalSystemNameRequested { get; private set; }

    /// <summary>
    ///     Gets a <see cref="Boolean"/> value indicating if the value of the <see cref="SnapsInAZfsSettings.ZfsPath"/>
    ///     setting was set to '*', indicating that auto-detection and configuration of <see cref="SnapsInAZfsSettings.ZfsPath"/>
    ///     based on the local system's fully qualified domain name has been requested.
    /// </summary>
    /// <remarks>
    ///     If this property is <see langword="true"/> after validation, the
    ///     <see cref="SnapsInAZfsSettings.AutoDetectAndSetZfsPath"/> method should be called on the settings object and then
    ///     validation should be performed again before using the <see cref="SnapsInAZfsSettings.ZfsPath"/> value for normal
    ///     operation.
    /// </remarks>
    public bool IsAutoConfigureZfsPathRequested { get; private set; }

    /// <summary>
    ///     Gets a <see cref="Boolean"/> value indicating if the value of the <see cref="SnapsInAZfsSettings.ZpoolPath"/>
    ///     setting was set to '*', indicating that auto-detection and configuration of <see cref="SnapsInAZfsSettings.ZpoolPath"/>
    ///     based on the local system's fully qualified domain name has been requested.
    /// </summary>
    /// <remarks>
    ///     If this property is <see langword="true"/> after validation, the
    ///     <see cref="SnapsInAZfsSettings.AutoDetectAndSetZpoolPath"/> method should be called on the settings object and then
    ///     validation should be performed again before using the <see cref="SnapsInAZfsSettings.ZpoolPath"/> value for normal
    ///     operation.
    /// </remarks>
    public bool IsAutoConfigureZpoolPathRequested { get; private set; }

    /// <summary>
    ///     Gets a boolean indicating if the count of validation errors is 0 (<see langword="false"/>) or non-zero
    ///     (<see langword="true"/>).
    /// </summary>
    /// <remarks>
    ///     If the error count is negative, another property has a bug.
    /// </remarks>
    public bool IsInvalid => ValidationErrors != 0;

    /// <summary>
    ///     Gets or sets a <see cref="Boolean"/> value indicating if the value of the <see cref="SnapsInAZfsSettings.LocalSystemName"/>
    ///     setting is invalid.
    /// </summary>
    public bool IsLocalSystemNameInvalid
    {
        get => _isLocalSystemNameInvalid;
        set
        {
            if ( value == _isLocalSystemNameInvalid && !_isSettingsObjectNull )
            {
                return;
            }

            _isLocalSystemNameInvalid = value;

            if ( value )
            {
                Interlocked.Increment ( ref _validationErrorCount );
            }
            else
            {
                Interlocked.Decrement ( ref _validationErrorCount );
            }
        }
    }

    /// <summary>
    ///     Gets or sets a <see cref="Boolean"/> value indicating if the <see cref="SnapsInAZfsSettings"/> reference was NOT a null
    ///     reference.
    /// </summary>
    public bool IsSettingsObjectNull
    {
        get => _isSettingsObjectNull;
        set
        {
            if ( value == _isSettingsObjectNull )
            {
                return;
            }

            _isSettingsObjectNull = value;

            if ( value )
            {
                Interlocked.Exchange ( ref _validationErrorCount, 1 );
            }
            else
            {
                Interlocked.Decrement ( ref _validationErrorCount );
            }
        }
    }

    /// <summary>
    ///     Gets or sets a <see cref="Boolean"/> value indicating if the value of the <see cref="SnapsInAZfsSettings.ZfsPath"/>
    ///     setting is invalid.
    /// </summary>
    public bool IsZfsPathInvalid
    {
        get => _isZfsPathInvalid;
        set
        {
            if ( value == _isZfsPathInvalid || _isSettingsObjectNull )
            {
                return;
            }

            _isZfsPathInvalid = value;

            if ( value )
            {
                Interlocked.Increment ( ref _validationErrorCount );
            }
            else
            {
                Interlocked.Decrement ( ref _validationErrorCount );
            }
        }
    }

    /// <summary>
    ///     Gets or sets a <see cref="Boolean"/> value indicating if the value of the <see cref="SnapsInAZfsSettings.ZpoolPath"/>
    ///     setting is invalid.
    /// </summary>
    public bool IsZpoolPathInvalid
    {
        get => _isZpoolPathInvalid;
        set
        {
            if ( value == _isZpoolPathInvalid || _isSettingsObjectNull )
            {
                return;
            }

            _isZpoolPathInvalid = value;

            if ( value )
            {
                Interlocked.Increment ( ref _validationErrorCount );
            }
            else
            {
                Interlocked.Decrement ( ref _validationErrorCount );
            }
        }
    }

    public int ValidationErrors => _validationErrorCount;

    public void AutoDetectAndSetLocalSystemName ( ref readonly SnapsInAZfsSettings settings )
    {
        if ( IsAutoConfigureLocalSystemNameRequested )
        {
            settings.AutoDetectAndSetLocalSystemName ( );
            Logger.Info ( $"Using auto-detected FQDN value `{settings.LocalSystemName}` for {nameof (SnapsInAZfsSettings.LocalSystemName)} during this instance of SnapsInAZfs." );

            return;
        }

        Logger.Debug ( $"Automatic detection of {nameof (SnapsInAZfsSettings.LocalSystemName)} not requested by configuration. No changes made to current value, \"{settings.LocalSystemName}\"." );
    }

    /// <summary>
    ///     Performs validation of the properties exposed by this type, using the provided <paramref name="settings"/> reference as the
    ///     configuration to validate
    /// </summary>
    /// <param name="settings">The <see cref="SnapsInAZfsSettings"/> instance to validate.</param>
    /// <param name="previousValidator">
    ///     Optional parameter which, if not null, causes the IsAutoConfigureXRequested values to be copied to the returned validator
    ///     before returning, to preserve auto-detect information.
    /// </param>
    /// <returns>
    ///     A new <see cref="SettingsValidator"/> object with all <see cref="Boolean"/> values set according to defined validation
    ///     logic.
    /// </returns>
    /// <remarks>
    ///     Does not modify <paramref name="settings"/>.<br/>
    ///     Logs specific errors for each validation issue encountered and validates all properties for which this type has an IsXValid
    ///     flag, before returning.
    /// </remarks>
    public static SettingsValidator Validate ( ref readonly SnapsInAZfsSettings settings, SettingsValidator? previousValidator = null )
    {
        Logger.Trace ( $"Clean validation of {nameof (SnapsInAZfsSettings)} instance requested." );

        SettingsValidator validator = new ( );

        if ( settings is not { } nonNullSettings )
        {
            Logger.Error ( "Null reference provided to settings validator. SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
            validator.IsSettingsObjectNull = true;

            return validator;
        }

        Logger.Debug ( $"{nameof (SnapsInAZfsSettings)} to validate: {JsonSerializer.Serialize ( nonNullSettings )}" );

        validator.ValidateLocalSystemName ( nonNullSettings.LocalSystemName );
        validator.ValidateZfsPath ( nonNullSettings.ZfsPath );
        validator.ValidateZpoolPath ( nonNullSettings.ZpoolPath );

        if ( previousValidator is { } )
        {
            validator.IsAutoConfigureLocalSystemNameRequested = previousValidator.IsAutoConfigureLocalSystemNameRequested;
            validator.IsAutoConfigureZfsPathRequested         = previousValidator.IsAutoConfigureZfsPathRequested;
            validator.IsAutoConfigureZpoolPathRequested       = previousValidator.IsAutoConfigureZpoolPathRequested;
        }

        return validator;
    }

    /// <summary>
    ///     Source-generated regex for validating the <see cref="SnapsInAZfsSettings.LocalSystemName"/> property value.
    /// </summary>
    [GeneratedRegex ( @"^(?:[a-zA-Z0-9_-]+\.)*[a-zA-Z0-9_-]+\.?$", RegexOptions.Singleline | RegexOptions.CultureInvariant )]
    private static partial Regex LocalSystemNameRegex ( );

    /// <summary>
    ///     Checks the provided string for invalid values or explicit auto-detect and returns a boolean indicating if it was determined
    ///     to be invalid.
    /// </summary>
    /// <param name="localSystemName">
    ///     The string to validate against the rules for the <see cref="SnapsInAZfsSettings.LocalSystemName"/> setting.
    /// </param>
    /// <returns>
    ///     <see langword="true"/>, if the string is invalid for the <see cref="SnapsInAZfsSettings.LocalSystemName"/> setting;
    ///     <see langword="false"/>, otherwise.
    /// </returns>
    private void ValidateLocalSystemName ( [NotNullWhen ( false )] string? localSystemName )
    {
        if ( string.IsNullOrWhiteSpace ( localSystemName ) )
        {
            Logger.Error ( $"Missing, empty, or all-whitespace value for {nameof (SnapsInAZfsSettings.LocalSystemName)}. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
            IsLocalSystemNameInvalid = true;

            return;
        }

        switch ( localSystemName )
        {
            case SnapsInAZfsSettings.AutoDetectSpecifier:
                Logger.Debug ( $"Auto-detection of {nameof (SnapsInAZfsSettings.LocalSystemName)} requested by configuration." );
                IsAutoConfigureLocalSystemNameRequested = true;
                IsLocalSystemNameInvalid                = false;

                return;
            case { Length: > 255 }:
                Logger.Error ( $"{nameof (SnapsInAZfsSettings.LocalSystemName)} value \"{localSystemName}\" is longer than the maximum allowed length of 255 characters. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
                IsLocalSystemNameInvalid = true;

                return;
            case { } when LocalSystemNameRegex ( ).IsMatch ( localSystemName ):
                Logger.Debug ( $"{nameof (SnapsInAZfsSettings.LocalSystemName)} value \"{localSystemName}\" does not appear to be invalid." );
                IsLocalSystemNameInvalid = false;

                return;
            default:
                Logger.Error ( $"Invalid value for {nameof (SnapsInAZfsSettings.LocalSystemName)}. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
                IsLocalSystemNameInvalid = true;

                return;
        }
    }

    private void ValidateZfsPath ( string zfsPath )
    {
        if ( string.IsNullOrWhiteSpace ( zfsPath ) )
        {
            Logger.Error ( $"Missing, empty, or all-whitespace value for {nameof (SnapsInAZfsSettings.ZfsPath)}. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
            IsZfsPathInvalid = true;

            return;
        }

        switch ( zfsPath )
        {
            case SnapsInAZfsSettings.AutoDetectSpecifier:
                Logger.Debug ( $"Auto-detection of {nameof (SnapsInAZfsSettings.ZfsPath)} requested by configuration." );
                IsAutoConfigureZfsPathRequested = true;
                IsZfsPathInvalid                = false;

                return;
            case { } when File.Exists ( zfsPath ):
                Logger.Debug ( $"{nameof (SnapsInAZfsSettings.ZfsPath)} value \"{zfsPath}\" does not appear to be invalid." );
                IsZfsPathInvalid = false;

                return;
            default:
                Logger.Error ( $"Invalid value for {nameof (SnapsInAZfsSettings.ZfsPath)}. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
                IsZfsPathInvalid = true;

                return;
        }
    }

    private void ValidateZpoolPath ( string zpoolPath )
    {
        if ( string.IsNullOrWhiteSpace ( zpoolPath ) )
        {
            Logger.Error ( $"Missing, empty, or all-whitespace value for {nameof (SnapsInAZfsSettings.ZpoolPath)}. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
            IsZpoolPathInvalid = true;

            return;
        }

        switch ( zpoolPath )
        {
            case SnapsInAZfsSettings.AutoDetectSpecifier:
                Logger.Debug ( $"Auto-detection of {nameof (SnapsInAZfsSettings.ZpoolPath)} requested by configuration." );
                IsAutoConfigureZpoolPathRequested = true;
                IsZpoolPathInvalid                = false;

                return;
            case { } when File.Exists ( zpoolPath ):
                Logger.Debug ( $"{nameof (SnapsInAZfsSettings.ZpoolPath)} value \"{zpoolPath}\" does not appear to be invalid." );
                IsZpoolPathInvalid = false;

                return;
            default:
                Logger.Error ( $"Invalid value for {nameof (SnapsInAZfsSettings.ZpoolPath)}. This setting is required and must be valid or SIAZ will terminate except when run in configuration mode (`--config-console` option). See documentation for requirements." );
                IsZpoolPathInvalid = true;

                return;
        }
    }
}
