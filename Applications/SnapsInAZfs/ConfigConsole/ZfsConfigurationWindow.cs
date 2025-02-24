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

namespace SnapsInAZfs.ConfigConsole;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Interop.Zfs.ZfsCommandRunner;
using Interop.Zfs.ZfsTypes;
using JetBrains.Annotations;
using Terminal.Gui;
using Terminal.Gui.Trees;
using TreeNodes;

[MustDisposeResource]
public sealed partial class ZfsConfigurationWindow
{
    private static readonly Logger Logger = LogManager.GetLogger ( $"{ConfigConsole.ConfigConsoleNamespace}{nameof (ZfsConfigurationWindow)}" )!;

    private readonly ConcurrentDictionary<string, ZfsRecord> _treeDatasets = [ ];

    public ZfsConfigurationWindow ( )
    {
        // ReSharper disable once HeapView.DelegateAllocation
        // ReSharper disable once HeapView.ObjectAllocation.Possible
        Initialized += ZfsConfigurationWindowOnInitialized;
        InitializeComponent ( );
    }

    private bool                            _eventsEnabled;
    private ZfsObjectConfigurationTreeNode? SelectedTreeNode => zfsTreeView?.SelectedObject as ZfsObjectConfigurationTreeNode;

    private void ClearAllPropertyFields ( bool manageEventHandlers = false )
    {
        if ( manageEventHandlers )
        {
            DisableEventHandlers ( );
        }

        nameTextField.Clear ( );
        typeTextField.Clear ( );
        enabledRadioGroup.Clear ( );
        enabledSourceTextField.Clear ( );
        takeSnapshotsRadioGroup.Clear ( );
        takeSnapshotsSourceTextField.Clear ( );
        pruneSnapshotsRadioGroup.Clear ( );
        pruneSnapshotsSourceTextField.Clear ( );
        recursionRadioGroup.Clear ( );
        recursionSourceTextField.Clear ( );
        templateSourceTextField.Clear ( );
        retentionFrequentTextField.Clear ( );
        retentionHourlyTextField.Clear ( );
        retentionDailyTextField.Clear ( );
        retentionWeeklyTextField.Clear ( );
        retentionMonthlyTextField.Clear ( );
        retentionYearlyTextField.Clear ( );
        retentionPruneDeferralTextField.Clear ( );
        recentFrequentTextField.Clear ( );
        recentHourlyTextField.Clear ( );
        recentDailyTextField.Clear ( );
        recentWeeklyTextField.Clear ( );
        recentMonthlyTextField.Clear ( );
        recentYearlyTextField.Clear ( );

        if ( manageEventHandlers )
        {
            EnableEventHandlers ( );
        }
    }

    private void DisableEventHandlers ( )
    {
        if ( !_eventsEnabled )
        {
            return;
        }

        Logger.Debug ( "Disabling event handlers for zfs configuration fields" );

        // ReSharper disable HeapView.ObjectAllocation.Possible
        refreshButton.Clicked                        -= RefreshZfsTreeViewFromZfs;
        resetCurrentButton.Clicked                   -= ResetCurrentButtonOnClicked;
        zfsTreeView.SelectionChanged                 -= zfsTreeViewOnSelectionChanged;
        enabledRadioGroup.SelectedItemChanged        -= EnabledRadioGroupSelectedItemChanged;
        enabledInheritButton.Clicked                 -= EnabledInheritButtonClick;
        takeSnapshotsRadioGroup.SelectedItemChanged  -= TakeSnapshotsRadioGroupSelectedItemChanged;
        takeSnapshotsInheritButton.Clicked           -= TakeSnapshotsInheritButtonClick;
        pruneSnapshotsRadioGroup.SelectedItemChanged -= PruneSnapshotsRadioGroupSelectedItemChanged;
        pruneSnapshotsInheritButton.Clicked          -= PruneSnapshotsInheritButtonClick;
        recursionRadioGroup.SelectedItemChanged      -= RecursionRadioGroupSelectedItemChanged;
        recursionInheritButton.Clicked               -= RecursionInheritButtonClick;
        templateListView.SelectedItemChanged         -= TemplateListViewOnSelectedItemChanged;
        templateInheritButton.Clicked                -= TemplateInheritButtonClick;
        retentionFrequentTextField.Leave             -= RetentionFrequentTextFieldOnLeave;
        retentionFrequentInheritButton.Clicked       -= RetentionFrequentInheritButtonClick;
        retentionHourlyTextField.Leave               -= RetentionHourlyTextFieldOnLeave;
        retentionHourlyInheritButton.Clicked         -= RetentionHourlyInheritButtonClick;
        retentionDailyTextField.Leave                -= RetentionDailyTextFieldOnLeave;
        retentionDailyInheritButton.Clicked          -= RetentionDailyInheritButtonClick;
        retentionWeeklyTextField.Leave               -= RetentionWeeklyTextFieldOnLeave;
        retentionWeeklyInheritButton.Clicked         -= RetentionWeeklyInheritButtonClick;
        retentionMonthlyTextField.Leave              -= RetentionMonthlyTextFieldOnLeave;
        retentionMonthlyInheritButton.Clicked        -= RetentionMonthlyInheritButtonClick;
        retentionYearlyTextField.Leave               -= RetentionYearlyTextFieldOnLeave;
        retentionYearlyInheritButton.Clicked         -= RetentionYearlyInheritButtonClick;
        retentionPruneDeferralTextField.Leave        -= RetentionPruneDeferralTextFieldOnLeave;
        retentionPruneDeferralInheritButton.Clicked  -= RetentionPruneDeferralInheritButtonClick;
        saveCurrentButton.Clicked                    -= SaveCurrentButtonOnClicked;

        // ReSharper restore HeapView.ObjectAllocation.Possible
        _eventsEnabled = false;
        Logger.Debug ( "Event handlers for zfs configuration fields disabled" );
    }

