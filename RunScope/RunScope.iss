[Setup]
AppName=RunScope
AppVersion=1
WizardStyle=modern
DefaultDirName={autopf}\RunScope
DefaultGroupName=RunScope
SourceDir=C:\Projects\Infrastructure\RunScope\bin\Release\net6.0-windows
OutputDir=C:\Projects\Infrastructure\RunScope\Output
OutputBaseFilename=RunScopeSetup

[Files]
Source: "*.*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\RunScope"; Filename: "{app}\RunScope.exe"
Name: "{commondesktop}\RunScope" ; Filename: "{app}\RunScope.exe"
