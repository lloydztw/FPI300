using AUVision;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.UISpace.UIMVC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Traveller106;
using VisionDesigner;
using VisionDesigner.BlobFind;
using VisionDesigner.PositionFix;
using VsCommon.ControlSpace.MachineSpace;

// 關聯到 MINIX6 ???
using LineScanProcess = TravellerMINIX6.ProcessSpace.LineScanProcess;


namespace LaserAlignDX.Mvc.Ctrl
{
    /// <summary>
    /// 重整 MainX3UI 飛拍
    /// 將飛拍控制拉出來到 GaPlcFlyCameraCtrl
    /// ToDO: 
    /// 需要把 flyProcessPro 與 flyProcessProSpecial 的 AOI 部分 抽離到 AoiModel 模塊內
    /// </summary>
    public partial class GaPlcFlyCameraCtrl : IxTickable, IDisposable
    {
        const int TOTAL_FLY_CAMERAS = 4;

        #region MACHINE
        MainFPIX3MachineClass MACHINE
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

        #region PLC_FLY_CAMERA_EXCHANGE_DATA
        int m_iFlyIndex = 0;
        int[] m_iFlyResult = new int[4];
        float[] m_iFlyOffset = new float[4 * 3];

        bool m_plcStartOld = false;
        bool m_plcGetImageOld = false;

        bool m_plcFlyStartOld1 = false;
        bool m_plcFlyStartOld2 = false;
        #endregion

        #region LOT_DATA_FROM_PLC
        string m_StripId = "Strip_NONE";
        string m_LotId = "Lot_NONE";
        #endregion

        #region THE_BUFFER_LIST_OF_THE_FLY_DATA_BYTES
        /// <summary>
        /// The buffer List of the fly data bytes
        /// </summary>
        List<byte[]> _bytesFlyDatas = new List<byte[]>();
        //int m_W = 2448;
        //int m_H = 2048;
        #endregion

        #region RUNTIME_DATA
        Stopwatch m_flyStopWatch = new Stopwatch();
        #endregion

        #region GUI_MEMBERS
        MVSUI[] _DSFLYs;
        MVSUI DSFly0 => _DSFLYs[0];
        MVSUI DSFly1 => _DSFLYs[1];
        MVSUI DSFly2 => _DSFLYs[2];
        MVSUI DSFly3 => _DSFLYs[3];
        Control lblSerialNumber;
        #endregion

        public void Attach(MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _DSFLYs = DsFlys;
            lblSerialNumber = lblFlyCameraSerialNo;

            lblSerialNumber.DoubleClick += (s, e) => clearFlyDataBytes();
            FlyCamera.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;
        }

        public void Dispose()
        {
            cPositionFixToolObj?.Dispose();
            cPositionFixToolObj = null;
        }

        public void Tick()
        {
            TickPlc();
        }

        void clearFlyDataBytes()
        {
            _bytesFlyDatas?.Clear();
        }

        void updateFlyCameraSerialNumber(int serialNumber)
        {
            //frmOwner?.Invoke(new Action(() =>
            //{
            //    lblSerialNumber.Text = $"飞拍序号:{serialNumber}";
            //    lblSerialNumber.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
            //}));
        }

