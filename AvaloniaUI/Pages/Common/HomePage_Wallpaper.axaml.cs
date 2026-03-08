using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using AvaloniaUI.Pages.Common;
using AvaloniaUI.Utils;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public partial class Common_Wallpaper : UserControl
{
    private static Thickness? unselectedThickness;
    private static Thickness? selectedThickness;
    private static ImmutableSolidColorBrush? selectedBrush;

    private CancellationTokenSource? cancellationToken;

    private IWorkshopEntry? representing;
    private ItemFormatterBase? master;

    public Common_Wallpaper()
    {
        InitializeComponent();
        border.PointerPressed += (_, __) => _ = HandleSelection();
    }

    public void DrawSkeleton()
    {
        skeleton_Shimmer.IsVisible = true;

        this.representing = null;

        lbl_Title.Content = string.Empty;
        img_Icon.Background = null;
    }

    public async void StartDraw(IWorkshopEntry entry, ItemFormatterBase master)
    {
        await (cancellationToken?.CancelAsync() ?? Task.CompletedTask);
        cancellationToken = new CancellationTokenSource();

        await InternalDraw(entry, cancellationToken.Token);

        async Task InternalDraw(IWorkshopEntry entry, CancellationToken token)
        {
            skeleton_Shimmer.IsVisible = false;

            this.master = master;
            this.representing = entry;
            lbl_Title.Content = entry.getTitle;

            img_Icon.Background = null;
            ImageBrush? brush = await ImageFetcher.GetIcon(entry, token);

            if (token.IsCancellationRequested)
                return;

            img_Icon.Background = brush;
        }
    }

    private async Task HandleSelection()
    {
        if (master == null || representing == null)
            return;

        await master.SelectWallpaper(representing);
    }

    public void ToggleSelection(bool to)
    {
        unselectedThickness ??= new Thickness(0);
        selectedThickness ??= new Thickness(2);
        selectedBrush ??= new ImmutableSolidColorBrush(Color.FromRgb(88, 101, 242));

        border.BorderThickness = to ? selectedThickness.Value : unselectedThickness.Value;
        border.BorderBrush = to ? selectedBrush : null;
    }
}