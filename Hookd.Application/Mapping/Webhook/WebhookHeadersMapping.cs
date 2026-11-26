using System.Text.Json;

namespace Hookd.Application.Mapping.Webhook;

// Converts header objects to and from database JSON.
public static class WebhookHeadersMapping
{
    public static string? ToJson(Dictionary<string, string>? headers)
    {
        return headers is null || headers.Count == 0 ? null : JsonSerializer.Serialize(headers);
    }

    public static Dictionary<string, string> FromJson(string? json)
    {
        var headers = json is null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        return headers is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
    }

    public static bool AreEqual(string? first, string? second)
    {
        var firstHeaders = FromJson(first);
        var secondHeaders = FromJson(second);
        return firstHeaders.Count == secondHeaders.Count && firstHeaders.All(header =>
            secondHeaders.TryGetValue(header.Key, out var value) && header.Value == value);
    }
}
