using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaUI.Pages._HomePage;
using AvaloniaUI.Pages.Common;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages._WorkshopPage;

public partial class SteamWorkshopPage_Sidebar : UserControl, ISidebarContent
{
    private Common_Sidebar? master;

    private IWorkshopEntry? selected;
    private HomePage_SidePanel homePage_SidePanel;

    public SteamWorkshopPage_Sidebar()
    {
        InitializeComponent();

        homePage_SidePanel = new HomePage_SidePanel();
        cont.Children.Add(homePage_SidePanel);

        SteamCMDManager.onDownloadChange += (a, b) => _ = HandleDownloadStatusChange(a, b);
    }

    public async Task OnSelectWallpaper(Common_Sidebar master, IWorkshopEntry? entry)
    {
        selected = entry;
        this.master = master;

        if (WorkshopManager.TryGetWallpaperEntry(entry?.getId, out WorkshopEntry cachedVersion))
        {
            homePage_SidePanel.IsVisible = true;
            await homePage_SidePanel.OnSelectWallpaper(master, cachedVersion);
            return;
        }

        homePage_SidePanel.IsVisible = false;

        master.btn_Act.Label = SteamCMDManager.GetActiveStatus(entry!.getId)?.ToString() ?? "Download";
        master.btn_Act.ClearCallback();
        master.btn_Act.RegisterClick(DownloadWallpaper, DownloadStatus.Waiting.ToString());

        master.btn_Act2.Label = "Browse";
        master.btn_Act2.ClearCallback();
        master.btn_Act2.IsVisible = true;
        master.btn_Act2.RegisterClick(BrowseToWallpaper);

        master.btn_Act3.IsVisible = false;
    }



    private async Task DownloadWallpaper()
    {
        if (selected == null)
            return;

        await SteamCMDManager.DownloadAsset(selected.getId);
    }

    private async Task BrowseToWallpaper()
    {
        if (selected == null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://steamcommunity.com/sharedfiles/filedetails/?id={selected.getId}",
            UseShellExecute = true
        });
    }

    private async Task HandleDownloadStatusChange(long id, DownloadStatus status)
    {
        if (id != selected?.getId)
            return;

        Dispatcher.UIThread.Post(async () =>
        {
            if (status == DownloadStatus.Finished)
            {
                await Task.Delay(100); // wait for workshop to recache this as its on the same event

                if (WorkshopManager.TryGetWallpaperEntry(id, out WorkshopEntry entry))
                    await OnSelectWallpaper(master!, entry);
            }
            else
            {
                master!.btn_Act.Label = status.ToString();
            }

        });
    }
}