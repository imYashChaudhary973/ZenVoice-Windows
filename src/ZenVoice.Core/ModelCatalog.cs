namespace ZenVoice.Core;

public sealed record Model(
    string Id,
    string DisplayName,
    string Filename,
    string Sha256,
    long SizeBytes,
    string SourceRepository,
    string SourceRevision,
    string Format,
    bool EnglishOnly)
{
    public Uri DownloadUrl => new(
        $"{SourceRepository}/resolve/{SourceRevision}/{Filename}?download=true");
}

public static class ModelCatalog
{
    public const string WhisperCppRepo = "https://huggingface.co/ggerganov/whisper.cpp";
    public const string WhisperCppRevision = "5359861c739e955e79d9a303bcbc70fb988958b1";
    public const string DistilRepo = "https://huggingface.co/distil-whisper/distil-large-v3-ggml";
    public const string DistilRevision = "0d78dd96ed9fc152325f63b53788fec3b43de031";
    public const string ParakeetRepo = "https://huggingface.co/mudler/parakeet-cpp-gguf";
    public const string ParakeetRevision = "main";

    public static readonly Model Parakeet = new(
        EngineIds.ParakeetTdt,
        "Parakeet TDT v3",
        "tdt-0.6b-v3-q8_0.gguf",
        "4d69a4a6683f4f2d952bad794c1357ca6eb628027695b4699c5a9ad4cd07d757",
        940_663_680,
        ParakeetRepo,
        ParakeetRevision,
        "parakeet.cpp GGUF",
        EnglishOnly: false);

    public static readonly Model Turbo = Whisper(
        EngineIds.WhisperLargeV3Turbo,
        "Whisper Large V3 Turbo",
        "ggml-large-v3-turbo-q5_0.bin",
        "394221709cd5ad1f40c46e6031ca61bce88931e6e088c188294c6d5a55ffa7e2",
        574_041_195,
        englishOnly: false);

    public static readonly Model LargeV3 = Whisper(
        EngineIds.WhisperLargeV3,
        "Whisper Large V3",
        "ggml-large-v3-q5_0.bin",
        "d75795ecff3f83b5faa89d1900604ad8c780abd5739fae406de19f23ecd98ad1",
        1_081_140_203,
        englishOnly: false);

    public static readonly Model Distil = new(
        EngineIds.WhisperDistilLargeV3,
        "Distil-Whisper Large V3",
        "ggml-distil-large-v3.bin",
        "2883a11b90fb10ed592d826edeaee7d2929bf1ab985109fe9e1e7b4d2b69a298",
        1_519_521_155,
        DistilRepo,
        DistilRevision,
        "whisper.cpp GGML",
        EnglishOnly: true);

    public static IReadOnlyList<Model> Offered { get; } = [Parakeet, Turbo, LargeV3, Distil];

    public static Model? Find(string id) =>
        Offered.FirstOrDefault(m => m.Id == EngineIds.Canonical(id));

    private static Model Whisper(
        string id,
        string name,
        string filename,
        string sha256,
        long size,
        bool englishOnly) =>
        new(
            id,
            name,
            filename,
            sha256,
            size,
            WhisperCppRepo,
            WhisperCppRevision,
            "whisper.cpp GGML",
            englishOnly);
}
