# ZenVoice Windows

C# dictation app for Windows 11. The macOS app lives in [ZenVoice](https://github.com/imYashChaudhary973/ZenVoice).

UI is Win32 + GDI (graphite/violet). WinUI 3 is the long-term skin, not what runs.

```
dotnet test ZenVoice.slnx
dotnet publish src/ZenVoice.App/ZenVoice.App.csproj -c Release -r win-arm64 --self-contained true -o dist/win-arm64
```

On the machine: `install.cmd` copies to `%LOCALAPPDATA%\Programs\ZenVoice` and adds HKCU Run. `uninstall.cmd` removes the app; `uninstall.cmd /data` also wipes `%LOCALAPPDATA%\ZenVoice`. No Authenticode.

- Hotkey: `Ctrl+Alt+Space` start/stop
- Mic: WASAPI 16 kHz
- Models: Distil / Turbo / Large V3 under `%LOCALAPPDATA%\ZenVoice\Models`
- Cloud speech is opt-in (key in Credential Manager)
- Insert: clipboard + type, then Ctrl+V
- Encrypted history
- Window: Home, Models, History, Personalisation, Shortcuts, Settings
- Closing the window does not quit; tray **Exit** does

Not in v1: Parakeet, Smart/Cloud formatting, Agentic/Command/Write, Liquid Glass, signed installer.

Apache-2.0. Same product contract as the Mac app; not a Swift port.