    private void EnabledInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit enabled setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Enabled Setting", $"Inherit Enabled setting {node.TreeDataset.ParentDataset.Enabled.Value.ToString ( )} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.EnabledPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void EnabledRadioGroupSelectedItemChanged ( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemBooleanRadioGroupProperty ( enabledRadioGroup );
        UpdateButtonState ( );
        UpdateFieldsForSelectedZfsTreeNode ( );
    }

    private void EnableEventHandlers ( )
    {
        if ( _eventsEnabled )
        {
            return;
        }

        Logger.Debug ( "Enabling event handlers for zfs configuration fields" );

        // ReSharper disable HeapView.ObjectAllocation.Possible
        refreshButton.Clicked                        += RefreshZfsTreeViewFromZfs;
        resetCurrentButton.Clicked                   += ResetCurrentButtonOnClicked;
        zfsTreeView.SelectionChanged                 += zfsTreeViewOnSelectionChanged;
        enabledRadioGroup.SelectedItemChanged        += EnabledRadioGroupSelectedItemChanged;
        enabledInheritButton.Clicked                 += EnabledInheritButtonClick;
        takeSnapshotsRadioGroup.SelectedItemChanged  += TakeSnapshotsRadioGroupSelectedItemChanged;
        takeSnapshotsInheritButton.Clicked           += TakeSnapshotsInheritButtonClick;
        pruneSnapshotsRadioGroup.SelectedItemChanged += PruneSnapshotsRadioGroupSelectedItemChanged;
        pruneSnapshotsInheritButton.Clicked          += PruneSnapshotsInheritButtonClick;
        recursionRadioGroup.SelectedItemChanged      += RecursionRadioGroupSelectedItemChanged;
        recursionInheritButton.Clicked               += RecursionInheritButtonClick;
        templateListView.SelectedItemChanged         += TemplateListViewOnSelectedItemChanged;
        templateInheritButton.Clicked                += TemplateInheritButtonClick;
        retentionFrequentTextField.Leave             += RetentionFrequentTextFieldOnLeave;
        retentionFrequentInheritButton.Clicked       += RetentionFrequentInheritButtonClick;
        retentionHourlyTextField.Leave               += RetentionHourlyTextFieldOnLeave;
        retentionHourlyInheritButton.Clicked         += RetentionHourlyInheritButtonClick;
        retentionDailyTextField.Leave                += RetentionDailyTextFieldOnLeave;
        retentionDailyInheritButton.Clicked          += RetentionDailyInheritButtonClick;
        retentionWeeklyTextField.Leave               += RetentionWeeklyTextFieldOnLeave;
        retentionWeeklyInheritButton.Clicked         += RetentionWeeklyInheritButtonClick;
        retentionMonthlyTextField.Leave              += RetentionMonthlyTextFieldOnLeave;
        retentionMonthlyInheritButton.Clicked        += RetentionMonthlyInheritButtonClick;
        retentionYearlyTextField.Leave               += RetentionYearlyTextFieldOnLeave;
        retentionYearlyInheritButton.Clicked         += RetentionYearlyInheritButtonClick;
        retentionPruneDeferralTextField.Leave        += RetentionPruneDeferralTextFieldOnLeave;
        retentionPruneDeferralInheritButton.Clicked  += RetentionPruneDeferralInheritButtonClick;
        saveCurrentButton.Clicked                    += SaveCurrentButtonOnClicked;

        // ReSharper enable HeapView.ObjectAllocation.Possible
        _eventsEnabled = true;
        Logger.Debug ( "Event handlers for zfs configuration fields enabled" );
    }

    private void PruneSnapshotsInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit prune snapshots setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Prune Snapshots Setting", $"Inherit Prune Snapshots setting {node.TreeDataset.ParentDataset.PruneSnapshots.Value.ToString ( )} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.PruneSnapshotsPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void PruneSnapshotsRadioGroupSelectedItemChanged ( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemBooleanRadioGroupProperty ( pruneSnapshotsRadioGroup );
        UpdateButtonState ( );
        UpdateFieldsForSelectedZfsTreeNode ( );
    }

    private void RecursionInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit recursion setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Recursion Setting", $"Inherit Recursion setting {node.TreeDataset.ParentDataset.Recursion.Value} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.RecursionPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RecursionRadioGroupSelectedItemChanged ( SelectedItemChangedArgs? e )
    {
        ArgumentNullException.ThrowIfNull ( e, nameof (e) );

        if ( recursionRadioGroup.Data is not RadioGroupWithSourceViewData viewData )
        {
            throw new InvalidOperationException ( "Invalid or missing data when updating recursion setting." );
        }

        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update property!" );
        }

        if ( viewData.RadioGroup.GetSelectedLabelString ( ) != node.TreeDataset.Recursion.Value )
        {
            UpdateSelectedItemStringRadioGroupProperty ( recursionRadioGroup );
        }

