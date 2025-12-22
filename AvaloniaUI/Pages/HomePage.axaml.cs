using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaUI.Pages._HomePage;
using AvaloniaUI.Pages._HomePage.WallpaperProperties;
using AvaloniaUI.Utils;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages;

public partial class HomePage : UserControl
{
    public const string DEFAULT_SCALING_NAME = "Default";

    public const int ENTRY_SIZE = 150;
    public const int ENTRIES_PER_PAGE = 50;

    private int loadedPages = 0;
    private string? cachedNameFilter;

    private Dictionary<long, HomePage_Wallpaper> cachedWallpaperUI = new Dictionary<long, HomePage_Wallpaper>();

    private long? currentlySelectedWallpaper
    {
        set
        {
            if (m_currentlySelectedWallpaper.HasValue && cachedWallpaperUI.TryGetValue(m_currentlySelectedWallpaper.Value, out HomePage_Wallpaper? ui))
            {
                ui.ToggleSelection(false);
            }

            m_currentlySelectedWallpaper = value;
            grid_SidePanel.IsVisible = m_currentlySelectedWallpaper.HasValue;

            if (m_currentlySelectedWallpaper.HasValue && cachedWallpaperUI.TryGetValue(m_currentlySelectedWallpaper.Value, out ui))
            {
                ui.ToggleSelection(true);
            }
        }
        get => m_currentlySelectedWallpaper;
    }
    private long? m_currentlySelectedWallpaper;

    public HomePage()
    {
        InitializeComponent();
        SetupBasicOptions();

        inp_NameSearch.KeyUp += (_, __) => UpdateFilter();

        if (!Design.IsDesignMode)
        {
            DrawWallpapers(false);
        }
    }

    private void SetupBasicOptions()
    {
        string[] options = [DEFAULT_SCALING_NAME, .. System.Enum.GetNames(typeof(WallpaperSetter.ScalingOptions))];
        inp_SidePanel_Scaling.SelectedIndex = 0;
        inp_SidePanel_Scaling.ItemsSource = options;

        options = System.Enum.GetNames(typeof(WallpaperSetter.ClampOptions));
        inp_SidePanel_Clamp.SelectedIndex = 0;
        inp_SidePanel_Clamp.ItemsSource = options;

        btn_SidePanel_Set.RegisterClick(SetWallpaper);

        inp_SidePanel_OffsetY.Minimum = -1;
        inp_SidePanel_OffsetY.Maximum = 1;

        inp_SidePanel_OffsetX.Minimum = -1;
        inp_SidePanel_OffsetX.Maximum = 1;

        btn_LoadMore.RegisterClick(LoadExtraEntries);
    }

    private async void DrawWallpapers(bool additive)
    {
        WorkshopEntry[] wallpapers = WorkshopManager.GetCachedWallpaperEntries(inp_NameSearch.Text, null, loadedPages * ENTRIES_PER_PAGE, ENTRIES_PER_PAGE);

        if (!additive)
            grid_Content_Container.Children.Clear();

        for (int i = 0; i < wallpapers.Length; i++)
        {
            HomePage_Wallpaper ui = GetWallpaperUI(wallpapers[i]);

            ui.StartDraw(wallpapers[i], this);
            grid_Content_Container.Children.Add(ui);
        }

        int maxPages = (int)Math.Ceiling(WorkshopManager.GetWallpaperCount() / (float)ENTRIES_PER_PAGE);
        btn_LoadMore.IsVisible = loadedPages < maxPages - 1;

        HomePage_Wallpaper GetWallpaperUI(WorkshopEntry entry)
        {
            if (cachedWallpaperUI.TryGetValue(entry.id, out HomePage_Wallpaper? cached))
                return cached!;

            HomePage_Wallpaper wallpaperEntry = new HomePage_Wallpaper();
            wallpaperEntry.Height = ENTRY_SIZE;
            wallpaperEntry.Width = ENTRY_SIZE;

            cachedWallpaperUI.Add(entry.id, wallpaperEntry);
            return wallpaperEntry;
        }
    }

    public async void SelectWallpaper(long id)
    {
        currentlySelectedWallpaper = id;

        img_SidePanel_Icon.Background = null;
        lbl_SidePanel_Title.Content = "";

        if (!WorkshopManager.TryGetWallpaperEntry(id, out WorkshopEntry? entry))
            return;

        await entry!.Decode();
        lbl_SidePanel_Title.Content = entry.title;

        DrawWallpaperProperties(entry.properties);

        ImageBrush? brush = await ImageFetcher.GetIcon(id);
        img_SidePanel_Icon.Background = brush;
    }

    private void DrawWallpaperProperties(WorkshopEntry.Properties[]? props)
    {
        cont_SidePanel_CustomProperties.Children.Clear();

        if (props == null)
            return;

        IWallpaperProperty? ui;

        foreach (WorkshopEntry.Properties prop in props)
        {
            ui = GetWallpaperPropertyUI(prop.type ?? WorkshopEntry.PropertyType.INVALID);

            if (ui == null)
                continue;

            ui.Draw(prop);
            cont_SidePanel_CustomProperties.Children.Add((ui as UserControl)!);
        }
    }

    private IWallpaperProperty? GetWallpaperPropertyUI(WorkshopEntry.PropertyType type)
    {
        switch (type)
        {
            case WorkshopEntry.PropertyType.colour: return new HomePage_WallpaperProperties_Colour();
                //case WorkshopEntry.PropertyType.boolean: return new HomePage_WallpaperProperties_Bool();
                //case WorkshopEntry.PropertyType.combo: return new HomePage_WallpaperProperties_Combo();
                //case WorkshopEntry.PropertyType.text_input: return new HomePage_WallpaperProperties_TextInput();
                //case WorkshopEntry.PropertyType.scene_texture: return new HomePage_WallpaperProperties_SceneTexture();
        }

        return null;
    }

    private async Task SetWallpaper()
    {
        if (currentlySelectedWallpaper == null || !WorkshopManager.TryGetWallpaperEntry(currentlySelectedWallpaper.Value, out WorkshopEntry? entry))
            return;

        WallpaperSetter.WallpaperOptions options = new WallpaperSetter.WallpaperOptions();
        options.scalingOption = inp_SidePanel_Scaling.SelectedIndex - 1 >= 0 ? (WallpaperSetter.ScalingOptions)(inp_SidePanel_Scaling.SelectedIndex - 1) : null;
        options.clampOptions = (WallpaperSetter.ClampOptions)inp_SidePanel_Clamp.SelectedIndex;

        options.screens = WallpaperSetter.WorkOutScreenOffsets((float)inp_SidePanel_OffsetX.Value, (float)inp_SidePanel_OffsetY.Value);
        await WallpaperSetter.SetWallpaper(entry!.path, options);
    }

    private void LoadExtraEntries()
    {
        loadedPages++;
        DrawWallpapers(true);
    }

    private void UpdateFilter()
    {
        if (cachedNameFilter == inp_NameSearch.Text)
        {
            return;
        }

        cachedNameFilter = inp_NameSearch.Text;

        loadedPages = 0;
        DrawWallpapers(false);
    }
}