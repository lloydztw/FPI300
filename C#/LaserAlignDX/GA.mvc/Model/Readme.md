# FPI300 (X3) 專案

將來優化後的分層架構

# 1. AoiModel
	AoiModel_EmptyTray			(空盤檢查)
	AoiModel_ChipLoc			(晶粒定位)
	AoiModel_ChipMeasure		(晶粒尺寸)
	AoiModel_Defects			(瑕疵檢查)	(To Be Continued) (待分拆)
	AoiModel_ChipQrCode			(QRcode)	(To Be Continued) (待分拆)

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
	ReportBuilders			(檢測結果 報表生成)

# 5. Recipe
	請參考 GA.mvc\Model\Recipe\Readme.md

# 6. SysModel
	TravellerBigImagesHolder (巨圖管理)
	TravellerSysModel		 (整個系統模型)