        void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
        {
            // 如果 Recipe 編輯窗被打開, 則跳過此 event handler
            // (不是很好的設計!)
            if (Traveller106.Universal.IsOpenFlyForm)
            {
                return;
            }

            var plcIO = MACHINE?.PLCIO;
            if (plcIO == null)
                return;

            if (plcIO.bFlyReady)
            {
                // 複製 Data Bytes
                byte[] bmpbytes = new byte[cameraFrame.uBytes];
                Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);

                // 加入到 _bytesFlyDatas (the frame buffers list)
                _bytesFlyDatas.Add(bmpbytes);

                // 更新 gui
                updateFlyCameraSerialNumber(serialNumber: _bytesFlyDatas.Count);

                // 集滿 所有 (4個) 相機組
                if (_bytesFlyDatas.Count >= TOTAL_FLY_CAMERAS)
                {
                    // 清除 PLC 旗標 bFlyReady
                    plcIO.bFlyReady = false;

                    // 讀取 PLC iFlyStart
                    int iflystartindex = plcIO.iFlyStart;

                    m_iFlyIndex = 3;

                    // 處理 frame buffers
                    flyRunning(iflystartindex, cameraFrame);

                    // 回寫飛拍結果給 PLC
                    // plcIO.bFlyDone = true; 
                    plcIO.iFlyResult(m_iFlyResult);
                    plcIO.rOffset(m_iFlyOffset);

                    // 設定 PLC 旗標 bFlyDone
                    plcIO.bFlyDone = true;

                    // 清除 frame buffers list
                    _bytesFlyDatas.Clear();

                    #region LOG
                    //int[] ints0 = iFlyResult;
                    //float[] floats0 = iFlyOffset;
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
                    _LOG_FLY_RESULTS(m_iFlyResult, m_iFlyOffset);
                    #endregion

                    // 設定 PLC 旗標 bFlyReady
                    plcIO.bFlyReady = true;
                }
            }
        }

        void TickPlc()
        {
            //btnReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Red : Color.FromArgb(192, 255, 192));
            //if (m_LineScanProcess.IsOn)
            //    lblState.Text = ToChangeLanguage("执行-线扫测试中") + m_LineScanProcess.ID.ToString();
            //else
            //    lblState.Text = ToChangeLanguage("等待");

            //this.Invoke(new Action(() =>
            //{
            //    lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
            //    lblNumberStr.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
            //}));

            updateFlyCameraSerialNumber(serialNumber: _bytesFlyDatas.Count);

            var plcIO = MACHINE?.PLCIO;
            if (plcIO == null)
                return;

            if (plcIO.bSoftwareReady)
            {
                if (plcIO.bScanStart)
                {
                    if (!m_plcStartOld)
                    {
                        m_plcStartOld = true;

                        _LOG("接收到plc启动信号", Color.Black);
                        if (!m_LineScanProcess.IsOn)
                        {
                            m_StripId = plcIO.sStripID;
                            m_LotId = plcIO.sLotID;

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

                if (plcIO.iFlyStart == 1)
                {
                    if (!m_plcFlyStartOld1)
                    {
                        m_plcFlyStartOld1 = true;
                        _bytesFlyDatas.Clear();
                        _LOG("接收到plc飞拍1启动信号", Color.Black);
                    }
                }
                else
                {
                    m_plcFlyStartOld1 = false;
                }

                if (plcIO.iFlyStart == 2)
                {
                    if (!m_plcFlyStartOld2)
                    {
                        m_plcFlyStartOld2 = true;
                        _bytesFlyDatas.Clear();
                        _LOG("接收到plc飞拍2启动信号", Color.Black);
                    }
                }
                else
                {
                    m_plcFlyStartOld2 = false;
                }


                //if (plcIO.IsGetImage)
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
        //int[] m_iFlyResult { get => m_iFlyResult; set => m_iFlyResult = value; }
        //float[] m_iFlyOffset { get => m_iFlyOffset; set => m_iFlyOffset = value; }

        void flyRunning(int flyStart, JetEazy.CCDSpace.CameraFrame cameraFrame)
        {
            int camSerialIdx = _bytesFlyDatas.Count - 1;
            int iShowIndex = 0;

            // 從 the frame buffers list (_bytesFlyDatas) 後面,
            // 開始取出每一組取像 的 frame data bytes
            // QUESTION: 從後面往前取的目的是?

            while (camSerialIdx >= 0)
            {
                byte[] dataBytes = _bytesFlyDatas[camSerialIdx];

                using (Bitmap bmpOnTheFly = allocateBitmap(dataBytes, cameraFrame))
                {
                    if (FlyParaClass.Instance.xIsOpenMuit)
                        flyProcessProSpecial(flyStart, iShowIndex, bmpOnTheFly);
                    else
                        flyProcessPro(flyStart, iShowIndex, bmpOnTheFly);
                }

                // 這個 m_iFlyIndex 存在的目的是啥 ?
                m_iFlyIndex--;

                iShowIndex++;
                camSerialIdx--;
            }
        }

        void flyProcess(int flyStart, int flyIndex, Bitmap bmpOnTheFly)
        {
            m_flyStopWatch.Restart();
            ////转换图像
            //byte[] bmpbytes = new byte[cameraFrame.uBytes];
            //Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            //int iw = cameraFrame.iWidth;
            //int ih = cameraFrame.iHeight;
            //bmpFlyOperate.Dispose();
            //bmpFlyOperate = ConvertFromMONO(bmpbytes, iw, ih);
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

            using (CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(bmpOnTheFly))
            {
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

                //switch (flyIndex)
                //{
                //    case 0:
                //        DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                //        //DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        //DSFly0.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly0.mvdRenderActivex1.Display();
                //        break;
                //    case 1:
                //        DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                //        //DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        //DSFly1.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly1.mvdRenderActivex1.Display();
                //        break;
                //    case 2:
                //        DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                //        //DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        //DSFly2.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly2.mvdRenderActivex1.Display();
                //        break;
                //    case 3:
                //        DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                //        //DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        //DSFly3.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly3.mvdRenderActivex1.Display();
                //        break;
                //}

                updateToMvdDisplay(_DSFLYs[flyIndex], cMvdImage);

                //if (INI.Instance.IsSaveDebugBMP)
                //{
                //    string flypath = $"D:\\FlyImage";
                //    if (!Directory.Exists(flypath))
                //        Directory.CreateDirectory(flypath);
                //    string flyname = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{flyIndex.ToString()}.jpg";
                //    cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
                //}
            }
        }
        void flyProcessPro_bak_001(int flyStart, int flyIndex, Bitmap bmpOnTheFly)
        {
            m_flyStopWatch.Restart();

            // 建立 ROI
            RectangleF roiRectF = xRecipe.xRectRegionPrintFly;
            roiRectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            GaUtil.BoundRect(ref roiRectF, bmpOnTheFly.Size);

            Bitmap bmptemp = bmpOnTheFly.Clone(roiRectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            
            // AOI?
            int iret = xRecipe.PrintTempFlyRun(bmptemp);

            bmptemp?.Dispose();

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

                    m_iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + roiRectF.X,
                                               xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + roiRectF.Y);

                        //算出的pix需加入解析度
                        m_iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].X;
                        m_iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].Y;
                        m_iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
                case 2:

                    m_iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + roiRectF.X,
                                               xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + roiRectF.Y);

                        //算出的pix需加入解析度
                        m_iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].X;
                        m_iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].Y;
                        m_iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
            }

