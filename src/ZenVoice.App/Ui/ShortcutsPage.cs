namespace ZenVoice.App.Ui;

internal sealed class ShortcutsPage : ISettingsPage
{
    public string Id => "shortcuts";
    public string Title => "Shortcuts";
    public string Subtitle => "Press a shortcut, speak, press it again.";

    private IntPtr _section;
    private IntPtr _startTitle;
    private IntPtr _startSub;
    private IntPtr _startKey;
    private IntPtr _pasteTitle;
    private IntPtr _pasteSub;
    private IntPtr _pasteKey;
    private IntPtr _holdTitle;
    private IntPtr _holdKey;
    private IntPtr _banner;

    public void Mount(IntPtr parent)
    {
        var id = UiIds.Shortcuts;
        _section = NativeUi.Label(parent, id + 1, "Trigger", 0, 0, 200, 24);
        _startTitle = NativeUi.Label(parent, id + 2, "Start / stop dictation", 0, 0, 360, 22);
        _startSub = NativeUi.Label(parent, id + 3, "Press once to start, again to transcribe and insert", 0, 0, 360, 20);
        _startKey = NativeUi.Label(parent, id + 4, "Ctrl+Alt+Space", 0, 0, 160, 22);
        _pasteTitle = NativeUi.Label(parent, id + 5, "Paste latest dictation", 0, 0, 360, 22);
        _pasteSub = NativeUi.Label(parent, id + 6, "Re-insert the most recent transcript anywhere", 0, 0, 360, 20);
        _pasteKey = NativeUi.Label(parent, id + 7, "Windows v1 later", 0, 0, 160, 22);
        _holdTitle = NativeUi.Label(parent, id + 8, "Hold to dictate", 0, 0, 360, 22);
        _holdKey = NativeUi.Label(parent, id + 9, "Windows v1 later", 0, 0, 160, 22);
        _banner = NativeUi.Label(
            parent, id + 10,
            "A two-modifier shortcut is less likely to conflict with other apps.",
            0, 0, 500, 40);
    }

    public void Unmount()
    {
        NativeUi.Destroy(ref _section);
        NativeUi.Destroy(ref _startTitle);
        NativeUi.Destroy(ref _startSub);
        NativeUi.Destroy(ref _startKey);
        NativeUi.Destroy(ref _pasteTitle);
        NativeUi.Destroy(ref _pasteSub);
        NativeUi.Destroy(ref _pasteKey);
        NativeUi.Destroy(ref _holdTitle);
        NativeUi.Destroy(ref _holdKey);
        NativeUi.Destroy(ref _banner);
    }

    public void Layout(Win32.RECT bounds)
    {
        var x = bounds.Left;
        var y = bounds.Top;
        var w = bounds.Right - bounds.Left;
        var keyW = 160;
        var keyX = bounds.Right - keyW;
        var textW = Math.Max(80, w - keyW - 12);

        NativeUi.Move(_section, x, y, w, 24);
        y += 36;

        NativeUi.Move(_startTitle, x, y, textW, 22);
        NativeUi.Move(_startKey, keyX, y, keyW, 22);
        NativeUi.Move(_startSub, x, y + 22, textW, 20);
        y += 56;

        NativeUi.Move(_pasteTitle, x, y, textW, 22);
        NativeUi.Move(_pasteKey, keyX, y, keyW, 22);
        NativeUi.Move(_pasteSub, x, y + 22, textW, 20);
        y += 56;

        NativeUi.Move(_holdTitle, x, y, textW, 22);
        NativeUi.Move(_holdKey, keyX, y, keyW, 22);
        y += 48;

        NativeUi.Move(_banner, x, y, w, 40);
    }

    public void Refresh()
    {
    }

    public bool HandleCommand(int id) => false;
}
