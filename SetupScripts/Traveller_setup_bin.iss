; -- Inno Step Script File --
;
; Traveller106
;

[Setup]
AppPublisher=JetEazy System Co., Ltd.
AppPublisherURL=http://www.jeteazy.com
AppVersion=3.3.5.3
AppCopyright=Copyright (C) 2025 JetEazy System Co., Ltd.
;WizardImageFile=JetEazySetup.bmp

AppName=Traveller_Patch
DefaultDirName=D:\AUTOMATION\Eazy FPI30
DefaultGroupName=JetEazy FPI30
Compression=lzma
SolidCompression=yes
OutputDir=.\bin
OutputBaseFileName=Traveller_Setup_3.3.5.3

[Files]
; BIN & DLL
Source: "..\bin\Debug\FPI30AOIX3.exe";                                            DestDir: "{app}\_BIN_";         Flags: ignoreversion
Source: "..\bin\Debug\Traveller.Data.Packer.exe";                                 DestDir: "{app}\_BIN_";         Flags: ignoreversion
Source: "..\bin\Debug\*.dll";                                                     DestDir: "{app}\_BIN_";         Flags: ignoreversion
Source: "..\bin\Debug\NLog.config";                                               DestDir: "{app}\_BIN_";         Flags: ignoreversion
Source: "..\C#\FPI30_Aoi\bin\Debug\EzAoiEmptyTrayInspector.App.exe";              DestDir: "{app}\_BIN_";         Flags: ignoreversion
;Source: "..\C#\Dlls\ja\*.dll";                                                    DestDir: "{app}\_BIN_\ja";      Flags: ignoreversion
Source: "..\bin\Debug\dll\x64\*.dll";                                             DestDir: "{app}\_BIN_\dll\x64"; Flags: ignoreversion

; Omron dlls and exe
Source: "..\C#\Dlls\Omron\*.*";     DestDir: "{app}\_BIN_";                        Flags: ignoreversion
Source: "..\C#\Dlls\Omron\ja\*.*";  DestDir: "{app}\_BIN_\ja";                     Flags: ignoreversion
Source: ".\*.cmd";                  DestDir: "{app}\_BIN_";                        Flags: ignoreversion  

; INI & DB 
; (these files must be kept existing even after uninstall!)
Source: "..\SettingsFiles\LASER-MAIN_FPIX3\*.*"; \
          DestDir: "{app}\_V03_\LASER-MAIN_FPIX3"; \
          Flags: recursesubdirs onlyifdoesntexist uninsneveruninstall      

[Dirs]
; Folder of INI, DB, & Recipes  
; (this folder must be kept existing even after uninstall!)
Name: "{app}\_V03_\LASER-MAIN_FPIX3";           Flags: uninsneveruninstall

[Icons]
Name: "{commondesktop}\Travller106 主程式";      Filename: "{app}\_BIN_\FPI30AOIX3.exe";                WorkingDir: "{app}\_BIN_"
Name: "{group}\Travller106 主程式";              Filename: "{app}\_BIN_\FPI30AOIX3.exe";                WorkingDir: "{app}\_BIN_"
Name: "{group}\Travller106 參數打包程式";        Filename: "{app}\_BIN_\Traveller.Data.Packer.exe";     WorkingDir: "{app}\_BIN_"

[Code]
