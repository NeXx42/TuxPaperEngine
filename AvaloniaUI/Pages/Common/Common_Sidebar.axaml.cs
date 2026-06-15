using System;
using System.Threading.Tasks;
using AnimatedImage.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.Utils;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Pages.Common;

public interface ISidebarContent
{
    public Task OnSelectWallpaper(Common_Sidebar master, IWorkshopEntry? entry);
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

        //SteamCMDManager.onDownloadChange += UpdateDownloadStatus;
        _ = Draw(null);
    }

    public Common_Sidebar Setup<T>() where T : Control, ISidebarContent
    {
        T control = Activator.CreateInstance<T>();

        content = control;
        container.Children.Add(control);

        btn_Act2.IsVisible = false;
        btn_Act3.IsVisible = false;

        return this;
    }
    public void Open()
    {
        scroll_Content.ScrollToHome();
        ImageBehavior.SetAnimatedSource(img_SidePanel_Icon, null!);
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
        DrawTags(entry.getTags);

        AnimatedImageSource? img = await ImageFetcher.GetIcon(entry!);
        ImageBehavior.SetAnimatedSource(img_SidePanel_Icon, img ?? null!);

        await (content?.OnSelectWallpaper(this, entry) ?? Task.CompletedTask);
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