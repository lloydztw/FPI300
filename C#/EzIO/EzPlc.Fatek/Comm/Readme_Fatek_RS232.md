
# Communication Layer for FATEK PLC

永宏 PLC 通訊層 模塊 (使用 RS232)

## 資料夾
	<EzPlc.Fatek>
		│
		└── Comm										(通訊層 模塊)
			 ├── FatekCmdSpec							(Fatek 通訊指令 規格層)
			 │	  ├── .Enums							(Fatek 通訊指令 enums)
			 │	  └── Commands							(Fatek 通訊指令 實作)
			 │	 		
			 ├── FatekCommHost							(Fatek 串口通訊 載體)
			 │	  ├── 1_Commander						(Fatek 串口通訊 指令發送器, IxFatekComm 之實作)
			 │	  └── 2_API								(Fatek 串口通訊 舊有的介面, IxFatekApi 之實作)
			 │
			 └── FatekCommFactory.cs					(Factory Class, 由此生成 IxFatekComm 與 IxFatekApi)
		
## 相依模塊
	- EzComm
	- JetEazy.QUtility

