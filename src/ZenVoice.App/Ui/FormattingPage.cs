namespace ZenVoice.App.Ui;

internal sealed class FormattingPage : ISettingsPage
{
    public string Id => "personalisation";
    public string Title => "Personalisation";
    public string Subtitle => "How transcripts are cleaned before they land.";

    private IntPtr _section;
    private IntPtr _level;
    private IntPtr _off;
    private IntPtr _clean;
    private IntPtr _smart;
    private IntPtr _cloud;
    private IntPtr _caption;
    private IntPtr _banner;

    public void Mount(IntPtr parent)
    {
        _section = NativeUi.Label(parent, UiIds.Formatting + 1, "Text Formatting", 0, 0, 480, 24);
        _level = NativeUi.Label(parent, UiIds.Formatting + 2, "Formatting level", 0, 0, 480, 24);
        _off = NativeUi.Radio(parent, UiIds.Formatting + 3, "Off", 0, 0, 480, 28);
        _clean = NativeUi.Radio(parent, UiIds.Formatting + 4, "Clean", 0, 0, 480, 28);
        _smart = NativeUi.Label(parent, UiIds.Formatting + 5, "Smart", 0, 0, 480, 24);
        _cloud = NativeUi.Label(parent, UiIds.Formatting + 6, "Cloud", 0, 0, 480, 24);
        _caption = NativeUi.Label(
            parent, UiIds.Formatting + 7,
            "Smart formatting and Cloud formatting are macOS-only in this Windows v1.",
            0, 0, 480, 40);
        _banner = NativeUi.Label(
            parent, UiIds.Formatting + 8,
            "Replacements stay on this PC. Cloud mode sends finished text only.",
            0, 0, 480, 40);
        Refresh();
    }

    public void Unmount()
    {
        NativeUi.Destroy(ref _section);
        NativeUi.Destroy(ref _level);
        NativeUi.Destroy(ref _off);
        NativeUi.Destroy(ref _clean);
        NativeUi.Destroy(ref _smart);
        NativeUi.Destroy(ref _cloud);
        NativeUi.Destroy(ref _caption);
        NativeUi.Destroy(ref _banner);
    }

    public void Layout(Win32.RECT bounds)
    {
        var w = bounds.Right - bounds.Left;
        var x = bounds.Left;
        var y = bounds.Top;
        NativeUi.Move(_section, x, y, w, 24);
        NativeUi.Move(_level, x, y + 28, w, 24);
        NativeUi.Move(_off, x, y + 56, w, 28);
        NativeUi.Move(_clean, x, y + 88, w, 28);
        NativeUi.Move(_smart, x, y + 124, w, 24);
        NativeUi.Move(_cloud, x, y + 152, w, 24);
        NativeUi.Move(_caption, x, y + 184, w, 40);
        NativeUi.Move(_banner, x, y + 232, w, 40);
    }

    public void Refresh()
    {
        var off = AppSettings.Current.Formatting == "off";
        NativeUi.Checked(_off, off);
        NativeUi.Checked(_clean, !off);
    }

    public bool HandleCommand(int id)
    {
        if (id == UiIds.Formatting + 3)
        {
            AppSettings.Current.Formatting = "off";
            AppSettings.Current.Save();
            return true;
        }

        if (id == UiIds.Formatting + 4)
        {
            AppSettings.Current.Formatting = "clean";
            AppSettings.Current.Save();
            return true;
        }

        return false;
    }
}
