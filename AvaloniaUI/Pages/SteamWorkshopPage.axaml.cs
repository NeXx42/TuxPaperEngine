using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Pages.Common;
using AvaloniaUI.Utils;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages;

public partial class SteamWorkshopPage : UserControl
{
    private bool isSetup = false;
    private bool hasValidFilters = false;

    public SteamWorkshopPage()
    {
        InitializeComponent();
        ItemViewer.Setup(Sidebar.Setup(
            new Common_Sidebar.ActVars
            {
                label = "Download Wallpaper",
                callback = DownloadWallpaper
            },
            new Common_Sidebar.ActVars
            {
                label = "Browse",
                callback = BrowseToWallpaper
            }
        ), Filters, FetchEntries);
    }

    public async void LoadPage()
    {
        if (isSetup)
            return;

        isSetup = true;
        await ItemViewer.Reset();
    }

    private async Task<DataFetchResponse> FetchEntries(DataFetchRequest req)
    {
        if (!hasValidFilters)
        {
            await Filters.EngineStatus.RefreshStatus();

            hasValidFilters = true;
            var res = await SteamWorkshopManager.FetchItems(req, true);

            Filters.DrawTags(SteamWorkshopManager.getTags);

            return res;
        }

        return await SteamWorkshopManager.FetchItems(req, false);
    }


    private async Task DownloadWallpaper()
    {
        if (!ItemViewer.currentlySelectedWallpaper.HasValue)
            return;

        await SteamCMDManager.DownloadAsset(ItemViewer.currentlySelectedWallpaper.Value, MainWindow.getAuthenticationModal);
    }

    private async Task BrowseToWallpaper()
    {
        if (!ItemViewer.currentlySelectedWallpaper.HasValue)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://steamcommunity.com/sharedfiles/filedetails/?id={ItemViewer.currentlySelectedWallpaper.Value}",
            UseShellExecute = true
        });
    }
}