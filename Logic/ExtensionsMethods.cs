using System.Text.Json;

namespace Logic;

public static class ExtensionsMethods
{
    public static void TryGetDoubleProperty(this JsonElement el, string propertyName, out double? res)
    {
        res = null;

        if (el.TryGetProperty(propertyName, out var e) && e.TryGetDouble(out double d))
            res = d;
    }

    public static bool TryParseDouble(this string? inp, out double? res)
    {
        if (double.TryParse(inp, out double temp))
        {
            res = temp;
            return true;
        }

        res = null;
        return false;
    }

    public static bool TryParseLong(this string? inp, out long? res)
    {
        if (long.TryParse(inp, out long temp))
        {
            res = temp;
            return true;
        }

        res = null;
        return false;
    }
}
