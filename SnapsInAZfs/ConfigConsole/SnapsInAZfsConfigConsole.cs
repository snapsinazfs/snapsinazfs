#region MIT LICENSE

// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/

#endregion

using System.Text.Json;
using System.Text.Json.Serialization;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui;

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.24.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------

namespace SnapsInAZfs.ConfigConsole;

public sealed partial class SnapsInAZfsConfigConsole
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public SnapsInAZfsConfigConsole( )
    {
        // ReSharper disable HeapView.ObjectAllocation.Possible
        // ReSharper disable HeapView.DelegateAllocation
        Initialized += SnapsInAZfsConfigConsoleOnInitialized;
        Ready += SnapsInAZfsConfigConsoleOnReady;
        InitializeComponent( );
        globalConfigMenuItem.Action = ShowGlobalConfigurationWindow;
        globalConfigMenuItem.Shortcut = Key.CtrlMask | Key.g;
        templateConfigMenuItem.Action = ShowTemplateConfigurationWindow;
        templateConfigMenuItem.Shortcut = Key.CtrlMask | Key.t;
        zfsConfigMenuItem.Action = ShowZfsConfigurationWindow;
        zfsConfigMenuItem.Shortcut = Key.CtrlMask | Key.z;
        saveMenuItem.Action = SaveGlobalConfiguration;
        Application.RootKeyEvent += ApplicationRootKeyEvent;
        // ReSharper restore HeapView.ObjectAllocation.Possible
        // ReSharper restore HeapView.DelegateAllocation
    }

    private bool _eventsEnabled;
    private GlobalConfigurationWindow? _globalConfigurationWindow;
    private bool _globalConfigurationWindowShown;
    private TemplateConfigurationWindow? _templateConfigurationWindow;
    private bool _templateConfigurationWindowShown;
    private ZfsConfigurationWindow? _zfsConfigurationWindow;
    private bool _zfsConfigurationWindowShown;

    public static bool ZfsConfigurationWindowDisabledDueToError { get; set; }

    private bool ApplicationRootKeyEvent( KeyEvent e )
    {
        if ( !e.IsCtrl )
        {
            return false;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch ( e.Key & Key.CharMask )
        {
            case Key.g:
            case Key.G:
                globalConfigMenuItem.Action( );
                return true;
            case Key.t:
            case Key.T:
                templateConfigMenuItem.Action( );
                return true;
            case Key.z:
            case Key.Z:
                zfsConfigMenuItem.Action( );
                return true;
        }

        return false;
    }

    private void DisableEventHandlers( )
    {
        if ( !_eventsEnabled )
        {
            return;
        }

        _eventsEnabled = false;
    }

    private void EnableEventHandlers( )
    {
        if ( _eventsEnabled )
        {
            return;
        }

        _eventsEnabled = true;
    }

    private void HideGlobalConfigurationWindow( )
    {
        Remove( _globalConfigurationWindow );
        globalConfigMenuItem.Action = ShowGlobalConfigurationWindow;
        globalConfigMenuItem.Title = "Show _Global Configuration Window";
        _globalConfigurationWindowShown = false;
    }

    private void HideTemplateConfigurationWindow( )
    {
        Remove( _templateConfigurationWindow );
        templateConfigMenuItem.Action = ShowTemplateConfigurationWindow;
        templateConfigMenuItem.Title = "Show _Template Configuration Window";
        _templateConfigurationWindowShown = false;
    }

    private void HideZfsConfigurationWindow( )
    {
        Remove( _zfsConfigurationWindow );
        zfsConfigMenuItem.Action = ShowZfsConfigurationWindow;
        zfsConfigMenuItem.Title = "Show _Template Configuration Window";
        _zfsConfigurationWindowShown = false;
    }

    private void SaveGlobalConfiguration( )
    {
        try
        {
            DisableEventHandlers( );
            bool globalWindowNull = _globalConfigurationWindow is null;
            bool templateWindowNull = _templateConfigurationWindow is null;
            bool bothWindowsNull = globalWindowNull && templateWindowNull;
            bool globalConfigurationIsChanged = !globalWindowNull && _globalConfigurationWindow!.IsConfigurationChanged;
            bool onlyGlobalWindowNotNullAndNoChanges = !globalWindowNull && !globalConfigurationIsChanged && templateWindowNull;
            bool isAnyTemplateModified = TemplateConfigurationWindow.IsAnyTemplateModified;
            bool templatesAddedRemovedOrModified = TemplateConfigurationWindow.TemplatesAddedRemovedOrModified;
            bool onlyTemplateWindowNotNullAndNoChanges = globalWindowNull && !templateWindowNull && !isAnyTemplateModified && !templatesAddedRemovedOrModified;
            bool bothWindowsNotNullAndNoChanges = !globalWindowNull && !templateWindowNull && !globalConfigurationIsChanged && !isAnyTemplateModified && !templatesAddedRemovedOrModified;
            if ( bothWindowsNull || onlyGlobalWindowNotNullAndNoChanges || onlyTemplateWindowNotNullAndNoChanges || bothWindowsNotNullAndNoChanges )
            {
                Logger.Warn( "Save configuration requested when no changes were made." );
                int messageBoxResult = MessageBox.Query( "Are You Sure?", "No changes have been made to global configuration. Save anyway?", "Cancel", "Save" );
                if ( messageBoxResult == 0 )
                {
                    return;
                }

                SnapsInAZfsSettings copyOfCurrentSettings = Program.Settings! with { };
                (bool status, string reasonOrFile) copyConfigResult = ShowSaveDialog( copyOfCurrentSettings );
                if ( copyConfigResult.status )
                {
                    Logger.Info( "Copy of existing configuration saved to {0}", copyConfigResult.reasonOrFile );
                    return;
                }

                switch ( copyConfigResult.reasonOrFile )
                {
                    case "canceled":
                        Logger.Debug( "Canceled configuration save dialog" );
                        return;
                    case "no file name":
                        Logger.Error( "No file name provided in save dialog. Configuration copy not saved." );
                        return;
                    default:
                        Logger.Error( "Failed to save copy of configuration." );
                        return;
                }
            }

            switch ( globalWindowNull )
            {
                case true:
                    Logger.Warn( "Global Configuration Window was still null when we tried to access it. Creating new instance" );
                    _globalConfigurationWindow = new( );
                    break;
                case false when !_globalConfigurationWindow!.ValidateGlobalConfigValues( ):
                    Logger.Warn( "Global configuration input validation failed. Configuration not saved." );
                    MessageBox.ErrorQuery( "Invalid Global Configuration", "One or more entries in the global configuration window are invalid. Correct any invalid entries and try again.", "OK" );
                    return;
            }

            if ( !templateWindowNull && isAnyTemplateModified )
            {
                int dialogResult = MessageBox.Query( "Commit Modified Templates?", "You have pending template modifications that have not been committed and will therefore not be saved unless committed.\n\nCommit pending template changes now?", "Cancel Save", "Commit Templates" );
                if ( dialogResult != 1 )
                {
                    return;
                }

                TemplateConfigurationWindow.CommitModifiedTemplates( );
            }

            SnapsInAZfsSettings newSettingsToSave = new( )
            {
                DryRun = _globalConfigurationWindow!.dryRunRadioGroup.GetSelectedBooleanFromLabel( ),
                TakeSnapshots = _globalConfigurationWindow.takeSnapshotsRadioGroup.GetSelectedBooleanFromLabel( ),
                PruneSnapshots = _globalConfigurationWindow.pruneSnapshotsRadioGroup.GetSelectedBooleanFromLabel( ),
                LocalSystemName = _globalConfigurationWindow.localSystemNameTextBox.Text.ToString( )!,
                ZfsPath = _globalConfigurationWindow.pathToZfsTextField.Text.ToString( )!,
                ZpoolPath = _globalConfigurationWindow.pathToZpoolTextField.Text.ToString( )!,
                Templates = Program.Settings!.Templates,
                Monitoring = Program.Settings.Monitoring
            };
            newSettingsToSave.Monitoring.EnableHttp = _globalConfigurationWindow.httpMonitoringRadioGroup.GetSelectedBooleanFromLabel( );

            ( bool status, string reasonOrFile ) = ShowSaveDialog( newSettingsToSave );

            switch ( status, reasonOrFile )
            {
                case (true, _):
                    Logger.Info( "Configuration saved to {0}", reasonOrFile );
                    return;
                case (false, "canceled"):
                    Logger.Debug( "Canceled configuration save dialog" );
                    return;
                case (false, "no file name"):
                    Logger.Error( "No file name provided in save dialog. Configuration not saved." );
                    return;
                default:
                    Logger.Error( "Failed to save configuration." );
                    return;
            }
        }
        finally
        {
            EnableEventHandlers( );
        }
    }

    private void ShowGlobalConfigurationWindow( )
    {
        if ( _templateConfigurationWindowShown )
        {
            HideTemplateConfigurationWindow( );
        }

        if ( _zfsConfigurationWindowShown )
        {
            HideZfsConfigurationWindow( );
        }

        _globalConfigurationWindow ??= new( );
        Add( _globalConfigurationWindow );
        if ( ShowChild( _globalConfigurationWindow ) )
        {
            LayoutSubviews( );
            _globalConfigurationWindowShown = true;
            _globalConfigurationWindow.dryRunRadioGroup.SetFocus( );
            Logger.Debug( "Showing global configuration window" );
            globalConfigMenuItem.Action = HideGlobalConfigurationWindow;
            globalConfigMenuItem.Title = "Hide _Global Configuration Window";
        }
        else
        {
            Remove( _globalConfigurationWindow );
            Logger.Error( "Unable to show global configuration window" );
        }
    }

    private static (bool, string) ShowSaveDialog( SnapsInAZfsSettings settings )
    {
        using ( SaveDialog globalConfigSaveDialog = new( "Save Global Configuration", "Select file to save global configuration", new( ) { ".json" } ) )
        {
            globalConfigSaveDialog.DirectoryPath = "/etc/SnapsInAZfs";
            globalConfigSaveDialog.AllowsOtherFileTypes = true;
            globalConfigSaveDialog.CanCreateDirectories = true;
            globalConfigSaveDialog.Modal = true;
            Application.Run( globalConfigSaveDialog );
            if ( globalConfigSaveDialog.Canceled )
            {
                return ( false, "canceled" );
            }

            if ( globalConfigSaveDialog.FileName.IsEmpty )
            {
                return ( false, "no file name" );
            }

            string path = globalConfigSaveDialog.FilePath.ToString( ) ?? throw new InvalidOperationException( "Null string provided for save file name" );

            if ( File.Exists( path ) )
            {
                int overwriteResult = MessageBox.ErrorQuery( "Overwrite Existing File?", $"The file '{path}' already exists. Continue saving and overwrite this file?", "Cancel", "Overwrite" );
                if ( overwriteResult == 0 )
                {
                    return ( false, "canceled" );
                }
            }

            try
            {
                File.WriteAllText( path, JsonSerializer.Serialize( settings, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull } ) );
                return ( true, path );
            }
            catch ( Exception e )
            {
                Logger.Error( e, "Error saving settings to requested path {0}", path );
                return ( false, "error" );
            }
        }
    }

    private void ShowTemplateConfigurationWindow( )
    {
        if ( _globalConfigurationWindowShown )
        {
            HideGlobalConfigurationWindow( );
        }

        if ( _zfsConfigurationWindowShown )
        {
            HideZfsConfigurationWindow( );
        }

        _templateConfigurationWindow ??= new( );
        Add( _templateConfigurationWindow );
        if ( ShowChild( _templateConfigurationWindow ) )
        {
            LayoutSubviews( );
            _templateConfigurationWindowShown = true;
            _templateConfigurationWindow.templateListView.SetFocus( );
            Logger.Debug( "Showing template configuration window" );
            templateConfigMenuItem.Action = HideTemplateConfigurationWindow;
            templateConfigMenuItem.Title = "Hide _Template Configuration Window";
        }
        else
        {
            Logger.Error( "Unable to show template configuration window" );
        }
    }

    private void ShowZfsConfigurationWindow( )
    {
        if ( ZfsConfigurationWindowDisabledDueToError )
        {
            MessageBox.ErrorQuery( "ZFS Configuration Disabled", "ZFS Configuration Window has been disabled due to errors in configuration.\nResolve any reported errors and run the Configuration Console again.", "Bummer" );
            zfsConfigMenuItem.CanExecute = ( ) => false;
            zfsConfigMenuItem.Action = null;
            return;
        }

        if ( _globalConfigurationWindowShown )
        {
            HideGlobalConfigurationWindow( );
        }

        if ( _templateConfigurationWindowShown )
        {
            HideTemplateConfigurationWindow( );
        }

        _zfsConfigurationWindow ??= new( );
        Add( _zfsConfigurationWindow );
        if ( ShowChild( _zfsConfigurationWindow ) )
        {
            LayoutSubviews( );
            _zfsConfigurationWindowShown = true;
            _zfsConfigurationWindow.zfsTreeView.SetFocus( );
            Logger.Debug( "Showing ZFS configuration window" );
            zfsConfigMenuItem.Action = HideZfsConfigurationWindow;
            zfsConfigMenuItem.Title = "Hide ZFS Configuration Window";
        }
        else
        {
            Logger.Error( "Unable to show ZFS configuration window" );
        }
    }

    private void SnapsInAZfsConfigConsoleOnInitialized( object? sender, EventArgs e )
    {
        Logger.Trace( "Configuration console main window initialized. Setting global quit hotkey" );
        AddKeyBinding( Key.CtrlMask | Key.q, Command.QuitToplevel );
        quitMenuItem.Action = Application.Top.RequestStop;
        IsMdiContainer = true;
    }

    private void SnapsInAZfsConfigConsoleOnReady( )
    {
        Logger.Trace( "Configuration console main window ready" );
        EnableEventHandlers( );
    }
}
