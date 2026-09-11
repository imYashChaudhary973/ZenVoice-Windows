using System.Runtime.InteropServices;
using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class ParakeetTranscriber : ITranscriber
{
    internal static string DllPath => Path.Combine(AppContext.BaseDirectory, "parakeet.dll");

    internal static string ModelPath =>
        Path.Combine(AppSettings.ModelsDir, ModelCatalog.Parakeet.Filename);

    public bool IsAvailable => File.Exists(DllPath) && File.Exists(ModelPath);

    public string UnavailableReason =>
        !File.Exists(DllPath)
            ? "Put parakeet.dll beside ZenVoice.exe (parakeet.cpp)."
            : !File.Exists(ModelPath)
                ? "Parakeet TDT v3 is not installed. Open Models and download it."
                : "";

    public Task<string> TranscribeAsync(string wavPath, CancellationToken cancellationToken) =>
        Task.Run(() => Transcribe(wavPath), cancellationToken);

    // ponytail: load-per-call; cache parakeet_ctx if decode latency matters
    private static string Transcribe(string wavPath)
    {
        try
        {
            var ctx = Native.Load(ModelPath);
            if (ctx == 0)
            {
                throw new InvalidOperationException("parakeet.cpp failed to load the GGUF model.");
            }

            try
            {
                var text = Native.TranscribePath(ctx, wavPath, 0);
                if (text == 0)
                {
                    throw new InvalidOperationException("parakeet.cpp transcribe failed.");
                }

                try
                {
                    return Marshal.PtrToStringUTF8(text) ?? "";
                }
                finally
                {
                    Native.FreeString(text);
                }
            }
            finally
            {
                Native.Free(ctx);
            }
        }
        catch (DllNotFoundException)
        {
            throw new InvalidOperationException(
                "Put parakeet.dll beside ZenVoice.exe (parakeet.cpp).");
        }
        catch (EntryPointNotFoundException)
        {
            throw new InvalidOperationException(
                "parakeet.dll is missing the parakeet.cpp C API (parakeet_capi_load / parakeet_capi_transcribe_path).");
        }
    }

    private static class Native
    {
        private const string Dll = "parakeet";

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "parakeet_capi_load")]
        public static extern nint Load([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "parakeet_capi_transcribe_path")]
        public static extern nint TranscribePath(
            nint ctx,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string wavPath,
            int decoder);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "parakeet_capi_free")]
        public static extern void Free(nint ctx);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "parakeet_capi_free_string")]
        public static extern void FreeString(nint s);
    }
}
