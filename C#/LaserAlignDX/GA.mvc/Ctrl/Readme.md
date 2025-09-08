# FPI300 (X3) 專案

將 MainSpace.MainX3UI 的 主控制邏輯 拉出來

# GaMainCtrl 面對上層的 class
	由 GaMvcConfig.CreateMainCtrl() 決定 來組態用哪一個優化階段的版本
	(目前時做是使用 GaMainCtrl_v3 + GaPlcFlyCameraCtrl)

## 其內, 會根據優化版本階段, 會各自調用以下 模塊
	GaMainCtrl_v0
	GaMainCtrl_v1
	GaMainCtrl_v2
	GaMainCtrl_v3 + GaPlcFlyCameraCtrl (2.6.0.0 之後的版本)

## 目前是使用 v3, 實測穩定沒問題後, 會撤除 v2, v1, v0
	v0 保留原始 MainX3UI 的控制代碼
	v1 從冗長的 process_OnMessage, 抽出 UPDATE_MVD_FUNCTIONS
	v2 使用 ChipCellsViewer 取代原來的 MVSUI 來顯示 晶粒檢測結果
	v3 於 v2 基礎上, 把飛拍控制拉出來到 GaPlcFlyCameraCtrl

## 2025-09-07 加入 GaCalibCtrl 
	與以下模塊 協同完成 新的 校正操作 介面
		Gui\UiCalibTool
		Model\CalibAoiModel
		Model\Coordinates

## 2025-09-09 加入 GaRecipeEditCtrl
	準備中 ...
