# JetEazy.VG.Devices
	將原本 JetEazy.csproj 當中, 由 Victor/Gaara 所定義的,  
	與 硬件設備 【相關】 的, 
	通用工具類別, 放在此專案。

## 1. 資料夾/檔案

	<JetEazy.VG.Devices>
		│
		├── .Interface									(VG 共用的 interfaces)
		│	 ├── IAxis.cs								(VG 共用的 馬達控制 介面)
		│	 ├── ICam.cs								(VG 共用的 Area 相機 介面)
		│	 ├── IxLineScanCam.cs						(VG 共用的 LineScan 相機 介面)
		│	 └── IxTickable.cs							(所有需要用 Timer.Tick 的 class 都繼承自此 介面)
		│
		├── BasicSpace									
		│	 └── JzTCPClass.cs							(VG Tcpip 通訊) 
		│		
		│		
		├── CCDSpace									(VG 共用的 相機控制 實作類別)
		│	 ├── CamLinkDriver
		|	 │		├── Euresys
		|	 │		│	 └── MultiCam.cs
		|	 │		├── Linescan_Dvp2.cs
		|	 │		├── LINESCAN_HUARUI.cs
		|	 │		├── Linescan_iTK_.cs
		|	 │		├── Linescan_Mind_.cs
		|	 │		└── Linescan_Sim_.cs
		|	 │	 		
		|	 ├── CAM_DAHUA.cs
		|	 ├── CAM_HIKVISION.cs
		|	 ├── CAMERAClass.cs
		|	 ├── Dvp2Class.cs
		|	 │
		│	 └── GaCameraFactory.cs						(VG 相機 Factory 類別, 所有相機都經由此 class 生成 ICam 或 IxLineScanCam)
		│		
		│
		├── ControlSpace								
		│	 │		
		│	 ├── MotionSpace							(VG 馬達控制 實作類別)
		│	 │		├── GeoMotionClass.cs				(VG IAxis 馬達抽象類別)
		│	 │		├── CanMotionClass.cs				(VG IAxis 為 CAN電機類型馬達之實作, 繼承自 GeoMotionClass)
		│	 │		└── PLCMotionClass.cs				(VG IAxis 為 PLC控制類型馬達之實作, 繼承自 GeoMotionClass)
		│	 │		
		│	 ├── MotoCanSpace							(CAN 總線電機控制 支援庫)
		│	 │		├── CanMotoControl.cs				(CAN 管理層)			(Victor/Gaara 的 包裝碼)
		│	 │		├── CanDeviceCore.cs				(CAN 通信層)			(API wrapper, 使用 CanDeviceCore.dll)
		│	 │		├── CanDeviceMotor.cs				(CAN 電機設備層)		(API wrapper, 使用 CanDeviceCore.dll)
		│	 │		├── SocketCan.cs					(物理通訊層 tcpip)	(API wrapper, 使用 SocketCan.dll)
		│	 │		└── UsbCan.cs						(物理通訊層 usb)		(API wrapper, 使用 UsbCan.dll)
		│	 │		
		│	 ├── PLCSpace								(VG 共用的 PLC/Device 實作類別)
		│	 │		|
		│	 │		├── PLCComm.cs						(VsCommPLC 最終對外呈現的類別; 需要根據不同專案 於編程時期 決定其父類別! (不優的設計))
		│	 │		|
		│	 │		├── CipCompoletClass.cs				(Omron PLC)		(使用 Omron 軟件庫)
		│	 │		├── CstLightClass.cs				(燈光控制)		(使用 串口通訊)
		│	 │		├── EzBarcodeM3DHelper.cs			(Barcode相關)	(使用 串口通訊)
		│	 │		├── FatekClass.cs					(永宏 PLC)		(使用 串口通訊)
		│	 │		├── Mitsubishi_FX3UClass.cs			(三菱 PLC)		(使用 串口通訊)
		│	 │		├── Modbus_From_HSL.cs				(Modbus)		(使用 HslCommunication 第三方軟件庫)
		│	 │		├── ModbusRTUClass.cs				(Modbus)		(使用 串口通訊)
		│	 │		└── ModbusTcpFromNNModbus4.cs		(Modbus Tcpip)	(使用 NModbus4 第三方軟件庫)
		│	 │		
		│	 │
		│	 ├── AddressClass.cs						(位址, 根據不同專案 需要 修改其實作 (不優的設計))
		│	 ├── AlarmsClass.cs
		│	 ├── CCDClass.cs							(??? 這個似乎應該放在 CCDSpace 裡面 ???)
		│	 ├── COMClass.cs							(串口通訊)
		│	 ├── EventClass.cs
		│	 └── PLCAlarmsClass.cs
		│		
		│
		├── GlobalSpace									
		│	 └── Enums
		│		  ├── CameraEnums.cs					(此處存放 與 Camera 相關的 Enums)
		│		  ├── MachineEnums.cs					(此處存放 與 Motor, PLC IO, Machine 相關的 Enums)
		│		  ├── ProjectEnums.cs					(此處存放 與 "個別機台專案" 相依 的 全域 Enums)
		│		  └── MiscEnums.cs						(其他 暫時無分類的 一大坨 Enums)
		│
		├── OPSpace							
		│	 └── CalibrationPlateClass.cs
		│
		├── PropertyGridSpace							
		│	 └── PropGrid_CaliClass.cs
		│
		└── UISpace										
			├── VsHandleMotorUI.cs
			├── VsMotorParasClass.cs
			├── VsTouchMotorClass.cs
			└── VsTouchMotorUI.cs


## 2. 架構優化

將來至少要拆分成三大模塊:
	Camera
	PLC
	Motor

	其中 Motor 模塊, 依據應用場合
	可以引用 PLC, 或是 直接調用 CAN dlls, 或是 直接調用 各廠家軸卡 dlls
