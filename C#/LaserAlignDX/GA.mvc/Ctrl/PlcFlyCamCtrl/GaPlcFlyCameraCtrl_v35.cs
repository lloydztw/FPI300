#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *      2025-09-23 重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;

using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using LineScanProcess = TravellerMINIX6.ProcessSpace.LineScanProcess;
using PlcFlyResultCode = LaserAlignDX.PlcResultCode;

namespace LaserAlignDX.Mvc.Ctrl.V3
{
    /// <summary>
    /// 重整 MainX3UI 飛拍
    /// 將飛拍控制拉出來到 GaPlcFlyCameraCtrl
    /// AOI runs through IFLyCameraAoiRunner.
    /// PLC handshake, image ownership and UI remain in this controller.
    /// </summary>
    public class GaPlcFlyCameraCtrl : IxTickable, IDisposable
    {
        #region CONFIG
        static int TOTAL_FLY_FRAMES_COUNT => GaMvcConfig.TOTAL_FLY_FRAMES_COUNT;
        #endregion

        public event EventHandler OnFlyStarted;
        public event EventHandler OnFlyDone;

        #region MACHINE
        Traveller.FPI30.Contracts.IFpix3Machine MACHINE
        {
            get => Traveller106.Universal.Machine;
        }
        #endregion

        #region MODELS
        readonly ITravelerModel _sysModel;
        readonly AppResourceScope _resources = new AppResourceScope();
        volatile bool _disposed;
        IAoiFlyCamRunner _flyRunner => _sysModel.AoiModel.GetFlyCameraAoiRunner();
        #endregion

        #region GLOBAL_MESS_RECIPES
        RecipeFPIX3Class _xRecipe
        {
            get => RecipeFPIX3Class.Instance;
        }
        FlyParaClass _xFlyAoiParams
        {
            //get { return FlyParaClass.Instance; }
            get => _xRecipe.FlyAoiParams;
        }
        bool IsBusy()
        {
            var aoiModel = _sysModel.AoiModel;
            return LineScanSingleProcess.Instance.IsOn || LineScanProcess.Instance.IsOn || (aoiModel != null && aoiModel.Running);
        }
        #endregion

        #region EXTERNAL_PROCESS_不是很好的設計
        /// <summary>
        /// 主線掃 Process
        /// (目前暫時使用 Global 變量, 不是好的設計!)
        /// </summary>
        LineScanProcess m_LineScanProcess
        {
            get => LineScanProcess.Instance;
        }
        #endregion

        /// <summary>
        /// 是否跳過 飛拍相機的觸發事件.
        /// (目前暫時使用 Global 變量, 不是好的設計!)
        /// </summary>
        bool IsBypassFlyCameraTriggers
        {
            get => Traveller106.Universal.IsOpenFlyForm;
        }
        IxLineScanCam FlyCamera
        {
            get => Universal.IxFlyAreaCam;
        }

        #region PLC_CACHE_FLAGS
        volatile bool m_cachePlcStart = false;
        volatile bool m_cachePlcFlyDone = false;
        int m_cachePlcFlyStart = 0;
        #endregion

        #region THE_ON_THE_FLY_FRAME_BUFFERS
        volatile bool m_isOnTheFlyFrameEnabled = false;
        volatile bool m_isOnTheFlyFrameBusy = false;
        byte[][] m_onTheFlyFrameBuffers = new byte[TOTAL_FLY_FRAMES_COUNT][];
        int m_onTheFlyFrameCount = 0;
        #endregion

        #region LOT_DATA_FROM_PLC
        FlyLotData _lotData = new FlyLotData();
        #endregion

        #region GUI_MEMBERS
        Control _wndOwner;
        Control lblSerialNumber;
        IvFlyCamViewUI[] _DispUIs;
        string _flyCamSnTag;
        #endregion

        public GaPlcFlyCameraCtrl(ITravelerModel systemModel)
        {
            _sysModel = systemModel ?? throw new ArgumentNullException(nameof(systemModel));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _resources.Dispose(); }
            finally
            {
                OnFlyStarted = null;
                OnFlyDone = null;
            }
        }

