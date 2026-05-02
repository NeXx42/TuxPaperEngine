using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Common;
using Logic;
using Logic.db;

namespace AvaloniaUI.Pages.Settings.Common;

public partial class SettingsPage_Common_Textbox : UserControl
{
    private ConfigManager.ConfigKeys? configKey;

    public SettingsPage_Common_Textbox()
    {
        InitializeComponent();
    }

    public UserControl Init(string lbl, ConfigManager.ConfigKeys key, Action<Common_Button>? buttonHandler = null)
    {
        configKey = key;

        if (buttonHandler != null)
        {
            btn.IsVisible = true;
            cont.ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)];

            btn.ClearCallback();
            buttonHandler(btn);
        }
        else
        {
            btn.IsVisible = false;
            cont.ColumnDefinitions = [];
        }

        return Init(lbl);
    }

    public UserControl Init(string lbl)
    {
        lbl_Header.Content = lbl;

        inp_Value.KeyUp += (_, __) => _ = SaveToConfig();
        return this;
    }

    public async Task LoadFromConfig()
    {
        if (configKey == null)
            return;

        dbo_Config? key = await ConfigManager.GetConfigValue(configKey.Value);

        if (key != null)
        {
            inp_Value.Text = key.value;
        }
    }

    private async Task SaveToConfig()
    {
        if (configKey == null)
            return;

        await ConfigManager.SetConfigValue(configKey.Value, inp_Value.Text);
    }
}