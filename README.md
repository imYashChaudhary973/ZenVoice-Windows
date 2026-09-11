# ZenVoice Windows

C# dictation app for Windows 11. The macOS app lives in [ZenVoice](https://github.com/imYashChaudhary973/ZenVoice).

UI is Win32 + GDI (graphite/violet). WinUI 3 is the long-term skin, not what runs.

```
dotnet test ZenVoice.slnx
dotnet publish src/ZenVoice.App/ZenVoice.App.csproj -c Release -r win-arm64 --self-contained true -o dist/win-arm64
```

On the machine: `install.cmd` copies to `%LOCALAPPDATA%\Programs\ZenVoice` and adds HKCU Run. `uninstall.cmd` removes the app; `uninstall.cmd /data` also wipes `%LOCALAPPDATA%\ZenVoice`. No Authenticode.

- Hotkey: `Ctrl+Alt+Space` start/stop; Right Ctrl hold; `Ctrl+Alt+V` paste-last
- Mic: WASAPI 16 kHz
- Models: Distil / Turbo / Large V3 / Parakeet TDT v3 (`parakeet.dll` required to decode Parakeet)
- Cloud speech and Cloud formatting are opt-in (key in Credential Manager; formatting sends text only)
- Insert: clipboard + type, Ctrl+V, then UIA
- Encrypted history
- Window: Home, Models, History, Personalisation, Shortcuts, Settings
- Closing the window does not quit; tray **Exit** does
- Sign: `scripts/sign.ps1` if `ZENVOICE_PFX` is set (see `docs/SIGNING.md`)

Not running yet: WinUI 3 shell (`src/ZenVoice.WinUI`), Smart formatting, Agentic/Command/Write.

Apache-2.0. Same product contract as the Mac app; not a Swift port.
