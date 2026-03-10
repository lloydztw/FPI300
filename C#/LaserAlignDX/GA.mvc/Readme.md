# FPI300 (X3) 專案
 
此處置放 FPI300 專用, 優化後的 的 Model-View-Control 架構

# Ctrl
	- 請查閱各自的 ReadMe.md

# Gui (View)
	- 請查閱各自的 ReadMe.md

# Model
	- 請查閱各自的 ReadMe.md

# AOI 可檢測項目:
	a0: 空盤檢測
	a1: 晶粒定位
	a2: 尺寸量測
	a3: 瑕疵檢測 
	a4: QrCode

# PLC 觸發狀態 (MACHINEx3.PLCIO.iScanStatus)
	0 (pss0) : 執行次序為 a1 -> a2 -> a3  
	1 (pss1) : 執行次序為 a1 -> a2 -> a4
	2 (pss2) : 執行次序為 a0
