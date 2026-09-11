using ZenVoice.Core;

namespace ZenVoice.Core.Tests;

public class DictationControllerTests
{
    [Fact]
    public async Task ToggleStopInsertsCleanedText()
    {
        var capture = new FakeCapture();
        var transcriber = new FakeTranscriber("  hello    from\nZenVoice   ");
        var inserter = new FakeInserter();
        var hud = new FakeHud();
        var dictation = new DictationController(capture, transcriber, inserter, hud);

        await dictation.ToggleAsync();
        Assert.Equal(DictationPhase.Listening, dictation.Phase);

        await dictation.ToggleAsync();
        Assert.Equal(DictationPhase.Success, dictation.Phase);
        Assert.Equal("Hello from ZenVoice", dictation.LastTranscript);
        Assert.Equal("Hello from ZenVoice", inserter.LastText);
        Assert.Equal(InsertResult.Pasted, dictation.LastInsert);
        Assert.True(capture.Stopped);
    }

    [Fact]
    public async Task MissingEngineShowsErrorWithoutRecording()
    {
        var capture = new FakeCapture();
        var transcriber = new FakeTranscriber("hi") { IsAvailable = false, UnavailableReason = "no model" };
        var dictation = new DictationController(capture, transcriber, new FakeInserter(), new FakeHud());

        await dictation.ToggleAsync();
        Assert.Equal(DictationPhase.Error, dictation.Phase);
        Assert.False(capture.Started);
    }

    private sealed class FakeCapture : IAudioCapture
    {
        public bool Started { get; private set; }
        public bool Stopped { get; private set; }
        public double Level => 0.4;
        public void Start() => Started = true;
        public string StopToWav()
        {
            Stopped = true;
            return "speech.wav";
        }
    }

    private sealed class FakeTranscriber : ITranscriber
    {
        private readonly string _text;
        public FakeTranscriber(string text) => _text = text;
        public bool IsAvailable { get; set; } = true;
        public string UnavailableReason { get; set; } = "";
        public Task<string> TranscribeAsync(string wavPath, CancellationToken cancellationToken) =>
            Task.FromResult(_text);
    }

    private sealed class FakeInserter : ITextInserter
    {
        public string? LastText { get; private set; }
        public InsertResult Insert(string text)
        {
            LastText = text;
            return InsertResult.Pasted;
        }
    }

    private sealed class FakeHud : IHud
    {
        public void Show(DictationPhase phase, string? detail = null, double level = 0) { }
    }
}
