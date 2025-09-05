using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace;
using LaserAlignDX.UISpace.ChipCellsViewer;
using LaserAlignDX.UISpace.UIMVC;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.MachineSpace;


namespace LaserAlignDX.Mvc.Ctrl.V3
{
    /// <summary>
    /// 重整 MainX3UI
    /// (1) 使用 ChipCellsViewer 取代原來的 MVSUI 來顯示 晶粒檢測結果
    /// (2) 將飛拍控制拉出來到 GaPlcFlyCameraCtrl
    /// </summary>
    public partial class GaMainCtrl : Abs.GaMainCtrl, IxTickable
    {
        static bool OPT_USE_LETIAN_CHIP_CELL_VIEWER => GaMvcConfig.OPT_USE_LETIAN_CHIP_CELL_VIEWER;

        #region MACHINE
        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }
        #endregion

        #region GLOBAL_MESS
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        GaBigImageHolder _lineScanImageHolder
        {
            get => TravellerBigImagesHolder.Instance.LineScanImageHolder;
        }
        IProcessRunFPI _aoiModel
        {
            get => ProcessRunFPIClass.Instance;
        }
        bool IsBusy()
        {
            return _aoiModel.Running || LineScanSingleProcess.Instance.IsOn || LineScanProcess.Instance.IsOn;
        }
        #endregion

        #region GUI_MEMBERS
        Control _wndOwner;
        IvChipCellsViewer[] _DSMains;
        IvChipCellsViewer DSMain
        {
            get
            {
                // 每次都通訊一次
                int iscanIndex = MACHINE.PLCIO.iScanStage;
                var viewer = iscanIndex == 2 ?
                    _DSMains[1]:
                    _DSMains[0];
                return viewer;
            }
        }
        #endregion

        #region 飛拍控制
        GaPlcFlyCameraCtrl _plcFlyCameraCtrl = new GaPlcFlyCameraCtrl();
        #endregion

        public override void Attach(Control[] DsMains, MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            // CHIP_CELLS_VIEWERS
            _DSMains = new[]
            {
                buildChipCellsViewer(DsMains[0], CarrierEnum.C1),
                buildChipCellsViewer(DsMains[1], CarrierEnum.C2),
            };
            
            // FLY CAMERA CONTROLS
            _plcFlyCameraCtrl.Attach(DsFlys, lblFlyCameraSerialNo);

            // Owner Window
            _wndOwner = DsMains[0].Parent;
            System.Diagnostics.Debug.Assert(_wndOwner != null, "_wndOwner 不能為 null !");

            _wndOwner.HandleCreated += (s, e) => _wndOwner.BeginInvoke(new Action(() => _LOG("GaMailCtrl [V3]", Color.Blue)));
            _wndOwner.HandleDestroyed += (s, e) => _plcFlyCameraCtrl?.Dispose();

            // Processes
            InitAllProcesses();
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
                        Visible = true
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

        #region PROCESSES_這以後要納入_SYS_MODEL
        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        BaseProcess m_resetprocess
        {
            get { return ResetProcess.Instance; }
        }
        BaseProcess m_LineScanProcess
        {
            get { return LineScanProcess.Instance; }
        }
        BaseProcess m_MainProcess
        {
            get { return MainProcess.Instance; }
        }
        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
        }
        #endregion

        void InitAllProcesses()
        {
            //----------------------------------------------------------------
            // (1) 大部的 Processes 應該可以當成 MainProcess 的 Child Process,
            //      可以集中由 MainProcess 管理, 形成一體 Model.
            // (2) 以下對 Process Event Handler 的掛載.
            //      在 Model-View-Control 的架構規範下, 屬於 Control.
            //      ~ 以後再從 GUI(MainGdx3UI) 抽離出來.
            //----------------------------------------------------------------
            //m_mainprocess.OnCompleted += process_OnCompleted;
            // Buzzer 的結束 用來檢視是否有 NG 發生.
            m_MainProcess.OnMessage += process_OnMessage;
            m_MainProcess.OnCompleted += process_OnCompleted;
            m_BuzzerProcess.OnCompleted += buzzer_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnMessage += handle_aoi_run_message;
            m_SingleProcess.OnMessage += handle_aoi_run_message;

            var lineScanImagegHolder = TravellerBigImagesHolder.Instance.LineScanImageHolder;
            lineScanImagegHolder.OnImageChanged += LineScanImageHolder_OnImageChanged;

            var aoiEngine = ProcessRunFPIClass.Instance;
            aoiEngine.OnAoiProgressing += AoiEngine_OnAoiProgressing;
            aoiEngine.OnAoiBegin += AoiEngine_OnAoiBegin;
            aoiEngine.OnAoiEnd += AoiEngine_OnAoiEnd;
        }
        void TickAllProcesses()
        {
            m_resetprocess.Tick();
            m_BuzzerProcess.Tick();
            m_LineScanProcess.Tick();
            m_MainProcess.Tick();
            m_SingleProcess.Tick();
        }

