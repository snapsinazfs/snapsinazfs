using Terminal.Gui;
namespace SnapsInAZfs.ConfigConsole;

public sealed partial class SnapsInAZfsConfigConsole : Window
{

  private ColorScheme tgDefault;
  private ColorScheme midnightColorScheme;
  private MenuBar topMenuBar;
  private MenuBarItem fileMenu;
  private MenuItem saveMenuItem;
  private MenuItem quitMenuItem;
  private MenuBarItem windowMenu;
  private MenuItem globalConfigMenuItem;
  private MenuItem templateConfigMenuItem;
  private MenuItem zfsConfigMenuItem;

  private void InitializeComponent ( )
  {
    topMenuBar                    = new ( );
    tgDefault                     = new ( );
    tgDefault.Normal              = new ( Color.White, Color.Blue );
    tgDefault.HotNormal           = new ( Color.BrightCyan, Color.Blue );
    tgDefault.Focus               = new ( Color.Black, Color.Gray );
    tgDefault.HotFocus            = new ( Color.BrightBlue, Color.Gray );
    tgDefault.Disabled            = new ( Color.Brown, Color.Blue );
    midnightColorScheme           = new ( );
    midnightColorScheme.Normal    = new ( Color.BrightBlue, Color.Black );
    midnightColorScheme.HotNormal = new ( Color.Cyan, Color.Black );
    midnightColorScheme.Focus     = new ( Color.BrightBlue, Color.Black );
    midnightColorScheme.HotFocus  = new ( Color.Cyan, Color.Black );
    midnightColorScheme.Disabled  = new ( Color.DarkGray, Color.Black );
    Width                         = Dim.Fill ( 0 );
    Height                        = Dim.Fill ( 0 );
    X                             = 0;
    Y                             = 0;
    ColorScheme                   = this.midnightColorScheme;
    Modal                         = false;
    Border.BorderStyle            = BorderStyle.Double;
    Border.BorderBrush            = Color.White;
    Border.Effect3D               = false;
    Border.Effect3DBrush          = null;
    Border.DrawMarginFrame        = true;
    TextAlignment                 = TextAlignment.Left;
    Title                         = "SnapsInAZfs Configuration Console";
    topMenuBar.Width              = Dim.Fill ( 0 );
    topMenuBar.Height             = 1;
    topMenuBar.X                  = 0;
    topMenuBar.Y                  = 0;
    topMenuBar.ColorScheme        = this.tgDefault;
    topMenuBar.Data               = "topMenuBar";
    topMenuBar.TextAlignment      = TextAlignment.Left;
    fileMenu                      = new ( );
    fileMenu.Title                = "_File";
    saveMenuItem                  = new ( );
    saveMenuItem.Title            = "_Save";
    saveMenuItem.Data             = "saveMenuItem";
    quitMenuItem                  = new ( );
    quitMenuItem.Title            = "_Quit";
    quitMenuItem.Data             = "quitMenuItem";
    fileMenu.Children =
    [
      saveMenuItem,
      quitMenuItem
    ];
    windowMenu                   = new ( );
    windowMenu.Title             = "_Window";
    globalConfigMenuItem         = new ( );
    globalConfigMenuItem.Title   = "Show _Global Configuration Window";
    globalConfigMenuItem.Data    = "globalConfigMenuItem";
    templateConfigMenuItem       = new ( );
    templateConfigMenuItem.Title = "Show _Template Configuration Window";
    templateConfigMenuItem.Data  = "templateConfigMenuItem";
    zfsConfigMenuItem            = new ( );
    zfsConfigMenuItem.Title      = "Show ZFS Configuration Window";
    zfsConfigMenuItem.Data       = "zfsConfigMenuItem";
    windowMenu.Children =
    [
      globalConfigMenuItem,
      templateConfigMenuItem,
      zfsConfigMenuItem
    ];
    topMenuBar.Menus =
    [
      fileMenu,
      windowMenu
    ];
    Add ( topMenuBar );
  }
}
