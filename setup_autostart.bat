@echo off
title MihomoTray Startup Setup

set "EXE_PATH=%~dp0MihomoTray.exe"

echo ====================================================
echo             MihomoTray Autostart Setup
echo ====================================================
echo.
echo 1. Enable autostart on boot (Standard user, no UAC)
echo 2. Disable autostart
echo 3. Launch MihomoTray now
echo 0. Exit
echo.
set /p choice=Please select an option [1/2/3/0]: 

if "%choice%"=="1" goto ENABLE
if "%choice%"=="2" goto DISABLE
if "%choice%"=="3" goto START
if "%choice%"=="0" goto END
goto END

:ENABLE
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "MihomoTray" /t REG_SZ /d "\"%EXE_PATH%\"" /f >nul
echo.
echo [OK] Successfully registered startup entry in user registry!
echo Starting MihomoTray...
start "" "%EXE_PATH%"
echo [OK] Done! Look for the cat icon in your system tray.
pause
goto END

:DISABLE
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "MihomoTray" /f >nul 2>&1
echo.
echo [OK] Autostart disabled successfully!
pause
goto END

:START
start "" "%EXE_PATH%"
echo.
echo [OK] Launched! Look for the cat icon in your system tray.
timeout /t 2 >nul
goto END

:END
