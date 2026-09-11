namespace ZenVoice.Core;

public static class EngineIds
{
    public const string Whisper = "whisper";
    public const string WhisperLargeV3Turbo = "whisper-large-v3-turbo";
    public const string WhisperLargeV3 = "whisper-large-v3";
    public const string WhisperDistilLargeV3 = "whisper-distil-large-v3";
    public const string OpenAiTranscribe = "openai-transcribe";
    public const string GeminiTranscribe = "gemini-transcribe";
    public const string ElevenLabsScribe = "elevenlabs-scribe";
    public const string GrokTranscribe = "grok-transcribe";

    public static string Canonical(string engineId) =>
        engineId == Whisper ? WhisperLargeV3Turbo : engineId;

    public static bool IsCloudSpeech(string engineId)
    {
        var id = Canonical(engineId);
        return id is OpenAiTranscribe
            or GeminiTranscribe
            or ElevenLabsScribe
            or GrokTranscribe;
    }

    public static bool IsWhisperFamily(string engineId)
    {
        var id = Canonical(engineId);
        return ModelCatalog.Find(id) is { Format: var format }
            && format.Contains("whisper.cpp", StringComparison.Ordinal);
    }
}
