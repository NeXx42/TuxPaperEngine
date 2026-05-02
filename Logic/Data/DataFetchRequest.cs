namespace Logic.Data;

public struct DataFetchRequest
{
    public int skip;
    public int take;

    public int orderId;
    public string? textFilter;
    public string? resolutionFilter;

    public HashSet<string>? tags;
}
