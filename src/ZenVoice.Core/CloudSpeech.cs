using System.Text;
using System.Text.Json;

namespace ZenVoice.Core;

public static class CloudSpeech
{
    public const string OpenAiEndpoint = "https://api.openai.com/v1/audio/transcriptions";
    public const string OpenAiModel = "gpt-4o-mini-transcribe";
    public const string GeminiModel = "gemini-2.0-flash";
    public const string ScribeModel = "scribe_v2";
    public const string GrokEndpoint = "https://api.x.ai/v1/stt";
    public const string GeminiPrompt =
        "Transcribe this audio. Reply with the spoken words only. No labels, quotes, or commentary.";

    public static byte[] OpenAiBody(string model, string? language, string filename, byte[] wav)
    {
        const string boundary = "ZenVoiceBoundary";
        using var stream = new MemoryStream();
        void Field(string name, string value)
        {
            var chunk =
                $"--{boundary}\r\nContent-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}\r\n";
            stream.Write(Encoding.UTF8.GetBytes(chunk));
        }

        Field("model", model);
        if (!string.IsNullOrEmpty(language) && language != "auto")
        {
            Field("language", language);
        }

        Field("response_format", "json");
        stream.Write(Encoding.UTF8.GetBytes(
            $"--{boundary}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{filename}\"\r\nContent-Type: audio/wav\r\n\r\n"));
        stream.Write(wav);
        stream.Write(Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n"));
        return stream.ToArray();
    }

    public static byte[] GeminiBody(string? language, byte[] wav)
    {
        var prompt = GeminiPrompt;
        if (!string.IsNullOrEmpty(language) && language != "auto")
        {
            prompt += $" The spoken language is {language}.";
        }

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "audio/wav",
                                data = Convert.ToBase64String(wav)
                            }
                        }
                    }
                }
            },
            generationConfig = new { temperature = 0 }
        };
        return JsonSerializer.SerializeToUtf8Bytes(payload);
    }

    public static bool BodyLeaksClientIdentity(byte[] body)
    {
        var text = Encoding.UTF8.GetString(body);
        string[] forbidden =
        [
            "bundleIdentifier", "deviceID", "device_id", "installID", "install_id",
            "voiceProfile", "nextDictationContext"
        ];
        return forbidden.Any(token => text.Contains(token, StringComparison.Ordinal));
    }
}
