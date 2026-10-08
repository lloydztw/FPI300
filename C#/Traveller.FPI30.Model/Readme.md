# Traveller.FPI30.Model

FPI30 Model 與 AOI 檢測程式庫，目標為 .NET Framework 4.8／C# 7.3。
主程式以 ProjectReference 使用此 DLL；此專案不參考 FPI30AOIX3 或 Traveller.FPI30.Machine。

## 已抽離

- Recipe 基底、RecipeFPIX3Class、參數、DTO 與樣板影像管理。
- 晶粒、載台格位、Lot、飛拍結果與相關列舉。
- RegionCellsDataCollection 與 Zigzag：格位集合、蛇形走訪、檢測統計及格位資料生命週期管理。
- 座標轉換介面、工廠、V38 實作與座標遷移。
- UTF-8 INI、影像轉換及上述模型需要的幾何輔助類別。
- 原 `GA.mvc/Model/AoiModel` 全部程式：AOI 介面、基底、ProcessRunFPIClass、空盤、定位、量測、瑕疵、連錫、讀碼、飛拍與支援類別，位於 `AoiModels`。
- AOI 需要的 MVD 比對、讀碼與找線工具，以及共用的 ProcessEventArgs。
- CalibAoiModel：校正介面、V25／V3／V5 校正演算法與 JxCalibRecipe。
- SysModel：ITravelerModel、TravellerSysModel 與 TravellerBigImagesHolder。
- LotDataHolder：批次資料、檔名、線掃／飛拍／格位影像存檔；PlcDataPacker：結果編碼、蛇形排列與診斷輸出。

原有 namespace 與參數檔格式保留。工廠由 `OPT_TRANSFORM_V38` 選用 V38；
既有 V36／V37 程式碼仍保留，以避免在抽離時一併改變歷史相容行為。
外部使用者需要與新 DLL 一起重新編譯。

## 主程式提供的服務

`LaserAlignDX.AppModelBindings` 由 AppBootstrapper 建立與釋放：

- 訂閱 `RecipeFPIX3Class.TrainingRequested`，執行原有 AOI 訓練。
- 訂閱 `TrainingFailed`，顯示原有訓練失敗通知。
- 指定 `CameraGridProvider`，提供系統模型擁有的相機格點；Recipe 不負責釋放借用的格點。
- 訂閱 `ModelDiagnostics.Message/Error`，將 DLL 日誌轉送至主程式。

服務在訂閱時不建立 AOI 或系統模型，維持原有延遲初始化順序。
關閉時先釋放應用資源，再解除訂閱及還原格點提供端。

其他應用可自行提供相同服務。未訂閱訓練事件時，載入／儲存 Recipe 只處理資料；
未設定格點提供端時，`xCamGrid1/2` 回傳 null；未訂閱日誌時使用 Trace。
`TrfMigration.CreateLinearMigration` 必須由呼叫端傳入共用校正檔完整路徑。
第一階段仍保留 Recipe 與轉換工廠的既有 singleton 行為，不代表已支援同一行程中的多套獨立模型。

### AOI 執行環境

`AppBootstrapper` 透過 `AppAoiServices` 建立 `AoiRuntime.Bind(...)` 綁定，提供四組介面：

- `IAoiContext`：當前載台、座標轉換、巨圖與空盤檢測服務；不包含 PLC 寫入或校正控制。
- `IAoiSettings`：檢測、執行緒、補償與影像儲存設定，每次讀取目前的主程式設定。
- `IAoiLotData`：批次資料、檔名及診斷影像輸出，轉接 Model DLL 的 LotDataHolder。
- `IAoiResultFormatter`：轉接 Model DLL 的 PlcDataPacker，維持結果排列與格式轉換；Model 不連線或寫入 PLC。

其他應用可實作這些介面，以 `new AoiServices(context, settings, lotData, results)` 綁定後使用 AOI。
綁定時不呼叫系統模型的建立函式，避免 SystemModel 與 AOI 延遲初始化互相遞迴。
未綁定時，ProcessRunFPIClass.Instance 在分配 SDK 資源前明確拋出例外。
目前仍沿用單一 AOI／Recipe 的 singleton 設計，同一行程只允許一個有效綁定；不支援執行中切換 host。
呼叫端須先停止所有檢測／背景工作，再釋放 AOI，最後 Dispose 綁定。綁定不擁有或釋放 host 提供的影像、轉換及服務。
主程式由 AppBootstrapper 管理上述次序。

