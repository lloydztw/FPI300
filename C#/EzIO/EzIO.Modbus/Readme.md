# EzIO.Modbus

使用 Modbus通訊層 的設備, 對 IoDevice 的實作


## 資料夾
	<EzModbus>
		│
		├── Common			通用庫
		│
		├── HSL				(HSL 第三方庫, 已經被 NModbus4 取代)
		│
		├── NModbus4		(tcpip, RTU) (使用免費 NuGet NModbus4)
		│
		└── Scanner			(Modbus 設備 通用的 自動掃描器)


## 相依模塊
	- EzComm
	- EzIO.Mem
	- NModbus4		(NuGet)
	- NLog			(NuGet) (5.3.4)
	- HSL			(HslCommunication 保留)


## 定址
### 1. 暫存器類型與位址範圍
Modbus 將資料分為四種主要類型，每種類型在手冊中通常有一個 5 位數或 6 位數的定義代碼：

	資料類型				讀寫屬性		說明						手冊常見位址範圍
	
	Coils				讀/寫		數位輸出 (ON/OFF)		00001 - 09999
	Discrete Inputs		唯讀			數位輸入					10001 - 19999
	Input Registers		唯讀			類比輸入 (16-bit)		30001 - 39999
	Holding Registers	讀/寫		參數存取/控制 (16-bit)	40001 - 49999


換算公式：

    規範定義 <Modbus位址> = <PLC位址> + 1

	但是使用 NModbus4 (與 HSL) 都不加 1
