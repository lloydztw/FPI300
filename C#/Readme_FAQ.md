
# 軟件常見問題:


## 1. 第一次使用, 請執行 Dlls 下面的 通用庫 安裝包 
		
		JetEazyLibEx_4.8.x.x_Setup.exe


## 2 發生異常: OpenCvSharp.Internal.NativeMethods

- (2.1) 原因: 缺少 OpenCvSharpExtern.dll 與 opencv_videoio_ffmpeg480_64.dll
			於以下資料夾:
			
				bin/Debug/dll/x64
				bin/Debug/dll/x86
				bin/Release/dll/x64
				bin/Release/dll/x86

- (2.2) 解決方法:
	- (A) NuGet 移除 OpenCvSharp.Windows 後, 重新安裝
	- (B) 或者, 從其他專案的 (2.1) 資料夾, 複製到 本專案 bin 下.
		

## 3. 無法載入 dll

- (3.1) 確認 exe 專案 為 64-bit (不勾選 32-bit)

- (3.2) 確認 JetEazy.Crt.dll 版本正確, 並位在可取得的路徑
	   	- 可使用環境變量 設定路徑包含 C:\Program Files\Common Files\JetEazy


## 4. 瑕疵檢查 開機後 無法正常 判定
	- 原因1: 缺角區太大, 超過 參數設定的 瑕疵檢區塊 的 涵蓋範圍, 
			 軟件設計不佳, 目前暫時以 拉大 瑕疵檢區塊 的 涵蓋範圍 來解決.


## 5. 大陣列 Grid Builder
- To be continued

## 6. 使用選轉拉框 來框選 Golden
- To be continued
