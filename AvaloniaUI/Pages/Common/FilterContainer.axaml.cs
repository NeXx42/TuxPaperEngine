using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace AvaloniaUI.Pages.Common;

public interface IFilterHandler
{
    public void Bind(Action onRefilter);
    public void DrawTags(string[] tags);

    public string? GetTextFilter();
    public HashSet<string> GetTagFilter();
}

public partial class FilterContainer : UserControl, IFilterHandler
{
    private Action? refilterRequest;
    private Common_Tag[]? tagsUI;

    public FilterContainer()
    {
        InitializeComponent();

        inp_TxtFilter.KeyUp += (_, __) => refilterRequest?.Invoke();
    }

    public void Bind(Action onRefilter)
    {
        this.refilterRequest = onRefilter;
    }

    public void DrawTags(string[] tags)
    {
        tagsUI = new Common_Tag[tags.Length];

        for (int i = 0; i < tags.Length; i++)
        {
            var ui = new Common_Tag()
            {
                Height = 26
            };
            ui.Draw(tags[i], false, () => refilterRequest?.Invoke());

            container_tags.Children.Add(ui);
            tagsUI[i] = ui;
        }
    }

    public HashSet<string> GetTagFilter() => tagsUI?.Where(x => x.isSelected).Select(x => x.tag ?? string.Empty).ToHashSet() ?? new HashSet<string>();
    public string? GetTextFilter() => inp_TxtFilter.Text;
}