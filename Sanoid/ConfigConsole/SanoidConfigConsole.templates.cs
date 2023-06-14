// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NStack;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;

namespace Sanoid.ConfigConsole;

/// <summary>
///     The type containing the text user interface for Sanoid.net configuration
/// </summary>
public partial class SanoidConfigConsole
{
    private record TextValidateFieldSettings( TextValidateField Field, bool ValidateOnInput );
    private bool _templateConfigurationEventsEnabled;
    private bool _templateConfigurationTemplatesAddedOrRemoved;
    private readonly List<TextValidateFieldSettings> _templateConfigurationTextValidateFieldList = new( );
    private readonly List<TemplateConfigurationListItem> _templateListItems = ConfigConsole.Settings.Templates.Select( kvp => new TemplateConfigurationListItem( kvp.Key, kvp.Value with { }, kvp.Value with { } ) ).ToList( );
    private bool IsAnyTemplateModified => _templateListItems.Any( t => t.IsModified );
    private bool IsEveryPropertyTextValidateFieldValid => _templateConfigurationTextValidateFieldList.TrueForAll( tvf => tvf.Field.IsValid );
    private bool IsSelectedTemplateInUse => _baseDatasets.Any( kvp => kvp.Value.Template.Value == SelectedTemplateItem.TemplateName );

    private TemplateConfigurationListItem SelectedTemplateItem => _templateListItems[ templateConfigurationTemplateListView.SelectedItem ];
    private static readonly List<string> DayNamesLongAndAbbreviated = DateTimeFormatInfo.CurrentInfo.GetLongAndAbbreviatedDayNames( );
    private static readonly List<string> MonthNamesLongAndAbbreviated = DateTimeFormatInfo.CurrentInfo.GetMonthNames( );
    private static readonly List<int> TemplateConfigurationFrequentPeriodOptions = new( ) { 5, 10, 15, 20, 30 };
    private static ConcurrentDictionary<string, TemplateSettings> Templates = new( );

    private void InitializeTemplateEditorView( )
    {
        DisableTemplateConfigurationTabEventHandlers( );
        TemplateConfigurationHideTemplateConfigurationPropertiesFrame( );
        templateConfigurationTemplateListView.SetSource( _templateListItems );
        Templates.Clear( );
        Templates = new( ConfigConsole.Settings.Templates );
        templateConfigurationPropertiesSnapshotTimingFrame.CanFocus = false;
        templateConfigurationTemplatePropertiesFrame.CanFocus = false;
        templateConfigurationTemplateListFrame.CanFocus = false;
        templateConfigurationSnapshotNamingFrame.CanFocus = false;
        templateEditorWindow.CanFocus = false;
        templateConfigurationDeleteTemplateButton.Enabled = false;
        templateConfigurationResetCurrentButton.Enabled = false;
        templateConfigurationApplyCurrentButton.Enabled = false;
        TemplateConfigurationInitializeTemplatePropertiesTextValidateFieldList( );
        TemplateConfigurationSetValidateOnInputForAllTextValidateFields( );
        TemplateConfigurationUpdateTemplateListButtonStates( );
        TemplateConfigurationUpdateTemplatePropertiesButtonStates( );
        EnableTemplateConfigurationTabEventHandlers( );
    }

