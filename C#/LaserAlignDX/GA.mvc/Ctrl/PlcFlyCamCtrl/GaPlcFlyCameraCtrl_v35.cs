#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-23 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.GA.BasicSpace;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.MachineSpace;
using LineScanProcess = TravellerMINIX6.ProcessSpace.LineScanProcess;
using PlcFlyResultCode = LaserAlignDX.PlcResultCode;


namespace LaserAlignDX.Mvc.Ctrl.V3
{
    /// <summary>
    /// 重整 MainX3UI 飛拍
    /// 將飛拍控制拉出來到 GaPlcFlyCameraCtrl
    /// ToDO: 
    /// 需要把 flyProcessPro 與 flyProcessProSpecial 的 AOI 部分 抽離到 AoiModel 模塊內
    /// </summary>
    public class GaPlcFlyCameraCtrl : IxTickable
    {
        #region CONFIG
        static int TOTAL_FLY_FRAMES_COUNT => GaMvcConfig.TOTAL_FLY_FRAMES_COUNT;
        #endregion

        public event EventHandler OnFlyStarted;
        public event EventHandler OnFlyDone;

        #region MACHINE
        MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE; }
        }
        #endregion

        #region GLOBAL_MESS
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        FlyParaClass xFlyPara
        {
            get { return FlyParaClass.Instance; }
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
        #endregion

        public void Attach(Control[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _wndOwner = DsFlys[0].Parent;
            AttachDispUIs(DsFlys);
            lblSerialNumber = lblFlyCameraSerialNo;
            lblSerialNumber.DoubleClick += (s, e) => resetOnTheFlyFrameCount();
            FlyCamera.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;
            _sysModel.EmptyTrayAoiModel.OnFinalResulted += (s, e) => updateFlyCameraSerialNumber(-1);
        }
        public void Tick()
        {
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
        void updateFlyCameraSerialNumber(int serialNumber)
        {
            _wndOwner?.Invoke(new Action(() =>
            {
                var color = (serialNumber < 0 || Traveller106.Universal.IsOpenFlyForm) ? Color.Transparent : Color.Lime;
                var text = "飛拍序號";
                if (serialNumber > 0) text += $" : {serialNumber}";
                lblSerialNumber.Text = text;
                lblSerialNumber.BackColor = color;
            }));
        }

        /// <summary>
        /// 注意: 此 Event Handler 執行於 background thread
        /// </summary>
        void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame camFrameInfo, IntPtr pBuffer)
        {
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
                // 防止重複進入
                if (m_isOnTheFlyFrameBusy || !m_isOnTheFlyFrameEnabled)
                    return;

                // 設定 線程保護 旗標
                m_isOnTheFlyFrameBusy = true;

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
                m_isOnTheFlyFrameBusy = false;
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
                return stageNo == 1 ? xFlyPara.ptsOffset : xFlyPara.ptsOffset2;
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
                VsMessageBox.Warning("【軟件準備】 已經啟動連線, 無法進行 飛拍 離線測試!");
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
                        using (Bitmap bmpFly = GaImageUtil.LoadBigImage(fileName))
                        {
                            var flyID = new FlyID(idx + 1, idx);
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
            try
            {
                if (xFlyPara.xIsOpenMuit)   // 一般都是 false
                    flyProcessProSpecial(flyID, bmpFly);
                else
                    flyProcessPro(flyID, bmpFly);
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex, "flyProcessProXxx");
            }
            saveFlyCamImage(flyID, bmpFly, _lotData);
        }
        void flyProcessPro(FlyID flyID, Bitmap bmpFly)
        {
            var aoiMetaData = new FlyMetaData() { AlgorithmName = "MVD_TemplateMatch" };
            var aoiResult = new FlyAoiResult() { MetaData = aoiMetaData };
            //var flystopwatch = new Stopwatch();
            //flystopwatch.Restart();

            #region OLD_CODE
            //bmpFlyOperate.Dispose();
            //bmpFlyOperate = bmpInput;
            //RectangleF _rectF = new RectangleF(
            //    xRecipe.xRectRegionPrintFly.X,
            //    xRecipe.xRectRegionPrintFly.Y,
            //    xRecipe.xRectRegionPrintFly.Width,
            //    xRecipe.xRectRegionPrintFly.Height);
            //_rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            //BoundRect(ref _rectF, bmpFlyOperate.Size);
            #endregion

            #region OLD_CODE_FOR_CENTER_POINTS
            //PointF centerOrg = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            //PointF centerRun = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            #endregion

            RectangleF roiRect = xRecipe.xRectRegionPrintFly;
            PointF centerOrg = JetEazy.Qcvt.Center(ref roiRect);
            PointF centerRun = centerOrg;

            roiRect.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            GaUtil.Clip(ref roiRect, bmpFly.Size);

            using (Bitmap bmpCrop = bmpFly.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                int err = xRecipe.PrintTempFlyRun(bmpCrop);

                aoiResult.Code = err == 0 ? PlcFlyResultCode.OK : PlcFlyResultCode.NG;

                // 記入 GUI 畫圖所需要的數據
                // 注意: mvdRects.Count 有可能為 0 !!!
                var mvdRects = xRecipe.mvdprintFlytemp_Find.xMvdResultRects;
                if (mvdRects.Count > 0)
                {
                    aoiMetaData.xResultBox2D = mvdRects[0]?.ToBox2D();
                    //定位的角度 直接给plc
                    aoiResult.OffsetAngle = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                }
                else
                {
                    aoiMetaData.xResultBox2D = null;
                    aoiResult.OffsetAngle = 0;
                }
                FlyMetaData.Offset(aoiMetaData.xResultBox2D, roiRect.X, roiRect.Y);
                
                aoiMetaData.xTemplateRect = xRecipe.xRectRegionPrintFly;
                aoiMetaData.xBlobs = null;
                aoiMetaData.roiRect = roiRect;
                aoiMetaData.bmpFly = bmpFly;
                aoiMetaData.flyID = flyID;
                aoiMetaData.flyAoiResult = aoiResult;
            }

            #region OLD_CODE
            //PointF centerOrg = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            //PointF centerRun = new PointF(
            //    xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            #endregion

            #region OLD_CODE
            //int flyShowIndex = flyIndex + 1;
            //switch (flyStart)
            //{
            //    case 1:
            //        flyShowIndex = flyIndex + 1;
            //        break;
            //    case 2:
            //        flyShowIndex = flyIndex + 1 + 4;
            //        break;
            //}
            #endregion

            #region OLD_CODE
            //float _resolutionFly = INI.Instance.FlyImageResolution;
            //switch (flyStart)
            //{
            //    case 1:
            //        iFlyResult[flyIndex] = (err == 0 ? 1 : 2);
            //        if (err == 0)
            //        {
            //            //centerRun = new PointF(
            //            //        xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
            //            //        xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);
            //            ////算出的pix需加入解析度
            //            //iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            //iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            //iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;

            //            centerRun.X = foundResult.fCenterX + roiRect.X;
            //            centerRun.Y = foundResult.fCenterY + roiRect.Y;

            //            //算出的pix需加入解析度
            //            offsetX = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            offsetY = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            offsetAngle = foundResult.fAngle;

            //            iFlyOffset[flyIndex * 3 + 0] = offsetX;
            //            iFlyOffset[flyIndex * 3 + 1] = offsetY;
            //            iFlyOffset[flyIndex * 3 + 2] = offsetAngle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;

            //    case 2:
            //        iFlyResult[flyIndex] = (err == 0 ? 1 : 2);
            //        if (err == 0)
            //        {
            //            //centerRun = new PointF(
            //            //    xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + roiRect.X,
            //            //    xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + roiRect.Y);

            //            ////算出的pix需加入解析度
            //            //iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            //iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            //iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;

            //            centerRun.X = foundResult.fCenterX + roiRect.X;
            //            centerRun.Y = foundResult.fCenterY + roiRect.Y;

            //            //算出的 pix 需加入解析度
            //            offsetX = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
            //            offsetY = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
            //            offsetAngle = foundResult.fAngle;

            //            iFlyOffset[flyIndex * 3 + 0] = offsetX;
            //            iFlyOffset[flyIndex * 3 + 1] = offsetY;
            //            iFlyOffset[flyIndex * 3 + 2] = offsetAngle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;
            //}
            #endregion

            int flyStart = flyID.flyStart;
            if (flyStart == 1 || flyStart == 2)
            {
                if (aoiResult.Code == PlcFlyResultCode.OK)
                {
                    //centerRun.X = foundResult.fCenterX + roiRect.X;
                    //centerRun.Y = foundResult.fCenterY + roiRect.Y;
                    centerRun = aoiMetaData.xCentroid;

                    // 算出的 pix 需加入解析度
                    float flyCamResolution = INI.Instance.FlyImageResolution;
                    int flyShowID1 = flyID.ShowID;

                    //aoiResult.OffsetX = -(centerRun.X - centerOrg.X) * flyCamResolution + FlyOffsetUseStage[flyShowID1 - 1].X;
                    //aoiResult.OffsetY = -(centerRun.Y - centerOrg.Y) * flyCamResolution + FlyOffsetUseStage[flyShowID1 - 1].Y;
                    //aoiResult.OffsetAngle = (aoiMetaData.xResultBox2D != null) ? (float)(aoiMetaData.xResultBox2D.Theta * 180 / Math.PI) : 0f;

                    float _angle = aoiResult.OffsetAngle;// (aoiMetaData.xResultBox2D != null) ? (float)(aoiMetaData.xResultBox2D.Theta * 180 / Math.PI) : 0f;

                    // 根据应用场景选择顺序
                    var template = new TemplatePositioningCalculator2D.TemplateData(
                        new TemplatePositioningCalculator2D.Vector2(centerOrg.X, centerOrg.Y), 0);

                    var current = new TemplatePositioningCalculator2D.TemplateData(
                        new TemplatePositioningCalculator2D.Vector2(centerRun.X, centerRun.Y), _angle);

                    // 场景1：机械臂控制 - 通常先旋转后平移
                    //Console.WriteLine($"机械臂控制（推荐先旋转后平移）:");
                    var comp1 = TemplatePositioningCalculator2D.CalculateCompensation(
                        template, current, TemplatePositioningCalculator2D.ApplyOrder.RotateThenTranslate);
                    //comp1.PrintResult();

                    aoiResult.OffsetX = comp1.Translation.X * flyCamResolution + FlyOffsetUseStage[flyShowID1 - 1].X;
                    aoiResult.OffsetY = comp1.Translation.Y * flyCamResolution + FlyOffsetUseStage[flyShowID1 - 1].Y;
                    //aoiResult.OffsetAngle = comp1.AngleCompensation;

                    //_LOG($"FlyID[{flyID.ShowID}] {comp1.ResultString(flyCamResolution)}", Color.Blue);

                    //原先的计算结果 以下代码
                    ////// 场景2：UI元素定位 - 通常先平移后旋转
                    ////Console.WriteLine($"UI元素定位（推荐先平移后旋转）:");
                    //var comp2 = TemplatePositioningCalculator2D.CalculateCompensation(
                    //    template, current, TemplatePositioningCalculator2D.ApplyOrder.TranslateThenRotate);
                    ////comp2.PrintResult();

                    //aoiResult.OffsetX = comp2.Translation.X * flyCamResolution + FlyOffsetUseStage[flyShowID1 - 1].X;
                    //aoiResult.OffsetY = comp2.Translation.Y * flyCamResolution + FlyOffsetUseStage[flyShowID1 - 1].Y;
                    ////aoiResult.OffsetAngle = comp1.AngleCompensation;

                    //_LOG($"FlyID[{flyID.ShowID}] {comp2.ResultString(flyCamResolution)}", Color.Blue);

                }
                updateOneResult(flyID, aoiResult);
            }

            //flystopwatch.Stop();

            // 更新 GUI (暫時沿用原來的 逆行 調用處)
            _DispUIs[flyID.flyIndex].Update(aoiResult);
        }
        void flyProcessProSpecial(FlyID flyID, Bitmap bmpFly)
        {
            var aoiMetaData = new FlyMetaData() { AlgorithmName = "MVD_CheckSpecialAngle" };
            var aoiResult = new FlyAoiResult() { MetaData = aoiMetaData };
            //var flystopwatch = new Stopwatch();
            //flystopwatch.Restart();

            #region OLD_CODE
            //////转换图像
            ////byte[] bmpbytes = new byte[cameraFrame.uBytes];
            ////Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            //int iw = cameraFrame.iWidth;
            //int ih = cameraFrame.iHeight;
            //bmpFlyOperate.Dispose();
            //bmpFlyOperate = ConvertFromMONO(eBytes, iw, ih);
            //RectangleF _rectF = new RectangleF(
            //    xRecipe.xRectRegionPrintFly.X,
            //    xRecipe.xRectRegionPrintFly.Y,
            //    xRecipe.xRectRegionPrintFly.Width,
            //    xRecipe.xRectRegionPrintFly.Height);
            #endregion

            RectangleF roiRect = xRecipe.xRectRegionPrintFly;
            roiRect.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            GaUtil.Clip(ref roiRect, bmpFly.Size);

            using (Bitmap bmpCrop = bmpFly.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                bool ok = xRecipe.CheckSpecialAngle(bmpCrop, out var mvdBlobs, out float angle, out PointF centerPt);
                aoiResult.Code = ok ? PlcFlyResultCode.OK : PlcFlyResultCode.NG;
                aoiResult.OffsetAngle = ok ? angle : 0f;

                // 記入 GUI 畫圖所需要的數據
                aoiMetaData.xBlobs = mvdBlobs != null ? Array.ConvertAll(mvdBlobs.ToArray(), bi => bi.RectInfo.ToBox2D()) : null;
                FlyMetaData.Offset(aoiMetaData.xBlobs, roiRect.X, roiRect.Y);

                aoiMetaData.roiRect = roiRect;
                aoiMetaData.bmpFly = bmpFly;
                aoiMetaData.flyID = flyID;
                aoiMetaData.flyAoiResult = aoiResult;
            }

            #region OLD_CODE
            //int flyShowIndex = flyIndex + 1;
            //switch (flyStart)
            //{
            //    case 1:
            //        flyShowIndex = flyIndex + 1;
            //        break;
            //    case 2:
            //        flyShowIndex = flyIndex + 1 + 4;
            //        break;
            //}
            #endregion

            #region OLD_CODE
            //switch (flyStart)
            //{
            //    case 1:
            //        iFlyResult[flyIndex] = (bOK ? 1 : 2);
            //        if (bOK)
            //        {
            //            //算出的pix需加入解析度
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = angle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;
            //    case 2:
            //        iFlyResult[flyIndex] = (bOK ? 1 : 2);
            //        if (bOK)
            //        {
            //            //算出的pix需加入解析度
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = angle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }
            //        break;
            //}
            #endregion

            int flyStart = flyID.flyStart;
            if (flyStart == 1 || flyStart == 2)
            {
                updateOneResult(flyID, aoiResult);
            }

            //flystopwatch.Stop();

            // 更新 GUI (暫時沿用原來的 逆行 調用處)
            _DispUIs[flyID.flyIndex].Update(aoiResult);
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
        void saveFlyCamImage(FlyID flyID, Bitmap bmpFly, FlyLotData lotData)
        {
            if (INI.Instance.IsSaveDebugBMP)
            {
                try
                {
                    var tm = DateTime.Now;

                    int flyShowIndex = flyID.ShowID;
                    string stripID = lotData.StripID;
                    string lotID = lotData.LotID;

                    //>>> string path = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{stripID}";
                    string path = System.IO.Path.Combine(INI.Instance.ResultImagePath, "flyImage", tm.ToString("yyyyMMdd"), stripID);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{lotID}-[{flyShowIndex}]-{tm.ToString("yyyyMMddHHmmssfff")}.jpg";
                    fileName = System.IO.Path.Combine(path, fileName);

                    //>>> cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
                    GaImageUtil.SaveBigImage(fileName, bmpFly);
                }
                catch (Exception ex)
                {
                    LtDebug.LOG.Error(ex, "GaPlcFlyCameraCtrl.saveFlyCameraImage");
                }
            }
        }
    }
}