### 系統模型與校正

`TravellerSysModel` 由呼叫端透過建構子提供 AOI、校正物件工廠、`ISystemModelEnvironment` 與 `ICoordinateWriter`。
主程式的 `AppSystemModelEnvironment` 轉接目前參數名稱與參數／座標轉換檔路徑；`AppCoordinateWriter` 將座標送到 FPIX3 的載台及吸嘴排。
Model DLL 不查詢 Universal.Machine、GaMvcPaths 或 LtAoiFactory，也不建立 PLC 連線。
原有 WriteCoordsToPlc／WriteAllCoordsToPlc 呼叫介面保留；座標計算留在 Model，實際 I/O 由呼叫端提供。
原有無 PLC 行為也保留：批次寫入回傳失敗；單筆流程仍依座標計算結果回傳，由主程式 writer 在 PLC 不存在時略過寫入。

校正物件仍延遲建立，由系統模型釋放；傳入的 AOI 與 host 服務為借用，不由系統模型釋放。
系統模型仍會釋放共用的座標轉換、空盤檢測與巨圖登錄，因此目前不支援多個獨立系統模型同時存活。
`DisposeSharedResources()` 供主程式在建構失敗時清理已建立的共用資源。
V25／V3／V5 類別及 namespace 均保留，由主程式既有的組態選擇校正版本。

### 批次資料與結果輸出

`LotDataHolder` 與 `PlcDataPacker` 保留 `LaserAlignDX.Model` namespace，位於 `Data`；共用文字格式工具 `StringBuilderExt` 位於 `Utils`。
使用儲存或結果格式功能前，呼叫端須以 `ModelDataRuntime.Bind(new ModelDataServices(settings, imageUpload, activeCarrier))` 提供：

- `IDataOutputSettings`：即時影像路徑、儲存旗標、品質、統一 NG 碼與模擬診斷路徑。
- `IImageUploadService`：上傳開關與已儲存影像的上傳動作；不用 FTP 的 host 可提供停用的實作。
- `Func<CarrierEnum>`：模擬檢查使用的當前載台。

主程式由 `AppDataServices` 轉接 INI／Universal，延遲建立原有 FtpUploader，保留 FTP 設定與傳輸實作在主程式。
AppBootstrapper 先綁定資料服務，再綁定 AOI；釋放時資料服務最後解除。
這組服務不需要 AOI 綁定即可使用；未綁定時會明確拋出例外。仍保留 LotDataHolder singleton，只允許一組有效資料服務。
呼叫端須先停止並完成所有背景存檔／上傳工作，才可釋放綁定。此次搬移未增加背景工作等待機制，也未改變既有非同步存檔行為。

### 飛拍 AOI Runner

`IFLyCameraAoiRunner` 由既有 `AoiModel_FlyCam` 實作，完整單張流程位於 `AoiModel_FlyCam.Runner.cs`。
`IProcessRunFPI.GetFlyCameraAoiRunner()` 與 `GetFlyCameraAoi()` 回傳同一個子模型的不同介面；不額外建立 matcher 或 singleton。
`Run(FlyID, Bitmap, FlyAoiRunOptions)` 負責 ROI 裁切、一般／特殊角度模式、選用讀碼、補償與 FlyAoiResult／FlyMetaData。
ROI、延伸量、模式、解析度及已選好的位置補償由 Controller 傳入；底層 matcher 門檻及訓練仍沿用 Recipe。
主 AOI 建立飛拍模型時注入原本的 IAoiQrDecoder，因此讀碼來源不變；Runner 借用它，不負責釋放。
無參數建構子保留供既有比對／訓練用途；若沒有注入 decoder 而要求完整 Runner 讀碼，會明確拋出例外。

