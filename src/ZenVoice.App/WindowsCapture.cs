using System.Runtime.InteropServices;
using ZenVoice.Core;

namespace ZenVoice.App;

internal sealed class WindowsCapture : IAudioCapture
{
    private const int Rate = 16_000;
    private const int BufferMs = 100;
    private static readonly int BufferBytes = Rate * 2 * BufferMs / 1000;

    private readonly MemoryStream _pcm = new();
    private readonly List<IntPtr> _headers = [];
    private readonly object _lock = new();
    private IntPtr _hwi;
    private Win32.WaveInProc? _proc;
    private bool _running;

    public double Level { get; private set; }

    public void Start()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("Capture requires Windows.");
        }

        lock (_lock)
        {
            _pcm.SetLength(0);
            Level = 0;
            var fmt = new Win32.WAVEFORMATEX
            {
                wFormatTag = 1,
                nChannels = 1,
                nSamplesPerSec = Rate,
                wBitsPerSample = 16,
                nBlockAlign = 2,
                nAvgBytesPerSec = Rate * 2,
                cbSize = 0
            };
            _proc = OnWave;
            var rc = Win32.waveInOpen(
                out _hwi,
                Win32.WAVE_MAPPER,
                ref fmt,
                _proc,
                IntPtr.Zero,
                Win32.CALLBACK_FUNCTION);
            if (rc != 0)
            {
                throw new InvalidOperationException("No microphone.");
            }

            for (var i = 0; i < 3; i++)
            {
                AddBuffer();
            }

            _running = true;
            Win32.waveInStart(_hwi);
        }
    }

    public string StopToWav()
    {
        lock (_lock)
        {
            _running = false;
            if (_hwi != IntPtr.Zero)
            {
                Win32.waveInStop(_hwi);
                Win32.waveInReset(_hwi);
                foreach (var hdr in _headers)
                {
                    Win32.waveInUnprepareHeader(_hwi, hdr, Marshal.SizeOf<Win32.WAVEHDR>());
                    var parsed = Marshal.PtrToStructure<Win32.WAVEHDR>(hdr);
                    Marshal.FreeHGlobal(parsed.lpData);
                    Marshal.FreeHGlobal(hdr);
                }

                _headers.Clear();
                Win32.waveInClose(_hwi);
                _hwi = IntPtr.Zero;
            }

            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ZenVoice",
                "tmp");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "speech.wav");
            WriteWav(path, _pcm.ToArray());
            return path;
        }
    }

    private void AddBuffer()
    {
        var data = Marshal.AllocHGlobal(BufferBytes);
        var hdr = new Win32.WAVEHDR
        {
            lpData = data,
            dwBufferLength = (uint)BufferBytes
        };
        var hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Win32.WAVEHDR>());
        Marshal.StructureToPtr(hdr, hdrPtr, false);
        Win32.waveInPrepareHeader(_hwi, hdrPtr, Marshal.SizeOf<Win32.WAVEHDR>());
        Win32.waveInAddBuffer(_hwi, hdrPtr, Marshal.SizeOf<Win32.WAVEHDR>());
        _headers.Add(hdrPtr);
    }

    private void OnWave(IntPtr hwi, uint uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
    {
        if (uMsg != Win32.WIM_DATA || !_running || dwParam1 == IntPtr.Zero)
        {
            return;
        }

        var hdr = Marshal.PtrToStructure<Win32.WAVEHDR>(dwParam1);
        var recorded = (int)hdr.dwBytesRecorded;
        if (recorded > 0)
        {
            var chunk = new byte[recorded];
            Marshal.Copy(hdr.lpData, chunk, 0, recorded);
            lock (_lock)
            {
                _pcm.Write(chunk, 0, recorded);
            }

            Level = Rms(chunk);
        }

        hdr.dwFlags &= ~Win32.WHDR_DONE;
        hdr.dwBytesRecorded = 0;
        Marshal.StructureToPtr(hdr, dwParam1, false);
        Win32.waveInAddBuffer(hwi, dwParam1, Marshal.SizeOf<Win32.WAVEHDR>());
    }

    private static double Rms(byte[] pcm)
    {
        if (pcm.Length < 2)
        {
            return 0;
        }

        double sum = 0;
        var n = pcm.Length / 2;
        for (var i = 0; i < n; i++)
        {
            var s = BitConverter.ToInt16(pcm, i * 2) / 32768.0;
            sum += s * s;
        }

        return Math.Sqrt(sum / n);
    }

    private static void WriteWav(string path, byte[] pcm)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        bw.Write("RIFF"u8);
        bw.Write(36 + pcm.Length);
        bw.Write("WAVEfmt "u8);
        bw.Write(16);
        bw.Write((ushort)1);
        bw.Write((ushort)1);
        bw.Write(Rate);
        bw.Write(Rate * 2);
        bw.Write((ushort)2);
        bw.Write((ushort)16);
        bw.Write("data"u8);
        bw.Write(pcm.Length);
        bw.Write(pcm);
    }
}
