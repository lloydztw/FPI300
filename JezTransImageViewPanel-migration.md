# 使用 JezTransImageViewPanel 取代 JzDisplayLT

基準：`ChatGpt_Revision`，commit `557e06eae9ccb478c7411497b05879c440f4800b`。
本文件記錄此基準之後的畫布替換與依賴移除修改。

## 替換範圍

飛拍設定視窗 `FormFlySetup_V1` 的 DS1、DS2 直接使用既有
`LaserAlignDX.Mvc.Gui.JezTransImageViewPanel`。新增 `CviFlySetupOverlay`
處理繪圖與滑鼠框選；它不讀取相機、Recipe 或 Universal。

| 原本的功能 | 替換方式 |
| --- | --- |
| `DispUI.ReplaceDisplayImage` | `JezTransImageViewPanel.UpdateImage`，複製 Bitmap 為畫布持有的 Mat |
| `CaptureAction` | interactor 將滑鼠位置轉為影像座標，送出 `RegionSelected` |
| 十字線 | 直接使用 JetEazy.ImageViewerEx.dll 內的 CviCross；每個畫布各自建立實例 |
| `Mover`、`JzRectEAG` 結果框 | 使用既有 CviRotRectBox，由 GaMvdExt.ToBox2D 轉換 Blob.BoxInfo，再加上 ROI 位移 |
| `DefaultView` | `MatViewer.RebuildViewport`；首次顯示及非框選模式下左鍵雙擊使用 |
| 座標顯示、縮放和平移 | 使用既有 panel 與底層 CvMatViewer |

ROI 正規化後裁切至影像範圍；原有範本儲存流程保留。
開始取像時取消已啟用的框選，避免使用即時畫面選取舊 Recipe 圖片。
即時影像 Bitmap 在同步 UI 更新後釋放；畫布自己的 Mat 在換圖及 Dispose 時釋放。
panel 的 Handle 重建不再釋放仍在使用的 Mat。視窗 Dispose 時解除新增的事件及 interactor。

結果框使用 Blob.BoxInfo 的旋轉框中心、寬高及角度，取代原先混用 RectInfo 寬高與
`-BoxInfo.Angle + 90` 的組合。計算角度的 AOI 演算法未修改；顯示框需在原開發機核對。
CviFlySetupOverlay 只繪製拖曳中的 ROI；十字線與結果框各自註冊為 interactor，
重新計算或關閉視窗時解除註冊。專案不新增 CviCross 類別，直接引用既有 DLL。

## 依賴調整

- 從 `C#/FPI30_Main.sln` 移除 JzDisplayLT 專案及其組態項目。
- 從主程式與 `Traveller.FPI30.Models.csproj` 移除 JzDisplayLT ProjectReference。
- 刪除 template editor 中未使用的 DispUI 屬性、using，以及主視窗停用的初始化程式。
- 主程式與 Models 的 MoveGraphLibrary 參考、無用 using 及舊圖形操作註解全部移除。
- 刪除已退出建置的 JzDisplayLT 目錄，以及 14 個仍使用舊圖形的退役視窗／控制項及其 Designer、resx，共 74 個檔案。刪除前確認它們未被範圍內任何 csproj 項目引用，且與 Git HEAD 相同，未覆蓋本地修改；原版仍可從 Git 歷史取回。
- 原有 JetEazy/OpenCV 參考保留。

依要求未分析或修改 EzEmptyTrayInspector.Lib、EzEmptyTrayInspector.App、
EzEmptyTrayInspector.TestDemo。

## 驗證狀態

已解析範圍內 14 個 csproj、檢查 606 個 Compile 項目及 EmbeddedResource 路徑，
以 Windows 大小寫規則核對，均可定位。範圍內所有原始碼與建置設定（含未編譯檔案）
均不再含 MoveGraphLibrary、JzRectEAG、Auxi_Geometry、Auxi_Convert 使用。
三個排除專案未修改。`git diff --check` 通過。

目前環境沒有 .NET Framework 4.8、MSBuild 及 Windows GUI 執行環境，
尚未完成編譯或操作驗證，不能視為已可部署。

需在原開發機完成：

1. Clean/Rebuild `C#/FPI30_Main.sln`，確認不需要 JzDisplayLT.dll 或 MoveGraphLibrary.dll，並可解析 DLL 內的 CviCross。
2. 載圖、單次取像及連續取像；檢查座標、十字線、縮放、平移與雙擊適配。
3. 縮放／平移後從不同方向框選 ROI，含越界、零面積、拖出控制項及失去滑鼠捕捉。
   確認儲存範本位置與原圖一致。
4. 確認 AOI 回傳角度未變，並核對旋轉框與 Blob.BoxInfo 的位置、寬高及方向（含非零 ROI 位移）。
5. 反覆換圖、開關飛拍視窗及重建 panel handle，檢查記憶體與 Dispose 行為。
