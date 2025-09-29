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

using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace.UIMVC;
using LeTian.AoiLib;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;

// 關聯到 MINIX6 ???
using LineScanProcess = TravellerMINIX6.ProcessSpace.LineScanProcess;


namespace LaserAlignDX.Mvc.Ctrl.V3
{
    /// <summary>
    /// 使用 IFlyAoiModel
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
        IFlyAoiModel _flyAoiModel => _sysModel.FlyAoiModel;
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        FlyParaClass xFlyPara
        {
            get { return FlyParaClass.Instance; }
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
        MvdFlyResultDispUI[] _DispUIs;
        #endregion

        public void Attach(MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
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
        }
        void updateFlyCameraSerialNumber(int serialNumber)
        {
            _wndOwner?.Invoke(new Action(() =>
            {
                Color color = serialNumber < 0 || Traveller106.Universal.IsOpenFlyForm ? Color.Transparent : Color.Lime;
                string text = "飛拍序號";
                if (serialNumber > 0) text += $" : {serialNumber}";
                lblSerialNumber.BackColor = color;
                lblSerialNumber.Text = text;
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

        #region TOTAL_RESULT_DATA_FOR_PLC
        int[] m_iFlyResult = new int[4];
        float[] m_iFlyOffset = new float[4 * 3];
        #endregion

        void AttachDispUIs(MVSUI[] dispUIs)
        {
            _wndOwner = dispUIs[0].Parent;
            _DispUIs = Array.ConvertAll(dispUIs, ui => new MvdFlyResultDispUI(ui));
        }
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
                var aoiResult = _flyAoiModel.RunAoiOne(flyID, bmpFly);
                updateOneResult(flyID, aoiResult);
                _DispUIs[flyID.flyIndex].Update(aoiResult?.MetaData);
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex, "flyRunAoiOne");
            }
            saveFlyCamImage(flyID, bmpFly, _lotData);
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
