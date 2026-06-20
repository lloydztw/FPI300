#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Drawing;


namespace EzCamera.Interface
{
    /// <summary>
    /// 通用相機控制介面
    /// </summary>
    public interface IEzCamera : IEzCameraProps, IDisposable
    {
        /// <summary>
        /// 離開事件之後, LiveImage 會被 Sender 自動 Dispose() 而釋放.
        /// 事件接受者, 不需對 LiveImage 調用 Dispose().
        /// </summary>
        event EventHandler<EzLiveImageEventArgs> OnLiveImage;
        event EventHandler<EzCameraErrorEventArgs> OnError;
        event EventHandler OnLiveModeChanged;
        //>>> event EventHandler OnDeviceInfoChanged;

        /// <summary>
        /// 相機設備資訊
        /// </summary>
        IEzDeviceInfo DeviceInfo { get; }

        /// <summary>
        /// 可識讀名稱 
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// 同一台PC, 所有各廠家的相機全域編號
        /// (默認由生成順序自動編號,或可由開發者編定)
        /// </summary>
        int GlobalCamID { get; set; }

        /// <summary>
        /// 相機編號 (相對於同一廠家之相對編號)
        /// </summary>
        int CamID { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Init();

        /// <summary>
        /// 異常
        /// </summary>
        EzCameraError GetError();

        /// <summary>
        /// 是否為連續取像模式
        /// </summary>
        bool IsLiveMode();

        /// <summary>
        /// 開始即時連續取像
        /// </summary>
        void StartLiveMode();

        /// <summary>
        /// 停止即時連續取像
        /// </summary>
        void StopLiveMode();

        /// <summary>
        /// 主動拍照: 並傳回影像. 調用者必須負責 bmp.Dispose(),
        /// <br/> (1) 調用後 IsLiveMode() 會變為 False,
        /// <br/> (2) fireEvent == True 時, 會觸發 OnLiveImage 事件. 
        /// </summary>
        Bitmap Snapshot(bool fireEvent = false);

        /// <summary>
        /// 當 IsLiveMode()==false 時, 
        /// 此函式內部會調用 Snapshot(), 
        /// 並且觸發 OnLiveImage 事件.
        /// 否則無作用.
        /// </summary>
        void TriggerOneFrame();


#if (OPT_RESERVED)
        /// <summary>
        /// 將內部最後抓到的影像, 直接畫到 Graphics 上面 (保留)
        /// </summary>
        /// <param name="dst">目標 Graphics</param>
        /// <param name="rectDst">畫到 dst 方形區塊範圍</param>
        /// <param name="rectCrop">來源影像的 方形區塊範圍 </param>
        /// <param name="args">保留</param>
        void StretchBlt(Graphics dst, ref Rectangle rectDst, ref Rectangle rectCrop, object args = null);
#endif

        /// <summary>
        /// 取得像素顏色
        /// </summary>
        Color GetPixelColor(int x, int y);

        /// <summary>
        /// 是否內含 參數設定視窗
        /// </summary>
        bool HasPropertyPanel();

        /// <summary>
        /// 開啟或關閉 參數設定視窗
        /// </summary>
        void ShowPropertyPanel(bool show, object parentWindow = null);

        /// <summary>
        /// 是否為模擬相機
        /// </summary>
        bool IsSimulation();

        /// <summary>
        /// 當 IsSimulation()==True 可以開啟模擬影像檔案, 否則無作用.
        /// 以下特殊檔名, 會被 模擬相機 當成 指令:
        ///     [LAST_FILE]
        ///     [NEXT_FILE]
        ///     [PREV_FILE]
        ///     [DEFAULT]
        /// </summary>
        string Browse(string filePath);
    }
}
