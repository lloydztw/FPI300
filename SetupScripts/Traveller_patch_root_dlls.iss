; -- Inno Step Script File for EzCounter --
;
; LETIAN: creation 20120204
;

[Setup]
AppPublisher=JetEazy System Co., Ltd.
AppPublisherURL=http://www.jeteazy.com
AppVersion=3.0.7.0
AppCopyright=Copyright (C) 2025 JetEazy System Co., Ltd.
;WizardImageFile=JetEazySetup.bmp

AppName=Traveller_Patch
DefaultDirName=D:\AUTOMATION\Eazy FPI30
Compression=lzma
SolidCompression=yes
OutputDir=.\bin
OutputBaseFileName=Traveller_Patch_3.0.7.0_black_white_online

[Files]
; BIN & DLL
Source: "..\bin\Debug\FPI30AOIX3.exe";                                            DestDir: "{app}\_BIN_";   Flags: ignoreversion
Source: "..\bin\Debug\*.dll";                                                     DestDir: "{app}\_BIN_";   Flags: ignoreversion
Source: "..\C#\FPI30_Aoi\bin\Debug\EzAoiEmptyTrayInspector.App.exe";              DestDir: "{app}\_BIN_";   Flags: ignoreversion

[Icons]

[Code]
