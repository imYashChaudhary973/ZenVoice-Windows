@echo off
setlocal
set DEST=%LOCALAPPDATA%\Programs\ZenVoice
taskkill /IM ZenVoice.exe /F >nul 2>&1
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v ZenVoice /f >nul 2>&1
rmdir /S /Q "%DEST%" >nul 2>&1
if /I "%1"=="/data" (
  rmdir /S /Q "%LOCALAPPDATA%\ZenVoice" >nul 2>&1
  echo Removed app and %%LOCALAPPDATA%%\ZenVoice
) else (
  echo Removed app. Data kept. Use uninstall.cmd /data to wipe history and models.
)
exit /b 0
