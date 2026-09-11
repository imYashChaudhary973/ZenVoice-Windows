using ZenVoice.Core;

namespace ZenVoice.App.Ui;

internal sealed class PrivacyPage : ISettingsPage
{
    private readonly TranscriptVault _vault;
    private readonly Action? _deleteAll;
    private IntPtr _dictation;
    private IntPtr _save;
    private IntPtr _hint;
    private IntPtr _inventory;
    private IntPtr _counts;
    private IntPtr _wipe;
    private IntPtr _banner;
    private IntPtr _note;

    public PrivacyPage(TranscriptVault vault, Action? deleteAll = null)
    {
        _vault = vault;
        _deleteAll = deleteAll;
    }

    public string Id => "settings";
    public string Title => "Privacy & Data";
    public string Subtitle => "What ZenVoice keeps, and where.";

    public void Mount(IntPtr parent)
    {
        _dictation = NativeUi.Label(parent, UiIds.Privacy + 1, "Dictation privacy", 0, 0, 480, 20);
        _save = NativeUi.Check(parent, UiIds.Privacy + 2, "Save history", 0, 0, 400, 24);
        _hint = NativeUi.Label(
            parent, UiIds.Privacy + 3,
            "Encrypted locally. Pausing keeps existing records but stops new ones.",
            0, 0, 480, 36);
        _inventory = NativeUi.Label(parent, UiIds.Privacy + 4, "What's on this PC right now", 0, 0, 480, 20);
        _counts = NativeUi.Label(parent, UiIds.Privacy + 5, Counts(), 0, 0, 480, 40);
        _wipe = NativeUi.Button(parent, UiIds.Privacy + 6, "Delete all history", 0, 0, 180, 32);
        _banner = NativeUi.Label(
            parent, UiIds.Privacy + 7,
            "Model downloads use pinned revisions and SHA-256 verification. Local engines keep audio on this PC. Cloud speech engines upload the clip after you stop, using your own key.",
            0, 0, 480, 72);
        _note = NativeUi.Label(parent, UiIds.Privacy + 8, "Start with Windows is in the tray menu.", 0, 0, 480, 24);
        NativeUi.Checked(_save, AppSettings.Current.SaveHistory);
    }

    public void Unmount()
    {
        NativeUi.Destroy(ref _dictation);
        NativeUi.Destroy(ref _save);
        NativeUi.Destroy(ref _hint);
        NativeUi.Destroy(ref _inventory);
        NativeUi.Destroy(ref _counts);
        NativeUi.Destroy(ref _wipe);
        NativeUi.Destroy(ref _banner);
        NativeUi.Destroy(ref _note);
    }

    public void Layout(Win32.RECT bounds)
    {
        var x = bounds.Left;
        var y = bounds.Top;
        var w = bounds.Right - bounds.Left;
        NativeUi.Move(_dictation, x, y, w, 20);
        NativeUi.Move(_save, x, y + 24, w, 24);
        NativeUi.Move(_hint, x, y + 48, w, 36);
        NativeUi.Move(_inventory, x, y + 96, w, 20);
        NativeUi.Move(_counts, x, y + 120, w, 40);
        NativeUi.Move(_wipe, x, y + 168, 180, 32);
        NativeUi.Move(_banner, x, y + 212, w, 72);
        NativeUi.Move(_note, x, y + 292, w, 24);
    }

    public void Refresh()
    {
        NativeUi.Checked(_save, AppSettings.Current.SaveHistory);
        NativeUi.Text(_counts, Counts());
    }

    public bool HandleCommand(int id)
    {
        if (id == UiIds.Privacy + 2)
        {
            AppSettings.Current.SaveHistory = NativeUi.Checked(_save);
            AppSettings.Current.Save();
            return true;
        }

        if (id == UiIds.Privacy + 6)
        {
            _vault.DeleteAll();
            _deleteAll?.Invoke();
            Refresh();
            return true;
        }

        return false;
    }

    private string Counts()
    {
        var n = _vault.List().Count;
        var models = Directory.Exists(AppSettings.ModelsDir)
            ? Directory.GetFiles(AppSettings.ModelsDir).Length
            : 0;
        return $"Encrypted transcripts: {n} records\nLocal models: {models} files";
    }
}
