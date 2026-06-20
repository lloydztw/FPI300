# EzIO

使用 EzIO 架構之 PLC 軟件庫

## 資料夾
	<EzIO>
		│
		├── EzComm										(通訊層 (串口))
		│
		├── EzIO.Mem									(IO 點位 虛擬 暫存器/記憶體 之 模塊)
		│
		├── EzIO.Modbus									(支援 Modbus 協定 之 通訊層)										
		│
		├── EzPlc.Fatek									(永宏 Fatek PLC 對 IoMem/IoPoint 之實作)
		│	 └── EzFatekFactory							(永宏  的 點位 與其載體 都由此生成)					
		│
		├── EzPlc.Hcfa									(禾川 Hcfa PLC 對 IoMem/IoPoint 之實作)
		│	 └── EzHcfaFactory							(禾川 Hcfa 的 點位 與其載體 都由此生成)	
		│
		├── EzPlc.Omron									(歐姆龍 Omron PLC 對 IoMem/IoPoint 之實作)
		│	 └── EzOmronFactory							(歐姆龍 Omron 的 點位 與其載體 都由此生成)	
		│
		├── EzPlcTool.Gui								(有關 IoPoint 的 GUI 元件)	
		│
		├── TestDemo_Data								(演示用的數據庫)
		|	 ├── EzGlueDispenser						(演示 Hcfa 點位, 以 點膠機 為演示範例)
		|	 ├── EzTraveller106							(演示 Omron 點位, 以 德龍機台 為演示範例)
		|	 └── Presso									(演示 Fatek 點位, 以 壓合機 為演示範例)
		│
		├── TestDemo_EzPLC								(帶有 GUI 之 演示範例)
		|	 └── App
		|	 		├── DemoApp0						(最簡單的演示)
		|	 		├── DemoApp1						(Fatek 演示範例)
		|			├── DemoApp2						(Hcfa  演示範例)
		|			└── DemoApp3						(Omron 演示範例)
		|
		└── UnitTests_EzIO								(單元測試)								
		

## Quick Start 快速開始
	
1. 可以先直接 從 TestDemo_EzPLC 的 DemoApp0 入手.
	瀏覽一下 caller 端, 調用 IoMemory, IoPoint 的方式

2. 繼續瀏覽 DemoApp1 ~ DemoApp3

3. 瀏覽 EzPlc.XXX 的實作

