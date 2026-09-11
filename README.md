# ZenVoice Windows

C# dictation app for Windows 11. The macOS app lives in [ZenVoice](https://github.com/imYashChaudhary973/ZenVoice).

```
dotnet test ZenVoice.slnx
dotnet publish src/ZenVoice.App/ZenVoice.App.csproj -c Release -r win-arm64 --self-contained true -o dist/win-arm64
```

On the machine: copy `dist/win-arm64` to `C:\ZenVoice` (or run `install.cmd`).

- Hotkey: `Ctrl+Alt+Space` start/stop
- Models: Distil / Turbo / Large V3 under `%LOCALAPPDATA%\ZenVoice\Models`
- Cloud speech is opt-in (your key in Credential Manager)
- Window: Home, Models, History, Personalisation, Shortcuts, Settings
- Closing the window does not quit; tray **Exit** does

Apache-2.0. Same product contract as the Mac app; not a Swift port.
