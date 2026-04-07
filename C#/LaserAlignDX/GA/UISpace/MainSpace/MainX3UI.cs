using AUVision;
using Common.RecipeSpace;
using Eazy_Project_III;
using Eazy_Project_III.FormSpace;
using JetEazy.BasicSpace;
using JetEazy.CCDSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using JetEazy.XContainer;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.ControlSpace.MachineSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.RunSpace;
using LaserAlignDX.UISpace.UIMVC;
using NeedleX.ProcessSpace;
using OpenCvSharp.XFeatures2D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using VisionDesigner;
using VisionDesigner.BlobFind;
using VisionDesigner.PositionFix;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.UISpace.MainSpace
{
    public partial class MainX3UI : UserControl
    {
        List<CollectResultClass> collectResultClasses = new List<CollectResultClass>();
        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Traveller106.Universal.MACHINECollection;
            }
        }
        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE; }
        }

        const int FLYCOUNT = 4;
        //Bitmap[] bmpFlyOperate = new Bitmap[FLYCOUNT];
        Bitmap bmpFlyOperate = new Bitmap(1, 1);
        int iFlyIndex = 0;
        int[] iFlyResult = new int[4];
        float[] iFlyOffset = new float[4 * 3];
        Label lblNumberStr;

        bool m_plcStartOld = false;
        bool m_plcGetImageOld = false;

        bool m_plcFlyStartOld1 = false;
        bool m_plcFlyStartOld2 = false;

        string m_StripId = "Strip_NONE";
        string m_LotId = "Lot_NONE";

        Button btnReady;

        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        protected FlyParaClass xFlyPara
        {
            get { return FlyParaClass.Instance; }
        }
        protected InspectX3ParaClass InspectPara
        {
            get { return InspectX3ParaClass.Instance; }
        }
        protected ProcessRunFPIClass pRun
        {
            get { return ProcessRunFPIClass.Instance; }
        }

        IxLineScanCam IxFlyAreaCam
        {
            get { return Universal.IxFlyAreaCam; }
        }

        MVSUI DSMain
        {
            get
            {
                int iscanIndex = MACHINE.PLCIO.iScanStage;
                if (iscanIndex == 2)
                {
                    return mvsui2;
                }
                return mvsui1;
            }
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

        public MainX3UI()
        {
            InitializeComponent();
        }
        public void Init()
        {
            //int i = 0;
            //while (i < FLYCOUNT)
            //{
            //    bmpFlyOperate[i] = new Bitmap(1, 1);
            //    i++;
            //}

            //init_Display();
            //update_Display();
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);
            InitAllProcesses();

            //init_Display();
            //update_Display();

            //InitializeDataGridView();

            //MappingInit();

            lblNumberStr = label1;
            lblNumberStr.DoubleClick += LblNumberStr_DoubleClick;

            //lblState = label11;

            //btnSoftwareReady = button6;
            //btnSoftwareReady.Click += BtnSoftwareReady_Click;

            //btnReady = button6;
            //btnReady.Click += BtnReady_Click;
            //btnChangeReplaceImage = button1;
            //btnChangeReplaceImage.Click += BtnChangeReplaceImage_Click;

            //SizeChanged += MainX2UI_SizeChanged;

            SizeChanged += MainX3UI_SizeChanged;

            ////删除矩形菜单项，右键菜单中对应项会被删除
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdAddShape),
            //    System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdFile),
            //    System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdZoom),
            //  System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdRotate),
            //   System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdEraser),
            //   System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdShapeMenuPaste),
            //    System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);

            IxFlyAreaCam.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;

            HandleDestroyed += (s, e) => IxFlyAreaCam?.StopGrab();
        }

        private void DSFly0_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void LblNumberStr_DoubleClick(object sender, EventArgs e)
        {
            bytesFlyDatas.Clear();

            if (Traveller106.Universal.IsNoUseCCD)
            {
                string _flyfilenamepath = JzToolsClass.OpenFilePicker("JPEG Files (*.jpeg)|*.JPEG| + All files (*.*)|*.*", "");
                if (!string.IsNullOrEmpty(_flyfilenamepath))
                {
                    Bitmap bmp = new Bitmap(_flyfilenamepath);
                    flyProcessPro(1, 0, bmp);
                    bmp.Dispose();
                }
            }
        }

        List<byte[]> bytesFlyDatas = new List<byte[]>();
        int m_W = 2448;
        int m_H = 2048;

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
                this.Invoke(new Action(() =>
                {
                    lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
                }));
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
#if(NO_USE_CODE)
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

            CMvdImage cMvdImage = EzMvdImageConvertor.BitmapToCMvdImage(bmpFlyOperate);

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

        private void BtnReady_Click(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.bSoftwareReady = !MACHINE.PLCIO.bSoftwareReady;
            //if (m_LineScanProcess.IsOn)
            //    m_LineScanProcess.Stop();
        }

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
            m_MainProcess.OnCompleted += process_OnCompleted;
            m_BuzzerProcess.OnCompleted += buzzer_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnLiveImage += process_OnLiveImage;
            m_MainProcess.OnMessage += process_OnMessage;
            m_SingleProcess.OnMessage += process_OnMessage;
            m_LineScanProcess.OnMessage += process_OnMessage;
            m_SingleProcess.OnLiveImage += process_OnLiveImage;

            var aoiEngine = ProcessRunFPIClass.Instance;
            aoiEngine.OnAoiProgressing += AoiEngine_OnAoiProgressing;
            aoiEngine.OnAoiBegin += AoiEngine_OnAoiBegin;
            aoiEngine.OnAoiEnd += AoiEngine_OnAoiEnd;
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
            else if (sender == m_LineScanProcess || sender == m_SingleProcess)
            {
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
                    //DSMain.Invoke(new Action(() =>
                    //{
                    collectResultClasses.Clear();
                    //收集所有信息
                    string _collectStrMsg = string.Empty;
                    DSMain.mvdRenderActivex1.ClearShapes();
                    //string _reportStr = string.Empty;
                    StringBuilder reportBuilder = new StringBuilder();
                    reportBuilder.Append(ToReport1HeadStr());

                    //所有框的显示
                    foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
                    {
                        RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                        _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                        //BoundRect(ref _rectF, new Size((int)ProcessRunClass.Instance.cMvdInput.Width,
                        //                               (int)ProcessRunClass.Instance.cMvdInput.Height));

                        reportBuilder.Append(cell.ToReport1Str());

                        _collectStrMsg += $"({cell.ToResultStr()})";
                        CMvdRectangleF mvdRectangleF = cell.DrawResultRectF();
                        switch (pRun.xScanInspectMode)
                        {
                            case ScanInspectMode.NOTRAY:
                                //填写数据 疑似有料
                                try
                                {
                                    string strNoTray = cell.GetNoTrayDesc();
                                    if (!string.IsNullOrEmpty(strNoTray))
                                    {
                                        CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(mvdRectangleF.CenterX,
                                                                    mvdRectangleF.CenterY,
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
                                catch (Exception ex)
                                {
                                    //string errMsg = $"顯示結果異常: {ex.Message}\n\r\n\r@{ex.StackTrace}";
                                    //MessageBox.Show(errMsg);
                                    return;
                                }
                                break;
                            case ScanInspectMode.MEASUREAOI:
                            case ScanInspectMode.QRCODE:
                            default:
                                //try
                                {
                                    if (InspectPara.bOpenLineMeasure)
                                    {
                                        //画直线
                                        int i = 0;
                                        while (i < 4)
                                        {
                                            CMvdLineSegmentF mLine = cell.cMvdLineSegmentFsOut[i];
                                            if (mLine != null)
                                            {
                                                MVD_POINT_F s0 = new MVD_POINT_F(mLine.StartPoint.fX + _rectF.X,
                                                    mLine.StartPoint.fY + _rectF.Y);
                                                MVD_POINT_F s1 = new MVD_POINT_F(mLine.EndPoint.fX + _rectF.X,
                                                    mLine.EndPoint.fY + _rectF.Y);
                                                CMvdLineSegmentF newLine = new CMvdLineSegmentF(s0, s1);
                                                newLine.BorderColor = new MVD_COLOR(255, 0, 255);
                                                DSMain.mvdRenderActivex1.AddShape(newLine);
                                            }
                                            CMvdShape mvdShape = cell.cMvdShapesForFindLineRegion[i];
                                            if (mvdShape != null)
                                            {
                                                mvdShape.BorderColor = new MVD_COLOR(38, 127, 0);
                                                DSMain.mvdRenderActivex1.AddShape(mvdShape);
                                            }
                                            i++;
                                        }

                                        if (InspectPara.bCheckMeasureOffset)
                                        {
                                            //画直线
                                            i = 0;
                                            while (i < 4)
                                            {
                                                CMvdLineSegmentF mLine = cell.cMvdLineSegmentFsInSide[i];
                                                if (mLine != null)
                                                {
                                                    MVD_POINT_F s0 = new MVD_POINT_F(mLine.StartPoint.fX + _rectF.X,
                                                        mLine.StartPoint.fY + _rectF.Y);
                                                    MVD_POINT_F s1 = new MVD_POINT_F(mLine.EndPoint.fX + _rectF.X,
                                                        mLine.EndPoint.fY + _rectF.Y);
                                                    CMvdLineSegmentF newLine = new CMvdLineSegmentF(s0, s1);
                                                    newLine.BorderColor = new MVD_COLOR(112, 48, 160);
                                                    DSMain.mvdRenderActivex1.AddShape(newLine);
                                                }
                                                i++;
                                            }
                                        }
                                    }


                                    //显示结果的xy angle
                                    CMvdTextF cMvdTextFShowMain = new CMvdTextF(cell.DrawResultRectF().CenterX,
                                        cell.DrawResultRectF().CenterY,
                                        $"{cell.ToShowMainStr()}");
                                    cMvdTextFShowMain.BorderColor = new MVD_COLOR(0, 255, 0);// cell.DrawResultRectF().BorderColor;// new MVD_COLOR(0, 255, 0);
                                    cMvdTextFShowMain.FontWidth = 11;
                                    cMvdTextFShowMain.FillColor = new MVD_COLOR(0, 0, 0, 50);

                                    if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                                    {
                                        //引导数据
                                        if (INI.Instance.IsResultShowChar)
                                            DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowMain);
                                        //定位框
                                        DSMain.mvdRenderActivex1.AddShape(cell.DrawResultRectF());
                                        //DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(true));
                                    }
                                    else
                                    {
                                        //引导数据
                                        if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                                        {
                                            if (INI.Instance.IsResultShowChar)
                                            {

                                                cMvdTextFShowMain = new CMvdTextF(cell.DrawResultRectF().CenterX,
                                            cell.DrawResultRectF().CenterY,
                                            $"{cell.ToShowMainStr()}{Environment.NewLine}{GaUtil.GetEnumDescription(cell.inspectReason)}");
                                                cMvdTextFShowMain.BorderColor = new MVD_COLOR(255, 0, 0);
                                                DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowMain);

                                                //定位框
                                                DSMain.mvdRenderActivex1.AddShape(cell.DrawResultRectF());
                                            }
                                        }
                                        else
                                            DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(false));
                                    }

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
                                //catch (Exception ex)
                                //{
                                //    string errMsg = $"顯示結果異常: {ex.Message}\n\r\n\r@{ex.StackTrace}";
                                //    MessageBox.Show(errMsg);
                                //    return;
                                //}
                                break;
                        }
                    }

                    _LOG($"StripID:{pRun.StripId}", Color.Black);
                    _LOG($"LotID:{pRun.LotId}", Color.Black);
                    _LOG($"#数据信息:{_collectStrMsg}", Color.Black);


                    //存储report
                    string reportPath = $"{INI.Instance.ResultImagePath}\\report\\{DateTime.Now.ToString("yyyyMMdd")}\\{pRun.StripId}";
                    if (!System.IO.Directory.Exists(reportPath))
                    {
                        System.IO.Directory.CreateDirectory(reportPath);
                    }
                    GaUtil.SaveData(reportBuilder.ToString(), reportPath + $"\\{pRun.FileName.Replace(".jpg", ".csv")}");

                    #region 显示格点之外的料件

                    switch (pRun.xScanInspectMode)
                    {
                        case ScanInspectMode.NOTRAY:

                            foreach (var rect in xRecipe.xOutBlocs)
                            {
                                PointF ptCenter = new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                                CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(
                                                                                ptCenter.X,
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

                    #endregion

                    //_updateDgvData();
                    DSMain.mvdRenderActivex1.Display();

                    //MappingUpdate();
                    FireChangeState(MainS1State.M_SHOWRESULT, e.Tag as string);
                    if (pRun.IsPass)
                        FireChangeState(MainS1State.M_PASS);
                    else
                        FireChangeState(MainS1State.M_NG);
                    //}));
                }
                else if (e.Message.Contains("ResultX.Code"))
                {
                    INI.Instance.CurrentBarcodeStr = e.Tag as string;
                    FireChangeState(MainS1State.M_SHOWCODE, e.Tag as string);
                }
            }

            try
            {
                // Do whatever message you want to show to the operators.
                string msg = $"Process {((BaseProcess)sender).Name}, {e.Message}\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }

            CGOperate();
        }
        public string ToReport1HeadStr()
        {
            string str = string.Empty;

            str += $"编号" + ",";
            str += $"名称" + ",";
            str += $"是否检测" + ",";
            str += $"尺寸X" + ",";
            str += $"尺寸Y" + ",";
            str += $"位置偏移X" + ",";
            str += $"位置偏移Y" + ",";
            str += $"原始X" + ",";
            str += $"原始Y" + ",";
            str += $"引导偏移X" + ",";
            str += $"引导偏移Y" + ",";
            str += $"引导偏移角度" + ",";

            str += $"左边距" + ",";
            str += $"右边距" + ",";
            str += $"上边距" + ",";
            str += $"下边距" + ",";

            str += $"马达1-X" + ",";
            str += $"马达1-Y" + ",";
            str += $"马达2-X" + ",";
            str += $"马达2-Y" + ",";
            str += $"条码设定值" + ",";
            str += $"读取码" + ",";
            str += $"{Environment.NewLine}";

            return str;
        }

#if (OPT_MAIN_X6)
        string mainx6_path = "D:\\CollectPictures";
        private void MainX6Save()
        {
            Task task = new Task(() =>
            {
                try
                {
                    mainx6_path = "D:\\CollectPictures\\" + JzTimes.DateSerialString + "\\" + (ProcessRunFPIClass.Instance.IsPass ? "P-" : "F-") + ProcessRunFPIClass.Instance.FileBarcodeStr;

                    if (!Directory.Exists(mainx6_path + "\\000"))
                        Directory.CreateDirectory(mainx6_path + "\\000");

                    int qi = 0;
                    ProcessRunClass.Instance.cMvdInput.Clone().SaveImage(mainx6_path + "\\000\\P00-" + qi.ToString("000") + ".jpg", MVD_FILE_FORMAT.MVD_FILE_JPEG);


                }
                catch (Exception ex)
                {
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                }
            });
            task.Start();
        }
#endif

        void TickAllProcesses()
        {
            m_resetprocess.Tick();
            m_BuzzerProcess.Tick();
            m_LineScanProcess.Tick();
            m_MainProcess.Tick();
            m_SingleProcess.Tick();
        }

        private void process_OnLiveImage(object sender, ProcessEventArgs e)
        {
            if (e.Tag != null && e.Tag is Bitmap bmp)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        EventHandler<ProcessEventArgs> h = process_OnLiveImage;
                        this.Invoke(h, sender, e);
                    }
                    else
                    {
                        //NOTE: bmp 由 sender 維持其生命周期. 在此不用調用 Dispose !!!
                        //2025-08-28 @LETIAN: 巨圖 Bitmap 統一由 LineScanCamImageHolder 保管其生命週期 !!!
                        CMvdImage mvdImage = pRun.LineScanCamImageHolder.PeekMvdImage();
                        DSMain.mvdRenderActivex1.LoadImageFromObject(mvdImage);
                        DSMain.mvdRenderActivex1.ClearShapes();
                        DSMain.AddCross();
                        DSMain.mvdRenderActivex1.Display();
                    }
                }
                catch (Exception ex)
                {
                    //>>> 此一層的 try - catch 以後可以省略.
                    //>>> 會由 Event Sender 處理 exception
                    //throw ex;
                }
            }
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
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void buzzer_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = buzzer_OnCompleted;
                BeginInvoke(h, sender, e);
            }
            else
            {

            }
        }
        private void handle_main_process_completed(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = handle_main_process_completed;
                BeginInvoke(h, sender, e);
            }
            else
            {
                if (e.Message == "PartialCompleted")
                {

                }
                else
                {
                }
            }
        }

        FormProgressing _frmAoiProgressing = null;
        private void AoiEngine_OnAoiBegin(object sender, GaProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((EventHandler<GaProgressEventArgs>)AoiEngine_OnAoiBegin, sender, e);
            }
            else
            {
                if (_frmAoiProgressing == null)
                {
                    _frmAoiProgressing = new FormProgressing();
                    //_frmAoiProgressing.TopMost = true;
                    _frmAoiProgressing.SetTotalSteps(e.TotalSteps);
                    _frmAoiProgressing.UpdateProgress(e.CurrentStep);
                    _frmAoiProgressing.Show(this);
                    _frmAoiProgressing.BringToFront();
                }
            }
        }
        private void AoiEngine_OnAoiProgressing(object sender, GaProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<GaProgressEventArgs>)AoiEngine_OnAoiProgressing, sender, e);
            }
            else
            {
                _frmAoiProgressing?.UpdateProgress(e.CurrentStep);
            }
        }
        private void AoiEngine_OnAoiEnd(object sender, GaProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((EventHandler<GaProgressEventArgs>)AoiEngine_OnAoiEnd, sender, e);
            }
            else
            {
                _frmAoiProgressing?.Close();
                _frmAoiProgressing?.Dispose();
                _frmAoiProgressing = null;
            }
        }

        public void Tick()
        {
            _getPlcRunTick();
            TickAllProcesses();
        }
        public void ChangeRecipe()
        {

        }
        public void SetEnable(bool isendable)
        {
        }
        public void SetEnableState(bool isendable)
        {
        }

        private void _getPlcRunTick()
        {

            //btnReady.BackColor = (MACHINE.PLCIO.bSoftwareReady ? Color.Red : Color.FromArgb(192, 255, 192));
            //if (m_LineScanProcess.IsOn)
            //    lblState.Text = ToChangeLanguage("执行-线扫测试中") + m_LineScanProcess.ID.ToString();
            //else
            //    lblState.Text = ToChangeLanguage("等待");

            this.Invoke(new Action(() =>
            {
                lblNumberStr.Text = $"飞拍序号:{bytesFlyDatas.Count}";
                lblNumberStr.BackColor = (Traveller106.Universal.IsOpenFlyForm ? Control.DefaultBackColor : Color.Lime);
            }));

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

        void CGOperate()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public delegate void ChangeStateHandler(MainS1State status, object tag = null);
        public event ChangeStateHandler OnChangeState;
        protected void FireChangeState(MainS1State status, object tag = null)
        {
            if (OnChangeState != null)
            {
                OnChangeState(status, tag);
            }
        }

        #region AUTO_LAYOUT
        private void MainX3UI_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                _auto_layout();
            }
            catch
            {
            }
        }
        void _auto_layout()
        {
#if OPT_LETIAN_AUTO_LAYOUT

            var rcc = ClientRectangle;
            int pad = 3;

            int w = tabControl1.Width;
            int h = tabControl1.Height;

            //tabControl2.Width = rcc.Width - pad * 2;
            //tabControl2.Height = rcc.Height - pad * 3 - h;
            //tabControl2.Location = new Point(pad, pad);

            //groupBox1.Location = new Point(pad, tabControl2.Height + pad);
            //groupBox1.Width = rcc.Width - pad * 3 - w;
            //groupBox1.Height = h;

            //tabControl1.Location = new Point(groupBox1.Width + pad, tabControl2.Height + pad);

#endif
        }
        #endregion

        protected void _LOG(string msg, params object[] args)
        {
#if (true)
            Color color = Color.Black;

            int N = args.Length;
            if (N > 0 && args[N - 1] is Color)
            {
                color = (Color)args[N - 1];
                N -= 1;
            }

            var sb = new System.Text.StringBuilder();
            sb.Append(Name);
            sb.Append(", ");
            sb.Append(msg);

            for (int i = 0; i < N; i++)
            {
                sb.Append(", ");
                sb.Append(args[i]);
            }

            msg = sb.ToString();
            CommonLogClass.Instance.LogMessage(msg, color);
            //if (color == Color.Red)
            //    GdxGlobal.LOG.Warn(msg);
            //else
            //    GdxGlobal.LOG.Debug(msg);
#endif
            msg = Name + ", " + msg;
            //GdxGlobal.LOG.Log(msg, args);
        }
    }
}