Controller 保留四張影像反向順序、PLC 握手／結果陣列、載台補償選擇、UI 與 LotDataHolder 存檔。
輸入 Bitmap 與回傳 MetaData.bmpFly 均由 caller 擁有；同步 UI 更新及非同步存檔完成複製後，caller 才釋放影像。
完整執行、飛拍 matcher 訓練與釋放使用同一個鎖；主程式仍須避免其他 AOI 同時使用共用 QR decoder，並在釋放模型前停止相機回呼。
補償保留最後採用的 TranslateThenRotate 公式、超過 ±90° 的 XY 反向及原角度輸出。
一般比對也保留「集合非 null 即 OK」的既有判斷，並未順便改成要求集合非空。

此次整理另明確處理：單張 AOI 例外回報 NG／零補償，避免沿用上一張結果；相機 busy 旗標由 finally 清除。
離線四個顯示窗使用第一批的四個位置（FlyID(1, index)），不再把顯示窗 index 當成批次號產生無效的第 3／4 批。
特殊模式／例外的讀碼欄位為空字串，避免存檔沿用前一張條碼。

Runner 測試對實際 DLL 的執行核心提供替代 matcher／decoder，驗證 ROI、讀碼、補償邊界、特殊模式及影像所有權；
未執行需授權的 MVD 比對或實體飛拍，現場仍需使用原影像與設備確認。

## 相依與建置

需要既有 JetEazy／OpenCV／VisionDesigner 等 SDK，以及專案使用的 NuGet packages。
AOI 另使用原主程式的 MVD 演算法組件與 AUFindDotNet 結果型別；實際執行演算法仍需相符的原生 SDK 與授權。
直接專案參考包含 Contracts、JetEazy.Transform、JetEazy.PropertyGrid.Utils 與 EzAoiEmptyTrayInspector.Lib。
既有 PropertyGrid 編輯器 metadata 與 SDK 相依仍存在，這不是完全只依賴 .NET 基礎類別庫的資料 DLL。
四個 EzAoiEmptyTrayInspector csproj 未修改。

在 Visual Studio Developer PowerShell 執行：

```powershell
msbuild C#\Traveller.FPI30.Model\Traveller.FPI30.Model.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU
msbuild C#\Traveller.FPI30.Model\Traveller.FPI30.Model.csproj /t:Build /p:Configuration=Release /p:Platform=AnyCPU
```

發佈時須帶上相依 DLL 及輸出目錄中的 `dll` 原生函式庫子目錄。
現有 FreeImageNET 為 x64；雖然受控 Model 為 AnyCPU，目前執行驗證採 x64。

## 驗證

```powershell
& C#\UnitTests\ModelBoundary\RunChecks.ps1 -Configuration Debug
& C#\UnitTests\ModelBoundary\RunChecks.ps1 -Configuration Release
```

測試直接使用 Model DLL，檢查不載入主程式／Machine DLL、中文 INI 與 Recipe 存取、
V38 選用、格點座標、轉換登錄釋放、訓練回呼及主程式服務解除訂閱。
格位集合另驗證蛇形走訪、缺格補位、PASS／NG／空料／不明區塊統計，以及 Attach／TakeOver 的資料釋放責任。
AOI 邊界測試使用真正的 AppAoiServices 搭配測試 host，驗證延遲初始化、設定即時更新、批次資料共用、借用影像不被釋放、錯誤取消事件、結果轉送及 forceAllPass 判斷。
系統／校正測試使用合成圓點影像驗證 V25／V3／V5 格點與邊界裁切，再以 V38 計算已知座標。
測試使用真正的 AppCoordinateWriter 與替代 PLC 介面，檢查四組載台／吸嘴排對應、缺少 PLC、無效格點及正常／異常釋放的資源責任。
資料輸出測試使用真正的 LotDataHolder／PlcDataPacker，驗證結果碼、蛇形偏移、統一 NG 碼、替代格位及診斷 CSV，並以合成影像檢查線掃／格位存檔與上傳回呼；不連線 FTP。
測試不建立需授權的 MVD 演算法；實際定位／量測結果仍需以既有影像與 SDK 執行回歸比對。
測試資料只寫入測試的 `bin` 子目錄，不操作 PLC 或生產參數。

## 仍由主程式負責

校正 UI／Controller、報表、FTP 與 host 轉接器
仍由主程式編譯。Model 經由上述介面使用所需服務，不反向參考這些實作。
