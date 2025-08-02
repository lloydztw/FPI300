using Common.RecipeSpace;
using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.CCDSpace;
using JetEazy.Interface;
using JetEazy.XContainer;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.ControlSpace.MachineSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.RunSpace;
using LaserAlignDX.UISpace.UIMVC;
using NeedleX.ProcessSpace;
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
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
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

        Button btnReady;

        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        protected FlyParaClass xFlyPara
        {
            get { return FlyParaClass.Instance; }
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

        }

        private void LblNumberStr_DoubleClick(object sender, EventArgs e)
        {
            bytesFlyDatas.Clear();
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
                                flyProcessProSpecial(2, iFlyIndex, cameraFrame, bmpdata);
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

        #region 飞拍测试流程

        Stopwatch flystopwatch = new Stopwatch();

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

            CMvdImage cMvdImage = BitmapToCMvdImage(bmpFlyOperate);

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

            if (INI.Instance.IsSaveDebugBMP)
            {
                string flypath = $"D:\\FlyImage";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{flyIndex.ToString()}.jpg";
                cMvdImage.SaveImage(flypath + "\\" + flyname, MVD_FILE_FORMAT.MVD_FILE_JPEG);
            }
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
            BoundRect(ref _rectF, bmpFlyOperate.Size);
            Bitmap bmptemp = bmpFlyOperate.Clone(_rectF, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int iret = xRecipe.PrintTempFlyRun(bmptemp);
            PointF centerOrg = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);
            PointF centerRun = new PointF(xRecipe.xRectRegionPrintFly.X + xRecipe.xRectRegionPrintFly.Width / 2,
                xRecipe.xRectRegionPrintFly.Y + xRecipe.xRectRegionPrintFly.Height / 2);

            float _resolutionFly = INI.Instance.FlyImageResolution;

            switch (flyStart)
            {
                case 1:

                    iFlyResult[flyIndex] = (iret == 0 ? 1 : 2);
                    if (iret == 0)
                    {
                        centerRun = new PointF(xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterX + _rectF.X,
                           xRecipe.mvdprintFlytemp_Find.xResults[0].fCenterY + _rectF.Y);

                        //算出的pix需加入解析度
                        iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly;
                        iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly;
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
                        iFlyOffset[flyIndex * 3 + 0] = -(centerRun.X - centerOrg.X) * _resolutionFly;
                        iFlyOffset[flyIndex * 3 + 1] = -(centerRun.Y - centerOrg.Y) * _resolutionFly;
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

            CMvdImage cMvdImage = BitmapToCMvdImage(bmpFlyOperate);

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
                string flypath = $"D:\\FlyImage";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{flyShowIndex.ToString()}.jpg";
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

            CMvdImage cMvdImage = BitmapToCMvdImage(bmpFlyOperate);

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
                string flypath = $"D:\\FlyImage";
                if (!Directory.Exists(flypath))
                    Directory.CreateDirectory(flypath);
                string flyname = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{flyShowIndex.ToString()}.jpg";
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

                    //CMvdTextF cMvdTextF = new CMvdTextF(1800, 500, $"耗时:{ProcessRunFPIClass.Instance.ElapsedTime.ToString("0.00")}ms");
                    //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                    //cMvdTextF.FontWidth = 18;
                    //DSMain.mvdRenderActivex1.AddShape(cMvdTextF);

                    //收集所有信息
                    string _collectStrMsg = string.Empty;

                    //所有框的显示
                    foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
                    {
                        _collectStrMsg += $"({cell.ToResultStr()})";
                        ////显示结果的xy angle
                        //CMvdTextF cMvdTextFShowMain = new CMvdTextF(cell.DrawResultRectF().CenterX,
                        //    cell.DrawResultRectF().CenterY,
                        //    $"{cell.ToShowMainStr()}");
                        //cMvdTextFShowMain.BorderColor = cell.DrawResultRectF().BorderColor;// new MVD_COLOR(0, 255, 0);
                        //cMvdTextFShowMain.FontWidth = 18;

                        //CollectResultClass collectResult = new CollectResultClass();
                        //collectResult.loc = new RectangleF(cell.DrawResultRectF().LeftTopX,
                        //        cell.DrawResultRectF().LeftTopY,
                        //        cell.DrawResultRectF().Width,
                        //        cell.DrawResultRectF().Height);
                        //collectResult.ispass = cell.DrawResultRectF().BorderColor.nG == 255;
                        //collectResult.desc = string.Empty;
                        //collectResultClasses.Add(collectResult);

                        CMvdRectangleF mvdRectangleF = cell.DrawResultRectF();
                        switch (ProcessRunFPIClass.Instance.xScanInspectMode)
                        {
                            case ScanInspectMode.NOTRAY:
                                //填写数据 疑似有料
                                string strNoTray = cell.GetNoTrayDesc();
                                //CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(mvdRectangleF.CenterX,
                                //                                mvdRectangleF.CenterY,
                                //                                $"{cell.GetNoTrayDesc()}");
                                if (!string.IsNullOrEmpty(strNoTray))
                                {
                                    CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(mvdRectangleF.CenterX,
                                                                mvdRectangleF.CenterY,
                                                                $"{strNoTray}");

                                    cMvdTextFShowNoTray.BorderColor = new MVD_COLOR(255, 0, 0);
                                    cMvdTextFShowNoTray.FontWidth = 11;

                                    DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowNoTray);
                                    DSMain.mvdRenderActivex1.AddShape(cell.DrawNoTrayRectF(false));
                                }
                                else
                                {
                                    var mvdRect = cell.DrawNoTrayRectF();
                                    DSMain.mvdRenderActivex1.AddShape(mvdRect);
                                }

                                break;
                            case ScanInspectMode.MEASUREAOI:
                            case ScanInspectMode.QRCODE:
                            default:
                                //显示结果的xy angle
                                CMvdTextF cMvdTextFShowMain = new CMvdTextF(cell.DrawResultRectF().CenterX,
                                    cell.DrawResultRectF().CenterY,
                                    $"{cell.ToShowMainStr()}");
                                cMvdTextFShowMain.BorderColor = cell.DrawResultRectF().BorderColor;// new MVD_COLOR(0, 255, 0);
                                cMvdTextFShowMain.FontWidth = 11;

                                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                                {
                                    //引导数据
                                    DSMain.mvdRenderActivex1.AddShape(cMvdTextFShowMain);
                                    //定位框
                                    DSMain.mvdRenderActivex1.AddShape(cell.DrawResultRectF());
                                }
                                else
                                {
                                    DSMain.mvdRenderActivex1.AddShape(cell.DrawNoTrayRectF(false));
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


                                    //CollectResultClass collectResult2D = new CollectResultClass();
                                    //MVD_RECT_F mVD_RECT = cell.DrawBarcodePosition.GetBoundingRect();
                                    //collectResult2D.loc = new RectangleF(mVD_RECT.fX,
                                    //        mVD_RECT.fY,
                                    //        mVD_RECT.fWidth,
                                    //        mVD_RECT.fHeight);
                                    //collectResult2D.ispass = true;
                                    //collectResult2D.desc = cell.RunCodeInfo.Content;
                                    //collectResultClasses.Add(collectResult2D);

                                }
                                break;
                        }
                    }

                    //CommonLogClass.Instance.LogMessage($"批号:{xRecipe.xLotNoStr}#数据信息:{_collectStrMsg}", Color.Black);

                    //_updateDgvData();
                    DSMain.mvdRenderActivex1.Display();
                    //MappingUpdate();
                    FireChangeState(MainS1State.M_SHOWRESULT, e.Tag as string);
                    if (ProcessRunFPIClass.Instance.IsPass)
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
            if (e.Tag != null && e.Tag is Bitmap)
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
                        //@LETIAN: 2022/07/01 改用 GdxDispUI 增加一些 fps
                        // bmp 由 Sender maintains life cycle.
                        // 在此不用 Dispose
                        //Bitmap bmp = (Bitmap)e.Tag;
                        //dispUI1.UpdateLiveImage(bmp);
                        //DS1.ReplaceDisplayImage(bmp);
                        DSMain.mvdRenderActivex1.LoadImageFromObject(ProcessRunFPIClass.Instance.cMvdInput.Clone());
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

        private CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            CMvdImage cMvdImage = new CMvdImage();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }
    }
}
