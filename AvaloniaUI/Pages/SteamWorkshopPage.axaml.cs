using System;
using System.Collections.Generic;
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

    public SteamWorkshopPage()
    {
        InitializeComponent();
        ItemViewer.Setup(Sidebar.Setup(
            new Common_Sidebar.ActVars
            {
                label = "Apply Wallpaper",
                callback = DownloadWallpaper
            },
            new Common_Sidebar.ActVars
            {
                label = "Browse",
                callback = BrowseToFolder
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
        return await SteamWorkshopManager.FetchItems(req);
    }


    private async Task DownloadWallpaper()
    {
        if (!ItemViewer.currentlySelectedWallpaper.HasValue)
            return;


    }

    private async Task BrowseToFolder()
    {
        if (!ItemViewer.currentlySelectedWallpaper.HasValue)
            return;

    }
}