using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaUI.Common;
using AvaloniaUI.Pages.Modal;
using CSharpSqliteORM;
using Logic;
using Logic.db;
using Logic.Enums;

namespace AvaloniaUI.Pages.Settings;

public class SettingsPage_SettingsGroup_Directories : ISettingsPage
{
    private SettingsPage_SettingsGroupContainer? ui;

    private bool isLoading;
    private ComboBox? engineType;

    private Label? engineLocation_Label;
    private Common_Button? engineLocation_SelectDir;
    private Common_Button? engineLocation_InstallBtn;
    private TextBox? engineLocation_CustomCommand;

    public UserControl Setup()
    {
        ui = new SettingsPage_SettingsGroupContainer();

        Grid typeContainer = new Grid()
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(710, GridUnitType.Pixel)],
            Height = 35,
        };
        Label lbl = new Label()
        {
            Content = "Engine Type"
        };
        engineType = new ComboBox();
        engineType.ItemsSource = System.Enum.GetValues(typeof(EngineType));
        engineType.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        engineType.SelectionChanged += (_, __) => _ = OnEngineTypeChange();
        Grid.SetColumn(engineType, 1);
        typeContainer.Children.Add(lbl);
        typeContainer.Children.Add(engineType);
        ui.content.Children.Add(typeContainer);



        StackPanel engineManagementContainer = new StackPanel();
        Grid installerContainer = new Grid()
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(500, GridUnitType.Pixel), new ColumnDefinition(10, GridUnitType.Pixel), new ColumnDefinition(200, GridUnitType.Pixel)],
            Height = 35,
        };
        engineLocation_Label = new Label()
        {
            Content = "Engine Location"
        };
        engineLocation_SelectDir = new Common_Button
        {
            Label = "Select Directory"
        };
        engineLocation_CustomCommand = new TextBox()
        {

        };
        engineLocation_SelectDir.RegisterClick(SelectInstallLocation);
        engineLocation_CustomCommand.TextChanged += (_, __) => _ = UpdateCustomCommand();
        Grid.SetColumn(engineLocation_SelectDir, 1);
        Grid.SetColumn(engineLocation_CustomCommand, 1);

        engineLocation_InstallBtn = new Common_Button
        {
            Label = "Install"
        };
        Grid.SetColumn(engineLocation_InstallBtn, 3);

        installerContainer.Children.Add(engineLocation_Label);
        installerContainer.Children.Add(engineLocation_SelectDir);
        installerContainer.Children.Add(engineLocation_InstallBtn);
        installerContainer.Children.Add(engineLocation_CustomCommand);

        engineManagementContainer.Children.Add(installerContainer);

        ui.content.Children.Add(engineManagementContainer);
        return ui;
    }

    public async Task OnOpen()
    {
        ui!.IsVisible = true;

        await RedrawConditionalSettings();
    }

    private async Task RedrawConditionalSettings()
    {
        isLoading = true;
        dbo_Config? location = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation);
        dbo_Config? type = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.ExecutableType);

        engineType!.SelectedIndex = type == null ? (int)EngineType.SystemWide : int.Parse(type.value!);

        if (engineType!.SelectedIndex == (int)EngineType.Directory)
        {
            engineLocation_Label!.Content = "Engine location";

            engineLocation_CustomCommand!.IsVisible = false;
            engineLocation_SelectDir!.IsVisible = true;
            engineLocation_SelectDir!.Label = location?.value ?? "Select Existing Install";

            engineLocation_InstallBtn!.Label = location == null ? "Install" : "Reinstall";

            engineLocation_InstallBtn.ClearCallback();
            engineLocation_InstallBtn.RegisterClick(BeginInstall);
        }
        else
        {
            engineLocation_Label!.Content = "Engine command";

            engineLocation_SelectDir!.IsVisible = false;
            engineLocation_CustomCommand!.IsVisible = true;
            engineLocation_CustomCommand!.Text = location?.value ?? "linux-wallpaperengine";

            engineLocation_InstallBtn!.Label = "Reset";

            engineLocation_InstallBtn.ClearCallback();
            engineLocation_InstallBtn.RegisterClick(ResetCustomCommand);
        }
        isLoading = false;
    }

    public void Close()
    {
        ui!.IsVisible = false;
    }

    private async Task BeginInstall()
    {
        await MainWindow.OpenModal<Modal_Downloader>();
    }

    private async Task SelectInstallLocation()
    {
        var result = (await MainWindow.instance!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Engine Location"
        })).FirstOrDefault();

        if (result != null)
        {
            await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation, result.Path.AbsolutePath);
            await RedrawConditionalSettings();
        }
    }

    private async Task OnEngineTypeChange()
    {
        if (isLoading)
            return;

        await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.ExecutableType, engineType!.SelectedIndex.ToString());

        switch ((EngineType)engineType!.SelectedIndex)
        {
            case EngineType.SystemWide:
                await ResetCustomCommand();
                return;

            case EngineType.Directory:
                await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation, null);
                break;
        }

        await RedrawConditionalSettings();
    }

    private async Task UpdateCustomCommand()
    {
        if (isLoading)
            return;

        await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation, engineLocation_CustomCommand!.Text);
    }

    private async Task ResetCustomCommand()
    {
        await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation, "linux-wallpaperengine");
        await RedrawConditionalSettings();
    }
}
