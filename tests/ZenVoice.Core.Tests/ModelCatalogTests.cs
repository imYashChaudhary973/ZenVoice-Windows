using ZenVoice.Core;

namespace ZenVoice.Core.Tests;

public class ModelCatalogTests
{
    [Fact]
    public void OffersExactlyThreeWhisperFiles()
    {
        Assert.Equal(3, ModelCatalog.Offered.Count);
        Assert.Equal(
            [
                EngineIds.WhisperLargeV3Turbo,
                EngineIds.WhisperLargeV3,
                EngineIds.WhisperDistilLargeV3
            ],
            ModelCatalog.Offered.Select(m => m.Id));
    }

    [Fact]
    public void PinsTurboHashAndSize()
    {
        Assert.Equal(
            "394221709cd5ad1f40c46e6031ca61bce88931e6e088c188294c6d5a55ffa7e2",
            ModelCatalog.Turbo.Sha256);
        Assert.Equal(574_041_195, ModelCatalog.Turbo.SizeBytes);
        Assert.Contains("ggerganov/whisper.cpp/resolve/", ModelCatalog.Turbo.DownloadUrl.AbsoluteUri);
    }

    [Fact]
    public void CanonicalWhisperIsTurbo()
    {
        Assert.Equal(EngineIds.WhisperLargeV3Turbo, EngineIds.Canonical("whisper"));
        Assert.True(EngineIds.IsWhisperFamily("whisper"));
        Assert.False(EngineIds.IsCloudSpeech("whisper"));
    }

    [Theory]
    [InlineData(EngineIds.OpenAiTranscribe)]
    [InlineData(EngineIds.GeminiTranscribe)]
    [InlineData(EngineIds.ElevenLabsScribe)]
    [InlineData(EngineIds.GrokTranscribe)]
    public void CloudEnginesAreClassified(string id)
    {
        Assert.True(EngineIds.IsCloudSpeech(id));
        Assert.False(EngineIds.IsWhisperFamily(id));
        Assert.Null(ModelCatalog.Find(id));
    }
}
