using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaUI.Pages._HomePage;
using AvaloniaUI.Pages._HomePage.WallpaperProperties;
using AvaloniaUI.Pages.Common;
using AvaloniaUI.Utils;
using CSharpSqliteORM;
using Logic;
using Logic.Data;
using Logic.Database;
using Logic.Enums;

namespace AvaloniaUI.Pages;

public partial class HomePage : UserControl
{
    public const int PROPERTY_DEFAULT_HEIGHT = 30;
    public const int PROPERTY_DEFAULT_FONT_SIZE = 12;

    public const int ENTRY_SIZE = 150;
    private bool isSetup = false;

    public HomePage()
    {
        InitializeComponent();
        ItemViewer.Setup(Sidebar.Setup<HomePage_SidePanel>(), Filters, FetchEntries, ["Name", "Download Date", "Last Used"]);
    }

    public async void LoadPage(bool force = false)
    {
        if (isSetup && !force)
            return;

        isSetup = true;

        await WorkshopManager.RefreshLocalEntries();
        await Filters.EngineStatus.RefreshStatus();

        Filters.DrawTags(WorkshopManager.GetAllTags());
        await ItemViewer.Reset();
    }

    private Task<DataFetchResponse> FetchEntries(DataFetchRequest req)
    {
        return Task.FromResult(new DataFetchResponse()
        {
            entries = WorkshopManager.GetCachedWallpaperEntries(req.textFilter, req.tags, req.skip, req.take, (DownloadedWallpaperOrdering)req.orderId)
        });

    }
}