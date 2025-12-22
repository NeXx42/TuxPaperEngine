using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;

namespace AvaloniaUI.Pages.Settings.Display;

public partial class SettingsPage_Display_DisplayGroup_Screen : UserControl
{
    private string? representingDisplay;
    private Action<string, int>? onUpdateOrderCallback;

    public SettingsPage_Display_DisplayGroup_Screen()
    {
        InitializeComponent();
        inp_Priority.KeyUp += (_, __) => ValidateOrder();
    }

    public void Draw(ConfigManager.Screen screen, Action<string, int> onUpdateOrder)
    {
        representingDisplay = screen.screenName;

        lbl_ScreenName.Content = screen.screenName;
        inp_Priority.Text = screen.priority.ToString();

        onUpdateOrderCallback = onUpdateOrder;
    }

    private void ValidateOrder()
    {
        if (!int.TryParse(inp_Priority.Text, out int val) || string.IsNullOrEmpty(representingDisplay))
            return;

        onUpdateOrderCallback?.Invoke(representingDisplay, val);
    }
}