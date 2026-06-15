namespace Logic.Data;

public interface IWorkshopEntry
{
    public long getId { get; }

    public string? getGifPath { get; }
    public string? getIconPath { get; }
    public string? getTitle { get; }

    public string[]? getTags { get; }
}
