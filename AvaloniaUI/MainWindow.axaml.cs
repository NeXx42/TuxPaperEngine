using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaUI;

public partial class MainWindow : Window
{
    private ImmutableBlurEffect? blurEffect;

    public MainWindow()
    {
        InitializeComponent();

        blurEffect = new ImmutableBlurEffect(5);

        Settings.PointerPressed += (_, __) => ToggleSettings(false);
        btn_Settings.Click += (_, __) => ToggleSettings(true);

        ToggleSettings(false);
    }

    public void ToggleSettings(bool to)
    {
        Pages.Effect = to ? blurEffect : null;
        Settings.IsVisible = to;
    }

}