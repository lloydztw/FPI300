# EzIO.Mem

IO 點位/暫存器/記憶體 模塊


## 資料夾
	<EzIO.Mem>
		│
		├── .Enums										(Enums)
		│
		├── CommonBase									(基礎抽象層)
		|	 ├── Support								(內部支援庫)
		|	 |
		|	 ├─── IAddress.cs							(IO 位址 接口)
		|	 ├─── IoAddress.cs							(IO 位址 抽象類別)
		|	 |
		|	 ├─── IoPoint.cs							(泛型 IO 點位 抽象類別)
		|	 ├─── IoPointBit.cs							(1-Bit IO 點位 基礎類別)
		│	 └─── IoPointReg.cs							(N-Bit IO 暫存器 基礎類別)
		│
		├── IoMem
		│	 ├─── IMemory.cs							(IO 記憶體管理池 之 Inteface)
		│	 ├─── IoMemory.cs							(IO 記憶體管理池 之 抽象類別)	
		|	 └─── IoMemoryBank.cs						(一組連續位址的 IO 點位集合 Memory Bank)	
		│
		├── IoMemDevice
		│	 ├─── IoAutoScan.cs							(自動掃描器)
		│	 ├─── IoDevice.cs							(IO 設備 接口)							
		|	 ├─── IoDevPointBit.cs						(具備實體設備連動功能的 1-Bit IO 點位 基礎類別)	
		│	 └─── IoDevPointReg.cs						(具備實體設備連動功能的 N-Bit IO 暫存器 基礎類別)	
		│
		├── Sim											(離線模擬器)
		│
		└── Utils										(內部工具庫)
		

## 相依模塊
	- NLog
