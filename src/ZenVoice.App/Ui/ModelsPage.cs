using ZenVoice.Core;

namespace ZenVoice.App.Ui;

internal sealed class ModelsPage : ISettingsPage
{
    private static readonly (string Id, string Name)[] Cloud =
    [
        (EngineIds.OpenAiTranscribe, "OpenAI Transcribe"),
        (EngineIds.GeminiTranscribe, "Gemini Transcribe"),
        (EngineIds.ElevenLabsScribe, "Scribe v2"),
        (EngineIds.GrokTranscribe, "Grok Transcribe")
    ];

    private const int LocalBase = UiIds.Models + 10;
    private const int CloudBase = UiIds.Models + 40;

    private readonly Action<string>? _download;
    private IntPtr _intro;
    private IntPtr _cloudHead;
    private readonly List<IntPtr> _localLabel = [];
    private readonly List<IntPtr> _localUse = [];
    private readonly List<IntPtr> _localDl = [];
    private readonly List<IntPtr> _cloudLabel = [];
    private readonly List<IntPtr> _cloudEdit = [];
    private readonly List<IntPtr> _cloudSave = [];
    private readonly List<IntPtr> _cloudUse = [];

    public ModelsPage(Action<string>? download = null) => _download = download;

    public string Id => "models";
    public string Title => "Models";
    public string Subtitle => "Choose the engine. Use downloads its file if needed.";

    public void Mount(IntPtr parent)
    {
        _intro = NativeUi.Label(parent, UiIds.Models + 1, Subtitle, 0, 0, 480, 32);
        for (var i = 0; i < ModelCatalog.Offered.Count; i++)
        {
            var id = LocalBase + i * 3;
            _localLabel.Add(NativeUi.Label(parent, id, LocalText(i), 0, 0, 360, 28));
            _localUse.Add(NativeUi.Button(parent, id + 1, "Use", 0, 0, 72, 28));
            _localDl.Add(NativeUi.Button(parent, id + 2, "Download", 0, 0, 88, 28));
        }

        _cloudHead = NativeUi.Label(parent, UiIds.Models + 2, "Cloud speech (opt-in)", 0, 0, 480, 28);
        for (var i = 0; i < Cloud.Length; i++)
        {
            var id = CloudBase + i * 4;
            _cloudLabel.Add(NativeUi.Label(parent, id, CloudText(i), 0, 0, 220, 28));
            _cloudEdit.Add(NativeUi.Child(
                "EDIT", "",
                Win32.WS_BORDER | Win32.WS_TABSTOP | Win32.ES_AUTOHSCROLL | Win32.ES_PASSWORD,
                parent, id + 1, 0, 0, 180, 24));
            _cloudSave.Add(NativeUi.Button(parent, id + 2, "Save", 0, 0, 64, 28));
            _cloudUse.Add(NativeUi.Button(parent, id + 3, "Use", 0, 0, 72, 28));
        }
    }

    public void Unmount()
    {
        NativeUi.Destroy(ref _intro);
        NativeUi.Destroy(ref _cloudHead);
        Kill(_localLabel);
        Kill(_localUse);
        Kill(_localDl);
        Kill(_cloudLabel);
        Kill(_cloudEdit);
        Kill(_cloudSave);
        Kill(_cloudUse);
    }

    public void Layout(Win32.RECT bounds)
    {
        var x = bounds.Left;
        var y = bounds.Top;
        var w = Math.Max(bounds.Right - bounds.Left, 360);
        NativeUi.Move(_intro, x, y, w, 32);
        y += 40;
        for (var i = 0; i < _localLabel.Count; i++)
        {
            NativeUi.Move(_localLabel[i], x, y, w - 180, 28);
            NativeUi.Move(_localUse[i], x + w - 172, y, 72, 28);
            if (Installed(i))
            {
                Win32.ShowWindow(_localDl[i], Win32.SW_HIDE);
            }
            else
            {
                NativeUi.Move(_localDl[i], x + w - 92, y, 88, 28);
                Win32.ShowWindow(_localDl[i], Win32.SW_SHOW);
            }

            y += 36;
        }

        NativeUi.Move(_cloudHead, x, y, w, 28);
        y += 36;
        for (var i = 0; i < _cloudLabel.Count; i++)
        {
            NativeUi.Move(_cloudLabel[i], x, y, 200, 28);
            NativeUi.Move(_cloudEdit[i], x + 208, y + 2, Math.Max(w - 360, 80), 24);
            NativeUi.Move(_cloudSave[i], x + w - 148, y, 64, 28);
            NativeUi.Move(_cloudUse[i], x + w - 76, y, 72, 28);
            y += 36;
        }
    }

    public void Refresh()
    {
        for (var i = 0; i < _localLabel.Count; i++)
        {
            NativeUi.Text(_localLabel[i], LocalText(i));
        }

        for (var i = 0; i < _cloudLabel.Count; i++)
        {
            NativeUi.Text(_cloudLabel[i], CloudText(i));
        }
    }

    public bool HandleCommand(int id)
    {
        var local = id - LocalBase;
        if (local >= 0 && local < ModelCatalog.Offered.Count * 3)
        {
            var i = local / 3;
            var kind = local % 3;
            if (kind == 1)
            {
                UseLocal(i);
                return true;
            }

            if (kind == 2)
            {
                if (!Installed(i))
                {
                    _download?.Invoke(ModelCatalog.Offered[i].Id);
                }

                return true;
            }
        }

        var cloud = id - CloudBase;
        if (cloud >= 0 && cloud < Cloud.Length * 4)
        {
            var i = cloud / 4;
            var kind = cloud % 4;
            if (kind == 2)
            {
                var key = NativeUi.Text(_cloudEdit[i]).Trim();
                if (key.Length > 0)
                {
                    CloudKeyStore.Set(Cloud[i].Id, key);
                }

                Refresh();
                return true;
            }

            if (kind == 3)
            {
                AppSettings.Current.EngineId = Cloud[i].Id;
                AppSettings.Current.Save();
                Refresh();
                return true;
            }
        }

        return false;
    }

    private void UseLocal(int i)
    {
        var model = ModelCatalog.Offered[i];
        AppSettings.Current.EngineId = model.Id;
        AppSettings.Current.Save();
        if (!Installed(i))
        {
            _download?.Invoke(model.Id);
        }

        Refresh();
    }

    private static bool Installed(int i)
    {
        var model = ModelCatalog.Offered[i];
        return File.Exists(Path.Combine(AppSettings.ModelsDir, model.Filename));
    }

    private static string LocalText(int i)
    {
        var model = ModelCatalog.Offered[i];
        var state = Installed(i) ? "installed" : "missing";
        var active = AppSettings.Current.EngineId == model.Id ? "  Active" : "";
        return $"{model.DisplayName}  {model.SizeBytes / 1_000_000} MB  {state}{active}";
    }

    private static string CloudText(int i)
    {
        var (id, name) = Cloud[i];
        var active = AppSettings.Current.EngineId == id ? "  Active" : "";
        var key = CloudKeyStore.Get(id) is null ? "  Needs key" : "";
        return $"{name}{active}{key}";
    }

    private static void Kill(List<IntPtr> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var h = list[i];
            NativeUi.Destroy(ref h);
            list[i] = h;
        }

        list.Clear();
    }
}
