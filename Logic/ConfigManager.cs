using CSharpSqliteORM;
using Logic.db;

namespace Logic;

public static class ConfigManager
{
    public enum ConfigKeys
    {
        ExecutableLocation,
        WorkshopLocations,
    }

    public static string[]? localWorkshopLocations { private set; get; }
    public static Screen[]? screens { private set; get; }


    public static async Task Init()
    {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LinuxWallpaperEngineGUI.db");
        await Database_Manager.Init(dbPath);

        await LoadWorkshopLocations();
    }

    private static async Task LoadWorkshopLocations()
    {
        dbo_Config[] entries = await GetConfigValues(ConfigKeys.WorkshopLocations);

        if (entries.Length == 0)
        {
            localWorkshopLocations = [Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/workshop/content/431960")];
        }
        else
        {
            localWorkshopLocations = entries.Select(x => x.value!).ToArray();
        }
    }

    public static void RegisterDisplays(Screen[] screens) => ConfigManager.screens = screens;




    public static async Task<dbo_Config?> GetConfigValue(ConfigKeys key) => (await GetConfigValues(key)).FirstOrDefault();
    public static async Task<dbo_Config[]> GetConfigValues(ConfigKeys key) => await Database_Manager.GetItems<dbo_Config>(SQLFilter.Equal(nameof(dbo_Config.key), key.ToString()));




    public struct Screen
    {
        public string screenName;
        public int priority;
    }
}
