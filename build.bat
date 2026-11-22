@echo off
title MihomoTray Build Script

echo =======================================================
echo          MihomoTray Build (No SDK Required)
echo =======================================================
echo.

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    set "CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist "%CSC%" (
    echo [ERROR] Windows built-in C# compiler csc.exe not found!
    pause
    exit /b 1
)

echo Compiling MihomoTray.exe ...
"%CSC%" /target:winexe /win32icon:app.ico /out:MihomoTray.exe MihomoTray.cs

if %errorlevel% equ 0 (
    echo.
    echo [OK] Build successful! Output: MihomoTray.exe
) else (
    echo.
    echo [ERROR] Build failed, please inspect the compiler error above.
)

echo.
pause
