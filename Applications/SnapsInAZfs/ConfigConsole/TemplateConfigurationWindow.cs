#region MIT LICENSE

// Copyright 2024 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Collections.Concurrent;
using NStack;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TemplateConfigurationListItem = SnapsInAZfs.ConfigConsole.TreeNodes.TemplateConfigurationListItem;

namespace SnapsInAZfs.ConfigConsole;

public sealed partial class TemplateConfigurationWindow
{
    internal static bool TemplatesAddedRemovedOrModified;
    private static readonly ustring InvalidFieldValueDialogTitle = ustring.Make( InvalidFieldValueDialogTitleString );

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private static readonly List<int> TemplateConfigurationFrequentPeriodOptions = [5, 10, 15, 20, 30];

    private readonly HashSet<string> _modifiedProperties = [];
    private readonly HashSet<string> _namingProperties = [ComponentSeparator, PrefixTitleCase, TimestampFormatTitleCase, FrequentSuffixTitleCase, HourlySuffixTitleCase, DailySuffixTitleCase, WeeklySuffixTitleCase, MonthlySuffixTitleCase, YearlySuffixTitleCase];
    private readonly List<TextValidateFieldSettings> _templateConfigurationTextValidateFieldList = [];
    private readonly HashSet<string> _timingProperties = [FrequentPeriodTitleCase, HourlyMinuteTitleCase, DailyTimeTitleCase, WeeklyDayTitleCase, WeeklyTimeTitleCase, MonthlyDayTitleCase, MonthlyTimeTitleCase, YearlyMonthTitleCase, YearlyDayTitleCase, YearlyTimeTitleCase];
    private static ConcurrentDictionary<string, TemplateSettings> _templates = [];

    public TemplateConfigurationWindow( )
    {
        InitializeComponent( );
        InitializeTemplateEditorView( );
    }

    private bool _templateConfigurationEventsEnabled;
    internal static bool IsAnyTemplateModified => ConfigConsole.TemplateListItems.Any( static t => t.IsModified );
    internal TemplateConfigurationListItem SelectedTemplateItem => ConfigConsole.TemplateListItems[ templateListView.SelectedItem ];
    private bool IsEveryPropertyTextValidateFieldValid => _templateConfigurationTextValidateFieldList.TrueForAll( static tvf => tvf.Field.IsValid );
    private bool IsSelectedTemplateInUse => ConfigConsole.BaseDatasets.Any( kvp => kvp.Value.Template.Value == SelectedTemplateItem.TemplateName );

    private const string ComponentSeparator = "Component Separator";
    private const string DailySuffixTitleCase = "Daily Suffix";
    private const string DailyTimeTitleCase = "Daily Time";
    private const string FrequentPeriodTitleCase = "Frequent Period";
    private const string FrequentSuffixTitleCase = "Frequent Suffix";
    private const string HourlyMinuteTitleCase = "Hourly Minute";
    private const string HourlySuffixTitleCase = "Hourly Suffix";
    private const string InvalidFieldValueDialogTitleString = "Invalid Field Value";
    private const string MonthlyDayTitleCase = "Monthly Day";
    private const string MonthlySuffixTitleCase = "Monthly Suffix";
    private const string MonthlyTimeTitleCase = "Monthly Time";
    private const string PrefixTitleCase = "Prefix";
    private const string TimestampFormatTitleCase = "Timestamp Format";
    private const string WeeklyDayTitleCase = "Weekly Day";
    private const string WeeklySuffixTitleCase = "Weekly Suffix";
    private const string WeeklyTimeTitleCase = "Weekly Time";
    private const string YearlyDayTitleCase = "Yearly Day";
    private const string YearlyMonthTitleCase = "Yearly Month";
    private const string YearlySuffixTitleCase = "Yearly Suffix";
    private const string YearlyTimeTitleCase = "Yearly Time";

    internal static void CommitModifiedTemplates( )
    {
        Logger.Debug( "Committing modified templates to base collections" );
        foreach ( TemplateConfigurationListItem currentTemplate in ConfigConsole.TemplateListItems.Where( static currentTemplate => currentTemplate.IsModified ) )
        {
            TemplatesAddedRemovedOrModified = true;
            // ReSharper disable once HeapView.ObjectAllocation
            currentTemplate.BaseSettings = currentTemplate.ViewSettings with { };
            _templates[ currentTemplate.TemplateName ] = currentTemplate.BaseSettings;
        }

        Program.Settings!.Templates = _templates.ToDictionary( static pair => pair.Key, static pair => pair.Value );
    }

