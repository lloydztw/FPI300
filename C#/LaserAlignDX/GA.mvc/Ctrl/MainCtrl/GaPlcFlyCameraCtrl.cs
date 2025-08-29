using JetEazy.BasicSpace;
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
    public partial class GaPlcFlyCameraCtrl : IxTickable
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
        Stopwatch _flyStopWatch = new Stopwatch();
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
        
        public void Tick()
        {
            TickFlyCameras();
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
            if (this.IsBypassFlyCameraTriggers)
            {
                return;
            }

            if (MACHINE.PLCIO.bFlyReady)
            {
                // 收集 Data Bytes
                byte[] bmpbytes = new byte[cameraFrame.uBytes];
                Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);

                // 加入到 the BUFFER list of the Data Bytes
                _bytesFlyDatas.Add(bmpbytes);
                updateFlyCameraSerialNumber(serialNumber: _bytesFlyDatas.Count);

                // 集滿 所有 (4個) 相機組 
                if (_bytesFlyDatas.Count >= TOTAL_FLY_CAMERAS)
                {
                    MACHINE.PLCIO.bFlyReady = false;

                    int iflystartindex = MACHINE.PLCIO.iFlyStart;
                    m_iFlyIndex = 3;

                    flyRunning(iflystartindex, cameraFrame);

                    // 回寫飛拍結果給 PLC
                    // MACHINE.PLCIO.bFlyDone = true;
                    MACHINE.PLCIO.iFlyResult(m_iFlyResult);
                    MACHINE.PLCIO.rOffset(m_iFlyOffset);

                    // 設定 PLC 旗標 bFlyDone
                    MACHINE.PLCIO.bFlyDone = true;

                    // 清除 the BUFFER list of the Data Bytes
                    _bytesFlyDatas.Clear();

                    #region LOG
                    _LOG_FLY_RESULTS(m_iFlyResult, m_iFlyOffset);
                    #endregion

                    // 設定 PLC 旗標 bFlyReady
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

            updateFlyCameraSerialNumber(serialNumber: _bytesFlyDatas.Count);

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
                        
                        _bytesFlyDatas.Clear();
                        
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
                        
                        _bytesFlyDatas.Clear();
                        
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
        void flyRunning(int flyStart, JetEazy.CCDSpace.CameraFrame cameraFrame)
        {
            int iShowIndex = 0;

            // 從 the BUFFER list 後面,
            // 開始取出每一組 data bytes
            // why?

            for( int camSerialNo = _bytesFlyDatas.Count - 1;
                 camSerialNo >= 0;
                 camSerialNo --,
                 iShowIndex ++ )
            {
                byte[] dataBytes = _bytesFlyDatas[camSerialNo];

                using (Bitmap bmpOnTheFly = allocateBitmap(dataBytes, cameraFrame))
                {
                    if (FlyParaClass.Instance.xIsOpenMuit)
                        flyProcessProSpecial(flyStart, iShowIndex, bmpOnTheFly);
                    else
                        flyProcessPro(flyStart, iShowIndex, bmpOnTheFly);
                }

                // 這個 member data 存在的目的是啥 ?
                m_iFlyIndex--;
            }
        }

#if(OPT_NOT_USED)
        void flyProcess(int flyStart, int flyIndex, JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
        {
            _flyStopWatch.Restart();
            
            //转换图像
            byte[] bmpbytes = new byte[cameraFrame.uBytes];
            Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;

            m_bmpFlyOperate?.Dispose();
            m_bmpFlyOperate = convertFromMONO(bmpbytes, iw, ih);

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

            CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(m_bmpFlyOperate);

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
#endif

        void flyProcessPro(int flyStart, int flyIndex, Bitmap bmpOnTheFly)
        {
            _flyStopWatch.Restart();

            // 建立 ROI
            RectangleF _rectF = new RectangleF(
                                    xRecipe.xRectRegionPrintFly.X,
                                    xRecipe.xRectRegionPrintFly.Y,
                                    xRecipe.xRectRegionPrintFly.Width,
                                    xRecipe.xRectRegionPrintFly.Height);
            _rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            GaUtil.BoundRect(ref _rectF, bmpOnTheFly.Size);

            using (Bitmap bmptemp = bmpOnTheFly.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                int iret = xRecipe.PrintTempFlyRun(bmptemp);

                PointF centerOrg = new PointF(
                                        xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                                        xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
                PointF centerRun = new PointF(
                                        xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
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
                            centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                               xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

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
                            centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                              xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

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

                    _flyStopWatch.Stop();
                    long ms = _flyStopWatch.ElapsedMilliseconds;

                    var RectangleShape
                        = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
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

                    // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                    // 不要用複製貼上 !!!
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
        }
        void flyProcessProSpecial(int flyStart, int flyIndex, Bitmap bmpOnTheFly)
        {
            _flyStopWatch.Restart();

            // 建立 ROI
            RectangleF _rectF = new RectangleF(
                                    xRecipe.xRectRegionPrintFly.X,
                                    xRecipe.xRectRegionPrintFly.Y,
                                    xRecipe.xRectRegionPrintFly.Width,
                                    xRecipe.xRectRegionPrintFly.Height);
            _rectF.Inflate(xFlyPara.xExtendx, xFlyPara.xExtendy);
            GaUtil.BoundRect(ref _rectF, bmpOnTheFly.Size);

            using (Bitmap bmptemp = bmpOnTheFly.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
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

                    _flyStopWatch.Stop();
                    long ms = _flyStopWatch.ElapsedMilliseconds;

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
                                Center.X + _rectF.X,
                                Center.Y + _rectF.Y,
                                $"[{flyShowIndex}] x:{m_iFlyOffset[flyIndex * 3 + 0].ToString("0.000")}," +
                                $"y:{m_iFlyOffset[flyIndex * 3 + 1].ToString("0.000")}," +
                                $"a:{m_iFlyOffset[flyIndex * 3 + 2].ToString("0.000")}");
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

                    // 超過多行字數很多的 "重複" 的代碼, 請拉出成為 function
                    // 不要用複製貼上 !!!

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
        #endregion

        #region BITMAP_CONVERT_FUNCTIONS
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
            CommonLogClass.Instance.LogMessage(msg, color);
        }
        #endregion
    }
}
