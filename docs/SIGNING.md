# Signing

This repo does not ship a PFX. Never commit one.

On the Windows machine that signs, set:

```
ZENVOICE_PFX          full path to your .pfx
ZENVOICE_PFX_PASSWORD password for that PFX
```

PowerShell for the current session:

```
$env:ZENVOICE_PFX = 'C:\certs\zenvoice.pfx'
$env:ZENVOICE_PFX_PASSWORD = '...'
```

User-level (persists):

```
[Environment]::SetEnvironmentVariable('ZENVOICE_PFX', 'C:\certs\zenvoice.pfx', 'User')
[Environment]::SetEnvironmentVariable('ZENVOICE_PFX_PASSWORD', '...', 'User')
```

Then, from the repo root after publish:

```
powershell -NoProfile -File scripts\sign.ps1
powershell -NoProfile -File scripts\sign.ps1 dist\win-arm64\ZenVoice.exe
```

Default target is `dist\win-arm64\ZenVoice.exe`. Extra args are extra files.

If the env vars are missing, the script prints one line and exits 0 so CI can call it without a cert.

SmartScreen still warns until the certificate is trusted (EV, or enough reputation on an OV cert). A self-signed or untrusted PFX will not clear that.
