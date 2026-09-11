# Windows v1

This repository is the Windows C# app. macOS stays Swift in
[imYashChaudhary973/ZenVoice](https://github.com/imYashChaudhary973/ZenVoice).

C# + WinUI 3 is to Windows what Swift + SwiftUI is to Apple. whisper.cpp
v1.9.1 is a DLL via P/Invoke, not the XCFramework.

## What v1 ships

Press a shortcut, speak, press it again. Text lands in the focused app.
Local by default. Cloud speech only after **Use**.

| In | Out |
| --- | --- |
| Tray + HUD (idle / listening / transcribing / success / error) | Parakeet, Smart formatting, Agentic / Command / Write |
| Global hotkey; hold = Right Ctrl | Fn-hold, live overlay, Liquid Glass |
| WASAPI 16 kHz mono | Sparkle (use a signed installer instead) |
| Whisper Distil / Turbo / Large V3 | Pixel-perfect Mac screens |
| Optional cloud speech (OpenAI, Gemini, Scribe v2, Grok) | Linux (phase after v1) |
| Clipboard + Ctrl+V, then UI Automation; copy-only if elevated | |
| Encrypted history (AES-GCM, Credential Manager) | |
| Models, Shortcuts, Privacy, History | |
| Signed installer; uninstall can delete `%LOCALAPPDATA%\ZenVoice` | |

Default engine: Distil under 12 GB or no discrete GPU; Distil for English
otherwise; Turbo for non-English.

Shared with Mac only as **copied contracts**: model SHA-256s, cloud URLs and
bodies, privacy rules, dictation phases. Not Swift types.

## Isolation

```
This repo                         C# solution (ZenVoice.slnx)
imYashChaudhary973/ZenVoice       Mac only. Do not edit it for Windows.
```

## Four phases to v1

Do not start N+1 until N’s gate is green.

### Phase 1 — Foundation (this phase)

Portable `net10.0` library. Compiles on this Mac. No WinUI, no whisper.dll,
no mic.

- `src/ZenVoice.Core`: engine IDs, model catalogue, transcript
  cleaner, cloud-speech request shapes, dictation phases
- `tests/ZenVoice.Core.Tests`: same cleaner cases as Mac
  `ZenVoiceCoreChecks`
- `dotnet test ZenVoice.slnx`

**Gate:** tests pass.

### Phase 2 — Dictation loop (done on UTM Windows 11 ARM64)

Win32 tray + HUD. `Ctrl+Alt+Space` start/stop. Fake-engine run on the guest
showed HUD **Listening** then **Success: Hello from zenvoice** and a clipboard
toast with the same text.

### Phase 3 — Product surface (started, vault proven)

- AES-GCM SQLite vault; key in Credential Manager
- Guest wrote `%LOCALAPPDATA%\ZenVoice\Data\history.db` after the test insert
- Tray: Download Distil, Delete all history
- Cloud: `ZENVOICE_ENGINE` + `ZENVOICE_CLOUD_KEY`

### Phase 4 — Ship v1 (started on the UTM guest)

- `install.cmd` / `uninstall.cmd` (`/data` wipes `%LOCALAPPDATA%\ZenVoice`)
- Auto-start: tray **Start with Windows** (HKCU Run) plus Startup folder
  `ZenVoice.cmd` on the guest
- Lives at `C:\ZenVoice\ZenVoice.exe` on the ARM64 VM (process in session 2)
- No Authenticode cert yet; SmartScreen will warn on other PCs

That binary is Windows v1. Linux is a later from-scratch shell, not this
WinUI app on GTK.

## API map (Phase 2+)

| Job | Windows |
| --- | --- |
| Hotkey | `RegisterHotKey` |
| Mic | WASAPI → 16 kHz mono WAV |
| Insert | `SendInput` Ctrl+V, then UIA `ValuePattern` |
| Secrets | Credential Manager / DPAPI |
| Models | `%LOCALAPPDATA%\ZenVoice\Models` |
| History | `%LOCALAPPDATA%\ZenVoice\Data` |

## Calendar

| Phase | Time |
| --- | --- |
| 1 Foundation | days (started) |
| 2 Dictation loop | 3–5 weeks on a Windows box |
| 3 Product surface | 2–3 weeks |
| 4 Ship v1 | 2 weeks |
| **Installable v1** | **~8–11 weeks after Phase 2 starts** |

Phase 2 is blocked on a Windows 11 machine. Phase 1 is not.
