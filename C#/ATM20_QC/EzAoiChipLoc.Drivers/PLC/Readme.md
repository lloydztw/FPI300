# Traveller106.IO

德龍 Traveller106 機台 IO

## 資料夾
	<EzTraveller106>
		│
		├── .Enum										(Enum)
		│
		├── FPIX3_IO									(使用 EzPlc.Omron 實現 IO 點位)
		│
		└── Gaara										(使用 FPIX3_IO 來實作 萬子的舊代碼)


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

	其中 i = 0 ~ 5
