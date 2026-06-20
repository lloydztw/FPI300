# EzPlc.Omron

歐姆龍 PLC 模塊


## 資料夾
	<EzPlc.Omron>
		│
		├── Address										(OMRON 虛擬位址 之實作)
		│
		├── IoMem										
		│	 ├── OmronIoMemory.cs						(OMRON 對 IoMemory 之實作)
		|	 ├── OmronIoVar.cs							(OMRON 對 IoPoint 實作之基礎抽象類別)
		|	 ├── OmronIoVarBool.cs						(OMRON 對 1-bit bool 型態 IoPoint 之實作)
		|	 ├── OmronIoVarFloat32.cs					(OMRON 對 32-bit float 型態 IoPoint 之實作)
		|	 ├── OmronIoVarInt32.cs						(OMRON 對 32-bit int 型態 IoPoint 之實作)
		│	 └── OmronIoVarStr.cs						(OMRON 對 string 型態 IoPoint 之實作)
		│
		├── IoMemDevice										
		│	 └── OmronIoDevice.cs						(OMRON 對 IoDevice 之實作)
		│
		├── Scanner								
		│	 └── EzOmronAutoScanner.cs					(OMRON 對 IoAutoScan 之實作)
		│
		└── EzOmronFactory.cs							(最外層的 Factory Class, 一切由此生成)	
		

## 相依模塊
	- EzComm
	- EzIO.Mem
	- JetEazy.QUtility
	- NLog (5.3.4)
	- CIPCompolet64
	- CIPCompoletProxyLib
	- CIPCompoletProxyServer


## 軟元件定址

使用到的點位： 

	0:Gvl_PhotoPC.bSyncClock
	0:Gvl_PhotoPC.sRecipeName
	0:Gvl_PhotoPC.bSoftwareReady
	0:Gvl_PhotoPC.bScanStart
	0:Gvl_PhotoPC.bScanReady
	0:Gvl_PhotoPC.bScanDone
	0:Gvl_PhotoPC.iScanResult
	0:Gvl_PhotoPC.bQRUsed
	0:Gvl_PhotoPC.bQRJudgeUsed
	0:Gvl_PhotoPC.iScanStatus
	
	0:Axis[i].bHomed
	0:Axis[i].bAbsMoveDone
	0:Axis[i].bLimitP
	0:Axis[i].bLimitN
	0:Axis[i].bCalibrationCam
	0:Axis[i].bEnable
    0:Axis[i].bError
	0:Axis[i].bStart
	0:Axis[i].bHomeStart
	0:Axis[i].bJogFW
	0:Axis[i].bJogNW
	0:Axis[i].rSped
	0:Axis[i].rFPOS
	0:Axis[i].rRPOS

	其中 i = 0 ~ N
