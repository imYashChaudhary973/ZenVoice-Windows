using System.Text;
using Whisper.net;
using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class WhisperTranscriber : ITranscriber
{
    public static string ModelPath
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("ZENVOICE_MODEL_PATH");
            if (!string.IsNullOrWhiteSpace(env))
            {
                return env;
            }

            var model = ModelCatalog.Find(AppSettings.Current.EngineId) ?? ModelCatalog.Distil;
            return Path.Combine(AppSettings.ModelsDir, model.Filename);
        }
    }

    public bool IsAvailable => File.Exists(ModelPath);

    public string UnavailableReason =>
        $"{(ModelCatalog.Find(AppSettings.Current.EngineId) ?? ModelCatalog.Distil).DisplayName} is not installed. Open Models and download it.";

    public async Task<string> TranscribeAsync(string wavPath, CancellationToken cancellationToken)
    {
        using var factory = WhisperFactory.FromPath(ModelPath);
        using var processor = factory.CreateBuilder().WithLanguage("auto").Build();
        await using var stream = File.OpenRead(wavPath);
        var text = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(stream, cancellationToken))
        {
            text.Append(segment.Text);
        }

        return text.ToString();
    }
}