        public void Attach(Control[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _wndOwner = DsFlys[0].Parent;
            AttachDispUIs(DsFlys);
            lblSerialNumber = lblFlyCameraSerialNo;
            EventHandler resetFrames = (s, e) => resetOnTheFlyFrameCount();
            _resources.Add(() => lblSerialNumber.DoubleClick -= resetFrames);
            lblSerialNumber.DoubleClick += resetFrames;
            var camera = FlyCamera;
            _resources.Add(() => camera.LineTriggerAction -= IxFlyAreaCam_LineTriggerAction);
            camera.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;
            var emptyTray = _sysModel.EmptyTrayAoiModel;
            EventHandler<EzAoiEmptyTrayInspector.Model.AoiResultEventArgs> finalResult =
                (s, e) => updateFlyCameraSerialNumber(-1);
            _resources.Add(() => emptyTray.OnFinalResulted -= finalResult);
            emptyTray.OnFinalResulted += finalResult;

            var language = QMSG.Lang("gui");
            EventHandler languageChanged = (s, e) => { if (!_disposed) updateFlyCameraSerialNumberTag(); };
            _resources.Add(() => language.LanguageChanged -= languageChanged);
            language.LanguageChanged += languageChanged;
            updateFlyCameraSerialNumberTag();
        }
        public void Tick()
        {
            if (_disposed) return;
            TickPlc();
        }

        void resetOnTheFlyFrameCount()
        {
            m_onTheFlyFrameCount = 0;

            // 以下改由 Ctrl + 鼠標右擊 進行 飛拍離線測試

            #region OLD_CODE
            //if (Universal.IsNoUseCCD)
            //{
            //    string _flyfilenamepath = JzToolsClass.OpenFilePicker("JPG Files (*.jpg)|*.JPG|" + "All files (*.*)|*.*", "");
            //    if (!string.IsNullOrEmpty(_flyfilenamepath))
            //    {
            //        using (var bmpFly = new Bitmap(_flyfilenamepath))
            //        {
            //            var flyID = new FlyID(1, 0);
            //            flyRunAoiOne(flyID, bmpFly);
            //        }
            //    }
            //}
            #endregion
        }
        void updateFlyCameraSerialNumberTag()
        {
            _flyCamSnTag = QMSG.Text("飛拍序號", "gui");
            lblSerialNumber.Text = _flyCamSnTag;
        }
        void updateFlyCameraSerialNumber(int serialNumber)
        {
            if (_disposed) return;
            _wndOwner?.Invoke(new Action(() =>
            {
                if (_disposed) return;
                var color = (serialNumber < 0 || Traveller106.Universal.IsOpenFlyForm) ? Color.Transparent : Color.Lime;
                //var text = "飛拍序號";
                //if (serialNumber > 0) text += $" : {serialNumber}";
                lblSerialNumber.Text =
                    serialNumber > 0 ?
                    $"{_flyCamSnTag} : {serialNumber}" :
                    _flyCamSnTag;
                lblSerialNumber.BackColor = color;
            }));
        }

        /// <summary>
        /// 注意: 此 Event Handler 執行於 background thread
        /// </summary>
        void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame camFrameInfo, IntPtr pBuffer)
        {
            if (_disposed) return;
            // 如果 Recipe 編輯窗被打開, 則跳過此 event handler
            // (不是很好的設計!)
            if (IsBypassFlyCameraTriggers)
            {
                return;
            }

            var plcIO = MACHINE?.PLCIO;
            if (plcIO == null)
                return;

            if (plcIO.bFlyReady && plcIO.iFlyStart > 0)
            {
                // 離線模擬版本才使用 m_isOnTheFlyFrameEnabled 這個限制變量.
                if (Traveller106.Universal.IsNoUseCCD)
                    if (!m_isOnTheFlyFrameEnabled)
                        return;

                // 防止重複進入
                if (m_isOnTheFlyFrameBusy)
                    return;

                // 設定 線程保護 旗標
                m_isOnTheFlyFrameBusy = true;
                try
                {

                    // 複製 Data Bytes
                    byte[] dataBytes = new byte[camFrameInfo.uBytes];
                    Marshal.Copy(pBuffer, dataBytes, 0, dataBytes.Length);

                    // 存入 OnTheFlyFrameBuffers
                    // 快取 累加前的 m_onTheFlyFrameCount (防止線程效應)
                    int frameIdx = m_onTheFlyFrameCount++;
                    frameIdx %= TOTAL_FLY_FRAMES_COUNT;
                    m_onTheFlyFrameBuffers[frameIdx] = dataBytes;

                    // 更新 GUI (frameCount 就是 Gaara 的飛拍序號)
                    updateFlyCameraSerialNumber(serialNumber: m_onTheFlyFrameCount);

                    // 集滿 所有 4次 飛拍 圖像
                    if (m_onTheFlyFrameCount >= TOTAL_FLY_FRAMES_COUNT)
                    {
                        // 清除 PLC 旗標 bFlyReady
                        plcIO.bFlyReady = false;

                        // 讀取 PLC iFlyStart
                        int flyStartIndex = plcIO.iFlyStart;

                        // 把收集到的 frame buffers 進行 飛拍 像測
                        this.RunAoiAll(flyStartIndex, m_onTheFlyFrameBuffers, camFrameInfo.iWidth, camFrameInfo.iHeight);
                        this.GetAoiAllResults(out var flyResults, out var flyOffsets);

                        // 回寫飛拍結果給 PLC
                        plcIO.iFlyResult(flyResults);
                        plcIO.rOffset(flyOffsets);

                        #region LOG
                        //int[] ints0 = m_iFlyResult;
                        //float[] floats0 = m_iFlyOffset;
                        //StringBuilder sb = new StringBuilder();
                        //foreach (var ix in ints0)
                        //{
                        //    sb.Append(ix.ToString() + ",");
                        //}
                        //_LOG("iFlyResult:" + sb.ToString(), Color.Black);
                        //StringBuilder sb1 = new StringBuilder();
                        //foreach (var ix in floats0)
                        //{
                        //    sb1.Append(ix.ToString() + ",");
                        //}
                        //_LOG("iFlyOffset:" + sb1.ToString(), Color.Black);
                        _LOG_FLY_RESULTS(flyResults, flyOffsets);
                        #endregion

                        // 重置 m_onTheFlyFrameCount
                        resetOnTheFlyFrameCount();
                        // 暫停 OnTheFlyFrameBuf
                        m_isOnTheFlyFrameEnabled = false;

                        // 設定 PLC 旗標 bFlyDone
                        plcIO.bFlyDone = true;

                        // 設定 PLC 旗標 bFlyReady
                        plcIO.bFlyReady = true;

                        OnFlyDone?.Invoke(this, null);
                    }

                // 清除 線程保護 旗標
                }
                finally { m_isOnTheFlyFrameBusy = false; }
            }
        }
        void TickPlc()
        {
            var plcIO = MACHINE?.PLCIO;
            if (plcIO == null)
                return;

            if (plcIO.bSoftwareReady)
            {
                // (1) 讀取 PLC bScanStart 訊號
                bool bScanStart = plcIO.bScanStart;
                // (1.1) 如果有變化
                if (bScanStart != m_cachePlcStart)
                {
                    m_cachePlcStart = bScanStart;

                    if (bScanStart)
                    {
                        _LOG("接收到 PLC 启动信号", Color.Black);

                        if (!m_LineScanProcess.IsOn)
                        {
                            // 暫時屏蔽 OnTheFlyFrameBuffer
                            m_isOnTheFlyFrameEnabled = false;

                            // 更新 LotData
                            string stripID = plcIO.sStripID;
                            string lotID = plcIO.sLotID;
                            _lotData = new FlyLotData(stripID, lotID);

                            // 啟動 PROCESS
                            m_LineScanProcess.Start();
                        }
                        else
                        {
                            _LOG("测试中 PLC 重复启动", Color.Red);
                            LtDebug.LOG.Error("测试中 PLC 重复启动");
                        }
                    }
                }

                // (2) 讀取 PLC iFlyStart 訊號
                int iFlyStart = plcIO.iFlyStart;
                // (2.1) 如果有變化
                if (iFlyStart != m_cachePlcFlyStart)
                {
                    m_cachePlcFlyStart = iFlyStart;

                    if (iFlyStart > 0)
                    {
                        OnFlyStarted?.Invoke(this, null);

                        _LOG($"接收到 PLC 飛拍{iFlyStart} 启动信号", Color.Blue);

                        // 重置 OnTheFlyFrameCount
                        resetOnTheFlyFrameCount();
                        updateFlyCameraSerialNumber(serialNumber: 0);

                        // 啟用 OnTheFlyFrameBuffer
                        m_isOnTheFlyFrameEnabled = true;
                    }
                    else
                    {
                        updateFlyCameraSerialNumber(serialNumber: 0);
                    }
                }
            }
        }

