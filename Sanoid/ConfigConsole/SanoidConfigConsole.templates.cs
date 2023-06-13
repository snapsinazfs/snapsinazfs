// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Globalization;
using NStack;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;
using Terminal.Gui;

namespace Sanoid.ConfigConsole;

public partial class SanoidConfigConsole
{
    public record TemplateConfigurationTextValidateFieldViewData( string PropertyName );
    private void PopulateTemplatesListViewsWithStandardOptions( )
    {
        DisableTemplateConfigurationTabEventHandlers( );
        templateConfigurationTemplateListView.SetSource( _templateListItems );
        EnableTemplateConfigurationTabEventHandlers( );
    }
    private void EnableTemplateConfigurationTabEventHandlers( )
    {
        if ( _templateConfigurationEventsEnabled )
            return;

        templateConfigurationTemplateListView.SelectedItemChanged += TemplateConfigurationTemplateListViewOnSelectedItemChanged;
        _templateConfigurationEventsEnabled = true;
    }

    private void HideTemplateConfigurationPropertiesFrame( )
    {
        templateConfigurationTemplatePropertiesFrame.Visible = false;
    }
    private void ShowTemplateConfigurationPropertiesFrame( )
    {
        templateConfigurationTemplatePropertiesFrame.Visible = true;
    }

    private void TemplateConfigurationTemplateListViewOnSelectedItemChanged( ListViewItemEventArgs args )
    {
        DisableTemplateConfigurationTabEventHandlers( );
        templateConfigurationTemplateListView.EnsureSelectedItemVisible( );
        TemplateConfigurationListItem item = SelectedTemplateItem;
        if ( !templateConfigurationTemplatePropertiesFrame.Visible )
        {
            ShowTemplateConfigurationPropertiesFrame( );
        }

        templateConfigurationPropertiesNamingComponentSeparatorValidateField.Text = ustring.Make( item.ViewSettings.Formatting.ComponentSeparator );
        templateConfigurationPropertiesNamingPrefixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.Prefix );
        templateConfigurationPropertiesNamingFrequentSuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.FrequentSuffix );
        templateConfigurationPropertiesNamingHourlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.HourlySuffix );
        templateConfigurationPropertiesNamingDailySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.DailySuffix );
        templateConfigurationPropertiesNamingWeeklySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.WeeklySuffix );
        templateConfigurationPropertiesNamingMonthlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.MonthlySuffix );
        templateConfigurationPropertiesNamingYearlySuffixTextValidateField.Text = ustring.Make( item.ViewSettings.Formatting.YearlySuffix );
        templateConfigurationPropertiesNamingTimestampFormatTextField.Text = ustring.Make( item.ViewSettings.Formatting.TimestampFormatString );
        templateConfigurationPropertiesTimingFrequentPeriodRadioGroup.SelectedItem = _templateConfigurationFrequentPeriodOptions.IndexOf( item.ViewSettings.SnapshotTiming.FrequentPeriod );
        templateConfigurationPropertiesTimingHourlyMinuteTextValidateField.Text = item.ViewSettings.SnapshotTiming.HourlyMinute.ToString( );
        templateConfigurationPropertiesTimingDailyTimeTextValidateField.Text = item.ViewSettings.SnapshotTiming.DailyTime.ToString( "HH:mm:ss" );
        templateConfigurationPropertiesTimingWeeklyDayTextValidateField.Text = DateTimeFormatInfo.CurrentInfo.GetDayName( item.ViewSettings.SnapshotTiming.WeeklyDay );
        EnableTemplateConfigurationTabEventHandlers( );
    }

    private void TemplateSettingsSaveSelectedTemplate( )
    {
        List<string> dayNamesLongAndAbbreviated = DateTimeFormatInfo.CurrentInfo.DayNames.Union(DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames).ToList();
        DayOfWeek dayOfWeek = (DayOfWeek)( dayNamesLongAndAbbreviated.FindIndex( m => m.ToLowerInvariant( ) == "mon" ) % 7 );
    }

    private void DisableTemplateConfigurationTabEventHandlers( )
    {
        if ( !_templateConfigurationEventsEnabled )
            return;

        templateConfigurationTemplateListView.SelectedItemChanged -= TemplateConfigurationTemplateListViewOnSelectedItemChanged;

        _templateConfigurationEventsEnabled = false;
    }

    private TemplateConfigurationListItem SelectedTemplateItem => _templateListItems[ templateConfigurationTemplateListView.SelectedItem ];

    private bool _templateConfigurationEventsEnabled;
    private readonly List<TemplateConfigurationListItem> _templateListItems = ConfigConsole.Settings!.Templates.Select( kvp => new TemplateConfigurationListItem( kvp.Key, kvp.Value with{}, kvp.Value with { } ) ).ToList( );
    private readonly List<int> _templateConfigurationFrequentPeriodOptions = new( ) { 5, 10, 15, 20, 30 };
    private readonly List<int> _templateConfigurationHourlyMinuteOptions = Enumerable.Range( 0, 60 ).ToList( );
}
