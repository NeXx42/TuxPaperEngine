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
}
