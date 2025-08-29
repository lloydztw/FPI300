using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace.UIMVC;
using LeTian.AoiLib;
using NeedleX.ProcessSpace;
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
using VsCommon.ControlSpace.MachineSpace;


namespace LaserAlignDX.Mvc.Ctrl.V1
{
    /// <summary>
    /// 重整 MainX3UI
    /// 將有關 使用 MVD 顯示 晶粒檢測結果 的相關代碼 
    /// 蒐集重整成 updateMvd_xxx functions
    /// </summary>
    public partial class GaMainCtrl : Abs.GaMainCtrl, IxTickable
    {
        #region MACHINE
        //List<CollectResultClass> collectResultClasses = new List<CollectResultClass>();
        //protected MachineCollectionClass MACHINECollection
        //{
        //    get
        //    {
        //        return Traveller106.Universal.MACHINECollection;
        //    }
        //}
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
        FlyParaClass xFlyPara
        {
            get { return FlyParaClass.Instance; }
        }
        InspectX3ParaClass InspectPara
        {
            get { return InspectX3ParaClass.Instance; }
        }
        IProcessRunFPI pRun
        {
            get => ProcessRunFPIClass.Instance;
        }
        #endregion

        #region GUI_MEMBERS
        Control _wndOwner;
        MVSUI[] _DSMains;
        MVSUI DSMain
        {
            get
            {
                int iscanIndex = MACHINE.PLCIO.iScanStage;
                var viewer = iscanIndex == 2 ?
                    _DSMains[1]:
                    _DSMains[0];
                return viewer;
            }
        }
        #endregion

        public void Attach(MVSUI[] DsMains, MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            // MVS UI
            this._DSMains = DsMains;

            // Flys
            Attach(DsFlys, lblFlyCameraSerialNo);

            // Owner Window
            _wndOwner = _DSMains[0].Parent;
            System.Diagnostics.Debug.Assert(_wndOwner != null);
            _wndOwner.HandleCreated += (s, e) => _LOG("GaMailCtrl (v1)", Color.Blue);

            // Processes
            InitAllProcesses();
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

            var lineScanImagegHolder = FpiBigImagesHolder.Instance.LineScanImageHolder;
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

        #region UPDATE_MVD_FUNCTIONS
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
            // NOTE: 目前 cMvdInput 生命週期由 _FpiBigImagesHolder 保管 !!!
            //       不用重複 Clone() 來餵給 MVS
            //------------------------------------------------------------------------
            var lineScanImageHolder = FpiBigImagesHolder.Instance.LineScanImageHolder;
            CMvdImage mvdImage = lineScanImageHolder.PeekMvdImage();
            DSMain.mvdRenderActivex1.LoadImageFromObject(mvdImage);
            DSMain.mvdRenderActivex1.ClearShapes();
            DSMain.AddCross();
            DSMain.mvdRenderActivex1.Display();
        }
        void updateMvd_AoiResultData(ProcessEventArgs e)
        {
            //// 清除 MVD canvas
            DSMain.mvdRenderActivex1.ClearShapes();

            // 報表 & LOG
            generate_report_and_log();

            // 為每一個 Cell 更新 MVD 顯示元件
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                updateMvd_OneCellData(cell);
            }

            // 显示格点之外的料件
            updateMvd_OutGrid_Blocs();

            //// MVD render
            DSMain.mvdRenderActivex1.Display();

