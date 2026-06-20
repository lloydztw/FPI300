# EzPlc.Fatek

永宏 PLC 模塊


## 資料夾
	<EzPlc.Fatek>
		│
		├── Address										(Fatek 位址 之實作)
		│
		├── Comm										(通訊層 模塊)
		│    │
		│	 ├── FatekCmdSpec							(Fatek 通訊指令 規格層)
		|	 │	  ├── .Enums							(Fatek 通訊指令 enums)
		|	 │	  └── Commands							(Fatek 通訊指令 實作)
		|	 │	 		
		|	 ├── FatekCommHost							(Fatek 串口通訊 載體)
		|	 │	  ├── 1_Commander						(Fatek 串口通訊 指令發送器, IxFatekComm 之實作)
		|	 │	  └── 2_API								(Fatek 串口通訊 舊有的介面, IxFatekApi 之實作)
		|	 │
		│	 └── FatekCommFactory.cs					(Factory Class, 由此生成 IxFatekApi 與 IxFatekComm)
		│
		├── IoMem								
		│	 ├── FatekIoMemory.cs						(Fatek 對 IoMemory 之實作)
		│	 ├── FatekIoPointBit.cs						(Fatek 對 1-Bit IO 點位 之實作)
		│	 └── FatekIoPointReg.cs						(Fatek 對 N-Bit IO 暫存器 之實作)
		│
		├── IoMemDevice								
		│	 └── FatekIoDevice.cs						(Fatek 對 IoDevice 之實作)
		│
		├── Scanner								
		│	 ├── FatekAutoScanner.cs					(Fatek 對 IoAutoScan 之實作)
		│	 └── FatekScanCmdsBuilder.cs				(AutoScan 指令群 建構器)
		│
		│
		└── EzFatekFactory.cs							(最外層的 Factory Class, 一切由此生成)


## 相依模塊
	- EzComm
	- EzIO.Mem
	- JetEazy.QUtility
	- NLog (5.3.4)

## Fatek Address 定址 

    輸入繼電器 X => 1-bit,  連續 10進制 定址 (如 X0 ~ X9999)
    輸出繼電器 Y => 1-bit,  連續 10進制 定址 (如 Y0 ~ Y9999)
    輔助繼電器 M => 1-bit,  連續 10進制 定址 (如 M0 ~ M9999)
    暫存器	  R => 16-bit, 連續 10進制 定址 (如 R0 ~ R9999)
    暫存器	  D => 16-bit, 連續 10進制 定址 (如 D0 ~ D9999)


	繼電器群組 WX => 16-bit (每16個X組成), 必須是16的倍數定址 (例如 WX0, WX16, WX32, ... , WX9984)
	繼電器群組 WY => 16-bit (每16個Y組成), 必須是16的倍數定址, 與 WX 類似 (例如 WY0, WY16, WY32, ... , WY9984)
	繼電器群組 WM => 16-bit (每16個M組成), 必須是16的倍數定址, 與 WX 類似 (例如 WM0, WM16, WM32, ... , WM9984)
					其中: 
						WM0 代表 M0 ~ M15
						WM16 代表 M16 ~ M31
							...
						WM9984 代表 M9984 ~ M9999

					WX, WY 也是套用相同規則


	繼電器群組 DWX => 32-bit (每32個X組成), 必須是32的倍數定址 (例如 DWX0, DWX32, DWX64, ... , WX9968)
	繼電器群組 DWY => 32-bit (每32個Y組成), 必須是32的倍數定址, 與 DWX 類似 (例如 DWY0, DWY32, DWY64, ... , WY9968)
	繼電器群組 DWM => 32-bit (每32個M組成), 必須是32的倍數定址, 與 DWX 類似 (例如 DWM0, DWM32, DWM64, ... , WM9968)
					其中: 
						DWM0 代表 M0 ~ M31
						DWM32 代表 M32 ~ M63
							...
						DWM9968 代表 M9968 ~ M9999

					DWX, DWY 也是套用相同規則


    暫存器32Bit	DR => 32-bit (每2個R組成), 必須是 2 的倍數定址 (例如 DR0, DR2, DR4, ... , DR9998)
    暫存器32Bit	DD => 32-bit (每2個D組成), 必須是 2 的倍數定址 (例如 DD0, DD2, DD4, ... , DD9998)
					其中:
						DR0 代表 R0, R1
						DR2 代表 R2, R3
							...
						DR9998 代表 R9989, R9999

					DD 也是套用相同規則
