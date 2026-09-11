using System.Runtime.InteropServices;
using ZenVoice.Core;

namespace ZenVoice.App.Ui;

internal sealed class HistoryPage : ISettingsPage
{
    private readonly TranscriptVault _vault;
    private readonly Action? _deleteAll;
    private readonly List<string> _texts = [];
    private IntPtr _search;
    private IntPtr _copy;
    private IntPtr _delete;
    private IntPtr _list;
    private IntPtr _empty;

    public HistoryPage(TranscriptVault vault, Action? deleteAll = null)
    {
        _vault = vault;
        _deleteAll = deleteAll;
    }

    public string Id => "history";
    public string Title => "History";
    public string Subtitle => "Encrypted transcripts on this PC.";

    public void Mount(IntPtr parent)
    {
        _search = NativeUi.Edit(parent, UiIds.History + 1, "", 0, 0, 400, 28);
        _copy = NativeUi.Button(parent, UiIds.History + 4, "Copy selected", 0, 0, 140, 32);
        _delete = NativeUi.Button(parent, UiIds.History + 2, "Delete all history", 0, 0, 180, 32);
        _list = NativeUi.List(parent, UiIds.History + 3, 0, 0, 400, 200);
        _empty = NativeUi.Label(parent, UiIds.History + 5, "No dictations yet. Ctrl+Alt+Space to start.", 0, 0, 400, 40);
        Fill();
    }

    public void Unmount()
    {
        NativeUi.Destroy(ref _search);
        NativeUi.Destroy(ref _copy);
        NativeUi.Destroy(ref _delete);
        NativeUi.Destroy(ref _list);
        NativeUi.Destroy(ref _empty);
        _texts.Clear();
    }

    public void Layout(Win32.RECT bounds)
    {
        var w = bounds.Right - bounds.Left;
        NativeUi.Move(_search, bounds.Left, bounds.Top, w - 340, 28);
        NativeUi.Move(_copy, bounds.Right - 330, bounds.Top, 140, 32);
        NativeUi.Move(_delete, bounds.Right - 180, bounds.Top, 180, 32);
        var bodyTop = bounds.Top + 44;
        var bodyH = bounds.Bottom - bodyTop;
        NativeUi.Move(_list, bounds.Left, bodyTop, w, bodyH);
        NativeUi.Move(_empty, bounds.Left, bodyTop, w, 40);
    }

    public void Refresh() => Fill();

    public bool HandleCommand(int id)
    {
        if (id == UiIds.History + 4)
        {
            CopySelected();
            return true;
        }

        if (id == UiIds.History + 2)
        {
            _vault.DeleteAll();
            _deleteAll?.Invoke();
            Fill();
            return true;
        }

        if (id == UiIds.History + 1)
        {
            Fill();
            return true;
        }

        return id == UiIds.History + 3;
    }

    private void Fill()
    {
        if (_list == IntPtr.Zero)
        {
            return;
        }

        Win32.SendMessage(_list, Win32.LB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);
        _texts.Clear();
        var q = NativeUi.Text(_search).Trim();
        foreach (var row in _vault.List())
        {
            if (q.Length > 0 && row.Text.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            _texts.Add(row.Text);
            var line = $"{row.CreatedAt:yyyy-MM-dd HH:mm}  {row.Text}";
            Win32.SendMessage(_list, Win32.LB_ADDSTRING, IntPtr.Zero, line);
        }

        var empty = _texts.Count == 0;
        Win32.ShowWindow(_list, empty ? Win32.SW_HIDE : Win32.SW_SHOW);
        Win32.ShowWindow(_empty, empty ? Win32.SW_SHOW : Win32.SW_HIDE);
    }

    private void CopySelected()
    {
        var sel = Win32.SendMessage(_list, Win32.LB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt64();
        if (sel < 0 || sel >= _texts.Count)
        {
            return;
        }

        SetClipboard(_texts[(int)sel]);
    }

    private static void SetClipboard(string text)
    {
        if (!Win32.OpenClipboard(IntPtr.Zero))
        {
            return;
        }

        try
        {
            Win32.EmptyClipboard();
            var bytes = (text.Length + 1) * 2;
            var h = Win32.GlobalAlloc(Win32.GMEM_MOVEABLE, (UIntPtr)bytes);
            var ptr = Win32.GlobalLock(h);
            Marshal.Copy(text.ToCharArray(), 0, ptr, text.Length);
            Marshal.WriteInt16(ptr, text.Length * 2, 0);
            Win32.GlobalUnlock(h);
            Win32.SetClipboardData(Win32.CF_UNICODETEXT, h);
        }
        finally
        {
            Win32.CloseClipboard();
        }
    }
}
