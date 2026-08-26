#define ApplicationName = "MalfunctionBoard"
#define Publisher = "Malfunctionz"
#define ExecutableName = "MalfunctionBoard.exe"

[Setup]
AppId={{0779fa1d-0f42-4d61-8d57-9694c8fa4660}}
AppName={#ApplicationName}
AppVersion={#ApplicationVersion}
AppPublisher={#Publisher}

DefaultDirName={autopf}\{#ApplicationName}
DefaultGroupName={#ApplicationName}

OutputDir=output
OutputBaseFilename=MalfunctionBoard-{#ApplicationVersion}-win

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

PrivilegesRequired=admin

Compression=lzma
SolidCompression=yes

WizardStyle=modern

UninstallDisplayName={#ApplicationName}

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; \
  Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{autoprograms}\{#ApplicationName}"
Filename: "{app}\{#ExecutableName}"

Name: "{autodesktop}\{#ApplicationName}"
Filename: "{app}\{#ExecutableName}"

[Run]
Filename: "{app}\{#ExecutableName}"
Description: "Launch {#ApplicationName}"
Flags: nowait postinstall skipifsilent
