namespace ZenVoice.Core;

public enum InsertResult
{
    Pasted,
    CopiedOnly,
    BlockedBySecureInput
}

public interface IAudioCapture
{
    void Start();
    string StopToWav();
    double Level { get; }
}

public interface ITranscriber
{
    bool IsAvailable { get; }
    string UnavailableReason { get; }
    Task<string> TranscribeAsync(string wavPath, CancellationToken cancellationToken);
}

public interface ITextInserter
{
    InsertResult Insert(string text);
}

public interface IHud
{
    void Show(DictationPhase phase, string? detail = null, double level = 0);
}

public sealed class DictationController
{
    private readonly IAudioCapture _capture;
    private readonly ITranscriber _transcriber;
    private readonly ITextInserter _inserter;
    private readonly IHud _hud;
    private readonly TranscriptVault? _vault;
    private readonly TranscriptCleaner _cleaner = new();
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DictationPhase Phase { get; private set; } = DictationPhase.Idle;
    public InsertResult? LastInsert { get; private set; }
    public string LastTranscript { get; private set; } = "";
    public bool CleanTranscript { get; set; } = true;
    public bool SaveHistory { get; set; } = true;
    public string EngineId { get; set; } = "local";
    public Func<string, CancellationToken, Task<string>>? Refine { get; set; }
    public DictationController(
        IAudioCapture capture,
        ITranscriber transcriber,
        ITextInserter inserter,
        IHud hud,
        TranscriptVault? vault = null)
    {
        _capture = capture;
        _transcriber = transcriber;
        _inserter = inserter;
        _hud = hud;
        _vault = vault;
        _hud.Show(Phase);
    }
    public async Task ToggleAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Phase is DictationPhase.Listening)
            {
                await FinishAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            if (Phase is DictationPhase.Transcribing or DictationPhase.Inserting)
            {
                return;
            }

            Begin();
        }
        finally
        {
            _gate.Release();
        }
    }

    private void Begin()
    {
        if (!_transcriber.IsAvailable)
        {
            Phase = DictationPhase.Error;
            _hud.Show(Phase, _transcriber.UnavailableReason);
            return;
        }

        _capture.Start();
        Phase = DictationPhase.Listening;
        _hud.Show(Phase, level: _capture.Level);
    }

    private async Task FinishAsync(CancellationToken cancellationToken)
    {
        string wav;
        try
        {
            wav = _capture.StopToWav();
        }
        catch (Exception ex)
        {
            Phase = DictationPhase.Error;
            _hud.Show(Phase, ex.Message);
            return;
        }

        Phase = DictationPhase.Transcribing;
        _hud.Show(Phase);
        string raw;
        try
        {
            raw = await _transcriber.TranscribeAsync(wav, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Phase = DictationPhase.Error;
            _hud.Show(Phase, ex.Message);
            return;
        }

        var text = CleanTranscript ? _cleaner.Clean(raw) : raw.Trim();
        if (Refine is not null)
        {
            text = await Refine(text, cancellationToken).ConfigureAwait(false);
        }

        LastTranscript = text;
        if (text.Length == 0)
        {
            Phase = DictationPhase.Error;
            _hud.Show(Phase, "No speech");
            return;
        }

        Phase = DictationPhase.Inserting;
        _hud.Show(Phase);
        LastInsert = _inserter.Insert(text);
        if (SaveHistory)
        {
            _vault?.Add(text, EngineId);
        }
        Phase = DictationPhase.Success;
        var detail = LastInsert == InsertResult.Pasted ? text : "Copied — paste yourself";
        _hud.Show(Phase, detail);
    }
}
