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

namespace AvaloniaUI.Pages;

public partial class HomePage : UserControl
{
    public const int PROPERTY_DEFAULT_HEIGHT = 30;
    public const int PROPERTY_DEFAULT_FONT_SIZE = 12;

    public const int ENTRY_SIZE = 150;

    private bool isSetup = false;
    private HomePage_SidePanel wallpaperSettings;

    public HomePage()
    {
        InitializeComponent();

        wallpaperSettings = Sidebar.AddSubContainer<HomePage_SidePanel>();
        ItemViewer.Setup(Sidebar.Setup(
            new Common_Sidebar.ActVars
            {
                label = "Apply Wallpaper",
                callback = SetWallpaper
            },
            new Common_Sidebar.ActVars
            {
                label = "Browse",
                callback = BrowseToFolder
            },
            new Common_Sidebar.ActVars
            {
                label = "Reset",
                callback = ResetWallpaperOptions
            }
        ), Filters, FetchEntries);
    }

    public async void LoadPage(bool force = false)
    {
        if (isSetup && !force)
            return;

        isSetup = true;

        await WorkshopManager.RefreshLocalEntries();
        Filters.DrawTags(WorkshopManager.GetAllTags());

        await ItemViewer.Reset();
    }

    private async Task SetWallpaper()
    {
        if (ItemViewer.currentlySelectedWallpaper == null || !WorkshopManager.TryGetWallpaperEntry(ItemViewer.currentlySelectedWallpaper.Value, out WorkshopEntry? entry) || entry == null)
            return;

        await wallpaperSettings.SaveWallpaperOptions(ItemViewer.currentlySelectedWallpaper.Value);
        await WallpaperEngine.SetWallpaper(ItemViewer.currentlySelectedWallpaper.Value);
    }

    private Task BrowseToFolder()
    {
        if (ItemViewer.currentlySelectedWallpaper == null || !WorkshopManager.TryGetWallpaperEntry(ItemViewer.currentlySelectedWallpaper.Value, out WorkshopEntry? entry) || entry == null)
            return Task.CompletedTask;

        new Process() { StartInfo = new ProcessStartInfo { FileName = "xdg-open", Arguments = entry.path, UseShellExecute = false } }.Start();
        return Task.CompletedTask;
    }

    private async Task ResetWallpaperOptions()
    {
        if (ItemViewer.currentlySelectedWallpaper == null || !WorkshopManager.TryGetWallpaperEntry(ItemViewer.currentlySelectedWallpaper.Value, out WorkshopEntry? entry) || entry == null)
            return;

        await Database_Manager.Delete<dbo_WallpaperSettings>(SQLFilter.Equal(nameof(dbo_WallpaperSettings.wallpaperId), entry!.getId));
        await Sidebar.Draw(entry);
    }

    private Task<DataFetchResponse> FetchEntries(DataFetchRequest req)
    {
        return Task.FromResult(new DataFetchResponse()
        {
            entries = WorkshopManager.GetCachedWallpaperEntries(req.textFilter, req.tags, req.skip, req.take)
        });

    }
}