        #region LOG_FUNCTIONS
        void _LOG_FLY_RESULTS(int[] flyResults, float[] flyOffsets)
        {
            var sb = new StringBuilder();
            sb.Append("iFlyResult:");
            foreach (var ix in flyResults)
                sb.Append(ix).Append(',');
            _LOG(sb.ToString(), Color.Black);

            var sb1 = new StringBuilder();
            sb1.Append("iFlyOffset:");
            foreach (var ix in flyOffsets)
                sb1.Append(ix.ToString("0.000")).Append(',');
            _LOG(sb1.ToString(), Color.Black);
        }
        void _LOG(string msg, Color color)
        {
            GaUtil.LOG(msg, color);
        }
        #endregion

        #region RECIPE_OFFSETS
        PointF[] FlyOffsetUseStage
        {
            get
            {
                //var carrierID = _sysModel.ActiveCarrierID;
                //return carrierID == CarrierEnum.C1 ? xFlyPara.ptsOffset : xFlyPara.ptsOffset2;
                var plcIO = MACHINE?.PLCIO;
                var stageNo = plcIO != null ? plcIO.iScanStage : 1;
                return stageNo == 1 ? _xFlyAoiParams.ptsOffset : _xFlyAoiParams.ptsOffset2;
            }
        }
        #endregion

