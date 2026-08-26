@echo off
setlocal enabledelayedexpansion

:: Config
set "AppName=MalfunctionBoard"
set "InstallDir=%ProgramFiles%\%AppName%"
set "SourceDir=%~dp0MalfunctionBoard"

:: Verify source files
if not exist "%SourceDir%\%AppName%.exe" (
  echo ERROR: Could not find %AppName%.exe
  echo Expected: "%SourceDir%\%AppName%.exe"
  pause
  exit /b 1
)

:: Install application
echo Installing %AppName%...
echo.

robocopy "%SourceDir%" "%InstallDir%" /E /COPY:DAT /R:2 /W:2

if errorlevel 8 (
  echo.
  echo ERROR: Failed to copy application files.
  pause
  exit /b 1
)

:: Create desktop shortcut
for /f "delims=" %%i in ('powershell -NoProfile -Command "[Environment]::GetFolderPath('Desktop')"') do set "DesktopPath=%%i"

set "TargetPath=%InstallDir%\%AppName%.exe"
set "ShortcutPath=%DesktopPath%\%AppName%.lnk"
set "VbsPath=%TEMP%\MalfunctionBoard_CreateShortcut.vbs"

echo Set oWS = WScript.CreateObject("WScript.Shell") > "%VbsPath%"
echo Set oLink = oWS.CreateShortcut("%ShortcutPath%") >> "%VbsPath%"
echo oLink.TargetPath = "%TargetPath%" >> "%VbsPath%"
echo oLink.WorkingDirectory = "%InstallDir%" >> "%VbsPath%"
echo oLink.Save >> "%VbsPath%"

cscript //nologo "%VbsPath%"
del "%VbsPath%" >nul 2>&1

:: Determine version from directory name
for %%A in ("%~dp0.") do set "DirName=%%~nA"

set "Version=!DirName:*%AppName%-=!"
set "Version=!Version:-win=!"

echo.
echo Installed %AppName% version %Version%

:: Remove downloaded ZIP
del "%~dp0..\%AppName%-%Version%-win.zip" >nul 2>&1

:: Schedule removal of installer directory
cd /d "%TEMP%"

start "" /b cmd /c "timeout /t 2 /nobreak >nul & rmdir /s /q "%~dp0""

exit /b 0
