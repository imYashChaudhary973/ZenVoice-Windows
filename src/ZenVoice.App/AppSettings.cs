using System.Text.Json;
using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class AppSettings
{
    public string EngineId { get; set; } = EngineIds.WhisperDistilLargeV3;
    public string Formatting { get; set; } = "clean";
    public bool SaveHistory { get; set; } = true;
    public bool AutoStart { get; set; } = true;

    public static AppSettings Current { get; private set; } = Load();

    public static string Dir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ZenVoice");

    public static string ModelsDir => Path.Combine(Dir, "Models");

    private static string FilePath => Path.Combine(Dir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                if (loaded is not null)
                {
                    return loaded;
                }
            }
        }
        catch
        {
        }

        return new AppSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Dir);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        Current = this;
    }
}
