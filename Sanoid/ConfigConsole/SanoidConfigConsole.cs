// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.24.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui.Trees;

namespace Sanoid.ConfigConsole
{
    using System;
    using Terminal.Gui;

    public partial class SanoidConfigConsole
    {
        public SanoidConfigConsole( )
        {
            InitializeComponent( );

            resetGlobalConfigButton.Clicked += SetGlobalSettingsFieldsFromSettingsObject;
            saveGlobalConfigButton.Clicked += ShowSaveGlobalConfigDialog;
            quitMenuItem.Action = ( ) => Application.RequestStop( );
            SetGlobalSettingsFieldsFromSettingsObject( );
            zfsConfigurationRefreshButton.Clicked += RefreshZfsConfigurationTreeViewFromZfs;
            zfsConfigurationTreeView.SelectionChanged += ZfsConfigurationTreeViewOnSelectionChanged;
        }

        private void ZfsConfigurationTreeViewOnSelectionChanged( object sender, SelectionChangedEventArgs<ITreeNode> e )
        {
            ClearAllZfsPropertyFields( );

            if ( e.NewValue is SanoidZfsDataset ds )
            {
                SetZfsCommonPropertyFields( ds );
            }

        }

        private void ClearAllZfsPropertyFields( )
        {
            zfsConfigurationPropertiesNameTextField.Clear();
            zfsConfigurationPropertiesTypeTextField.Clear();
        }
        private void SetZfsCommonPropertyFields( [NotNull] SanoidZfsDataset ds)
        {
            ArgumentNullException.ThrowIfNull( ds, nameof( ds ) );

            zfsConfigurationPropertiesNameTextField.Text = ds.Name;
            zfsConfigurationPropertiesTypeTextField.Text = ds.Kind;
        }

        private async void RefreshZfsConfigurationTreeViewFromZfs( )
        {
            zfsConfigurationTreeView.ClearObjects();
            zfsConfigurationTreeView.AddObjects( await ZfsTasks.GetFullZfsConfigurationTreeAsync( ConfigConsole.Datasets, ConfigConsole.Snapshots, ConfigConsole.CommandRunner ).ConfigureAwait( true ) );
        }

        private void ShowSaveGlobalConfigDialog( )
        {
            if ( ValidateGlobalConfigValues( ) )
            {
                using ( SaveDialog globalConfigSaveDialog = new( "Save GLobal Configuration", "Select file to save global configuration", new( ) { ".json" } ) )
                {
                    globalConfigSaveDialog.AllowsOtherFileTypes = true;
                    globalConfigSaveDialog.CanCreateDirectories = true;
                    globalConfigSaveDialog.Modal = true;
                    Application.Run( globalConfigSaveDialog );
                    if ( globalConfigSaveDialog.Canceled )
                    {
                        return;
                    }

                    if ( globalConfigSaveDialog.FileName.IsEmpty )
                    {
                        return;
                    }

                    SanoidSettings settings = ConfigConsole.Settings;
                    settings.DryRun = dryRunRadioGroup.SelectedItem == 0;
                    settings.TakeSnapshots = takeSnapshotsRadioGroup.SelectedItem == 0;
                    settings.PruneSnapshots = pruneSnapshotsRadioGroup.SelectedItem == 0;
                    settings.ZfsPath = pathToZfsTextField.Text.ToString( );
                    settings.ZpoolPath = pathToZpoolTextField.Text.ToString( );
                    settings.Formatting.ComponentSeparator = snapshotNameComponentSeparatorValidatorField.Text.ToString( );
                    settings.Formatting.Prefix = snapshotNamePrefixTextField.Text.ToString( );
                    settings.Formatting.TimestampFormatString = snapshotNameTimestampFormatTextField.Text.ToString( );
                    settings.Formatting.FrequentSuffix = snapshotNameFrequentSuffixTextField.Text.ToString( );
                    settings.Formatting.HourlySuffix = snapshotNameHourlySuffixTextField.Text.ToString( );
                    settings.Formatting.DailySuffix = snapshotNameDailySuffixTextField.Text.ToString( );
                    settings.Formatting.WeeklySuffix = snapshotNameWeeklySuffixTextField.Text.ToString( );
                    settings.Formatting.MonthlySuffix = snapshotNameMonthlySuffixTextField.Text.ToString( );
                    settings.Formatting.YearlySuffix = snapshotNameYearlySuffixTextField.Text.ToString( ) ?? throw new InvalidOperationException( );


                    File.WriteAllText( globalConfigSaveDialog.FileName.ToString( ), JsonSerializer.Serialize( ConfigConsole.Settings, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
                }
            }
        }

        private bool ValidateGlobalConfigValues( )
        {
            if ( pathToZfsTextField.Text.IsEmpty )
            {
                return false;
            }

            return true;
        }

        private void SetGlobalSettingsFieldsFromSettingsObject( )
        {
            dryRunRadioGroup.SelectedItem = ConfigConsole.Settings.DryRun ? 0 : 1;
            takeSnapshotsRadioGroup.SelectedItem = ConfigConsole.Settings.TakeSnapshots ? 0 : 1;
            pruneSnapshotsRadioGroup.SelectedItem = ConfigConsole.Settings.PruneSnapshots ? 0 : 1;
            pathToZfsTextField.Text = ConfigConsole.Settings.ZfsPath;
            pathToZpoolTextField.Text = ConfigConsole.Settings.ZpoolPath;

            snapshotNameComponentSeparatorValidatorField.Text = ConfigConsole.Settings.Formatting.ComponentSeparator;
            snapshotNamePrefixTextField.Text = ConfigConsole.Settings.Formatting.Prefix;
            snapshotNameTimestampFormatTextField.Text = ConfigConsole.Settings.Formatting.TimestampFormatString;
            snapshotNameFrequentSuffixTextField.Text = ConfigConsole.Settings.Formatting.FrequentSuffix;
            snapshotNameHourlySuffixTextField.Text = ConfigConsole.Settings.Formatting.HourlySuffix;
            snapshotNameDailySuffixTextField.Text = ConfigConsole.Settings.Formatting.DailySuffix;
            snapshotNameWeeklySuffixTextField.Text = ConfigConsole.Settings.Formatting.WeeklySuffix;
            snapshotNameMonthlySuffixTextField.Text = ConfigConsole.Settings.Formatting.MonthlySuffix;
            snapshotNameYearlySuffixTextField.Text = ConfigConsole.Settings.Formatting.YearlySuffix;
        }
    }
}
