using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace AvaloniaUI.Pages.Common;

public interface IFilterHandler
{
    public void Bind(Action onRefilter, params string[] ordering);
    public void DrawTags(string[] tags);

    public int GetOrder();
    public string? GetTextFilter();
    public string? GetSelectedResolution();
    public HashSet<string> GetTagFilter();
}

public partial class FilterContainer : UserControl, IFilterHandler
{
    private bool inInit;
    private Action? refilterRequest;

    private Common_Tag[]? tagsUI;
    private Border[]? orderBorders;

    private int currentOrderBy
    {
        set
        {
            if (m_currentOrderBy == value)
                return;

            m_currentOrderBy = value;

            for (int i = 0; i < (orderBorders?.Length ?? 0); i++)
            {
                if (i != m_currentOrderBy)
                    orderBorders?[i]?.Classes.Clear();
                else
                    orderBorders?[i]?.Classes.Add("Selected");
            }

            RequestRefresh();
        }
        get => m_currentOrderBy;
    }
    private int m_currentOrderBy = -1;

    public FilterContainer()
    {
        inInit = true;
        InitializeComponent();

        inp_TxtFilter.KeyUp += (_, __) => RequestRefresh();
        inp_Dropdown.SelectionChanged += (_, __) => RequestRefresh();
    }

    public void Bind(Action onRefilter, params string[] ordering)
    {
        this.refilterRequest = onRefilter;

        orderBorders = new Border[ordering.Length];
        cont_Ordering.Children.Clear();

        for (int i = 0; i < orderBorders.Length; i++)
        {
            Border ctrl = new Border()
            {
                Child = new Label()
                {
                    Content = ordering[i]
                },
            };

            int temp = i;
            ctrl.PointerPressed += (_, __) => currentOrderBy = temp;

            cont_Ordering.Children.Add(ctrl);
            orderBorders[i] = ctrl;
        }

        currentOrderBy = 0;
        inInit = false;
    }

    public void DrawTags(string[] tags)
    {
        inInit = true;
        tagsUI = new Common_Tag[tags.Length];

        for (int i = 0; i < tags.Length; i++)
        {
            var ui = new Common_Tag()
            {
                Height = 26
            };
            ui.Draw(tags[i], false, RequestRefresh);

            container_tags.Children.Add(ui);
            tagsUI[i] = ui;
        }

        inInit = false;
    }

    public void DrawDropdowns(string[] resolutions)
    {
        inInit = true;
        inp_Dropdown.IsVisible = resolutions.Length > 0;

        if (resolutions.Length > 0)
        {
            inp_Dropdown.ItemsSource = (string[])["Everything", .. resolutions];
            inp_Dropdown.SelectedIndex = 0;
        }

        inInit = false;
    }

    public int GetOrder() => currentOrderBy;
    public string? GetTextFilter() => inp_TxtFilter.Text;
    public string? GetSelectedResolution() => inp_Dropdown.SelectedIndex == 0 ? null : inp_Dropdown.SelectedValue?.ToString();

    public HashSet<string> GetTagFilter() => tagsUI?.Where(x => x.isSelected).Select(x => x.tag ?? string.Empty).ToHashSet() ?? new HashSet<string>();

    private void RequestRefresh()
    {
        if (inInit)
            return;

        refilterRequest?.Invoke();
    }
}