        #region TOTAL_RESULT_DATA_FOR_PLC
        int[] m_iFlyResult = new int[4];
        float[] m_iFlyOffset = new float[4 * 3];
        #endregion

        void AttachDispUIs(Control[] dispUIs)
        {
            _wndOwner = dispUIs[0].Parent;
            _DispUIs = Array.ConvertAll(dispUIs, ui => buildFlyCamViewer(ui));
            connectPopupMenuEvents(_DispUIs);
        }
        IvFlyCamViewUI buildFlyCamViewer(Control panel)
        {
            //(1) 如果傳進來的已經是 JezFlyViewPanel
            if (panel is JezFlyViewPanel jezViewer)
            {
                return jezViewer;
            }
            //(2) 如果傳進來的是其他視窗控件
            else if (panel is Control childWnd)
            {
                // 生成新的 JezFlyViewPanel
                var viewer = new JezFlyViewPanel
                {
                    Location = childWnd.Location,
                    Size = childWnd.Size,
                    Dock = childWnd.Dock,
                    Visible = true,
                };
                // 與舊的 childWnd 互換角色
                var parent = childWnd.Parent;
                childWnd.Visible = false;
                parent.Controls.Add(viewer);
                return viewer;
            }
            //(3) 不支援
            else
            {
                return null;
            }
        }
        void connectPopupMenuEvents(IvFlyCamViewUI[] dispUIs)
        {
            if (dispUIs == null)
                return;

            int idx = 0;
            foreach (var ui in dispUIs)
            {
                if (ui is JezFlyViewPanel flyPanel)
                {
                    flyPanel.menuTestFlyCamAoi.Click += MenuTestFlyCamAoi_Click;
                    flyPanel.menuTestFlyCamAoi.Tag = idx;
                }
                idx++;
            }
        }

