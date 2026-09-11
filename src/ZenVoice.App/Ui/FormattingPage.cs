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
    private IntPtr _endpointLabel;
    private IntPtr _endpoint;
    private IntPtr _key;
    private IntPtr _save;

    public void Mount(IntPtr parent)
    {
        _section = NativeUi.Label(parent, UiIds.Formatting + 1, "Text Formatting", 0, 0, 480, 24);
        _level = NativeUi.Label(parent, UiIds.Formatting + 2, "Formatting level", 0, 0, 480, 24);
        _off = NativeUi.Radio(parent, UiIds.Formatting + 3, "Off", 0, 0, 480, 28);
        _clean = NativeUi.Radio(parent, UiIds.Formatting + 4, "Clean", 0, 0, 480, 28);
        _smart = NativeUi.Label(parent, UiIds.Formatting + 5, "Smart — Apple Intelligence, not on Windows", 0, 0, 480, 24);
        _cloud = NativeUi.Radio(parent, UiIds.Formatting + 6, "Cloud", 0, 0, 480, 28);
        _caption = NativeUi.Label(
            parent, UiIds.Formatting + 7,
            "Sends finished text only to your endpoint. Never audio.",
            0, 0, 480, 40);
        _endpointLabel = NativeUi.Label(parent, UiIds.Formatting + 8, "Endpoint", 0, 0, 480, 20);
        _endpoint = NativeUi.Edit(parent, UiIds.Formatting + 9, "", 0, 0, 480, 24);
        _key = NativeUi.Child(
            "EDIT", "",
            Win32.WS_BORDER | Win32.WS_TABSTOP | Win32.ES_AUTOHSCROLL | Win32.ES_PASSWORD,
            parent, UiIds.Formatting + 10, 0, 0, 360, 24);
        _save = NativeUi.Button(parent, UiIds.Formatting + 11, "Save", 0, 0, 72, 28);
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
        NativeUi.Destroy(ref _endpointLabel);
        NativeUi.Destroy(ref _endpoint);
        NativeUi.Destroy(ref _key);
        NativeUi.Destroy(ref _save);
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
        NativeUi.Move(_cloud, x, y + 152, w, 28);
        NativeUi.Move(_caption, x, y + 184, w, 40);
        NativeUi.Move(_endpointLabel, x, y + 228, w, 20);
        NativeUi.Move(_endpoint, x, y + 250, w, 24);
        var saveW = 72;
        NativeUi.Move(_key, x, y + 282, Math.Max(80, w - saveW - 8), 24);
        NativeUi.Move(_save, x + w - saveW, y + 280, saveW, 28);
    }

    public void Refresh()
    {
        var fmt = AppSettings.Current.Formatting;
        NativeUi.Checked(_off, fmt == "off");
        NativeUi.Checked(_clean, fmt == "clean");
        NativeUi.Checked(_cloud, fmt == "cloud");
        NativeUi.Text(_endpoint, AppSettings.Current.CloudFormatEndpoint);
    }

    public bool HandleCommand(int id)
    {
        if (id == UiIds.Formatting + 3)
        {
            return SetFormatting("off");
        }

        if (id == UiIds.Formatting + 4)
        {
            return SetFormatting("clean");
        }

        if (id == UiIds.Formatting + 6)
        {
            return SetFormatting("cloud");
        }

        if (id == UiIds.Formatting + 11)
        {
            AppSettings.Current.CloudFormatEndpoint = NativeUi.Text(_endpoint).Trim();
            AppSettings.Current.Save();
            var key = NativeUi.Text(_key).Trim();
            if (key.Length > 0)
            {
                CloudKeyStore.Set("cloud-format", key);
            }

            return true;
        }

        return false;
    }

    private bool SetFormatting(string value)
    {
        AppSettings.Current.Formatting = value;
        AppSettings.Current.Save();
        Refresh();
        return true;
    }
}