        private void process_OnMessage(object sender, ProcessEventArgs e)
        {
            if (sender == m_MainProcess)
            {
                if (e.Message.Contains("Reset.Data"))
                {
                }
                else if (e.Message.Contains("Record.Start"))
                {
                    FireChangeState(MainS1State.LS_START);
                }
                else if (e.Message.Contains("Record.Stop"))
                {
                    FireChangeState(MainS1State.LS_STOP);
                }
            }

            try
            {
                // Do whatever message you want to show to the operators.
                string msg = $"Process {((BaseProcess)sender).Name}, {e.Message}\n";
                _LOG(msg, Color.Black);
            }
            catch
            {
            }

            //CGOperate();
        }
        private void process_OnLiveImage(object sender, ProcessEventArgs e)
        {
            //if (e.Tag != null && e.Tag is Bitmap)
            //{
            //    try
            //    {
            //        if (_wndOwner.InvokeRequired)
            //        {
            //            EventHandler<ProcessEventArgs> h = process_OnLiveImage;
            //            _wndOwner.Invoke(h, sender, e);
            //        }
            //        else
            //        {
            //            //@LETIAN: 2022/07/01 改用 GdxDispUI 增加一些 fps
            //            // bmp 由 Sender maintains life cycle.
            //            // 在此不用 Dispose
            //            //Bitmap bmp = (Bitmap)e.Tag;
            //            //dispUI1.UpdateLiveImage(bmp);
            //            //DS1.ReplaceDisplayImage(bmp);

            //            //問題: 誰負責對新生成的 mvdImage 進行 Dispose() ? 
            //            //DSMain.mvdRenderActivex1.LoadImageFromObject(pRun.cMvdInput.Clone());
            //            //DSMain.mvdRenderActivex1.ClearShapes();
            //            //DSMain.AddCross();
            //            //DSMain.mvdRenderActivex1.Display();
            //            updateMvd_LineScanImage();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        //>>> 此一層的 try - catch 以後可以省略.
            //        //>>> 會由 Event Sender 處理 exception
            //        //throw ex;
            //    }
            //}
        }
        private void process_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (sender == m_resetprocess)
            {
                if (m_resetprocess.RelateString == "CloseWindows")
                {
                    //執行的關閉流程 這裏則跳出
                    return;
                }
            }

