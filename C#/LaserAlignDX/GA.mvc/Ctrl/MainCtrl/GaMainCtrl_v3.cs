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

using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace.ChipCellsViewer;
using LaserAlignDX.UISpace.UIMVC;
using NeedleX.ProcessSpace;
using OpenCvSharp.Extensions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.IOSpace;
using VsCommon.ControlSpace.MachineSpace;


namespace LaserAlignDX.Mvc.Ctrl.V3
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
            if (plcIO is IPlcIoFPIX3Sim sim)
            {
                sim.simActiveStage((int)carrierID + 1);
                sim.sRecipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            }
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

        #region FLY_CTRL
        GaPlcFlyCameraCtrl _flyCtrl = new GaPlcFlyCameraCtrl();
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
            _flyCtrl.Attach(DsFlys, lblFlyCameraSerialNo);

            // 自動關閉 ZoomInOut
            new GaAutoDisableZoomCtrl().Attach(_flyCtrl, _DSMains[0].Window, _DSMains[1].Window);

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
            _wndOwner.HandleDestroyed += (s, e) => _flyCtrl = null;

            // SIMULATION
            var plcIO = MACHINE?.PLCIO;
            if (plcIO is IPlcIoFPIX3Sim sim)
            {
                sim.sRecipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
                sim.OnRequestSimLineScan += Sim_OnRequestSimLineScan;
            }
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
        private void Sim_OnRequestSimLineScan(object sender, DoWorkEventArgs e)
        {
            if(_wndOwner.InvokeRequired)
            {
                _wndOwner.Invoke((DoWorkEventHandler)Sim_OnRequestSimLineScan, sender, e);
            }
            else
            {
                //-----------------------------------------------------------------------
                // 【模擬】
                //-----------------------------------------------------------------------
                bool isEmpty = !ActiveViewer.HasImage();
                if (isEmpty)
                {
                    checkPlcStageID(_activeCarrierID);
                    string fileName = GaUtil.BrowseImageFile();
                    if (fileName != null)
                        loadLineScanImage(fileName);
                }
                isEmpty = !ActiveViewer.HasImage();
                e.Cancel = isEmpty;
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
            _flyCtrl?.Tick();

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
}
