# FPI300 (X3) 專案
 
準備將 主程式中 所有 錯誤訊息 與 提示字串 放置於此
以方便後續 進行 語言翻譯

## MVC 第一階段：應用程式物件生命週期

- `AppBootstrapper` 是主程式的物件組裝入口。`Program.Main` 以 `using` 持有它，讓正常退出、初始化失敗及主視窗建立失敗都走相同的釋放流程。
- `AppBootstrapper` 延遲透過 Model DLL 的 `TravellerModelFactory.Create()` 取得 `ITravellerSysModel`，再由系統模型延遲建立 AOI／校正模型。主控制器與飛拍控制器注入同一個系統 Model，AOI 及子模型透過 `IProcessRunFPI`／`IAoiXXX` 介面使用。
- `AppAoiSettings` 僅轉接 INI／Universal 設定。原有 `AppAoiServices` 已移除；系統狀態直接由 `ITravellerSysModel` 提供，LotDataHolder 直接實作 `IAoiLotData`，結果格式由 Model 內的 PlcDataPacker 負責。
- `GaMvcConfig` 保留既有方法作為相容入口，轉交 `AppBootstrapper`／`AppDialogService`；不再自行保管 Model 或 Controller。
- `AppDialogService` 負責既有編輯視窗的開啟。校正模型與報表工廠回傳的臨時物件仍由呼叫端保管；系統 Model 保管自己延遲建立的校正模型。
- `Universal.Initial`／`Dispose` 轉交組裝入口；設備實際初始化與關閉保留於 `InitializeCore`／`DisposeHardware`。
- 關閉順序為：解除控制器訂閱並釋放 Timer → 關閉設備 → 釋放系統 Model（內含 AOI、校正、共用影像／座標資料及 runtime 綁定）→ 釋放 Recipe → 解除 Model 事件及資料服務綁定。主程式不再單獨釋放 AOI。
- `AppResourceScope` 反序執行已登記的清理工作；某項清理失敗仍繼續處理其他項目，最後彙總例外。重複關閉不重複清理；關閉後禁止重新取得服務。

此階段保留既有 PLC／相機流程、AOI 演算法、全域 Recipe 與 Model 內部的相容存取。它們的資料介面與執行緒調度留待後續分層處理。尚未接軌的 `Traveller_FPI30_Models` 沒有同步搬移，避免混入兩份原始碼的演算法差異。

驗證入口：`C#/UnitTests/AppLifecycle/RunChecks.ps1`。它以正式的組裝入口及資源範圍原始碼搭配假設備／Model，檢查延遲建立、注入同一实例、關閉順序、初始化失敗、清理失敗及重複關閉，另以 Roslyn 檢查主專案語法；不啟動實際設備，也不修改現有 AOI 測試專案。

## 手動錄製與重播 UI 操作

主程式啟動後按 `Ctrl+Shift+R`，或在主視窗標題列按右鍵選「操作錄製 / 重播」。

1. 先回到要開始操作的畫面，按「開始錄製」，面板會隱藏。
2. 在 App 中操作；按 `Ctrl+Shift+F12` 停止並重新開啟面板。
3. 按「儲存錄製…」選擇 `.uirec.xml` 檔案。取消儲存不會清掉記憶體中的錄製；既有 `.fpiui.xml` 仍可開啟重播。
4. 下次啟動 App 後，先回到相同起始畫面與資料，再按「選擇檔案重播…」。不會在啟動時自動重播。
5. 重播期間仍可用 `Ctrl+Shift+F12` 停止；已執行的操作不會被回復。

支援標準 WinForms 按鈕、核取／單選按鈕、文字、下拉選項、單選清單、數值及分頁；鍵盤輸入以欄位結果記錄，Enter／Escape 對應標準預設／取消按鈕，Space 對應按鈕。密碼欄位不錄製。一般文字會存入 XML，儲存位置由使用者選擇。

不支援 AOI 自訂畫布拖曳、滑鼠滾輪／連續按住操作、多選清單、DataGridView 儲存格、ToolStrip 選單、Windows 原生檔案對話框／MessageBox、視窗關閉按鈕及自訂鍵盤快捷鍵。這些操作不會記錄，包含它們的流程不能完整重播；請先選擇只使用支援控制項的流程。

控制項以視窗型別與 Name、父子路徑定位；未命名控制項使用同型別同層索引，未命名按鈕另核對文字。每步保留最多 30 秒等待間隔，目標不可用時再等待最多 10 秒。視窗／控制項不唯一、選項文字改變、值超出範圍或等待逾時會停止，面板顯示原因。重播前應使用相同語言、Recipe 與起始畫面；它會執行實際 UI 事件，驗證請沿用 `SIM` 模式。

錄製引擎、檔案格式與操作面板已移至獨立的 [JetEazy.WinForms.Recording](../../JetEazy.WinForms.Recording/Readme.md) DLL。主程式只保留 `FormMainDX.UiRecording.cs` 的入口與標題列選單整合，透過 ProjectReference 使用 DLL。主視窗建立及釋放錄製面板；只使用此 App UI 執行緒的訊息，不安裝全系統鍵盤或滑鼠掛鉤。主視窗關閉時移除訊息篩選器與計時器。

驗證入口：`C#/UnitTests/UiRecording/RunChecks.ps1`，先獨立建置 DLL，再以 DLL 參考執行獨立測試視窗，檢查組件邊界／宿主釋放／錄製／Unicode 儲存／彈出視窗重播／取消／逾時及目標驗證，不載入機台或 AOI。
