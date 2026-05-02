using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using HtmlAgilityPack;
using Logic.Data;
using Logic.Enums;

namespace Logic;

public static class SteamWorkshopManager
{
    public static HttpClient httpClient
    {
        get
        {
            if (m_Client == null)
                m_Client = new HttpClient();

            return m_Client;
        }
    }
    private static HttpClient? m_Client;
    private static CancellationTokenSource? activeQuery;

    private static string[]? tags;
    private static string[]? resolutions;

    public static string[] getTags => tags ?? [];
    public static string[] getResolutions => resolutions ?? [];

    public static async Task<DataFetchResponse> FetchItems(DataFetchRequest filter, bool recacheFilters)
    {
        await (activeQuery?.CancelAsync() ?? Task.CompletedTask);
        activeQuery = new CancellationTokenSource();

        try
        {
            string url = BuildURLFromFilter(filter);
            HtmlDocument doc = await LoadDocument(url, activeQuery.Token);

            if (recacheFilters)
            {
                await RecacheFilters(doc);
            }

            return new DataFetchResponse()
            {
                entries = await ScrapeAllWorkshopElements(doc, activeQuery.Token)
            };
        }
        catch (TaskCanceledException)
        {
            return new DataFetchResponse();
        }
        catch (Exception e)
        {
            return new DataFetchResponse(e);
        }
    }

    private static async Task<SteamWorkshopEntry[]> ScrapeAllWorkshopElements(HtmlDocument doc, CancellationToken token)
    {
        HtmlNodeCollection entries = doc.DocumentNode.SelectNodes("//div[@class='workshopItem']");
        ConcurrentBag<SteamWorkshopEntry> results = new ConcurrentBag<SteamWorkshopEntry>();

        await Parallel.ForEachAsync(entries, token, async (i, token) =>
        {
            SteamWorkshopEntry? res = await ScrapeWorkshopElements(i, token);

            if (res != null)
                results.Add(res);
        });

        return results.ToArray();
    }

    private static Task<SteamWorkshopEntry?> ScrapeWorkshopElements(HtmlNode item, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromResult<SteamWorkshopEntry?>(null);

        string id = item.SelectSingleNode(".//a[contains(@class, 'ugc')]")?.GetDataAttribute("publishedfileid")?.Value ?? "";
        string name = item.SelectSingleNode(".//div[contains(@class, 'workshopItemTitle')]")?.InnerHtml ?? "";
        string imgUrl = item.SelectSingleNode(".//img[contains(@class, 'workshopItemPreviewImage')]")?.GetAttributeValue("src", "") ?? "";

        if (long.TryParse(id, out long _id))
        {
            return Task.FromResult<SteamWorkshopEntry?>(new SteamWorkshopEntry()
            {
                id = _id,
                name = name,
                imgUrl = imgUrl,
            });
        }

        return Task.FromResult<SteamWorkshopEntry?>(null);
    }

    private static async Task<HtmlDocument> LoadDocument(string url, CancellationToken token)
    {
        string html = await httpClient.GetStringAsync(url, token);

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc;
    }

    private static async Task RecacheFilters(HtmlDocument doc)
    {
        HtmlNodeCollection allTags = doc.DocumentNode.SelectNodes("//input[@class='inputTagsFilter']");
        tags = allTags.Select(x => x?.GetAttributeValue("value", "")!).Order().ToArray();

        var node = doc.DocumentNode.SelectSingleNode("//div[@class='tag_category_desc' and normalize-space(text())='Resolution']");
        resolutions = node.NextSibling.NextSibling.ChildNodes.Select(x => x?.GetAttributeValue("value", "")!).Where(x => !string.IsNullOrEmpty(x) && x != "-1").Order().ToArray();
    }


    private static string BuildURLFromFilter(DataFetchRequest filter)
    {
        StringBuilder sb = new StringBuilder($"https://steamcommunity.com/workshop/browse/?appid={ConfigManager.WALLPAPER_ENGINE_ID}&");

        if (!string.IsNullOrEmpty(filter.textFilter))
        {
            sb.Append($"&searchtext={filter.textFilter}");
        }

        if (!string.IsNullOrEmpty(filter.resolutionFilter))
        {
            sb.Append($"&requiredtags[]={filter.resolutionFilter.Replace(" ", "+")}");
        }

        if (filter.tags != null)
            foreach (string str in filter.tags)
            {
                sb.Append($"&requiredtags[]={str}");
            }

        sb.Append("&browsesort=");
        switch ((SteamWallpaperOrdering)filter.orderId)
        {
            case SteamWallpaperOrdering.Popular: sb.Append("trend&days=7"); break;
            case SteamWallpaperOrdering.New: sb.Append("mostrecent"); break;
            case SteamWallpaperOrdering.LastUpdated: sb.Append("lastupdated"); break;
            case SteamWallpaperOrdering.MostSubscribed: sb.Append("totaluniquesubscribers"); break;
        }

        sb.Append($"&p={filter.skip}");
        sb.Append("&rss=1");

        return sb.ToString();
    }
}
