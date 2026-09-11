using System.Net;
using System.Text;
using ZenVoice.Core;

namespace ZenVoice.Core.Tests;

public class CloudClientTests
{
    [Fact]
    public void MissingKeyIsUnavailable()
    {
        var client = new CloudSpeechClient(EngineIds.OpenAiTranscribe, () => null);
        Assert.False(client.IsAvailable);
    }

    [Fact]
    public async Task OpenAiResponseTextIsReturned()
    {
        var handler = new StubHandler("""{"text":" hello there "}""");
        var http = new HttpClient(handler);
        var client = new CloudSpeechClient(EngineIds.OpenAiTranscribe, () => "sk-test", http);
        Assert.True(client.IsAvailable);
        var wav = Path.GetTempFileName();
        await File.WriteAllBytesAsync(wav, "RIFF"u8.ToArray());
        var text = await client.TranscribeAsync(wav, CancellationToken.None);
        Assert.Equal(" hello there ", text);
        Assert.Contains("api.openai.com", handler.LastUri!.Host);
        File.Delete(wav);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _json;
        public Uri? LastUri { get; private set; }

        public StubHandler(string json) => _json = json;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }
}
