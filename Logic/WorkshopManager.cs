using System.Collections.Concurrent;
using Logic.Data;
using Logic.Enums;

namespace Logic;

public static class WorkshopManager
{
    private static int? filterEntriesCount;
    private static ConcurrentDictionary<long, WorkshopEntry> cachedEntries = new ConcurrentDictionary<long, WorkshopEntry>();

    public static int GetWallpaperCount() => filterEntriesCount ?? 0;

    public static string[] GetAllTags()
    {
        SteamCMDManager.onDownloadChange += (a, b) => _ = OnSteamDownloadComplete(a, b);

        HashSet<string> existingTags = new HashSet<string>();

        foreach (var entry in cachedEntries.Values)
        {
            foreach (string tag in entry?.tags ?? [])
            {
                if (!existingTags.Contains(tag))
                    existingTags.Add(tag);
            }
        }

        return existingTags.Order().ToArray();
    }

    public static async Task RefreshLocalEntries()
    {
        List<DirectoryInfo> folders = new List<DirectoryInfo>();

        foreach (string dir in ConfigManager.localWorkshopLocations!)
        {
            if (!Directory.Exists(dir))
                continue;

            string[] entries = Directory.GetDirectories(dir);
            foreach (string wallpaper in entries)
            {
                folders.Add(new DirectoryInfo(wallpaper));
            }
        }

        folders = folders.OrderByDescending(x => x.CreationTimeUtc).ToList();

        await Parallel.ForEachAsync(folders, async (DirectoryInfo dir, CancellationToken token) =>
        {
            if (!long.TryParse(Path.GetFileName(dir.FullName), out long wallpaperId) || cachedEntries.ContainsKey(wallpaperId))
                return;

            WorkshopEntry entry = new WorkshopEntry(wallpaperId, dir.FullName, dir.CreationTimeUtc);
            await entry.DecodeBasic();

            cachedEntries.TryAdd(wallpaperId, entry);
        });
    }

    public static async Task OnSteamDownloadComplete(long id, DownloadStatus status)
    {
        if (status != DownloadStatus.Finished)
            return;

        foreach (string root in ConfigManager.localWorkshopLocations!)
        {
            if (!Directory.Exists(root))
                continue;

            string path = Path.Combine(root, id.ToString());

            if (!Directory.Exists(path))
                continue;

            DirectoryInfo dir = new DirectoryInfo(path);
            WorkshopEntry entry = new WorkshopEntry(id, dir.FullName, dir.CreationTimeUtc);
            await entry.DecodeBasic();

            cachedEntries.TryAdd(id, entry);
        }
    }

    public static WorkshopEntry[] GetCachedWallpaperEntries(string? nameSearch, HashSet<string>? tags, int skip, int take, DownloadedWallpaperOrdering ordering)
    {
        IEnumerable<WorkshopEntry> entries = cachedEntries.Values;

        if (!string.IsNullOrEmpty(nameSearch))
        {
            entries = entries.Where(x => x.title?.Contains(nameSearch, StringComparison.InvariantCultureIgnoreCase) ?? false);
        }

        if (tags?.Count > 0)
        {
            entries = entries.Where(
                x => x.tags?.Where(x => tags.Contains(x)).Count() == tags.Count
            );
        }

        filterEntriesCount = entries.Count();
        IEnumerable<WorkshopEntry> items;

        switch (ordering)
        {
            case DownloadedWallpaperOrdering.DownloadDate:
                items = entries.OrderByDescending(x => x.creationDate);
                break;

            case DownloadedWallpaperOrdering.LastUsed:
                items = entries.OrderByDescending(x => x.lastUsed).ThenByDescending(x => x.creationDate);
                break;

            default:
                items = entries.OrderBy(x => x.title);
                break;
        }

        return items.Skip(skip).Take(take).ToArray();
    }

    public static bool TryGetWallpaperEntry(long? id, out WorkshopEntry entry)
    {
        if (!id.HasValue)
        {
            entry = null!;
            return false;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        return cachedEntries.TryGetValue(id.Value, out entry) && entry != null;
#pragma warning restore CS8601 // Possible null reference assignment.
    }
}
