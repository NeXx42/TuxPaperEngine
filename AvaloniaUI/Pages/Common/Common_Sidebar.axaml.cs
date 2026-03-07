using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Utils;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public interface ISidebarContent
{
    public Task OnSelectWallpaper(IWorkshopEntry entry);
}

public partial class Common_Sidebar : UserControl
{
    private ISidebarContent? content;

    public struct ActVars
    {
        public string label;
        public Func<Task> callback;
    }

    public Common_Sidebar()
    {
        InitializeComponent();
        _ = Draw(null);
    }

    public Common_Sidebar Setup(ActVars mainAct, params ActVars[] subActs)
    {
        btn_Act2.IsVisible = false;
        btn_Act3.IsVisible = false;

        btn_Act.Label = mainAct.label;
        btn_Act.RegisterClick(mainAct.callback);

        if (subActs.Length >= 1)
        {
            btn_Act2.IsVisible = true;
            btn_Act2.Label = subActs[0].label;
            btn_Act2.RegisterClick(subActs[0].callback);
        }

        if (subActs.Length >= 2)
        {
            btn_Act3.IsVisible = true;
            btn_Act3.Label = subActs[1].label;
            btn_Act3.RegisterClick(subActs[1].callback);
        }

        return this;
    }

    public T AddSubContainer<T>() where T : UserControl, ISidebarContent
    {
        T control = Activator.CreateInstance<T>();

        content = control;
        container.Children.Add(control);

        return control;
    }

    public void Open()
    {
        scroll_Content.ScrollToHome();
        img_SidePanel_Icon.Background = null;
        lbl_SidePanel_Title.Content = "";
    }

    public async Task Draw(IWorkshopEntry? entry)
    {
        if (entry == null)
        {
            cont_NoContent.IsVisible = true;
            cont_Content.IsVisible = false;
            return;
        }

        cont_NoContent.IsVisible = false;
        cont_Content.IsVisible = true;

        lbl_SidePanel_Title.Content = entry.getTitle;
        img_SidePanel_Icon.Background = await ImageFetcher.GetIcon(entry!);
        DrawTags(entry.getTags);

        await (content?.OnSelectWallpaper(entry) ?? Task.CompletedTask);
    }

    private void DrawTags(string[]? tags)
    {
        container_Tags.Children.Clear();

        if (tags == null)
            return;

        foreach (string tag in tags)
        {
            Common_Tag tagUI = new Common_Tag();
            tagUI.Draw(tag, false);

            container_Tags.Children.Add(tagUI);
        }
    }
}