        UpdateFieldsForSelectedZfsTreeNode ( );
        UpdateButtonState ( );
    }

    private void RefreshZfsTreeViewFromZfs ( )
    {
        // Disabling this warning because we are intentionally firing this off asynchronously.
        // The callee will handle button state.
#pragma warning disable CS4014
        ClearAllPropertyFields ( true );
        RefreshZfsTreeViewFromZfsAsync ( true );
#pragma warning restore CS4014
    }

    private async Task RefreshZfsTreeViewFromZfsAsync ( bool manageEventHandlers = false )
    {
        Logger.Debug ( "Refreshing zfs configuration tree view" );

        if ( manageEventHandlers )
        {
            DisableEventHandlers ( );
        }

        try
        {
            Logger.Debug ( "Clearing objects from zfs configuration tree view" );
            zfsTreeView.ClearObjects ( );
            _treeDatasets.Clear ( );
            ConfigConsole.BaseDatasets.Clear ( );
            ConfigConsole.Snapshots.Clear ( );
            Logger.Debug ( "Getting zfs objects from zfs and populating configuration tree view" );
            List<ITreeNode> treeRootNodes = await ZfsTasks.GetFullZfsConfigurationTreeAsync ( Program.Settings!, ConfigConsole.BaseDatasets, _treeDatasets, ConfigConsole.Snapshots, ConfigConsole.CommandRunner! ).ConfigureAwait ( true );
            zfsTreeView.AddObjects ( treeRootNodes );
            UpdateButtonState ( );
            zfsTreeView.SetFocus ( );
        }
        catch ( Exception e )
        {
            Logger.Error ( e, "Error getting ZFS configuration tree" );
        }
        finally
        {
            if ( manageEventHandlers )
            {
                EnableEventHandlers ( );
            }
        }

        Logger.Debug ( "Finished refreshing zfs configuration tree view" );
    }

    private void ResetCurrentButtonOnClicked ( )
    {
        DisableEventHandlers ( );
        ClearAllPropertyFields ( );
        SelectedTreeNode?.CopyBaseDatasetPropertiesToTreeDataset ( );
        UpdateFieldsForSelectedZfsTreeNode ( false );
        UpdateButtonState ( );
        EnableEventHandlers ( );
    }

    private void RetentionDailyInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit daily retention setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Daily Retention Setting", $"Inherit Daily Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionDaily.Value.ToString ( )} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionDailyPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionDailyTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update daily retention setting!" );
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionDailyTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionDailyPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionDailyPropertyName, retentionDailyTextField.Text ?? "(null)", min, max );
                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for Daily snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionDailyTextField.Text = node.TreeDataset.SnapshotRetentionDaily.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionDaily.Value )
            {
                UpdateSelectedItemIntProperty ( retentionDailyTextField, ZfsPropertyNames.SnapshotRetentionDailyPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void RetentionFrequentInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit frequent retention setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Frequent Retention Setting", $"Inherit Frequent Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionFrequent.Value.ToString ( )} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionFrequentTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update frequent retention setting!" );
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionFrequentTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionFrequentPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, retentionFrequentTextField.Text ?? "(null)", min, max );

                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for Frequent snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionFrequentTextField.Text = node.TreeDataset.SnapshotRetentionFrequent.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionFrequent.Value )
            {
                UpdateSelectedItemIntProperty ( retentionFrequentTextField, ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void RetentionHourlyInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit hourly retention setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Hourly Retention Setting", $"Inherit Hourly Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionHourly.Value.ToString ( )} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionHourlyTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update hourly retention setting!" );
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionHourlyTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionHourlyPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, retentionHourlyTextField.Text ?? "(null)", min, max );
                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for Hourly snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionHourlyTextField.Text = node.TreeDataset.SnapshotRetentionHourly.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionHourly.Value )
            {
                UpdateSelectedItemIntProperty ( retentionHourlyTextField, ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void RetentionMonthlyInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit monthly retention setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Monthly Retention Setting", $"Inherit Monthly Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionMonthly.Value.ToString ( )} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionMonthlyTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update monthly retention setting!" );
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionMonthlyTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, retentionMonthlyTextField.Text ?? "(null)", min, max );
                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for Monthly snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionMonthlyTextField.Text = node.TreeDataset.SnapshotRetentionMonthly.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionMonthly.Value )
            {
                UpdateSelectedItemIntProperty ( retentionMonthlyTextField, ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void RetentionPruneDeferralInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit prune deferral setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Prune Deferral Retention Setting", $"Inherit Prune Deferral Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionPruneDeferral.Value} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionPruneDeferralTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            Logger.Warn ( $"Unexpected call to {nameof (RetentionPruneDeferralTextFieldOnLeave)} without selected node." );

            return;
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionPruneDeferralTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, retentionPruneDeferralTextField.Text ?? "(null)", min, max );
                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for PruneDeferral snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionPruneDeferralTextField.Text = node.TreeDataset.SnapshotRetentionPruneDeferral.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionPruneDeferral.Value )
            {
                UpdateSelectedItemIntProperty ( retentionPruneDeferralTextField, ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void RetentionWeeklyInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            Logger.Warn ( $"Unexpected call to {nameof (RetentionWeeklyInheritButtonClick)} without selected node." );

            return;
        }

        int queryResult = MessageBox.Query ( "Inherit Weekly Retention Setting", $"Inherit Weekly Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionWeekly.Value} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionWeeklyTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            Logger.Warn ( $"Unexpected call to {nameof (RetentionWeeklyTextFieldOnLeave)} without selected node." );

            return;
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionWeeklyTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, retentionWeeklyTextField.Text ?? "(null)", min, max );
                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for Weekly snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionWeeklyTextField.Text = node.TreeDataset.SnapshotRetentionWeekly.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionWeekly.Value )
            {
                UpdateSelectedItemIntProperty ( retentionWeeklyTextField, ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void RetentionYearlyInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update property!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Yearly Retention Setting", $"Inherit Yearly Snapshot Retention setting {node.TreeDataset.ParentDataset.SnapshotRetentionYearly.Value} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void RetentionYearlyTextFieldOnLeave ( FocusEventArgs e )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update yearly retention setting!" );
        }

        try
        {
            DisableEventHandlers ( );
            int fieldIntValue = retentionYearlyTextField.Text.ToInt32 ( -1 );
            ( int min, int max ) = ZfsPropertyValueConstants.IntPropertyRanges [ ZfsPropertyNames.SnapshotRetentionYearlyPropertyName ];

            if ( fieldIntValue < min || fieldIntValue > max )
            {
                Logger.Warn ( "Invalid value entered for {0}: {1}. Must be a valid integer between {2:D} and {3:D}", ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, retentionYearlyTextField.Text ?? "(null)", min, max );
                MessageBox.ErrorQuery ( "Invalid Retention Property Value", $"The value for Yearly snapshot retention must be an integer from 0 to {int.MaxValue:D}.\nValue will revert to previous setting.", "OK" );
                retentionYearlyTextField.Text = node.TreeDataset.SnapshotRetentionYearly.Value.ToString ( );

                return;
            }

            if ( fieldIntValue != node.TreeDataset.SnapshotRetentionYearly.Value )
            {
                UpdateSelectedItemIntProperty ( retentionYearlyTextField, ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, fieldIntValue );
            }
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private async void SaveCurrentButtonOnClicked ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit frequent retention setting!" );
        }

        try
        {
            DisableEventHandlers ( );

            if ( ConfigConsole.CommandRunner is null )
            {
                Logger.Error ( "ZFS Command runner is null. Cannot continue with save operation" );
            }

            if ( !node.IsModified || !node.IsLocallyModified )
            {
                Logger.Info ( "Selected ZFS object was not modified when save was requested. This should not happen" );

                return;
            }

            string       zfsObjectPath             = node.TreeDataset.Name;
            bool         areAnyPropertiesModified  = node.IsLocallyModified;
            bool         areAnyPropertiesInherited = node.GetInheritedZfsProperties ( out List<IZfsProperty>? inheritedZfsProperties );
            List<string> pendingCommands           = [ ];

            if ( areAnyPropertiesModified )
            {
                node.GetModifiedZfsProperties ( out List<IZfsProperty>? modifiedZfsProperties );
                pendingCommands.Add ( $"zfs set {modifiedZfsProperties!.ToStringForZfsSet ( )} {zfsObjectPath}" );
            }

            if ( areAnyPropertiesInherited )
            {
                pendingCommands.AddRange ( inheritedZfsProperties!.Select ( inheritedProperty => $"zfs inherit {inheritedProperty.Name} {zfsObjectPath}" ) );
            }

            int dialogResult = MessageBox.ErrorQuery ( "Confirm Saving ZFS Object Configuration", $"The following commands will be executed:\n{pendingCommands.ToNewlineSeparatedString ( )}\n\nTHIS OPERATION CANNOT BE UNDONE", 0, "Cancel", "Save" );

            switch ( dialogResult )
            {
                case 0:
                    Logger.Debug ( "User canceled save confirmation for ZFS object {0}", zfsObjectPath );

                    return;
                case 1:
                    Logger.Debug ( "User confirmed the pending zfs set operation {0}", pendingCommands.ToNewlineSeparatedString ( ) );

                    break;
            }

            Logger.Info ( "Saving changes to {0}", zfsObjectPath );

            if ( areAnyPropertiesModified )
            {
                node.GetModifiedZfsProperties ( out List<IZfsProperty>? modifiedZfsProperties );
                ZfsCommandRunnerOperationStatus setPropertiesResult = await ZfsTasks.SetPropertiesForDatasetAsync ( Program.Settings!.DryRun, zfsObjectPath, modifiedZfsProperties!, ConfigConsole.CommandRunner! ).ConfigureAwait ( true );
                Logger.Trace ( "Set properties result was {0}", setPropertiesResult );

                switch ( setPropertiesResult )
                {
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Debug ( "Set properties operation successful for {0}", zfsObjectPath );

                        break;
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Info ( "DRY RUN: Pretending set properties operation was successful for {0}", zfsObjectPath );

                        break;
                    case ZfsCommandRunnerOperationStatus.OneOrMoreOperationsFailed:
                    default:
                        Logger.Error ( "Setting ZFS properties for ZFS object {0} failed", zfsObjectPath );

                        break;
                }
            }

            if ( areAnyPropertiesInherited )
            {
                ZfsCommandRunnerOperationStatus inheritPropertiesResult = await ZfsTasks.InheritPropertiesForDatasetAsync ( Program.Settings!.DryRun, zfsObjectPath, inheritedZfsProperties!, ConfigConsole.CommandRunner! ).ConfigureAwait ( true );

                switch ( inheritPropertiesResult )
                {
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Info ( "DRY RUN: Pretending all requested properties were inherited successfully for {0}", zfsObjectPath );

                        break;
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Info ( "DRY RUN: Pretending all requested properties were inherited successfully for {0}", zfsObjectPath );

                        break;
                    case ZfsCommandRunnerOperationStatus.OneOrMoreOperationsFailed:
                    default:
                        Logger.Error ( "Inheriting ZFS properties for ZFS object {0} failed", zfsObjectPath );

                        break;
                }
            }

            Logger.Debug ( "Applying inheritable properties to children of {0} in tree", zfsObjectPath );
            node.CopyTreeDatasetPropertiesToBaseDataset ( );
        }
        finally
        {
            UpdateFieldsForSelectedZfsTreeNode ( false );
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
    }

    private void SetCanFocusStates ( )
    {
        zfsTreeView.Enabled                 = true;
        zfsTreeView.CanFocus                = true;
        enabledRadioGroup.Enabled           = true;
        enabledRadioGroup.CanFocus          = true;
        takeSnapshotsRadioGroup.Enabled     = true;
        takeSnapshotsRadioGroup.CanFocus    = true;
        pruneSnapshotsRadioGroup.Enabled    = true;
        pruneSnapshotsRadioGroup.CanFocus   = true;
        retentionFrequentTextField.Enabled  = true;
        retentionFrequentTextField.CanFocus = true;
        retentionHourlyTextField.Enabled    = true;
        retentionHourlyTextField.CanFocus   = true;
        retentionDailyTextField.Enabled     = true;
        retentionDailyTextField.CanFocus    = true;
        retentionWeeklyTextField.Enabled    = true;
        retentionWeeklyTextField.CanFocus   = true;
        retentionMonthlyTextField.Enabled   = true;
        retentionMonthlyTextField.CanFocus  = true;
        retentionYearlyTextField.Enabled    = true;
        retentionYearlyTextField.CanFocus   = true;
    }

    private void SetReadOnlyStates ( )
    {
        nameTextField.ReadOnly                 = true;
        typeTextField.ReadOnly                 = true;
        enabledSourceTextField.ReadOnly        = true;
        takeSnapshotsSourceTextField.ReadOnly  = true;
        pruneSnapshotsSourceTextField.ReadOnly = true;
        templateSourceTextField.ReadOnly       = true;
        recursionSourceTextField.ReadOnly      = true;
        recentFrequentTextField.ReadOnly       = true;
        recentHourlyTextField.ReadOnly         = true;
        recentDailyTextField.ReadOnly          = true;
        recentWeeklyTextField.ReadOnly         = true;
        recentMonthlyTextField.ReadOnly        = true;
        recentYearlyTextField.ReadOnly         = true;
    }

    private void SetTabStops ( )
    {
        SetTabStopsForTreeFrame ( 0 );
        SetTabStopsForGeneralPropertiesFrame ( 1 );
        SetPropertiesForRetentionPropertiesFrame ( 2 );
        SetTabStopsForActionsFrame ( 3 );

        void SetTabStopsForGeneralPropertiesFrame ( int generalFrameIndex )
        {
            generalFrame.TabStop  = true;
            generalFrame.TabIndex = generalFrameIndex;

            nameLabel.TabStop                     = false;
            nameTextField.TabStop                 = false;
            typeLabel.TabStop                     = false;
            typeTextField.TabStop                 = false;
            enabledLabel.TabStop                  = false;
            enabledRadioGroup.TabStop             = true;
            enabledRadioGroup.TabIndex            = 0;
            enabledSourceLabel.TabStop            = false;
            enabledSourceTextField.TabStop        = false;
            takeSnapshotsLabel.TabStop            = false;
            takeSnapshotsRadioGroup.TabStop       = true;
            takeSnapshotsRadioGroup.TabIndex      = 1;
            takeSnapshotsSourceLabel.TabStop      = false;
            takeSnapshotsSourceTextField.TabStop  = false;
            pruneSnapshotsLabel.TabStop           = false;
            pruneSnapshotsRadioGroup.TabStop      = true;
            pruneSnapshotsRadioGroup.TabIndex     = 2;
            pruneSnapshotsSourceLabel.TabStop     = false;
            pruneSnapshotsSourceTextField.TabStop = false;
            recursionLabel.TabStop                = false;
            recursionRadioGroup.TabStop           = true;
            recursionRadioGroup.TabIndex          = 3;
            recursionSourceLabel.TabStop          = false;
            recursionSourceTextField.TabStop      = false;
            templateLabel.TabStop                 = false;
            templateListView.TabStop              = true;
            templateListView.TabIndex             = 4;
            templateSourceLabel.TabStop           = false;
            templateSourceTextField.TabStop       = false;
        }

        void SetPropertiesForRetentionPropertiesFrame ( int retentionFrameIndex )
        {
            retentionFrame.TabStop  = true;
            retentionFrame.TabIndex = retentionFrameIndex;

            recentFrame.TabStop                 = false;
            retentionFrequentLabel.TabStop      = false;
            retentionFrequentTextField.TabStop  = true;
            retentionFrequentTextField.TabIndex = 0;
            retentionHourlyLabel.TabStop        = false;
            retentionHourlyTextField.TabStop    = true;
            retentionHourlyTextField.TabIndex   = 1;
            retentionDailyLabel.TabStop         = false;
            retentionDailyTextField.TabStop     = true;
            retentionDailyTextField.TabIndex    = 2;
            retentionWeeklyLabel.TabStop        = false;
            retentionWeeklyTextField.TabStop    = true;
            retentionWeeklyTextField.TabIndex   = 3;
            retentionMonthlyLabel.TabStop       = false;
            retentionMonthlyTextField.TabStop   = true;
            retentionMonthlyTextField.TabIndex  = 4;
            retentionYearlyLabel.TabStop        = false;
            retentionYearlyTextField.TabStop    = true;
            retentionYearlyTextField.TabIndex   = 5;
        }

        void SetTabStopsForTreeFrame ( int treeFrameIndex )
        {
            zfsConfigurationTreeFrame.TabStop  = true;
            zfsConfigurationTreeFrame.TabIndex = treeFrameIndex;
            zfsTreeView.TabStop                = true;
            zfsTreeView.TabIndex               = 0;
        }

        void SetTabStopsForActionsFrame ( int actionsFrameIndex )
        {
            zfsConfigurationActionsFrame.TabStop  = true;
            zfsConfigurationActionsFrame.TabIndex = actionsFrameIndex;
        }
    }

    private void SetTagsForPropertyFields ( )
    {
        enabledRadioGroup.Data        = new RadioGroupWithSourceViewData ( ZfsPropertyNames.EnabledPropertyName,        enabledRadioGroup,        enabledSourceTextField );
        takeSnapshotsRadioGroup.Data  = new RadioGroupWithSourceViewData ( ZfsPropertyNames.TakeSnapshotsPropertyName,  takeSnapshotsRadioGroup,  takeSnapshotsSourceTextField );
        pruneSnapshotsRadioGroup.Data = new RadioGroupWithSourceViewData ( ZfsPropertyNames.PruneSnapshotsPropertyName, pruneSnapshotsRadioGroup, pruneSnapshotsSourceTextField );
        recursionRadioGroup.Data      = new RadioGroupWithSourceViewData ( ZfsPropertyNames.RecursionPropertyName,      recursionRadioGroup,      recursionSourceTextField );
        templateListView.Data         = new ListViewWithSourceViewData ( ZfsPropertyNames.TemplatePropertyName, templateSourceTextField );
    }

    private void TakeSnapshotsInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit take snapshot setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Take Snapshots Setting", $"Inherit Take Snapshots setting {node.TreeDataset.ParentDataset.TakeSnapshots.Value} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.TakeSnapshotsPropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void TakeSnapshotsRadioGroupSelectedItemChanged ( SelectedItemChangedArgs args )
    {
        UpdateSelectedItemBooleanRadioGroupProperty ( takeSnapshotsRadioGroup );
        UpdateButtonState ( );
        UpdateFieldsForSelectedZfsTreeNode ( );
    }

    private void TemplateInheritButtonClick ( )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to inherit template setting!" );
        }

        int queryResult = MessageBox.Query ( "Inherit Template Setting", $"Inherit Template setting {node.TreeDataset.ParentDataset.Template.Value} from {node.TreeDataset.ParentDataset.Name}?", 0, "Cancel", "Inherit" );

        switch ( queryResult )
        {
            case 0:
                return;
            case 1:
                node.InheritPropertyFromParent ( ZfsPropertyNames.TemplatePropertyName );
                UpdateFieldsForSelectedZfsTreeNode ( );
                UpdateButtonState ( );

                return;
        }
    }

    private void TemplateListViewOnSelectedItemChanged ( ListViewItemEventArgs? args )
    {
        ArgumentNullException.ThrowIfNull ( args, nameof (args) );

        if ( templateListView.Data is not ListViewWithSourceViewData viewData )
        {
            throw new InvalidOperationException ( "Invalid or missing data when updating template data." );
        }

        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update property!" );
        }

        ref readonly ZfsProperty<string> newProperty = ref node.UpdateTreeNodeProperty ( viewData.PropertyName, ConfigConsole.TemplateListItems [ args.Item ].TemplateName );
        viewData.SourceTextField.Text = newProperty.InheritedFrom;
        UpdateFieldsForSelectedZfsTreeNode ( );
        UpdateButtonState ( );
    }

    private void UpdateButtonState ( )
    {
        if ( zfsTreeView.Objects.Any ( ) && zfsTreeView.SelectedObject is ZfsObjectConfigurationTreeNode node )
        {
            resetCurrentButton.Enabled = saveCurrentButton.Enabled = node is { IsModified: true, IsLocallyModified: true };
        }
        else
        {
            resetCurrentButton.Enabled = false;
            saveCurrentButton.Enabled  = false;
        }
    }

    private void UpdateEnabledPropertyFields ( ZfsRecord treeDataset )
    {
        enabledRadioGroup.SelectedItem = treeDataset.Enabled.AsTrueFalseRadioIndex ( );
        enabledRadioGroup.ColorScheme  = treeDataset.Enabled.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        enabledRadioGroup.Enabled      = true;
        enabledSourceTextField.Text    = treeDataset.Enabled.InheritedFrom;
        enabledInheritButton.Enabled   = treeDataset is { IsPoolRoot: false, Enabled.IsLocal: true };
    }

    private void UpdateFieldsForSelectedZfsTreeNode ( bool manageEventHandlers = true )
    {
        if ( manageEventHandlers )
        {
            DisableEventHandlers ( );
        }

        if ( SelectedTreeNode?.TreeDataset is { } treeDataset )
        {
            nameTextField.Text = treeDataset.Name;
            typeTextField.Text = treeDataset.Kind;
            UpdateEnabledPropertyFields ( treeDataset );
            UpdateTakeSnapshotFields ( treeDataset );
            UpdatePruneSnapshotsFields ( treeDataset );
            recursionRadioGroup.Enabled = true;

            try
            {
                recursionRadioGroup.SelectedItem = treeDataset.Recursion.Value switch { ZfsPropertyValueConstants.SnapsInAZfs => 0, ZfsPropertyValueConstants.ZfsRecursion => 1, _ => throw new InvalidOperationException ( $"Invalid recursion value {treeDataset.Recursion.Value}" ) };
                recursionRadioGroup.ColorScheme  = treeDataset.Recursion.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
                recursionSourceTextField.Text    = treeDataset.Recursion.InheritedFrom;
            }
            catch ( InvalidOperationException e )
            {
                int dialogResult = MessageBox.ErrorQuery ( "Invalid ZFS Property Value", e.Message, 2, $"Set to '{ZfsPropertyValueConstants.SnapsInAZfs}'", $"Set to '{ZfsPropertyValueConstants.ZfsRecursion}'", "Do Nothing", "Exit" );

                switch ( dialogResult )
                {
                    case 0:
                    case 1:
                        recursionRadioGroup.SelectedItem = dialogResult;
                        UpdateSelectedItemStringRadioGroupProperty ( recursionRadioGroup );
                        recursionRadioGroup.ColorScheme = treeDataset.Recursion.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
                        recursionSourceTextField.Text   = treeDataset.Recursion.InheritedFrom;
                        UpdateButtonState ( );

                        break;
                    case 2:
                        break;
                    case 3:
                        Application.RequestStop ( );

                        return;
                    default:
                        throw new InvalidOperationException ( $"Unexpected dialogResult received: {dialogResult}", e );
                }
            }

            recursionInheritButton.Enabled = treeDataset is { IsPoolRoot: false, Recursion.IsLocal: true };

            int templateIndex = ConfigConsole.TemplateListItems.FindIndex ( t => t.TemplateName == treeDataset.Template.Value );

            if ( templateIndex == -1 )
            {
                MessageBox.ErrorQuery ( "Template Not Found", $"The template \"{treeDataset.Template.Value}\", for {treeDataset.Kind} \"{treeDataset.Name}\", was not found in SnapsInAZfs' configuration.\nIs the template defined in one of the expected SnapsInAZfs[.local].json files?\n\nEditing of ZFS properties will be disabled for this session.\nFix the template configuration and run the Config Console again.", "OK - ZFS Configuration Window will be disabled for this session" );
                SnapsInAZfsConfigConsole.ZfsConfigurationWindowDisabledDueToError = true;
                SuperView.Remove ( this );

                return;
            }

            templateListView.SelectedItem = templateIndex;
            templateListView.ColorScheme  = treeDataset.Template.IsInherited ? inheritedPropertyListViewColorScheme : localPropertyListViewColorScheme;
            templateListView.EnsureSelectedItemVisible ( );
            templateListView.Enabled      = true;
            templateSourceTextField.Text  = treeDataset.Template.InheritedFrom;
            templateInheritButton.Enabled = treeDataset is { IsPoolRoot: false, Template.IsLocal: true };

            retentionFrequentTextField.Text             = treeDataset.SnapshotRetentionFrequent.ValueString;
            retentionFrequentTextField.ColorScheme      = treeDataset.SnapshotRetentionFrequent.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionFrequentTextField.Enabled          = true;
            retentionFrequentInheritButton.Enabled      = treeDataset is { IsPoolRoot: false, SnapshotRetentionFrequent.IsLocal: true };
            retentionHourlyTextField.Text               = treeDataset.SnapshotRetentionHourly.ValueString;
            retentionHourlyTextField.ColorScheme        = treeDataset.SnapshotRetentionHourly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionHourlyTextField.Enabled            = true;
            retentionHourlyInheritButton.Enabled        = treeDataset is { IsPoolRoot: false, SnapshotRetentionHourly.IsLocal: true };
            retentionDailyTextField.Text                = treeDataset.SnapshotRetentionDaily.ValueString;
            retentionDailyTextField.ColorScheme         = treeDataset.SnapshotRetentionDaily.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionDailyTextField.Enabled             = true;
            retentionDailyInheritButton.Enabled         = treeDataset is { IsPoolRoot: false, SnapshotRetentionDaily.IsLocal: true };
            retentionWeeklyTextField.Text               = treeDataset.SnapshotRetentionWeekly.ValueString;
            retentionWeeklyTextField.ColorScheme        = treeDataset.SnapshotRetentionWeekly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionWeeklyTextField.Enabled            = true;
            retentionWeeklyInheritButton.Enabled        = treeDataset is { IsPoolRoot: false, SnapshotRetentionWeekly.IsLocal: true };
            retentionMonthlyTextField.Text              = treeDataset.SnapshotRetentionMonthly.ValueString;
            retentionMonthlyTextField.ColorScheme       = treeDataset.SnapshotRetentionMonthly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionMonthlyTextField.Enabled           = true;
            retentionMonthlyInheritButton.Enabled       = treeDataset is { IsPoolRoot: false, SnapshotRetentionMonthly.IsLocal: true };
            retentionYearlyTextField.Text               = treeDataset.SnapshotRetentionYearly.ValueString;
            retentionYearlyTextField.ColorScheme        = treeDataset.SnapshotRetentionYearly.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionYearlyTextField.Enabled            = true;
            retentionYearlyInheritButton.Enabled        = treeDataset is { IsPoolRoot: false, SnapshotRetentionYearly.IsLocal: true };
            retentionPruneDeferralTextField.Text        = treeDataset.SnapshotRetentionPruneDeferral.ValueString;
            retentionPruneDeferralTextField.ColorScheme = treeDataset.SnapshotRetentionPruneDeferral.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
            retentionPruneDeferralTextField.Enabled     = true;
            retentionPruneDeferralInheritButton.Enabled = treeDataset is { IsPoolRoot: false, SnapshotRetentionPruneDeferral.IsLocal: true };

            recentFrequentTextField.Text = treeDataset.LastFrequentSnapshotTimestamp.IsLocal ? treeDataset.LastFrequentSnapshotTimestamp.ValueString : "None";
            recentHourlyTextField.Text   = treeDataset.LastHourlySnapshotTimestamp.IsLocal ? treeDataset.LastHourlySnapshotTimestamp.ValueString : "None";
            recentDailyTextField.Text    = treeDataset.LastDailySnapshotTimestamp.IsLocal ? treeDataset.LastDailySnapshotTimestamp.ValueString : "None";
            recentWeeklyTextField.Text   = treeDataset.LastWeeklySnapshotTimestamp.IsLocal ? treeDataset.LastWeeklySnapshotTimestamp.ValueString : "None";
            recentMonthlyTextField.Text  = treeDataset.LastMonthlySnapshotTimestamp.IsLocal ? treeDataset.LastMonthlySnapshotTimestamp.ValueString : "None";
            recentYearlyTextField.Text   = treeDataset.LastYearlySnapshotTimestamp.IsLocal ? treeDataset.LastYearlySnapshotTimestamp.ValueString : "None";
        }

        if ( manageEventHandlers )
        {
            EnableEventHandlers ( );
        }
    }

    private void UpdatePruneSnapshotsFields ( ZfsRecord treeDataset )
    {
        pruneSnapshotsRadioGroup.SelectedItem = treeDataset.PruneSnapshots.AsTrueFalseRadioIndex ( );
        pruneSnapshotsRadioGroup.ColorScheme  = treeDataset.PruneSnapshots.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        pruneSnapshotsRadioGroup.Enabled      = true;
        pruneSnapshotsSourceTextField.Text    = treeDataset.PruneSnapshots.InheritedFrom;
        pruneSnapshotsInheritButton.Enabled   = treeDataset is { IsPoolRoot: false, PruneSnapshots.IsLocal: true };
    }

    private void UpdateSelectedItemBooleanRadioGroupProperty ( RadioGroup radioGroup )
    {
        if ( radioGroup.Data is not RadioGroupWithSourceViewData viewData )
        {
            throw new ArgumentException ( "Invalid or missing data when updating boolean value for radio group.", nameof (radioGroup) );
        }

        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update property!" );
        }

        ref readonly ZfsProperty<bool> newProperty = ref node.UpdateTreeNodeProperty ( viewData.PropertyName, radioGroup.GetSelectedBooleanFromLabel ( ) );
        viewData.SourceTextField.Text = newProperty.InheritedFrom;
    }

    private void UpdateSelectedItemIntProperty ( TextValidateField field, string propertyName, int propertyValue )
    {
        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update property!" );
        }

        ref readonly ZfsProperty<int> newProperty = ref node.UpdateTreeNodeProperty ( propertyName, propertyValue );
        field.ColorScheme = newProperty.IsInherited ? inheritedPropertyTextFieldColorScheme : localPropertyTextFieldColorScheme;
    }

    private void UpdateSelectedItemStringRadioGroupProperty ( RadioGroup radioGroup )
    {
        if ( radioGroup.Data is not RadioGroupWithSourceViewData viewData )
        {
            throw new ArgumentException ( "Invalid or missing data when updating string value for radio group.", nameof (radioGroup) );
        }

        if ( SelectedTreeNode is not { } node )
        {
            throw new InvalidOperationException ( "Null tree node on attempt to update property!" );
        }

        ref readonly ZfsProperty<string> newProperty = ref node.UpdateTreeNodeProperty ( viewData.PropertyName, radioGroup.GetSelectedLabelString ( ) );
        viewData.SourceTextField.Text = newProperty.InheritedFrom;
    }

    private void UpdateTakeSnapshotFields ( ZfsRecord treeDataset )
    {
        takeSnapshotsRadioGroup.SelectedItem = treeDataset.TakeSnapshots.AsTrueFalseRadioIndex ( );
        takeSnapshotsRadioGroup.ColorScheme  = treeDataset.TakeSnapshots.IsInherited ? inheritedPropertyRadioGroupColorScheme : localPropertyRadioGroupColorScheme;
        takeSnapshotsRadioGroup.Enabled      = true;
        takeSnapshotsSourceTextField.Text    = treeDataset.TakeSnapshots.InheritedFrom;
        takeSnapshotsInheritButton.Enabled   = treeDataset is { IsPoolRoot: false, TakeSnapshots.IsLocal: true };
    }

    private async void ZfsConfigurationWindowOnInitialized ( object? sender, EventArgs e )
    {
        try
        {
            Logger.Trace ( "Zfs Configuration Window initialized" );
            ConfiguredTaskAwaitable zfsRefreshTask = RefreshZfsTreeViewFromZfsAsync ( ).ConfigureAwait ( true );
            templateListView.SetSource ( ConfigConsole.TemplateListItems );
            SetCanFocusStates ( );
            SetTagsForPropertyFields ( );
            SetTabStops ( );
            SetReadOnlyStates ( );
            await zfsRefreshTask;
            UpdateButtonState ( );
            EnableEventHandlers ( );
        }
        catch ( Exception ex )
        {
            Logger.Error ( ex, $"Error in {nameof (ZfsConfigurationWindow)} initialization." );
            int dialogResult = MessageBox.ErrorQuery ( $"Error initializing {nameof (ZfsConfigurationWindow)}", $"Error initializing {nameof (ZfsConfigurationWindow)}.\r\nIt is recommended that you exit now.", 0, "Exit", "Continue (not recommended)" );

            switch ( dialogResult )
            {
                case 0:
                    Logger.Info ( $"User exited as suggested after {nameof (ZfsConfigurationWindow)} initialization error." );

                    return;
                case 1:
                    Logger.Warn ( $"User chose to continue after {nameof (ZfsConfigurationWindow)} initialization error." );

                    Environment.FailFast ( null );

                    break;
            }
        }
    }

    private void zfsTreeViewOnSelectionChanged ( object? sender, SelectionChangedEventArgs<ITreeNode> e )
    {
        ArgumentNullException.ThrowIfNull ( sender );
        DisableEventHandlers ( );

        ClearAllPropertyFields ( );

        UpdateFieldsForSelectedZfsTreeNode ( false );
        UpdateButtonState ( );
        EnableEventHandlers ( );
    }

    private sealed record RadioGroupWithSourceViewData ( string PropertyName, RadioGroup RadioGroup, TextField SourceTextField );

    private sealed record ListViewWithSourceViewData ( string PropertyName, TextField SourceTextField );
}