            using (CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(bmpOnTheFly))
            {
                m_flyStopWatch.Stop();
                long ms = m_flyStopWatch.ElapsedMilliseconds;

                //舊代碼: 這裡怎麼沒有使用 mvd tool 來處理 cMvdImage ???

                //转正的图形
                var _MatchResult = xRecipe.mvdprintFlytemp_Find.xResults[0];
                _MatchResult.fCenterX += roiRectF.X;
                _MatchResult.fCenterY += roiRectF.Y;
                var RectangleShapeBase
                        = new CMvdRectangleF(centerOrg.X, centerOrg.Y, roiRectF.Width, roiRectF.Height);
                var RectangleShape
                        = PositionFixRun(RectangleShapeBase,
                                        new RectangleF(0, 0, roiRectF.Width, roiRectF.Height),
                                        new Rectangle(0, 0, bmpOnTheFly.Width, bmpOnTheFly.Height),
                                        _MatchResult) as CMvdRectangleF;

                //var RectangleShape
                //    = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
                if (m_iFlyResult[flyIndex] == 1)
                    RectangleShape.BorderColor = new MVD_COLOR(0, 255, 0);
                else
                    RectangleShape.BorderColor = new MVD_COLOR(255, 0, 0);

                //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
                //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                //cMvdTextF.FontWidth = 20;

                CMvdTextF cMvdTextFResult = new CMvdTextF(
                            RectangleShape.CenterX,
                            RectangleShape.CenterY,
                            $"[{flyShowIndex}] x:{m_iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                            $"y:{m_iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                            $"a:{m_iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
                cMvdTextFResult.BorderColor = new MVD_COLOR(0, 255, 0);
                cMvdTextFResult.FontWidth = 20;

                //-------------------------------------------------------------------------------------------------------------------
                // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                // 不要用複製貼上 !!!
                //-------------------------------------------------------------------------------------------------------------------
                //switch (flyIndex)
                //{
                //    case 0:
                //        DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly0.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly0.AddCross();
                //        DSFly0.mvdRenderActivex1.Display();
                //        break;
                //    case 1:
                //        DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly1.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly1.AddCross();
                //        DSFly1.mvdRenderActivex1.Display();
                //        break;
                //    case 2:
                //        DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly2.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly2.AddCross();
                //        DSFly2.mvdRenderActivex1.Display();
                //        break;
                //    case 3:
                //        DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly3.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly3.AddCross();
                //        DSFly3.mvdRenderActivex1.Display();
                //        break;
                //}
                updateToMvdDisplay(_DSFLYs[flyIndex], cMvdImage, cMvdTextFResult, RectangleShape);

                //-------------------------------------------------------------------------------------------------------------------
                // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                // 不要用複製貼上 !!!
                //-------------------------------------------------------------------------------------------------------------------
                //if (INI.Instance.IsSaveDebugBMP)
                //{
                //    string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                //    if (!Directory.Exists(flypath))
                //        Directory.CreateDirectory(flypath);
                //    string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                //    cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
                //}

                saveFlyCameraImage(flyShowIndex, cMvdImage);
            }
        }

        void flyProcessPro(int flyStart, int flyIndex, Bitmap bmpOnTheFly)
        {
            m_flyStopWatch.Restart();

            // 建立 ROI
            // 因為 RectangleF 是 struct, 用 = 本身就是 copy 的動作, 所以不需要再額外 new RectangleF(...)
            RectangleF roiRectF = xRecipe.xRectRegionPrintFly;
            roiRectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            GaUtil.BoundRect(ref roiRectF, bmpOnTheFly.Size);

            int iret;
            using (Bitmap bmpCrop = bmpOnTheFly.Clone(roiRectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                // 舊代碼 對應此處的 bmptemp 使用後 沒有釋放!
                iret = xRecipe.PrintTempFlyRun(bmpCrop);
            }

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

                    m_iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + roiRectF.X,
                                               xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + roiRectF.Y);

                        //算出的pix需加入解析度
                        m_iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].X;
                        m_iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].Y;
                        m_iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
                case 2:

