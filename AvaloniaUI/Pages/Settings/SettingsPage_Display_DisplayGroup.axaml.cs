using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;

namespace AvaloniaUI.Pages.Settings;

public partial class SettingsPage_Display_DisplayGroup : UserControl
{
    public SettingsPage_Display_DisplayGroup()
    {
        InitializeComponent();
    }

    public async Task Draw(ConfigManager.Screen screen)
    {
        lbl_ScreenName.Content = screen.screenName;
    }
}