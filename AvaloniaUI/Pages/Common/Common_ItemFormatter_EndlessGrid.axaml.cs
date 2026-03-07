using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public partial class Common_ItemFormatter_EndlessGrid : ItemFormatterBase
{
    public const int ENTRIES_PER_PAGE = 45;
    public const int ENTRY_SIZE = 160;

    private int loadedPages = 0;
    private Dictionary<long, Common_Wallpaper> cachedWallpaperUI = new Dictionary<long, Common_Wallpaper>();

    public override long? currentlySelectedWallpaper
    {
        set
        {
            if (m_currentlySelectedWallpaper.HasValue && cachedWallpaperUI.TryGetValue(m_currentlySelectedWallpaper.Value, out Common_Wallpaper? ui))
            {
                ui.ToggleSelection(false);
            }

            m_currentlySelectedWallpaper = value;

            if (m_currentlySelectedWallpaper.HasValue && cachedWallpaperUI.TryGetValue(m_currentlySelectedWallpaper.Value, out ui))
            {
                ui.ToggleSelection(true);
            }
        }
        get => m_currentlySelectedWallpaper;
    }
    private long? m_currentlySelectedWallpaper;


    public Common_ItemFormatter_EndlessGrid()
    {
        InitializeComponent();
        btn_LoadMore.RegisterClick(LoadExtraEntries);
    }

    public override async Task Reset()
    {
        await MainWindow.AsyncLoad(WorkshopManager.RefreshLocalEntries);
        await Draw(false, true);

        await base.Reset();
    }

    public override async Task Draw(bool additive, bool resetPaging)
    {
        if (resetPaging)
        {
            loadedPages = 0;
            currentlySelectedWallpaper = null;
        }

        DataFetchResponse res = await dataFetcher!(new DataFetchRequest()
        {
            skip = loadedPages * ENTRIES_PER_PAGE,
            take = ENTRIES_PER_PAGE,

            textFilter = filter?.GetTextFilter(),
            tags = filter?.GetTagFilter(),
        });

        if (!additive)
            grid_Content_Container.Children.Clear();

        for (int i = 0; i < res.entries!.Length; i++)
        {
            Common_Wallpaper ui = GetWallpaperUI(res.entries![i]);
            ui.Width = ENTRY_SIZE;
            ui.Height = ENTRY_SIZE;

            ui.StartDraw(res.entries![i], this);
            grid_Content_Container.Children.Add(ui);
        }

        int maxPages = (int)Math.Ceiling(WorkshopManager.GetWallpaperCount() / (float)ENTRIES_PER_PAGE);
        btn_LoadMore.IsVisible = loadedPages < maxPages - 1;

        Common_Wallpaper GetWallpaperUI(IWorkshopEntry entry)
        {
            if (cachedWallpaperUI.TryGetValue(entry.getId, out Common_Wallpaper? cached))
                return cached!;

            Common_Wallpaper wallpaperEntry = new Common_Wallpaper();
            wallpaperEntry.Height = HomePage.ENTRY_SIZE;
            wallpaperEntry.Width = HomePage.ENTRY_SIZE;

            cachedWallpaperUI.Add(entry.getId, wallpaperEntry);
            return wallpaperEntry;
        }
    }

    private async Task LoadExtraEntries()
    {
        loadedPages++;
        await Draw(true, false);
    }
}