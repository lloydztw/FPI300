# 1 舊版問題:

## 1.1: IxLineScanCam.GetFreeImageBitmap(int size = 0) 所取得的 FreeImageBitmap 是由誰 其維持生命週期?
## 1.2: frmFPIRecipe 內的 xTimer 沒有人管理其生命週期! ==> 已修正
## 1.3: 如何將 空盤檢測結果 傳換到 Gaara 的資料群組?


# 2. 局部 待改善之處

## 2.1 所有的 Error codes 與 Error message 統一集中到 專屬的 Class 或 Enum
## 2.2 frmXXX 的 class 第一個字母 全部改為 "大寫" 改為 FrmXxx 或 FormXxx


# 3. 大架構 待改善之處

## 3.1 使用 繼承 與 virtual functions 取代 switch case
	- 目前不同的專案 都 使用 VersionEnum 與 OptionEnum 於執行時期 搭配 switch case 決定 程序的運行. 這樣非常不優 !!!
	- 改善: 
		以 MainControlUI 為例:
		-- 他的主要功能是實作於 MainX1UI, MainX2UI, MainX3UI 等 Classes
		-- 應該把 這些 Classes 抽出 共通的 Interface IvMainControlUI 繼承之.
		-- 於創始階段, 根據 VersionEnum 與 OptionEnum 來生成對應的 實作物件, 就可以避免後續一直重複使用 switch case.
		-- Object Oriented (台灣:物件導向) (中國:面對對象) 程式設計 的精隨之一, 就是用 繼承 + virtual functions 來取代 四處散落的 switch case.

## 3.2 使用 Model-View-Control 架構
	- 待續