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
    public Properties[]? properties;

    private int decodeStatus;

    public WorkshopEntry(long id, string path)
    {
        this.id = id;
        this.path = path;

        decodeStatus = 0;
    }

    private async Task<JsonDocument?> ReadJson()
    {
        string projectPath = Path.Combine(path, "project.json");

        if (!File.Exists(projectPath))
            return null;

        using StreamReader reader = new StreamReader(projectPath);
        string json = await reader.ReadToEndAsync();

        return JsonDocument.Parse(json);
    }

    public async Task<bool> DecodeBasic(JsonDocument? doc = null)
    {
        if (decodeStatus >= 1)
            return true;

        decodeStatus = 1;

        doc ??= await ReadJson();
        if (doc == null) return false;

        title = doc.RootElement.GetProperty("title").GetString();
        iconPath = doc.RootElement.GetProperty("preview").GetString();
        tags = doc.RootElement.GetProperty("tags").Deserialize<string[]>();

        return true;
    }

    public async Task<bool> Decode(JsonDocument? doc = null)
    {
        if (decodeStatus >= 2)
            return true;

        doc ??= await ReadJson();

        if (decodeStatus < 1)
            await DecodeBasic(doc);

        decodeStatus = 2;
        if (doc == null) return false;

        if (doc.RootElement.TryGetProperty("general", out JsonElement general))
        {
            JsonElement properties = general.GetProperty("properties");

            List<Properties> temp = new List<Properties>();

            foreach (JsonProperty prop in properties.EnumerateObject())
                temp.Add(new Properties(prop));

            this.properties = temp.ToArray();
        }

        return true;
    }


    public class Properties
    {
        public string? propertyName;
        public int? order;
        public string? text;
        public PropertyType? type;
        public string? value;

        public Properties(JsonProperty parent)
        {
            propertyName = parent.Name;

            try
            {
                order = parent.Value.GetProperty("order").GetInt32();
                text = parent.Value.GetProperty("text").GetString();
                value = parent.Value.GetProperty("value").GetString();

                type = DeserializeType(parent.Value.GetProperty("type").GetString());
            }
            catch { }
        }

        private PropertyType DeserializeType(string? type)
        {
            switch (type)
            {
                case "color": return PropertyType.colour;
                case "bool": return PropertyType.boolean;
                case "combo": return PropertyType.combo;
                case "textinput": return PropertyType.text_input;
                case "scenetexture": return PropertyType.scene_texture;

                default: return PropertyType.INVALID;
            }
        }
    }

    public enum PropertyType
    {
        INVALID,
        colour,
        boolean,
        combo,
        text_input,
        scene_texture,
    }
}
