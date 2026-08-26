@echo off
:: Copy application to ProgramFiles directory
robocopy "%~dp0MalfunctionBoard" "%ProgramFiles%\MalfunctionBoard" /E /COPYALL

:: Get path to user desktop
for /f "delims=" %%i in ('powershell -command "[Environment]::GetFolderPath('Desktop')"') do set "DesktopPath=%%i"

:: Define target executable and shortcut paths
set "TargetPath=%ProgramFiles%\MalfunctionBoard\MalfunctionBoard.exe"
set "ShortcutPath=%DesktopPath%\MalfunctionBoard.lnk"

:: Create VBScript for creating the shortcut
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%temp%\createShortcut.vbs"
echo sLinkFile = "%ShortcutPath%" >> "%temp%\createShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%temp%\createShortcut.vbs"
echo oLink.TargetPath = "%TargetPath%" >> "%temp%\createShortcut.vbs"
echo oLink.Save >> "%temp%\createShortcut.vbs"

:: Create shortcut and cleanup
cscript //nologo "%temp%\createShortcut.vbs"
del "%temp%\createShortcut.vbs"
