# FPI300 (X3) 專案
 
此處置放 FPI300 專用, 優化後的 的 Model-View-Control 架構

# Ctrl
	- 請查閱各自的 ReadMe.md

# Gui (View)
	- 請查閱各自的 ReadMe.md

# Model
	- 請查閱各自的 ReadMe.md

# AOI 可檢測項目:
	Aoi0: 空盤檢測
	Aoi1: 晶粒定位
	Aoi2: 尺寸量測
	Aoi3: 瑕疵檢測 
	Aoi4: QrCode

# PLC 觸發狀態 (IPlcIoFPIX3.iScanStatus)
	0 (pss0) : 執行次序為 Aoi1 -> Aoi2 -> Aoi3  
	1 (pss1) : 執行次序為 Aoi1 -> Aoi2 -> Aoi4
	2 (pss2) : 執行次序為 Aoi0

# PLC 自動傳遞/設定 Recipe
	目前寫在兩處 GUI
	1. FormMainDX.RUNUI_TriggerAction(...)
		- 收到 RunStatusEnum.CHANGERECIPE 訊息 (由 RuiUI 傳遞) 時
			調用 GUI function 來自動選定參數.

	2. RunUI.Tick()
		- 當 plcIO.bSoftwareReady == true 的時候, 會實時 Polling
		- 跟 GUI 不一樣的時候, 透過 RunStatusEnum.CHANGERECIPE 傳遞

	3. RunUI.btnSoftwareReady_Click()
		- 透過 RunStatusEnum.CHANGERECIPE 傳遞
