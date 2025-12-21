using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Logic;

namespace AvaloniaUI;

public partial class MainWindow : Window
{
    private ImmutableBlurEffect? blurEffect;

    public MainWindow()
    {
        InitializeComponent();
        RegisterScreens();

        blurEffect = new ImmutableBlurEffect(5);

        Settings.PointerPressed += (_, __) => ToggleSettings(false);
        btn_Settings.Click += (_, __) => ToggleSettings(true);

        ToggleSettings(false);
    }

    private void RegisterScreens()
    {
        List<ConfigManager.Screen> unpackedScreens = new List<ConfigManager.Screen>();

        foreach (var screen in this.Screens.All)
        {
            unpackedScreens.Add(new ConfigManager.Screen()
            {
                screenName = screen.DisplayName!
            });
        }

        ConfigManager.RegisterDisplays(unpackedScreens.ToArray());

    }

    public void ToggleSettings(bool to)
    {
        Pages.Effect = to ? blurEffect : null;
        Settings.IsVisible = to;

        if (to)
            Page_Settings.OnOpen();
    }

}