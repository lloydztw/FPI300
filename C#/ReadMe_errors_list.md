# 勘誤表:

## 1 待處理之問題:
	- 0001: IxLineScanCam.GetFreeImageBitmap(int size = 0) 所取得的 FreeImageBitmap 是由誰 其維持生命週期?
	- 0003: 如何將 空盤檢測結果 傳換到 Gaara 的資料群組?
	- 0004: 原代碼有 BUG : Universal.Close() 貌似永遠不會被調用到 !!! ==> 改插入使用 Universal.Dispose() , 持續觀察中 ...
	- 0005: ProcessRunFPIClass._convert_lt_result_to_gaara 等待完成實作.
	- 0006: 專案 JetEazy.proj 如果更新 NuGet 套件 OpenCvSharp 4.8 會有相容性問題. ==> 待排查, 暫時使用低版本號 !

# 2 已修正之問題:
	- 0002: frmFPIRecipe 內的 xTimer 沒有人管理其生命週期! ==> 已修正 (LeTian @ 2025/07/31)
