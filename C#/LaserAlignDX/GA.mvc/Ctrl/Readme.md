# FPI300 (X3) 專案

將 MainSpace.MainX3UI 的 主控制邏輯 拉出來

# GaMainCtrl 面對上層的 class

## 其內, 會根據優化版本階段, 會各自調用以下 模塊
	GaMainCtrl_v0
	GaMainCtrl_v1
	GaMainCtrl_v2
	GaMainCtrl_v3 + GaPlcFlyCameraCtrl

## 目前是使用 v2, 下一階段優化目標的是使用 v3
	v0 保留原始 MainX3UI 的控制代碼
	v1 從冗長的 process_OnMessage, 抽出 UPDATE_MVD_FUNCTIONS
	v2 使用 ChipCellsViewer 取代原來的 MVSUI 來顯示 晶粒檢測結果
	v3 於 v2 基礎上, 把飛拍控制拉出來到 GaPlcFlyCameraCtrl
