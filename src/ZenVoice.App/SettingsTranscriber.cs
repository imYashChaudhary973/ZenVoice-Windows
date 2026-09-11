using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class SettingsTranscriber : ITranscriber
{
    public bool IsAvailable => Resolve().IsAvailable;

    public string UnavailableReason => Resolve().UnavailableReason;

    public Task<string> TranscribeAsync(string wavPath, CancellationToken cancellationToken) =>
        Resolve().TranscribeAsync(wavPath, cancellationToken);

    internal static ITranscriber Resolve()
    {
        var id = EngineIds.Canonical(AppSettings.Current.EngineId);
        if (EngineIds.IsCloudSpeech(id))
        {
            return new CloudSpeechClient(id, () => CloudKeyStore.Get(id));
        }

        if (id == EngineIds.ParakeetTdt)
        {
            return new ParakeetTranscriber();
        }

        return new WhisperTranscriber();
    }
}