            // FIRE EVENTS
            FireChangeState(MainS1State.M_SHOWRESULT, e.Tag as string);
            if (pRun.IsPass)
                FireChangeState(MainS1State.M_PASS);
            else
                FireChangeState(MainS1State.M_NG);
        }
        void updateMvd_OneCellData(RegionCellX3Class cell)
        {
            switch (pRun.xScanInspectMode)
            {
                case ScanInspectMode.NOTRAY:
                    updateMvd_OneCellData_NoTray(cell);
                    break;
                case ScanInspectMode.MEASUREAOI:
                case ScanInspectMode.QRCODE:
                default:
                    updateMvd_OneCellData_Location(cell);
                    break;
            }
        }
        void updateMvd_OneCellData_NoTray(RegionCellX3Class cell)
        {
            //填写数据 疑似有料
            //try
            {
                string strNoTray = cell.GetNoTrayDesc();
                if (!string.IsNullOrEmpty(strNoTray))
                {
                    CMvdRectangleF mvdDrawResultRectF = cell.DrawResultRectF();
                    CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(mvdDrawResultRectF.CenterX,
                                                                  mvdDrawResultRectF.CenterY,
                                                                  $"{strNoTray}");

                    cMvdTextFShowNoTray.BorderColor = new MVD_COLOR(255, 0, 0);
                    cMvdTextFShowNoTray.FontWidth = 11;
                    //if (INI.Instance.IsResultShowChar)
                    DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowNoTray);
                    DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(false));
                }
                else
                {
                    var mvdRect = cell.DrawBaseRectFFixSize();
                    DSMain.mvdRenderActivex1.AddShape(mvdRect);
                }
            }
            //catch (Exception ex)
            //{
            //    string errMsg = $"顯示結果異常: {ex.Message}\n\r\n\r@{ex.StackTrace}";
            //    MessageBox.Show(errMsg);
            //    return;
            //}
        }
        void updateMvd_OneCellData_Location(RegionCellX3Class cell)
        {
            RectangleF cellRect = cell.viewRectF;

            cellRect.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);

            //BoundRect(ref _rectF, new Size((int)ProcessRunClass.Instance.cMvdInput.Width,
            //                               (int)ProcessRunClass.Instance.cMvdInput.Height));

            //try
            {
                if (InspectPara.bOpenLineMeasure)
                {
                    // 繪製 找到的邊線
                    updateMvd_OneCellData_LinesOutSide(cell, cellRect.Location);

                    // 繪製 邊線手拉框
                    updateMvd_OneCellData_LinesOutsideBoxes(cell);

                    if (InspectPara.bCheckMeasureOffset)
                    {
                        // 繪製 cMvdLineSegmentFsInSide
                        updateMvd_OneCellData_LinesInSide(cell, cellRect.Location);
                    }
                }

                // 文字
                updateMvd_OneCellData_Text(cell);

                //二维码
                updateMvd_OneCellData_Barcode(cell);
            }
            //catch (Exception ex)
            //{
            //    string errMsg = $"顯示結果異常: {ex.Message}\n\r\n\r@{ex.StackTrace}";
            //    MessageBox.Show(errMsg);
            //    return;
            //}
        }
        void updateMvd_OneCellData_LinesOutsideBoxes(RegionCellX3Class cell)
        {
            if (InspectPara.bOpenLineMeasure)
            {
                // 繪製 邊線手拉框
                foreach (CMvdShape mvdShape in cell.cMvdShapesForFindLineRegion)
                {
                    if (mvdShape != null)
                    {
                        mvdShape.BorderColor = new MVD_COLOR(38, 127, 0);
                        DSMain.mvdRenderActivex1.AddShape(mvdShape);
                    }
                }
            }
        }
        void updateMvd_OneCellData_LinesOutSide(RegionCellX3Class cell, PointF offset)
        {
            if (InspectPara.bOpenLineMeasure)
            {
                // 繪製 找到的邊線
                foreach (CMvdLineSegmentF mLine in cell.cMvdLineSegmentFsOut)
                {
                    if (mLine != null)
                    {
                        var p1 = mLine.StartPoint;
                        var p2 = mLine.EndPoint;
                        p1.fX += offset.X;
                        p1.fY += offset.Y;
                        p2.fX += offset.X;
                        p2.fY += offset.Y;
                        //MVD_POINT_F s0 = new MVD_POINT_F(mLine.StartPoint.fX + offset.X,
                        //    mLine.StartPoint.fY + offset.Y);
                        //MVD_POINT_F s1 = new MVD_POINT_F(mLine.EndPoint.fX + offset.X,
                        //    mLine.EndPoint.fY + offset.Y);
                        //CMvdLineSegmentF newLine = new CMvdLineSegmentF(s0, s1);
                        CMvdLineSegmentF newLine = new CMvdLineSegmentF(p1, p2);
                        newLine.BorderColor = new MVD_COLOR(255, 0, 255);
                        DSMain.mvdRenderActivex1.AddShape(newLine);
                    }
                }
            }
        }
        void updateMvd_OneCellData_LinesInSide(RegionCellX3Class cell, PointF offset)
        {
            if (InspectPara.bOpenLineMeasure && InspectPara.bCheckMeasureOffset)
            {
                foreach (CMvdLineSegmentF mLine in cell.cMvdLineSegmentFsInSide)
                {
                    if (mLine != null)
                    {
                        var p1 = mLine.StartPoint;
                        var p2 = mLine.EndPoint;
                        p1.fX += offset.X;
                        p1.fY += offset.Y;
                        p2.fX += offset.X;
                        p2.fY += offset.Y;
                        //MVD_POINT_F s0 = new MVD_POINT_F(
                        //      mLine.StartPoint.fX + offset.X,
                        //      mLine.StartPoint.fY + offset.Y);
                        //MVD_POINT_F s1 = new MVD_POINT_F(
                        //      mLine.EndPoint.fX + offset.X,
                        //      mLine.EndPoint.fY + offset.Y);
                        CMvdLineSegmentF newLine = new CMvdLineSegmentF(p1, p2);
                        newLine.BorderColor = new MVD_COLOR(112, 48, 160);
                        DSMain.mvdRenderActivex1.AddShape(newLine);
                    }
                }
            }
        }
        void updateMvd_OneCellData_Text(RegionCellX3Class cell)
        {
            try
            {
                // 注意: MVD 顯示中文, 在 繁簡不同windows下, 會出現亂碼 !!!
                var formatter = new MainDispTextFormatter();
                string cellInfoStr = formatter.Format(cell);

                var mvdDrawResultRectF = cell.DrawResultRectF();

                //显示结果的xy angle
                CMvdTextF cMvdTextFShowMain = new CMvdTextF(
                                                mvdDrawResultRectF.CenterX,
                                                mvdDrawResultRectF.CenterY,
                                                cellInfoStr);

                cMvdTextFShowMain.BorderColor = new MVD_COLOR(0, 255, 0);// cell.DrawResultRectF().BorderColor;// new MVD_COLOR(0, 255, 0);
                cMvdTextFShowMain.FontWidth = 11;
                cMvdTextFShowMain.FillColor = new MVD_COLOR(0, 0, 0, 50);

                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                {
                    //引导数据
                    if (INI.Instance.IsResultShowChar)
                        DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowMain);
                    //定位框
                    DSMain.mvdRenderActivex1.AddShape(mvdDrawResultRectF);
                    //DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(true));
                }
                else
                {
                    //引导数据
                    if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                    {
                        if (INI.Instance.IsResultShowChar)
                        {
                            cMvdTextFShowMain = new CMvdTextF(
                                mvdDrawResultRectF.CenterX,
                                mvdDrawResultRectF.CenterY,
                                $"{cellInfoStr}{Environment.NewLine}{GaUtil.GetEnumDescription(cell.inspectReason)}");

                            cMvdTextFShowMain.BorderColor = new MVD_COLOR(255, 0, 0);
                            DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowMain);

                            //定位框
                            DSMain.mvdRenderActivex1.AddShape(mvdDrawResultRectF);
                        }
                    }
                    else
                    {
                        DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(false));
                    }
                }
            }
            catch(Exception ex)
            {
                LtDebug.LOG.Error(ex, "updateMvd_OneCellData_Text 異常!");
            }
        }
        void updateMvd_OneCellData_Barcode(RegionCellX3Class cell)
        {
            //二维码
            if (cell.DrawBarcodePosition != null)
            {
                DSMain.mvdRenderActivex1.AddShape(cell.DrawBarcodePosition);
                CMvdTextF _CodeText
                    = new CMvdTextF(cell.DrawBarcodePosition.GetVertex(2).fX,
                                                  cell.DrawBarcodePosition.GetVertex(2).fY + 120,
                                                  cell.RunCodeInfo.Content);
                _CodeText.BorderColor = new MVD_COLOR(0, 255, 0);
                //_CodeText.FontWidth = 11;
                _CodeText.FillColor = new MVD_COLOR(0, 0, 0);
                DSMain.mvdRenderActivex1.AddShape(_CodeText);

            }
        }
        void updateMvd_OutGrid_Blocs()
        {
            // 显示格点之外的料件
            switch (pRun.xScanInspectMode)
            {
                case ScanInspectMode.NOTRAY:

                    foreach (var rect in xRecipe.xOutBlocs)
                    {
                        PointF ptCenter = new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                        CMvdTextF cMvdTextFShowNoTray = new CMvdTextF( ptCenter.X,
                                                                       ptCenter.Y,
                                                                       $"疑似有料");

                        cMvdTextFShowNoTray.BorderColor = new MVD_COLOR(255, 0, 0);
                        cMvdTextFShowNoTray.FontWidth = 11;

                        DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowNoTray);
                        CMvdRectangleF rectRect = new CMvdRectangleF(ptCenter.X, ptCenter.Y, 200, 200);
                        rectRect.BorderColor = new MVD_COLOR(255, 0, 0);
                        DSMain.mvdRenderActivex1.AddShape(rectRect);
                    }

                    break;
            }
        }
        #endregion

        void generate_report_and_log()
        {
            string lotId = pRun.LotId;
            string stripId = pRun.StripId;
            string fileName = pRun.FileName;

            // 報表
            IxReportBuilder report = Universal.GetReportBuilder();
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

        public override void Tick()
        {
            TickFlyCameras();
            TickAllProcesses();
        }

        void CGOperate()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }


    //------------------------------------------
    // 準備分離 PlcFlyCameraCtrl
    //------------------------------------------
    partial class GaMainCtrl
    {
        #region GUI_MEMBERS
        Control lblSerialNumber;
        MVSUI[] _DSFLYs;
        MVSUI DSFly0 => _DSFLYs[0];
        MVSUI DSFly1 => _DSFLYs[1];
        MVSUI DSFly2 => _DSFLYs[2];
        MVSUI DSFly3 => _DSFLYs[3];
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

        #region FLY_DATA_BYTES
        List<byte[]> bytesFlyDatas = new List<byte[]>();
        #endregion

        void Attach(MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _DSFLYs = DsFlys;
            lblSerialNumber = lblFlyCameraSerialNo;

            lblSerialNumber.DoubleClick += (s, e) => clearFlyDataBytes();
            IxFlyAreaCam.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;
        }
        
        void clearFlyDataBytes()
        {
            bytesFlyDatas?.Clear();
        }

        void updateFlyCameraSerialNumber(int serialNumber)
        {
            _wndOwner?.Invoke(new Action(() =>
            {
                lblSerialNumber.Text = $"飞拍序号:{serialNumber}";
                lblSerialNumber.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
            }));
        }
        
        void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
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

                //frmOwner.Invoke(new Action(() =>
                //{
                //    lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
                //}));
                updateFlyCameraSerialNumber(serialNumber: bytesFlyDatas.Count);

                if (bytesFlyDatas.Count >= 4)
                {
                    MACHINE.PLCIO.bFlyReady = false;

                    int iflystartindex = MACHINE.PLCIO.iFlyStart;
                    iFlyIndex = 3;
                    flyRunning(iflystartindex, cameraFrame);

                    //MACHINE.PLCIO.bFlyDone = true;

                    MACHINE.PLCIO.iFlyResult(iFlyResult);
                    MACHINE.PLCIO.rOffset(iFlyOffset);

                    MACHINE.PLCIO.bFlyDone = true;

                    bytesFlyDatas.Clear();

                    #region LOG_RESULTS
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
                    #endregion

                    MACHINE.PLCIO.bFlyReady = true;
                }
            }
        }

        void TickFlyCameras()
        {

            //btnReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Red : Color.FromArgb(192, 255, 192));
            //if (m_LineScanProcess.IsOn)
            //    lblState.Text = ToChangeLanguage("执行-线扫测试中") + m_LineScanProcess.ID.ToString();
            //else
            //    lblState.Text = ToChangeLanguage("等待");

            //frmOwner.Invoke(new Action(() =>
            //{
            //    lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
            //    lblNumberStr.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
            //}));
            updateFlyCameraSerialNumber(serialNumber: bytesFlyDatas.Count);

            if (MACHINE.PLCIO.bSoftwareReady)
            {
                if (MACHINE.PLCIO.bScanStart)
                {
                    if (!m_plcStartOld)
                    {
                        m_plcStartOld = true;

                        _LOG("接收到plc启动信号", Color.Black);
                        if (!m_LineScanProcess.IsOn)
                        {
                            m_StripId = MACHINE.PLCIO.sStripID;
                            m_LotId = MACHINE.PLCIO.sLotID;

                            m_LineScanProcess.Start();
                        }
                        else
                        {
                            _LOG("测试中#PLC重复启动", Color.Black);
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
                        _LOG("接收到plc飞拍1启动信号", Color.Black);
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
                        _LOG("接收到plc飞拍2启动信号", Color.Black);
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

                //        _LOG("接收到plc抓图信号", Color.Black);
                //        if (!m_LineScanProcess.IsOn)
                //        {
                //            m_LineScanProcess.Start("Snap");
                //        }
                //        else
                //        {
                //            _LOG("测试中#PLC重复抓图", Color.Black);
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
        void flyProcessPro(int flyStart, int flyIndex, JetEazy.CCDSpace.CameraFrame cameraFrame, byte[] eBytes)
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
            GaUtil.BoundRect(ref _rectF, bmpFlyOperate.Size);
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

            var RectangleShape
                = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
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
            GaUtil.BoundRect(ref _rectF, bmpFlyOperate.Size);
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

            CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(bmpFlyOperate);

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
        Bitmap ConvertFromMONO(byte[] rgbaData, int width, int height)
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
