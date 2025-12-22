using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;

namespace AvaloniaUI.Pages.Settings.Display;

public partial class SettingsPage_Display_DisplayGroup : UserControl
{
    public SettingsPage_Display_DisplayGroup()
    {
        InitializeComponent();
    }

    public async Task Draw()
    {
        RedrawScreens();

    }

    private void RedrawScreens()
    {
        container.Children.Clear();
        ConfigManager.Screen[] screens = ConfigManager.GetScreensOrdered();

        foreach (var screen in screens)
        {
            SettingsPage_Display_DisplayGroup_Screen ui = new SettingsPage_Display_DisplayGroup_Screen();
            ui.Draw(screen, UpdateScreenOrder);
            ui.Width = 300;
            ui.Height = 150;

            container.Children.Add(ui);
        }
    }

    private async void UpdateScreenOrder(string screenName, int to)
    {
        await ConfigManager.UpdateDisplayOrder(screenName, to);

        RedrawScreens();
    }
}