                    m_iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + roiRectF.X,
                                               xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + roiRectF.Y);

                        //算出的pix需加入解析度
                        m_iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].X;
                        m_iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly + xFlyPara.ptsOffset[flyShowIndex - 1].Y;
                        m_iFlyOffset[flyIndex * 3 + 2] = xRecipe.mvdprintFlytemp_Find.xResults[0].fAngle;
                    }
                    else
                    {
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
            }

            // 舊代碼 此處的 cMvdImage 使用後 沒有釋放!
            using (CMvdImage cMvdImage = GaImageUtil.BitmapToCMvdImage(bmpOnTheFly))
            {
                m_flyStopWatch.Stop();
                long ms = m_flyStopWatch.ElapsedMilliseconds;

                var RectangleShape = new CMvdRectangleF(centerRun.X, centerRun.Y, roiRectF.Width, roiRectF.Height);

                if (m_iFlyResult[flyIndex] == 1)
                {
                    //转正的图形
                    var _MatchResult = xRecipe.mvdprintFlytemp_Find.xResults[0];
                    ////_MatchResult.fCenterX += _rectF.X;
                    ////_MatchResult.fCenterY += _rectF.Y;

                    //var RectangleShapeBase
                    //    = new CMvdRectangleF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                    //                         xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2,
                    //                         xRecipe.xRectRegionPrintFly.Width,
                    //                         xRecipe.xRectRegionPrintFly.Height);
                    var RectangleShapeBase = GaImageUtil.ToCMvdRectangleF(ref xRecipe.xRectRegionPrint);

                    //RectangleShapeBase
                    //    = new CMvdRectangleF(0,
                    //                         0,
                    //                         xRecipe.xRectRegionPrintFly.Width,
                    //                         xRecipe.xRectRegionPrintFly.Height);

                    RectangleShape = PositionFixRun(RectangleShapeBase,
                                                    xRecipe.xRectRegionPrintFly,
                                                    new Rectangle(0, 0, bmpOnTheFly.Width, bmpOnTheFly.Height),
                                                    _MatchResult) as CMvdRectangleF;

                    RectangleShape.CenterX += roiRectF.X;
                    RectangleShape.CenterY += roiRectF.Y;
                }

                //var RectangleShape
                //    = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
                if (m_iFlyResult[flyIndex] == 1)
                    RectangleShape.BorderColor = new MVD_COLOR(0, 255, 0);
                else
                    RectangleShape.BorderColor = new MVD_COLOR(255, 0, 0);

                //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
                //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                //cMvdTextF.FontWidth = 20;

                //添加十字线
                CMvdLineSegmentF v1 = new CMvdLineSegmentF(new MVD_POINT_F(0, bmpOnTheFly.Height / 2),
                                                           new MVD_POINT_F(bmpOnTheFly.Width, bmpOnTheFly.Height / 2))
                {
                    BorderColor = new MVD_COLOR(255, 215, 0),
                    BorderWidth = 1
                };
                CMvdLineSegmentF h1 = new CMvdLineSegmentF(new MVD_POINT_F(bmpOnTheFly.Width / 2, 0),
                                                           new MVD_POINT_F(bmpOnTheFly.Width / 2, bmpOnTheFly.Height))
                {
                    BorderColor = new MVD_COLOR(255, 215, 0),
                    BorderWidth = 1
                };

                //文字方塊
                CMvdTextF cMvdTextFResult = new CMvdTextF(
                                RectangleShape.CenterX,
                                RectangleShape.CenterY,
                                $"[{flyShowIndex}] x:{m_iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                                $"y:{m_iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                                $"a:{m_iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");

                cMvdTextFResult.BorderColor = new MVD_COLOR(0, 255, 0);
                cMvdTextFResult.FontWidth = 20;

                //-------------------------------------------------------------------------------------------------------------------
                // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                // 不要用複製貼上 !!!
                //-------------------------------------------------------------------------------------------------------------------
                //switch (flyIndex)
                //{
                //    case 0:
                //        DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        DSFly0.mvdRenderActivex1.AddShape(v1);
                //        DSFly0.mvdRenderActivex1.AddShape(h1);
                //        //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly0.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly0.AddCross();
                //        DSFly0.mvdRenderActivex1.Display();
                //        break;
                //    case 1:
                //        DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        DSFly1.mvdRenderActivex1.AddShape(v1);
                //        DSFly1.mvdRenderActivex1.AddShape(h1);
                //        //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly1.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly1.AddCross();
                //        DSFly1.mvdRenderActivex1.Display();
                //        break;
                //    case 2:
                //        DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        DSFly2.mvdRenderActivex1.AddShape(v1);
                //        DSFly2.mvdRenderActivex1.AddShape(h1);
                //        //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly2.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly2.AddCross();
                //        DSFly2.mvdRenderActivex1.Display();
                //        break;
                //    case 3:
                //        DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        DSFly3.mvdRenderActivex1.AddShape(v1);
                //        DSFly3.mvdRenderActivex1.AddShape(h1);
                //        //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly3.mvdRenderActivex1.AddShape(RectangleShape);
                //        DSFly3.AddCross();
                //        DSFly3.mvdRenderActivex1.Display();
                //        break;
                //}
                updateToMvdDisplay(_DSFLYs[flyIndex], cMvdImage, v1, h1, cMvdTextFResult, RectangleShape);

                //-------------------------------------------------------------------------------------------------------------------
                // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                // 不要用複製貼上 !!!
                //-------------------------------------------------------------------------------------------------------------------
                //if (INI.Instance.IsSaveDebugBMP)
                //{
                //    string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                //    if (!Directory.Exists(flypath))
                //        Directory.CreateDirectory(flypath);
                //    string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                //    cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
                //}
                saveFlyCameraImage(flyShowIndex, cMvdImage);
            }
        }
        void flyProcessProSpecial(int flyStart, int flyIndex, Bitmap bmpOnTheFly)
        {
            m_flyStopWatch.Restart();

            // 建立 ROI
            // 因為 RectangleF 是 struct, 用 = 本身就是 copy 的動作, 所以不需要再額外 new RectangleF(...)
            RectangleF roiRectF = xRecipe.xRectRegionPrintFly;
            GaUtil.BoundRect(ref roiRectF, bmpOnTheFly.Size);

            // 舊代碼 bmptemp 使用後 沒有釋放!
            Bitmap bmptemp = bmpOnTheFly.Clone(roiRectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            bool bOK = xRecipe.CheckSpecialAngle(bmptemp, out List<CBlobInfo> list, out float angle, out System.Drawing.PointF Center);

            bmptemp?.Dispose();

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

                    m_iFlyResult[flyIndex] = (bOK ? 1 : 2);
                    if (bOK)
                    {
                        //算出的pix需加入解析度
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = angle;
                    }
                    else
                    {
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
                case 2:

                    m_iFlyResult[flyIndex] = (bOK ? 1 : 2);
                    if (bOK)
                    {
                        //算出的pix需加入解析度
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = angle;
                    }
                    else
                    {
                        m_iFlyOffset[flyIndex * 3 + 0] = 0;
                        m_iFlyOffset[flyIndex * 3 + 1] = 0;
                        m_iFlyOffset[flyIndex * 3 + 2] = 0;
                    }

                    break;
            }

            using (CMvdImage cMvdImage = GaImageUtil.BitmapToCMvdImage(bmpOnTheFly))
            {
                m_flyStopWatch.Stop();
                long ms = m_flyStopWatch.ElapsedMilliseconds;

                //var RectangleShape1 = new CMvdRectangleF(roiRectF.X, roiRectF.Y, roiRectF.Width, roiRectF.Height);
                //var RectangleShape2 = new CMvdRectangleF(roiRectF.X, roiRectF.Y, roiRectF.Width, roiRectF.Height);
                var RectangleShape1 = GaImageUtil.ToCMvdRectangleF(ref roiRectF);
                var RectangleShape2 = GaImageUtil.ToCMvdRectangleF(ref roiRectF);

                if (bOK)
                {
                    RectangleShape1 =
                        new CMvdRectangleF( list[0].RectInfo.CenterX + roiRectF.X,
                                            list[0].RectInfo.CenterY + roiRectF.Y,
                                            list[0].RectInfo.Width,
                                            list[0].RectInfo.Height);
                    RectangleShape2 =
                        new CMvdRectangleF( list[1].RectInfo.CenterX + roiRectF.X,
                                            list[1].RectInfo.CenterY + roiRectF.Y,
                                            list[1].RectInfo.Width,
                                            list[1].RectInfo.Height);
                }

                if (m_iFlyResult[flyIndex] == 1)
                    RectangleShape1.BorderColor = new MVD_COLOR(0, 255, 0);
                else
                    RectangleShape1.BorderColor = new MVD_COLOR(255, 0, 0);
                if (m_iFlyResult[flyIndex] == 1)
                    RectangleShape2.BorderColor = new MVD_COLOR(0, 255, 0);
                else
                    RectangleShape2.BorderColor = new MVD_COLOR(255, 0, 0);

                //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
                //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                //cMvdTextF.FontWidth = 20;

                CMvdTextF cMvdTextFResult = new CMvdTextF(
                                Center.X + roiRectF.X,
                                Center.Y + roiRectF.Y,
                                $"[{flyShowIndex}] x:{m_iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                                $"y:{m_iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                                $"a:{m_iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
                cMvdTextFResult.BorderColor = new MVD_COLOR(0, 255, 0);
                cMvdTextFResult.FontWidth = 15;

                //-------------------------------------------------------------------------------------------------------------------
                // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                // 不要用複製貼上 !!!
                //-------------------------------------------------------------------------------------------------------------------
                //switch (flyIndex)
                //{
                //    case 0:
                //        DSFly0.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly0.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly0.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly0.mvdRenderActivex1.AddShape(RectangleShape1);
                //        DSFly0.mvdRenderActivex1.AddShape(RectangleShape2);
                //        DSFly0.AddCross();
                //        DSFly0.mvdRenderActivex1.Display();
                //        break;
                //    case 1:
                //        DSFly1.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly1.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly1.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly1.mvdRenderActivex1.AddShape(RectangleShape1);
                //        DSFly1.mvdRenderActivex1.AddShape(RectangleShape2);
                //        DSFly1.AddCross();
                //        DSFly1.mvdRenderActivex1.Display();
                //        break;
                //    case 2:
                //        DSFly2.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly2.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly2.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly2.mvdRenderActivex1.AddShape(RectangleShape1);
                //        DSFly2.mvdRenderActivex1.AddShape(RectangleShape2);
                //        DSFly2.AddCross();
                //        DSFly2.mvdRenderActivex1.Display();
                //        break;
                //    case 3:
                //        DSFly3.mvdRenderActivex1.LoadImageFromObject(cMvdImage);
                //        //DSFly3.mvdRenderActivex1.AddShape(cMvdTextF);
                //        DSFly3.mvdRenderActivex1.AddShape(cMvdTextFResult);
                //        DSFly3.mvdRenderActivex1.AddShape(RectangleShape1);
                //        DSFly3.mvdRenderActivex1.AddShape(RectangleShape2);
                //        DSFly3.AddCross();
                //        DSFly3.mvdRenderActivex1.Display();
                //        break;
                //}
                updateToMvdDisplay(_DSFLYs[flyIndex], cMvdImage, cMvdTextFResult, RectangleShape1, RectangleShape2);

                //-------------------------------------------------------------------------------------------------------------------
                // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                // 不要用複製貼上 !!!
                //-------------------------------------------------------------------------------------------------------------------
                //if (INI.Instance.IsSaveDebugBMP)
                //{
                //    string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                //    if (!Directory.Exists(flypath))
                //        Directory.CreateDirectory(flypath);
                //    string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                //    cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
                //}
                saveFlyCameraImage(flyShowIndex, cMvdImage);
            }
        }

#if (OLD_CODE)
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
#endif

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

        #region PRIVATE_HELPER_FUNCTIONS
        void updateToMvdDisplay(MVSUI dispUI, CMvdImage mvdImage, params CMvdShape[] shapes)
        {
            var render = dispUI.mvdRenderActivex1;
            render.LoadImageFromObject(mvdImage);

            foreach (var shape in shapes)
                if (shape != null)
                    render.AddShape(shape);

            dispUI.AddCross();
            render.Display();
        }
        void saveFlyCameraImage(int flyShowIndex, CMvdImage cMvdImage)
        {
            if (INI.Instance.IsSaveDebugBMP)
            {
                string flypath = $"{INI.Instance.ResultImagePath}\\flyImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{m_StripId}";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{m_LotId}-[{flyShowIndex.ToString()}]-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.jpg";
                cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
            }
        }
        Bitmap allocateBitmap(byte[] dataBytes, JetEazy.CCDSpace.CameraFrame cameraFrame)
        {
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;
            Bitmap bmp = createMonoBitmap(dataBytes, iw, ih);
            return bmp;
        }
        Bitmap createMonoBitmap(byte[] rgbaData, int width, int height)
        {
            var pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);

            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                pixelFormat);

            IntPtr intPtr = bitmapData.Scan0;
            Marshal.Copy(rgbaData, 0, intPtr, rgbaData.Length);
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
            //>>> GaUtil.LOG(msg, args);
            _LOG(msg, color);
        }
        #endregion
    }
}
