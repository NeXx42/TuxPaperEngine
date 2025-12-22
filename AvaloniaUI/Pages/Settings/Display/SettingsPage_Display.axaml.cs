using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaUI.Pages.Settings.Display;

public partial class SettingsPage_Display : UserControl
{
    public SettingsPage_Display()
    {
        InitializeComponent();
    }

    public async Task Draw()
    {
        await comp_DisplayGroups.Draw();
    }
}