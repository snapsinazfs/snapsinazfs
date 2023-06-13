// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using NStack;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace Sanoid.ConfigConsole;

public partial class SanoidConfigConsole
{
    private readonly ConcurrentDictionary<string, SanoidZfsDataset> _baseDatasets = new( );
    private readonly ConcurrentDictionary<string, IZfsProperty> _modifiedPropertiesSinceLastSaveForCurrentItem = new( );
    private readonly ConcurrentDictionary<string, SanoidZfsDataset> _treeDatasets = new( );
    private bool _zfsConfigurationEventsEnabled;
    private ZfsObjectConfigurationTreeNode SelectedTreeNode => (ZfsObjectConfigurationTreeNode)zfsConfigurationTreeView.SelectedObject;

    private void UpdateZfsConfigurationButtonState( )
    {
        if ( zfsConfigurationTreeView.Objects.Any( ) && zfsConfigurationTreeView.SelectedObject is ZfsObjectConfigurationTreeNode node )
        {
            zfsConfigurationResetCurrentButton.Enabled = zfsConfigurationSaveCurrentButton.Enabled = node.BaseDataset != node.TreeDataset;
        }
        else
        {
            zfsConfigurationResetCurrentButton.Enabled = false;
            zfsConfigurationSaveCurrentButton.Enabled = false;
        }
    }

    private void ShowZfsConfigurationPropertyFrames( )
    {
        zfsConfigurationPropertiesFrame.Visible = true;
        if ( SelectedTreeNode.TreeDataset.Kind == "snapshot" )
        {
            zfsConfigurationSnapshotPropertiesFrame.Visible = true;
        }
    }

    private void HideZfsConfigurationPropertyFrames( )
    {
        zfsConfigurationPropertiesFrame.Visible = false;
        zfsConfigurationSnapshotPropertiesFrame.Visible = false;
    }

