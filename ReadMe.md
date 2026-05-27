# FPI30 專案群

## 資料夾

	<solution_root>
		│
		├── NewFPI30AOIX3								(主控程式)
		│	  └─ FPI30_Main.sln
		│
		├── FPI30_AOI									(AOI 空盤檢測 (LeTian))
		│	  ├─ EzAoiEmptyTrayInspector.Lib
		│	  ├─ EzAoiEmptyTrayInspector.App
		│	  └─ FPI30_Aoi.sln
		│
		├── JetEazy.Calib.Transform						(座標轉換基礎類別)
		│	  └─ JetEazy.Transform.csproj
		│
		├── Traveller.Data.Packer						(參數打包工具)
		│
		├── EzCamera.Gui.LiveViewer						(Jez Modules: Camera Viewer)
		├── JetEazy.Basics								(Jez Modules: 與"硬件無關"的共用庫)
		├── JetEazy.Calib								(Jez Modules: 舊版 座標轉換)
		├── JetEazy.VG.Camera							(Jez Modules: 與"硬件相關"的共用庫 (但不太通用, 還是會有個別專案相依的修改碼))
		└── JzDisplayLT									(Jez Modules: 慣用的 Image Viewer)



# 1. 源代碼架構整理

## 1.1 局部 待改善之處
- 1.1.1 所有的 Error codes 與 Error message 統一集中到 專屬的 Class 或 Enum
- 1.1.2 frmXXX 的 class 第一個字母 全部改為 "大寫" 改為 FrmXxx 或 FormXxx

## 1.2 大架構 待改善之處
- 1.2.1 使用 繼承 與 virtual functions 取代 switch case
	- 目前不同的專案 都 使用 VersionEnum 與 OptionEnum 於執行時期 搭配 switch case 決定 程序的運行. 這樣非常不優 !!!
	- 改善: 
		以 MainControlUI 為例:
		-- 他的主要功能是實作於 MainX1UI, MainX2UI, MainX3UI 等 Classes
		-- 應該把 這些 Classes 抽出 共通的 Interface IvMainControlUI 繼承之.
		-- 於創始階段, 根據 VersionEnum 與 OptionEnum 來生成對應的 實作物件, 就可以避免後續一直重複使用 switch case.
		-- Object Oriented (台灣:物件導向) (中國:面對對象) 程式設計 的精隨之一, 就是用 繼承 + virtual functions 來取代 四處散落的 switch case.
- 1.2.2 使用 Model-View-Control 架構
	- 請參考 [LeTian 的架構圖](Doc/mvc_diagram.jpg)

	

# 2. FPI30 源代碼 閱覽註解

## 使用硬件 (與其實作)
	- Camera
		* 大線掃相機 DVP2
			<hw_root>\CCDSpace\CamLinkDriver\ Linescan_Dvp2.cs

		* 飛拍相機 DVP2
			<hw_root>\CCDSpace\CamLinkDriver\ Linescan_Dvp2.cs

	- PLC
		* Omron
			<hw_root>\ControlSpace\PLCSpace\ CipCompoletClass.cs
		
	- Motor
		* 透過 PLC 控制馬達
			<hw_root>\ControlSpace\MotionSpace\ PLCMotionClass.cs
