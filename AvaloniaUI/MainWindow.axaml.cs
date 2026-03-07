using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Logic;

namespace AvaloniaUI;

public partial class MainWindow : Window
{
    public static MainWindow? instance { private set; get; }

    private ImmutableBlurEffect? blurEffect;

    public MainWindow()
    {
        instance = this;

        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        RegisterScreens();

        blurEffect = new ImmutableBlurEffect(5);

        Navbar.Setup(0, SelectPage);

        //btn_Settings.RegisterClick(() => ToggleSettings(true));

        //cont_SettingsContainer.PointerPressed += (_, __) => ToggleSettings(false);
        //Page_Settings.PointerPressed += (_, e) => e.Handled = true;

        //ToggleSettings(false);

        //btn_Library.RegisterClick(() => OpenMenu(nameof(btn_Library)));
        //btn_Workshop.RegisterClick(() => OpenMenu(nameof(btn_Workshop)));

        //OpenMenu(nameof(btn_Library));
    }

    private async void RegisterScreens()
    {
        List<ConfigManager.Screen> unpackedScreens = new List<ConfigManager.Screen>();

        foreach (var screen in this.Screens.All)
        {
            unpackedScreens.Add(new ConfigManager.Screen()
            {
                screenName = screen.DisplayName!
            });
        }

        await ConfigManager.RegisterDisplays(unpackedScreens.ToArray());
    }

    private void SelectPage(int page)
    {
        Page_Home.IsVisible = false;
        Page_Workshop.IsVisible = false;
        Page_Settings.IsVisible = false;

        switch (page)
        {
            case 0:
                Page_Home.IsVisible = true;
                Page_Home.LoadPage();
                break;

            case 1:
                Page_Workshop.IsVisible = true;
                Page_Workshop.LoadPage();
                break;

            case 2:
                Page_Settings.IsVisible = true;
                Page_Settings.OnOpen();
                break;
        }
    }

    public static async Task AsyncLoad(Func<Task> task)
    {
        //instance!.Pages.Effect = instance.blurEffect;
        Dispatcher.UIThread.Post(() => { });

        await task();
        //instance.Pages.Effect = null;
    }

    public static async Task<T> AsyncLoad_WithReturn<T>(Func<Task<T>> task)
    {
        //instance!.Pages.Effect = instance.blurEffect;
        Dispatcher.UIThread.Post(() => { });

        T res = await task();
        //instance.Pages.Effect = null;

        return res;
    }
}