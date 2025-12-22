using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaUI.Utils;
using Logic.Data;

namespace AvaloniaUI.Pages._HomePage;

public partial class HomePage_Wallpaper : UserControl
{
    private static Thickness? unselectedThickness;
    private static Thickness? selectedThickness;
    private static ImmutableSolidColorBrush? selectedBrush;

    private long? representingId;
    private HomePage? master;

    public HomePage_Wallpaper()
    {
        InitializeComponent();

        img_Icon.PointerPressed += (_, __) => HandleSelection();
    }

    public async void StartDraw(WorkshopEntry entry, HomePage master)
    {
        this.master = master;
        this.representingId = entry.id;

        lbl_Title.Content = "loading";
        img_Icon.Background = null;

        await entry.DecodeBasic();
        lbl_Title.Content = entry.title;

        ImageBrush? brush = await ImageFetcher.GetIcon(entry.id);
        img_Icon.Background = brush;
    }

    private void HandleSelection()
    {
        if (master == null || !representingId.HasValue)
            return;

        master.SelectWallpaper(representingId.Value);
    }

    public void ToggleSelection(bool to)
    {
        unselectedThickness ??= new Thickness(0);
        selectedThickness ??= new Thickness(2);
        selectedBrush ??= new ImmutableSolidColorBrush(Color.FromRgb(0, 255, 0));

        border.BorderThickness = to ? selectedThickness.Value : unselectedThickness.Value;
        border.BorderBrush = to ? selectedBrush : null;
    }
}