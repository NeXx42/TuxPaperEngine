namespace Logic.Data;

public struct DataFetchRequest
{
    public int skip;
    public int take;

    public int orderId;
    public string? textFilter;
    public HashSet<string>? tags;
}
