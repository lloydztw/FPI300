# JetEazy.WinForms.Recording

可獨立建置的 .NET Framework 4.8／AnyCPU WinForms 操作錄製 DLL，只參考 System、System.Core、System.Drawing、System.Windows.Forms、System.Xml。沒有 FPI30、Machine、Model、JetEazy 硬體 SDK 或其他專案參考。

## 使用內建操作面板

在 WinForms 主視窗的 UI 執行緒（例如 `OnLoad`）建立一個面板，並保存它。面板不會立即顯示，也不會自動錄製或重播。

```csharp
using JetEazy.WinForms.Recording;

private UiRecordingPanel _recordingPanel;

protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    _recordingPanel = new UiRecordingPanel(this);
}

private void RecordingMenu_Click(object sender, EventArgs e)
{
    _recordingPanel.OpenPanel();
}
```

- `Ctrl+Shift+R` 開啟面板；`Ctrl+Shift+F12` 停止錄製或重播。
- 操作流程為「開始錄製 → 停止 → 儲存錄製」，下次手動「選擇檔案重播」。
- 儲存副檔名為 `.uirec.xml`；開啟檔案仍支援舊的 `.fpiui.xml`。XML 格式版本仍為 1，既有錄製內容不需要轉檔。
- 主視窗 Dispose 時，面板及引擎會自動 Dispose；也可提早自行 Dispose。每個 UI 執行緒應只建立一個錄製器／面板，避免快捷鍵與錄製操作重複。
- 標題列系統選單整合由宿主負責；FPI30 的整合範例位於 `FormMainDX.UiRecording.cs`。

## 使用引擎建立自己的面板

```csharp
private UiActionRecorder _recorder;

private void InitializeRecorder(Form mainForm, Form customRecorderForm)
{
    _recorder = new UiActionRecorder(mainForm, customRecorderForm);
}

private void StartRecording() => _recorder.StartRecording();

private void StopAndSave(string path)
{
    _recorder.Stop();
    _recorder.Recording.Save(path);
}

private void Replay(string path) => _recorder.Play(UiRecording.Load(path));

private void StopReplay() => _recorder.Stop();
```

在各按鈕事件呼叫這些方法；StartRecording 與 Play 都不阻塞 UI 訊息迴圈，須保持 recorder 存活到操作結束。宿主 Dispose 時自動釋放引擎，也可提早呼叫 Dispose。`Changed` 事件與 `Status`、`IsRecording`、`IsPlaying` 可用於更新自訂面板；`ShowPanel` 事件表示使用者按了面板快捷鍵或重播發生錯誤。建構子第二個參數可省略，用來排除自訂錄製面板的操作。

公開資料型別：`UiRecording`、`UiRecordedAction`、`UiControlPart`、`UiActionKind`。控制項定位與動作派送實作為 internal，不作為宿主擴充契約。

## 支援範圍

標準 WinForms 按鈕、核取／單選按鈕、文字、下拉選項、單選清單、數值及分頁。文字以輸入結果記錄；密碼欄位不記錄。只觀察本程式 UI 執行緒的訊息，沒有全系統鍵盤／滑鼠掛鉤。

不支援自訂畫布拖曳、連續按住、滑鼠滾輪、多選清單、DataGridView 儲存格、ToolStrip 選單、原生檔案對話框／MessageBox、視窗關閉按鈕及自訂快捷鍵。包含這些操作的流程不能完整重播。

錄製檔仍對應原宿主的視窗型別、控制項 Name 與父子路徑，不能直接用於另一個介面不同的 App。重播前需回到相同起始畫面、語言與資料；控制項應設定穩定且唯一的 Name。每步保留最多 30 秒間隔，目標不可用時再等待最多 10 秒；名稱不唯一、選項文字改變或數值無效時停止。停止不會回復已執行的 UI 操作。

## 建置與驗證

在儲存庫根目錄執行：

```powershell
msbuild 'C#\JetEazy.WinForms.Recording\JetEazy.WinForms.Recording.csproj' /t:Build /p:Configuration=Debug
msbuild 'C#\JetEazy.WinForms.Recording\JetEazy.WinForms.Recording.csproj' /t:Build /p:Configuration=Release
& 'C#\UnitTests\UiRecording\RunChecks.ps1' -Configuration Debug
& 'C#\UnitTests\UiRecording\RunChecks.ps1' -Configuration Release
```

產物在 `bin/Debug` 或 `bin/Release`。測試先獨立建置 DLL，再以 DLL 參考編譯測試程式，不編入 DLL 原始碼、不載入 FPI30 或硬體。它驗證組件邊界、宿主釋放、Unicode 儲存、彈出視窗重播、取消、逾時與控制項／數值驗證。
