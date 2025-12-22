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
        await SettingsGroup_Display.Draw();
    }
}