namespace ZenVoice.Core;

public sealed class CloudSpeechClient : ITranscriber
{
    private readonly HttpClient _http;
    private readonly string _engineId;
    private readonly Func<string?> _key;

    public CloudSpeechClient(string engineId, Func<string?> key, HttpClient? http = null)
    {
        _engineId = EngineIds.Canonical(engineId);
        _key = key;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(18) };
    }

    public bool IsAvailable => EngineIds.IsCloudSpeech(_engineId) && !string.IsNullOrWhiteSpace(_key());

    public string UnavailableReason =>
        IsAvailable ? "" : "Add a cloud key and tap Use on a cloud engine.";

    public async Task<string> TranscribeAsync(string wavPath, CancellationToken cancellationToken)
    {
        var key = _key() ?? throw new InvalidOperationException(UnavailableReason);
        var wav = await File.ReadAllBytesAsync(wavPath, cancellationToken).ConfigureAwait(false);
        using var req = Build(_engineId, key, wav);
        using var resp = await _http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cloud speech HTTP {(int)resp.StatusCode}: {body}");
        }

        return Parse(_engineId, body);
    }

    private static HttpRequestMessage Build(string engineId, string key, byte[] wav)
    {
        if (engineId == EngineIds.GeminiTranscribe)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/{CloudSpeech.GeminiModel}:generateContent")
            {
                Content = new ByteArrayContent(CloudSpeech.GeminiBody(null, wav))
            };
            req.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            req.Headers.TryAddWithoutValidation("x-goog-api-key", key);
            return req;
        }

        var url = engineId == EngineIds.GrokTranscribe
            ? CloudSpeech.GrokEndpoint
            : engineId == EngineIds.ElevenLabsScribe
                ? "https://api.elevenlabs.io/v1/speech-to-text"
                : CloudSpeech.OpenAiEndpoint;
        var req2 = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new ByteArrayContent(
                CloudSpeech.OpenAiBody(CloudSpeech.OpenAiModel, null, "speech.wav", wav))
        };
        req2.Content.Headers.TryAddWithoutValidation(
            "Content-Type", "multipart/form-data; boundary=ZenVoiceBoundary");
        if (engineId == EngineIds.ElevenLabsScribe)
        {
            req2.Headers.TryAddWithoutValidation("xi-api-key", key);
        }
        else
        {
            req2.Headers.TryAddWithoutValidation("Authorization", "Bearer " + key);
        }

        return req2;
    }

    private static string Parse(string engineId, string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (engineId == EngineIds.GeminiTranscribe)
        {
            return root.GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString()
                ?? throw new InvalidDataException("empty gemini");
        }

        if (root.TryGetProperty("text", out var text))
        {
            return text.GetString() ?? "";
        }

        if (root.TryGetProperty("transcript", out var tr))
        {
            return tr.GetString() ?? "";
        }

        throw new InvalidDataException("unreadable cloud transcript");
    }
}
