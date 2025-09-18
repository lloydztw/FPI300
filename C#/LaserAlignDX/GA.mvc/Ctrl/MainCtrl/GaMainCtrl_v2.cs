#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-11 開始適用於 2.6.x.x 以後的版本
 *      2025-08-29 開始準備重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AUVision;
using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace.ChipCellsViewer;
using LaserAlignDX.UISpace.UIMVC;
using NeedleX.ProcessSpace;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using VisionDesigner;
using VisionDesigner.BlobFind;
using VisionDesigner.PositionFix;
using VsCommon.ControlSpace.MachineSpace;


namespace LaserAlignDX.Mvc.Ctrl.V2
{
    /// <summary>
    /// 重整 MainX3UI
    /// 使用 ChipCellsViewer 取代原來的 MVSUI 來顯示 晶粒檢測結果
    /// </summary>
    public partial class GaMainCtrl : Abs.GaMainCtrl, IxTickable
    {
        #region CONFIG
        static bool OPT_USE_LETIAN_CHIP_CELL_VIEWER => GaMvcConfig.OPT_USE_LETIAN_CHIP_CELL_VIEWER;
        #endregion

        #region MACHINE
        MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE; }
        }
        void mxReadPlcStageID(out CarrierEnum carrierID)
        {
            carrierID = CarrierEnum.C1;
            var plcIO = MACHINE?.PLCIO;
            if (plcIO != null)
            {
                var index = plcIO.iScanStage;
                if (index == 2)
                    carrierID = CarrierEnum.C2;
            }
        }
        void mxSimPlcStageID(CarrierEnum carrierID)
        {
            var plcIO = MACHINE?.PLCIO;
            plcIO?.simActiveStage((int)carrierID + 1);
        }
        #endregion

        #region GLOBAL_MESS_RECIPES
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        FlyParaClass xFlyPara
        {
            get { return FlyParaClass.Instance; }
        }
        #endregion

        #region GLOBAL_MODELS
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        IProcessRunFPI _aoiModel => _sysModel.AoiModel;
        GaBigImageHolder _lineScanImageHolder => _sysModel.LineScanImageHolder;
        bool IsBusy()
        {
            return _aoiModel.Running || LineScanSingleProcess.Instance.IsOn || LineScanProcess.Instance.IsOn;
        }
        #endregion

        #region PROCESSES_這以後要納入_SYS_MODEL
        BaseProcess m_LineScanProcess
        {
            get { return LineScanProcess.Instance; }
        }
        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
        }
        #endregion

        #region GUI_MEMBERS
        Control _wndOwner;
        IvChipCellsViewer[] _DSMains;
        IvChipCellsViewer ActiveViewer
        {
            get => _DSMains[(int)_activeCarrierID];
        }
        CarrierEnum _activeCarrierID;
        #endregion

        public override void Attach(Control[] DsMains, MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            // CHIP_CELLS_VIEWERS
            _DSMains = new[]
            {
                buildChipCellsViewer(DsMains[0], CarrierEnum.C1),
                buildChipCellsViewer(DsMains[1], CarrierEnum.C2),
            };

            // Owner Window
            _wndOwner = _DSMains[0].Window.Parent;
            System.Diagnostics.Debug.Assert(_wndOwner != null, "_wndOwner 不能為 null !");

            // FLY CAMERA Display UI
            Attach(DsFlys, lblFlyCameraSerialNo);

            // Even tHandlers
            InitEventHandlers();

            // 延遲顯示初始設定
            _wndOwner.HandleCreated += (s, e) =>
            {
                _wndOwner.BeginInvoke(new Action(() =>
                {
                    _LOG("GaMailCtrl [V2]", Color.Blue);
                    _LOG($"參數資料夾 = {Traveller106.Universal.MAINPATH}", Color.Blue);
                    _sysModel.ApplyRecipe();
                }));
            };
        }

        IvChipCellsViewer buildChipCellsViewer(Control panel, CarrierEnum carrierID)
        {
            //(1) 使用新的 ChipCellsViewer
            if (OPT_USE_LETIAN_CHIP_CELL_VIEWER)
            {
                //(1.1) 如果傳進來的已經是 JezChipCellsViewPanel
                if (panel is JezChipCellsViewPanel jezViewer)
                {
                    jezViewer.CarrierID = carrierID;
                    jezViewer.IsActive = carrierID == CarrierEnum.C1;
                    connectPopupMenuEvents(jezViewer, carrierID);
                    return jezViewer;
                }
                //(1.2) 如果傳進來的是其他視窗控件
                else if (panel is Control childWnd)
                {
                    // 生成新的 JezChipCellsViewPanel
                    var viewer = new JezChipCellsViewPanel
                    {
                        CarrierID = carrierID,
                        Location = childWnd.Location,
                        Size = childWnd.Size,
                        Dock = childWnd.Dock,
                        Visible = true,
                        IsActive = carrierID == CarrierEnum.C1,
                    };
                    // 與舊的 childWnd 互換角色
                    var parent = childWnd.Parent;
                    childWnd.Visible = false;
                    parent.Controls.Add(viewer);
                    connectPopupMenuEvents(viewer, carrierID);
                    return viewer;
                }
                else
                {
                    return null;
                }
            }
            //(2) 使用舊有的 MVSUI
            else
            {
                if (panel is MVSUI mvsui)
                    return new MvsChipCellsViewer(mvsui);
                return null;
            }
        }
        void InitEventHandlers()
        {
            _lineScanImageHolder.OnImageChanged += LineScanImageHolder_OnImageChanged;

            m_LineScanProcess.OnStarted += OnAoiProcess_Started;
            m_LineScanProcess.OnCompleted += OnAoiProcess_Completed;
            m_SingleProcess.OnStarted += OnAoiProcess_Started;
            m_SingleProcess.OnCompleted += OnAoiProcess_Completed;

            _aoiModel.OnAoiProgressing += AoiEngine_OnAoiProgressing;
            _aoiModel.OnAoiBegin += AoiEngine_OnAoiBegin;
            _aoiModel.OnAoiEnd += AoiEngine_OnAoiEnd;

            _sysModel.OnError += SysModel_OnError;
        }
        void TickAllProcesses()
        {
            m_LineScanProcess.Tick();
            m_SingleProcess.Tick();
        }

        #region EVENT_HANDLERS
        private void SysModel_OnError(object sender, ProcessEventArgs e)
        {
            if (_wndOwner.InvokeRequired)
            {
                _wndOwner.BeginInvoke((EventHandler<ProcessEventArgs>)SysModel_OnError, sender, e);
            }
            else
            {
                VsMessageBox.Warning(e.Message);
            }
        }
        private void OnAoiProcess_Started(object sender, ProcessEventArgs e)
        {
            FireChangeState(MainS1State.LS_START);
            ActiveViewer.Reset();
        }
        private void OnAoiProcess_Completed(object sender, ProcessEventArgs e)
        {
            FireChangeState(MainS1State.LS_STOP);
            update_AoiResult(e);
            CGOperate();
        }
        private void LineScanImageHolder_OnImageChanged(object sender, EventArgs e)
        {
            if (_wndOwner.InvokeRequired)
            {
                _wndOwner.Invoke((EventHandler)LineScanImageHolder_OnImageChanged, sender, e);
            }
            else
            {
                update_LineScanImage();
            }
        }
        #endregion

        void update_LineScanImage()
        {
            // 更新 GUI 畫面
            ActiveViewer.UpdateImageSrc(_lineScanImageHolder);
        }
        void update_AoiResult(ProcessEventArgs e)
        {
            // 更新 GUI 畫面
            ActiveViewer.UpdateCells(xRecipe.xRegionCells, (int)_aoiModel.xScanInspectMode);

            // 生成 報表 & LOG
            generate_AoiReportAndLog();

            // FIRE EVENTS 通知上層 UI
            bool isPass = _aoiModel.IsPass;
            FireChangeState(MainS1State.M_SHOWRESULT, e.Tag as string);
            FireChangeState(isPass ? MainS1State.M_PASS : MainS1State.M_NG);
        }
        void generate_AoiReportAndLog()
        {
            string lotId = _aoiModel.LotId;
            string stripId = _aoiModel.StripId;
            string fileName = _aoiModel.FileName;

            // 報表
            IxReportBuilder report = GaMvcConfig.CreateReportBuilder();
            report.GenerateReport(stripId, fileName);

            // LOG
            var logFormatter = new LogTextFormatter();
            string msg = logFormatter.Format(xRecipe.xRegionCells);
            _LOG($"StripID: {stripId}", Color.Black);
            _LOG($"LotID: {lotId}", Color.Black);
            _LOG($"#数据信息: {msg}", Color.Black);
        }

        #region EVENT_HANDLERS_FOR_PROGRESS_BAR
        FormProgressing _frmAoiProgressing = null;
        private void AoiEngine_OnAoiProgressing(object sender, GaProgressEventArgs e)
        {
            if (_wndOwner.InvokeRequired)
            {
                _wndOwner.BeginInvoke((EventHandler<GaProgressEventArgs>)AoiEngine_OnAoiProgressing, sender, e);
            }
            else
            {
                _frmAoiProgressing?.UpdateProgress(e.CurrentStep);
            }
        }
        private void AoiEngine_OnAoiBegin(object sender, GaProgressEventArgs e)
        {
            if (_wndOwner.InvokeRequired)
            {
                _wndOwner.Invoke((EventHandler<GaProgressEventArgs>)AoiEngine_OnAoiBegin, sender, e);
            }
            else
            {
                if (_frmAoiProgressing == null)
                {
                    _frmAoiProgressing = new FormProgressing();
                    //_frmAoiProgressing.TopMost = true;
                    _frmAoiProgressing.SetTotalSteps(e.TotalSteps);
                    _frmAoiProgressing.UpdateProgress(e.CurrentStep);
                    _frmAoiProgressing.Show(_wndOwner);
                    _frmAoiProgressing.BringToFront();
                }
            }
        }
        private void AoiEngine_OnAoiEnd(object sender, GaProgressEventArgs e)
        {
            if (_wndOwner.InvokeRequired)
            {
                _wndOwner.Invoke((EventHandler<GaProgressEventArgs>)AoiEngine_OnAoiEnd, sender, e);
            }
            else
            {
                _frmAoiProgressing?.Close();
                _frmAoiProgressing?.Dispose();
                _frmAoiProgressing = null;
            }
        }
        #endregion

        #region EVENT_HANDLERS_FOR_POPUP_MENU
        private void connectPopupMenuEvents(Control dsMain, CarrierEnum carrierID)
        {
            if (dsMain is JezChipCellsViewPanel ccvPanel)
            {
                ccvPanel.contextMenuStrip1.VisibleChanged += ContextMenuStrip1_VisibleChanged;
                ccvPanel.menuLoadImage.Click += MenuLoadImage_Click;
                ccvPanel.menuTestChipInspect.Click += MenuTestChipInspect_Click;
                ccvPanel.menuTestEmptyTrayInspect.Click += MenuTestEmptyTrayInspect_Click;
                ccvPanel.menuTestQRCode.Click += MenuTestQRCode_Click;

                ccvPanel.contextMenuStrip1.Tag = carrierID;
                ccvPanel.menuLoadImage.Tag = carrierID;
                ccvPanel.menuTestChipInspect.Tag = carrierID;
                ccvPanel.menuTestEmptyTrayInspect.Tag = carrierID;
                ccvPanel.menuTestQRCode.Tag = carrierID;
            }
        }
        private void ContextMenuStrip1_VisibleChanged(object sender, EventArgs e)
        {
            if (sender is ContextMenuStrip menu)
            {
                if (menu.Visible && menu.Tag is CarrierEnum carrierID)
                {
                    bool ok = checkPlcStageID(carrierID);
                    menu.Enabled = ok;
                }
            }
        }
        private void MenuLoadImage_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;

            string fileName = GaUtil.BrowseImageFile();
            if (fileName != null)
            {
                loadLineScanImage(fileName);
            }
        }
        private void MenuTestChipInspect_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;

            if(_lineScanImageHolder.IsEmpty() || !ActiveViewer.HasImage())
                MenuLoadImage_Click(sender, e);

            if (promptCheckImageHolder())
            {
                ActiveViewer.Reset();
                LineScanSingleProcess.Instance.Start(ScanInspectMode.MEASUREAOI);
            }
        }
        private void MenuTestEmptyTrayInspect_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;

            if (_lineScanImageHolder.IsEmpty() || !ActiveViewer.HasImage())
                MenuLoadImage_Click(sender, e);

            if (promptCheckImageHolder())
            {
                ActiveViewer.Reset();
                LineScanSingleProcess.Instance.Start(ScanInspectMode.NOTRAY);
            }
        }
        private void MenuTestQRCode_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;

            if (_lineScanImageHolder.IsEmpty() || !ActiveViewer.HasImage())
                MenuLoadImage_Click(sender, e);

            if (promptCheckImageHolder())
            {
                ActiveViewer.Reset();
                LineScanSingleProcess.Instance.Start(ScanInspectMode.QRCODE);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        bool checkPlcStageID(CarrierEnum targetID)
        {
            if (Universal.IsNoUseIO)
            {
                simulateChangeStage(targetID);
            }
            bool ok = _activeCarrierID == targetID;
            return ok;
        }
        void simulateChangeStage(CarrierEnum targetID)
        {
            if (Universal.IsNoUseIO)
            {
                var oldID = _activeCarrierID;
                if (oldID == targetID)
                    return;

                mxSimPlcStageID(targetID);
                mxReadPlcStageID(out var activeID);
                updateActiveCarrierID(activeID);              //@<<< Simulation

                //-----------------------------------------------------------------------
                // 【模擬】
                //  因為 AOI 計算都是使用 _lineScanImageHolder
                //  所以必須 把 ActiveViewer 的 Image
                //  載回到 _lineScanImageHolder
                //-----------------------------------------------------------------------
                if (ActiveViewer.HasImage() && !_lineScanImageHolder.IsEmpty())
                {
                    var matViewer = (ActiveViewer as JezChipCellsViewPanel)?.MatViewer;
                    var img = matViewer?.Image;
                    if (img != null)
                    {
                        var srcName = _lineScanImageHolder.SrcName;
                        var bmp = BitmapConverter.ToBitmap(img);
                        _lineScanImageHolder.TakeOver(bmp, srcName);
                    }
                }
            }
        }
        bool promptCheckBusy()
        {
            if (IsBusy())
            {
                MessageBox.Show("AOI 執行中", "AOI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            return false;
        }
        bool promptCheckImageHolder()
        {
            if (_lineScanImageHolder.IsEmpty())
            {
                MessageBox.Show("請先 加載圖檔 或 取像", "AOI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }
        void loadLineScanImage(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(fileName))
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                var bmp = GaImageUtil.LoadBigImage(fileName);
                _lineScanImageHolder?.TakeOver(bmp, System.IO.Path.GetFileName(fileName));

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void updateActiveCarrierID(CarrierEnum carrierID, bool force = false)
        {
            if (_activeCarrierID != carrierID || force)
            {
                _activeCarrierID = carrierID;
                _DSMains[0].IsActive = _activeCarrierID == CarrierEnum.C1;
                _DSMains[1].IsActive = _activeCarrierID == CarrierEnum.C2;

                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
                _sysModel.ActiveCarrierID = carrierID;
                _sysModel.ApplyRecipe();
                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        #endregion

        public override void Tick()
        {
            TickFlyCameras();
            TickAllProcesses();

            // 從 PLC 讀取 指定的 載台號
            mxReadPlcStageID(out var carrierID);
            updateActiveCarrierID(carrierID);             //@<<< PLC Tick
        }

        void CGOperate()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }


    //---------------------------------------------
    // 以下代碼, 準備分離成獨立的 PlcFlyCameraCtrl
    //---------------------------------------------
    partial class GaMainCtrl
    {
        #region GUI_MEMBERS
        MVSUI[] _DSFLYs;
        MVSUI DSFly0 => _DSFLYs[0];
        MVSUI DSFly1 => _DSFLYs[1];
        MVSUI DSFly2 => _DSFLYs[2];
        MVSUI DSFly3 => _DSFLYs[3];
        Control lblSerialNumber;
        Control lblNumberStr => lblSerialNumber;
        #endregion

        #region PLC_FLY_CAMERA_EXCHANGE_DATA
        const int FLYCOUNT = 4;
        //Bitmap[] bmpFlyOperate = new Bitmap[FLYCOUNT];
        Bitmap bmpFlyOperate = new Bitmap(1, 1);
        int iFlyIndex = 0;
        int[] iFlyResult = new int[4];
        float[] iFlyOffset = new float[4 * 3];

        bool m_plcStartOld = false;
        bool m_plcGetImageOld = false;

        bool m_plcFlyStartOld1 = false;
        bool m_plcFlyStartOld2 = false;
        #endregion

        #region LOT_DATA_FROM_PLC
        string m_StripId = "Strip_NONE";
        string m_LotId = "Lot_NONE";
        #endregion

        IxLineScanCam IxFlyAreaCam
        {
            get { return Universal.IxFlyAreaCam; }
        }
        PointF[] FlyOffsetUseStage
        {
            get
            {
                int iscanIndex = MACHINE.PLCIO.iScanStage;
                if (iscanIndex == 2)
                {
                    return xFlyPara.ptsOffset2;
                }
                return xFlyPara.ptsOffset;
            }
        }

        #region FLY_DATA_BYTES
        List<byte[]> bytesFlyDatas = new List<byte[]>();
        int _lastFlySerialNo = -1;
        #endregion

        void Attach(MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _DSFLYs = DsFlys;
            lblSerialNumber = lblFlyCameraSerialNo;

            lblSerialNumber.DoubleClick += (s, e) => clearFlyDataBytes();
            IxFlyAreaCam.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;

            _wndOwner.HandleDestroyed += (s, e) => Dispose();
        }

        void Dispose()
        {
            cPositionFixToolObj?.Dispose();
            cPositionFixToolObj = null;
        }

        void clearFlyDataBytes()
        {
            bytesFlyDatas?.Clear();
            _lastFlySerialNo = -1;
        }

        void updateFlyCameraSerialNumber(int serialNumber)
        {
            if (_lastFlySerialNo != serialNumber)
            {
                _lastFlySerialNo = serialNumber;
                _wndOwner?.Invoke(new Action(() =>
                {
                    lblSerialNumber.Text = $"飞拍序号:{serialNumber}";
                    lblSerialNumber.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
                }));
            }
        }

        void TickFlyCameras()
        {
            _getPlcRunTick();
        }

        private void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
        {
            if (Traveller106.Universal.IsOpenFlyForm)
            {
                return;
            }

            var plcIO = MACHINE?.PLCIO;
            if (plcIO == null)
                return;

            if (plcIO.bFlyReady)
            {
                //转换图像
                byte[] bmpbytes = new byte[cameraFrame.uBytes];
                Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
                bytesFlyDatas.Add(bmpbytes);

                //_wndOwner?.Invoke(new Action(() =>
                //{
                //    lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
                //}));
                updateFlyCameraSerialNumber(serialNumber: bytesFlyDatas.Count);

                if (bytesFlyDatas.Count >= 4)
                {
                    plcIO.bFlyReady = false;

                    int iflystartindex = plcIO.iFlyStart;
                    iFlyIndex = 3;
                    flyRunning(iflystartindex, cameraFrame);

                    //plcIO.bFlyDone = true;

                    plcIO.iFlyResult(iFlyResult);
                    plcIO.rOffset(iFlyOffset);

                    plcIO.bFlyDone = true;

                    bytesFlyDatas.Clear();

                    int[] ints0 = iFlyResult;
                    float[] floats0 = iFlyOffset;

                    StringBuilder sb = new StringBuilder();
                    foreach (var ix in ints0)
                    {
                        sb.Append(ix.ToString() + ",");
                    }
                    _LOG("iFlyResult:" + sb.ToString(), Color.Black);

                    StringBuilder sb1 = new StringBuilder();
                    foreach (var ix in floats0)
                    {
                        sb1.Append(ix.ToString() + ",");
                    }
                    _LOG("iFlyOffset:" + sb1.ToString(), Color.Black);

                    plcIO.bFlyReady = true;
                }
            }
        }

        private void _getPlcRunTick()
        {
            //btnReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Red : Color.FromArgb(192, 255, 192));
            //if (m_LineScanProcess.IsOn)
            //    lblState.Text = ToChangeLanguage("执行-线扫测试中") + m_LineScanProcess.ID.ToString();
            //else
            //    lblState.Text = ToChangeLanguage("等待");
            //if (_lastFlySerialNo != bytesFlyDatas.Count)
            //{
            //    _lastFlySerialNo = bytesFlyDatas.Count;
            //    _wndOwner?.Invoke(new Action(() =>
            //    {
            //        lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
            //        lblNumberStr.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
            //    }));
            //}

            updateFlyCameraSerialNumber(serialNumber: bytesFlyDatas.Count);

            if (MACHINE.PLCIO.bSoftwareReady)
            {
                if (MACHINE.PLCIO.bScanStart)
                {
                    if (!m_plcStartOld)
                    {
                        m_plcStartOld = true;

                        CommonLogClass.Instance.LogMessage("接收到plc启动信号", Color.Black);
                        if (!m_LineScanProcess.IsOn)
                        {
                            m_StripId = MACHINE.PLCIO.sStripID;
                            m_LotId = MACHINE.PLCIO.sLotID;

                            m_LineScanProcess.Start();
                        }
                        else
                        {
                            CommonLogClass.Instance.LogMessage("测试中#PLC重复启动", Color.Black);
                        }
                    }
                }
                else
                {
                    m_plcStartOld = false;
                }

                if (MACHINE.PLCIO.iFlyStart == 1)
                {
                    if (!m_plcFlyStartOld1)
                    {
                        m_plcFlyStartOld1 = true;
                        bytesFlyDatas.Clear();
                        CommonLogClass.Instance.LogMessage("接收到plc飞拍1启动信号", Color.Black);
                    }
                }
                else
                {
                    m_plcFlyStartOld1 = false;
                }

                if (MACHINE.PLCIO.iFlyStart == 2)
                {
                    if (!m_plcFlyStartOld2)
                    {
                        m_plcFlyStartOld2 = true;
                        bytesFlyDatas.Clear();
                        CommonLogClass.Instance.LogMessage("接收到plc飞拍2启动信号", Color.Black);
                    }
                }
                else
                {
                    m_plcFlyStartOld2 = false;
                }


                //if (MACHINE.PLCIO.IsGetImage)
                //{
                //    if (!m_plcGetImageOld)
                //    {
                //        m_plcGetImageOld = true;

                //        CommonLogClass.Instance.LogMessage("接收到plc抓图信号", Color.Black);
                //        if (!m_LineScanProcess.IsOn)
                //        {
                //            m_LineScanProcess.Start("Snap");
                //        }
                //        else
                //        {
                //            CommonLogClass.Instance.LogMessage("测试中#PLC重复抓图", Color.Black);
                //        }
                //    }
                //}
                //else
                //{
                //    m_plcGetImageOld = false;
                //}
            }
        }

        /* 备份20250801
                private void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
                {
                    if (Traveller106.Universal.IsOpenFlyForm)
                    {
                        return;
                    }

                    if (MACHINE.PLCIO.bFlyReady)
                    {
                        //转换图像
                        byte[] bmpbytes = new byte[cameraFrame.uBytes];
                        Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
                        bytesFlyDatas.Add(bmpbytes);
                        this.Invoke(new Action(() =>
                        {
                            lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
                        }));
                        if (bytesFlyDatas.Count >= 4)
                        {
                            iFlyIndex = 3;
                            if (MACHINE.PLCIO.iFlyStart == 1)
                            {
                                foreach (byte[] bmpdata in bytesFlyDatas)
                                {
                                    if (FlyParaClass.Instance.xIsOpenMuit)
                                        flyProcessProSpecial(1, iFlyIndex, cameraFrame, bmpdata);
                                    else
                                        flyProcessPro(1, iFlyIndex, cameraFrame, bmpdata);
                                    iFlyIndex--;
                                }
                            }
                            else if (MACHINE.PLCIO.iFlyStart == 2)
                            {
                                foreach (byte[] bmpdata in bytesFlyDatas)
                                {
                                    if (FlyParaClass.Instance.xIsOpenMuit)
                                        flyProcessProSpecial(2, iFlyIndex, cameraFrame, bmpdata);
                                    else
                                        flyProcessPro(2, iFlyIndex, cameraFrame, bmpdata);
                                    iFlyIndex--;
                                }
                            }
                            else
                            {
                                foreach (byte[] bmpdata in bytesFlyDatas)
                                {
                                    if (FlyParaClass.Instance.xIsOpenMuit)
                                        flyProcessProSpecial(1, iFlyIndex, cameraFrame, bmpdata);
                                    else
                                        flyProcessPro(1, iFlyIndex, cameraFrame, bmpdata);
                                    iFlyIndex--;
                                }
                            }

                            MACHINE.PLCIO.bFlyDone = true;
                            MACHINE.PLCIO.iFlyResult(iFlyResult);
                            MACHINE.PLCIO.rOffset(iFlyOffset);

                            bytesFlyDatas.Clear();
                        }
                    }
                }
        */

        #region 飞拍测试流程

        Stopwatch flystopwatch = new Stopwatch();
        void flyRunning(int flyStart, JetEazy.CCDSpace.CameraFrame cameraFrame)
        {
            int iCount = bytesFlyDatas.Count - 1;
            int iShowIndex = 0;
            while (iCount > -1)
            {
                byte[] bmpdata = bytesFlyDatas[iCount];

                if (FlyParaClass.Instance.xIsOpenMuit)
                    flyProcessProSpecial(flyStart, iShowIndex, cameraFrame, bmpdata);
                else
                    flyProcessPro(flyStart, iShowIndex, cameraFrame, bmpdata);

                iCount--;
                iFlyIndex--;
                iShowIndex++;
            }
        }
#if (NO_USE_CODE)
        void flyProcess(int flyStart, int flyIndex, JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
        {
            flystopwatch.Restart();
            //转换图像
            byte[] bmpbytes = new byte[cameraFrame.uBytes];
            Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;
            bmpFlyOperate.Dispose();
            bmpFlyOperate = ConvertFromMONO(bmpbytes, iw, ih);
            //RectangleF _rectF = new RectangleF(
            //    xRecipe.xRectRegionPrintFly.X,
            //    xRecipe.xRectRegionPrintFly.Y,
            //    xRecipe.xRectRegionPrintFly.Width,
            //    xRecipe.xRectRegionPrintFly.Height);
            //_rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            //BoundRect(ref _rectF, bmpFlyOperate.Size);
            //Bitmap bmptemp = bmpFlyOperate.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //int iret = xRecipe.PrintTempFlyRun(bmptemp);
            //PointF centerOrg = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            //PointF centerRun = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
            //    xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);

            //float _resolutionFly = INI.Instance.FlyImageResolution;

            //switch (flyStart)
            //{
            //    case 1:

            //        iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
            //        if (iret == 0)
            //        {
            //            centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
            //               xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

            //            //算出的pix需加入解析度
            //            iFlyOffset[flyIndex * 3 + 0] = (centerRun.X - centerOrg.X) * _resolutionFly;
            //            iFlyOffset[flyIndex * 3 + 1] = (centerRun.Y - centerOrg.Y) * _resolutionFly;
            //            iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }

            //        break;
            //    case 2:

            //        iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
            //        if (iret == 0)
            //        {
            //            centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
            //              xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

            //            //算出的pix需加入解析度
            //            iFlyOffset[flyIndex * 3 + 0] = (centerRun.X - centerOrg.X) * _resolutionFly;
            //            iFlyOffset[flyIndex * 3 + 1] = (centerRun.Y - centerOrg.Y) * _resolutionFly;
            //            iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
            //        }
            //        else
            //        {
            //            iFlyOffset[flyIndex * 3 + 0] = 0;
            //            iFlyOffset[flyIndex * 3 + 1] = 0;
            //            iFlyOffset[flyIndex * 3 + 2] = 0;
            //        }

            //        break;
            //}

            CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(bmpFlyOperate);

            //flystopwatch.Stop();
            //long ms = flystopwatch.ElapsedMilliseconds;

            //var RectangleShape
            //    = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
            //if (iFlyResult[flyIndex] == 1)
            //    RectangleShape.BorderColor = new MVD_COLOR(0, 255, 0);
            //else
            //    RectangleShape.BorderColor = new MVD_COLOR(255, 0, 0);

            //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
            //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
            //cMvdTextF.FontWidth = 20;

            //CMvdTextF cMvdTextFResult = new CMvdTextF(RectangleShape.CenterX,
            //    RectangleShape.CenterY,
            //    $"[{flyIndex}] x:{iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
            //    $"y:{iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
            //    $"a:{iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
            //cMvdTextFResult.BorderColor = cMvdTextF.BorderColor;
            //cMvdTextFResult.FontWidth = 20;

            switch (flyIndex)
            {
                case 0:
                    DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                    //DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    //DSFly0.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly0.mvdRenderActivex1.Display();
                    break;
                case 1:
                    DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                    //DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    //DSFly1.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly1.mvdRenderActivex1.Display();
                    break;
                case 2:
                    DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                    //DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    //DSFly2.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly2.mvdRenderActivex1.Display();
                    break;
                case 3:
                    DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                    //DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    //DSFly3.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly3.mvdRenderActivex1.Display();
                    break;
            }

            //if (INI.Instance.IsSaveDebugBMP)
            //{
            //    string flypath = $"D:\\FlyImage";
            //    if (!Directory.Exists(flypath))
            //        Directory.CreateDirectory(flypath);
            //    string flyname = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{flyIndex.ToString()}.jpg";
            //    cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
            //}
        }
        void flyProcessProBAK01(int flyStart, int flyIndex, JetEazy.CCDSpace.CameraFrame cameraFrame, byte[] eBytes)
        {
            flystopwatch.Restart();
            ////转换图像
            //byte[] bmpbytes = new byte[cameraFrame.uBytes];
            //Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;
            bmpFlyOperate.Dispose();
            bmpFlyOperate = ConvertFromMONO(eBytes, iw, ih);
            RectangleF _rectF = new RectangleF(
                xRecipe.xRectRegionPrintFly.X,
                xRecipe.xRectRegionPrintFly.Y,
                xRecipe.xRectRegionPrintFly.Width,
                xRecipe.xRectRegionPrintFly.Height);
            _rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            BoundRect(ref _rectF, bmpFlyOperate.Size);
            Bitmap bmptemp = bmpFlyOperate.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int iret = xRecipe.PrintTempFlyRun(bmptemp);
            PointF centerOrg = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            PointF centerRun = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);

            float _resolutionFly = INI.Instance.FlyImageResolution;

            int flyShowIndex = flyIndex + 1;
            switch (flyStart)
            {
                case 1:
                    flyShowIndex = flyIndex + 1;
                    break;
                case 2:
                    flyShowIndex = flyIndex + 1 + 4;
                    break;
            }

            switch (flyStart)
            {
                case 1:

                    iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                           xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].X;
                        iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].Y;
                        iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
                case 2:

                    iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                          xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].X;
                        iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].Y;
                        iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
            }

            CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(bmpFlyOperate);

            flystopwatch.Stop();
            long ms = flystopwatch.ElapsedMilliseconds;

            //转正的图形
            var _MatchResult = xRecipe.mvdprintFlytemp_Find.xResults[0];
            _MatchResult.fCenterX += _rectF.X;
            _MatchResult.fCenterY += _rectF.Y;
            var RectangleShapeBase
                = new CMvdRectangleF(centerOrg.X, centerOrg.Y, _rectF.Width, _rectF.Height);
            var RectangleShape
                   = PositionFixRun(RectangleShapeBase,
                                    new RectangleF(0, 0, _rectF.Width, _rectF.Height),
                                    new Rectangle(0, 0, bmpFlyOperate.Width, bmpFlyOperate.Height),
                                    _MatchResult) as CMvdRectangleF;

            //var RectangleShape
            //    = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
            if (iFlyResult[flyIndex] == 1)
                RectangleShape.BorderColor = new MVD_COLOR(0, 255, 0);
            else
                RectangleShape.BorderColor = new MVD_COLOR(255, 0, 0);

            //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
            //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
            //cMvdTextF.FontWidth = 20;



            CMvdTextF cMvdTextFResult = new CMvdTextF(RectangleShape.CenterX,
                RectangleShape.CenterY,
                $"[{flyShowIndex}] x:{iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                $"y:{iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                $"a:{iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
            cMvdTextFResult.BorderColor = new MVD_COLOR(0, 255, 0);
            cMvdTextFResult.FontWidth = 20;

            switch (flyIndex)
            {
                case 0:
                    DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly0.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly0.AddCross();
                    DSFly0.mvdRenderActivex1.Display();
                    break;
                case 1:
                    DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly1.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly1.AddCross();
                    DSFly1.mvdRenderActivex1.Display();
                    break;
                case 2:
                    DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly2.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly2.AddCross();
                    DSFly2.mvdRenderActivex1.Display();
                    break;
                case 3:
                    DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly3.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly3.AddCross();
                    DSFly3.mvdRenderActivex1.Display();
                    break;
            }

            if (INI.Instance.IsSaveDebugBMP)
            {
                string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
            }
        }
#endif
        void flyProcessPro(int flyStart, int flyIndex, JetEazy.CCDSpace.CameraFrame cameraFrame, byte[] eBytes)
        {
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;
            flyProcessPro(flyStart, flyIndex, ConvertFromMONO(eBytes, iw, ih));
        }
        void flyProcessPro(int flyStart, int flyIndex, Bitmap bmpInput)
        {
            flystopwatch.Restart();
            bmpFlyOperate.Dispose();
            bmpFlyOperate = bmpInput;
            RectangleF _rectF = new RectangleF(
                xRecipe.xRectRegionPrintFly.X,
                xRecipe.xRectRegionPrintFly.Y,
                xRecipe.xRectRegionPrintFly.Width,
                xRecipe.xRectRegionPrintFly.Height);
            _rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            BoundRect(ref _rectF, bmpFlyOperate.Size);
            Bitmap bmptemp = bmpFlyOperate.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int iret = xRecipe.PrintTempFlyRun(bmptemp);
            PointF centerOrg = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            PointF centerRun = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);

            float _resolutionFly = INI.Instance.FlyImageResolution;

            int flyShowIndex = flyIndex + 1;
            switch (flyStart)
            {
                case 1:
                    flyShowIndex = flyIndex + 1;
                    break;
                case 2:
                    flyShowIndex = flyIndex + 1 + 4;
                    break;
            }

            switch (flyStart)
            {
                case 1:

                    iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                           xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
                        iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
                        iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
                case 2:

                    iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                          xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].X;
                        iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + FlyOffsetUseStage[flyShowIndex - 1].Y;
                        iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
            }

            CMvdImage cMvdImage = GaImageUtil.BitmapToCMvdImage(bmpFlyOperate);

            flystopwatch.Stop();
            long ms = flystopwatch.ElapsedMilliseconds;

            var RectangleShape
                = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
            if (iFlyResult[flyIndex] == 1)
            {
                //转正的图形
                var _MatchResult = xRecipe.mvdprintFlytemp_Find.xResults[0];
                //_MatchResult.fCenterX += _rectF.X;
                //_MatchResult.fCenterY += _rectF.Y;
                var RectangleShapeBase
                    = new CMvdRectangleF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                                         xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2,
                                         xRecipe.xRectRegionPrintFly.Width,
                                         xRecipe.xRectRegionPrintFly.Height);

                //RectangleShapeBase
                //    = new CMvdRectangleF(0,
                //                         0,
                //                         xRecipe.xRectRegionPrintFly.Width,
                //                         xRecipe.xRectRegionPrintFly.Height);
                RectangleShape
                       = PositionFixRun(RectangleShapeBase,
                                        xRecipe.xRectRegionPrintFly,
                                        new Rectangle(0, 0, bmpFlyOperate.Width, bmpFlyOperate.Height),
                                        _MatchResult) as CMvdRectangleF;

                RectangleShape.CenterX += _rectF.X;
                RectangleShape.CenterY += _rectF.Y;
            }

            //var RectangleShape
            //    = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
            if (iFlyResult[flyIndex] == 1)
                RectangleShape.BorderColor = new MVD_COLOR(0, 255, 0);
            else
                RectangleShape.BorderColor = new MVD_COLOR(255, 0, 0);

            //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
            //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
            //cMvdTextF.FontWidth = 20;
            //添加十字线
            CMvdLineSegmentF v1 = new CMvdLineSegmentF(new MVD_POINT_F(0, bmpFlyOperate.Height / 2),
                new MVD_POINT_F(bmpFlyOperate.Width, bmpFlyOperate.Height / 2));
            v1.BorderColor = new MVD_COLOR(255, 215, 0);
            v1.BorderWidth = 1;
            CMvdLineSegmentF h1 = new CMvdLineSegmentF(new MVD_POINT_F(bmpFlyOperate.Width / 2, 0),
                new MVD_POINT_F(bmpFlyOperate.Width / 2, bmpFlyOperate.Height));
            h1.BorderColor = new MVD_COLOR(255, 215, 0);
            h1.BorderWidth = 1;

            CMvdTextF cMvdTextFResult = new CMvdTextF(RectangleShape.CenterX,
                RectangleShape.CenterY,
                $"[{flyShowIndex}] x:{iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                $"y:{iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                $"a:{iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
            cMvdTextFResult.BorderColor = new MVD_COLOR(0, 255, 0);
            cMvdTextFResult.FontWidth = 20;

            switch (flyIndex)
            {
                case 0:
                    DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    DSFly0.mvdRenderActivex1.AddShape(v1);
                    DSFly0.mvdRenderActivex1.AddShape(h1);
                    //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly0.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly0.AddCross();
                    DSFly0.mvdRenderActivex1.Display();
                    break;
                case 1:
                    DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    DSFly1.mvdRenderActivex1.AddShape(v1);
                    DSFly1.mvdRenderActivex1.AddShape(h1);
                    //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly1.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly1.AddCross();
                    DSFly1.mvdRenderActivex1.Display();
                    break;
                case 2:
                    DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    DSFly2.mvdRenderActivex1.AddShape(v1);
                    DSFly2.mvdRenderActivex1.AddShape(h1);
                    //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly2.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly2.AddCross();
                    DSFly2.mvdRenderActivex1.Display();
                    break;
                case 3:
                    DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    DSFly3.mvdRenderActivex1.AddShape(v1);
                    DSFly3.mvdRenderActivex1.AddShape(h1);
                    //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly3.mvdRenderActivex1.AddShape(RectangleShape);
                    DSFly3.AddCross();
                    DSFly3.mvdRenderActivex1.Display();
                    break;
            }

            if (INI.Instance.IsSaveDebugBMP)
            {
                string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
            }
        }
        void flyProcessProSpecial(int flyStart, int flyIndex, JetEazy.CCDSpace.CameraFrame cameraFrame, byte[] eBytes)
        {
            flystopwatch.Restart();
            ////转换图像
            //byte[] bmpbytes = new byte[cameraFrame.uBytes];
            //Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;
            bmpFlyOperate.Dispose();
            bmpFlyOperate = ConvertFromMONO(eBytes, iw, ih);
            RectangleF _rectF = new RectangleF(
                xRecipe.xRectRegionPrintFly.X,
                xRecipe.xRectRegionPrintFly.Y,
                xRecipe.xRectRegionPrintFly.Width,
                xRecipe.xRectRegionPrintFly.Height);
            _rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            BoundRect(ref _rectF, bmpFlyOperate.Size);
            Bitmap bmptemp = bmpFlyOperate.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            bool bOK = xRecipe.CheckSpecialAngle(bmptemp, out List<CBlobInfo> list, out float angle, out System.Drawing.PointF Center);

            int flyShowIndex = flyIndex + 1;
            switch (flyStart)
            {
                case 1:
                    flyShowIndex = flyIndex + 1;
                    break;
                case 2:
                    flyShowIndex = flyIndex + 1 + 4;
                    break;
            }

            switch (flyStart)
            {
                case 1:

                    iFlyResult[flyIndex] = (bOK ? 1 : 2);
                    if (bOK)
                    {
                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = angle;
                    }
                    else
                    {
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
                case 2:

                    iFlyResult[flyIndex] = (bOK ? 1 : 2);
                    if (bOK)
                    {
                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = angle;
                    }
                    else
                    {
                        iFlyOffset[flyIndex * 3 + 0] = 0;
                        iFlyOffset[flyIndex * 3 + 1] = 0;
                        iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
            }

            CMvdImage cMvdImage = GaImageUtil.BitmapToCMvdImage(bmpFlyOperate);

            flystopwatch.Stop();
            long ms = flystopwatch.ElapsedMilliseconds;

            var RectangleShape1 = new CMvdRectangleF(_rectF.X, _rectF.Y, _rectF.Width, _rectF.Height);
            var RectangleShape2 = new CMvdRectangleF(_rectF.X, _rectF.Y, _rectF.Width, _rectF.Height);

            if (bOK)
            {
                RectangleShape1 =
                   new CMvdRectangleF(list[0].RectInfo.CenterX + _rectF.X,
                   list[0].RectInfo.CenterY + _rectF.Y,
                   list[0].RectInfo.Width,
                   list[0].RectInfo.Height);
                RectangleShape2 =
                    new CMvdRectangleF(list[1].RectInfo.CenterX + _rectF.X,
                    list[1].RectInfo.CenterY + _rectF.Y,
                    list[1].RectInfo.Width,
                    list[1].RectInfo.Height);
            }

            if (iFlyResult[flyIndex] == 1)
                RectangleShape1.BorderColor = new MVD_COLOR(0, 255, 0);
            else
                RectangleShape1.BorderColor = new MVD_COLOR(255, 0, 0);
            if (iFlyResult[flyIndex] == 1)
                RectangleShape2.BorderColor = new MVD_COLOR(0, 255, 0);
            else
                RectangleShape2.BorderColor = new MVD_COLOR(255, 0, 0);

            //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
            //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
            //cMvdTextF.FontWidth = 20;



            CMvdTextF cMvdTextFResult = new CMvdTextF(Center.X + _rectF.X,
               Center.Y + _rectF.Y,
                $"[{flyShowIndex}] x:{iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                $"y:{iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                $"a:{iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
            cMvdTextFResult.BorderColor = new MVD_COLOR(0, 255, 0);
            cMvdTextFResult.FontWidth = 15;

            switch (flyIndex)
            {
                case 0:
                    DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly0.mvdRenderActivex1.AddShape(RectangleShape1);
                    DSFly0.mvdRenderActivex1.AddShape(RectangleShape2);
                    DSFly0.AddCross();
                    DSFly0.mvdRenderActivex1.Display();
                    break;
                case 1:
                    DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly1.mvdRenderActivex1.AddShape(RectangleShape1);
                    DSFly1.mvdRenderActivex1.AddShape(RectangleShape2);
                    DSFly1.AddCross();
                    DSFly1.mvdRenderActivex1.Display();
                    break;
                case 2:
                    DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly2.mvdRenderActivex1.AddShape(RectangleShape1);
                    DSFly2.mvdRenderActivex1.AddShape(RectangleShape2);
                    DSFly2.AddCross();
                    DSFly2.mvdRenderActivex1.Display();
                    break;
                case 3:
                    DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                    //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                    DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                    DSFly3.mvdRenderActivex1.AddShape(RectangleShape1);
                    DSFly3.mvdRenderActivex1.AddShape(RectangleShape2);
                    DSFly3.AddCross();
                    DSFly3.mvdRenderActivex1.Display();
                    break;
            }

            if (INI.Instance.IsSaveDebugBMP)
            {
                string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
            }
        }
        void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        private Bitmap ConvertFromMONO(byte[] rgbaData, int width, int height)
        {
            var pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);

            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                pixelFormat);

            IntPtr intPtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(rgbaData, 0, intPtr, rgbaData.Length);
            bitmap.UnlockBits(bitmapData);

            System.Drawing.Imaging.ColorPalette tempPalette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
            }
            bitmap.Palette = tempPalette;

            return bitmap;
        }
        VisionDesigner.PositionFix.CPositionFixTool cPositionFixToolObj = null;
        /// <summary>
        /// 计算修正后的位置框
        /// </summary>
        /// <param name="eMVDInput">输入转换的形状</param>
        /// <param name="templateRectF">模板尺寸</param>
        /// <param name="runRect">输入图片尺寸</param>
        /// <param name="templateRunResult">定位的结果</param>
        /// <returns>返回位置的形状</returns>
        public CMvdShape PositionFixRun(CMvdShape eMVDInput, RectangleF templateRectF, Rectangle runRect, xFindResult templateRunResult)
        {
            // CreateInstance
            if (cPositionFixToolObj == null)
                cPositionFixToolObj = new CPositionFixTool();

            // Set basic parameter

            cPositionFixToolObj.BasicParam.BasePoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRectF.X + templateRectF.Width / 2, templateRectF.Y + templateRectF.Height / 2), 0);

            cPositionFixToolObj.BasicParam.RunningPoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRunResult.fCenterX, templateRunResult.fCenterY), templateRunResult.fAngle);

            cPositionFixToolObj.BasicParam.RunImageSize = new MVD_SIZE_I(runRect.Width, runRect.Height);

            cPositionFixToolObj.BasicParam.FixMode = MVD_POSFIX_MODE.MVD_POSFIX_MODE_HVA;

            cPositionFixToolObj.BasicParam.InitialShape = eMVDInput;

            // Running

            cPositionFixToolObj.Run();

            // Get the result
            return cPositionFixToolObj.Result.CorrectedShape;
        }

        #endregion

        #region GaUtil_取代重複碼
#if (false)
        void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
#endif
#endregion
    }
}
