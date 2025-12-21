using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Pages.Settings;
using Logic;

namespace AvaloniaUI.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    public async void OnOpen()
    {
        await UpdateDisplays();
    }

    private async Task UpdateDisplays()
    {
        setting_Display_cont_Screens.Children.Clear();
        ConfigManager.Screen[]? screens = ConfigManager.screens;

        if (screens == null)
            return;

        foreach (ConfigManager.Screen screen in screens)
        {
            SettingsPage_Display_DisplayGroup displayGroup = new SettingsPage_Display_DisplayGroup();
            await displayGroup.Draw(screen);

            setting_Display_cont_Screens.Children.Add(displayGroup);
        }
    }
}