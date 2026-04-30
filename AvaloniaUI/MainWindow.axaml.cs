using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaUI.Interfaces;
using AvaloniaUI.Pages.Modal;
using Logic;

namespace AvaloniaUI;

public partial class MainWindow : Window
{
    public static MainWindow? instance { private set; get; }
    public static IAuthenticationModal getAuthenticationModal => instance!.modal_auth;

    private IModal? activeModal;

    public MainWindow()
    {
        instance = this;

        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        RegisterScreens();
        _ = CloseModal();

        modalContainer.PointerPressed += (_, __) => _ = CloseModal();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Navbar.Setup(0, SelectPage);
        base.OnLoaded(e);
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
        Dispatcher.UIThread.Post(() => { });

        await task();
    }

    public static async Task<T> AsyncLoad_WithReturn<T>(Func<Task<T>> task)
    {
        Dispatcher.UIThread.Post(() => { });

        T res = await task();
        return res;
    }


    public static async Task<T> OpenModal<T>() where T : Control, IModal
    {
        MainWindow.instance!.modalContainer.IsVisible = true;

        T modal = Activator.CreateInstance<T>();
        modal.PointerPressed += (_, e) => e.Handled = true;

        MainWindow.instance!.modalContainer.Children.Add(modal);
        MainWindow.instance!.activeModal = modal;

        return modal;
    }

    public static async Task CloseModal()
    {
        if (instance!.activeModal?.isBlocking ?? false)
            return;

        instance!.modalContainer.Children.Clear();
        instance!.activeModal = null;

        instance!.modalContainer.IsVisible = false;
    }
}