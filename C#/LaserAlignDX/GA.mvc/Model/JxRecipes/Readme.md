# FPI300 (X3) 專案

將來優化後的分層架構

# 1. AoiModel
	ChipLocator			(晶粒定位)
	ChipMeasurer		(晶粒尺寸)	(To Be Continued)
	ChipQrCode			(QRcode)	(To Be Continued)
	DefectsInspector	(瑕疵檢查)	(To Be Continued)
	EmptyTrayInspector	(空盤檢查)	(To Be Continued)

# 2. CustomerModel
	DisplayTextFormatter (檢測結果 客製格式化文字)
	ReportBuilders		 (檢測結果 報表生成)

# 3. Recipe
## 使用 JxRecipeCombo 目前納入了
	EmtryTrayParams
	GaChipParams

## 目前沿用既有相關的 Classes
	OPSpace\RecipeSpace\
		RecipeFPIX3Class
			FlyParaClass
			InspectX3ParaClass
			NoTrayParaClass
			LineScanCalibrateClass

# 4. 目前 MVD Aoi 相關模塊
## Template Matcher
	BasicSpace\
		MvdFindClass

## LineSegment Finder
	BasicSpace\MvdLineFinders
		IMvdLineFinder
		MvdFindLineClass_v1
		MvdPairLineClass_v1
