using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Utils;
using Logic;

namespace AvaloniaUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        SetupDependencies();
    }

    private async void SetupDependencies()
    {
        if (!Design.IsDesignMode)
        {
            await ConfigManager.Init();
            await WorkshopManager.Init(ImageFetcher.HandleBrushCreation);
        }

    }


    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}