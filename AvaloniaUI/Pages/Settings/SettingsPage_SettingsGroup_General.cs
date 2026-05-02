using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaUI.Common;
using AvaloniaUI.Pages.Settings.Common;
using Logic;

namespace AvaloniaUI.Pages.Settings;

public class SettingsPage_SettingsGroup_General : ISettingsPage
{
    private SettingsPage_SettingsGroupContainer? ui;

    private SettingsPage_Common_DirectorySelector? startupScriptDir;

    private Common_Button? steamAuthenticateButton;
    private SettingsPage_Common_Textbox? steamUsername;

    public UserControl Setup()
    {
        ui = new SettingsPage_SettingsGroupContainer();
        ui.lbl_SettingsName.Content = "General Settings";

        startupScriptDir = new SettingsPage_Common_DirectorySelector();
        ui.content.Children.Add(startupScriptDir.Init("Save startup script to", ConfigManager.ConfigKeys.SaveStartupScriptLocation, (p) => string.IsNullOrEmpty(p) ? string.Empty : Path.Combine(p, "startup.sh")));

        steamUsername = new SettingsPage_Common_Textbox();
        ui.content.Children.Add(steamUsername.Init("Steam Username", ConfigManager.ConfigKeys.SteamUsername, HandleSteamCMDAuthenticateButton));

        return ui;
    }

    public async Task OnOpen()
    {
        ui!.IsVisible = true;

        await startupScriptDir!.LoadFromConfig();
        await steamUsername!.LoadFromConfig();
    }

    public void Close()
    {
        ui!.IsVisible = false;
    }

    private void HandleSteamCMDAuthenticateButton(Common_Button btn)
    {
        steamAuthenticateButton = btn;

        steamAuthenticateButton.Width = 300;
        steamAuthenticateButton.Label = "Authenticate";
        steamAuthenticateButton.RegisterClick(AttemptSteamCMDLogin, "Authenticating");
    }

    private async Task AttemptSteamCMDLogin()
    {
        bool status = await SteamCMDManager.TryToAuthenticate();

        Dispatcher.UIThread.Invoke(() =>
        {
            if (status)
            {

                steamAuthenticateButton!.OverwriteLabel("Authenticated");
            }
            else
            {
                steamAuthenticateButton!.OverwriteLabel("Failed to authenticate");
            }
        });
    }
}
