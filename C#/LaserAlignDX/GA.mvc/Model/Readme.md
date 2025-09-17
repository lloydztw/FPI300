# FPI300 (X3) 專案

將來優化後的分層架構

# 1. AoiModel
	ChipLocator			(晶粒定位)
	ChipMeasurer		(晶粒尺寸)	(To Be Continued)
	ChipQrCode			(QRcode)	(To Be Continued)
	DefectsInspector	(瑕疵檢查)	(To Be Continued)
	EmptyTrayInspector	(空盤檢查)	(To Be Continued)

## 1.1 MVD LineSegment Finder
	GA\BasicSpace\MvdLineFinders\
		IMvdLineFinder.cs
		MvdFindLineClass_v1.cs
		MvdPairLineClass_v1.cs
## 1.2 MVD Template Matcher
	GA\BasicSpace\
		MvdFindClass.cs

# 2. CalibAoiModel
	校正所需要的像測 Aoi Model

# 3. 座標轉換系統
	PicGridPoints
	TravellerTransforms

# 4. CustomerModel
	DisplayTextFormatter (檢測結果 客製格式化文字)
	ReportBuilders		 (檢測結果 報表生成)

# 5. Recipe
	請參考 GA.mvc\Model\Recipe\Readme.md

# 6. SysModel
	TravellerBigImagesHolder (巨圖管理)
	TravellerSysModel		 (整個系統模型)

