using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public abstract class ItemFormatterBase : UserControl
{
    protected Func<DataFetchRequest, Task<DataFetchResponse>>? dataFetcher;

    protected Common_Sidebar? sidebar;
    protected IFilterHandler? filter;

    public abstract long? currentlySelectedWallpaper { get; set; }

    public virtual void Setup(Common_Sidebar sidebar, IFilterHandler filter, Func<DataFetchRequest, Task<DataFetchResponse>> dataFetcher)
    {
        this.dataFetcher = dataFetcher;

        this.filter = filter;
        this.sidebar = sidebar;

        filter?.Bind(() => Draw(false, true));
    }

    public abstract Task Reset();
    public abstract Task Draw(bool additive, bool resetPaging);

    public virtual async Task SelectWallpaper(IWorkshopEntry entry)
    {
        if (entry.getId == currentlySelectedWallpaper)
            return;

        currentlySelectedWallpaper = entry.getId;
        await (sidebar?.Draw(entry) ?? Task.CompletedTask);
    }
}