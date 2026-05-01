using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Logic;
using Logic.Data;
using Logic.db;

namespace AvaloniaUI.Common;

public partial class Common_EngineStatus : UserControl
{
    public Common_EngineStatus()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            WallpaperEngine.OnStatusChange += () => _ = RefreshStatus();
            _ = RefreshStatus();
        }
    }

    public async Task RefreshStatus()
    {
        try
        {
            await WallpaperEngine.TryFindExecutableLocation();
            string? activeWallpaper = null;

            try
            {
                dbo_Config? active = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.LastSetWallpaper);

                if (active != null && long.TryParse(active.value, out long id))
                {
                    if (WorkshopManager.TryGetWallpaperEntry(id, out WorkshopEntry? item) && item != null)
                    {
                        await item.DecodeBasic();
                        activeWallpaper = item.title;
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(activeWallpaper))
            {
                lbl_Active.Content = "";
                inp_Status.Background = new SolidColorBrush(Color.Parse("#F2C94C"));
            }
            else
            {
                lbl_Active.Content = activeWallpaper;
                inp_Status.Background = new SolidColorBrush(Color.Parse("#27AE60"));
            }
        }
        catch
        {
            lbl_Active.Content = "No engine installed";
            inp_Status.Background = new SolidColorBrush(Color.Parse("#EB5757"));
        }
    }
}