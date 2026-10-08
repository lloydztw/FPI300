# FPIX3 Machine 與 Config

此階段將 FPIX3 的機台、PLC IO 與模擬 IO 從主程式移至獨立的 .NET Framework 4.8 DLL。共用列舉與 IO 的既有 namespace、列舉數值保留，以減少呼叫端及既有設定的變動；`LaserAlignDX` namespace 的共用列舉現在位於 Config，並不代表參考主程式。

IFpix3Machine 使用 namespace `Traveller.FPI30.Config`；專案、資料夾及輸出 DLL 均採用此名稱。

## 專案相依方向

- 主程式 → `Traveller.FPI30.Machine`、`Traveller.FPI30.Config`。
- Machine → Config、`JetEazy.Basics`、`JetEazy.VG.Devices`。
- Config → .NET Framework 基礎組件，沒有 PLC SDK、WinForms 或主程式參考。
- Machine 不參考主程式、Model、`GaMvcConfig` 或 `AppBootstrapper`。既有 JetEazy 底層組件仍包含部分 UI 與原生 SDK 相依，尚未在此階段重整。

Config 包含 `IFpix3Machine`、`IPlcIoFPIX3`、`IPlcIoFPIX3Sim`、`CarrierEnum` 與 `SuckerRowEnum`。目前固定使用 FPIX3，已移除機型選擇列舉。`IPlcIoFPIX3` 保留操作介面；接受 `VsCommPLC[]` 的初始化方法改由 Machine 內的 `IInitializableFpix3Io` 定義。

Machine 包含 `MainFPIX3MachineClass`、`MainFPIX3IOClass`、`MainFPIX3IOSim` 及共用基底 `GeoMachineClass`／`GeoIOClass`。主程式已移除 X1／X2／MiniX6 的 Machine、IO、Module 與 ChannelBarcode 依賴鏈及編譯項目；目前僅建立 FPIX3。

## 建立與使用

```csharp
using System;
using Traveller.FPI30.Config;
using Traveller.FPI30.Machine;

// 預設 1 個 PLC、16 個馬達、2 個光源。
IFpix3Machine machine = Fpix3MachineFactory.Create(workPath, motionCount: 16);
try
{
    if (!machine.Initial(isSimulation, isSimulation))
        throw new InvalidOperationException("FPIX3 initialization failed.");

    // 呼叫端依原有節奏呼叫 Tick；PLCIO 提供操作契約。
    machine.Tick();
    var io = machine.PLCIO;
}
finally
{
    machine.Close();
}
```

Factory 每次建立新的物件，回傳 `IFpix3Machine`；實作類別的建構子限制為 DLL 內部使用。`Create` 檢查工作路徑不可空白、裝置數量不可為負，允許零裝置的模擬測試。設定目錄固定為 `workPath/MAIN_FPIX3`。

`Create` 僅建立物件；`Initial` 才開啟裝置，尚未初始化也可以安全呼叫 `Close`。主程式以 `Universal.Machine`（`IFpix3Machine`）直接持有 Factory 回傳的物件，已移除原本的集合包裝。`AppBootstrapper` 經 `Universal.InitializeCore`／`DisposeHardware` 管理啟閉流程，初始化前先記錄機台，以便啟動失敗時關閉部分已開啟的連線。

一般 IO、Tick、重連與 FPS 操作使用介面；既有馬達、光源及 PLC 診斷功能仍有轉型至 `MainFPIX3MachineClass` 的相容存取，尚未完成這些驅動操作的契約化。Config 不因此加入硬體 SDK 參考。掃描狀態機、相機擷取、AOI 與 UI Controller 仍留在主程式，PLC 位址、結果排序與握手時序不變。

## 獨立建置與驗證

在 Visual Studio Developer PowerShell、儲存庫根目錄執行：

```powershell
msbuild 'C#\Traveller.FPI30.Config\Traveller.FPI30.Config.csproj' /t:Build /p:Configuration=Debug
msbuild 'C#\Traveller.FPI30.Machine\Traveller.FPI30.Machine.csproj' /t:Build /p:Configuration=Debug
& 'C#\UnitTests\MachineBoundary\RunChecks.ps1' -Configuration Debug
& 'C#\UnitTests\MachineBoundary\RunChecks.ps1' -Configuration Release
```

Machine 建置會連同 Config 與既有 JetEazy 相依專案建置，不需要建置或參考主程式；仍需要既有 JetEazy／硬體 SDK 建置環境。產物位於各專案的 `bin\Debug` 或 `bin\Release`。

檢查程式直接使用正式 DLL，驗證 Factory 的參數、預設數量與獨立實例、初始化前關閉、組件相依、共用列舉數值、模擬 IO 選擇、線掃→飛拍→空盤交握、重設及取消要求。模擬流程使用零 PLC／馬達／光源數量，不開啟實際硬體。實機連線、PLC 讀寫及馬達動作仍需現場驗證。
