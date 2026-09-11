@echo off
setlocal
set "DEST=%LOCALAPPDATA%\Programs\ZenVoice"
set SRC=%~dp0
if exist "%SRC%dist\win-arm64\ZenVoice.exe" set SRC=%SRC%dist\win-arm64\
if not exist "%SRC%ZenVoice.exe" (
  echo ZenVoice.exe not next to this script or in dist\win-arm64
  exit /b 1
)

taskkill /IM ZenVoice.exe /F >nul 2>&1
mkdir "%DEST%" >nul 2>&1
xcopy /E /I /Y "%SRC%*" "%DEST%\" >nul
copy /Y "%~dp0uninstall.cmd" "%DEST%\" >nul

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v ZenVoice /t REG_SZ /d "\"%DEST%\ZenVoice.exe\"" /f >nul

set "UNINST=HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\ZenVoice"
reg add "%UNINST%" /v DisplayName /t REG_SZ /d "ZenVoice" /f >nul
reg add "%UNINST%" /v Publisher /t REG_SZ /d "Yash Chaudhary" /f >nul
reg add "%UNINST%" /v InstallLocation /t REG_SZ /d "%DEST%" /f >nul
reg add "%UNINST%" /v DisplayIcon /t REG_SZ /d "%DEST%\ZenVoice.exe" /f >nul
reg add "%UNINST%" /v UninstallString /t REG_SZ /d "\"%DEST%\uninstall.cmd\"" /f >nul
reg add "%UNINST%" /v DisplayVersion /t REG_SZ /d "0.4.5" /f >nul
reg add "%UNINST%" /v NoModify /t REG_DWORD /d 1 /f >nul
reg add "%UNINST%" /v NoRepair /t REG_DWORD /d 1 /f >nul

set "LNK_SM=%APPDATA%\Microsoft\Windows\Start Menu\Programs\ZenVoice.lnk"
set "LNK_DESK=%USERPROFILE%\Desktop\ZenVoice.lnk"
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\ZenVoice.exe" >nul 2>&1
powershell -NoProfile -Command "$w=New-Object -ComObject WScript.Shell; $exe=Join-Path $env:DEST 'ZenVoice.exe'; $s=$w.CreateShortcut($env:LNK_SM); $s.TargetPath=$exe; $s.WorkingDirectory=$env:DEST; $s.Save(); $s=$w.CreateShortcut($env:LNK_DESK); $s.TargetPath=$exe; $s.WorkingDirectory=$env:DEST; $s.Save()"

start "" "%DEST%\ZenVoice.exe"
echo Installed to %DEST%
echo Starts with Windows. Tray icon: Ctrl+Alt+Space
exit /b 0
