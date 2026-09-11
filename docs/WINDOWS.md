# Windows v1

This repository is the Windows C# app. macOS stays Swift in
[imYashChaudhary973/ZenVoice](https://github.com/imYashChaudhary973/ZenVoice).

C# + Win32/GDI (graphite/violet settings window). WinUI 3 is the long-term
skin, not what runs. whisper.cpp v1.9.1 is a DLL via P/Invoke, not the
XCFramework.

Code lives in `src/`. Tests: `dotnet test ZenVoice.slnx`.

## What v1 ships

Press `Ctrl+Alt+Space`, speak, press it again. Text lands in the focused app.
Local by default. Cloud speech only after **Use**.

| In | Out |
| --- | --- |
| Tray + Win32/GDI window (Home / Models / History / Personalisation / Shortcuts / Settings) | Parakeet |
| `Ctrl+Alt+Space` start/stop | Smart / Cloud formatting |
| WASAPI 16 kHz mono | Agentic / Command / Write |
| Whisper Distil / Turbo / Large V3 | Liquid Glass |
| Optional cloud (keys in Credential Manager) | Signed installer / Authenticode |
| Clipboard + type / Ctrl+V | |
| Encrypted history (AES-GCM, Credential Manager) | |
| `install.cmd` / `uninstall.cmd`, HKCU Run | |

Default engine: Distil under 12 GB or no discrete GPU; Distil for English
otherwise; Turbo for non-English.

Shared with Mac only as **copied contracts**: model SHA-256s, cloud URLs and
bodies, privacy rules, dictation phases. Not Swift types.

## Isolation

```
This repo                         C# solution (ZenVoice.slnx)
imYashChaudhary973/ZenVoice       Mac only. Do not edit it for Windows.
```

## Phases

### Phase 1 — Foundation (done)

Portable `net10.0` library. Compiles on macOS.

- `src/ZenVoice.Core`: engine IDs, model catalogue, transcript
  cleaner, cloud-speech request shapes, dictation phases
- `tests/ZenVoice.Core.Tests`: same cleaner cases as Mac
  `ZenVoiceCoreChecks`
- `dotnet test ZenVoice.slnx`

### Phase 2 — Dictation loop (done on UTM Windows 11 ARM64)

Win32 tray + HUD. `Ctrl+Alt+Space` start/stop. Real Whisper on the guest.

### Phase 3 — Product surface (mostly)

Home, Models, History, Personalisation, Shortcuts, Settings. AES-GCM SQLite
vault and cloud keys in Credential Manager.

### Phase 4 — Ship v1 (partial)

- `install.cmd` → `%LOCALAPPDATA%\Programs\ZenVoice`, HKCU Run
- `uninstall.cmd` (`/data` wipes `%LOCALAPPDATA%\ZenVoice`)
- No Authenticode; SmartScreen will warn on other PCs

## API map

| Job | Windows |
| --- | --- |
| Hotkey | `RegisterHotKey` |
| Mic | WASAPI → 16 kHz mono WAV |
| Insert | clipboard + type, then `SendInput` Ctrl+V |
| Secrets | Credential Manager / DPAPI |
| Models | `%LOCALAPPDATA%\ZenVoice\Models` |
| History | `%LOCALAPPDATA%\ZenVoice\Data` |
