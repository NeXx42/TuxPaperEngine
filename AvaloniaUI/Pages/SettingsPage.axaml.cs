using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;

namespace AvaloniaUI.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();

        UpdateDisplays();
    }

    private void UpdateDisplays()
    {
        //foreach (ConfigManager.Screen screen in ConfigManager.screens)
        //{
        //
        //}
    }
}