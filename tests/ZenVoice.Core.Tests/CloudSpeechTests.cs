using System.Text;
using ZenVoice.Core;

namespace ZenVoice.Core.Tests;

public class CloudSpeechTests
{
    private static readonly byte[] Wav = "RIFF....WAVEfmt "u8.ToArray();

    [Fact]
    public void OpenAiBodyHasModelLanguageAndFile()
    {
        var body = Encoding.UTF8.GetString(
            CloudSpeech.OpenAiBody(CloudSpeech.OpenAiModel, "en", "speech.wav", Wav));
        Assert.Contains("gpt-4o-mini-transcribe", body);
        Assert.Contains("language", body);
        Assert.Contains("speech.wav", body);
        Assert.False(CloudSpeech.BodyLeaksClientIdentity(Encoding.UTF8.GetBytes(body)));
    }

    [Fact]
    public void GeminiBodyHasPromptAndWavNoIdentity()
    {
        var bytes = CloudSpeech.GeminiBody("en", Wav);
        var body = Encoding.UTF8.GetString(bytes);
        Assert.Contains("Transcribe this audio", body);
        Assert.Contains("audio/wav", body);
        Assert.False(CloudSpeech.BodyLeaksClientIdentity(bytes));
    }
}
