; Constants
#define appname "PerfectWardApi" 
#define version "1.2"

#define dll_TaskScheduler "Microsoft.Win32.TaskScheduler.dll"
#define dll_MSSQLDriver "MSSQLDriver.dll"
#define dll_PerfectWardApi "PerfectWardApi.dll"
#define exe_PwTaskService "PwTaskService.exe"
#define exe_Installer "PerfectWard API Connector.exe"
#define exe_NSudo "NSudo.exe"

#define rp "bin"

; Setup window title
[Messages]
SetupAppTitle = Setup - {#appname}
SetupWindowTitle = Setup - {#appname} v{#version}

[Setup]
AppName={#appname}
AppVersion={#version}
WizardStyle=modern
DefaultDirName={autopf}\{#appname}
DefaultGroupName={#appname}
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=PerfectWardApi_v{#version}
OutputDir=_INSTALL_BUILD
PrivilegesRequired=admin
SetupIconFile=Connector\pw_logo_1line_transparent_210x35_1WE_icon.ico
UninstallDisplayIcon={app}\{#exe_Installer}
UninstallDisplayName={#appname} v{#version}
UsedUserAreasWarning=no

; Dirs
[Dirs]
Name: "{commonappdata}\PerfectWardAPI"; Permissions: everyone-full

; Built project files
[Files]
Source: {#rp}\{#dll_MSSQLDriver}; DestDir: "{app}"
Source: {#rp}\{#exe_PwTaskService}; DestDir: "{app}"
Source: {#rp}\{#dll_TaskScheduler}; DestDir: "{app}"
Source: {#rp}\{#dll_PerfectWardApi}; DestDir: "{app}"
Source: {#rp}\{#exe_Installer}; DestDir: "{app}"
Source: {#rp}\{#exe_NSudo}; DestDir: "{app}"

; Start menu icons
[Icons]       
Name: "{group}\API Installer"; Filename: "{app}\{#exe_NSudo}"; WorkingDir: "{app}"; Parameters: "-U:S ""{#exe_Installer}"""; IconFilename: "{app}\{#exe_Installer}"
Name: "{group}\Log Files"; Filename: "{commonappdata}\PerfectWardAPI"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"; WorkingDir: "{app}"

; Run after setup
[Run]
Filename: "{app}\{#exe_NSudo}"; WorkingDir: "{app}"; Parameters: "-U:S ""{#exe_Installer}"""; Flags: runascurrentuser nowait postinstall

; During uninstall, run installer with "-uninstall" cli arg and wait for termination.
; This will terminate PwTask processes, remove debug logs, unset environment variables and remove the scheduled task.
[UninstallRun]
Filename: "{app}\{#exe_Installer}"; Parameters: "-uninstall"; WorkingDir: "{app}"; StatusMsg: "PwApi Task is being uninstalled..."; Flags: waituntilterminated

; Hide "run list" from final page of setup
[Code]
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
    WizardForm.RunList.Visible := False;
end;