    private void TemplateConfigurationInitializeTemplatePropertiesTextValidateFieldList( )
    {
        _templateConfigurationTextValidateFieldList.Clear( );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingComponentSeparatorValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingPrefixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingFrequentSuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingHourlySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingDailySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingWeeklySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingMonthlySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesNamingYearlySuffixTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesTimingHourlyMinuteTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesTimingWeeklyDayTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesTimingMonthlyDayTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesTimingYearlyMonthTextValidateField, true ) );
        _templateConfigurationTextValidateFieldList.Add( new( templateConfigurationPropertiesTimingYearlyDayTextValidateField, true ) );
    }

    private void TemplateConfigurationSetValidateOnInputForAllTextValidateFields( )
    {
        _templateConfigurationTextValidateFieldList.ForEach( item => ( (TextRegexProvider)item.Field.Provider ).ValidateOnInput = item.ValidateOnInput );
        ( (TextRegexProvider)templateConfigurationNewTemplateNameTextValidateField.Provider ).ValidateOnInput = false;
    }

    private void EnableTemplateConfigurationTabEventHandlers( )
    {
        if ( _templateConfigurationEventsEnabled )
        {
            return;
        }

        templateConfigurationTemplateListView.SelectedItemChanged += TemplateConfigurationTemplateListViewOnSelectedItemChanged;
        templateConfigurationAddTemplateButton.Clicked += TemplateConfigurationAddTemplateButtonOnClicked;
        templateConfigurationDeleteTemplateButton.Clicked += TemplateConfigurationDeleteTemplateButtonOnClicked;
        templateConfigurationApplyCurrentButton.Clicked+= TemplateConfigurationApplyCurrentButtonOnClicked;
        templateConfigurationNewTemplateNameTextValidateField.KeyPress += TemplateConfigurationNewTemplateNameTextValidateFieldOnKeyPress;
        templateConfigurationSaveAllButton.Clicked += TemplateSettingsSaveAllButtonOnClicked;
        templateConfigurationResetCurrentButton.Clicked += TemplateConfigurationResetCurrentButtonOnClicked;
        templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Leave += TemplateConfigurationPropertiesTimingHourlyMinuteTextValidateFieldOnLeave;
        templateConfigurationPropertiesTimingDailyTimeTimeField.Leave += TemplateConfigurationPropertiesTimingDailyTimeTimeFieldOnLeave;
        templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.KeyPress += TemplateConfigurationPropertiesTimingHourlyMinuteTextValidateFieldOnKeyPress;
        templateConfigurationPropertiesTimingDailyTimeTimeField.KeyPress += TemplateConfigurationPropertiesTimingDailyTimeTimeFieldOnKeyPress;
        _templateConfigurationEventsEnabled = true;
    }

    private void TemplateConfigurationApplyCurrentButtonOnClicked( )
    {
        if ( !IsEveryPropertyTextValidateFieldValid )
        {
            SelectedTemplateItem.ViewSettings.SnapshotTiming = SelectedTemplateItem.ViewSettings.SnapshotTiming with
            {
                DailyTime = TimeOnly.FromTimeSpan( templateConfigurationPropertiesTimingDailyTimeTimeField.Time ),
                WeeklyTime = TimeOnly.FromTimeSpan( templateConfigurationPropertiesTimingWeeklyTimeTimeField.Time ),
                MonthlyTime = TimeOnly.FromTimeSpan( templateConfigurationPropertiesTimingMonthlyTimeTimeField.Time ),
                YearlyTime = TimeOnly.FromTimeSpan( templateConfigurationPropertiesTimingYearlyTimeTimeField.Time )
            };
        }
    }

    private void TemplateConfigurationPropertiesTimingDailyTimeTimeFieldOnKeyPress( KeyEventEventArgs args )
    {
        bool isTimeValueDifferent = SelectedTemplateItem.ViewSettings.SnapshotTiming.DailyTime != templateConfigurationPropertiesTimingDailyTimeTimeField.Time.ToTimeOnly( );
        templateConfigurationApplyCurrentButton.Enabled = isTimeValueDifferent && IsEveryPropertyTextValidateFieldValid;
    }

    private void TemplateConfigurationPropertiesTimingHourlyMinuteTextValidateFieldOnKeyPress( KeyEventEventArgs args )
    {
        bool fieldIsValid = templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.IsValid;
        bool isMinuteValueDifferent = SelectedTemplateItem.ViewSettings.SnapshotTiming.HourlyMinute != templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Text.ToInt32( int.MinValue );
        templateConfigurationApplyCurrentButton.Enabled = fieldIsValid && isMinuteValueDifferent && IsEveryPropertyTextValidateFieldValid;
    }

    private void TemplateConfigurationNewTemplateNameTextValidateFieldOnKeyPress( KeyEventEventArgs args )
    {
        if ( !templateConfigurationNewTemplateNameTextValidateField.Text.IsEmpty && templateConfigurationNewTemplateNameTextValidateField.IsValid )
        {
            string newTemplateName = templateConfigurationNewTemplateNameTextValidateField.Text.ToString( )!;
            templateConfigurationAddTemplateButton.Enabled = newTemplateName != "default" && !Templates.ContainsKey( newTemplateName );
        }
        else
        {
            templateConfigurationAddTemplateButton.Enabled = false;
        }
    }

    /// <exception cref="ApplicationException">
    ///     If removal of the selected template from <see cref="Templates" /> fails.<br />
    ///     Should be treated as fatal by any consumers
    /// </exception>
    private void TemplateConfigurationDeleteTemplateButtonOnClicked( )
    {
        try
        {
            DisableTemplateConfigurationTabEventHandlers( );
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
            int indexToRemove = templateConfigurationTemplateListView.SelectedItem;
            templateConfigurationTemplateListView.SelectedItem -= 1;
            templateConfigurationTemplateListView.EnsureSelectedItemVisible( );
            _templateListItems.RemoveAt( indexToRemove );
            if ( !Templates.TryRemove( templateName, out _ ) )
            {
                // The application state is inconsistent if this happens, and it isn't safe to continue
                string errorMessage = $"Failed to remove template {templateName} from UI dictionary";
                TemplateRemovalException ex = new( errorMessage );
                Logger.Fatal( ex, errorMessage );
                throw ex;
            }

            _templateConfigurationTemplatesAddedOrRemoved = true;
        }
        catch ( ApplicationException )
        {
            // If we threw the KeyNotFoundException above, we need to exit.
            // Also, re-throw so we can deal with it in the caller as well.
            Application.Top.RequestStop( );
            throw;
        }
        TemplateConfigurationUpdateTemplateListButtonStates( );
        EnableTemplateConfigurationTabEventHandlers( );
    }

    private void TemplateConfigurationAddTemplateButtonOnClicked( )
    {
        if ( !Templates.TryGetValue( "default", out _ ) )
        {
            string errorMessage = "'default' template does not exist. Not creating new template.";
            Logger.Error( errorMessage );
            MessageBox.ErrorQuery( "Error Adding Template", errorMessage, 0, "OK" );
            return;
        }

        if ( !templateConfigurationNewTemplateNameTextValidateField.IsValid )
        {
            string errorMessage = "New template name not valid. Not creating new template.";
            Logger.Error( errorMessage );
            MessageBox.ErrorQuery( "Error Adding Template", errorMessage, 0, "OK" );
            return;
        }

        string? newTemplateName = templateConfigurationNewTemplateNameTextValidateField.Text.ToString( );
        if ( Templates.ContainsKey( newTemplateName! ) )
        {
            string errorMessage = $"A template named {newTemplateName} already exists.";
            Logger.Error( errorMessage );
            MessageBox.ErrorQuery( "Error Adding Template", errorMessage, 0, "OK" );
            return;
        }

        Templates[ newTemplateName! ] = SelectedTemplateItem.ViewSettings with { };
        _templateListItems.Add( new( newTemplateName!, SelectedTemplateItem.ViewSettings with { }, SelectedTemplateItem.ViewSettings with { } ) );
        _templateConfigurationTemplatesAddedOrRemoved = true;
        TemplateConfigurationUpdateTemplateListButtonStates( );
        TemplateConfigurationUpdateTemplatePropertiesButtonStates( );
    }

    private void TemplateConfigurationPropertiesTimingDailyTimeTimeFieldOnLeave( FocusEventArgs args )
    {
        TemplateConfigurationUpdateTemplatePropertiesButtonStates( );
    }

    private void TemplateConfigurationPropertiesTimingHourlyMinuteTextValidateFieldOnLeave( FocusEventArgs args )
    {
        if ( templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.IsValid )
        {
            if ( !int.TryParse( templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Text.ToString( ), out int hourlyMinute ) && hourlyMinute is < 0 or > 59 )
            {
                const string errorMessage = "The value entered for Hourly Minute is invalid. Field will be reset to previous value.";
                Logger.Warn( errorMessage );
                MessageBox.ErrorQuery( "Invalid Hourly Minute Value", errorMessage, "OK" );
                templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Text = SelectedTemplateItem.ViewSettings.SnapshotTiming.HourlyMinute.ToString( "D2" );
                return;
            }

            SelectedTemplateItem.ViewSettings.SnapshotTiming = SelectedTemplateItem.ViewSettings.SnapshotTiming with { HourlyMinute = hourlyMinute };
        }

        TemplateConfigurationUpdateTemplateListButtonStates( );
    }

    private void TemplateConfigurationResetCurrentButtonOnClicked( )
    {
        DisableTemplateConfigurationTabEventHandlers( );
        SelectedTemplateItem.ViewSettings = SelectedTemplateItem.BaseSettings with { };
        TemplateConfigurationSetFieldsForSelectedItem( );
        TemplateConfigurationUpdateTemplatePropertiesButtonStates( );
        TemplateConfigurationUpdateTemplateListButtonStates( );
        EnableTemplateConfigurationTabEventHandlers( );
    }

    private void TemplateConfigurationUpdateTemplateListButtonStates( )
    {
        templateConfigurationSaveAllButton.Enabled = ( _templateConfigurationTemplatesAddedOrRemoved || IsAnyTemplateModified ) && IsEveryPropertyTextValidateFieldValid;
        templateConfigurationDeleteTemplateButton.Enabled = templateConfigurationTemplateListView.SelectedItem >= 0 && !IsSelectedTemplateInUse;
        templateConfigurationAddTemplateButton.Enabled = templateConfigurationNewTemplateNameTextValidateField.IsValid;
    }

    private void TemplateConfigurationUpdateTemplatePropertiesButtonStates( )
    {
        templateConfigurationResetCurrentButton.Enabled = SelectedTemplateItem.IsModified;
        templateConfigurationApplyCurrentButton.Enabled = SelectedTemplateItem.IsModified;
    }

    private void TemplateConfigurationHideTemplateConfigurationPropertiesFrame( )
    {
        templateConfigurationTemplatePropertiesFrame.Visible = false;
    }

    private void TemplateConfigurationShowTemplateConfigurationPropertiesFrame( )
    {
        templateConfigurationTemplatePropertiesFrame.Visible = true;
    }

    private void TemplateConfigurationTemplateListViewOnSelectedItemChanged( ListViewItemEventArgs args )
    {
        DisableTemplateConfigurationTabEventHandlers( );

        templateConfigurationTemplateListView.EnsureSelectedItemVisible( );

        TemplateConfigurationSetFieldsForSelectedItem( );

        TemplateConfigurationUpdateTemplateListButtonStates( );

        EnableTemplateConfigurationTabEventHandlers( );
    }

    private void TemplateConfigurationSetFieldsForSelectedItem( )
    {
        if ( !templateConfigurationTemplatePropertiesFrame.Visible )
        {
            TemplateConfigurationShowTemplateConfigurationPropertiesFrame( );
        }

        TemplateConfigurationListItem item = SelectedTemplateItem;
        templateConfigurationPropertiesNamingComponentSeparatorValidateField.Text = ustring.Make( item.ViewSettings.Formatting.ComponentSeparator );
        templateConfigurationPropertiesNamingPrefixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.Prefix );
        templateConfigurationPropertiesNamingFrequentSuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.FrequentSuffix );
        templateConfigurationPropertiesNamingHourlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.HourlySuffix );
        templateConfigurationPropertiesNamingDailySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.DailySuffix );
        templateConfigurationPropertiesNamingWeeklySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.WeeklySuffix );
        templateConfigurationPropertiesNamingMonthlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.MonthlySuffix );
        templateConfigurationPropertiesNamingYearlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.YearlySuffix );
        templateConfigurationPropertiesNamingTimestampFormatTextField.Text = ustring.Make( item.ViewSettings.Formatting.TimestampFormatString );
        templateConfigurationPropertiesTimingFrequentPeriodRadioGroup.SelectedItem = TemplateConfigurationFrequentPeriodOptions.IndexOf( item.ViewSettings.SnapshotTiming.FrequentPeriod );
        templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Text = item.ViewSettings.SnapshotTiming.HourlyMinute.ToString( "D2" );
        templateConfigurationPropertiesTimingDailyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.DailyTime.ToTimeSpan( );
        templateConfigurationPropertiesTimingWeeklyDayTextValidateField.Text = DateTimeFormatInfo.CurrentInfo.GetDayName( item.ViewSettings.SnapshotTiming.WeeklyDay );
        templateConfigurationPropertiesTimingWeeklyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.WeeklyTime.ToTimeSpan( );
        templateConfigurationPropertiesTimingMonthlyDayTextValidateField.Text = item.ViewSettings.SnapshotTiming.MonthlyDay.ToString( );
        templateConfigurationPropertiesTimingMonthlyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.MonthlyTime.ToTimeSpan( );
        templateConfigurationPropertiesTimingYearlyMonthTextValidateField.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName( item.ViewSettings.SnapshotTiming.YearlyMonth );
        templateConfigurationPropertiesTimingYearlyDayTextValidateField.Text = item.ViewSettings.SnapshotTiming.YearlyDay.ToString( );
        templateConfigurationPropertiesTimingYearlyTimeTimeField.Time = item.ViewSettings.SnapshotTiming.YearlyTime.ToTimeSpan( );
    }

    private void TemplateSettingsSaveAllButtonOnClicked( )
    {
        TemplateConfigurationValidator validator = new( );

        if ( !validator.ValidateFieldValues( this ) )
        {
            return;
        }

        SelectedTemplateItem.ViewSettings = SelectedTemplateItem.ViewSettings with
        {
            Formatting = new( )
            {
                ComponentSeparator = validator.NamingComponentSeparator!,
                Prefix = validator.NamingPrefix!,
                TimestampFormatString = validator.NamingTimestampFormatString!,
                FrequentSuffix = validator.NamingFrequentSuffix!,
                HourlySuffix = validator.NamingHourlySuffix!,
                DailySuffix = validator.NamingDailySuffix!,
                WeeklySuffix = validator.NamingWeeklySuffix!,
                MonthlySuffix = validator.NamingMonthlySuffix!,
                YearlySuffix = validator.NamingYearlySuffix!
            },
            SnapshotTiming = SelectedTemplateItem.ViewSettings.SnapshotTiming with
            {
                FrequentPeriod = validator.TimingFrequentPeriod!.Value,
                HourlyMinute = validator.TimingHourlyMinute!.Value,
                DailyTime = validator.TimingDailyTime,
                WeeklyDay = validator.TimingWeeklyDay!.Value,
                WeeklyTime = validator.TimingWeeklyTime,
                MonthlyDay = validator.TimingMonthlyDay!.Value,
                MonthlyTime = validator.TimingMonthlyTime,
                YearlyMonth = validator.TimingYearlyMonth!.Value,
                YearlyDay = validator.TimingYearlyDay!.Value,
                YearlyTime = validator.TimingYearlyTime
            }
        };

        if ( SelectedTemplateItem.IsModified )
        {
            Templates[ SelectedTemplateItem.TemplateName ] = SelectedTemplateItem.ViewSettings;
            TemplateConfigurationShowSaveDialog( );
        }

        TemplateConfigurationUpdateTemplateListButtonStates( );
    }

    private void TemplateConfigurationShowSaveDialog( )
    {
        using ( SaveDialog saveDialog = new( "Save Global Configuration", "Select file to save global configuration", new( ) { ".json" } ) )
        {
            saveDialog.AllowsOtherFileTypes = true;
            saveDialog.CanCreateDirectories = true;
            saveDialog.Modal = true;
            Application.Run( saveDialog );
            if ( saveDialog.Canceled )
            {
                return;
            }

            if ( saveDialog.FileName.IsEmpty )
            {
                return;
            }

            SanoidSettings settings = ConfigConsole.Settings with
            {
                Templates = Templates.ToDictionary( kvp => kvp.Key, kvp => kvp.Value )
            };
            SelectedTemplateItem.BaseSettings = SelectedTemplateItem.ViewSettings with { };
            ConfigConsole.Settings = settings with { };

            File.WriteAllText( saveDialog.FileName.ToString( ) ?? throw new InvalidOperationException( "Null string provided for save file name" ), JsonSerializer.Serialize( ConfigConsole.Settings, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
        }

        TemplateConfigurationUpdateTemplateListButtonStates( );
    }

    private void DisableTemplateConfigurationTabEventHandlers( )
    {
        if ( !_templateConfigurationEventsEnabled )
        {
            return;
        }

        templateConfigurationTemplateListView.SelectedItemChanged -= TemplateConfigurationTemplateListViewOnSelectedItemChanged;
        templateConfigurationAddTemplateButton.Clicked -= TemplateConfigurationAddTemplateButtonOnClicked;
        templateConfigurationDeleteTemplateButton.Clicked -= TemplateConfigurationDeleteTemplateButtonOnClicked;
        templateConfigurationNewTemplateNameTextValidateField.KeyPress -= TemplateConfigurationNewTemplateNameTextValidateFieldOnKeyPress;
        templateConfigurationSaveAllButton.Clicked -= TemplateSettingsSaveAllButtonOnClicked;
        templateConfigurationResetCurrentButton.Clicked -= TemplateConfigurationResetCurrentButtonOnClicked;
        templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Leave -= TemplateConfigurationPropertiesTimingHourlyMinuteTextValidateFieldOnLeave;
        templateConfigurationPropertiesTimingDailyTimeTimeField.Leave -= TemplateConfigurationPropertiesTimingDailyTimeTimeFieldOnLeave;
        templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.KeyPress -= TemplateConfigurationPropertiesTimingHourlyMinuteTextValidateFieldOnKeyPress;
        templateConfigurationPropertiesTimingDailyTimeTimeField.KeyPress -= TemplateConfigurationPropertiesTimingDailyTimeTimeFieldOnKeyPress;

        _templateConfigurationEventsEnabled = false;
    }

    private class TemplateConfigurationValidator
    {
        private SanoidConfigConsole? ConfigConsole { get; set; }
        public string? NamingComponentSeparator { get; private set; }
        public string? NamingDailySuffix { get; private set; }
        public string? NamingFrequentSuffix { get; private set; }
        public string? NamingHourlySuffix { get; private set; }
        public string? NamingMonthlySuffix { get; private set; }
        public string? NamingPrefix { get; private set; }
        public string? NamingTimestampFormatString { get; private set; }
        public string? NamingWeeklySuffix { get; private set; }
        public string? NamingYearlySuffix { get; private set; }
        public TimeOnly TimingDailyTime { get; private set; }
        public int? TimingFrequentPeriod { get; private set; }
        public int? TimingHourlyMinute { get; private set; }
        public int? TimingMonthlyDay { get; private set; }
        public TimeOnly TimingMonthlyTime { get; private set; }
        public DayOfWeek? TimingWeeklyDay { get; private set; }
        public TimeOnly TimingWeeklyTime { get; private set; }
        public int? TimingYearlyDay { get; private set; }
        public int? TimingYearlyMonth { get; private set; }
        public TimeOnly TimingYearlyTime { get; private set; }

        public bool ValidateFieldValues( SanoidConfigConsole configConsole )
        {
            ConfigConsole = configConsole;
            if ( ConfigConsole is null )
            {
                return false;
            }

            bool isValid = true;
            string templateName = ConfigConsole.SelectedTemplateItem.TemplateName;

            // Naming fields
            NamingComponentSeparator = ConfigConsole.templateConfigurationPropertiesNamingComponentSeparatorValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingComponentSeparatorValidateField?.Text?.ToString( ) : null;
            NamingPrefix = ConfigConsole.templateConfigurationPropertiesNamingPrefixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingPrefixTextValidateField?.Text?.ToString( ) : null;
            NamingTimestampFormatString = ConfigConsole.templateConfigurationPropertiesNamingTimestampFormatTextField?.Text?.ToString( );
            NamingFrequentSuffix = ConfigConsole.templateConfigurationPropertiesNamingFrequentSuffixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingFrequentSuffixTextValidateField?.Text?.ToString( ) : null;
            NamingHourlySuffix = ConfigConsole.templateConfigurationPropertiesNamingHourlySuffixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingHourlySuffixTextValidateField?.Text?.ToString( ) : null;
            NamingDailySuffix = ConfigConsole.templateConfigurationPropertiesNamingDailySuffixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingDailySuffixTextValidateField?.Text?.ToString( ) : null;
            NamingWeeklySuffix = ConfigConsole.templateConfigurationPropertiesNamingWeeklySuffixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingWeeklySuffixTextValidateField?.Text?.ToString( ) : null;
            NamingMonthlySuffix = ConfigConsole.templateConfigurationPropertiesNamingMonthlySuffixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingMonthlySuffixTextValidateField?.Text?.ToString( ) : null;
            NamingYearlySuffix = ConfigConsole.templateConfigurationPropertiesNamingYearlySuffixTextValidateField.IsValid ? ConfigConsole.templateConfigurationPropertiesNamingYearlySuffixTextValidateField?.Text?.ToString( ) : null;
            if ( string.IsNullOrWhiteSpace( NamingComponentSeparator ) )
            {
                Logger.Warn( "Snapshot template component separator value {0} for template {1} is invalid", NamingComponentSeparator, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingPrefixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingPrefix ) )
            {
                Logger.Warn( "Snapshot template prefix value {0} for template {1} is invalid", NamingPrefix, templateName );
                isValid = false;
            }

            if ( string.IsNullOrWhiteSpace( NamingTimestampFormatString ) )
            {
                Logger.Warn( "Snapshot template timestamp format string value {0} for template {1} is invalid", NamingTimestampFormatString, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingFrequentSuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingFrequentSuffix ) )
            {
                Logger.Warn( "Snapshot template frequent suffix value {0} for template {1} is invalid", NamingFrequentSuffix, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingHourlySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingHourlySuffix ) )
            {
                Logger.Warn( "Snapshot template hourly suffix value {0} for template {1} is invalid", NamingHourlySuffix, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingDailySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingDailySuffix ) )
            {
                Logger.Warn( "Snapshot template daily suffix value {0} for template {1} is invalid", NamingDailySuffix, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingWeeklySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingWeeklySuffix ) )
            {
                Logger.Warn( "Snapshot template weekly suffix value {0} for template {1} is invalid", NamingWeeklySuffix, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingMonthlySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingMonthlySuffix ) )
            {
                Logger.Warn( "Snapshot template monthly suffix value {0} for template {1} is invalid", NamingMonthlySuffix, templateName );
                isValid = false;
            }

            if ( !ConfigConsole.templateConfigurationPropertiesNamingYearlySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingYearlySuffix ) )
            {
                Logger.Warn( "Snapshot template yearly suffix value {0} for template {1} is invalid", NamingYearlySuffix, templateName );
                isValid = false;
            }

            // Timing fields
            string? weeklyDayString = ConfigConsole.templateConfigurationPropertiesTimingWeeklyDayTextValidateField?.Text?.ToString( );
            int findIndex = DayNamesLongAndAbbreviated.FindIndex( m => m.ToLowerInvariant( ) == weeklyDayString?.ToLowerInvariant( ) );
            int index = findIndex % 7;
            DayOfWeek? dayOfWeek = (DayOfWeek)index;
            TimingFrequentPeriod = TemplateConfigurationFrequentPeriodOptions[ ConfigConsole.templateConfigurationPropertiesTimingFrequentPeriodRadioGroup.SelectedItem ];
            TimingHourlyMinute = ConfigConsole.templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Text.ToNullableInt32( );
            TimingDailyTime = TimeOnly.FromTimeSpan( ConfigConsole.templateConfigurationPropertiesTimingDailyTimeTimeField.Time );
            TimingWeeklyDay = dayOfWeek;
            TimingWeeklyTime = TimeOnly.FromTimeSpan( ConfigConsole.templateConfigurationPropertiesTimingWeeklyTimeTimeField.Time );
            ustring monthlyDayString = ConfigConsole.templateConfigurationPropertiesTimingMonthlyDayTextValidateField.Text;
            TimingMonthlyDay = monthlyDayString?.ToNullableInt32( );
            TimingMonthlyTime = TimeOnly.FromTimeSpan( ConfigConsole.templateConfigurationPropertiesTimingMonthlyTimeTimeField.Time );
            string? enteredYearlyMonthString = ConfigConsole.templateConfigurationPropertiesTimingYearlyMonthTextValidateField?.Text?.ToString( );
            if ( int.TryParse( enteredYearlyMonthString, out int enteredYearlyMonthIntValue ) )
            {
                TimingYearlyMonth = enteredYearlyMonthIntValue;
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( enteredYearlyMonthString ) )
                {
                    int numberOfMonthsInYear = CultureInfo.CurrentCulture.Calendar.GetMonthsInYear( DateTimeOffset.Now.Year );
                    int monthIndex = MonthNamesLongAndAbbreviated.FindIndex( m => m.ToLower( CultureInfo.CurrentCulture ) == enteredYearlyMonthString.ToLower( CultureInfo.CurrentCulture ) );
                    if ( monthIndex >= 0 )
                    {
                        TimingYearlyMonth = ( monthIndex % numberOfMonthsInYear ) + 1;
                    }
                }
            }

            TimingYearlyDay = ConfigConsole.templateConfigurationPropertiesTimingYearlyDayTextValidateField?.Text?.ToNullableInt32( );
            TimingYearlyTime = TimeOnly.FromTimeSpan( ConfigConsole.templateConfigurationPropertiesTimingYearlyTimeTimeField.Time );
            if ( TimingFrequentPeriod is null || !TemplateConfigurationFrequentPeriodOptions.Contains( (int)TimingFrequentPeriod ) )
            {
                Logger.Warn( "Snapshot template frequent period value {0:00} for template {1} is invalid", TimingFrequentPeriod, templateName );
                isValid = false;
            }

            if ( TimingHourlyMinute is null or < 0 or > 59 )
            {
                Logger.Warn( "Snapshot template hourly minute value {0:00} for template {1} is invalid", TimingHourlyMinute, templateName );
                isValid = false;
            }

            if ( TimingWeeklyDay is null )
            {
                Logger.Warn( "Snapshot template weekly day value {0} for template {1} is invalid", weeklyDayString, templateName );
                isValid = false;
            }

            if ( TimingMonthlyDay is null )
            {
                Logger.Warn( "Snapshot template monthly day value {0} for template {1} is invalid", monthlyDayString, templateName );
                isValid = false;
            }

            if ( TimingYearlyMonth is null )
            {
                Logger.Warn( "Snapshot template yearly month value {0} for template {1} is invalid", enteredYearlyMonthString, templateName );
                isValid = false;
            }

            if ( TimingYearlyDay is null )
            {
                Logger.Warn( "Snapshot template yearly day value {0} for template {1} is invalid", TimingYearlyDay, templateName );
                isValid = false;
            }

            return isValid;
        }
    }
}
