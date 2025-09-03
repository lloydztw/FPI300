# 原有的 Enums.cs 太繁雜, 進行了以下分類:
 
## BasicEnums.cs
	- 此處存放 與 【個別機台專案】 無關 的 全域共用 Enums
	- JetEazy.Basics.dll 必須保持 與 【個別機台專案】 不相依, 才能通用於各種專案.

## ProjectEnums.cs
	- 此處存放 與 "個別機台專案" 相依 的 全域 Enums

## CameraEnums.cs
	- 此處存放 與 Camera 相關的 Enums

## MachineEnums.cs
	- 此處存放 與 Motor, PLC IO, Machine 相關的 Enums

## MiscEnums.cs
	- 其他 暫時無分類的 一大坨 Enums
