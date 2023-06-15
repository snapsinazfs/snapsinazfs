// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Globalization;
using NStack;

namespace Sanoid.ConfigConsole;

internal class TemplateConfigurationValidator
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public string? NamingComponentSeparator { get; private set; }
    public string? NamingDailySuffix { get; private set; }
    public string? NamingFrequentSuffix { get; private set; }
    public string? NamingHourlySuffix { get; private set; }
    public string? NamingMonthlySuffix { get; private set; }
    public string? NamingPrefix { get; private set; }
    public string? NamingTimestampFormatString { get; private set; }
    public string? NamingWeeklySuffix { get; private set; }
    public string? NamingYearlySuffix { get; private set; }
    private TemplateConfigurationWindow? TemplateWindow { get; set; }
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

    public (bool IsValid, List<TemplateValidationException> ValidationExceptions) ValidateFieldValues( TemplateConfigurationWindow templateWindow )
    {
        TemplateWindow = templateWindow;
        if ( TemplateWindow is null )
        {
            return ( false, new( ) );
        }

        bool isValid = true;
        string templateName = TemplateWindow.SelectedTemplateItem.TemplateName;
        List<TemplateValidationException> validationExceptions = new( );
        // Naming fields
        NamingComponentSeparator = TemplateWindow.namingComponentSeparatorValidateField.IsValid ? TemplateWindow.namingComponentSeparatorValidateField?.Text?.ToString( ) : null;
        NamingPrefix = TemplateWindow.namingPrefixTextValidateField.IsValid ? TemplateWindow.namingPrefixTextValidateField?.Text?.ToString( ) : null;
        NamingTimestampFormatString = TemplateWindow.namingTimestampFormatTextField?.Text?.ToString( );
        NamingFrequentSuffix = TemplateWindow.namingFrequentSuffixTextValidateField.IsValid ? TemplateWindow.namingFrequentSuffixTextValidateField?.Text?.ToString( ) : null;
        NamingHourlySuffix = TemplateWindow.namingHourlySuffixTextValidateField.IsValid ? TemplateWindow.namingHourlySuffixTextValidateField?.Text?.ToString( ) : null;
        NamingDailySuffix = TemplateWindow.namingDailySuffixTextValidateField.IsValid ? TemplateWindow.namingDailySuffixTextValidateField?.Text?.ToString( ) : null;
        NamingWeeklySuffix = TemplateWindow.namingWeeklySuffixTextValidateField.IsValid ? TemplateWindow.namingWeeklySuffixTextValidateField?.Text?.ToString( ) : null;
        NamingMonthlySuffix = TemplateWindow.namingMonthlySuffixTextValidateField.IsValid ? TemplateWindow.namingMonthlySuffixTextValidateField?.Text?.ToString( ) : null;
        NamingYearlySuffix = TemplateWindow.namingYearlySuffixTextValidateField.IsValid ? TemplateWindow.namingYearlySuffixTextValidateField?.Text?.ToString( ) : null;
        if ( string.IsNullOrWhiteSpace( NamingComponentSeparator ) )
        {
            string errorMessage = $"Snapshot name component separator value {NamingComponentSeparator} for template {templateName} is invalid";
            TemplateValidationException ex = new( "Name Component Separator", NamingComponentSeparator, TemplateWindow.namingComponentSeparatorValidateField, errorMessage );
            validationExceptions.Add( ex );
            Logger.Warn( ex, errorMessage );
            isValid = false;
        }

        if ( !TemplateWindow.namingPrefixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingPrefix ) )
        {
            string errorMessage = $"Snapshot name prefix value {NamingPrefix} for template {templateName} is invalid";
            TemplateValidationException ex = new( "Name Prefix", NamingPrefix, TemplateWindow.namingPrefixTextValidateField, errorMessage );
            validationExceptions.Add( ex );
            Logger.Warn( ex, errorMessage );
            isValid = false;
        }

        if ( string.IsNullOrWhiteSpace( NamingTimestampFormatString ) )
        {
            string errorMessage = $"Snapshot timestamp format string value {NamingTimestampFormatString} for template {templateName} is invalid";
            TemplateValidationException ex = new( "Timestamp Format String", NamingTimestampFormatString, TemplateWindow.namingTimestampFormatTextField, errorMessage );
            validationExceptions.Add( ex );
            Logger.Warn( ex, errorMessage );
            isValid = false;
        }
        else
        {
            try
            {
                string validationTestDateString = DateTimeOffset.Now.ToString( NamingTimestampFormatString );
                Logger.Debug( "Snapshot temmplate timestamp format string for template {0} is valid and results in output of the form: {1}", templateName, validationTestDateString );
            }
            catch ( FormatException ex )
            {
                string errorMessage = $"Snapshot timestamp format string value {NamingTimestampFormatString} for template {templateName} is not in the correct format. Please see Microsoft documentation for DateTime format strings.";
                TemplateValidationException tve = new( "Timestamp Format String", NamingTimestampFormatString, TemplateWindow.namingTimestampFormatTextField!, errorMessage, ex );
                validationExceptions.Add( tve );
                Logger.Warn( tve, errorMessage );
                isValid = false;
            }
        }

        if ( !TemplateWindow.namingFrequentSuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingFrequentSuffix ) )
        {
            Logger.Warn( "Snapshot template frequent suffix value {0} for template {1} is invalid", NamingFrequentSuffix, templateName );
            isValid = false;
        }

        if ( !TemplateWindow.namingHourlySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingHourlySuffix ) )
        {
            Logger.Warn( "Snapshot template hourly suffix value {0} for template {1} is invalid", NamingHourlySuffix, templateName );
            isValid = false;
        }

        if ( !TemplateWindow.namingDailySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingDailySuffix ) )
        {
            Logger.Warn( "Snapshot template daily suffix value {0} for template {1} is invalid", NamingDailySuffix, templateName );
            isValid = false;
        }

        if ( !TemplateWindow.namingWeeklySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingWeeklySuffix ) )
        {
            Logger.Warn( "Snapshot template weekly suffix value {0} for template {1} is invalid", NamingWeeklySuffix, templateName );
            isValid = false;
        }

        if ( !TemplateWindow.namingMonthlySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingMonthlySuffix ) )
        {
            Logger.Warn( "Snapshot template monthly suffix value {0} for template {1} is invalid", NamingMonthlySuffix, templateName );
            isValid = false;
        }

        if ( !TemplateWindow.namingYearlySuffixTextValidateField!.IsValid || string.IsNullOrWhiteSpace( NamingYearlySuffix ) )
        {
            Logger.Warn( "Snapshot template yearly suffix value {0} for template {1} is invalid", NamingYearlySuffix, templateName );
            isValid = false;
        }

        // Timing fields
        string weeklyDayString = TemplateWindow.timingWeeklyDayTextValidateField.Text.ToString( )!;

        // This exception can't be thrown by the call to string.Equals, because it only happens if the third argument isn't valid
        // ReSharper disable ExceptionNotDocumentedOptional
        int weeklyDayStringIndex = CultureTimeHelpers.DayNamesLongAndAbbreviated.FindIndex( m => string.Equals( m, weeklyDayString, StringComparison.InvariantCultureIgnoreCase ) );
        // ReSharper disable ExceptionNotDocumentedOptional
        DayOfWeek dayOfWeek = (DayOfWeek)( weeklyDayStringIndex % 7 );

        TimingFrequentPeriod = TemplateConfigurationWindow.TemplateConfigurationFrequentPeriodOptions[ TemplateWindow.frequentPeriodRadioGroup.SelectedItem ];
        TimingHourlyMinute = TemplateWindow.timingHourlyMinuteTextValidateField.Text.ToNullableInt32( );
        TimingDailyTime = TimeOnly.FromTimeSpan( TemplateWindow.dailyTimeTimeField.Time );
        TimingWeeklyDay = dayOfWeek;
        TimingWeeklyTime = TimeOnly.FromTimeSpan( TemplateWindow.weeklyTimeTimeField.Time );
        ustring monthlyDayString = TemplateWindow.timingMonthlyDayTextValidateField.Text;
        int monthlyDayInt = monthlyDayString.ToInt32( -1 );
        int numberOfMonthsInYear = CultureInfo.CurrentCulture.Calendar.GetMonthsInYear( DateTimeOffset.Now.Year );
        TimingMonthlyDay = monthlyDayInt;
        TimingMonthlyTime = TimeOnly.FromTimeSpan( TemplateWindow.monthlyTimeTimeField.Time );
        TimingYearlyMonth = TemplateWindow.yearlyMonthComboBox.SelectedItem + 1;
        TimingYearlyDay = TemplateWindow.timingYearlyDayTextValidateField?.Text?.ToNullableInt32( );
        TimingYearlyTime = TimeOnly.FromTimeSpan( TemplateWindow.yearlyTimeTimeField.Time );
        if ( TimingFrequentPeriod is null || !TemplateConfigurationWindow.TemplateConfigurationFrequentPeriodOptions.Contains( (int)TimingFrequentPeriod ) )
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

        if ( TimingYearlyDay is null )
        {
            Logger.Warn( "Snapshot template yearly day value {0} for template {1} is invalid", TimingYearlyDay, templateName );
            isValid = false;
        }

        return ( isValid, validationExceptions );
    }
}
