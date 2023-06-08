#nullable enable
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
        private SanoidZfsDataset? _zfsConfigurationCurrentSelectedItem;
        List<string> _templateListItems = ConfigConsole.Settings!.Templates.Keys.ToList( );
        private bool _eventsEnabled = false;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public SanoidConfigConsole( )
        {
            InitializeComponent( );

            quitMenuItem.Action = ( ) => Application.RequestStop( );

            SetGlobalSettingsFieldsFromSettingsObject( );
            configCategoryTabView.SelectedTabChanged += ConfigCategoryTabViewOnSelectedTabChanged;
            HideZfsConfigurationPropertyFrames( );
            SetTabStopsForRootLevelObjects( );
            SetCanFocusStateForZfsConfigurationViews( );
            SetTabStopsForZfsConfigurationWindow( );
            SetPropertiesForReadonlyFields( );

            EnableEventHandlers();
        }

        private void HideZfsConfigurationPropertyFrames( )
        {
            zfsConfigurationPropertiesFrame.Visible = false;
        }
        private void ShowZfsConfigurationPropertyFrames( )
        {
            zfsConfigurationPropertiesFrame.Visible = true;
        }

        private void ConfigCategoryTabViewOnSelectedTabChanged( object? sender, TabView.TabChangedEventArgs e )
        {
            if ( e.NewTab.View.Text == "ZFS Configuration" )
            {
                zfsConfigurationTreeView.SetFocus( );
            }
        }

        private void SetTabStopsForRootLevelObjects( )
        {
            configCategoryTabView.TabStop = true;
            configCategoryTabView.CanFocus = true;
        }

        private void SetTabStopsForZfsConfigurationWindow( )
        {
            zfsConfigurationWindow.TabStop = false;
            zfsConfigurationTreeFrame.TabStop = false;
            zfsConfigurationPropertiesFrame.TabStop = false;
            zfsConfigurationCommonPropertiesFrame.TabStop = false;
            zfsConfigurationSnapshotPropertiesFrame.TabStop = false;
            zfsConfigurationActionsFrame.TabStop = false;
            zfsConfigurationPropertiesNameLabel.TabStop = false;
            zfsConfigurationPropertiesTypeLabel.TabStop = false;
            zfsConfigurationPropertiesEnabledLabel.TabStop = false;
            zfsConfigurationPropertiesEnabledSourceLabel.TabStop = false;
            zfsConfigurationPropertiesEnabledSourceTextField.TabStop = false;
            zfsConfigurationPropertiesTakeSnapshotsLabel.TabStop = false;
            zfsConfigurationPropertiesTakeSnapshotsSourceLabel.TabStop = false;
            zfsConfigurationPropertiesTakeSnapshotsSourceTextField.TabStop = false;
            zfsConfigurationPropertiesPruneSnapshotsLabel.TabStop = false;
            zfsConfigurationPropertiesPruneSnapshotsSourceLabel.TabStop = false;
            zfsConfigurationPropertiesPruneSnapshotsSourceTextField.TabStop = false;
            zfsConfigurationPropertiesTemplateLabel.TabStop = false;
            zfsConfigurationPropertiesTemplateSourceLabel.TabStop = false;
            zfsConfigurationPropertiesTemplateSourceTextField.TabStop = false;
            zfsConfigurationPropertiesRecursionLabel.TabStop = false;
            zfsConfigurationPropertiesRecursionSourceLabel.TabStop = false;
            zfsConfigurationPropertiesRecursionSourceTextField.TabStop = false;

            zfsConfigurationTreeView.TabStop = true;
            zfsConfigurationTreeView.TabIndex = 0;
            zfsConfigurationTreeView.CanFocus = true;

            zfsConfigurationPropertiesEnabledRadioGroup.TabStop = true;
            zfsConfigurationPropertiesEnabledRadioGroup.TabIndex = 1;
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.TabStop = true;
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.TabIndex = 2;
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.TabStop = true;
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.TabIndex = 3;
            zfsConfigurationPropertiesTemplateTextField.TabStop = true;
            zfsConfigurationPropertiesTemplateTextField.TabIndex = 4;
        }

        private void SetCanFocusStateForZfsConfigurationViews( )
        {
            zfsConfigurationTreeFrame.CanFocus = false;
            zfsConfigurationActionsFrame.CanFocus = false;
            zfsConfigurationPropertiesFrame.CanFocus = false;
            zfsConfigurationCommonPropertiesFrame.CanFocus = false;
            zfsConfigurationSnapshotPropertiesFrame.CanFocus = false;

            zfsConfigurationPropertiesRetentionFrequentTextField.Enabled = true;
            zfsConfigurationPropertiesRetentionFrequentTextField.CanFocus = true;
            zfsConfigurationPropertiesRetentionHourlyTextField.Enabled = true;
            zfsConfigurationPropertiesRetentionHourlyTextField.CanFocus = true;
            zfsConfigurationPropertiesRetentionDailyTextField.Enabled = true;
            zfsConfigurationPropertiesRetentionDailyTextField.CanFocus = true;
            zfsConfigurationPropertiesRetentionWeeklyTextField.Enabled = true;
            zfsConfigurationPropertiesRetentionWeeklyTextField.CanFocus = true;
            zfsConfigurationPropertiesRetentionMonthlyTextField.Enabled = true;
            zfsConfigurationPropertiesRetentionMonthlyTextField.CanFocus = true;
            zfsConfigurationPropertiesRetentionYearlyTextField.Enabled = true;
            zfsConfigurationPropertiesRetentionYearlyTextField.CanFocus = true;
        }

        private void SetPropertiesForReadonlyFields( )
        {
            zfsConfigurationPropertiesNameTextField.ReadOnly = true;
            zfsConfigurationPropertiesNameTextField.CanFocus = false;
            zfsConfigurationPropertiesTypeTextField.ReadOnly = true;
            zfsConfigurationPropertiesTypeTextField.CanFocus = false;
            zfsConfigurationPropertiesEnabledSourceTextField.ReadOnly = true;
            zfsConfigurationPropertiesEnabledSourceTextField.CanFocus = false;
            zfsConfigurationPropertiesTakeSnapshotsSourceTextField.ReadOnly = true;
            zfsConfigurationPropertiesTakeSnapshotsSourceTextField.CanFocus = false;
            zfsConfigurationPropertiesPruneSnapshotsSourceTextField.ReadOnly = true;
            zfsConfigurationPropertiesPruneSnapshotsSourceTextField.CanFocus = false;
            zfsConfigurationPropertiesTemplateSourceTextField.ReadOnly = true;
            zfsConfigurationPropertiesTemplateSourceTextField.CanFocus = false;
            zfsConfigurationPropertiesRecursionSourceTextField.ReadOnly = true;
            zfsConfigurationPropertiesRecursionSourceTextField.CanFocus = false;

            zfsConfigurationPropertiesRecentFrequentTextField.CanFocus = false;
            zfsConfigurationPropertiesRecentFrequentTextField.ReadOnly = true;
            zfsConfigurationPropertiesRecentHourlyTextField.CanFocus = false;
            zfsConfigurationPropertiesRecentHourlyTextField.ReadOnly = true;
            zfsConfigurationPropertiesRecentDailyTextField.CanFocus = false;
            zfsConfigurationPropertiesRecentDailyTextField.ReadOnly = true;
            zfsConfigurationPropertiesRecentWeeklyTextField.CanFocus = false;
            zfsConfigurationPropertiesRecentWeeklyTextField.ReadOnly = true;
            zfsConfigurationPropertiesRecentMonthlyTextField.CanFocus = false;
            zfsConfigurationPropertiesRecentMonthlyTextField.ReadOnly = true;
            zfsConfigurationPropertiesRecentYearlyTextField.CanFocus = false;
            zfsConfigurationPropertiesRecentYearlyTextField.ReadOnly = true;
        }

        private void DisableEventHandlers( )
        {
            if ( !_eventsEnabled )
                return;
            Logger.Debug( "Disabling event handlers for zfs configuration fields" );
            resetGlobalConfigButton.Clicked -= SetGlobalSettingsFieldsFromSettingsObject;
            saveGlobalConfigButton.Clicked -= ShowSaveGlobalConfigDialog;
            zfsConfigurationRefreshButton.Clicked -= RefreshZfsConfigurationTreeViewFromZfs;
            zfsConfigurationResetCurrentButton.Clicked -= ZfsConfigurationResetCurrentButtonOnClicked;
            zfsConfigurationTreeView.SelectionChanged -= ZfsConfigurationTreeViewOnSelectionChanged;
            zfsConfigurationPropertiesEnabledRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged;
            zfsConfigurationPropertiesEnabledRadioGroup.MouseClick -= ZfsConfigurationPropertiesEnabledRadioGroupOnMouseClick;
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged;
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.MouseClick -= ZfsConfigurationPropertiesEnabledRadioGroupOnMouseClick;
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged;
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.MouseClick -= ZfsConfigurationPropertiesEnabledRadioGroupOnMouseClick;
            zfsConfigurationSaveCurrentButton.Clicked -= ZfsConfigurationSaveCurrentButtonOnClicked;
            _eventsEnabled = false;
            Logger.Debug( "Event handlers for zfs configuration fields disabled" );
        }

        private void EnableEventHandlers( )
        {
            if ( _eventsEnabled )
                return;
            Logger.Debug( "Enabling event handlers for zfs configuration fields" );
            resetGlobalConfigButton.Clicked += SetGlobalSettingsFieldsFromSettingsObject;
            saveGlobalConfigButton.Clicked += ShowSaveGlobalConfigDialog;
            zfsConfigurationRefreshButton.Clicked += RefreshZfsConfigurationTreeViewFromZfs;
            zfsConfigurationResetCurrentButton.Clicked += ZfsConfigurationResetCurrentButtonOnClicked ;
            zfsConfigurationTreeView.SelectionChanged += ZfsConfigurationTreeViewOnSelectionChanged;
            zfsConfigurationPropertiesEnabledRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged;
            zfsConfigurationPropertiesEnabledRadioGroup.MouseClick += ZfsConfigurationPropertiesEnabledRadioGroupOnMouseClick;
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesTakeSnapshotsRadioGroup_SelectedItemChanged;
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.MouseClick += ZfsConfigurationPropertiesTakeSnapshotsRadioGroupOnMouseClick;
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesPruneSnapshotsRadioGroup_SelectedItemChanged;
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.MouseClick += ZfsConfigurationPropertiesPruneSnapshotsRadioGroupOnMouseClick;
            zfsConfigurationSaveCurrentButton.Clicked += ZfsConfigurationSaveCurrentButtonOnClicked;
            _eventsEnabled = true;
            Logger.Debug( "Event handlers for zfs configuration fields enabled" );
        }

        private void ZfsConfigurationResetCurrentButtonOnClicked( )
        {
            DisableEventHandlers();
            ClearAllZfsPropertyFields(false);
            UpdateZfsCommonPropertyFieldsForCurrentlySelectedObject( );
            EnableEventHandlers();
        }

        private void ZfsConfigurationSaveCurrentButtonOnClicked( )
        {
            if ( ValidateZfsObjectConfigValues( ) )
            {
                using ( Dialog saveZfsObjectDialog = new( "Confirm Saving ZFS Object Configuration",80,7, new Button("Cancel",true), new Button("Save") ) )
                {
                    saveZfsObjectDialog.ButtonAlignment = Dialog.ButtonAlignments.Center;
                    saveZfsObjectDialog.AutoSize = true;
                    saveZfsObjectDialog.ColorScheme = whiteOnRed;
                    saveZfsObjectDialog.Text = "The following command(s) will be executed: ";
                    saveZfsObjectDialog.Modal = true;
                    Application.Run( saveZfsObjectDialog );
                }
            }
        }

        private bool ValidateZfsObjectConfigValues( )
        {
            return true;
        }

        private void ZfsConfigurationPropertiesEnabledRadioGroupOnMouseClick( MouseEventArgs args )
        {
            ArgumentNullException.ThrowIfNull( args, nameof( args ) );

            zfsConfigurationPropertiesEnabledSourceTextField.Text = "local";
        }

        private void ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
        {
            ArgumentNullException.ThrowIfNull( args, nameof( args ) );

            zfsConfigurationPropertiesEnabledSourceTextField.Text = "local";
        }
        private void ZfsConfigurationPropertiesTakeSnapshotsRadioGroupOnMouseClick( MouseEventArgs args )
        {
            ArgumentNullException.ThrowIfNull( args, nameof( args ) );

            zfsConfigurationPropertiesTakeSnapshotsSourceTextField.Text = "local";
        }

        private void ZfsConfigurationPropertiesTakeSnapshotsRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
        {
            ArgumentNullException.ThrowIfNull( args, nameof( args ) );

            zfsConfigurationPropertiesTakeSnapshotsSourceTextField.Text = "local";
        }
        private void ZfsConfigurationPropertiesPruneSnapshotsRadioGroupOnMouseClick( MouseEventArgs args )
        {
            ArgumentNullException.ThrowIfNull( args, nameof( args ) );

            zfsConfigurationPropertiesPruneSnapshotsSourceTextField.Text = "local";
        }

        private void ZfsConfigurationPropertiesPruneSnapshotsRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
        {
            ArgumentNullException.ThrowIfNull( args, nameof( args ) );

            zfsConfigurationPropertiesPruneSnapshotsSourceTextField.Text = "local";
        }

        private void ZfsConfigurationTreeViewOnSelectionChanged( object? sender, SelectionChangedEventArgs<ITreeNode> e )
        {
            DisableEventHandlers();
            ArgumentNullException.ThrowIfNull( sender );

            ClearAllZfsPropertyFields( );

            if ( e.NewValue is SanoidZfsDataset ds )
            {
                _zfsConfigurationCurrentSelectedItem = ds;
                UpdateZfsCommonPropertyFieldsForCurrentlySelectedObject( );
            }

            EnableEventHandlers( );
        }

        private void ClearAllZfsPropertyFields( bool manageEventHandlers = false )
        {
            if ( manageEventHandlers )
            {
                DisableEventHandlers( );
            }

            zfsConfigurationPropertiesNameTextField.Clear( );
            zfsConfigurationPropertiesTypeTextField.Clear( );
            zfsConfigurationPropertiesEnabledRadioGroup.Clear( );
            zfsConfigurationPropertiesEnabledSourceTextField.Clear( );
            zfsConfigurationPropertiesTakeSnapshotsRadioGroup.Clear( );
            zfsConfigurationPropertiesTakeSnapshotsSourceTextField.Clear( );
            zfsConfigurationPropertiesPruneSnapshotsRadioGroup.Clear( );
            zfsConfigurationPropertiesPruneSnapshotsSourceTextField.Clear( );
            if ( manageEventHandlers )
            {
                EnableEventHandlers( );
            }
        }

        private void UpdateZfsCommonPropertyFieldsForCurrentlySelectedObject( bool manageEventHandlers = true )
        {
            if(manageEventHandlers)
                DisableEventHandlers( );

            if ( _zfsConfigurationCurrentSelectedItem is { } ds )
            {
                ShowZfsConfigurationPropertyFrames();
                zfsConfigurationPropertiesNameTextField.Text = ds.Name;
                zfsConfigurationPropertiesTypeTextField.Text = ds.Kind;
                zfsConfigurationPropertiesEnabledRadioGroup.SelectedItem = ds.Enabled.AsTrueFalseRadioIndex( );
                zfsConfigurationPropertiesEnabledRadioGroup.ColorScheme = ds.Enabled.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
                zfsConfigurationPropertiesEnabledSourceTextField.Text = ds.Enabled.IsInherited ? ds.Enabled.Source[ 15.. ] : ds.Enabled.Source;
                zfsConfigurationPropertiesTakeSnapshotsRadioGroup.SelectedItem = ds.TakeSnapshots.AsTrueFalseRadioIndex( );
                zfsConfigurationPropertiesTakeSnapshotsRadioGroup.ColorScheme = ds.TakeSnapshots.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
                zfsConfigurationPropertiesTakeSnapshotsSourceTextField.Text = ds.TakeSnapshots.IsInherited ? ds.TakeSnapshots.Source[ 15.. ] : ds.TakeSnapshots.Source;
                zfsConfigurationPropertiesPruneSnapshotsRadioGroup.SelectedItem = ds.PruneSnapshots.AsTrueFalseRadioIndex( );
                zfsConfigurationPropertiesPruneSnapshotsRadioGroup.ColorScheme = ds.PruneSnapshots.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
                zfsConfigurationPropertiesPruneSnapshotsSourceTextField.Text = ds.TakeSnapshots.IsInherited ? ds.TakeSnapshots.Source[ 15.. ] : ds.PruneSnapshots.Source;
                zfsConfigurationPropertiesTemplateTextField.Text = ds.Template.Value;
                zfsConfigurationPropertiesTemplateTextField.ColorScheme = ds.Template.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
                zfsConfigurationPropertiesTemplateSourceTextField.Text = ds.Template.IsInherited ? ds.Template.Source[ 15.. ] : ds.Template.Source;
                zfsConfigurationPropertiesRecursionRadioGroup.SelectedItem = ds.Recursion.Value switch { "sanoid" => 0, "zfs" => 1, _ => throw new InvalidOperationException( "Invalid recursion value" ) };
                zfsConfigurationPropertiesRecursionRadioGroup.ColorScheme = ds.Recursion.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
                zfsConfigurationPropertiesRecursionSourceTextField.Text = ds.Recursion.IsInherited ? ds.Recursion.Source[ 15.. ] : ds.Recursion.Source;

                zfsConfigurationPropertiesRetentionFrequentTextField.Text = ds.SnapshotRetentionFrequent.Value.ToString( );
                zfsConfigurationPropertiesRetentionFrequentTextField.ColorScheme = ds.SnapshotRetentionFrequent.IsInherited ? this.inheritedPropertyTextFieldColorScheme : this.localPropertyTextFieldColorScheme;
                zfsConfigurationPropertiesRetentionHourlyTextField.Text = ds.SnapshotRetentionHourly.Value.ToString( );
                zfsConfigurationPropertiesRetentionHourlyTextField.ColorScheme = ds.SnapshotRetentionHourly.IsInherited ? this.inheritedPropertyTextFieldColorScheme : this.localPropertyTextFieldColorScheme;
                zfsConfigurationPropertiesRetentionDailyTextField.Text = ds.SnapshotRetentionDaily.Value.ToString( );
                zfsConfigurationPropertiesRetentionDailyTextField.ColorScheme = ds.SnapshotRetentionDaily.IsInherited ? this.inheritedPropertyTextFieldColorScheme : this.localPropertyTextFieldColorScheme;
                zfsConfigurationPropertiesRetentionWeeklyTextField.Text = ds.SnapshotRetentionWeekly.Value.ToString( );
                zfsConfigurationPropertiesRetentionWeeklyTextField.ColorScheme = ds.SnapshotRetentionWeekly.IsInherited ? this.inheritedPropertyTextFieldColorScheme : this.localPropertyTextFieldColorScheme;
                zfsConfigurationPropertiesRetentionMonthlyTextField.Text = ds.SnapshotRetentionMonthly.Value.ToString( );
                zfsConfigurationPropertiesRetentionMonthlyTextField.ColorScheme = ds.SnapshotRetentionMonthly.IsInherited ? this.inheritedPropertyTextFieldColorScheme : this.localPropertyTextFieldColorScheme;
                zfsConfigurationPropertiesRetentionYearlyTextField.Text = ds.SnapshotRetentionYearly.Value.ToString( );
                zfsConfigurationPropertiesRetentionYearlyTextField.ColorScheme = ds.SnapshotRetentionYearly.IsInherited ? this.inheritedPropertyTextFieldColorScheme : this.localPropertyTextFieldColorScheme;

                zfsConfigurationPropertiesRecentFrequentTextField.Text = ds.LastFrequentSnapshotTimestamp.IsLocal ? ds.LastFrequentSnapshotTimestamp.Value.ToString( "O" ) : string.Empty;
                zfsConfigurationPropertiesRecentHourlyTextField.Text = ds.LastHourlySnapshotTimestamp.IsLocal ? ds.LastHourlySnapshotTimestamp.Value.ToString( "O" ) : string.Empty;
                zfsConfigurationPropertiesRecentDailyTextField.Text = ds.LastDailySnapshotTimestamp.IsLocal ? ds.LastDailySnapshotTimestamp.Value.ToString( "O" ) : string.Empty;
                zfsConfigurationPropertiesRecentWeeklyTextField.Text = ds.LastWeeklySnapshotTimestamp.IsLocal ? ds.LastWeeklySnapshotTimestamp.Value.ToString( "O" ) : string.Empty;
                zfsConfigurationPropertiesRecentMonthlyTextField.Text = ds.LastMonthlySnapshotTimestamp.IsLocal ? ds.LastMonthlySnapshotTimestamp.Value.ToString( "O" ) : string.Empty;
                zfsConfigurationPropertiesRecentYearlyTextField.Text = ds.LastYearlySnapshotTimestamp.IsLocal ? ds.LastYearlySnapshotTimestamp.Value.ToString( "O" ) : string.Empty;
            }
            if(manageEventHandlers)
                EnableEventHandlers( );
        }

        private async void RefreshZfsConfigurationTreeViewFromZfs( )
        {
            Logger.Debug( "Refreshing zfs configuration tree view" );
            DisableEventHandlers( );
            Logger.Debug( "Clearing objects from zfs configuration tree view" );
            zfsConfigurationTreeView.ClearObjects( );
            ConfigConsole.Snapshots.Clear( );
            ConfigConsole.Datasets.Clear( );
            Logger.Debug( "Getting zfs objects from zfs and populating configuration tree view" );
            zfsConfigurationTreeView.AddObjects( await ZfsTasks.GetFullZfsConfigurationTreeAsync( ConfigConsole.Datasets, ConfigConsole.Snapshots, ConfigConsole.CommandRunner! ).ConfigureAwait( true ) );
            zfsConfigurationTreeView.RebuildTree( );
            EnableEventHandlers( );
            zfsConfigurationTreeView.SetFocus( );
            Logger.Debug( "Finished refreshing zfs configuration tree view" );
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

                    SanoidSettings? settings = ConfigConsole.Settings;
                    settings!.DryRun = dryRunRadioGroup.SelectedItem == 0;
                    settings.TakeSnapshots = takeSnapshotsRadioGroup.SelectedItem == 0;
                    settings.PruneSnapshots = pruneSnapshotsRadioGroup.SelectedItem == 0;
                    settings.ZfsPath = pathToZfsTextField.Text?.ToString( ) ?? string.Empty;
                    settings.ZpoolPath = pathToZpoolTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.ComponentSeparator = snapshotNameComponentSeparatorValidatorField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.Prefix = snapshotNamePrefixTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.TimestampFormatString = snapshotNameTimestampFormatTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.FrequentSuffix = snapshotNameFrequentSuffixTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.HourlySuffix = snapshotNameHourlySuffixTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.DailySuffix = snapshotNameDailySuffixTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.WeeklySuffix = snapshotNameWeeklySuffixTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.MonthlySuffix = snapshotNameMonthlySuffixTextField.Text?.ToString( ) ?? string.Empty;
                    settings.Formatting.YearlySuffix = snapshotNameYearlySuffixTextField.Text.ToString( ) ?? throw new InvalidOperationException( );


                    File.WriteAllText( globalConfigSaveDialog.FileName.ToString( ) ?? throw new InvalidOperationException( "Null string provided for save file name"), JsonSerializer.Serialize( ConfigConsole.Settings, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
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
            DisableEventHandlers();

            Logger.Debug( "Setting global configuration fields to values in settings" );

            dryRunRadioGroup.SelectedItem = ConfigConsole.Settings!.DryRun ? 0 : 1;
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

            Logger.Debug( "Finished etting global configuration fields to values in settings" );
            EnableEventHandlers();
        }
    }
}
