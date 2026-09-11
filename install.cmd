@echo off
setlocal
set DEST=%LOCALAPPDATA%\Programs\ZenVoice
set SRC=%~dp0
if exist "%SRC%dist\win-arm64\ZenVoice.exe" set SRC=%SRC%dist\win-arm64\
if not exist "%SRC%ZenVoice.exe" (
  echo ZenVoice.exe not next to this script or in dist\win-arm64
  exit /b 1
)

taskkill /IM ZenVoice.exe /F >nul 2>&1
mkdir "%DEST%" >nul 2>&1
xcopy /E /I /Y "%SRC%*" "%DEST%\" >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v ZenVoice /t REG_SZ /d "\"%DEST%\ZenVoice.exe\"" /f >nul
copy /Y "%DEST%\ZenVoice.exe" "%APPDATA%\Microsoft\Windows\Start Menu\Programs\ZenVoice.exe" >nul 2>&1
start "" "%DEST%\ZenVoice.exe"
echo Installed to %DEST%
echo Starts with Windows. Tray icon: Ctrl+Alt+Space
exit /b 0