    private void AddTemplateButtonOnClicked( )
    {
        if ( !_templates.TryGetValue( "default", out _ ) )
        {
            const string errorMessage = "'default' template does not exist. Not creating new template.";
            Logger.Error( errorMessage );
            MessageBox.ErrorQuery( "Error Adding Template", errorMessage, 0, "OK" );
            return;
        }

        if ( !newTemplateNameTextValidateField.IsValid )
        {
            const string errorMessage = "New template name not valid. Not creating new template.";
            Logger.Error( errorMessage );
            MessageBox.ErrorQuery( "Error Adding Template", errorMessage, 0, "OK" );
            return;
        }

        string? newTemplateName = newTemplateNameTextValidateField.Text.ToString( );
        if ( _templates.ContainsKey( newTemplateName! ) )
        {
            string errorMessage = $"A template named {newTemplateName} already exists.";
            Logger.Error( errorMessage );
            MessageBox.ErrorQuery( "Error Adding Template", errorMessage, 0, "OK" );
            return;
        }

        _templates[ newTemplateName! ] = SelectedTemplateItem.ViewSettings with { };
        ConfigConsole.TemplateListItems.Add( new( newTemplateName!, SelectedTemplateItem.ViewSettings with { }, SelectedTemplateItem.ViewSettings with { } ) );
        TemplatesAddedRemovedOrModified = true;
        UpdateTemplateListButtonStates( );
        UpdateTemplatePropertiesButtonStates( );
    }

    private void ApplyCurrentButtonOnClicked( )
    {
        if ( !IsEveryPropertyTextValidateFieldValid )
        {
            Logger.Error( "Apply template button was clicked while fields were not valid. This should not happen. Please report this." );
            return;
        }

        if ( _modifiedProperties.Count == 0 )
        {
            Logger.Error( "Apply template button was clicked while no values were different from their original values. This should not happen. Please report this." );
            return;
        }

        if ( SelectedTemplateItem.TemplateName == "default" )
        {
            int dialogResult = MessageBox.ErrorQuery( "Modifying Default Template", "You are about to modify the default template.\nThis is not recommended.\nApply changes anyway?", "Cancel", "Apply Default Template Changes" );
            if ( dialogResult == 0 )
            {
                return;
            }
        }

        if ( _modifiedProperties.Overlaps( _namingProperties ) )
        {
            SelectedTemplateItem.ViewSettings.Formatting = new( )
            {
                ComponentSeparator = componentSeparatorValidateField.Text.ToString( )!,
                Prefix = prefixTextValidateField.Text.ToString( )!,
                TimestampFormatString = timestampFormatTextField.Text.ToString( )!,
                FrequentSuffix = frequentSuffixTextValidateField.Text.ToString( )!,
                HourlySuffix = hourlySuffixTextValidateField.Text.ToString( )!,
                DailySuffix = dailySuffixTextValidateField.Text.ToString( )!,
                WeeklySuffix = weeklySuffixTextValidateField.Text.ToString( )!,
                MonthlySuffix = monthlySuffixTextValidateField.Text.ToString( )!,
                YearlySuffix = yearlySuffixTextValidateField.Text.ToString( )!
            };
        }

        if ( _modifiedProperties.Overlaps( _timingProperties ) )
        {
            SelectedTemplateItem.ViewSettings.SnapshotTiming = new( )
            {
                FrequentPeriod = int.Parse( frequentPeriodRadioGroup.GetSelectedLabelString( ) ),
                HourlyMinute = hourlyMinuteTextValidateField.Text.ToInt32( ),
                DailyTime = dailyTimeTimeField.Time.ToTimeOnly( ),
                WeeklyDay = (DayOfWeek)weeklyDayComboBox.SelectedItem,
                WeeklyTime = weeklyTimeTimeField.Time.ToTimeOnly( ),
                MonthlyDay = monthlyDayTextValidateField.Text.ToInt32( ),
                MonthlyTime = monthlyTimeTimeField.Time.ToTimeOnly( ),
                YearlyMonth = yearlyMonthComboBox.SelectedItem + 1,
                YearlyDay = yearlyDayTextValidateField.Text.ToInt32( ),
                YearlyTime = yearlyTimeTimeField.Time.ToTimeOnly( )
            };
        }

        _modifiedProperties.Clear( );
        UpdatePropertiesFrameViewState( );
    }

    private void ComponentSeparatorValidateFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( ComponentSeparator );

