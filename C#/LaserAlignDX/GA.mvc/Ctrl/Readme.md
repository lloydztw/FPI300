# FPI300 (X3) 專案

將 MainSpace.MainX3UI 的 主控制邏輯 拉出來

# GaMainCtrl 面對上層的 class
	由 GaMvcConfig.CreateMainCtrl() 決定 來組態用哪一個優化階段的版本
	(目前時做是使用 GaMainCtrl_v3 + GaPlcFlyCameraCtrl)

## 其內, 會根據優化版本階段, 會各自調用以下 模塊
	GaMainCtrl_v0 (保留原始 MainX3UI 的控制代碼)
	GaMainCtrl_v1 (從冗長的 process_OnMessage, 抽出 UPDATE_MVD_FUNCTIONS)
	GaMainCtrl_v2 (使用 ChipCellsViewer 取代原來的 MVSUI 來顯示 晶粒檢測結果)
	GaMainCtrl_v3 + GaPlcFlyCameraCtrl_v35 (3.0.7.3 之後的版本)

## 目前是使用 V3
	GaMailCtrl_v3 搭配 GaPlcFlyCameraCtrl_v35

## 2025-09-07 加入 GaCalibCtrl 
	與以下模塊 協同完成 新的 校正操作 介面
		Gui\UiCalibTool
		Model\CalibAoiModel
		Model\Coordinates

## 2025-09-09 加入 GaRecipeEditCtrl
	2025-09-14 搭配
		Gui\UiRecipeEditor
			\FormRecipeEditor
			\FormLightControl
			\IvRecipeEditor

## 2025-09-19 加入 GaTemplateEditCtrl
	搭配 Gui\UiRecipeEditor
			\FromTemplateEditor
			\IvTemplateEditorUI

## 2025-09-23 加入 GaPlcFlyCameraCtrl
	Ctrl\PlcFlyCamCtrl
			\GaPlcFlyCameraCtrl_v35
			\GaAutoDisableZoomCtrl
