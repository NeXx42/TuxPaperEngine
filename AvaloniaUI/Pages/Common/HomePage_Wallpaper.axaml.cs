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
using Avalonia.Threading;
using AvaloniaUI.Pages.Common;
using AvaloniaUI.Utils;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public partial class Common_Wallpaper : UserControl
{
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
        cont_DownloadStatus.IsVisible = false;
        cont_ActiveWallpaperStatus.IsVisible = false;

        this.representing = null;

        lbl_Title.Content = string.Empty;
        img_Icon.Background = null;
    }

    public async void StartDraw(IWorkshopEntry entry, ItemFormatterBase master)
    {
        await (cancellationToken?.CancelAsync() ?? Task.CompletedTask);
        cancellationToken = new CancellationTokenSource();

        await InternalDraw(entry, cancellationToken.Token);

        RedrawStatus(SteamCMDManager.GetActiveStatus(entry.getId));
        UpdateActiveStatus(WallpaperEngine.getActiveWallpaper);

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
        if (to)
        {
            border.Classes.Add("Selected");
        }
        else
        {
            border.Classes.Remove("Selected");
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SteamCMDManager.onDownloadChange += UpdateDownloadStatus;
        WallpaperEngine.OnActiveWallpaperChange += UpdateActiveStatus;

        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SteamCMDManager.onDownloadChange -= UpdateDownloadStatus;
        WallpaperEngine.OnActiveWallpaperChange -= UpdateActiveStatus;

        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateDownloadStatus(long id, DownloadStatus status)
    {
        if (id != representing?.getId)
            return;

        Dispatcher.UIThread.Post(() => RedrawStatus(status));
    }

    private void RedrawStatus(DownloadStatus? status)
    {
        cont_DownloadStatus.IsVisible = (status ?? DownloadStatus.Finished) != DownloadStatus.Finished;

        if (cont_DownloadStatus.IsVisible)
        {
            lbl_DownloadStatus.Content = status!.ToString();
        }
    }

    private void UpdateActiveStatus(long? id)
    {
        if (id != representing?.getId)
        {
            cont_ActiveWallpaperStatus.IsVisible = false;
            return;
        }

        cont_ActiveWallpaperStatus.IsVisible = true;
    }
}