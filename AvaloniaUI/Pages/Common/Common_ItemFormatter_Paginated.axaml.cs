using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Common;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public partial class Common_ItemFormatter_Paginated : ItemFormatterBase
{
    public const int ENTRY_SIZE = 180;
    public const int ENTRIES_PER_PAGE = 30;

    private Common_Wallpaper[] generatedUI;
    private IWorkshopEntry[]? entries;

    private int currentPage = 0;
    private int[] currentPageNavigateOptions;

    public override long? currentlySelectedWallpaper
    {
        set
        {
            m_currentlySelectedWallpaper = value;

            for (int i = 0; i < generatedUI.Length; i++)
            {
                bool isSelected = i < entries?.Length && entries[i].getId == currentlySelectedWallpaper;
                generatedUI[i].ToggleSelection(isSelected);
            }
        }
        get => m_currentlySelectedWallpaper;
    }
    private long? m_currentlySelectedWallpaper;

    public IWorkshopEntry? GetItemByID(long id) => entries?.FirstOrDefault(x => x.getId == id);

    public Common_ItemFormatter_Paginated()
    {
        InitializeComponent();
        generatedUI = new Common_Wallpaper[ENTRIES_PER_PAGE];

        for (int i = 0; i < ENTRIES_PER_PAGE; i++)
        {
            var ui = new Common_Wallpaper();
            ui.Width = ENTRY_SIZE;
            ui.Height = ENTRY_SIZE;

            ui.IsVisible = false;

            generatedUI[i] = ui;
            grid_Content_Container.Children.Add(ui);
        }

        currentPageNavigateOptions = [0, 0, 0, 0, 0];

        btn_Page_1.RegisterClick(() => ChangePageNumber(0));
        btn_Page_2.RegisterClick(() => ChangePageNumber(1));
        btn_Page_3.RegisterClick(() => ChangePageNumber(2));
        btn_Page_4.RegisterClick(() => ChangePageNumber(3));
        btn_Page_5.RegisterClick(() => ChangePageNumber(4));
    }

    public override async Task Draw(bool additive, bool resetPaging)
    {
        if (resetPaging)
        {
            currentPage = 0;
        }

        grid_Content_Container.IsVisible = true;
        lbl_Content_NetworkResponse.IsVisible = false;

        foreach (Common_Wallpaper ui in generatedUI)
        {
            ui.IsVisible = true;
            ui.DrawSkeleton();
        }

        DataFetchResponse res = await dataFetcher!(new DataFetchRequest()
        {
            skip = currentPage,
            take = ENTRIES_PER_PAGE,

            textFilter = filter?.GetTextFilter(),
            tags = filter?.GetTagFilter()
        });

        entries = res.entries;

        if (entries != null)
        {
            for (int i = 0; i < generatedUI.Length; i++)
            {
                if (i < entries?.Length)
                {
                    generatedUI[i].IsVisible = true;
                    generatedUI[i].StartDraw(entries[i], this);
                }
                else
                {
                    generatedUI[i].IsVisible = false;
                }
            }
        }

        if (res.exception != null)
        {
            grid_Content_Container.IsVisible = false;
            lbl_Content_NetworkResponse.IsVisible = true;
            lbl_Content_NetworkResponse.Content = res.exception.Message;
        }
    }

    public override async Task Reset()
    {
        currentPage = 1;
        currentlySelectedWallpaper = null;

        await ChangePageNumber(0);
    }

    private async Task ChangePageNumber(int navigateIndex)
    {
        currentPage = currentPageNavigateOptions[navigateIndex];
        currentlySelectedWallpaper = null;

        int lowestPage = Math.Max(currentPage - 2, 1);
        int pageNum = lowestPage;

        Common_Button[] btns = [
            btn_Page_1,
            btn_Page_2,
            btn_Page_3,
            btn_Page_4,
            btn_Page_5,
        ];

        for (int i = 0; i < 5; i++)
        {
            currentPageNavigateOptions[i] = pageNum;
            btns[i].Label = pageNum.ToString();

            if (pageNum == currentPage)
            {
                btns[i].Classes.Add("Positive2");
            }
            else
            {
                btns[i].Classes.Remove("Positive2");
            }

            pageNum++;
        }

        await Draw(false, false);
    }

    private async void UpdateFilter(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        await ChangePageNumber(0);
    }
}