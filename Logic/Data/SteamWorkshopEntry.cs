namespace Logic.Data;

public class SteamWorkshopEntry : IWorkshopEntry
{
    public required long id;
    public string? name;
    public string? imgUrl;
    public string[]? tags;

    public long getId => id;
    public string? getTitle => name;
    public string? getIconPath => imgUrl;

    public string[]? getTags => tags;

}