            try
            {
                string msg = $"Process {((BaseProcess)sender).Name}, Completed!\n";
                _LOG(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void buzzer_OnCompleted(object sender, ProcessEventArgs e)
        {
            //if (InvokeRequired)
            //{
            //    EventHandler<ProcessEventArgs> h = buzzer_OnCompleted;
            //    BeginInvoke(h, sender, e);
            //}
            //else
            //{

            //}
        }
        private void handle_aoi_run_message(object sender, ProcessEventArgs e)
        {
            if (sender != m_LineScanProcess && sender != m_SingleProcess)
                return;

            if (e.Message.Contains("Result.1"))
            {
                FireChangeState(MainS1State.M_PASS);
            }
            else if (e.Message.Contains("Result.2"))
            {
                FireChangeState(MainS1State.M_NG);
            }
            else if (e.Message.Contains("Record.Start"))
            {
                //MappingReset();
                //FireChangeState(MainS1State.LS_START);
            }
            else if (e.Message.Contains("Record.Stop"))
            {
                //FireChangeState(MainS1State.LS_STOP);
            }
            else if (e.Message.Contains("Show.X"))
            {
                updateMvd_AoiResultData(e);
            }
            else if (e.Message.Contains("ResultX.Code"))
            {
                INI.Instance.CurrentBarcodeStr = e.Tag as string;
                FireChangeState(MainS1State.M_SHOWCODE, e.Tag as string);
            }

            try
            {
                string msg = $"Process {((BaseProcess)sender).Name}, {e.Message}\n";
                _LOG(msg, Color.Black);
            }
            catch
            {
            }

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
                updateMvd_LineScanImage();
            }
        }

        #region UPDATE_CELL_RESULT_FUNCTIONS
        void updateMvd_LineScanImage()
        {
            //------------------------------------------------------------------------
            // 舊代碼寫法
            //------------------------------------------------------------------------
            //CMvdImage mvdImage = pRun.cMvdInput.Clone();
            //DSMain.mvdRenderActivex1.LoadImageFromObject(mvdImage);
            //DSMain.mvdRenderActivex1.ClearShapes();
            //DSMain.AddCross();
            //DSMain.mvdRenderActivex1.Display();

            //------------------------------------------------------------------------
            // 新代碼
            // NOTE: 目前 cMvdInput 生命週期由 TravellerBigImagesHolder 保管 !!!
            //       不用重複 Clone() 來餵給 MVS
            //------------------------------------------------------------------------
            DSMain.UpdateImageSrc(_lineScanImageHolder, _lineScanImageHolder.SrcName);
        }
        void updateMvd_AoiResultData(ProcessEventArgs e)
        {
            DSMain.UpdateCells(xRecipe.xRegionCells, (int)_aoiModel.xScanInspectMode);

            // 報表 & LOG
            generate_report_and_log();

            // FIRE EVENTS
            FireChangeState(MainS1State.M_SHOWRESULT, e.Tag as string);
            if (_aoiModel.IsPass)
                FireChangeState(MainS1State.M_PASS);
            else
                FireChangeState(MainS1State.M_NG);
        }
        #endregion

        void generate_report_and_log()
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
                ccvPanel.menuLoadImage.Click += MenuLoadImage_Click;
                ccvPanel.menuTestChipInspect.Click += MenuTestChipInspect_Click;
                ccvPanel.menuTestEmptyTrayInspect.Click += MenuTestEmptyTrayInspect_Click;
                ccvPanel.menuTestQRCode.Click += MenuTestQRCode_Click;

                ccvPanel.menuLoadImage.Tag = carrierID;
                ccvPanel.menuTestChipInspect.Tag = carrierID;
                ccvPanel.menuTestEmptyTrayInspect.Tag = carrierID;
                ccvPanel.menuTestQRCode.Tag = carrierID;
            }
        }
        private void MenuLoadImage_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;

            string fileName = browseImageFile();
            if (fileName != null)
            {
                loadImage(fileName);
            }
        }
        private void MenuTestChipInspect_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;
            if (promptCheckImageHolder())
            {
                DSMain.Reset();
                LineScanSingleProcess.Instance.Start(ScanInspectMode.MEASUREAOI);
            }
        }
        private void MenuTestEmptyTrayInspect_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;
            if (promptCheckImageHolder())
            {
                DSMain.Reset();
                LineScanSingleProcess.Instance.Start(ScanInspectMode.NOTRAY);
            }
        }
        private void MenuTestQRCode_Click(object sender, EventArgs e)
        {
            if (promptCheckBusy())
                return;
            if (promptCheckImageHolder())
            {
                DSMain.Reset();
                LineScanSingleProcess.Instance.Start(ScanInspectMode.QRCODE);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        bool promptCheckBusy()
        {
            if(IsBusy())
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
        string browseImageFile()
        {
            string fileName = null;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Image";
                dlg.Filter = "JPG Files(*.jpg)|*.jpg|BMP Files(*.bmp)|*.bmp|PNG Files(*.png)|*.png";
                dlg.FileName = "*.jpg";

                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        dlg.InitialDirectory = System.IO.Path.GetDirectoryName(fileName);
                    }
                    catch
                    {

                    }
                }

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    //ResetAndClear();
                    fileName = dlg.FileName;
                }
                else
                {
                    fileName = null;
                }
            }
            return fileName;
        }
        void loadImage(string fileName)
        {
            if (fileName != null)
            {
                var oldCursor = setCursor(Cursors.WaitCursor);
                var bmp = GaImageUtil.LoadBigImage(fileName);
                _lineScanImageHolder?.TakeOver(bmp, System.IO.Path.GetFileName(fileName));
                setCursor(oldCursor);
            }
        }
        Cursor setCursor(Cursor cursor)
        {
            var frmOwner = _wndOwner?.FindForm();
            if (frmOwner != null)
            {
                var old = frmOwner.Cursor;
                frmOwner.Cursor = cursor;
                frmOwner.Invalidate();
                return old;
            }
            return Cursors.Default;
        }
        #endregion

        public override void Tick()
        {
            _plcFlyCameraCtrl?.Tick();
            TickAllProcesses();
        }

        void CGOperate()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
