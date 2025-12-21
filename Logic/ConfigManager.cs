using CSharpSqliteORM;
using Gdk;
using Logic.db;

namespace Logic;

public static class ConfigManager
{
    public enum ConfigKeys
    {
        WorkshopLocations,
    }

    public static string[]? localWorkshopLocations;
    public static Screen[]? screens;


    public static async Task Init()
    {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LinuxWallpaperEngineGUI.db");
        await Database_Manager.Init(dbPath);

        await LoadWorkshopLocations();

        //FindMonitors();
    }

    private static async Task LoadWorkshopLocations()
    {
        dbo_Config[] entries = await Database_Manager.GetItems<dbo_Config>(SQLFilter.Equal(nameof(dbo_Config.key), ConfigKeys.WorkshopLocations.ToString()));

        if (entries.Length == 0)
        {
            localWorkshopLocations = [Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/workshop/content/431960")];
        }
        else
        {
            localWorkshopLocations = entries.Select(x => x.value!).ToArray();
        }
    }

    private static void FindMonitors()
    {

        Display display = Display.Default;
        int monitorCount = display.NMonitors;

        screens = new Screen[monitorCount];

        for (int i = 0; i < monitorCount; i++)
        {
            screens[i] = new Screen(display.GetMonitor(i));
        }
    }


    public struct Screen
    {
        public string screenName;
        public float offsetX;
        public float offsetY;

        public Screen(Gdk.Monitor monitor)
        {
            screenName = monitor.Display.Name;
        }
    }
}