    private void SetTagsForZfsPropertyFields( )
    {
        zfsConfigurationPropertiesEnabledRadioGroup.Data = new RadioGroupWithSourceViewData( ZfsPropertyNames.EnabledPropertyName, zfsConfigurationPropertiesEnabledRadioGroup, zfsConfigurationPropertiesEnabledSourceTextField );
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.Data = new RadioGroupWithSourceViewData( ZfsPropertyNames.TakeSnapshotsPropertyName, zfsConfigurationPropertiesTakeSnapshotsRadioGroup, zfsConfigurationPropertiesTakeSnapshotsSourceTextField );
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.Data = new RadioGroupWithSourceViewData( ZfsPropertyNames.PruneSnapshotsPropertyName, zfsConfigurationPropertiesPruneSnapshotsRadioGroup, zfsConfigurationPropertiesPruneSnapshotsSourceTextField );
        zfsConfigurationPropertiesRecursionRadioGroup.Data = new RadioGroupWithSourceViewData( ZfsPropertyNames.RecursionPropertyName, zfsConfigurationPropertiesRecursionRadioGroup, zfsConfigurationPropertiesRecursionSourceTextField );
        zfsConfigurationPropertiesTemplateTextField.Data = new TextFieldWithSourceViewData( ZfsPropertyNames.TemplatePropertyName, zfsConfigurationPropertiesTemplateTextField, zfsConfigurationPropertiesTemplateSourceTextField );
        zfsConfigurationPropertiesRetentionFrequentTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, zfsConfigurationPropertiesRetentionFrequentTextField, 0, int.MaxValue );
        zfsConfigurationPropertiesRetentionHourlyTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, zfsConfigurationPropertiesRetentionHourlyTextField, 0, int.MaxValue );
        zfsConfigurationPropertiesRetentionDailyTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, zfsConfigurationPropertiesRetentionDailyTextField, 0, int.MaxValue );
        zfsConfigurationPropertiesRetentionWeeklyTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, zfsConfigurationPropertiesRetentionWeeklyTextField, 0, int.MaxValue );
        zfsConfigurationPropertiesRetentionMonthlyTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, zfsConfigurationPropertiesRetentionMonthlyTextField, 0, int.MaxValue );
        zfsConfigurationPropertiesRetentionYearlyTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, zfsConfigurationPropertiesRetentionYearlyTextField, 0, int.MaxValue );
        zfsConfigurationPropertiesRetentionPruneDeferralTextField.Data = new RetentionTextValidateFieldViewData( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, zfsConfigurationPropertiesRetentionPruneDeferralTextField, 0, 100 );
    }

    private void SetTabStopsForZfsConfigurationWindow( )
    {
        zfsConfigurationWindow.TabStop = true;
        zfsConfigurationTreeFrame.TabStop = true;
        zfsConfigurationPropertiesFrame.TabStop = true;
        zfsConfigurationCommonPropertiesFrame.TabStop = true;
        zfsConfigurationSnapshotPropertiesFrame.TabStop = true;
        zfsConfigurationActionsFrame.TabStop = true;
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

    private void ZfsConfigurationPropertiesTemplateTextFieldOnLeave( FocusEventArgs args )
    {
        ArgumentNullException.ThrowIfNull( args, nameof( args ) );
        UpdateSelectedItemTextFieldStringProperty( (TextFieldWithSourceViewData)args.View.Data, "local" );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
        UpdateZfsConfigurationButtonState( );
    }

    private void ZfsConfigurationPropertiesRecursionRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemStringRadioGroupProperty( zfsConfigurationPropertiesRecursionRadioGroup, "local" );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
        UpdateZfsConfigurationButtonState( );
    }

    private void ZfsConfigurationPropertiesRetentionTextFieldOnLeave( FocusEventArgs args )
    {
        ArgumentNullException.ThrowIfNull( args, nameof( args ) );

        UpdateSelectedItemTextValidateFieldIntProperty( (RetentionTextValidateFieldViewData)args.View.Data, "local" );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
        UpdateZfsConfigurationButtonState( );
    }

    private void ZfsConfigurationResetCurrentButtonOnClicked( )
    {
        DisableZfsConfigurationTabEventHandlers( );
        ClearAllZfsPropertyFields( );
        _modifiedPropertiesSinceLastSaveForCurrentItem.Clear( );
        RestorePreviousSelectedItem( );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( false );
        UpdateZfsConfigurationButtonState( );
        EnableEventHandlers( );
    }

    private void ZfsConfigurationSaveCurrentButtonOnClicked( )
    {
        try
        {
            DisableZfsConfigurationTabEventHandlers( );

            if ( !SelectedTreeNode.IsModified )
            {
                return;
            }

            // The buttons are disposable, but the Dialog will dispose them when it is closed
            Button cancelButton = new( "Cancel", true );
            Button saveButton = new( "Save" );
            using Dialog saveZfsObjectDialog = new( "Confirm Saving ZFS Object Configuration", 80, 7, cancelButton, saveButton );
            bool saveConfirmed = false;
            string zfsObjectPath = SelectedTreeNode.Text;

            cancelButton.Clicked += OnCancelButtonOnClicked;
            saveButton.Clicked += OnSaveButtonOnClicked;

            saveZfsObjectDialog.ButtonAlignment = Dialog.ButtonAlignments.Center;
            saveZfsObjectDialog.AutoSize = true;
            saveZfsObjectDialog.ColorScheme = whiteOnRed;
            saveZfsObjectDialog.TextAlignment = TextAlignment.Centered;
            saveZfsObjectDialog.VerticalTextAlignment = VerticalTextAlignment.Middle;
            saveZfsObjectDialog.Text = $"The following command will be executed:\nzfs set {_modifiedPropertiesSinceLastSaveForCurrentItem.ToStringForZfsSet( )} {zfsObjectPath}\n\nTHIS OPERATION CANNOT BE UNDONE";
            saveZfsObjectDialog.Modal = true;

            Application.Run( saveZfsObjectDialog );

            if ( saveConfirmed && ConfigConsole.Settings is not null && ConfigConsole.CommandRunner is not null )
            {
                Logger.Info( "Saving {0}", zfsObjectPath );
                if ( ZfsTasks.SetPropertiesForDataset( ConfigConsole.Settings.DryRun, zfsObjectPath, _modifiedPropertiesSinceLastSaveForCurrentItem.Values.ToList( ), ConfigConsole.CommandRunner ) || ConfigConsole.Settings.DryRun )
                {
                    Logger.Debug( "Applying inheritable properties to children of {0} in tree", zfsObjectPath );
                    foreach ( KeyValuePair<string, IZfsProperty> kvp in _modifiedPropertiesSinceLastSaveForCurrentItem )
                    {
                        switch ( kvp.Value )
                        {
                            case ZfsProperty<bool> boolProp:
                                UpdateDescendentsBooleanPropertyInheritance( SelectedTreeNode, boolProp, $"inherited from {SelectedTreeNode.Text}" );
                                break;
                            case ZfsProperty<int> intProp:
                                UpdateDescendentsIntPropertyInheritance( SelectedTreeNode, intProp, $"inherited from {SelectedTreeNode.Text}" );
                                break;
                            case ZfsProperty<string> stringProp:
                                UpdateDescendentsStringPropertyInheritance( SelectedTreeNode, stringProp, $"inherited from {SelectedTreeNode.Text}" );
                                break;
                            case ZfsProperty<DateTimeOffset> dtoProp:
                                UpdateDescendentsDateTimeOffsetPropertyInheritance( SelectedTreeNode, dtoProp, $"inherited from {SelectedTreeNode.Text}" );
                                break;
                        }
                    }

                    _modifiedPropertiesSinceLastSaveForCurrentItem.Clear( );
                    SelectedTreeNode.BaseDataset = SelectedTreeNode.TreeDataset with { };
                }
            }

            // Fine to ignore this warning here, because we are explicitly un-wiring the events before any disposal will occur.
            // The button click handlers are un-subscribed and THEN the dialog is asked to exit.
            // ReSharper disable AccessToDisposedClosure
            void OnCancelButtonOnClicked( )
            {
                cancelButton.Clicked -= OnCancelButtonOnClicked;
                saveButton.Clicked -= OnSaveButtonOnClicked;
                RequestStop( saveZfsObjectDialog );
            }

            void OnSaveButtonOnClicked( )
            {
                saveConfirmed = true;
                cancelButton.Clicked -= OnCancelButtonOnClicked;
                saveButton.Clicked -= OnSaveButtonOnClicked;
                RequestStop( saveZfsObjectDialog );
            }
            // ReSharper restore AccessToDisposedClosure
        }
        finally
        {
            UpdateZfsCommonPropertyFieldsForSelectedTreeNode( false );
            UpdateZfsConfigurationButtonState( );
            EnableZfsConfigurationTabEventHandlers( );
        }
    }

    private void ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick( MouseEventArgs args )
    {
        RadioGroupWithSourceViewData viewData = (RadioGroupWithSourceViewData)args.MouseEvent.View.Data;
        ZfsProperty<bool> newProperty = SelectedTreeNode.TreeDataset.UpdateProperty( viewData.PropertyName, viewData.RadioGroup.GetSelectedBooleanFromLabel( ), "local" );
        _modifiedPropertiesSinceLastSaveForCurrentItem[ viewData.PropertyName ] = newProperty;
        viewData.SourceTextField.Text = newProperty.Source;
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
        UpdateZfsConfigurationButtonState( );
    }

    private void ZfsConfigurationPropertiesStringRadioGroupOnMouseClick( MouseEventArgs args )
    {
        RadioGroupWithSourceViewData viewData = (RadioGroupWithSourceViewData)args.MouseEvent.View.Data;
        IZfsProperty newProperty = SelectedTreeNode.TreeDataset.UpdateProperty( viewData.PropertyName, viewData.RadioGroup.GetSelectedLabelString( ), "local" );
        _modifiedPropertiesSinceLastSaveForCurrentItem[ viewData.PropertyName ] = newProperty;
        viewData.SourceTextField.Text = newProperty.Source;
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
        UpdateZfsConfigurationButtonState( );
    }

    /// <summary>
    ///     Performs a depth-first recursion on the entire tree, updating all boolean properties appropriately
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="prop"></param>
    /// <param name="source"></param>
    private static void UpdateDescendentsBooleanPropertyInheritance( ZfsObjectConfigurationTreeNode currentNode, ZfsProperty<bool> prop, string source )
    {
        foreach ( ZfsObjectConfigurationTreeNode child in currentNode.Children.Cast<ZfsObjectConfigurationTreeNode>( ) )
        {
            // If this child already has the property defined locally, we can skip it (and thus its descendents as well)
            if ( child.TreeDataset[ prop.Name ].IsLocal )
            {
                continue;
            }

            UpdateDescendentsBooleanPropertyInheritance( child, prop, source );
        }

        // This is the final base case, for the node we started from
        if ( currentNode.TreeDataset[ prop.Name ].IsLocal )
        {
            return;
        }

        // For everyone that makes it this far, we need to inherit, so update the tree and base copies of the property
        currentNode.TreeDataset.UpdateProperty( prop.Name, prop.Value, source );
        currentNode.BaseDataset.UpdateProperty( prop.Name, prop.Value, source );
    }

    private static void UpdateDescendentsIntPropertyInheritance( ZfsObjectConfigurationTreeNode currentNode, ZfsProperty<int> prop, string source )
    {
        foreach ( ZfsObjectConfigurationTreeNode child in currentNode.Children.Cast<ZfsObjectConfigurationTreeNode>( ) )
        {
            UpdateDescendentsIntPropertyInheritance( child, prop, source );
        }

        // If this node already has the property defined locally, we can stop at this level
        if ( currentNode.TreeDataset[ prop.Name ].IsLocal )
        {
            return;
        }

        currentNode.TreeDataset.UpdateProperty( prop.Name, prop.Value, source );
        currentNode.BaseDataset.UpdateProperty( prop.Name, prop.Value, source );
    }

    private static void UpdateDescendentsStringPropertyInheritance( ZfsObjectConfigurationTreeNode currentNode, ZfsProperty<string> prop, string source )
    {
        foreach ( ZfsObjectConfigurationTreeNode child in currentNode.Children.Cast<ZfsObjectConfigurationTreeNode>( ) )
        {
            UpdateDescendentsStringPropertyInheritance( child, prop, source );
        }

        // If this node already has the property defined locally, we can stop at this level
        if ( currentNode.TreeDataset[ prop.Name ].IsLocal )
        {
            return;
        }

        currentNode.TreeDataset.UpdateProperty( prop.Name, prop.Value, source );
        currentNode.BaseDataset.UpdateProperty( prop.Name, prop.Value, source );
    }

    private static void UpdateDescendentsDateTimeOffsetPropertyInheritance( ZfsObjectConfigurationTreeNode currentNode, ZfsProperty<DateTimeOffset> prop, string source )
    {
        foreach ( ZfsObjectConfigurationTreeNode child in currentNode.Children.Cast<ZfsObjectConfigurationTreeNode>( ) )
        {
            UpdateDescendentsDateTimeOffsetPropertyInheritance( child, prop, source );
        }

        // If this node already has the property defined locally, we can stop at this level
        if ( currentNode.TreeDataset[ prop.Name ].IsLocal )
        {
            return;
        }

        currentNode.TreeDataset.UpdateProperty( prop.Name, prop.Value, source );
        currentNode.BaseDataset.UpdateProperty( prop.Name, prop.Value, source );
    }

    private void UpdateSelectedItemBooleanRadioGroupProperty( RadioGroup radioGroup, string? propertySource = null )
    {
        RadioGroupWithSourceViewData viewData = (RadioGroupWithSourceViewData)radioGroup.Data;
        ZfsProperty<bool> newProperty = SelectedTreeNode.TreeDataset.UpdateProperty( viewData.PropertyName, radioGroup.GetSelectedBooleanFromLabel( ), propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source );
        _modifiedPropertiesSinceLastSaveForCurrentItem[ viewData.PropertyName ] = newProperty;
        viewData.SourceTextField.Text = propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source;
    }

    private void UpdateSelectedItemStringRadioGroupProperty( RadioGroup radioGroup, string? propertySource = null )
    {
        RadioGroupWithSourceViewData viewData = (RadioGroupWithSourceViewData)radioGroup.Data;
        ZfsProperty<string> newProperty = (ZfsProperty<string>)SelectedTreeNode.TreeDataset.UpdateProperty( viewData.PropertyName, radioGroup.GetSelectedLabelString( ), propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source );
        _modifiedPropertiesSinceLastSaveForCurrentItem[ viewData.PropertyName ] = newProperty;
        viewData.SourceTextField.Text = propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source;
    }

    private void UpdateSelectedItemTextFieldStringProperty( TextFieldWithSourceViewData viewData, string? propertySource = null )
    {
        if ( viewData.ValueTextField.Text?.ToString( ) is not { } propertyValue || string.IsNullOrWhiteSpace( propertyValue ) )
        {
            return;
        }

        ZfsProperty<string> newProperty = (ZfsProperty<string>)SelectedTreeNode.TreeDataset.UpdateProperty( viewData.PropertyName, propertyValue, propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source );
        _modifiedPropertiesSinceLastSaveForCurrentItem[ viewData.PropertyName ] = newProperty;
        viewData.SourceTextField.Text = propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source;
    }

    private void UpdateSelectedItemTextValidateFieldIntProperty( RetentionTextValidateFieldViewData viewData, string? propertySource = null )
    {
        if ( !int.TryParse( viewData.ValueTextField.Text?.ToString( ), out int intValue ) && ( intValue < viewData.MinValue || intValue > viewData.MaxValue ) )
        {
            Logger.Info( "Invalid value entered for {0}: {1}. Must be a valid integer between {2} and {3}", viewData.PropertyName, viewData.ValueTextField.Text, viewData.MinValue, viewData.MaxValue );
            viewData.ValueTextField.Text = ustring.Make( ( (ZfsProperty<int>)SelectedTreeNode.TreeDataset[ viewData.PropertyName ] ).Value );
            return;
        }

        ZfsProperty<int> newProperty = SelectedTreeNode.TreeDataset.UpdateProperty( viewData.PropertyName, intValue, propertySource ?? SelectedTreeNode.TreeDataset[ viewData.PropertyName ].Source );
        _modifiedPropertiesSinceLastSaveForCurrentItem[ viewData.PropertyName ] = newProperty;
        viewData.ValueTextField.ColorScheme = SelectedTreeNode.TreeDataset[ viewData.PropertyName ].IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
    }

    private void ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemBooleanRadioGroupProperty( zfsConfigurationPropertiesEnabledRadioGroup, "local" );
        UpdateZfsConfigurationButtonState( );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
    }

    private void ZfsConfigurationPropertiesTakeSnapshotsRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemBooleanRadioGroupProperty( zfsConfigurationPropertiesTakeSnapshotsRadioGroup, "local" );
        UpdateZfsConfigurationButtonState( );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
    }

    private void ZfsConfigurationPropertiesPruneSnapshotsRadioGroup_SelectedItemChanged( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemBooleanRadioGroupProperty( zfsConfigurationPropertiesPruneSnapshotsRadioGroup, "local" );
        UpdateZfsConfigurationButtonState( );
        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( );
    }

    private void ZfsConfigurationTreeViewOnSelectionChanged( object? sender, SelectionChangedEventArgs<ITreeNode> e )
    {
        ArgumentNullException.ThrowIfNull( sender );
        DisableZfsConfigurationTabEventHandlers( );

        ClearAllZfsPropertyFields( );
        _modifiedPropertiesSinceLastSaveForCurrentItem.Clear( );

        // Be sure to set the previously-selected object back to its previous state if it wasn't saved
        if ( e.OldValue is ZfsObjectConfigurationTreeNode { IsModified: true } old )
        {
            old.TreeDataset = old.BaseDataset with { };
        }

        UpdateZfsCommonPropertyFieldsForSelectedTreeNode( false );
        UpdateZfsConfigurationButtonState( );
        EnableZfsConfigurationTabEventHandlers( );
    }

    private void RestorePreviousSelectedItem( )
    {
        SelectedTreeNode.TreeDataset = SelectedTreeNode.BaseDataset with { };
    }

    private void ClearAllZfsPropertyFields( bool manageEventHandlers = false )
    {
        if ( manageEventHandlers )
        {
            DisableZfsConfigurationTabEventHandlers( );
        }

        zfsConfigurationPropertiesNameTextField.Clear( );
        zfsConfigurationPropertiesTypeTextField.Clear( );
        zfsConfigurationPropertiesEnabledRadioGroup.Clear( );
        zfsConfigurationPropertiesEnabledSourceTextField.Clear( );
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.Clear( );
        zfsConfigurationPropertiesTakeSnapshotsSourceTextField.Clear( );
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.Clear( );
        zfsConfigurationPropertiesPruneSnapshotsSourceTextField.Clear( );
        zfsConfigurationPropertiesRecursionRadioGroup.Clear( );
        zfsConfigurationPropertiesRecursionSourceTextField.Clear( );
        zfsConfigurationPropertiesTemplateTextField.Clear( );
        zfsConfigurationPropertiesTemplateSourceTextField.Clear( );
        zfsConfigurationPropertiesRetentionFrequentTextField.Clear( );
        zfsConfigurationPropertiesRetentionHourlyTextField.Clear( );
        zfsConfigurationPropertiesRetentionDailyTextField.Clear( );
        zfsConfigurationPropertiesRetentionWeeklyTextField.Clear( );
        zfsConfigurationPropertiesRetentionMonthlyTextField.Clear( );
        zfsConfigurationPropertiesRetentionYearlyTextField.Clear( );
        zfsConfigurationPropertiesRetentionPruneDeferralTextField.Clear( );
        zfsConfigurationPropertiesRecentFrequentTextField.Clear( );
        zfsConfigurationPropertiesRecentHourlyTextField.Clear( );
        zfsConfigurationPropertiesRecentDailyTextField.Clear( );
        zfsConfigurationPropertiesRecentWeeklyTextField.Clear( );
        zfsConfigurationPropertiesRecentMonthlyTextField.Clear( );
        zfsConfigurationPropertiesRecentYearlyTextField.Clear( );

        if ( manageEventHandlers )
        {
            EnableZfsConfigurationTabEventHandlers( );
        }
    }

    private void UpdateZfsCommonPropertyFieldsForSelectedTreeNode( bool manageEventHandlers = true )
    {
        if ( manageEventHandlers )
        {
            DisableZfsConfigurationTabEventHandlers( );
        }

        ShowZfsConfigurationPropertyFrames( );
        zfsConfigurationPropertiesNameTextField.Text = SelectedTreeNode.TreeDataset.Name;
        zfsConfigurationPropertiesTypeTextField.Text = SelectedTreeNode.TreeDataset.Kind;
        zfsConfigurationPropertiesEnabledRadioGroup.SelectedItem = SelectedTreeNode.TreeDataset.Enabled.AsTrueFalseRadioIndex( );
        zfsConfigurationPropertiesEnabledRadioGroup.ColorScheme = SelectedTreeNode.TreeDataset.Enabled.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        zfsConfigurationPropertiesEnabledSourceTextField.Text = SelectedTreeNode.TreeDataset.Enabled.InheritedFrom;
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.SelectedItem = SelectedTreeNode.TreeDataset.TakeSnapshots.AsTrueFalseRadioIndex( );
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.ColorScheme = SelectedTreeNode.TreeDataset.TakeSnapshots.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        zfsConfigurationPropertiesTakeSnapshotsSourceTextField.Text = SelectedTreeNode.TreeDataset.TakeSnapshots.InheritedFrom;
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.SelectedItem = SelectedTreeNode.TreeDataset.PruneSnapshots.AsTrueFalseRadioIndex( );
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.ColorScheme = SelectedTreeNode.TreeDataset.PruneSnapshots.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        zfsConfigurationPropertiesPruneSnapshotsSourceTextField.Text = SelectedTreeNode.TreeDataset.PruneSnapshots.InheritedFrom;
        zfsConfigurationPropertiesRecursionRadioGroup.SelectedItem = SelectedTreeNode.TreeDataset.Recursion.Value switch { "sanoid" => 0, "zfs" => 1, _ => throw new InvalidOperationException( "Invalid recursion value" ) };
        zfsConfigurationPropertiesRecursionRadioGroup.ColorScheme = SelectedTreeNode.TreeDataset.Recursion.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        zfsConfigurationPropertiesRecursionSourceTextField.Text = SelectedTreeNode.TreeDataset.Recursion.InheritedFrom;
        zfsConfigurationPropertiesTemplateTextField.Text = SelectedTreeNode.TreeDataset.Template.Value;
        zfsConfigurationPropertiesTemplateTextField.ColorScheme = SelectedTreeNode.TreeDataset.Template.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesTemplateSourceTextField.Text = SelectedTreeNode.TreeDataset.Template.InheritedFrom;

        zfsConfigurationPropertiesRetentionFrequentTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionFrequent.ValueString;
        zfsConfigurationPropertiesRetentionFrequentTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionFrequent.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesRetentionHourlyTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionHourly.ValueString;
        zfsConfigurationPropertiesRetentionHourlyTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionHourly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesRetentionDailyTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionDaily.ValueString;
        zfsConfigurationPropertiesRetentionDailyTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionDaily.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesRetentionWeeklyTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionWeekly.ValueString;
        zfsConfigurationPropertiesRetentionWeeklyTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionWeekly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesRetentionMonthlyTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionMonthly.ValueString;
        zfsConfigurationPropertiesRetentionMonthlyTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionMonthly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesRetentionYearlyTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionYearly.ValueString;
        zfsConfigurationPropertiesRetentionYearlyTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionYearly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
        zfsConfigurationPropertiesRetentionPruneDeferralTextField.Text = SelectedTreeNode.TreeDataset.SnapshotRetentionPruneDeferral.ValueString;
        zfsConfigurationPropertiesRetentionPruneDeferralTextField.ColorScheme = SelectedTreeNode.TreeDataset.SnapshotRetentionPruneDeferral.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;

        zfsConfigurationPropertiesRecentFrequentTextField.Text = SelectedTreeNode.TreeDataset.LastFrequentSnapshotTimestamp.IsLocal ? SelectedTreeNode.TreeDataset.LastFrequentSnapshotTimestamp.ValueString : "None";
        zfsConfigurationPropertiesRecentHourlyTextField.Text = SelectedTreeNode.TreeDataset.LastHourlySnapshotTimestamp.IsLocal ? SelectedTreeNode.TreeDataset.LastHourlySnapshotTimestamp.ValueString : "None";
        zfsConfigurationPropertiesRecentDailyTextField.Text = SelectedTreeNode.TreeDataset.LastDailySnapshotTimestamp.IsLocal ? SelectedTreeNode.TreeDataset.LastDailySnapshotTimestamp.ValueString : "None";
        zfsConfigurationPropertiesRecentWeeklyTextField.Text = SelectedTreeNode.TreeDataset.LastWeeklySnapshotTimestamp.IsLocal ? SelectedTreeNode.TreeDataset.LastWeeklySnapshotTimestamp.ValueString : "None";
        zfsConfigurationPropertiesRecentMonthlyTextField.Text = SelectedTreeNode.TreeDataset.LastMonthlySnapshotTimestamp.IsLocal ? SelectedTreeNode.TreeDataset.LastMonthlySnapshotTimestamp.ValueString : "None";
        zfsConfigurationPropertiesRecentYearlyTextField.Text = SelectedTreeNode.TreeDataset.LastYearlySnapshotTimestamp.IsLocal ? SelectedTreeNode.TreeDataset.LastYearlySnapshotTimestamp.ValueString : "None";

        if ( manageEventHandlers )
        {
            EnableZfsConfigurationTabEventHandlers( );
        }
    }

    private async void RefreshZfsConfigurationTreeViewFromZfs( )
    {
        Logger.Debug( "Refreshing zfs configuration tree view" );
        DisableZfsConfigurationTabEventHandlers( );
        try
        {
            HideZfsConfigurationPropertyFrames( );
            Logger.Debug( "Clearing objects from zfs configuration tree view" );
            zfsConfigurationTreeView.ClearObjects( );
            _treeDatasets.Clear( );
            _baseDatasets.Clear( );
            _modifiedPropertiesSinceLastSaveForCurrentItem.Clear( );
            Logger.Debug( "Getting zfs objects from zfs and populating configuration tree view" );
            List<ITreeNode> treeRootNodes = await ZfsTasks.GetFullZfsConfigurationTreeAsync( _baseDatasets, _treeDatasets, ConfigConsole.Snapshots, ConfigConsole.CommandRunner! ).ConfigureAwait( true );

            zfsConfigurationTreeView.AddObjects( treeRootNodes );
            UpdateZfsConfigurationButtonState( );
            zfsConfigurationTreeView.SetFocus( );
        }
        catch ( Exception e )
        {
            Logger.Error( e, "Error getting ZFS configuration tree" );
        }

        EnableZfsConfigurationTabEventHandlers( );
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

                File.WriteAllText( globalConfigSaveDialog.FileName.ToString( ) ?? throw new InvalidOperationException( "Null string provided for save file name" ), JsonSerializer.Serialize( ConfigConsole.Settings, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
            }
        }
    }

    private void DisableZfsConfigurationTabEventHandlers( )
    {
        if ( !_zfsConfigurationEventsEnabled )
        {
            return;
        }

        zfsConfigurationRefreshButton.Clicked -= RefreshZfsConfigurationTreeViewFromZfs;
        zfsConfigurationResetCurrentButton.Clicked -= ZfsConfigurationResetCurrentButtonOnClicked;
        zfsConfigurationTreeView.SelectionChanged -= ZfsConfigurationTreeViewOnSelectionChanged;
        zfsConfigurationPropertiesEnabledRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesEnabledRadioGroup.MouseClick -= ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick;
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesTakeSnapshotsRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.MouseClick -= ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick;
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesPruneSnapshotsRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.MouseClick -= ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick;
        zfsConfigurationPropertiesRecursionRadioGroup.SelectedItemChanged -= ZfsConfigurationPropertiesRecursionRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesRecursionRadioGroup.MouseClick -= ZfsConfigurationPropertiesStringRadioGroupOnMouseClick;
        zfsConfigurationPropertiesTemplateTextField.Leave -= ZfsConfigurationPropertiesTemplateTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionFrequentTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionHourlyTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionDailyTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionWeeklyTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionMonthlyTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionYearlyTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionPruneDeferralTextField.Leave -= ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationSaveCurrentButton.Clicked -= ZfsConfigurationSaveCurrentButtonOnClicked;
        _zfsConfigurationEventsEnabled = false;
    }

    private void EnableZfsConfigurationTabEventHandlers( )
    {
        if ( _zfsConfigurationEventsEnabled )
        {
            return;
        }

        zfsConfigurationRefreshButton.Clicked += RefreshZfsConfigurationTreeViewFromZfs;
        zfsConfigurationResetCurrentButton.Clicked += ZfsConfigurationResetCurrentButtonOnClicked;
        zfsConfigurationTreeView.SelectionChanged += ZfsConfigurationTreeViewOnSelectionChanged;
        zfsConfigurationPropertiesEnabledRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesEnabledRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesEnabledRadioGroup.MouseClick += ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick;
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesTakeSnapshotsRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesTakeSnapshotsRadioGroup.MouseClick += ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick;
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesPruneSnapshotsRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesPruneSnapshotsRadioGroup.MouseClick += ZfsConfigurationPropertiesBooleanRadioGroupOnMouseClick;
        zfsConfigurationPropertiesRecursionRadioGroup.SelectedItemChanged += ZfsConfigurationPropertiesRecursionRadioGroup_SelectedItemChanged;
        zfsConfigurationPropertiesRecursionRadioGroup.MouseClick += ZfsConfigurationPropertiesStringRadioGroupOnMouseClick;
        zfsConfigurationPropertiesTemplateTextField.Leave += ZfsConfigurationPropertiesTemplateTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionFrequentTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionHourlyTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionDailyTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionWeeklyTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionMonthlyTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionYearlyTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationPropertiesRetentionPruneDeferralTextField.Leave += ZfsConfigurationPropertiesRetentionTextFieldOnLeave;
        zfsConfigurationSaveCurrentButton.Clicked += ZfsConfigurationSaveCurrentButtonOnClicked;
        _zfsConfigurationEventsEnabled = true;
    }

    private record RadioGroupWithSourceViewData( string PropertyName, RadioGroup RadioGroup, TextField SourceTextField );

    private record TextFieldWithSourceViewData( string PropertyName, TextField ValueTextField, TextField SourceTextField );

    private record RetentionTextValidateFieldViewData( string PropertyName, TextValidateField ValueTextField, int MinValue, int MaxValue );
}