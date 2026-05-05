# JetEazy.Basics
	將原本 JetEazy.csproj 當中, 由 Victor/Gaara 所定義的,  
	與 硬件設備 【無關】 的, 
	通用工具類別, 放在此專案。


## 資料夾/檔案

	<JetEazy.Basics>
		│
		├── .Enums										(此處存放 與 個別機台專案 【無關】 的 全域共用 Enums)
		│
		├── BasicSpace									
		│	 ├── AoiLib									(VG 共用的 AOI演算庫)
		│	 └── Utils									(VG 共用的 工具庫) 
		│		
		├── DBSpace										(VG 共用的 資料庫存取類別))
		│	 ├── AccDBClass.cs
		│	 ├── EsssDBClass.cs
		│	 ├── RCDBClass.cs
		│	 └── RUNDBClass.cs
		│
		├── FormSpace									(VG 共用的 Form 類別))
		│	 ├── AccountForm.cs
		│	 ├── LoginForm.cs
		│	 └── VsMessageBox.cs
		│
		├── PropertyGridSpace							(VG 自製 PropertyGrid UIEditor 所要用到的 TypeConverters)
		│	 └── UITypeConverters
		│			├── EnumTypConverter.cs
		│			└── NumericUpDownTypeConverter.cs
		│
		└── Resources									(FormSpace 中 VsMessageBox 所用到的圖片資源)
			├── information.png
			├── question.png
			└── warning-sign.png


## 專案依賴
	- JetEazy.Basics.dll 除了 JetEazy.QUtility 之外, 不依賴任何其他 JetEazy 【個別機台專案】的 dll, 才可以獨立使用於任何專案.
