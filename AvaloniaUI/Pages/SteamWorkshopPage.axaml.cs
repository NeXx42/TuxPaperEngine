using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Pages._WorkshopPage;
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
        ItemViewer.Setup(Sidebar.Setup<SteamWorkshopPage_Sidebar>(), Filters, FetchEntries, "Trending", "New", "Last Updated", "Most Subscribed");
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
}