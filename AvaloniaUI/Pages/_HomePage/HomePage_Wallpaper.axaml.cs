using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaUI.Utils;
using Logic.Data;

namespace AvaloniaUI.Pages._HomePage;

public partial class HomePage_Wallpaper : UserControl
{
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
}