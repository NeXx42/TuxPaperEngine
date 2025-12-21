using System.Text.Json;

namespace Logic.Data;

public class WorkshopEntry
{
    public long id;
    public string path;
    public string? iconPath;
    public string? title;
    public string[]? tags;
    public string? type;

    public WorkshopEntry(long id, string path)
    {
        this.id = id;
        this.path = path;
    }

    public async Task<bool> Decode()
    {
        string projectJson = Path.Combine(path, "project.json");

        if (!File.Exists(projectJson))
            return false;

        using StreamReader reader = new StreamReader(projectJson);
        string json = await reader.ReadToEndAsync();

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        title = root.GetProperty("title").GetString();
        iconPath = root.GetProperty("preview").GetString();

        return true;
    }
}