        if ( !componentSeparatorValidateField.IsValid )
        {
            MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, $"Value entered for {ComponentSeparator} is invalid. Must be exactly one character from the following set: [0-9a-zA-Z:.+_-]" );
            componentSeparatorValidateField.SetFocus( );
        }

        if ( SelectedTemplateItem.ViewSettings.Formatting.ComponentSeparator != componentSeparatorValidateField.Text.ToString( ) )
        {
            _modifiedProperties.Add( ComponentSeparator );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void DailySuffixTextValidateFieldOnLeave( FocusEventArgs e )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( DailySuffixTitleCase );

            if ( !dailySuffixTextValidateField.IsValid )
            {
                int messageBoxResult = MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Daily Suffix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z].\nDefault is \"daily\".", 1, "Fix It Myself", "Use Default Value" );
                if ( messageBoxResult == 1 )
                {
                    dailySuffixTextValidateField.Text = "hourly";
                }
                else
                {
                    dailySuffixTextValidateField.SetFocus( );
                    return;
                }
            }

            string suffix = dailySuffixTextValidateField.Text.ToString( )!;
            if ( SelectedTemplateItem.ViewSettings.Formatting.DailySuffix != suffix )
            {
                _modifiedProperties.Add( DailySuffixTitleCase );
            }

            UpdatePropertiesFrameViewState( suffix );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void DailyTimeTimeFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( DailyTimeTitleCase );

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.DailyTime != dailyTimeTimeField.Time.ToTimeOnly( ) )
        {
            _modifiedProperties.Add( DailyTimeTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    /// <exception cref="ApplicationException">
    ///     If removal of the selected template from <see cref="_templates" /> fails.<br />
    ///     Should be treated as fatal by any consumers
    /// </exception>
    private void DeleteDeleteTemplateButtonOnClicked( )
    {
        try
        {
            DisableEventHandlers( );
            string templateName = SelectedTemplateItem.TemplateName;
            Logger.Debug( "Validating selected template {0} for removal", templateName );
            if ( templateName == "default" )
            {
                const string errorMessage = "Cannot delete the default template.";
                Logger.Warn( errorMessage );
                MessageBox.ErrorQuery( "Cannot Delete Template", errorMessage, 0, "OK" );
                return;
            }

            if ( IsSelectedTemplateInUse )
            {
                string errorMessage = $"Selected template {templateName} is in use by one or more objects in ZFS. Cannot delete template.";
                Logger.Warn( errorMessage );
                MessageBox.ErrorQuery( "Cannot Delete Template", errorMessage, 0, "OK" );
                return;
            }

            Logger.Debug( "Template {0} can be removed. Removing", templateName );

            // Grab the currently selected index, decrement the selection in the ListView,
            // and remove the saved index from the source collection
            int indexToRemove = templateListView.SelectedItem;
            templateListView.SelectedItem -= 1;
            templateListView.EnsureSelectedItemVisible( );
            ConfigConsole.TemplateListItems.RemoveAt( indexToRemove );
            if ( !_templates.TryRemove( templateName, out _ ) )
            {
                // The application state is inconsistent if this happens, and it isn't safe to continue
                string errorMessage = $"Failed to remove template {templateName} from UI dictionary";
                TemplateRemovalException ex = new( errorMessage );
                Logger.Fatal( ex, errorMessage );
                Environment.FailFast( errorMessage, ex );
                throw ex;
            }

            TemplatesAddedRemovedOrModified = true;
        }
        catch ( ApplicationException )
        {
            // If we threw the KeyNotFoundException above, we need to exit.
            // Also, re-throw so we can deal with it in the caller as well.
            Application.Top.RequestStop( );
            throw;
        }

        UpdateTemplateListButtonStates( );
        EnableEventHandlers( );
    }

    private void DisableEventHandlers( )
    {
        if ( !_templateConfigurationEventsEnabled )
        {
            return;
        }

        // Template list object events
        templateListView.SelectedItemChanged -= TemplateListViewOnSelectedItemChanged;
        addTemplateButton.Clicked -= AddTemplateButtonOnClicked;
        deleteTemplateButton.Clicked -= DeleteDeleteTemplateButtonOnClicked;
        newTemplateNameTextValidateField.KeyPress -= NewTemplateNameTextValidateFieldOnKeyPress;
        commitAllButton.Clicked -= ShowCommitConfirmationDialog;

        // Properties area field/button events
        applyCurrentButton.Clicked -= ApplyCurrentButtonOnClicked;
        resetCurrentButton.Clicked -= ResetCurrentButtonOnClicked;

        componentSeparatorValidateField.Leave -= ComponentSeparatorValidateFieldOnLeave;
        prefixTextValidateField.Leave -= PrefixTextValidateFieldOnLeave;
        timestampFormatTextField.Leave -= TimestampFormatTextFieldOnLeave;
        frequentSuffixTextValidateField.Leave -= FrequentSuffixTextValidateFieldOnLeave;
        hourlySuffixTextValidateField.Leave -= HourlySuffixTextValidateFieldOnLeave;
        dailySuffixTextValidateField.Leave -= DailySuffixTextValidateFieldOnLeave;
        weeklySuffixTextValidateField.Leave -= WeeklySuffixTextValidateFieldOnLeave;
        monthlySuffixTextValidateField.Leave -= MonthlySuffixTextValidateFieldOnLeave;
        yearlySuffixTextValidateField.Leave -= YearlySuffixTextValidateFieldOnLeave;

        frequentPeriodRadioGroup.SelectedItemChanged -= FrequentPeriodRadioGroupOnSelectedItemChanged;
        yearlyMonthComboBox.SelectedItemChanged -= YearlyMonthComboBoxOnSelectedItemChanged;
        yearlyDayTextValidateField.Leave -= YearlyDayTextValidateFieldOnLeave;
        yearlyTimeTimeField.Leave -= YearlyTimeTimeFieldOnLeave;
        monthlyDayTextValidateField.Leave -= MonthlyDayTextValidateFieldOnLeave;
        monthlyTimeTimeField.Leave -= MonthlyTimeTimeFieldOnLeave;
        weeklyDayComboBox.SelectedItemChanged -= WeeklyDayComboBoxOnSelectedItemChanged;
        weeklyTimeTimeField.Leave -= WeeklyTimeTimeFieldOnLeave;
        dailyTimeTimeField.Leave -= DailyTimeTimeFieldOnLeave;
        hourlyMinuteTextValidateField.Leave -= HourlyMinuteTextValidateFieldOnLeave;

        _templateConfigurationEventsEnabled = false;
    }

    private static void EatKeyPresses( KeyEventEventArgs e )
    {
        // Disallow editing this combobox text
        // If the key press event is any control sequence, just return (to allow hot keys to work).
        // If it's a character, set Handled=true to swallow the event and not change the text.
        // The Key enum is an unsigned int, so we can mask and check for greater than zero
        if ( e.KeyEvent.IsCtrl )
        {
            return;
        }

        if ( ( e.KeyEvent.Key & Key.CharMask ) > 0 )
        {
            e.Handled = true;
        }
    }

    private void EnableEventHandlers( )
    {
        if ( _templateConfigurationEventsEnabled )
        {
            return;
        }

        // Template list object events
        templateListView.SelectedItemChanged += TemplateListViewOnSelectedItemChanged;
        addTemplateButton.Clicked += AddTemplateButtonOnClicked;
        deleteTemplateButton.Clicked += DeleteDeleteTemplateButtonOnClicked;
        newTemplateNameTextValidateField.KeyPress += NewTemplateNameTextValidateFieldOnKeyPress;
        commitAllButton.Clicked += ShowCommitConfirmationDialog;

        // Properties area field/button events
        applyCurrentButton.Clicked += ApplyCurrentButtonOnClicked;
        resetCurrentButton.Clicked += ResetCurrentButtonOnClicked;

        componentSeparatorValidateField.Leave += ComponentSeparatorValidateFieldOnLeave;
        prefixTextValidateField.Leave += PrefixTextValidateFieldOnLeave;
        timestampFormatTextField.Leave += TimestampFormatTextFieldOnLeave;
        frequentSuffixTextValidateField.Leave += FrequentSuffixTextValidateFieldOnLeave;
        hourlySuffixTextValidateField.Leave += HourlySuffixTextValidateFieldOnLeave;
        dailySuffixTextValidateField.Leave += DailySuffixTextValidateFieldOnLeave;
        weeklySuffixTextValidateField.Leave += WeeklySuffixTextValidateFieldOnLeave;
        monthlySuffixTextValidateField.Leave += MonthlySuffixTextValidateFieldOnLeave;
        yearlySuffixTextValidateField.Leave += YearlySuffixTextValidateFieldOnLeave;

        frequentPeriodRadioGroup.SelectedItemChanged += FrequentPeriodRadioGroupOnSelectedItemChanged;
        yearlyMonthComboBox.SelectedItemChanged += YearlyMonthComboBoxOnSelectedItemChanged;
        yearlyDayTextValidateField.Leave += YearlyDayTextValidateFieldOnLeave;
        yearlyTimeTimeField.Leave += YearlyTimeTimeFieldOnLeave;
        monthlyDayTextValidateField.Leave += MonthlyDayTextValidateFieldOnLeave;
        monthlyTimeTimeField.Leave += MonthlyTimeTimeFieldOnLeave;
        weeklyDayComboBox.SelectedItemChanged += WeeklyDayComboBoxOnSelectedItemChanged;
        weeklyTimeTimeField.Leave += WeeklyTimeTimeFieldOnLeave;
        dailyTimeTimeField.Leave += DailyTimeTimeFieldOnLeave;
        hourlyMinuteTextValidateField.Leave += HourlyMinuteTextValidateFieldOnLeave;

        _templateConfigurationEventsEnabled = true;
    }

    private void FrequentPeriodRadioGroupOnSelectedItemChanged( SelectedItemChangedArgs obj )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( FrequentPeriodTitleCase );

            if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.FrequentPeriod != int.Parse( frequentPeriodRadioGroup.GetSelectedLabelString( ) ) )
            {
                _modifiedProperties.Add( FrequentPeriodTitleCase );
            }

            UpdatePropertiesFrameViewState( );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void FrequentSuffixTextValidateFieldOnLeave( FocusEventArgs e )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( FrequentSuffixTitleCase );

            if ( !frequentSuffixTextValidateField.IsValid )
            {
                int messageBoxResult = MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Frequent Suffix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z].\nDefault is \"frequently\".", 1, "Fix It Myself", "Use Default Value" );
                if ( messageBoxResult == 1 )
                {
                    frequentSuffixTextValidateField.Text = "frequently";
                }
                else
                {
                    frequentSuffixTextValidateField.SetFocus( );
                    return;
                }
            }

            string suffix = frequentSuffixTextValidateField.Text.ToString( )!;
            if ( SelectedTemplateItem.ViewSettings.Formatting.FrequentSuffix != suffix )
            {
                _modifiedProperties.Add( FrequentSuffixTitleCase );
            }

            UpdatePropertiesFrameViewState( suffix );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void HourlyMinuteTextValidateFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( HourlyMinuteTitleCase );

        if ( !hourlyMinuteTextValidateField.IsValid )
        {
            MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, $"Value entered for {HourlyMinuteTitleCase} is invalid. Must be a 1 or 2 digit numeric value from 0 to 59" );
            monthlyDayTextValidateField.SetFocus( );
            return;
        }

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.HourlyMinute != int.Parse( hourlyMinuteTextValidateField.Text.ToString( )! ) )
        {
            _modifiedProperties.Add( HourlyMinuteTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void HourlySuffixTextValidateFieldOnLeave( FocusEventArgs e )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( HourlySuffixTitleCase );

            if ( !hourlySuffixTextValidateField.IsValid )
            {
                int messageBoxResult = MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Hourly Suffix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z].\nDefault is \"hourly\".", 1, "Fix It Myself", "Use Default Value" );
                if ( messageBoxResult == 1 )
                {
                    hourlySuffixTextValidateField.Text = "hourly";
                }
                else
                {
                    hourlySuffixTextValidateField.SetFocus( );
                    return;
                }
            }

            string suffix = hourlySuffixTextValidateField.Text.ToString( )!;
            if ( SelectedTemplateItem.ViewSettings.Formatting.HourlySuffix != suffix )
            {
                _modifiedProperties.Add( HourlySuffixTitleCase );
            }

            UpdatePropertiesFrameViewState( suffix );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void InitializeComboBoxes( )
    {
        yearlyMonthComboBox.SetSource( CultureTimeHelpers.MonthNamesLong );
        yearlyMonthComboBox.ReadOnly = true;
        yearlyMonthComboBox.HideDropdownListOnClick = true;
        yearlyMonthComboBox.KeyPress += EatKeyPresses;

        weeklyDayComboBox.SetSource( CultureTimeHelpers.DayNamesLong );
        weeklyDayComboBox.ReadOnly = true;
        weeklyDayComboBox.HideDropdownListOnClick = true;
        weeklyDayComboBox.KeyPress += EatKeyPresses;
    }

    private void InitializeTemplateEditorView( )
    {
        DisableEventHandlers( );
        templateListView.SetSource( ConfigConsole.TemplateListItems );
        _templates.Clear( );
        _templates = new( Program.Settings!.Templates );
        InitializeComboBoxes( );
        SetInitialButtonState( );
        InitializeTemplatePropertiesTextValidateFieldList( );
        SetValidateOnInputForAllTextValidateFields( );
        SetReadOnlyFields( );
        UpdateTemplateListButtonStates( );
        UpdateTemplatePropertiesButtonStates( );
        SetTabStops( );
        EnableEventHandlers( );
        templateListView.SelectedItem = 0;
        templateListView.SetFocus( );
    }

    private void InitializeTemplatePropertiesTextValidateFieldList( )
    {
        _templateConfigurationTextValidateFieldList.Clear( );
        _templateConfigurationTextValidateFieldList.Add( new( componentSeparatorValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( prefixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( frequentSuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( hourlySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( dailySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( weeklySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( monthlySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( yearlySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( hourlyMinuteTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( monthlyDayTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( yearlyDayTextValidateField, true ) );
    }

    private void MonthlyDayTextValidateFieldOnLeave( FocusEventArgs obj )
    {
        _modifiedProperties.Remove( MonthlyDayTitleCase );

        if ( !monthlyDayTextValidateField.IsValid )
        {
            MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, $"Value entered for {MonthlyDayTitleCase} is invalid. Must be a 1 or 2 digit positive numeric value" );
            monthlyDayTextValidateField.SetFocus( );
            return;
        }

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.MonthlyDay != int.Parse( monthlyDayTextValidateField.Text.ToString( )! ) )
        {
            _modifiedProperties.Add( MonthlyDayTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void MonthlySuffixTextValidateFieldOnLeave( FocusEventArgs obj )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( MonthlySuffixTitleCase );

            if ( !monthlySuffixTextValidateField.IsValid )
            {
                int messageBoxResult = MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Monthly Suffix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z].\nDefault is \"monthly\".", 1, "Fix It Myself", "Use Default Value" );
                if ( messageBoxResult == 1 )
                {
                    monthlySuffixTextValidateField.Text = "monthly";
                }
                else
                {
                    monthlySuffixTextValidateField.SetFocus( );
                    return;
                }
            }

            string suffix = monthlySuffixTextValidateField.Text.ToString( )!;
            if ( SelectedTemplateItem.ViewSettings.Formatting.MonthlySuffix != suffix )
            {
                _modifiedProperties.Add( MonthlySuffixTitleCase );
            }

            UpdatePropertiesFrameViewState( suffix );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void MonthlyTimeTimeFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( MonthlyTimeTitleCase );

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.MonthlyTime != monthlyTimeTimeField.Time.ToTimeOnly( ) )
        {
            _modifiedProperties.Add( MonthlyTimeTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void NewTemplateNameTextValidateFieldOnKeyPress( KeyEventEventArgs e )
    {
        if ( !newTemplateNameTextValidateField.Text.IsEmpty && newTemplateNameTextValidateField.IsValid )
        {
            string newTemplateName = newTemplateNameTextValidateField.Text.ToString( )!;
            addTemplateButton.Enabled = newTemplateName != "default" && !_templates.ContainsKey( newTemplateName );
        }
        else
        {
            addTemplateButton.Enabled = false;
        }
    }

    private void PrefixTextValidateFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( PrefixTitleCase );

        if ( !prefixTextValidateField.IsValid )
        {
            MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Prefix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z]" );
            prefixTextValidateField.SetFocus( );
            return;
        }

        if ( SelectedTemplateItem.ViewSettings.Formatting.Prefix != prefixTextValidateField.Text.ToString( ) )
        {
            _modifiedProperties.Add( PrefixTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void ResetCurrentButtonOnClicked( )
    {
        DisableEventHandlers( );
        _modifiedProperties.Clear( );
        SelectedTemplateItem.ViewSettings = SelectedTemplateItem.BaseSettings with { };
        SetFieldsForSelectedItem( );
        UpdateTemplatePropertiesButtonStates( );
        UpdateTemplateListButtonStates( );
        EnableEventHandlers( );
    }

    private void SetFieldsForSelectedItem( )
    {
        TemplateConfigurationListItem item = SelectedTemplateItem;
        componentSeparatorValidateField.Text = ustring.Make( item.ViewSettings.Formatting.ComponentSeparator );
        prefixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.Prefix );
        frequentSuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.FrequentSuffix );
        hourlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.HourlySuffix );
        dailySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.DailySuffix );
        weeklySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.WeeklySuffix );
        monthlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.MonthlySuffix );
        yearlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.YearlySuffix );
        timestampFormatTextField.Text = ustring.Make( item.ViewSettings.Formatting.TimestampFormatString );
        frequentPeriodRadioGroup.SelectedItem = TemplateConfigurationFrequentPeriodOptions.IndexOf( item.ViewSettings.SnapshotTiming.FrequentPeriod );
        hourlyMinuteTextValidateField.Text = item.ViewSettings.SnapshotTiming.HourlyMinute.ToString( "D2" );
        dailyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.DailyTime.ToTimeSpan( );
        weeklyDayComboBox.SelectedItem = (int)item.ViewSettings.SnapshotTiming.WeeklyDay;
        weeklyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.WeeklyTime.ToTimeSpan( );
        monthlyDayTextValidateField.Text = item.ViewSettings.SnapshotTiming.MonthlyDay.ToString( );
        monthlyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.MonthlyTime.ToTimeSpan( );
        yearlyDayTextValidateField.Text = item.ViewSettings.SnapshotTiming.YearlyDay.ToString( );
        yearlyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.YearlyTime.ToTimeSpan( );
        yearlyMonthComboBox.SelectedItem = item.ViewSettings.SnapshotTiming.YearlyMonth - 1;
    }

    private void SetInitialButtonState( )
    {
        deleteTemplateButton.Enabled = false;
        resetCurrentButton.Enabled = false;
        applyCurrentButton.Enabled = false;
    }

    private void SetReadOnlyFields( )
    {
        exampleTextField.ReadOnly = true;
        exampleTextField.CanFocus = false;
        exampleTextField.TabStop = false;
    }

    private static void SetTabStops( )
    {
        //TODO: Set up tab order
    }

    private void SetValidateOnInputForAllTextValidateFields( )
    {
        _templateConfigurationTextValidateFieldList.ForEach( static item => ( (TextRegexProvider)item.Field.Provider ).ValidateOnInput = item.ValidateOnInput );
        ( (TextRegexProvider)newTemplateNameTextValidateField.Provider ).ValidateOnInput = false;
    }

    private static void ShowCommitConfirmationDialog( )
    {
        int dialogResult = MessageBox.Query( "Commit Templates?", "This will commit all currently applied changes to templates to memory.\n\nNOTE: THIS WILL NOT SAVE YOUR CHANGES TO DISK!\n\nYou must use the Save option in the File menu to save changes to disk.\n\nChanges to templates that have not been applied will not be committed.", 0, "Cancel", "Commit" );
        switch ( dialogResult )
        {
            case 0:
                Logger.Debug( "User canceled template commit dialog" );
                return;
            case 1:
                Logger.Debug( "User confirmed commit dialog" );
                CommitModifiedTemplates( );
                break;
        }
    }

    private void TemplateListViewOnSelectedItemChanged( ListViewItemEventArgs e )
    {
        DisableEventHandlers( );

        _modifiedProperties.Clear( );
        templateListView.EnsureSelectedItemVisible( );
        SetFieldsForSelectedItem( );
        UpdatePropertiesFrameViewState( );
        UpdateTemplateListButtonStates( );

        EnableEventHandlers( );
    }

    private void TimestampFormatTextFieldOnLeave( FocusEventArgs e )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( TimestampFormatTitleCase );

            if ( timestampFormatTextField.Text.IsEmpty )
            {
                int messageBoxResult = MessageBox.ErrorQuery( "Invalid Field Value", "Timestamp format string cannot be empty or whitespace-only", "Fix It Myself", "Use Default" );
                if ( messageBoxResult == 1 )
                {
                    timestampFormatTextField.Text = FormattingSettings.GetDefault( ).TimestampFormatString;
                }
                else
                {
                    timestampFormatTextField.SetFocus( );
                    return;
                }
            }

            string timestampFormatString = timestampFormatTextField.Text.ToString( )!;
            DateTimeOffset now = DateTimeOffset.Now;
            char[] buffer = new char[128];
            if ( !now.TryFormat( buffer.AsSpan( ), out _, timestampFormatString.AsSpan( ) ) )
            {
                Logger.Warn( "Invalid timestamp format string specified." );
                const string formatDocumentationUrl = "https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings";
                MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, $"Timestamp Format String is not a valid format string. See the following documentation for valid format strings:\n{formatDocumentationUrl}" );
                return;
            }

            if ( SelectedTemplateItem.ViewSettings.Formatting.TimestampFormatString != timestampFormatTextField.Text.ToString( ) )
            {
                _modifiedProperties.Add( TimestampFormatTitleCase );
            }

            UpdatePropertiesFrameViewState( );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void UpdateExampleText( string periodString )
    {
        if ( !IsEveryPropertyTextValidateFieldValid )
        {
            return;
        }

        string timestampFormatString = timestampFormatTextField.Text.ToString( )!;
        DateTimeOffset now = DateTimeOffset.Now;
        char[] buffer = new char[128];
        if ( !now.TryFormat( buffer.AsSpan( ), out _, timestampFormatString.AsSpan( ) ) )
        {
            Logger.Warn( "Invalid timestamp format string specified." );
            return;
        }

        string prefixString = prefixTextValidateField.Text.ToString( )!;
        string componentSeparator = componentSeparatorValidateField.Text.ToString( )!;
        exampleTextField.Text = $"{prefixString}{componentSeparator}{DateTimeOffset.Now.ToString( timestampFormatString )}{componentSeparator}{periodString}";
    }

    /// <summary>
    ///     Updates buttons and other input-dependent fields
    /// </summary>
    /// <remarks>Intended to be run after an input field value has changed</remarks>
    private void UpdatePropertiesFrameViewState( string? periodString = null )
    {
        UpdateTemplatePropertiesButtonStates( );
        string dailySuffixString = dailySuffixTextValidateField.IsValid && dailySuffixTextValidateField.Text.Length > 0 ? dailySuffixTextValidateField.Text.ToString( )! : "invalid";
        UpdateExampleText( periodString ?? dailySuffixString );
    }

    private void UpdateTemplateListButtonStates( )
    {
        commitAllButton.Enabled = IsAnyTemplateModified;
        deleteTemplateButton.Enabled = templateListView.SelectedItem >= 0 && !IsSelectedTemplateInUse && SelectedTemplateItem.TemplateName != "default";
        addTemplateButton.Enabled = newTemplateNameTextValidateField.IsValid && newTemplateNameTextValidateField.Text.ToString( )! != "default";
    }

    private void UpdateTemplatePropertiesButtonStates( )
    {
        resetCurrentButton.Enabled = _modifiedProperties.Count > 0;
        applyCurrentButton.Enabled = _modifiedProperties.Count > 0;
    }

    private void WeeklyDayComboBoxOnSelectedItemChanged( ListViewItemEventArgs e )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( WeeklyDayTitleCase );
            if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.WeeklyDay != (DayOfWeek)e.Item )
            {
                _modifiedProperties.Add( WeeklyDayTitleCase );
            }
        }
        finally
        {
            EnableEventHandlers( );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void WeeklySuffixTextValidateFieldOnLeave( FocusEventArgs obj )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( WeeklySuffixTitleCase );

            if ( !weeklySuffixTextValidateField.IsValid )
            {
                int messageBoxResult = MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Weekly Suffix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z].\nDefault is \"weekly\".", 1, "Fix It Myself", "Use Default Value" );
                if ( messageBoxResult == 1 )
                {
                    weeklySuffixTextValidateField.Text = "weekly";
                }
                else
                {
                    weeklySuffixTextValidateField.SetFocus( );
                    return;
                }
            }

            string suffix = weeklySuffixTextValidateField.Text.ToString( )!;
            if ( SelectedTemplateItem.ViewSettings.Formatting.WeeklySuffix != suffix )
            {
                _modifiedProperties.Add( WeeklySuffixTitleCase );
            }

            UpdatePropertiesFrameViewState( suffix );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void WeeklyTimeTimeFieldOnLeave( FocusEventArgs obj )
    {
        _modifiedProperties.Remove( WeeklyTimeTitleCase );

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.WeeklyTime != weeklyTimeTimeField.Time.ToTimeOnly( ) )
        {
            _modifiedProperties.Add( WeeklyTimeTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void YearlyDayTextValidateFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( YearlyDayTitleCase );

        if ( !yearlyDayTextValidateField.IsValid )
        {
            MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, $"Value entered for {YearlyDayTitleCase} is invalid. Must be a 1 or 2 digit positive numeric value" );
            yearlyDayTextValidateField.SetFocus( );
            return;
        }

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.YearlyDay != int.Parse( yearlyDayTextValidateField.Text.ToString( )! ) )
        {
            _modifiedProperties.Add( YearlyDayTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void YearlyMonthComboBoxOnSelectedItemChanged( ListViewItemEventArgs e )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( YearlyMonthTitleCase );
            if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.YearlyMonth != e.Item + 1 )
            {
                _modifiedProperties.Add( YearlyMonthTitleCase );
            }
        }
        finally
        {
            EnableEventHandlers( );
        }

        UpdatePropertiesFrameViewState( );
    }

    private void YearlySuffixTextValidateFieldOnLeave( FocusEventArgs obj )
    {
        try
        {
            DisableEventHandlers( );
            _modifiedProperties.Remove( YearlySuffixTitleCase );

            if ( !yearlySuffixTextValidateField.IsValid )
            {
                int messageBoxResult = MessageBox.ErrorQuery( InvalidFieldValueDialogTitle, "Value entered for Yearly Suffix is invalid. Must be 1-12 characters from the following set: [0-9a-zA-Z].\nDefault is \"yearly\".", 1, "Fix It Myself", "Use Default Value" );
                if ( messageBoxResult == 1 )
                {
                    yearlySuffixTextValidateField.Text = "yearly";
                }
                else
                {
                    yearlySuffixTextValidateField.SetFocus( );
                    return;
                }
            }

            string suffix = yearlySuffixTextValidateField.Text.ToString( )!;
            if ( SelectedTemplateItem.ViewSettings.Formatting.YearlySuffix != suffix )
            {
                _modifiedProperties.Add( YearlySuffixTitleCase );
            }

            UpdatePropertiesFrameViewState( suffix );
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void YearlyTimeTimeFieldOnLeave( FocusEventArgs e )
    {
        _modifiedProperties.Remove( YearlyTimeTitleCase );

        if ( SelectedTemplateItem.ViewSettings.SnapshotTiming.YearlyTime != yearlyTimeTimeField.Time.ToTimeOnly( ) )
        {
            _modifiedProperties.Add( YearlyTimeTitleCase );
        }

        UpdatePropertiesFrameViewState( );
    }

    private sealed record TextValidateFieldSettings( TextValidateField Field, bool ValidateOnInput );
}