        #region EVENT_HANDLERS
        /// <summary>
        /// 離線測試飛拍AOI
        /// </summary>
        private void MenuTestFlyCamAoi_Click(object sender, EventArgs e)
        {
            var plcIO = MACHINE?.PLCIO;
            if (plcIO != null && plcIO.bSoftwareReady)
            {
                //VsMessageBox.Warning("【軟件準備】 已經啟動連線, 無法進行 飛拍 離線測試!");
                VsMessageBox.Warning(QMSG.Text(Prompts.Waring_Software_Ready_Can_NOT_Simulate_Fly_Camera));
                return;
            }

            if (sender is ToolStripMenuItem menuItem)
            {
                if (menuItem.Tag is int idx && idx >= 0 && idx < _DispUIs.Length)
                {
                    string fileName = GaUtil.BrowseImageFile();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
                        using (Bitmap bmpFly = GaImageUtil.LoadBigImage(fileName, autoSaveJpg:true))
                        {
                            var flyID = new FlyID(1, idx);
                            flyRunAoiOne(flyID, bmpFly);
                        }
                        GaUtil.SetCursor(_wndOwner, oldCursor);
                    }
                }
            }
        }
        #endregion

        void RunAoiAll(int flyStart, byte[][] framesBufBytes, int frameWidth, int frameHeight)
        {
            int index = 0;
            for (int iFrameIdx = framesBufBytes.Length - 1; iFrameIdx >= 0; iFrameIdx--, index++)
            {
                // 為何要倒著順序 取出 frame buffer?
                byte[] bytes = framesBufBytes[iFrameIdx];

                using (var bmpFly = GaImageUtil.CreateBitmapU8(bytes, frameWidth, frameHeight))
                {
                    var flyID = new FlyID(flyStart, index);
                    flyRunAoiOne(flyID, bmpFly);
                }
            }
        }
        void GetAoiAllResults(out int[] flyResults, out float[] flyOffsets)
        {
            // flyResults 飛拍結果 定義:
            //      1: Ok,
            //      2: Ng,
            //      3: 空
            flyResults = m_iFlyResult;

            // flyOffsets 飛拍補償 定義:
            //      (X, Y, R) 4組, 一共 12 個 float
            flyOffsets = m_iFlyOffset;
        }

        void flyRunAoiOne(FlyID flyID, Bitmap bmpFly)
        {
            FlyAoiResult result;
            try
            {
                var parameters = _xFlyAoiParams;
                var options = new FlyAoiRunOptions
                {
                    TemplateRegion = _xRecipe.xRectRegionPrintFly,
                    ExtendX = parameters.xExtendx,
                    ExtendY = parameters.xExtendy,
                    UseSpecialAngle = parameters.xIsOpenMuit,
                    DecodeCode = parameters.bOpenCodeReader,
                    ImageResolution = INI.Instance.FlyImageResolution,
                    PositionOffset = parameters.xIsOpenMuit ? PointF.Empty : FlyOffsetUseStage[flyID.ShowID - 1]
                };
                result = _flyRunner.Run(flyID, bmpFly, options);
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "FlyCameraAoiRunner.Run");
                // Never send a previous frame's result after an AOI failure.
                result = new FlyAoiResult
                {
                    Code = PlcFlyResultCode.NG, CodeStr = string.Empty,
                    MetaData = new FlyMetaData { flyID = flyID, bmpFly = bmpFly, AlgorithmName = "AOI_Error" }
                };
                result.MetaData.flyAoiResult = result;
            }
            updateOneResult(flyID, result);
            _lotData.CodeStr = result.CodeStr ?? string.Empty;
            // Save and display while the caller-owned image is still alive.
            AsyncSaveFlyCameraImage(flyID, bmpFly, _lotData);
            _DispUIs[flyID.flyIndex].Update(result);
        }
        void updateOneResult(FlyID flyID, FlyAoiResult oneResult)
        {
            int flyIndex = flyID.flyIndex;
            if (oneResult != null && oneResult.Code == PlcFlyResultCode.OK)
            {
                m_iFlyResult[flyIndex] = (int)oneResult.Code;
                m_iFlyOffset[flyIndex * 3 + 0] = oneResult.OffsetX;
                m_iFlyOffset[flyIndex * 3 + 1] = oneResult.OffsetY;
                m_iFlyOffset[flyIndex * 3 + 2] = oneResult.OffsetAngle;
            }
            else
            {
                m_iFlyResult[flyIndex] = (int)PlcFlyResultCode.NG;
                m_iFlyOffset[flyIndex * 3 + 0] = 0;
                m_iFlyOffset[flyIndex * 3 + 1] = 0;
                m_iFlyOffset[flyIndex * 3 + 2] = 0;
            }
        }

        /// <summary>
        /// 非同步保存 原圖 (caller 負責 bmpFullfov 生命)
        /// </summary>
        void AsyncSaveFlyCameraImage(FlyID flyID, Bitmap bmpFly, FlyLotData lotData)
        {
            var dumper = LotDataHolder.Instance;
            dumper.AsyncSaveFlyCameraImage(flyID, bmpFly, lotData, m_iFlyResult);
        }

        void _LOG_ERROR(Exception ex, string message)
        {
            LtDebug.LOG.Error(ex, message);
            GaUtil.LOG($"[Error] {ex.Message}", Color.Red);
        }
    }
}
