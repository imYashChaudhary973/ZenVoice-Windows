using System.Text;
using System.Text.Json;

namespace ZenVoice.App;

internal static class CloudFormatter
{
    public static async Task<string> Apply(string text, CancellationToken ct)
    {
        var endpoint = AppSettings.Current.CloudFormatEndpoint;
        var key = CloudKeyStore.Get("cloud-format");
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
        {
            return text;
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint.Trim())
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { text }),
                    Encoding.UTF8,
                    "application/json")
            };
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + key);
            using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return resp.IsSuccessStatusCode ? Parse(body) : text;
        }
        catch
        {
            return text;
        }
    }

    private static string Parse(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (TryString(root, "text", out var t))
                {
                    return t;
                }

                if (TryString(root, "content", out var c))
                {
                    return c;
                }
            }

            if (root.ValueKind == JsonValueKind.String)
            {
                return root.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
        }

        return body;
    }

    private static bool TryString(JsonElement root, string name, out string value)
    {
        value = "";
        if (!root.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = el.GetString() ?? "";
        return true;
    }
}
