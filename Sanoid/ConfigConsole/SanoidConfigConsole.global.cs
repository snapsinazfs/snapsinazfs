// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.ConfigConsole;

public partial class SanoidConfigConsole
{
    private bool _globalConfigurationTabEventsEnabled;

    private void DisableGlobalConfigTabEventHandlers( )
    {
        if ( !_globalConfigurationTabEventsEnabled )
            return;

        resetGlobalConfigButton.Clicked -= GlobalConfigurationResetGlobalConfigButtonOnClicked;
        saveGlobalConfigButton.Clicked -= ShowSaveGlobalConfigDialog;

        _globalConfigurationTabEventsEnabled = false;
    }

    private void EnableGlobalConfigTabEventHandlers( )
    {
        if ( _globalConfigurationTabEventsEnabled )
            return;
        resetGlobalConfigButton.Clicked += GlobalConfigurationResetGlobalConfigButtonOnClicked;
        saveGlobalConfigButton.Clicked += ShowSaveGlobalConfigDialog;
        _globalConfigurationTabEventsEnabled = true;
    }

    private bool GlobalConfigurationValidateGlobalConfigValues( )
    {
        if ( pathToZfsTextField.Text.IsEmpty || pathToZpoolTextField.Text.IsEmpty )
        {
            return false;
        }

        if ( Environment.OSVersion.Platform == PlatformID.Unix && !File.Exists( pathToZfsTextField.Text.ToString( ) ) || !File.Exists( pathToZpoolTextField.Text.ToString( ) ) )
        {
            return false;
        }

        return true;
    }

    private void GlobalConfigurationResetGlobalConfigButtonOnClicked( )
    {
        GlobalConfigurationSetFieldsFromSettingsObject( );
    }

    private void GlobalConfigurationSetFieldsFromSettingsObject( bool manageEventHandlers = true )
    {
        if ( manageEventHandlers )
        {
            DisableGlobalConfigTabEventHandlers( );
        }

        Logger.Debug( "Setting global configuration fields to values in settings" );

        dryRunRadioGroup.SelectedItem = ConfigConsole.Settings.DryRun ? 0 : 1;
        takeSnapshotsRadioGroup.SelectedItem = ConfigConsole.Settings.TakeSnapshots ? 0 : 1;
        pruneSnapshotsRadioGroup.SelectedItem = ConfigConsole.Settings.PruneSnapshots ? 0 : 1;
        pathToZfsTextField.Text = ConfigConsole.Settings.ZfsPath;
        pathToZpoolTextField.Text = ConfigConsole.Settings.ZpoolPath;

        Logger.Debug( "Finished etting global configuration fields to values in settings" );

        if ( manageEventHandlers )
        {
            EnableGlobalConfigTabEventHandlers( );
        }
    }
}
