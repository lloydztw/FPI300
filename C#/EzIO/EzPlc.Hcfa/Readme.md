# EzPlc.Hcfa

禾川 PLC 模塊


## 資料夾
	<EzPlc.Hcfa>
		│
		├── Address										(Hcfa 位址 之實作)
		│
		├── IoMem										
		│	 ├── HcfaIoMemory.cs						(Hcfa 對 IoMemory 之實作)
		|	 ├── HcfaIoPointBit.cs						(Hcfa 對 1-Bit IO 點位 之實作)
		│	 └── HcfaIoPointReg.cs						(Hcfa 對 N-Bit IO 暫存器 之實作)
		│
		├── IoMemDevice										
		│	 └── HcfaIoDevice.cs						(Hcfa 對 IoDevice 之實作)
		│
		└── EzHcfaFactory.cs							(最外層的 Factory Class, 一切由此生成)								
		

## 相依模塊
	- EzComm
	- EzIO.Mem
	- EzIO.Modbus
	- NModbus4		(NuGet)
	- NLog			(NuGet) (5.3.4)


## Hcfa Address 定址
這類 PLC 採用與三菱 FX 系列相似的編碼方式： 

    輸入繼電器 (X)：採 8進制 定址（如 X0~X7, X10~X17）。
    輸出繼電器 (Y)：採 8進制 定址（如 Y0~Y7, Y10~Y17）。
    輔助繼電器 (M)：採 10進制 定址（如 M0, M100）。
    資料暫存器 (D)：採 10進制 定址（如 D0, D1000）。

### 位元長度尾綴字母
	無 => 1-bit
	X => 1-bit
	B => 4-bit
	W => 16-bit
	D => 32-bit

### 字串標記 對照表
	I => 輸入繼電器 (X) 8進制
	Q => 輸出繼電器 (Y) 8進制
	M => 輔助繼電器 (M) 10進制

	IX <a.b> => 輸入繼電器 (X) 單點 (1-bit 打包) 8進制:
					IX0.0 ~ IX0.7
					IX1.0 ~ IX1.7
					IX2.0 ~ IX2.7
					IX3.0 ~ IX3.7
				
	QX <a.b> => 輸出繼電器 (Y) 單點 (1-bit 打包) 8進制:
					QX0     (0*8)       + 1024    
					QX1016  (1016*8)    + 1024  
					QX1144
					QX1272
					QX1400
					QX1528
					QX1656
					QX1784  (1784*8)    + 1024  

	QB <nnnn.b> => 輸出繼電器 (Y) 群組 (8-bit 打包, 每群包含8點) 8進制:
					QB1000.0
					QB1020.0
					QB1040.0
					QB1060.0
					QB1080.0
					QB1120.0
					QB1200.0
					QB1300.0
					QB1520.0
					QB1521.0
					QB1522.0
					QB1540.0
					QB1544.0
					QB1545.0
					QB1547.0	(也用於 input)
					QB1548.0	(也用於 input)
					QB1553.0

			正確使用規範是: 
				如果有 ".0" 代表 1-bit 點位, 即該 QB 的 bit0 
				如果沒有 ".0" 代表 整個 8-bit 數據  


	MW <nnnn> => 輔助繼電器 (M) 群組 (16-bit 打包, 每群包含16點, 10進制)：
					MW1000
					MW1020
					MW1040
					MW1060
					MW1100
					MW1300
					MW1340


## 通訊定址 (Modbus)
當使用 HMI（人機介面）或上位機讀取禾川 PLC 時，需對應到 Modbus 位址。
禾川通常提供對照表將內部 Y/X/M/D 轉換為 Modbus 的 0xxxx, 1xxxx, 3xxxx, 4xxxx 地址區段。

	Y (Q) => 0xxxx
	X (I) => 1xxxx
	M => 3xxxx
	D => 4xxxx

	一般 xxxx 是以 1 為 base
	ModbusAddress = (PLC_Address​) + 1


## 當前 Gaara 所用的 Modbus_From_HSL
	
	HSL 其定址轉換 並沒有 +1
	目前只用到 IX, QX, QB, MW 四種 Category

	IX 使用 ReadDiscrete
	QX 使用 ReadBool, Write (bool on)
	QB 使用 ReadBool, Write (bool on)
	MW 使用 ReadInt16, Write (int value)
