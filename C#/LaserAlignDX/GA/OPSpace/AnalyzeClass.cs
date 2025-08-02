using AForge.Imaging.Filters;
using AForge.Math;
using AHBlobPro;
using AUVision;
using BSA.ControlSpace;
using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
//using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.PropertyGridSpace;
using JetEazy.QMath;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TestDemo.LaserDot;
using Traveller106;
using VisionDesigner;
//using VM.PlatformSDKCS;


//using VM.PlatformSDKCS;

//using VM.PlatformSDKCS;
using WindowsFormsApp11;
using WorldOfMoveableObjects;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

//using TravellerMINIX6.BasicSpace.ReadBarcode;

namespace TravellerMINIX6.OPSpace
{
    public class RegionCellClass
    {

        CommonLogClass logInfo
        {
            get { return Universal.COMMON_LOG_INFOS; }
        }

        public int Index = 0;
        public string Name = "";
        public string Result = "";
        public string lblName = "";
        public int CellRow = 0;
        public int CellCol = 0;
        public RectangleF viewRectF = new RectangleF();

        public int CropValue = 33;
        public int SampleValue = 7;
        public ProcessImageMode m_ProcessImageMode = ProcessImageMode.V2;

        public int PreThresholdValue { get; set; } = 50;
        public int PreThresholdValueMark2 { get; set; } = 50;

        public bool PreDir { get; set; } = false;
        public int PreCenterExtendx { get; set; } = 5;
        public int PreCenterExtendy { get; set; } = 5;
        public float PreLineLeastDistance { get; set; } = 0.5f;
        public bool PreIsOpenAuFind { get; set; } = false;
        public bool IsByPass { get; set; } = false;
        public int PreMarkMinArea { get; set; } = 500;
        public bool IsByPassFromManual { get; set; } = false;
        //public LaserDotCoordinate laserDotCoordinate { get; set; } = new LaserDotCoordinate();

        //public PointF k0 { get; set; } = new PointF(0, 0);
        //public PointF k1 { get; set; } = new PointF(10, 10);

        [Browsable(true)]
        public RectangleF Mark0 { get; set; } = new RectangleF(0, 0, 100, 100);
        [Browsable(true)]
        public RectangleF Mark1 { get; set; } = new RectangleF(300, 300, 100, 100);

        [Browsable(true)]
        public RectangleF MarkLine { get; set; } = new RectangleF(300, 300, 100, 100);

        [Browsable(true)]
        public RectangleF MarkLine1 { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(true)]
        public RectangleF MarkLine2 { get; set; } = new RectangleF(300, 300, 100, 100);

        public AUFindClass myFind = new AUFindClass();
        public FindParaClass paraClass = new FindParaClass();

        public AUFindClass myFind_dir = new AUFindClass();
        public FindParaClass paraClass_dir = new FindParaClass();

        public void Dispose()
        {
            if (myFind != null)
                myFind.Dispose();
            if (myFind_dir != null)
                myFind_dir.Dispose();

            //myFind = null;
            //myFind_dir = null;
        }

        //public XinliFindLineClass SideLeftLine = new XinliFindLineClass();
        #region 四边的参数

        [Browsable(false)]
        public RectangleF SideLeft { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF SideRight { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF SideTop { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF SideBottom { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF DirRectF { get; set; } = new RectangleF(0, 0, 100, 100);

        const string LSCat5 = "A05.四周找边设定";

        //public LaserOffsetStart fourStartPosition { get; set; } = LaserOffsetStart.Center;

        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.左边从左往右")]
        [Browsable(true)]
        public bool dir_left { get; set; } = true;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.左边从白到黑")]
        [Browsable(true)]
        public bool dir_left_black { get; set; } = false;


        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.右边从左往右")]
        [Browsable(true)]
        public bool dir_right { get; set; } = false;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.右边从白到黑")]
        [Browsable(true)]
        public bool dir_right_black { get; set; } = false;


        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.上边从上往下")]
        [Browsable(true)]
        public bool dir_top { get; set; } = true;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.上边从白到黑")]
        [Browsable(true)]
        public bool dir_top_black { get; set; } = false;

        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.下边从上往下")]
        [Browsable(true)]
        public bool dir_bottom { get; set; } = false;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.下边从白到黑")]
        [Browsable(true)]
        public bool dir_bottom_black { get; set; } = false;

        #endregion


        #region 检测方向设定

        const string LSCat6 = "A06.检测方向设定";

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.开启判断方向")]
        [Browsable(true)]
        public bool inspect_dir_open { get; set; } = false;

        //[CategoryAttribute(LSCat6), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("A02.相似度")]
        //[Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        //public float inspect_dir_tolerance { get; set; } = 0.8f;

        //[CategoryAttribute(LSCat6), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("A03.角度")]
        //[Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 180, 0.1f, 2)]
        //public float inspect_dir_angle { get; set; } = 15;

        //[CategoryAttribute(LSCat6), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("A04.采样值")]
        //[Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 300)]
        //public int inspect_dir_samplesize { get; set; } = 15;

        #endregion

        public FunTool funTool { get; set; } = FunTool.AUFIND;

        public void SetTrainImage(Bitmap bmpInput)
        {
            if (!PreIsOpenAuFind)
                return;

            //RectangleF rect = new RectangleF(0, 0, bmpInput.Width, bmpInput.Height);
            ////取出Train的小图
            //Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //myFind.bmpItem = bmpTrain;

            myFind.bmpItem = new Bitmap(bmpInput);
        }

        public bool Train()
        {
            if (!PreIsOpenAuFind)
                return true;

            RectangleF rect = new RectangleF(0, 0, myFind.bmpItem.Width, myFind.bmpItem.Height);
            //取出Train的小图
            //Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //Bitmap bmpTrain=new Bitmap(bmpInput);
            //myFind.bmpItem = bmpTrain;

            switch (Universal.FACTORYNAME)
            {
                case FactoryName.DONGGUAN:
                case FactoryName.DAGUI:
                    switch (m_ProcessImageMode)
                    {
                        case ProcessImageMode.V3:
                            myFind.myFunTool = FunTool.Gray; // funTool;
                            break;
                        default:
                            myFind.myFunTool = FunTool.HP; // funTool;
                            break;
                    }
                    break;
                default:
                    myFind.myFunTool = FunTool.AUFIND;
                    break;
            }
            bool bOK = myFind.Train2(rect, paraClass);

            //bmpTrain.Dispose();
            myFind.bmpItem.Dispose();
            return bOK;
        }

        public void SetTrainDirImage(Bitmap bmpInput)
        {
            if (!inspect_dir_open)
                return;

            //RectangleF rect = new RectangleF(0, 0, bmpInput.Width, bmpInput.Height);
            ////取出Train的小图
            //Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //myFind_dir.bmpItem = bmpTrain;

            myFind_dir.bmpItem = new Bitmap(bmpInput);
        }
        public bool TrainDir()
        {
            if (!inspect_dir_open)
                return true;

            RectangleF rect = new RectangleF(0, 0, myFind_dir.bmpItem.Width, myFind_dir.bmpItem.Height);
            //取出Train的小图
            //Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //Bitmap bmpTrain = new Bitmap(bmpInput);
            //myFind_dir.bmpItem = bmpTrain;

            switch (Universal.FACTORYNAME)
            {
                case FactoryName.DONGGUAN:
                case FactoryName.DAGUI:
                    myFind_dir.myFunTool = FunTool.HP;
                    break;
            }
            bool bOK = myFind_dir.Train2(rect, paraClass_dir);

            myFind_dir.bmpItem.Dispose();
            return bOK;
        }

        public Bitmap BmpRun = new Bitmap(1, 1);
        public RectangleF RealRectangleF = new RectangleF();

        /// <summary>
        /// 左上
        /// </summary>
        public PointF xP1 = new PointF();
        /// <summary>
        /// 右上
        /// </summary>
        public PointF xP2 = new PointF();
        /// <summary>
        /// 右下
        /// </summary>
        public PointF xP3 = new PointF();
        /// <summary>
        /// 左下
        /// </summary>
        public PointF xP4 = new PointF();
        /// <summary>
        /// 中心
        /// </summary>
        public PointF xPCenter = new PointF();

        /// <summary>
        /// 矩形中心世界坐标
        /// </summary>
        public PointF xPCenterWorld = new PointF();
        /// <summary>
        /// 矩形左上世界坐标
        /// </summary>
        public PointF xP1World = new PointF();
        /// <summary>
        /// 矩形右上世界坐标
        /// </summary>
        public PointF xP2World = new PointF();
        /// <summary>
        /// 矩形右下世界坐标
        /// </summary>
        public PointF xP3World = new PointF();
        /// <summary>
        /// 矩形左下世界坐标
        /// </summary>
        public PointF xP4World = new PointF();

        /// <summary>
        /// mark2矩形相对于固定点的view点
        /// </summary>
        public PointF xPMark2Offset = new PointF();
        /// <summary>
        /// mark2矩形相对于固定点的world点
        /// </summary>
        public PointF xPMark2OffsetWorld = new PointF();

        /// <summary>
        /// mark十字相对于固定点的view点
        /// </summary>
        public PointF xPCenterOffset = new PointF();
        /// <summary>
        /// mark十字相对于固定点的world点
        /// </summary>
        public PointF xPCenterOffsetWorld = new PointF();


        /// <summary>
        /// mark十字给laser的view点
        /// </summary>
        public PointF xPCenterLaserOffset = new PointF();
        /// <summary>
        /// mark十字给laser的world点
        /// </summary>
        public PointF xPCenterLaserOffsetWorld = new PointF();

        //public PointF xPLineFirst = new PointF();
        public PointF xPSecond = new PointF();

        //public PointF xPLineFirstw = new PointF();
        public PointF xPSecondw = new PointF();

        /// <summary>
        /// 相对于固定点的view点
        /// </summary>
        public PointF xPSecondOffset = new PointF();
        /// <summary>
        /// 相对于固定点的world点
        /// </summary>
        public PointF xPSecondOffsetWorld = new PointF();

        RectangleF m_AlignRectF = new RectangleF();

        public double Angle(double offset = 0)
        {
            //get

            double angleOfLine = 0;
            if (xP2World.X > xP1World.X)
                angleOfLine = Math.Atan2((xP2World.Y - xP1World.Y), (xP2World.X - xP1World.X)) * 180 / Math.PI;
            else
                angleOfLine = Math.Atan2((xP1World.Y - xP2World.Y), (xP1World.X - xP2World.X)) * 180 / Math.PI;
            return angleOfLine - offset;

        }
        public double AngleRun(double offset = 0)
        {
            //get

            double angleOfLine = 0;
            if (xP2World.X > xP1World.X)
                angleOfLine = Math.Atan2((xP2World.Y - xP1World.Y), (xP2World.X - xP1World.X)) * 180 / Math.PI;
            else
                angleOfLine = Math.Atan2((xP1World.Y - xP2World.Y), (xP1World.X - xP2World.X)) * 180 / Math.PI;
            return angleOfLine - offset;

        }
        public int nType = 1;
        public string nDesc = string.Empty;

        private string m_DebugPath = Universal.DEBUGRESULTPATH + "\\" + JzTimes.DateTimeSerialString + "";
        public string DebugPath
        {
            get { return m_DebugPath; }
            set { m_DebugPath = value; }
        }

        private bool m_IsSaveDebugPicture = false;
        public bool IsSaveDebugPicture
        {
            get { return m_IsSaveDebugPicture; }
            set { m_IsSaveDebugPicture = value; }
        }
        public void Run()
        {
            Result = "";
            switch (m_ProcessImageMode)
            {
                case ProcessImageMode.V7:
                    readBarcodeFuntionV7(BmpRun);
                    break;
                case ProcessImageMode.V6:
                    readBarcodeFuntionV6(BmpRun);
                    break;
                case ProcessImageMode.V5:
                    readBarcodeFuntionV5(BmpRun);
                    break;
                default:
                    readBarcodeFuntion(BmpRun);
                    break;
            }

        }

        private void readBarcodeFuntion(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            if (IsByPass || IsByPassFromManual)
            {
                nDesc = ToChangeLanguage("不检测");
                nType = 0;
                return;
            }

            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            bool _isdrawresult = m_IsSaveDebugPicture;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = preProcessImage(bmpSized);

            //定位偏移距离
            PointF _runOffset = new PointF(0, 0);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2图片预处理{myElapsedTime} ms");

            switch (m_ProcessImageMode)
            {
                case ProcessImageMode.V3:
                case ProcessImageMode.V11:

                    if (PreIsOpenAuFind)
                    {
                        myFind.bmpFind = bmpSizedPre;
                        bool bOK = myFind.FindLarge();

                        if (bOK)
                        {
                            if (myFind.xResults.Count > 0)
                            {
                                if (myFind.xResults[0].fScore < paraClass.tolerance)
                                {
                                    nDesc = $"{ToChangeLanguage("定位失败分数")}{myFind.xResults[0].fScore}";
                                    nType = 0;

                                    bmpSizedPre.Dispose();
                                    bmpSized.Dispose();
                                    return;
                                }

                                //nDesc = $"{ToChangeLanguage("定位成功分数")}{myFind.xResults[0].fScore.ToString("0.000")}#";
                                //nType = 0;

                                if (myFind.xResults.Count > 0)
                                {
                                    _runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                                }

                                if (myFind.xResults.Count > 0)
                                {
                                    _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - bmpInput.Width / 2,
                                                                           myFind.xResults[0].fCenterY - bmpInput.Height / 2);
                                }

                                switch (m_ProcessImageMode)
                                {
                                    case ProcessImageMode.V11:

                                        m_AlignRectF = SimpleRectF(_runBmpCenter, viewRectF.Width / 2, viewRectF.Height / 2);
                                        BoundRect(ref m_AlignRectF, bmpSized.Size);
                                        Bitmap bmpAlignV11 = (Bitmap)bmpSized.Clone(m_AlignRectF, PixelFormat.Format24bppRgb);

                                        bmpSizedPre.Dispose();
                                        bmpSizedPre = preProcessImage(bmpAlignV11);

                                        bmpAlignV11.Dispose();

                                        break;
                                    case ProcessImageMode.V3:

                                        //定位完成切出定位后的小图 并处理

                                        m_AlignRectF = SimpleRectF(_runBmpCenter, viewRectF.Width / 2, viewRectF.Height / 2);
                                        BoundRect(ref m_AlignRectF, bmpSized.Size);
                                        Bitmap bmpAlign = (Bitmap)bmpSized.Clone(m_AlignRectF, PixelFormat.Format24bppRgb);

                                        bmpSizedPre.Dispose();
                                        bmpSizedPre = preProcessImage(bmpAlign);

                                        bmpAlign.Dispose();

                                        #region 判断中间全部是亮面

                                        // CreateInstance

                                        VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

                                        // Set input image

                                        //VisionDesigner.CMvdImage cInputImg = new CMvdImage();

                                        RectangleF rectblob = SimpleRectF(new PointF(bmpSizedPre.Width / 2, bmpSizedPre.Height / 2), 50, 50);
                                        Bitmap bmpblob = bmpSizedPre.Clone(rectblob, PixelFormat.Format8bppIndexed);

                                        VisionDesigner.CMvdImage cInputImg2 = BitmapToCMvdImage(bmpblob);
                                        //Bitmap bmp24 = new Bitmap(1, 1);
                                        //if (bmpFind.PixelFormat != PixelFormat.Format8bppIndexed)
                                        //{
                                        //    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                                        //    bmp24 = grayscale.Apply(bmpFind);
                                        //    cInputImg2 = BitmapToCMvdImage(bmp24);
                                        //}
                                        // Set input image
                                        //Bitmap bmp24 = bmpinput.Clone(new Rectangle(0, 0, bmpinput.Width, bmpinput.Height), PixelFormat.Format8bppIndexed);

                                        if (cInputImg2.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                                        {
                                            //当前程序仅支持mono8。因此像素格会转换.
                                            cInputImg2.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                                        }

                                        //cInputImg.InitImage("InputTest.bmp");

                                        cBlobFindToolObj.InputImage = cInputImg2;

                                        // Set ROI region (optional)

                                        cBlobFindToolObj.ROI = new VisionDesigner.CMvdRectangleF(cInputImg2.Width / 2, cInputImg2.Height / 2, cInputImg2.Width, cInputImg2.Height);

                                        cBlobFindToolObj.SetRunParam("Polarity", "BrightObject");

                                        // Running

                                        cBlobFindToolObj.Run();

                                        // Get the result

                                        VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = cBlobFindToolObj.Result;

                                        //Console.WriteLine("Blob Num: {0}", cBlobFindRes.BlobInfo.Count);

                                        //foreach (var item in m_stBlobFindToolObj.Result.BlobInfo)

                                        //{

                                        //    Console.WriteLine("Index: {0}, Angle {1}", item.DomainIndex, item.BoxInfo.Angle);

                                        //    // …More information

                                        //}

                                        if (cBlobFindRes.BlobInfo.Count > 0)
                                        {
                                            VisionDesigner.BlobFind.CBlobInfo cBlob = cBlobFindRes.BlobInfo[0];
                                            double arearatio = cBlob.Area / (cInputImg2.Width * cInputImg2.Height);
                                            if (arearatio < 0.8)
                                            {
                                                nDesc = ToChangeLanguage("定位失败");
                                                nType = 0;

                                                bmpSizedPre.Dispose();
                                                bmpSized.Dispose();

                                                cBlobFindToolObj.Dispose();
                                                cBlobFindToolObj = null;
                                                return;
                                            }
                                        }
                                        else
                                        {

                                            nDesc = ToChangeLanguage("定位失败");
                                            nType = 0;

                                            bmpSizedPre.Dispose();
                                            bmpSized.Dispose();

                                            cBlobFindToolObj.Dispose();
                                            cBlobFindToolObj = null;
                                            return;

                                        }

                                        cBlobFindToolObj.Dispose();
                                        cBlobFindToolObj = null;
                                        #endregion




                                        //myFind.bmpFind = bmpSizedPre;
                                        //bOK = myFind.FindLarge();
                                        //#region 再次判断

                                        //if (bOK)
                                        //{
                                        //    if (myFind.xResults.Count > 0)
                                        //    {
                                        //        if (myFind.xResults[0].fScore < paraClass.tolerance)
                                        //        {
                                        //            nDesc = $"{ToChangeLanguage("定位失败,分数")}{myFind.xResults[0].fScore}";
                                        //            nType = 0;

                                        //            bmpSizedPre.Dispose();
                                        //            bmpSized.Dispose();
                                        //            return;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        nDesc = ToChangeLanguage("定位失败");
                                        //        nType = 0;

                                        //        bmpSizedPre.Dispose();
                                        //        bmpSized.Dispose();
                                        //        return;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    nDesc = ToChangeLanguage("定位失败");
                                        //    nType = 0;

                                        //    bmpSizedPre.Dispose();
                                        //    bmpSized.Dispose();
                                        //    return;
                                        //}

                                        //#endregion


                                        break;
                                }

                                //_runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                            }
                            else
                            {
                                nDesc = ToChangeLanguage("定位失败");
                                nType = 0;

                                bmpSizedPre.Dispose();
                                bmpSized.Dispose();
                                return;
                            }


                            //if (_runOffset.X > 5 || _runOffset.Y > 5)
                            //{
                            //    int xfuck = 0;
                            //}
                        }
                        else
                        {
                            nDesc = ToChangeLanguage("定位失败");
                            nType = 0;

                            bmpSizedPre.Dispose();
                            bmpSized.Dispose();
                            return;
                        }
                    }

                    break;
            }


            if (inspect_dir_open)
            {

                switch (m_ProcessImageMode)
                {
                    case ProcessImageMode.V11:
                    case ProcessImageMode.V3:

                        //_runOffset = new PointF(0, 0);

                        break;
                    default:
                        break;

                }

                Rectangle dirrect =
                      new Rectangle((int)(DirRectF.X + _runOffset.X),
                    (int)(DirRectF.Y + _runOffset.Y),
                   (int)DirRectF.Width,
                    (int)DirRectF.Height);

                BoundRect(ref dirrect, bmpInput.Size);
                Bitmap bmpdir = (Bitmap)bmpInput.Clone(dirrect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                if (m_IsSaveDebugPicture)
                {
                    bmpdir.Save(DebugPath + "\\dir_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                myFind_dir.bmpFind = bmpdir;
                bool bOK = myFind_dir.FindLarge();
                bmpdir.Dispose();
                if (bOK)
                {
                    if (myFind_dir.xResults.Count > 0)
                    {
                        if (myFind_dir.xResults[0].fScore < paraClass_dir.tolerance)
                        {
                            nDesc = $"{ToChangeLanguage("方向定位失败分数")}{myFind_dir.xResults[0].fScore}";
                            nType = 0;

                            bmpSizedPre.Dispose();
                            bmpSized.Dispose();
                            return;
                        }

                        //nDesc += $"{ToChangeLanguage("定位成功分数")}{myFind_dir.xResults[0].fScore.ToString("0.000")}#";
                        //nType = 0;
                        //_runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                    }
                    else
                    {
                        nDesc = ToChangeLanguage("方向定位失败");
                        nType = 0;

                        bmpSizedPre.Dispose();
                        bmpSized.Dispose();
                        return;
                    }

                    //if (myFind.xResults.Count > 0)
                    //{
                    //    _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                    //                                           myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    //}
                }
                else
                {
                    nDesc = ToChangeLanguage("方向定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2定位产品{myElapsedTime} ms");

            List<PointF> pts = new List<PointF>();

            switch (m_ProcessImageMode)
            {
                case ProcessImageMode.V2:

                    bool bOK = false;
                    //mark0
                    RectangleF runmark0 = new RectangleF(Mark0.X + _runOffset.X,
                       Mark0.Y + _runOffset.Y,
                       Mark0.Width,
                       Mark0.Height);

                    runmark0.Inflate(PreCenterExtendx, PreCenterExtendy);

                    calMarkBlob(bmpSizedPre, runmark0, PreMarkMinArea, out bOK, PreThresholdValue);
                    if (!bOK)
                    {
                        nDesc = ToChangeLanguage("Mark定位失败");
                        nType = 0;

                        bmpSizedPre.Dispose();
                        bmpSized.Dispose();
                        return;
                    }

                    pts = getCalImageRectLineFourEx001(bmpSizedPre);
                    break;
                case ProcessImageMode.V11:
                    _runOffset = new PointF(0, 0);
                    pts = getCalImageRectLineFourEx001_VM2(bmpSizedPre, _runOffset, m_AlignRectF);
                    //pts = getCalImageRectLineFourEx001_VM(bmpSizedPre, _runOffset);
                    break;
                case ProcessImageMode.V3:
                    pts = getCalImageRectLineVM2(bmpSizedPre, new Point(bmpSizedPre.Width / 2, bmpSizedPre.Height / 2), m_AlignRectF);
                    break;
                case ProcessImageMode.V8://接入shell的算法

                    float[] lineAngle;
                    PointF center;

                    FindRectByVM findRectByVM = new FindRectByVM();
                    PointF[] linsInfo = findRectByVM.RunFindEdgeTool(bmpSizedPre, out lineAngle, out center);

                    if (linsInfo == null || lineAngle == null)
                    {
                        nDesc = ToChangeLanguage("找边失败");
                        nType = 0;

                        bmpSizedPre.Dispose();
                        bmpSized.Dispose();
                        return;
                    }
                    //下边
                    LineClass line1 = new LineClass(linsInfo[0], linsInfo[1]);
                    //左边
                    LineClass line2 = new LineClass(linsInfo[2], linsInfo[3]);
                    //上边
                    LineClass line3 = new LineClass(linsInfo[4], linsInfo[5]);
                    //右边
                    LineClass line4 = new LineClass(linsInfo[6], linsInfo[7]);

                    PointF p1 = line3.FindIntersection(line2);//左上
                    PointF p2 = line3.FindIntersection(line4);//右上

                    PointF p4 = line1.FindIntersection(line2);//左下
                    PointF p3 = line1.FindIntersection(line4);//右下

                    PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

                    xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
                    xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
                    xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
                    xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
                    xPCenter = new PointF(tempptf.X + center.X, tempptf.Y + center.Y);

                    pts.Clear();

                    pts.Add(center);
                    pts.Add(p1);
                    pts.Add(p2);
                    pts.Add(p3);
                    pts.Add(p4);

                    break;
                default:
                    pts = getCalImageRectLineVM(bmpSizedPre, _runBmpCenter);
                    break;
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算测试时间{myElapsedTime} ms");

            if (pts.Count == 5)
            {
                nDesc = ToChangeLanguage("成功");
                nType = 1;

                watch.Stop();

                if (_isdrawresult)
                {
                    int _sample = SampleValue;
                    int _cropValue = CropValue * 2;

                    Bitmap bmpOrgDraw = new Bitmap(1, 1);
                    Bitmap bmp = new Bitmap(1, 1);
                    if (_isdrawresult)
                    {
                        bmpOrgDraw = (Bitmap)bmpInput.Clone();// new Bitmap(bmpInput);
                        bmp = (Bitmap)bmpSizedPre.Clone();// new Bitmap(bmpSizedPre);

                        myElapsedTime = jzTimesDurRecord.msDuriation;
                        jzTimesDurRecord.Cut();
                        logInfo.Log($"x2建图{myElapsedTime} ms");
                    }
                    Graphics g = Graphics.FromImage(bmpOrgDraw);

                    Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
                    g.DrawRectangle(new Pen(Color.Lime, 3), rLevel);

                    g.DrawLine(new Pen(Color.Red, 3), pts[1], pts[2]);
                    g.DrawLine(new Pen(Color.Red, 3), pts[1], pts[4]);
                    g.DrawLine(new Pen(Color.Red, 3), pts[2], pts[3]);
                    g.DrawLine(new Pen(Color.Red, 3), pts[3], pts[4]);

                    g.DrawString($"P1", new Font("宋体", 18), Brushes.Red, pts[1]);
                    g.DrawString($"P2", new Font("宋体", 18), Brushes.Red, pts[2]);
                    g.DrawString($"P3", new Font("宋体", 18), Brushes.Red, pts[3]);
                    g.DrawString($"P4", new Font("宋体", 18), Brushes.Red, pts[4]);

                    //交叉线
                    g.DrawLine(new Pen(Color.Red, 3), pts[1], pts[3]);
                    g.DrawLine(new Pen(Color.Red, 3), pts[2], pts[4]);

                    RectangleF rectangleF = SimpleRectF(pts[0], 2, 2);
                    if (float.IsNaN(rectangleF.X) || float.IsNaN(rectangleF.Y))
                    {

                    }
                    else
                        g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleF });
                    rLevel = SimpleRect(_runBmpCenter, _cropValue / 2, bmpInput.Height / 2 - 1);
                    g.DrawRectangle(new Pen(Color.Lime, 3), rLevel);
                    g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", new Font("宋体", 18), Brushes.Red, new PointF(18, 38));
                    g.Dispose();

                    if (m_IsSaveDebugPicture)
                    {
                        bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }

                    bmp.Dispose();
                    bmpOrgDraw.Dispose();
                }
            }
            else
            {
                nDesc = ToChangeLanguage("计算失败");
                nType = 0;
            }

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2结果画线{myElapsedTime} ms");

            bmpSizedPre.Dispose();
            bmpSized.Dispose();

        }

        private void readBarcodeFuntionV5(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            if (IsByPass || IsByPassFromManual)
            {
                nDesc = ToChangeLanguage("不检测");
                nType = 0;
                return;
            }

            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            bool _isdrawresult = m_IsSaveDebugPicture;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            //定位偏移距离
            PointF _runOffset = new PointF(0, 0);

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = new Bitmap(bmpSized);// preProcessImage(bmpSized);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2图片预处理{myElapsedTime} ms");
            #endregion
            bool bOK = false;

            if (PreIsOpenAuFind)
            {
                myFind.bmpFind = bmpSizedPre;
                bOK = myFind.FindLarge();

                if (bOK)
                {
                    if (myFind.xResults.Count > 0)
                    {
                        _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                                                               myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    }
                    //return;
                }
                else
                {
                    nDesc = ToChangeLanguage("定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
            }
            if (inspect_dir_open)
            {

                Rectangle dirrect =
                      new Rectangle((int)(DirRectF.X + _runOffset.X),
                    (int)(DirRectF.Y + _runOffset.Y),
                   (int)DirRectF.Width,
                    (int)DirRectF.Height);
                BoundRect(ref dirrect, bmpInput.Size);
                Bitmap bmpdir = (Bitmap)bmpInput.Clone(dirrect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                myFind_dir.bmpFind = bmpdir;
                bOK = myFind_dir.FindLarge();
                bmpdir.Dispose();
                if (bOK)
                {
                    if (myFind_dir.xResults.Count > 0)
                    {
                        if (myFind_dir.xResults[0].fScore < paraClass_dir.tolerance)
                        {
                            nDesc = $"{ToChangeLanguage("方向定位失败分数")}{myFind_dir.xResults[0].fScore}";
                            nType = 0;

                            bmpSizedPre.Dispose();
                            bmpSized.Dispose();
                            return;
                        }
                        //_runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                    }
                    else
                    {
                        nDesc = ToChangeLanguage("方向定位失败");
                        nType = 0;

                        bmpSizedPre.Dispose();
                        bmpSized.Dispose();
                        return;
                    }

                    //if (myFind.xResults.Count > 0)
                    //{
                    //    _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                    //                                           myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    //}
                }
                else
                {
                    nDesc = ToChangeLanguage("方向定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2定位产品{myElapsedTime} ms");

            List<PointF> pts = new List<PointF>();
            RectangleF runmark0 = new RectangleF();
            RectangleF runmark1 = new RectangleF();
            PointF p1x = new PointF();
            PointF p2x = new PointF();
            PointF pSecond = new PointF();

            RectangleF runmarkline = new RectangleF();

            switch (m_ProcessImageMode)
            {
                //case ProcessImageMode.V2:
                //    pts = getCalImageRectLineFour(bmpSizedPre);
                //    break;
                case ProcessImageMode.V5:

                    bOK = false;
                    //mark0
                    runmark0 = new RectangleF(Mark0.X + _runOffset.X,
                       Mark0.Y + _runOffset.Y,
                       Mark0.Width,
                       Mark0.Height);

                    runmark0.Inflate(PreCenterExtendx, PreCenterExtendy);

                    p1x = calMarkBlob(bmpSizedPre, runmark0, PreMarkMinArea, out bOK, PreThresholdValue);
                    xP1 = new PointF(p1x.X + viewRectF.X, p1x.Y + viewRectF.Y);
                    xPCenter = new PointF(xP1.X, xP1.Y);
                    xP3 = new PointF(xP1.X, xP1.Y);

                    if (bOK)
                    {
                        //mark1
                        runmark1 = new RectangleF(Mark1.X + _runOffset.X,
                          Mark1.Y + _runOffset.Y,
                          Mark1.Width,
                          Mark1.Height);

                        runmark1.Inflate(PreCenterExtendx, PreCenterExtendy);

                        p2x = calMarkBlob(bmpSizedPre, runmark1, PreMarkMinArea, out bOK, PreThresholdValue);
                        xP2 = new PointF(p2x.X + viewRectF.X, p2x.Y + viewRectF.Y);
                        xP4 = new PointF(xP2.X, xP2.Y);
                    }

                    if (bOK)
                    {

                        runmarkline = new RectangleF(MarkLine.X + _runOffset.X,
                          MarkLine.Y + _runOffset.Y,
                          MarkLine.Width,
                          MarkLine.Height);

                        #region 找直线

                        Bitmap bmpinput0 = bmpSizedPre.Clone(runmarkline, PixelFormat.Format24bppRgb);

                        AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                        bmpinput0 = grayscale.Apply(bmpinput0);

                        AForge.Imaging.Filters.Threshold threshold = new Threshold(PreThresholdValue);
                        bmpinput0 = threshold.Apply(bmpinput0);

                        AForge.Imaging.Filters.Invert invert = new Invert();
                        bmpinput0 = invert.Apply(bmpinput0);

                        AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                        fillHoles2.MaxHoleHeight = 100;
                        fillHoles2.MaxHoleWidth = 100;
                        fillHoles2.CoupledSizeFiltering = false;
                        bmpinput0 = fillHoles2.Apply(bmpinput0);

                        LineClass lineClass = getVericalLine(bmpinput0, true);
                        bmpinput0.Dispose();


                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += runmarkline.X;
                        p1.Y += runmarkline.Y;

                        p2.X += runmarkline.X;
                        p2.Y += runmarkline.Y;

                        //xPLineFirst = new PointF(p1.X, p1.Y);
                        //xPSecond = new PointF(p2.X, p2.Y);

                        double angleOfLine = 0;
                        if (p2.X > p1.X)
                            angleOfLine = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
                        else
                            angleOfLine = Math.Atan2((p1.Y - p2.Y), (p1.X - p2.X)) * 180 / Math.PI;

                        double k = Math.Tan(angleOfLine * Math.PI / 180);
                        double b = p1x.Y - k * p1x.X;

                        pSecond = new PointF(p2x.X, (float)(k * p2x.X + b));
                        xPSecond = new PointF(pSecond.X + viewRectF.X, pSecond.Y + viewRectF.Y);


                        #endregion
                    }

                    break;
                default:
                    pts = getCalImageRectLine(bmpSizedPre);
                    break;
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算测试时间{myElapsedTime} ms");

            if (!bOK)
            {
                nDesc = ToChangeLanguage("Mark定位失败");
                nType = 0;

                bmpSizedPre.Dispose();
                bmpSized.Dispose();
                return;
            }

            nDesc = ToChangeLanguage("成功");
            nType = 1;

            watch.Stop();

            if (_isdrawresult)
            {
                int _sample = SampleValue;
                int _cropValue = CropValue * 2;

                Bitmap bmpOrgDraw = new Bitmap(1, 1);
                //Bitmap bmp = new Bitmap(1, 1);
                if (_isdrawresult)
                {
                    bmpOrgDraw = (Bitmap)bmpInput.Clone();// new Bitmap(bmpInput);
                    //bmp = (Bitmap)bmpSizedPre.Clone();// new Bitmap(bmpSizedPre);

                    myElapsedTime = jzTimesDurRecord.msDuriation;
                    jzTimesDurRecord.Cut();
                    logInfo.Log($"x2建图{myElapsedTime} ms");
                }
                Graphics g = Graphics.FromImage(bmpOrgDraw);
                RectangleF rectangleFmark0 = SimpleRectF(p1x, 2, 2);
                RectangleF rectangleFmark1 = SimpleRectF(p2x, 2, 2);
                //RectangleF rectangleFmark1 = SimpleRectF(p2x, 2, 2);

                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0, rectangleFmark1 });
                g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { runmark0, runmark1 });

                g.DrawLine(new Pen(Color.Lime, 3), p1x, pSecond);
                g.DrawLine(new Pen(Color.Lime, 3), p2x, pSecond);

                g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", new Font("宋体", 18), Brushes.Red, new PointF(18, 38));
                g.Dispose();

                if (m_IsSaveDebugPicture)
                {
                    //bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                //bmp.Dispose();
                bmpOrgDraw.Dispose();
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2结果画Mark点{myElapsedTime} ms");

            bmpSizedPre.Dispose();
            bmpSized.Dispose();

        }
        private void readBarcodeFuntionV6(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            if (IsByPass || IsByPassFromManual)
            {
                nDesc = ToChangeLanguage("不检测");
                nType = 0;
                return;
            }

            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            bool _isdrawresult = m_IsSaveDebugPicture;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            //定位偏移距离
            PointF _runOffset = new PointF(0, 0);

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = new Bitmap(bmpSized);// preProcessImage(bmpSized);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2图片预处理{myElapsedTime} ms");
            #endregion
            bool bOK = false;

            if (PreIsOpenAuFind)
            {
                myFind.bmpFind = bmpSizedPre;
                bOK = myFind.FindLarge();

                if (bOK)
                {
                    if (myFind.xResults.Count > 0)
                    {
                        _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                                                               myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    }
                    //return;
                }
                else
                {
                    nDesc = ToChangeLanguage("定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
            }
            if (inspect_dir_open)
            {

                Rectangle dirrect =
                      new Rectangle((int)(DirRectF.X + _runOffset.X),
                    (int)(DirRectF.Y + _runOffset.Y),
                   (int)DirRectF.Width,
                    (int)DirRectF.Height);
                BoundRect(ref dirrect, bmpInput.Size);
                Bitmap bmpdir = (Bitmap)bmpInput.Clone(dirrect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                myFind_dir.bmpFind = bmpdir;
                bOK = myFind_dir.FindLarge();
                bmpdir.Dispose();
                if (bOK)
                {
                    if (myFind_dir.xResults.Count > 0)
                    {
                        if (myFind_dir.xResults[0].fScore < paraClass_dir.tolerance)
                        {
                            nDesc = $"{ToChangeLanguage("方向定位失败分数")}{myFind_dir.xResults[0].fScore}";
                            nType = 0;

                            bmpSizedPre.Dispose();
                            bmpSized.Dispose();
                            return;
                        }
                        //_runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                    }
                    else
                    {
                        nDesc = ToChangeLanguage("方向定位失败");
                        nType = 0;

                        bmpSizedPre.Dispose();
                        bmpSized.Dispose();
                        return;
                    }

                    //if (myFind.xResults.Count > 0)
                    //{
                    //    _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                    //                                           myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    //}
                }
                else
                {
                    nDesc = ToChangeLanguage("方向定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2定位产品{myElapsedTime} ms");

            List<PointF> pts = new List<PointF>();
            RectangleF runmark0 = new RectangleF();
            RectangleF runmark1 = new RectangleF();
            PointF p1x = new PointF();
            PointF p2x = new PointF();
            PointF pSecond = new PointF();

            RectangleF runmarkline = new RectangleF();

            RectangleF runmarkline1 = new RectangleF();
            RectangleF runmarkline2 = new RectangleF();
            LineClass fitline = null;

            switch (m_ProcessImageMode)
            {
                //case ProcessImageMode.V2:
                //    pts = getCalImageRectLineFour(bmpSizedPre);
                //    break;
                case ProcessImageMode.V5:

                    bOK = false;
                    //mark0
                    runmark0 = new RectangleF(Mark0.X + _runOffset.X,
                       Mark0.Y + _runOffset.Y,
                       Mark0.Width,
                       Mark0.Height);

                    runmark0.Inflate(PreCenterExtendx, PreCenterExtendy);

                    p1x = calMarkBlob(bmpSizedPre, runmark0, PreMarkMinArea, out bOK, PreThresholdValue);
                    xP1 = new PointF(p1x.X + viewRectF.X, p1x.Y + viewRectF.Y);
                    xPCenter = new PointF(xP1.X, xP1.Y);
                    xP3 = new PointF(xP1.X, xP1.Y);

                    if (bOK)
                    {
                        //mark1
                        runmark1 = new RectangleF(Mark1.X + _runOffset.X,
                          Mark1.Y + _runOffset.Y,
                          Mark1.Width,
                          Mark1.Height);

                        runmark1.Inflate(PreCenterExtendx, PreCenterExtendy);

                        p2x = calMarkBlob(bmpSizedPre, runmark1, PreMarkMinArea, out bOK, PreThresholdValue);
                        xP2 = new PointF(p2x.X + viewRectF.X, p2x.Y + viewRectF.Y);
                        xP4 = new PointF(xP2.X, xP2.Y);
                    }

                    if (bOK)
                    {

                        runmarkline = new RectangleF(MarkLine.X + _runOffset.X,
                          MarkLine.Y + _runOffset.Y,
                          MarkLine.Width,
                          MarkLine.Height);

                        #region 找直线

                        Bitmap bmpinput0 = bmpSizedPre.Clone(runmarkline, PixelFormat.Format24bppRgb);

                        AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                        bmpinput0 = grayscale.Apply(bmpinput0);

                        AForge.Imaging.Filters.Threshold threshold = new Threshold(PreThresholdValue);
                        bmpinput0 = threshold.Apply(bmpinput0);

                        AForge.Imaging.Filters.Invert invert = new Invert();
                        bmpinput0 = invert.Apply(bmpinput0);

                        AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                        fillHoles2.MaxHoleHeight = 100;
                        fillHoles2.MaxHoleWidth = 100;
                        fillHoles2.CoupledSizeFiltering = false;
                        bmpinput0 = fillHoles2.Apply(bmpinput0);

                        LineClass lineClass = getVericalLine(bmpinput0, true);
                        bmpinput0.Dispose();


                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += runmarkline.X;
                        p1.Y += runmarkline.Y;

                        p2.X += runmarkline.X;
                        p2.Y += runmarkline.Y;

                        //xPLineFirst = new PointF(p1.X, p1.Y);
                        //xPSecond = new PointF(p2.X, p2.Y);

                        double angleOfLine = 0;
                        if (p2.X > p1.X)
                            angleOfLine = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
                        else
                            angleOfLine = Math.Atan2((p1.Y - p2.Y), (p1.X - p2.X)) * 180 / Math.PI;

                        double k = Math.Tan(angleOfLine * Math.PI / 180);
                        double b = p1x.Y - k * p1x.X;

                        pSecond = new PointF(p2x.X, (float)(k * p2x.X + b));
                        xPSecond = new PointF(pSecond.X + viewRectF.X, pSecond.Y + viewRectF.Y);


                        #endregion
                    }

                    break;
                case ProcessImageMode.V6:

                    bOK = false;
                    //mark0
                    runmark0 = new RectangleF(Mark0.X + _runOffset.X,
                       Mark0.Y + _runOffset.Y,
                       Mark0.Width,
                       Mark0.Height);

                    runmark0.Inflate(PreCenterExtendx, PreCenterExtendy);

                    p1x = calMarkBlob(bmpSizedPre, runmark0, PreMarkMinArea, out bOK, PreThresholdValue);
                    xP1 = new PointF(p1x.X + viewRectF.X, p1x.Y + viewRectF.Y);
                    xPCenter = new PointF(xP1.X, xP1.Y);
                    xP3 = new PointF(xP1.X, xP1.Y);

                    if (bOK)
                    {
                        //mark1
                        runmark1 = new RectangleF(Mark1.X + _runOffset.X,
                          Mark1.Y + _runOffset.Y,
                          Mark1.Width,
                          Mark1.Height);

                        runmark1.Inflate(PreCenterExtendx, PreCenterExtendy);

                        p2x = calMarkBlob(bmpSizedPre, runmark1, PreMarkMinArea, out bOK, PreThresholdValue);
                        xP2 = new PointF(p2x.X + viewRectF.X, p2x.Y + viewRectF.Y);
                        xP4 = new PointF(xP2.X, xP2.Y);
                    }

                    if (bOK)
                    {

                        runmarkline1 = new RectangleF(MarkLine1.X + _runOffset.X,
                          MarkLine1.Y + _runOffset.Y,
                          MarkLine1.Width,
                          MarkLine1.Height);

                        runmarkline2 = new RectangleF(MarkLine2.X + _runOffset.X,
                        MarkLine2.Y + _runOffset.Y,
                        MarkLine2.Width,
                        MarkLine2.Height);

                        #region 找直线

                        LineClass half1 = findLine(bmpSizedPre, runmarkline1);
                        LineClass half2 = findLine(bmpSizedPre, runmarkline2);

                        List<PointF> ptflines = new List<PointF>();
                        ptflines.Add(half1.FirstPt);
                        ptflines.Add(half1.SecondPt);
                        ptflines.Add(half2.FirstPt);
                        ptflines.Add(half2.SecondPt);

                        fitline = getLineForPointF(ptflines.ToArray(), false, 1);

                        PointF p1 = fitline.FirstPt;
                        PointF p2 = fitline.SecondPt;

                        double angleOfLine = 0;
                        if (p2.X > p1.X)
                            angleOfLine = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
                        else
                            angleOfLine = Math.Atan2((p1.Y - p2.Y), (p1.X - p2.X)) * 180 / Math.PI;

                        double k = Math.Tan(angleOfLine * Math.PI / 180);
                        double b = p1x.Y - k * p1x.X;

                        pSecond = new PointF(p2x.X, (float)(k * p2x.X + b));
                        xPSecond = new PointF(pSecond.X + viewRectF.X, pSecond.Y + viewRectF.Y);


                        #endregion
                    }

                    break;
                default:
                    pts = getCalImageRectLine(bmpSizedPre);
                    break;
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算测试时间{myElapsedTime} ms");

            if (!bOK)
            {
                nDesc = ToChangeLanguage("Mark定位失败");
                nType = 0;

                bmpSizedPre.Dispose();
                bmpSized.Dispose();
                return;
            }

            nDesc = ToChangeLanguage("成功");
            nType = 1;

            watch.Stop();

            if (_isdrawresult)
            {
                int _sample = SampleValue;
                int _cropValue = CropValue * 2;

                Bitmap bmpOrgDraw = new Bitmap(1, 1);
                //Bitmap bmp = new Bitmap(1, 1);
                if (_isdrawresult)
                {
                    bmpOrgDraw = (Bitmap)bmpInput.Clone();// new Bitmap(bmpInput);
                    //bmp = (Bitmap)bmpSizedPre.Clone();// new Bitmap(bmpSizedPre);

                    myElapsedTime = jzTimesDurRecord.msDuriation;
                    jzTimesDurRecord.Cut();
                    logInfo.Log($"x2建图{myElapsedTime} ms");
                }
                Graphics g = Graphics.FromImage(bmpOrgDraw);
                RectangleF rectangleFmark0 = SimpleRectF(p1x, 2, 2);
                RectangleF rectangleFmark1 = SimpleRectF(p2x, 2, 2);
                //RectangleF rectangleFmark1 = SimpleRectF(p2x, 2, 2);

                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0, rectangleFmark1 });
                g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { runmark0, runmark1, runmarkline1, runmarkline2 });

                g.DrawLine(new Pen(Color.Lime, 3), p1x, pSecond);
                g.DrawLine(new Pen(Color.Lime, 3), p2x, pSecond);
                g.DrawLine(new Pen(Color.Lime, 3), fitline.FirstPt, fitline.SecondPt);

                g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", new Font("宋体", 18), Brushes.Red, new PointF(18, 38));
                g.Dispose();

                if (m_IsSaveDebugPicture)
                {
                    //bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                //bmp.Dispose();
                bmpOrgDraw.Dispose();
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2结果画Mark点{myElapsedTime} ms");

            bmpSizedPre.Dispose();
            bmpSized.Dispose();

        }
        private void readBarcodeFuntionV7(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            if (IsByPass || IsByPassFromManual)
            {
                nDesc = ToChangeLanguage("不检测");
                nType = 0;
                return;
            }

            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            bool _isdrawresult = m_IsSaveDebugPicture;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            //定位偏移距离
            PointF _runOffset = new PointF(0, 0);

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = new Bitmap(bmpSized);// preProcessImage(bmpSized);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2图片预处理{myElapsedTime} ms");
            #endregion

            bool bOK = false;

            if (PreIsOpenAuFind)
            {
                myFind.bmpFind = bmpSizedPre;
                bOK = myFind.FindLarge();
                //bOK = true;
                if (bOK)
                {
                    if (myFind.xResults.Count > 0)
                    {
                        _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                                                               myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    }
                    //return;
                }
                else
                {
                    nDesc = ToChangeLanguage("定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
                //_runOffset = new PointF(0, 0);
            }
            if (inspect_dir_open)
            {

                Rectangle dirrect =
                      new Rectangle((int)(DirRectF.X + _runOffset.X),
                    (int)(DirRectF.Y + _runOffset.Y),
                   (int)DirRectF.Width,
                    (int)DirRectF.Height);
                BoundRect(ref dirrect, bmpInput.Size);
                Bitmap bmpdir = (Bitmap)bmpInput.Clone(dirrect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                myFind_dir.bmpFind = bmpdir;
                bOK = myFind_dir.FindLarge();
                bmpdir.Dispose();
                if (bOK)
                {
                    if (myFind_dir.xResults.Count > 0)
                    {
                        if (myFind_dir.xResults[0].fScore < paraClass_dir.tolerance)
                        {
                            nDesc = $"{ToChangeLanguage("方向定位失败分数")}{myFind_dir.xResults[0].fScore}";
                            nType = 0;

                            bmpSizedPre.Dispose();
                            bmpSized.Dispose();
                            return;
                        }
                        //_runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                    }
                    else
                    {
                        nDesc = ToChangeLanguage("方向定位失败");
                        nType = 0;

                        bmpSizedPre.Dispose();
                        bmpSized.Dispose();
                        return;
                    }

                    //if (myFind.xResults.Count > 0)
                    //{
                    //    _runOffset = new System.Drawing.PointF(myFind.xResults[0].fCenterX - _runBmpCenter.X,
                    //                                           myFind.xResults[0].fCenterY - _runBmpCenter.Y);
                    //}
                }
                else
                {
                    nDesc = ToChangeLanguage("方向定位失败");
                    nType = 0;

                    bmpSizedPre.Dispose();
                    bmpSized.Dispose();
                    return;
                }
            }

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2定位产品{myElapsedTime} ms");

            List<PointF> pts = new List<PointF>();
            RectangleF runmark0 = new RectangleF();
            RectangleF runmark1 = new RectangleF();
            PointF p1x = new PointF();
            PointF p2x = new PointF();
            PointF pSecond = new PointF();

            RectangleF runmarkline = new RectangleF();

            RectangleF runmarkline1 = new RectangleF();
            RectangleF runmarkline2 = new RectangleF();
            LineClass fitline = null;

            switch (m_ProcessImageMode)
            {
                case ProcessImageMode.V7:

                    bOK = false;
                    //mark0
                    runmark0 = new RectangleF(Mark0.X + _runOffset.X,
                       Mark0.Y + _runOffset.Y,
                       Mark0.Width,
                       Mark0.Height);

                    runmark0.Inflate(PreCenterExtendx, PreCenterExtendy);

                    p1x = calMarkBlob(bmpSizedPre, runmark0, PreMarkMinArea, out bOK, PreThresholdValue);
                    xP1 = new PointF(p1x.X + viewRectF.X, p1x.Y + viewRectF.Y);
                    xPCenter = new PointF(xP1.X, xP1.Y);
                    xP3 = new PointF(xP1.X, xP1.Y);

                    if (bOK)
                    {
                        //mark1
                        runmark1 = new RectangleF(Mark1.X + _runOffset.X,
                          Mark1.Y + _runOffset.Y,
                          Mark1.Width,
                          Mark1.Height);

                        runmark1.Inflate(PreCenterExtendx, PreCenterExtendy);

                        p2x = calMarkBlob(bmpSizedPre, runmark1, PreMarkMinArea, out bOK, PreThresholdValueMark2);
                        xP2 = new PointF(p2x.X + viewRectF.X, p2x.Y + viewRectF.Y);
                        xP4 = new PointF(xP2.X, xP2.Y);
                    }
                    break;
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算测试时间{myElapsedTime} ms");

            if (!bOK)
            {
                nDesc = ToChangeLanguage("Mark定位失败");
                nType = 0;

                bmpSizedPre.Dispose();
                bmpSized.Dispose();
                return;
            }

            nDesc = ToChangeLanguage("成功");
            nType = 1;

            watch.Stop();

            if (_isdrawresult)
            {
                int _sample = SampleValue;
                int _cropValue = CropValue * 2;

                Bitmap bmpOrgDraw = new Bitmap(1, 1);
                //Bitmap bmp = new Bitmap(1, 1);
                if (_isdrawresult)
                {
                    bmpOrgDraw = (Bitmap)bmpInput.Clone();// new Bitmap(bmpInput);
                    //bmp = (Bitmap)bmpSizedPre.Clone();// new Bitmap(bmpSizedPre);

                    myElapsedTime = jzTimesDurRecord.msDuriation;
                    jzTimesDurRecord.Cut();
                    logInfo.Log($"x2建图{myElapsedTime} ms");
                }
                Graphics g = Graphics.FromImage(bmpOrgDraw);
                RectangleF rectangleFmark0 = SimpleRectF(p1x, 2, 2);
                RectangleF rectangleFmark1 = SimpleRectF(p2x, 2, 2);
                //RectangleF rectangleFmark1 = SimpleRectF(p2x, 2, 2);

                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0, rectangleFmark1 });
                g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { runmark0, runmark1, runmarkline1, runmarkline2 });

                g.DrawLine(new Pen(Color.Lime, 3), p1x, pSecond);
                g.DrawLine(new Pen(Color.Lime, 3), p2x, pSecond);
                //g.DrawLine(new Pen(Color.Lime, 3), fitline.FirstPt, fitline.SecondPt);

                g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", new Font("宋体", 18), Brushes.Red, new PointF(18, 38));
                g.Dispose();

                if (m_IsSaveDebugPicture)
                {
                    //bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                //bmp.Dispose();
                bmpOrgDraw.Dispose();
            }


            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2结果画Mark点{myElapsedTime} ms");

            bmpSizedPre.Dispose();
            bmpSized.Dispose();

        }

        JzFindObjectClass m_Find = new JzFindObjectClass();
        PointF calMarkBlob(Bitmap bmpinput, RectangleF cropRect, int minarea, out bool bok, int thresholdvalue = 50)
        {
            bok = false;
            PointF ret = new PointF(cropRect.X + cropRect.Width / 2, cropRect.Y + cropRect.Height / 2);

            RectangleF cropRectnew = new RectangleF(cropRect.X, cropRect.Y, cropRect.Width, cropRect.Height);

            BoundRect(ref cropRectnew, bmpinput.Size);
            Bitmap bmptemp = (Bitmap)bmpinput.Clone(cropRectnew, PixelFormat.Format24bppRgb);
            m_Find.AH_SetThreshold(ref bmptemp, thresholdvalue);
            m_Find.AH_FindBlob(bmptemp, true);

            if (m_Find.FoundList.Count > 0)
            {
                int maxindex = m_Find.GetMaxRectIndex();
                if (m_Find.FoundList[maxindex].Area >= minarea)
                {
                    bok = true;
                    ret = new PointF((float)m_Find.FoundList[maxindex].rotatedRectangleF.fCX + cropRectnew.X,
                                (float)m_Find.FoundList[maxindex].rotatedRectangleF.fCY + cropRectnew.Y);
                }
            }
            bmptemp.Dispose();

            return ret;
        }

        /// <summary>
        /// 四边切图找边
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns></returns>
        List<PointF> getCalImageRectLineFour(Bitmap bmpInput)
        {
            List<PointF> list = new List<PointF>();

            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);

            Bitmap[] bitmaps = new Bitmap[4];
            LineClass[] lineClasses = new LineClass[4];
            Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            //左边
            Rectangle rleft = new Rectangle(0, rLevel.Y, _runBmpCenter.X - PreCenterExtendx, rLevel.Height);
            BoundRect(ref rleft, bmpInput.Size);
            bitmaps[0] = (Bitmap)bmpInput.Clone(rleft, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //右边
            Rectangle rright = new Rectangle(rLevel.Width - PreCenterExtendx - 1, rLevel.Y, _runBmpCenter.X + PreCenterExtendx, rLevel.Height);
            BoundRect(ref rright, bmpInput.Size);
            bitmaps[1] = (Bitmap)bmpInput.Clone(rright, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            rLevel = SimpleRect(_runBmpCenter, _cropValue / 2, bmpInput.Height / 2 - 1);
            //上边
            Rectangle rtop = new Rectangle(rLevel.X, 0, rLevel.Width, _runBmpCenter.Y - PreCenterExtendy);
            BoundRect(ref rtop, bmpInput.Size);
            bitmaps[2] = (Bitmap)bmpInput.Clone(rtop, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //下边
            Rectangle rbottom = new Rectangle(rLevel.X, rLevel.Height - PreCenterExtendy - 1, rLevel.Width, _runBmpCenter.Y + PreCenterExtendy);
            BoundRect(ref rbottom, bmpInput.Size);
            bitmaps[3] = (Bitmap)bmpInput.Clone(rbottom, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int item = 0; item < bitmaps.Length; item++)
            //Parallel.For(0, bitmaps.Length, item =>
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bitmaps[item] = grayscale.Apply(bitmaps[item]);

                AForge.Imaging.Filters.Invert invert = new Invert();
                bitmaps[item] = invert.Apply(bitmaps[item]);

                AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                fillHoles2.MaxHoleHeight = 150;
                fillHoles2.MaxHoleWidth = 150;
                fillHoles2.CoupledSizeFiltering = true;
                bitmaps[item] = fillHoles2.Apply(bitmaps[item]);

                switch (item)
                {
                    case 0:

                        #region 左边
                        lineClasses[0] = getLevelLine(bitmaps[item], true);
                        #endregion

                        break;
                    case 1:

                        #region 右边
                        lineClasses[1] = getLevelLine(bitmaps[item], false);
                        #endregion

                        break;

                    case 2:

                        #region 上边
                        lineClasses[2] = getVericalLine(bitmaps[item], true);
                        #endregion

                        break;
                    case 3:

                        #region 下边
                        lineClasses[3] = getVericalLine(bitmaps[item], false);
                        #endregion

                        break;
                }

                if (m_IsSaveDebugPicture)
                {
                    Bitmap bmpff = new Bitmap(bitmaps[item]);
                    Graphics g = Graphics.FromImage(bmpff);
                    g.DrawLine(new Pen(Color.Red, 3), lineClasses[item].FirstPt, lineClasses[item].SecondPt);
                    //bitmaps[item].Save(DebugPath + "\\item_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Save(DebugPath + "\\itemff_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Dispose();
                }


                bitmaps[item].Dispose();
                //});
            }

            LineClass line1 = getConvert(lineClasses[0], rleft);
            LineClass line2 = getConvert(lineClasses[1], rright);
            LineClass line3 = getConvert(lineClasses[2], rtop);
            LineClass line4 = getConvert(lineClasses[3], rbottom);

            PointF p1 = line3.FindIntersection(line1);
            PointF p2 = line3.FindIntersection(line2);

            PointF p4 = line4.FindIntersection(line1);
            PointF p3 = line4.FindIntersection(line2);


            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);

            #endregion

            list.Add(_recultPointf);
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);

            return list;
        }
        /// <summary>
        /// 四边切图找边
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns></returns>
        List<PointF> getCalImageRectLineFourEx001(Bitmap bmpInput)
        {
            List<PointF> list = new List<PointF>();

            XinliFindLineClass[] xinliFindLines = new XinliFindLineClass[4];

            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            #region 抠出图像

            Bitmap[] bitmaps = new Bitmap[4];
            LineClass[] lineClasses = new LineClass[4];
            //Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            //左边
            Rectangle rleft =
                new Rectangle((int)SideLeft.X,
                (int)SideLeft.Y,
               (int)SideLeft.Width,
                (int)SideLeft.Height);
            BoundRect(ref rleft, bmpInput.Size);
            bitmaps[0] = (Bitmap)bmpInput.Clone(rleft, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //右边
            Rectangle rright =
                 new Rectangle((int)SideRight.X,
                (int)SideRight.Y,
               (int)SideRight.Width,
                (int)SideRight.Height);
            BoundRect(ref rright, bmpInput.Size);
            bitmaps[1] = (Bitmap)bmpInput.Clone(rright, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //上边
            Rectangle rtop =
                 new Rectangle((int)SideTop.X,
                (int)SideTop.Y,
               (int)SideTop.Width,
                (int)SideTop.Height);
            BoundRect(ref rtop, bmpInput.Size);
            bitmaps[2] = (Bitmap)bmpInput.Clone(rtop, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //下边
            Rectangle rbottom =
                  new Rectangle((int)SideBottom.X,
                (int)SideBottom.Y,
               (int)SideBottom.Width,
                (int)SideBottom.Height);
            BoundRect(ref rbottom, bmpInput.Size);
            bitmaps[3] = (Bitmap)bmpInput.Clone(rbottom, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            #endregion

            //for (int item = 0; item < bitmaps.Length; item++)
            Parallel.For(0, bitmaps.Length, item =>
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bitmaps[item] = grayscale.Apply(bitmaps[item]);

                AForge.Imaging.Filters.Threshold threshold = new Threshold(PreThresholdValue);
                bitmaps[item] = threshold.Apply(bitmaps[item]);

                //AForge.Imaging.Filters.Invert invert = new Invert();
                //bitmaps[item] = invert.Apply(bitmaps[item]);

                AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                fillHoles2.MaxHoleHeight = 100;
                fillHoles2.MaxHoleWidth = 100;
                fillHoles2.CoupledSizeFiltering = false;
                bitmaps[item] = fillHoles2.Apply(bitmaps[item]);

                switch (item)
                {
                    case 0:

                        #region 左边
                        //lineClasses[0] = getLevelLine(bitmaps[item], true);


                        xinliFindLines[0] = new XinliFindLineClass();

                        //bmpinput0.Save("D:\\bmp00_left.bmp", ImageFormat.Bmp);

                        xinliFindLines[0].IsLeftToRight = dir_left;
                        xinliFindLines[0].IsBlack = dir_left_black;
                        xinliFindLines[0].SampleValue = SampleValue;
                        xinliFindLines[0].LeastPix = PreLineLeastDistance;
                        lineClasses[0] = xinliFindLines[0].GetLevelLine(bitmaps[item]);

                        #endregion

                        break;
                    case 1:

                        #region 右边
                        //lineClasses[1] = getLevelLine(bitmaps[item], false);

                        xinliFindLines[1] = new XinliFindLineClass();

                        xinliFindLines[1].IsLeftToRight = dir_right;
                        xinliFindLines[1].IsBlack = dir_right_black;
                        xinliFindLines[1].SampleValue = SampleValue;
                        xinliFindLines[1].LeastPix = PreLineLeastDistance;
                        lineClasses[1] = xinliFindLines[1].GetLevelLine(bitmaps[item]);

                        #endregion

                        break;

                    case 2:

                        #region 上边
                        //lineClasses[2] = getVericalLine(bitmaps[item], true);

                        xinliFindLines[2] = new XinliFindLineClass();
                        xinliFindLines[2].IsLeftToRight = dir_top;
                        xinliFindLines[2].IsBlack = dir_top_black;
                        xinliFindLines[2].SampleValue = SampleValue;
                        xinliFindLines[2].LeastPix = PreLineLeastDistance;
                        lineClasses[2] = xinliFindLines[2].GetVericalLine(bitmaps[item]);

                        #endregion

                        break;
                    case 3:

                        #region 下边
                        //lineClasses[3] = getVericalLine(bitmaps[item], false);


                        xinliFindLines[3] = new XinliFindLineClass();
                        xinliFindLines[3].IsLeftToRight = dir_bottom;
                        xinliFindLines[3].IsBlack = dir_bottom_black;
                        xinliFindLines[3].SampleValue = SampleValue;
                        xinliFindLines[3].LeastPix = PreLineLeastDistance;
                        lineClasses[3] = xinliFindLines[3].GetVericalLine(bitmaps[item]);
                        #endregion

                        break;
                }

                if (m_IsSaveDebugPicture)
                {
                    Bitmap bmpff = new Bitmap(bitmaps[item]);
                    Graphics g = Graphics.FromImage(bmpff);
                    g.DrawLine(new Pen(Color.Red, 3), lineClasses[item].FirstPt, lineClasses[item].SecondPt);
                    //bitmaps[item].Save(DebugPath + "\\item_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Save(DebugPath + "\\itemff_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Dispose();
                }


                bitmaps[item].Dispose();
            });
            //}

            LineClass line1 = getConvert(lineClasses[0], rleft);
            LineClass line2 = getConvert(lineClasses[1], rright);
            LineClass line3 = getConvert(lineClasses[2], rtop);
            LineClass line4 = getConvert(lineClasses[3], rbottom);

            //lefttop左上
            PointF p1 = line3.FindIntersection(line1);
            //righttop右上
            PointF p2 = line3.FindIntersection(line2);
            //leftbottom左下
            PointF p4 = line4.FindIntersection(line1);
            //rightbottom右下
            PointF p3 = line4.FindIntersection(line2);


            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);



            list.Add(_recultPointf);
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);

            return list;
        }
        List<PointF> getCalImageRectLineFourEx001_VM(Bitmap bmpInput, PointF eOffset)
        {
            List<PointF> list = new List<PointF>();

            XinliFindLineClass[] xinliFindLines = new XinliFindLineClass[4];

            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            #region 抠出图像

            Bitmap[] bitmaps = new Bitmap[4];
            LineClass[] lineClasses = new LineClass[4];
            //Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            //左边
            Rectangle rleft =
                new Rectangle((int)(SideLeft.X + eOffset.X),
                (int)(SideLeft.Y + eOffset.Y),
               (int)SideLeft.Width,
                (int)SideLeft.Height);
            BoundRect(ref rleft, bmpInput.Size);
            bitmaps[0] = (Bitmap)bmpInput.Clone(rleft, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //右边
            Rectangle rright =
                 new Rectangle((int)(SideRight.X + eOffset.X),
                (int)(SideRight.Y + eOffset.Y),
               (int)SideRight.Width,
                (int)SideRight.Height);
            BoundRect(ref rright, bmpInput.Size);
            bitmaps[1] = (Bitmap)bmpInput.Clone(rright, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //上边
            Rectangle rtop =
                 new Rectangle((int)(SideTop.X + eOffset.X),
                (int)(SideTop.Y + eOffset.Y),
               (int)SideTop.Width,
                (int)SideTop.Height);
            BoundRect(ref rtop, bmpInput.Size);
            bitmaps[2] = (Bitmap)bmpInput.Clone(rtop, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //下边
            Rectangle rbottom =
                  new Rectangle((int)(SideBottom.X + eOffset.X),
                (int)(SideBottom.Y + eOffset.Y),
               (int)SideBottom.Width,
                (int)SideBottom.Height);
            BoundRect(ref rbottom, bmpInput.Size);
            bitmaps[3] = (Bitmap)bmpInput.Clone(rbottom, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            #endregion

            //for (int item = 0; item < bitmaps.Length; item++)
            Parallel.For(0, bitmaps.Length, item =>
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bitmaps[item] = grayscale.Apply(bitmaps[item]);

                AForge.Imaging.Filters.Threshold threshold = new Threshold(PreThresholdValue);
                bitmaps[item] = threshold.Apply(bitmaps[item]);

                //AForge.Imaging.Filters.Invert invert = new Invert();
                //bitmaps[item] = invert.Apply(bitmaps[item]);

                //AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                //fillHoles2.MaxHoleHeight = 100;
                //fillHoles2.MaxHoleWidth = 100;
                //fillHoles2.CoupledSizeFiltering = false;
                //bitmaps[item] = fillHoles2.Apply(bitmaps[item]);

                switch (item)
                {
                    case 0:

                        #region 左边
                        //lineClasses[0] = getLevelLine(bitmaps[item], true);


                        xinliFindLines[0] = new XinliFindLineClass();

                        //bmpinput0.Save("D:\\bmp00_left.bmp", ImageFormat.Bmp);

                        xinliFindLines[0].IsLeftToRight = dir_left;
                        xinliFindLines[0].IsBlack = dir_left_black;
                        xinliFindLines[0].SampleValue = SampleValue;
                        xinliFindLines[0].LeastPix = PreLineLeastDistance;
                        lineClasses[0] = xinliFindLines[0].GetLevelLineVM(bitmaps[item]);

                        #endregion

                        break;
                    case 1:

                        #region 右边
                        //lineClasses[1] = getLevelLine(bitmaps[item], false);

                        xinliFindLines[1] = new XinliFindLineClass();

                        xinliFindLines[1].IsLeftToRight = dir_right;
                        xinliFindLines[1].IsBlack = dir_right_black;
                        xinliFindLines[1].SampleValue = SampleValue;
                        xinliFindLines[1].LeastPix = PreLineLeastDistance;
                        lineClasses[1] = xinliFindLines[1].GetLevelLineVM(bitmaps[item]);

                        #endregion

                        break;

                    case 2:

                        #region 上边
                        //lineClasses[2] = getVericalLine(bitmaps[item], true);

                        xinliFindLines[2] = new XinliFindLineClass();
                        xinliFindLines[2].IsLeftToRight = dir_top;
                        xinliFindLines[2].IsBlack = dir_top_black;
                        xinliFindLines[2].SampleValue = SampleValue;
                        xinliFindLines[2].LeastPix = PreLineLeastDistance;
                        lineClasses[2] = xinliFindLines[2].GetVericalLineVM(bitmaps[item]);

                        #endregion

                        break;
                    case 3:

                        #region 下边
                        //lineClasses[3] = getVericalLine(bitmaps[item], false);


                        xinliFindLines[3] = new XinliFindLineClass();
                        xinliFindLines[3].IsLeftToRight = dir_bottom;
                        xinliFindLines[3].IsBlack = dir_bottom_black;
                        xinliFindLines[3].SampleValue = SampleValue;
                        xinliFindLines[3].LeastPix = PreLineLeastDistance;
                        lineClasses[3] = xinliFindLines[3].GetVericalLineVM(bitmaps[item]);
                        #endregion

                        break;
                }

                if (m_IsSaveDebugPicture)
                {
                    Bitmap bmpff = new Bitmap(bitmaps[item]);
                    Graphics g = Graphics.FromImage(bmpff);
                    g.DrawLine(new Pen(Color.Red, 3), lineClasses[item].FirstPt, lineClasses[item].SecondPt);
                    //bitmaps[item].Save(DebugPath + "\\item_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Save(DebugPath + "\\itemff_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Dispose();
                }


                bitmaps[item].Dispose();
            });
            //}

            LineClass line1 = getConvert(lineClasses[0], rleft);
            LineClass line2 = getConvert(lineClasses[1], rright);
            LineClass line3 = getConvert(lineClasses[2], rtop);
            LineClass line4 = getConvert(lineClasses[3], rbottom);

            //lefttop左上
            PointF p1 = line3.FindIntersection(line1);
            //righttop右上
            PointF p2 = line3.FindIntersection(line2);
            //leftbottom左下
            PointF p4 = line4.FindIntersection(line1);
            //rightbottom右下
            PointF p3 = line4.FindIntersection(line2);


            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);



            list.Add(_recultPointf);
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);

            return list;
        }
        List<PointF> getCalImageRectLineFourEx001_VM2(Bitmap bmpInput, PointF eOffset, RectangleF eAlignRectF)
        {
            List<PointF> list = new List<PointF>();

            XinliFindLineClass[] xinliFindLines = new XinliFindLineClass[4];

            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            #region 抠出图像

            Bitmap[] bitmaps = new Bitmap[4];
            LineClass[] lineClasses = new LineClass[4];
            //Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            //左边
            Rectangle rleft =
                new Rectangle((int)(SideLeft.X + eOffset.X),
                (int)(SideLeft.Y + eOffset.Y),
               (int)SideLeft.Width,
                (int)SideLeft.Height);
            BoundRect(ref rleft, bmpInput.Size);
            bitmaps[0] = (Bitmap)bmpInput.Clone(rleft, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //右边
            Rectangle rright =
                 new Rectangle((int)(SideRight.X + eOffset.X),
                (int)(SideRight.Y + eOffset.Y),
               (int)SideRight.Width,
                (int)SideRight.Height);
            BoundRect(ref rright, bmpInput.Size);
            bitmaps[1] = (Bitmap)bmpInput.Clone(rright, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //上边
            Rectangle rtop =
                 new Rectangle((int)(SideTop.X + eOffset.X),
                (int)(SideTop.Y + eOffset.Y),
               (int)SideTop.Width,
                (int)SideTop.Height);
            BoundRect(ref rtop, bmpInput.Size);
            bitmaps[2] = (Bitmap)bmpInput.Clone(rtop, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //下边
            Rectangle rbottom =
                  new Rectangle((int)(SideBottom.X + eOffset.X),
                (int)(SideBottom.Y + eOffset.Y),
               (int)SideBottom.Width,
                (int)SideBottom.Height);
            BoundRect(ref rbottom, bmpInput.Size);
            bitmaps[3] = (Bitmap)bmpInput.Clone(rbottom, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            #endregion

            //for (int item = 0; item < bitmaps.Length; item++)
            Parallel.For(0, bitmaps.Length, item =>
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bitmaps[item] = grayscale.Apply(bitmaps[item]);

                AForge.Imaging.Filters.Threshold threshold = new Threshold(PreThresholdValue);
                bitmaps[item] = threshold.Apply(bitmaps[item]);

                //AForge.Imaging.Filters.Invert invert = new Invert();
                //bitmaps[item] = invert.Apply(bitmaps[item]);

                //AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                //fillHoles2.MaxHoleHeight = 100;
                //fillHoles2.MaxHoleWidth = 100;
                //fillHoles2.CoupledSizeFiltering = false;
                //bitmaps[item] = fillHoles2.Apply(bitmaps[item]);

                switch (item)
                {
                    case 0:

                        #region 左边
                        //lineClasses[0] = getLevelLine(bitmaps[item], true);


                        xinliFindLines[0] = new XinliFindLineClass();

                        //bmpinput0.Save("D:\\bmp00_left.bmp", ImageFormat.Bmp);

                        xinliFindLines[0].IsLeftToRight = dir_left;
                        xinliFindLines[0].IsBlack = dir_left_black;
                        xinliFindLines[0].SampleValue = SampleValue;
                        xinliFindLines[0].LeastPix = PreLineLeastDistance;
                        lineClasses[0] = xinliFindLines[0].GetLevelLineVM(bitmaps[item]);

                        #endregion

                        break;
                    case 1:

                        #region 右边
                        //lineClasses[1] = getLevelLine(bitmaps[item], false);

                        xinliFindLines[1] = new XinliFindLineClass();

                        xinliFindLines[1].IsLeftToRight = dir_right;
                        xinliFindLines[1].IsBlack = dir_right_black;
                        xinliFindLines[1].SampleValue = SampleValue;
                        xinliFindLines[1].LeastPix = PreLineLeastDistance;
                        lineClasses[1] = xinliFindLines[1].GetLevelLineVM(bitmaps[item]);

                        #endregion

                        break;

                    case 2:

                        #region 上边
                        //lineClasses[2] = getVericalLine(bitmaps[item], true);

                        xinliFindLines[2] = new XinliFindLineClass();
                        xinliFindLines[2].IsLeftToRight = dir_top;
                        xinliFindLines[2].IsBlack = dir_top_black;
                        xinliFindLines[2].SampleValue = SampleValue;
                        xinliFindLines[2].LeastPix = PreLineLeastDistance;
                        lineClasses[2] = xinliFindLines[2].GetVericalLineVM(bitmaps[item]);

                        #endregion

                        break;
                    case 3:

                        #region 下边
                        //lineClasses[3] = getVericalLine(bitmaps[item], false);


                        xinliFindLines[3] = new XinliFindLineClass();
                        xinliFindLines[3].IsLeftToRight = dir_bottom;
                        xinliFindLines[3].IsBlack = dir_bottom_black;
                        xinliFindLines[3].SampleValue = SampleValue;
                        xinliFindLines[3].LeastPix = PreLineLeastDistance;
                        lineClasses[3] = xinliFindLines[3].GetVericalLineVM(bitmaps[item]);
                        #endregion

                        break;
                }

                if (m_IsSaveDebugPicture)
                {
                    Bitmap bmpff = new Bitmap(bitmaps[item]);
                    Graphics g = Graphics.FromImage(bmpff);
                    g.DrawLine(new Pen(Color.Red, 3), lineClasses[item].FirstPt, lineClasses[item].SecondPt);
                    //bitmaps[item].Save(DebugPath + "\\item_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Save(DebugPath + "\\itemff_result_" + lblName + item.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    bmpff.Dispose();
                }


                bitmaps[item].Dispose();
            });
            //}

            LineClass line1 = getConvert(lineClasses[0], rleft);
            LineClass line2 = getConvert(lineClasses[1], rright);
            LineClass line3 = getConvert(lineClasses[2], rtop);
            LineClass line4 = getConvert(lineClasses[3], rbottom);

            //lefttop左上
            PointF p1x = line3.FindIntersection(line1);
            //righttop右上
            PointF p2x = line3.FindIntersection(line2);
            //leftbottom左下
            PointF p4x = line4.FindIntersection(line1);
            //rightbottom右下
            PointF p3x = line4.FindIntersection(line2);


            PointF p1 = new PointF(eAlignRectF.X + p1x.X, eAlignRectF.Y + p1x.Y);
            PointF p2 = new PointF(eAlignRectF.X + p2x.X, eAlignRectF.Y + p2x.Y);
            PointF p3 = new PointF(eAlignRectF.X + p3x.X, eAlignRectF.Y + p3x.Y);
            PointF p4 = new PointF(eAlignRectF.X + p4x.X, eAlignRectF.Y + p4x.Y);


            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            //PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            PointF tempptf = new PointF(RealRectangleF.X, RealRectangleF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);



            list.Add(_recultPointf);
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);

            return list;
        }
        /// <summary>
        /// 整个画面处理找边
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns></returns>
        List<PointF> getCalImageRectLine(Bitmap bmpInput)
        {
            List<PointF> list = new List<PointF>();

            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);

            #region 水平方向

            Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            BoundRect(ref rLevel, bmpInput.Size);
            ix = 0;
            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(0, rLevel.Y + ix * _sample, rLevel.Width, _sample);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int x1, x2 = 0;
                if (PreDir)
                {
                    x1 = GetLineInXDir(bmp2, false, 0 + PreCenterExtendx, 0, Color.White);
                    x2 = GetLineInXDir(bmp2, true, bmp2.Width - PreCenterExtendx, 0, Color.White);
                }
                else
                {
                    x1 = GetLineInXDir(bmp2, false, bmp2.Width / 2 - PreCenterExtendx, 0, Color.Black);
                    x2 = GetLineInXDir(bmp2, true, bmp2.Width / 2 + PreCenterExtendx, 0, Color.Black);
                }


                points1[ix] = new PointF(x1, rectangle.Y);
                points2[ix] = new PointF(x2, rectangle.Y);

                bmp2.Dispose();

                ix++;
            }

            LineClass line1 = getLineForPointF(points1, pix: PreLineLeastDistance);
            LineClass line2 = getLineForPointF(points2, pix: PreLineLeastDistance);

            #endregion

            //myElapsedTime = jzTimesDurRecord.msDuriation;
            //jzTimesDurRecord.Cut();
            //logInfo.Log($"x2计算1 level{myElapsedTime} ms");


            #region 垂直方向

            //垂直方向
            rLevel = SimpleRect(_runBmpCenter, _cropValue / 2, bmpInput.Height / 2 - 1);
            BoundRect(ref rLevel, bmpInput.Size);
            ix = 0;
            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(rLevel.X + ix * _sample, 0, _sample, rLevel.Height);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int y1, y2 = 0;
                if (PreDir)
                {
                    y1 = GetLineInYDir(bmp2, false, 0 + PreCenterExtendy, 0, Color.White);
                    y2 = GetLineInYDir(bmp2, true, bmp2.Height - PreCenterExtendy, 0, Color.White);
                }
                else
                {
                    y1 = GetLineInYDir(bmp2, false, bmp2.Height / 2 - PreCenterExtendy, 0, Color.Black);
                    y2 = GetLineInYDir(bmp2, true, bmp2.Height / 2 + PreCenterExtendy, 0, Color.Black);
                }


                points1[ix] = new PointF(rectangle.X, y1);
                points2[ix] = new PointF(rectangle.X, y2);

                bmp2.Dispose();

                ix++;
            }

            LineClass line3 = getLineForPointF(points1, false, pix: PreLineLeastDistance);
            LineClass line4 = getLineForPointF(points2, false, pix: PreLineLeastDistance);

            #endregion

            //myElapsedTime = jzTimesDurRecord.msDuriation;
            //jzTimesDurRecord.Cut();
            //logInfo.Log($"x2计算2 verical{myElapsedTime} ms");

            PointF p3 = line3.FindIntersection(line1);//右下
            PointF p4 = line3.FindIntersection(line2);//左下

            PointF p2 = line4.FindIntersection(line1);//右上
            PointF p1 = line4.FindIntersection(line2);//左上

            //PointF p1 = line3.FindIntersection(line1);
            //PointF p2 = line3.FindIntersection(line2);

            //PointF p4 = line4.FindIntersection(line1);
            //PointF p3 = line4.FindIntersection(line2);


            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);

            list.Add(_recultPointf);
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);

            return list;
        }
        List<PointF> getCalImageRectLineVM(Bitmap bmpInput, Point eCenterCal)
        {
            List<PointF> list = new List<PointF>();

            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);

            #region 水平方向

            Rectangle rLevel = SimpleRect(eCenterCal, bmpInput.Width / 2 - 1, _cropValue / 2);
            BoundRect(ref rLevel, bmpInput.Size);

            XinliFindLineClass xinliFindLine = new XinliFindLineClass();
            xinliFindLine.IsLeftToRight = true;

            Rectangle rectangle = new Rectangle(0, rLevel.Y, rLevel.Width / 2, rLevel.Height);
            Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //bmp2.Save("D:\\SMALL_left.png", ImageFormat.Png);
            LineClass line2 = xinliFindLine.GetLevelLineVM_small(bmp2, rectangle);

            rectangle = new Rectangle(0 + rLevel.Width / 2, rLevel.Y, rLevel.Width / 2, rLevel.Height);
            bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //bmp2.Save("D:\\SMALL_right.png", ImageFormat.Png);
            LineClass line1 = xinliFindLine.GetLevelLineVM_small(bmp2, rectangle, false);

            #endregion

            //myElapsedTime = jzTimesDurRecord.msDuriation;
            //jzTimesDurRecord.Cut();
            //logInfo.Log($"x2计算1 level{myElapsedTime} ms");


            #region 垂直方向

            //垂直方向
            rLevel = SimpleRect(eCenterCal, _cropValue / 2, bmpInput.Height / 2 - 1);
            BoundRect(ref rLevel, bmpInput.Size);

            rectangle = new Rectangle(rLevel.X, 0, rLevel.Width, rLevel.Height / 2);
            bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            LineClass line4 = xinliFindLine.GetVericalLineVM_samll(bmp2, rectangle);

            rectangle = new Rectangle(rLevel.X, 0 + rLevel.Height / 2, rLevel.Width, rLevel.Height / 2);
            bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            LineClass line3 = xinliFindLine.GetVericalLineVM_samll(bmp2, rectangle, false);

            #endregion

            //myElapsedTime = jzTimesDurRecord.msDuriation;
            //jzTimesDurRecord.Cut();
            //logInfo.Log($"x2计算2 verical{myElapsedTime} ms");

            PointF p3 = line3.FindIntersection(line1);//右下
            PointF p4 = line3.FindIntersection(line2);//左下

            PointF p2 = line4.FindIntersection(line1);//右上
            PointF p1 = line4.FindIntersection(line2);//左上

            //PointF p1 = line3.FindIntersection(line1);
            //PointF p2 = line3.FindIntersection(line2);

            //PointF p4 = line4.FindIntersection(line1);
            //PointF p3 = line4.FindIntersection(line2);


            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);

            list.Add(_recultPointf);

            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);


            return list;
        }
        List<PointF> getCalImageRectLineVM2(Bitmap bmpInput, Point eCenterCal, RectangleF eAlignRectF)
        {
            List<PointF> list = new List<PointF>();

            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);

            #region 水平方向

            Rectangle rLevel = SimpleRect(eCenterCal, bmpInput.Width / 2 - 1, _cropValue / 2);
            BoundRect(ref rLevel, bmpInput.Size);

            XinliFindLineClass xinliFindLine = new XinliFindLineClass();
            xinliFindLine.IsLeftToRight = true;

            Rectangle rectangle = new Rectangle(0, rLevel.Y, rLevel.Width / 2, rLevel.Height);
            Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //bmp2.Save("D:\\SMALL_left.png", ImageFormat.Png);
            LineClass line2 = xinliFindLine.GetLevelLineVM_small(bmp2, rectangle);

            rectangle = new Rectangle(0 + rLevel.Width / 2, rLevel.Y, rLevel.Width / 2, rLevel.Height);
            bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //bmp2.Save("D:\\SMALL_right.png", ImageFormat.Png);
            LineClass line1 = xinliFindLine.GetLevelLineVM_small(bmp2, rectangle, false);

            #endregion

            //myElapsedTime = jzTimesDurRecord.msDuriation;
            //jzTimesDurRecord.Cut();
            //logInfo.Log($"x2计算1 level{myElapsedTime} ms");


            #region 垂直方向

            //垂直方向
            rLevel = SimpleRect(eCenterCal, _cropValue / 2, bmpInput.Height / 2 - 1);
            BoundRect(ref rLevel, bmpInput.Size);

            rectangle = new Rectangle(rLevel.X, 0, rLevel.Width, rLevel.Height / 2);
            bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            LineClass line4 = xinliFindLine.GetVericalLineVM_samll(bmp2, rectangle);

            rectangle = new Rectangle(rLevel.X, 0 + rLevel.Height / 2, rLevel.Width, rLevel.Height / 2);
            bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            LineClass line3 = xinliFindLine.GetVericalLineVM_samll(bmp2, rectangle, false);

            #endregion

            //myElapsedTime = jzTimesDurRecord.msDuriation;
            //jzTimesDurRecord.Cut();
            //logInfo.Log($"x2计算2 verical{myElapsedTime} ms");

            PointF p3x = line3.FindIntersection(line1);//右下
            PointF p4x = line3.FindIntersection(line2);//左下

            PointF p2x = line4.FindIntersection(line1);//右上
            PointF p1x = line4.FindIntersection(line2);//左上

            //PointF p1 = line3.FindIntersection(line1);
            //PointF p2 = line3.FindIntersection(line2);

            //PointF p4 = line4.FindIntersection(line1);
            //PointF p3 = line4.FindIntersection(line2);

            PointF p1 = new PointF(eAlignRectF.X + p1x.X, eAlignRectF.Y + p1x.Y);
            PointF p2 = new PointF(eAlignRectF.X + p2x.X, eAlignRectF.Y + p2x.Y);
            PointF p3 = new PointF(eAlignRectF.X + p3x.X, eAlignRectF.Y + p3x.Y);
            PointF p4 = new PointF(eAlignRectF.X + p4x.X, eAlignRectF.Y + p4x.Y);



            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            //PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);
            //PointF tempptf = new PointF(eAlignRectF.X + RealRectangleF.X, eAlignRectF.Y + RealRectangleF.Y);
            PointF tempptf = new PointF(RealRectangleF.X, RealRectangleF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);

            list.Add(_recultPointf);

            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);


            return list;
        }
        LineClass getLevelLine(Bitmap bmpInput, bool islefttoright = true)
        {
            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(0, ix * _sample, bmpInput.Width - 1, _sample);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int x1;
                if (PreDir)
                {
                    if (islefttoright)
                        x1 = GetLineInXDir(bmp2, false, 0, 0, Color.Black);
                    else
                        x1 = GetLineInXDir(bmp2, true, bmp2.Width, 0, Color.Black);
                }
                else
                {
                    if (!islefttoright)
                        x1 = GetLineInXDir(bmp2, false, 0, 0, Color.White);
                    else
                        x1 = GetLineInXDir(bmp2, true, bmp2.Width, 0, Color.White);
                }

                points1[ix] = new PointF(x1, rectangle.Y);

                bmp2.Dispose();

                ix++;
            }
            LineClass line = getLineForPointF(points1, pix: PreLineLeastDistance);
            return line;
        }
        LineClass getVericalLine(Bitmap bmpInput, bool istoptobottom = true)
        {
            int _sample = SampleValue;
            int _cropValue = bmpInput.Width - 1;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(ix * _sample, 0, _sample, bmpInput.Height - 1);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int y1 = 0;
                if (PreDir)
                {
                    if (istoptobottom)
                        y1 = GetLineInYDir(bmp2, false, 0, 0, Color.Black);
                    else
                        y1 = GetLineInYDir(bmp2, true, bmp2.Height, 0, Color.Black);
                }
                else
                {
                    if (!istoptobottom)
                        y1 = GetLineInYDir(bmp2, false, 0, 0, Color.White);
                    else
                        y1 = GetLineInYDir(bmp2, true, bmp2.Height, 0, Color.White);
                }
                points1[ix] = new PointF(rectangle.X, y1);
                bmp2.Dispose();

                ix++;
            }
            LineClass line = getLineForPointF(points1, false, pix: PreLineLeastDistance);
            return line;
        }

        LineClass getConvert(LineClass line, Rectangle rectx)
        {
            PointF p1 = new PointF(line.FirstPt.X + rectx.X, line.FirstPt.Y + rectx.Y);
            PointF p2 = new PointF(line.SecondPt.X + rectx.X, line.SecondPt.Y + rectx.Y);

            return new LineClass(p1, p2);
        }
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

        #region 两边找直线

        LineClass findLine(Bitmap bmpinput, RectangleF croprectf, int thresholdvalue = 128)
        {
            Bitmap bmpinput0 = bmpinput.Clone(croprectf, PixelFormat.Format24bppRgb);

            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmpinput0 = grayscale.Apply(bmpinput0);

            AForge.Imaging.Filters.Threshold threshold = new Threshold(thresholdvalue);
            bmpinput0 = threshold.Apply(bmpinput0);

            AForge.Imaging.Filters.Invert invert = new Invert();
            bmpinput0 = invert.Apply(bmpinput0);

            AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
            fillHoles2.MaxHoleHeight = 100;
            fillHoles2.MaxHoleWidth = 100;
            fillHoles2.CoupledSizeFiltering = false;
            bmpinput0 = fillHoles2.Apply(bmpinput0);

            LineClass lineClass = getVericalLine(bmpinput0, true);
            bmpinput0.Dispose();

            PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
            PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

            p1.X += croprectf.X;
            p1.Y += croprectf.Y;

            p2.X += croprectf.X;
            p2.Y += croprectf.Y;
            LineClass lineClassx = new LineClass(p1, p2);
            return lineClassx;
        }

        #endregion


        #region 备份算法
        private void readBarcodeFuntion2(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            bool _isdrawresult = m_IsSaveDebugPicture;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = preProcessImage(bmpSized);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2图片预处理{myElapsedTime} ms");

            if (PreIsOpenAuFind)
            {
                myFind.bmpFind = bmpSizedPre;
                bool bOK = myFind.FindLarge();

                if (bOK)
                {
                    if (myFind.xResults.Count > 0)
                    {
                        _runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                    }
                }
                else
                {
                    nDesc = ToChangeLanguage("定位失败");
                    nType = 0;
                    return;
                }
            }

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2定位产品{myElapsedTime} ms");


            Bitmap bmpOrgDraw = new Bitmap(1, 1);
            Bitmap bmp = new Bitmap(1, 1);
            if (_isdrawresult)
            {
                bmpOrgDraw = (Bitmap)bmpInput.Clone();// new Bitmap(bmpInput);
                bmp = (Bitmap)bmpSizedPre.Clone();// new Bitmap(bmpSizedPre);

                myElapsedTime = jzTimesDurRecord.msDuriation;
                jzTimesDurRecord.Cut();
                logInfo.Log($"x2建图{myElapsedTime} ms");
            }

            //fillHoles = null;
            //bradleyLocalThresholding = null;
            //grayscale = null;


            Graphics g = Graphics.FromImage(bmpOrgDraw);

            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            #region 水平方向

            Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            BoundRect(ref rLevel, bmpInput.Size);
            ix = 0;
            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(0, rLevel.Y + ix * _sample, rLevel.Width, _sample);
                Bitmap bmp2 = (Bitmap)bmpSizedPre.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int x1, x2 = 0;
                if (PreDir)
                {
                    x1 = GetLineInXDir(bmp2, false, 0 + PreCenterExtendx, 0, Color.White);
                    x2 = GetLineInXDir(bmp2, true, bmp2.Width - PreCenterExtendx, 0, Color.White);
                }
                else
                {
                    x1 = GetLineInXDir(bmp2, false, bmp2.Width / 2 - PreCenterExtendx, 0, Color.Black);
                    x2 = GetLineInXDir(bmp2, true, bmp2.Width / 2 + PreCenterExtendx, 0, Color.Black);
                }


                points1[ix] = new PointF(x1, rectangle.Y);
                points2[ix] = new PointF(x2, rectangle.Y);

                bmp2.Dispose();

                ix++;
            }

            LineClass line1 = getLineForPointF(points1, pix: PreLineLeastDistance);
            LineClass line2 = getLineForPointF(points2, pix: PreLineLeastDistance);

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算1 level{myElapsedTime} ms");

            if (_isdrawresult)
                g.DrawRectangle(new Pen(Color.Lime, 3), rLevel);

            #region 垂直方向

            //垂直方向
            rLevel = SimpleRect(_runBmpCenter, _cropValue / 2, bmpInput.Height / 2 - 1);
            BoundRect(ref rLevel, bmpInput.Size);
            ix = 0;
            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(rLevel.X + ix * _sample, 0, _sample, rLevel.Height);
                Bitmap bmp2 = (Bitmap)bmpSizedPre.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int y1, y2 = 0;
                if (PreDir)
                {
                    y1 = GetLineInYDir(bmp2, false, 0 + PreCenterExtendy, 0, Color.White);
                    y2 = GetLineInYDir(bmp2, true, bmp2.Height - PreCenterExtendy, 0, Color.White);
                }
                else
                {
                    y1 = GetLineInYDir(bmp2, false, bmp2.Height / 2 - PreCenterExtendy, 0, Color.Black);
                    y2 = GetLineInYDir(bmp2, true, bmp2.Height / 2 + PreCenterExtendy, 0, Color.Black);
                }


                points1[ix] = new PointF(rectangle.X, y1);
                points2[ix] = new PointF(rectangle.X, y2);

                bmp2.Dispose();

                ix++;
            }

            LineClass line3 = getLineForPointF(points1, false, pix: PreLineLeastDistance);
            LineClass line4 = getLineForPointF(points2, false, pix: PreLineLeastDistance);

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算2 verical{myElapsedTime} ms");

            PointF p1 = line3.FindIntersection(line1);
            PointF p2 = line3.FindIntersection(line2);

            PointF p4 = line4.FindIntersection(line1);
            PointF p3 = line4.FindIntersection(line2);
            if (_isdrawresult)
            {
                g.DrawLine(new Pen(Color.Red, 3), p1, p2);
                g.DrawLine(new Pen(Color.Red, 3), p1, p4);
                g.DrawLine(new Pen(Color.Red, 3), p2, p3);
                g.DrawLine(new Pen(Color.Red, 3), p3, p4);

                //交叉线
                g.DrawLine(new Pen(Color.Red, 3), p1, p3);
                g.DrawLine(new Pen(Color.Red, 3), p2, p4);
            }

            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            RectangleF rectangleF = SimpleRectF(_recultPointf, 2, 2);

            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算完成{myElapsedTime} ms");

            watch.Stop();

            if (_isdrawresult)
            {
                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleF });
                g.DrawRectangle(new Pen(Color.Lime, 3), rLevel);
                g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", new Font("宋体", 18), Brushes.Red, new PointF(18, 38));
            }
            g.Dispose();

            #endregion

            if (m_IsSaveDebugPicture)
            {
                bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            nDesc = ToChangeLanguage("成功");
            bmpSizedPre.Dispose();
            bmpSized.Dispose();
            bmp.Dispose();
            bmpOrgDraw.Dispose();

        }
#if BAK_20240924
private void readBarcodeFuntion2(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            bool _isdrawresult = m_IsSaveDebugPicture;

            //图像中心
            Point _runBmpCenter = new Point(bmpInput.Width / 2, bmpInput.Height / 2);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = preProcessImage(bmpSized);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2图片预处理{myElapsedTime} ms");

            //if (PreIsOpenAuFind)
            {
                myFind.bmpFind = bmpSizedPre;
                bool bOK = myFind.FindLarge();

                if (bOK)
                {
                    if (myFind.xResults.Count > 0)
                    {
                        _runBmpCenter = new System.Drawing.Point((int)myFind.xResults[0].fCenterX, (int)myFind.xResults[0].fCenterY);
                    }
                }
                else
                {
                    nDesc = "定位失败";
                    nType = 0;
                    return;
                }
            }

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2定位产品{myElapsedTime} ms");


            Bitmap bmpOrgDraw = new Bitmap(1, 1);
            Bitmap bmp = new Bitmap(1, 1);
            if (_isdrawresult)
            {
                bmpOrgDraw = (Bitmap)bmpInput.Clone();// new Bitmap(bmpInput);
                bmp = (Bitmap)bmpSizedPre.Clone();// new Bitmap(bmpSizedPre);

                myElapsedTime = jzTimesDurRecord.msDuriation;
                jzTimesDurRecord.Cut();
                logInfo.Log($"x2建图{myElapsedTime} ms");
            }

            //fillHoles = null;
            //bradleyLocalThresholding = null;
            //grayscale = null;


            Graphics g = Graphics.FromImage(bmpOrgDraw);

            int _sample = SampleValue;
            int _cropValue = CropValue * 2;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            #region 水平方向

            Rectangle rLevel = SimpleRect(_runBmpCenter, bmpInput.Width / 2 - 1, _cropValue / 2);
            BoundRect(ref rLevel, bmpInput.Size);
            ix = 0;
            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(0, rLevel.Y + ix * _sample, rLevel.Width, _sample);
                Bitmap bmp2 = (Bitmap)bmpSizedPre.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int x1, x2 = 0;
                if (PreDir)
                {
                    x1 = GetLineInXDir(bmp2, false, 0 + PreCenterExtendx, 0, Color.White);
                    x2 = GetLineInXDir(bmp2, true, bmp2.Width - PreCenterExtendx, 0, Color.White);
                }
                else
                {
                    x1 = GetLineInXDir(bmp2, false, bmp2.Width / 2 - PreCenterExtendx, 0, Color.Black);
                    x2 = GetLineInXDir(bmp2, true, bmp2.Width / 2 + PreCenterExtendx, 0, Color.Black);
                }


                points1[ix] = new PointF(x1, rectangle.Y);
                points2[ix] = new PointF(x2, rectangle.Y);

                bmp2.Dispose();

                ix++;
            }

            LineClass line1 = getLineForPointF(points1, pix: PreLineLeastDistance);
            LineClass line2 = getLineForPointF(points2, pix: PreLineLeastDistance);

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算1 level{myElapsedTime} ms");

            if (_isdrawresult)
                g.DrawRectangle(new Pen(Color.Lime, 3), rLevel);

            #region 垂直方向

            //垂直方向
            rLevel = SimpleRect(_runBmpCenter, _cropValue / 2, bmpInput.Height / 2 - 1);
            BoundRect(ref rLevel, bmpInput.Size);
            ix = 0;
            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(rLevel.X + ix * _sample, 0, _sample, rLevel.Height);
                Bitmap bmp2 = (Bitmap)bmpSizedPre.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int y1, y2 = 0;
                if (PreDir)
                {
                    y1 = GetLineInYDir(bmp2, false, 0 + PreCenterExtendy, 0, Color.White);
                    y2 = GetLineInYDir(bmp2, true, bmp2.Height - PreCenterExtendy, 0, Color.White);
                }
                else
                {
                    y1 = GetLineInYDir(bmp2, false, bmp2.Height / 2 - PreCenterExtendy, 0, Color.Black);
                    y2 = GetLineInYDir(bmp2, true, bmp2.Height / 2 + PreCenterExtendy, 0, Color.Black);
                }


                points1[ix] = new PointF(rectangle.X, y1);
                points2[ix] = new PointF(rectangle.X, y2);

                bmp2.Dispose();

                ix++;
            }

            LineClass line3 = getLineForPointF(points1, false, pix: PreLineLeastDistance);
            LineClass line4 = getLineForPointF(points2, false, pix: PreLineLeastDistance);

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算2 verical{myElapsedTime} ms");

            PointF p1 = line3.FindIntersection(line1);
            PointF p2 = line3.FindIntersection(line2);

            PointF p4 = line4.FindIntersection(line1);
            PointF p3 = line4.FindIntersection(line2);
            if (_isdrawresult)
            {
                g.DrawLine(new Pen(Color.Red, 3), p1, p2);
                g.DrawLine(new Pen(Color.Red, 3), p1, p4);
                g.DrawLine(new Pen(Color.Red, 3), p2, p3);
                g.DrawLine(new Pen(Color.Red, 3), p3, p4);

                //交叉线
                g.DrawLine(new Pen(Color.Red, 3), p1, p3);
                g.DrawLine(new Pen(Color.Red, 3), p2, p4);
            }

            LineClass l1 = new LineClass(p1, p3);
            LineClass l2 = new LineClass(p2, p4);

            PointF _recultPointf = l1.FindIntersection(l2);
            RectangleF rectangleF = SimpleRectF(_recultPointf, 2, 2);

            PointF tempptf = new PointF(viewRectF.X, viewRectF.Y);

            xP1 = new PointF(tempptf.X + p1.X, tempptf.Y + p1.Y);
            xP2 = new PointF(tempptf.X + p2.X, tempptf.Y + p2.Y);
            xP3 = new PointF(tempptf.X + p3.X, tempptf.Y + p3.Y);
            xP4 = new PointF(tempptf.X + p4.X, tempptf.Y + p4.Y);
            xPCenter = new PointF(tempptf.X + _recultPointf.X, tempptf.Y + _recultPointf.Y);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"x2计算完成{myElapsedTime} ms");

            watch.Stop();

            if (_isdrawresult)
            {
                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleF });
                g.DrawRectangle(new Pen(Color.Lime, 3), rLevel);
                g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", new Font("宋体", 18), Brushes.Red, new PointF(18, 38));
            }
            g.Dispose();

            #endregion

            if (m_IsSaveDebugPicture)
            {
                bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            nDesc = "成功";
            bmpSizedPre.Dispose();
            bmpSized.Dispose();
            bmp.Dispose();
            bmpOrgDraw.Dispose();

        }

 private void readBarcodeFuntion0(Bitmap bmpInput)
        {
            Bitmap bmpbarcode0 = new Bitmap(bmpInput);

            try
            {
                //Gray
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                Bitmap bmpgray = grayscale.Apply(bmpInput);

                //Otsu
                AForge.Imaging.Filters.OtsuThreshold otsuThreshold = new AForge.Imaging.Filters.OtsuThreshold();
                Bitmap bmpotsu = otsuThreshold.Apply(bmpgray);

                AForge.Imaging.Filters.ExtractBiggestBlob extractBiggestBlob = new AForge.Imaging.Filters.ExtractBiggestBlob();
                Bitmap bmpextractBiggestBlob = extractBiggestBlob.Apply(bmpotsu);
                Rectangle rectangle = new Rectangle(extractBiggestBlob.BlobPosition.X,
                                                                           extractBiggestBlob.BlobPosition.Y,
                                                                           bmpextractBiggestBlob.Width * 3 / 4,
                                                                           bmpextractBiggestBlob.Height);

                //切出标签位置
                rectangle.Inflate(-260, -50);
                JzToolsClass jzToolsClass = new JzToolsClass();
                jzToolsClass.BoundRect(ref rectangle, bmpInput.Size);
                Bitmap bmp00 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Bitmap bmp01 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                //Gray
                AForge.Imaging.Filters.Grayscale grayscale1 = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                Bitmap bmpgray1 = grayscale1.Apply(bmp00);

                //Invert
                AForge.Imaging.Filters.Invert invertx = new AForge.Imaging.Filters.Invert();
                Bitmap bmpinvertx = invertx.Apply(bmpgray1);

                extractBiggestBlob.Apply(bmpinvertx);

                rectangle = new Rectangle(extractBiggestBlob.BlobPosition.X,
                                                                           extractBiggestBlob.BlobPosition.Y,
                                                                           bmpextractBiggestBlob.Width,
                                                                           bmpextractBiggestBlob.Height);

                rectangle.Inflate(10, 10);
                jzToolsClass.BoundRect(ref rectangle, bmp01.Size);

                bmpbarcode0.Dispose();
                bmpbarcode0 = (Bitmap)bmp01.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                //if (!System.IO.Directory.Exists(_path))
                //    System.IO.Directory.CreateDirectory(_path);
                //bmpbarcode0.Save(_path + "\\run_" + item.Index.ToString() + "" + ".png",
                //                                                            System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                bmpbarcode0.Dispose();
                bmpbarcode0 = new Bitmap(bmpInput);
            }

            //IBarcode IxBarcode = new BarcodeGzx1Class();
            //IxBarcode.InputImage = bmpbarcode0;
            ////IxBarcode.InputImage = item.BmpRun;
            ////new Bitmap(item.BmpRun, JzTools.Resize(item.BmpRun.Size, -2));// item.BmpRun;
            //int iret = IxBarcode.Run();
            //if (iret == 0)
            //{
            //    Result = IxBarcode.BarcodeStr.Replace("BIN", "");
            //}
            //else
            //{
            //    Result = "";
            //}

        }
        private void readBarcodeFuntion1(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 4;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = new Bitmap(bmpSized);
            //Bitmap bmpSized = new Bitmap(bmpInput, Resize(bmpInput.Size, _iSized));
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmpSizedPre = grayscale.Apply(bmpSizedPre);
            AForge.Imaging.Filters.Erosion erosion = new AForge.Imaging.Filters.Erosion();
            bmpSizedPre = erosion.Apply(bmpSizedPre);
            bmpSizedPre = erosion.Apply(bmpSizedPre);
            bmpSizedPre = erosion.Apply(bmpSizedPre);
            Bitmap bmp = new Bitmap(bmpSizedPre);
            int penWidth = 1;
            Font MyFontX = new Font("Arial", 18);
            JetGrayImg grayimage = new JetGrayImg(bmp);
            JetImgproc.Threshold(grayimage, _iRange, grayimage);

            SolidBrush B = new SolidBrush(Color.Red);
            Font MyFont = new Font("Arial", 10);
            SolidBrush Bx = new SolidBrush(Color.Red);

            //grayimage.ToBitmap().Save("threshold.bmp");

            JetBlob jetBlob = new JetBlob();
            //这里需要调成参数设置
            jetBlob.Labeling(grayimage, JConnexity.Connexity4, JBlobLayer.BlackLayer);
            //if (cboFindMode.Text == "白底黑字")
            //    jetBlob.Labeling(grayimage, JConnexity.Connexity4, JBlobLayer.BlackLayer);
            //else
            //    jetBlob.Labeling(grayimage, JConnexity.Connexity4, JBlobLayer.WhiteLayer);
            int icount = jetBlob.BlobCount;
            watch.Stop();
            //this.Text = "用时: " + watch.ElapsedMilliseconds + " ms";// + " ms 共找到: " + icount + " 个斑点";
            Graphics g = Graphics.FromImage(bmp);
            float set_area_value = 8000;
            float set_area_valueMax = 100000;
            float set_ratio_value = 10;
            //参数设置
            //float.TryParse(txtArea.Text.Trim(), out set_area_value);
            //float.TryParse(txtAreaMax.Text.Trim(), out set_area_valueMax);
            //float.TryParse(txtRatio.Text.Trim(), out set_ratio_value);
            //int.TryParse(txtLineWidth.Text.Trim(), out penWidth);

            #endregion

            for (int i = 0; i < icount; i++)
            {
                int iArea = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.Area);
                JRotatedRectangleF jetrect = JetBlobFeature.ComputeMinRectangle(jetBlob, i);
                if (iArea > set_area_value && iArea < set_area_valueMax && IsInRangeRatio(jetrect.fWidth, jetrect.fHeight, set_ratio_value))
                {
                    #region 寻找二维码的角点
                    //int LeftMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.LeftMost);
                    //int RightMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.RightMost);
                    //int TopMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.TopMost);
                    //int BottomMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.BottomMost);
                    Point ptCenter = new Point((int)jetrect.fCX, (int)jetrect.fCY);

                    double iWidth = jetrect.fWidth;
                    double iHeight = jetrect.fHeight;
                    if (jetrect.fWidth < jetrect.fHeight)
                    {
                        iWidth = jetrect.fHeight;
                        iHeight = jetrect.fWidth;

                        jetrect.fAngle += 90;
                    }

                    Rectangle myRect = SimpleRect(ptCenter, (int)iWidth / 2, (int)iHeight / 2);
                    myRect.Inflate(15, 15);

                    //坐标系
                    Pen pAxis = new Pen(Color.Blue, 15);
                    pAxis.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    //x axis
                    //g.DrawLine(pAxis, new PointF(0, ptCenter.Y), new PointF(4912, ptCenter.Y));
                    //g.DrawLine(pAxis, new PointF(ptCenter.X, 0), new PointF(ptCenter.X, 3684));

                    //转换矩形的四个角
                    Point[] myPts = RectToPoint(myRect, -jetrect.fAngle);

                    //Point[] myPts = RectToPoint(myRect, 0);
                    Pen p = new Pen(Color.Lime, penWidth);
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;

                    Pen pBottom = new Pen(Color.Red, penWidth);
                    pBottom.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    g.DrawLine(p, myPts[0], myPts[1]);
                    g.DrawLine(p, myPts[0], myPts[2]);
                    g.DrawLine(p, myPts[1], myPts[3]);
                    g.DrawLine(pBottom, myPts[2], myPts[3]);

                    Point ptStart = GetCenterPoint(myPts[0], myPts[1]);
                    Point ptEnd = GetCenterPoint(myPts[2], myPts[3]);

                    Pen pRobot = new Pen(Color.Lime, penWidth);
                    //pRobot.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    if (myRect.Width <= myRect.Height + 3 && myRect.Width >= myRect.Height - 3)
                        g.DrawLine(pRobot, ptStart, ptEnd);
                    else
                    {
                        g.DrawLine(p, myPts[0], myPts[1]);
                        g.DrawLine(p, myPts[0], myPts[2]);
                        g.DrawLine(p, myPts[1], myPts[3]);
                        g.DrawLine(pBottom, myPts[2], myPts[3]);
                    }

                    #endregion

                    #region 转正二维码
                    //0 2
                    //1 3
                    OpenCvSharp.Point[] approx = new OpenCvSharp.Point[4];
                    approx[0] = new OpenCvSharp.Point(myPts[0].X, myPts[0].Y);
                    approx[1] = new OpenCvSharp.Point(myPts[2].X, myPts[2].Y);
                    approx[2] = new OpenCvSharp.Point(myPts[3].X, myPts[3].Y);
                    approx[3] = new OpenCvSharp.Point(myPts[1].X, myPts[1].Y);

                    ////排序
                    //Array.Sort(approx, (cs1, cs2) =>
                    //{
                    //    if (cs1 != null && cs1 != null)
                    //    {
                    //        if (cs1.Y > cs2.Y)
                    //            return 1;
                    //        else if (cs1.Y == cs2.Y)
                    //        {
                    //            if (cs1.X < cs2.X)
                    //                return 1;
                    //            else return -1;
                    //        }
                    //        else
                    //            return -1;
                    //    }
                    //    return 0;

                    //});

                    OpenCvSharp.Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpSized);
                    //算法找出的角点
                    OpenCvSharp.Point2f[] srcPt = new OpenCvSharp.Point2f[4];
                    srcPt[0] = approx[0];
                    srcPt[1] = approx[1];
                    srcPt[2] = approx[2];
                    srcPt[3] = approx[3];

                    //srcPt[0] = approx[3];
                    //srcPt[1] = approx[2];
                    //srcPt[2] = approx[0];
                    //srcPt[3] = approx[1];

                    //最小外接矩形
                    OpenCvSharp.RotatedRect rect = OpenCvSharp.Cv2.MinAreaRect(srcPt);
                    OpenCvSharp.Rect box = rect.BoundingRect();
                    OpenCvSharp.Point2f[] dstPt = new OpenCvSharp.Point2f[4];

                    dstPt[0].X = box.X;
                    dstPt[0].Y = box.Y;

                    dstPt[1].X = box.X + box.Width;
                    dstPt[1].Y = box.Y;

                    dstPt[2].X = box.X + box.Width;
                    dstPt[2].Y = box.Y + box.Height;

                    dstPt[3].X = box.X;
                    dstPt[3].Y = box.Y + box.Height;

                    OpenCvSharp.Mat src2 = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpSized);
                    OpenCvSharp.Mat final = new OpenCvSharp.Mat();
                    OpenCvSharp.Mat warpmatrix = OpenCvSharp.Cv2.GetPerspectiveTransform(srcPt, dstPt);//获得变换矩阵
                    OpenCvSharp.Cv2.WarpPerspective(src2, final, warpmatrix, src.Size());//投射变换，将结果赋给final

                    Bitmap temp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(final);
                    Bitmap temp1 = (Bitmap)CutImage(temp, (int)dstPt[0].X, (int)dstPt[0].Y, (int)dstPt[2].X, (int)dstPt[2].Y);
                    //if (jetrect.fAngle <= 90)
                    //    temp1.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    //else
                    //    temp1.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    #endregion

                    #region 读码

                    //IBarcode IxBarcode = new BarcodeGzx1Class();
                    //IxBarcode.InputImage = temp1;
                    //int iret = IxBarcode.Run();
                    //if (iret == 0)
                    //{
                    //    g.DrawString($"Type[{IxBarcode.TestType}] 解码:{IxBarcode.BarcodeStr}", MyFontX, Bx, new PointF(18, 18));
                    //    //temp1.Save(Application.StartupPath + $"\\WarpCollect\\{IxBarcode.BarcodeStr}_{i}.bmp",
                    //    //    System.Drawing.Imaging.ImageFormat.Bmp);
                    //    Result = IxBarcode.BarcodeStr.Replace("BIN", "");
                    //    Result = IxBarcode.BarcodeStr;
                    //    break;
                    //}
                    //else
                    //{
                    //    IxBarcode.InputImage = new Bitmap(temp1, temp1.Width / 2, temp1.Height / 2);
                    //    iret = IxBarcode.Run();
                    //    if (iret == 0)
                    //    {
                    //        g.DrawString($"2Type[{IxBarcode.TestType}] 解码:{IxBarcode.BarcodeStr}", MyFontX, Bx, new PointF(18, 18));
                    //        //temp1.Save(Application.StartupPath + $"\\WarpCollect\\{IxBarcode.BarcodeStr}_{i}.bmp",
                    //        //    System.Drawing.Imaging.ImageFormat.Bmp);
                    //        Result = IxBarcode.BarcodeStr.Replace("BIN", "");
                    //        Result = IxBarcode.BarcodeStr;
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        //temp1.Save(Application.StartupPath + $"\\WarpCollect\\NoneCode_{i}.bmp",
                    //        //    System.Drawing.Imaging.ImageFormat.Bmp);
                    //        g.DrawString($"2Type[{IxBarcode.TestType}] ", MyFontX, Bx, new PointF(18, 18));
                    //        Result = "";
                    //    }


                    //    ////temp1.Save(Application.StartupPath + $"\\WarpCollect\\NoneCode_{i}.bmp",
                    //    ////    System.Drawing.Imaging.ImageFormat.Bmp);
                    //    //g.DrawString($"Type[{IxBarcode.TestType}] ", MyFontX, Bx, new PointF(18, 18));
                    //    //Result = "";
                    //}
                    #endregion

                    temp.Dispose();
                    temp1.Dispose();
                }

            }
            watch.Stop();
            g.DrawString($"解码耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", MyFontX, Bx, new PointF(18, 38));
            g.Dispose();

            if (m_IsSaveDebugPicture)
                bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            bmpSized.Dispose();
            bmp.Dispose();


        }
        private void readBarcodeFuntion2(Bitmap bmpInput)
        {
            if (m_IsSaveDebugPicture)
                BmpRun.Save(DebugPath + "\\bmpRun_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            #region 参数设置及图像预处理
            int _iRange = 180;//blob灰阶值 
            int _iSized = 1;//缩图值
            Bitmap bmpSized = new Bitmap(bmpInput, bmpInput.Width / _iSized, bmpInput.Height / _iSized);
            Bitmap bmpSizedPre = new Bitmap(bmpSized);
            //Bitmap bmpSized = new Bitmap(bmpInput, Resize(bmpInput.Size, _iSized));
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmpSizedPre = grayscale.Apply(bmpSizedPre);
            AForge.Imaging.Filters.HistogramEqualization histogramEqualization = new AForge.Imaging.Filters.HistogramEqualization();
            bmpSizedPre = histogramEqualization.Apply(bmpSizedPre);
            AForge.Imaging.Filters.SISThreshold sISThreshold = new AForge.Imaging.Filters.SISThreshold();
            bmpSizedPre = sISThreshold.Apply(bmpSizedPre);
            AForge.Imaging.Filters.FillHoles fillHoles = new AForge.Imaging.Filters.FillHoles();
            bmpSizedPre = fillHoles.Apply(bmpSizedPre);
            //AForge.Imaging.Filters.Erosion erosion = new AForge.Imaging.Filters.Erosion();
            //bmpSizedPre = erosion.Apply(bmpSizedPre);
            //bmpSizedPre = erosion.Apply(bmpSizedPre);
            //bmpSizedPre = erosion.Apply(bmpSizedPre);
            Bitmap bmp = new Bitmap(bmpSizedPre);
            int penWidth = 1;
            Font MyFontX = new Font("Arial", 18);
            JetGrayImg grayimage = new JetGrayImg(bmp);
            JetImgproc.Threshold(grayimage, _iRange, grayimage);

            SolidBrush B = new SolidBrush(Color.Red);
            Font MyFont = new Font("Arial", 10);
            SolidBrush Bx = new SolidBrush(Color.Red);

            //grayimage.ToBitmap().Save("threshold.bmp");

            JetBlob jetBlob = new JetBlob();
            //这里需要调成参数设置
            jetBlob.Labeling(grayimage, JConnexity.Connexity4, JBlobLayer.WhiteLayer);
            //if (cboFindMode.Text == "白底黑字")
            //    jetBlob.Labeling(grayimage, JConnexity.Connexity4, JBlobLayer.BlackLayer);
            //else
            //    jetBlob.Labeling(grayimage, JConnexity.Connexity4, JBlobLayer.WhiteLayer);
            int icount = jetBlob.BlobCount;
            watch.Stop();
            //this.Text = "用时: " + watch.ElapsedMilliseconds + " ms";// + " ms 共找到: " + icount + " 个斑点";
            //Bitmap bmpOrgDraw = new Bitmap(BmpRun);
            Graphics g = Graphics.FromImage(bmp);
            float set_area_value = 50000;
            float set_area_valueMax = 120000;
            float set_ratio_value = 10;
            //参数设置
            //float.TryParse(txtArea.Text.Trim(), out set_area_value);
            //float.TryParse(txtAreaMax.Text.Trim(), out set_area_valueMax);
            //float.TryParse(txtRatio.Text.Trim(), out set_ratio_value);
            //int.TryParse(txtLineWidth.Text.Trim(), out penWidth);

            #endregion

            for (int i = 0; i < icount; i++)
            {
                int iArea = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.Area);
                JRotatedRectangleF jetrect = JetBlobFeature.ComputeMinRectangle(jetBlob, i);
                if (iArea > set_area_value && iArea < set_area_valueMax && IsInRangeRatio(jetrect.fWidth, jetrect.fHeight, set_ratio_value))
                {
                    #region 寻找二维码的角点
                    //int LeftMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.LeftMost);
                    //int RightMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.RightMost);
                    //int TopMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.TopMost);
                    //int BottomMost = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.BottomMost);
                    Point ptCenter = new Point((int)jetrect.fCX, (int)jetrect.fCY);

                    double iWidth = jetrect.fWidth;
                    double iHeight = jetrect.fHeight;
                    if (jetrect.fWidth < jetrect.fHeight)
                    {
                        iWidth = jetrect.fHeight;
                        iHeight = jetrect.fWidth;

                        jetrect.fAngle += 90;
                    }

                    Rectangle myRect = SimpleRect(ptCenter, (int)iWidth / 2, (int)iHeight / 2);
                    //myRect.Inflate(15, 15);

                    //坐标系
                    Pen pAxis = new Pen(Color.Blue, 15);
                    pAxis.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    //x axis
                    //g.DrawLine(pAxis, new PointF(0, ptCenter.Y), new PointF(4912, ptCenter.Y));
                    //g.DrawLine(pAxis, new PointF(ptCenter.X, 0), new PointF(ptCenter.X, 3684));

                    //转换矩形的四个角
                    Point[] myPts = RectToPoint(myRect, -jetrect.fAngle);

                    //Point[] myPts = RectToPoint(myRect, 0);
                    Pen p = new Pen(Color.Lime, penWidth);
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;

                    Pen pBottom = new Pen(Color.Red, penWidth);
                    pBottom.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    g.DrawLine(p, myPts[0], myPts[1]);
                    g.DrawLine(p, myPts[0], myPts[2]);
                    g.DrawLine(p, myPts[1], myPts[3]);
                    g.DrawLine(pBottom, myPts[2], myPts[3]);

                    g.DrawLine(p, myPts[0], myPts[3]);
                    g.DrawLine(p, myPts[1], myPts[2]);

                    //Point ptStart = GetCenterPoint(myPts[0], myPts[1]);
                    //Point ptEnd = GetCenterPoint(myPts[2], myPts[3]);

                    //Pen pRobot = new Pen(Color.Lime, penWidth);
                    //pRobot.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    //if (myRect.Width <= myRect.Height + 3 && myRect.Width >= myRect.Height - 3)
                    //    g.DrawLine(pRobot, ptStart, ptEnd);
                    //else
                    //{
                    //    g.DrawLine(p, myPts[0], myPts[1]);
                    //    g.DrawLine(p, myPts[0], myPts[2]);
                    //    g.DrawLine(p, myPts[1], myPts[3]);
                    //    g.DrawLine(pBottom, myPts[2], myPts[3]);
                    //}

                    #endregion
                }

            }
            watch.Stop();
            g.DrawString($"耗时:{watch.ElapsedMilliseconds.ToString("0.000")} ms", MyFontX, Bx, new PointF(18, 38));
            g.Dispose();

            if (m_IsSaveDebugPicture)
            {
                bmp.Save(DebugPath + "\\result_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                //bmpOrgDraw.Save(DebugPath + "\\result_draw_" + lblName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }


            bmpSized.Dispose();
            bmp.Dispose();
            //bmpOrgDraw.Dispose();

        }

#endif

        #endregion

        private Bitmap preProcessImage(Bitmap bmpinput)
        {
            Bitmap bmpSizedPre = (Bitmap)bmpinput.Clone();// new Bitmap(bmpinput);
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmpSizedPre = grayscale.Apply(bmpSizedPre);

            switch (m_ProcessImageMode)
            {
                case ProcessImageMode.V1:
                    AForge.Imaging.Filters.BradleyLocalThresholding bradleyLocalThresholding = new AForge.Imaging.Filters.BradleyLocalThresholding();
                    bmpSizedPre = bradleyLocalThresholding.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.FillHoles fillHoles = new AForge.Imaging.Filters.FillHoles();
                    fillHoles.MaxHoleHeight = 100;
                    fillHoles.MaxHoleWidth = 100;
                    fillHoles.CoupledSizeFiltering = false;
                    bmpSizedPre = fillHoles.Apply(bmpSizedPre);
                    break;
                case ProcessImageMode.V2:

                    //AForge.Imaging.Filters.Threshold threshold = new Threshold(PreThresholdValue);
                    //bmpSizedPre = threshold.Apply(bmpSizedPre);
                    //AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    //fillHoles2.MaxHoleHeight = 100;
                    //fillHoles2.MaxHoleWidth = 100;
                    //fillHoles2.CoupledSizeFiltering = false;
                    //bmpSizedPre = fillHoles2.Apply(bmpSizedPre);

                    break;
                case ProcessImageMode.V3:
                    AForge.Imaging.Filters.HistogramEqualization histogramEqualization = new HistogramEqualization();
                    bmpSizedPre = histogramEqualization.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.SISThreshold sISThreshold = new SISThreshold();
                    bmpSizedPre = sISThreshold.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.Closing closing = new Closing();
                    bmpSizedPre = closing.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    fillHoles2.MaxHoleHeight = 100;
                    fillHoles2.MaxHoleWidth = 100;
                    fillHoles2.CoupledSizeFiltering = false;
                    bmpSizedPre = fillHoles2.Apply(bmpSizedPre);

                    break;
            }

            return bmpSizedPre;
        }
        private LineClass getLineForPointF(PointF[] points1, bool swap = true, float pix = 0.5f)
        {
            QvLineFit jzLineFit = new QvLineFit();
            jzLineFit.Swap = swap;
            jzLineFit.LeastSquareFit(points1);


            PointF p1 = new PointF(points1[0].X, points1[0].Y);
            PointF p2 = new PointF(points1[points1.Length - 1].X, points1[points1.Length - 1].Y);

            if (swap)
            {
                if (jzLineFit.A != 0)
                    p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
            }
            else
            {
                if (jzLineFit.A != 0)
                    p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
            }

            //if (jzLineFit.A != 0)
            //    points1[0].X = (float)((double)points1[0].Y * jzLineFit.A + jzLineFit.B);

            //if (jzLineFit.A != 0)
            //    points1[points1.Length - 1].X = (float)((double)points1[points1.Length - 1].Y * jzLineFit.A + jzLineFit.B);

            LineClass lineClass = new LineClass(p1, p2);
            lineClass.IsSwap = swap;
            //if (swap)
            {
                //lineClass.IsSwap = true;
                List<PointF> lines = new List<PointF>();
                int i = 0;
                while (i < points1.Length)
                {
                    double xlen = jzLineFit.GetPointLength(points1[i]);
                    if (xlen < pix)
                    {
                        lines.Add(points1[i]);
                    }

                    i++;
                }

                if (lines.Count >= 2)
                {
                    jzLineFit.Swap = swap;
                    jzLineFit.LeastSquareFit(lines.ToArray());

                    p1 = lines[0];
                    p2 = lines[lines.Count - 1];

                    if (swap)
                    {
                        if (jzLineFit.A != 0)
                            p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
                    }
                    else
                    {
                        if (jzLineFit.A != 0)
                            p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
                    }

                    lineClass.IsSwap = swap;
                    lineClass = new LineClass(p1, p2);
                }
            }


            return lineClass;
        }
        private LineClass getLineForPointFBAK01(PointF[] points1, bool swap = true, float pix = 2)
        {
            QvLineFit jzLineFit = new QvLineFit();
            jzLineFit.Swap = swap;
            jzLineFit.LeastSquareFit(points1);

            PointF p1 = new PointF(points1[0].X, points1[0].Y);
            PointF p2 = new PointF(points1[points1.Length - 1].X, points1[points1.Length - 1].Y);

            if (swap)
            {
                if (jzLineFit.A != 0)
                    p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
            }
            else
            {
                if (jzLineFit.A != 0)
                    p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
            }

            //if (jzLineFit.A != 0)
            //    points1[0].X = (float)((double)points1[0].Y * jzLineFit.A + jzLineFit.B);

            //if (jzLineFit.A != 0)
            //    points1[points1.Length - 1].X = (float)((double)points1[points1.Length - 1].Y * jzLineFit.A + jzLineFit.B);

            LineClass lineClass = new LineClass(p1, p2);
            if (swap)
            {
                lineClass.IsSwap = true;
                List<PointF> lines = new List<PointF>();
                int i = 0;
                while (i < points1.Length)
                {
                    double xlen = lineClass.GetVerticalLength(points1[i]);
                    if (xlen < pix)
                    {
                        lines.Add(points1[i]);
                    }

                    i++;
                }

                if (lines.Count >= 2)
                {
                    jzLineFit.Swap = swap;
                    jzLineFit.LeastSquareFit(lines.ToArray());

                    p1 = lines[0];
                    p2 = lines[lines.Count - 1];

                    if (swap)
                    {
                        if (jzLineFit.A != 0)
                            p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
                    }
                    else
                    {
                        if (jzLineFit.A != 0)
                            p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
                    }

                    lineClass = new LineClass(p1, p2);
                }
            }


            return lineClass;
        }



        Size Resize(Size OrgSize, int Ratio)
        {
            Size retSize;

            if (Ratio > 0)
                retSize = new Size(OrgSize.Width << Ratio, OrgSize.Height << Ratio);
            else
                retSize = new Size(OrgSize.Width >> -Ratio, OrgSize.Height >> -Ratio);

            retSize.Width = Math.Max(retSize.Width, 1);
            retSize.Height = Math.Max(retSize.Height, 1);

            return retSize;
        }
        bool IsInRangeRatio(double FromValue, double CompValue, double Ratio)
        {
            return (FromValue >= (CompValue * (1 - (Ratio / 100d)))) && (FromValue <= (CompValue * (1 + (Ratio / 100d))));
        }
        /// <summary>  
        /// 对一个坐标点按照一个中心进行旋转  
        /// </summary>  
        /// <param name="center">中心点</param>  
        /// <param name="p1">要旋转的点</param>  
        /// <param name="angle">旋转角度，笛卡尔直角坐标</param>  
        /// <returns></returns>  
        Point PointRotate(Point center, Point p1, double angle)
        {
            Point tmp = new Point();
            double angleHude = angle * Math.PI / 180;/*角度变成弧度*/
            double x1 = (p1.X - center.X) * Math.Cos(angleHude) + (p1.Y - center.Y) * Math.Sin(angleHude) + center.X;
            double y1 = -(p1.X - center.X) * Math.Sin(angleHude) + (p1.Y - center.Y) * Math.Cos(angleHude) + center.Y;
            tmp.X = (int)x1;
            tmp.Y = (int)y1;
            return tmp;
        }

        Rectangle SimpleRect(Point Pt, int Width, int Height)
        {
            Rectangle rect = SimpleRect(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        Rectangle SimpleRect(Point Pt)
        {
            return new Rectangle(Pt.X, Pt.Y, 1, 1);
        }
        RectangleF SimpleRectF(PointF Pt, int Width, int Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt, float Width, float Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt)
        {
            return new RectangleF(Pt.X, Pt.Y, 1, 1);
        }

        Point[] RectToPoint(Rectangle xRect, double xAngle)
        {
            Point[] pts = new Point[4];

            Point ptCenter = GetRectCenter(xRect);
            pts[0] = xRect.Location;
            pts[1] = new Point(xRect.Location.X, xRect.Bottom);
            pts[2] = new Point(xRect.Right, xRect.Location.Y);
            pts[3] = new Point(xRect.Right, xRect.Bottom);

            pts[0] = PointRotate(ptCenter, pts[0], xAngle);
            pts[1] = PointRotate(ptCenter, pts[1], xAngle);
            pts[2] = PointRotate(ptCenter, pts[2], xAngle);
            pts[3] = PointRotate(ptCenter, pts[3], xAngle);

            return pts;
        }
        Point GetRectCenter(Rectangle Rect)
        {
            return new Point(Rect.X + (Rect.Width >> 1), Rect.Y + (Rect.Height >> 1));
        }
        Point GetCenterPoint(Point P1, Point P2)
        {
            return new Point((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
        }
        /// <summary>
        /// 剪裁图片
        /// </summary>
        /// <param name="src">原图片</param>
        /// <param name="left">左坐标</param>
        /// <param name="top">顶部坐标</param>
        /// <param name="right">右坐标</param>
        /// <param name="bottom">底部坐标</param>
        /// <returns>剪裁后的图片</returns>
        Image CutImage(Image src, int left, int top, int right, int bottom)
        {
            Bitmap srcBitmap = new Bitmap(src);
            int width = right - left;
            int height = bottom - top;
            Bitmap destBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(destBitmap))
            {
                g.Clear(Color.Transparent);
                //设置画布的描绘质量         
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(srcBitmap, new Rectangle(0, 0, width, height), left, top, width, height, GraphicsUnit.Pixel);
            }
            return destBitmap;
        }

        object obj = new object();
        private int GetLineInYDir(Bitmap bmp, bool IsReverse, int FromY, int XLocation, Color StopColor)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
                BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                //try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)(void*)Scan0;
                        byte* pucPtr;
                        byte* pucStart;

                        int xmin = rectbmp.X;
                        int ymin = rectbmp.Y;
                        int xmax = xmin + rectbmp.Width;
                        int ymax = ymin + rectbmp.Height;

                        int x = XLocation;
                        int y = ymin;
                        int iStride = bmpData.Stride;

                        y = FromY;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        pucStart[0] = (byte)(255 - StopColor.R);
                        pucStart[1] = (byte)(255 - StopColor.G);
                        pucStart[2] = (byte)(255 - StopColor.B);

                        if (!IsReverse)
                        {
                            while (y < ymax - 1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    //y--;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart += iStride;

                                y++;
                            }
                        }
                        else
                        {
                            while (y > 0)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    y++;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart -= iStride;

                                y--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return y;
                    }
                }
                //catch (Exception e)
                //{
                //    bmp.UnlockBits(bmpData);

                //    //if (IsDebug)
                //    //    MessageBox.Show("Error :" + e.ToString());

                //    return -1;
                //}
            }
        }
        private int GetLineInXDir(Bitmap bmp, bool IsReverse, int FromX, int YLocation, Color StopColor)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
                BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                //try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)(void*)Scan0;
                        byte* pucPtr;
                        byte* pucStart;

                        int xmin = rectbmp.X;
                        int ymin = rectbmp.Y;
                        int xmax = xmin + rectbmp.Width;
                        int ymax = ymin + rectbmp.Height;

                        int x = xmin;
                        int y = YLocation;
                        int iStride = bmpData.Stride;

                        x = FromX;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        pucStart[0] = (byte)(255 - StopColor.R);
                        pucStart[1] = (byte)(255 - StopColor.G);
                        pucStart[2] = (byte)(255 - StopColor.B);

                        if (!IsReverse)
                        {
                            while (x < xmax - 1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    //x--;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;

                                }

                                pucStart += 4;

                                x++;
                            }
                        }
                        else
                        {
                            while (x > 0)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    x++;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart -= 4;

                                x--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return x;
                    }
                }
                //catch (Exception e)
                //{
                //    bmp.UnlockBits(bmpData);

                //    //if (IsDebug)
                //    //    MessageBox.Show("Error :" + e.ToString());

                //    return -1;
                //}
            }
        }
        Rectangle SimpleRect(Size Sz)
        {
            return new Rectangle(0, 0, Sz.Width, Sz.Height);
        }

        public void BoundRect(ref Rectangle InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public int BoundValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }

    }
    public class AnalyzeClass
    {
        private string m_ReadBarcodeStr = string.Empty;
        public string ReadBarcodeStr
        {
            get { return m_ReadBarcodeStr; }
            set { m_ReadBarcodeStr = value; }
        }

        private string m_ChipClassDataFilePath = string.Empty;
        private string m_ChipClassBarcode = string.Empty;
        public string ChipClassDataFilePath
        {
            get { return m_ChipClassDataFilePath; }
            set { m_ChipClassDataFilePath = value; }
        }
        public string ChipClassBarcode
        {
            get { return m_ChipClassBarcode; }
            set { m_ChipClassBarcode = value; }
        }

        private bool _saveDebugPicture = false;
        /// <summary>
        /// 保存测试过程中的图片
        /// </summary>
        public bool SaveDebugPicture
        {
            get { return _saveDebugPicture; }
            set { _saveDebugPicture = value; }
        }

        private int _index = 0;
        private string _name = "";
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _saveFileName = string.Empty;
        public string SaveFileName
        {
            get { return _saveFileName; }
            set { _saveFileName = value; }
        }
        private string m_path = "";
        public string myPath
        {
            get { return m_path; }
            set { m_path = value; }
        }
        PropGrid_CaliClass propGrid_CaliClass
        {
            get { return Universal.CaliClass; }
        }
        RecipeMiniX6Class myRecipe
        {
            get { return RecipeMiniX6Class.Instance; }
        }
        CommonLogClass logInfo
        {
            get { return Universal.COMMON_LOG_INFOS; }
        }

        public List<RegionCellClass> myListCell = new List<RegionCellClass>();

        private float _rowoffset = 1;
        public float Rowoffset { get { return _rowoffset; } }
        private float _coloffset = 1;
        public float Coloffset { get { return _coloffset; } }

        public int Row = 1;
        public int Column = 1;
        public RectangleF RectFStart = new RectangleF();
        public RectangleF RectFEnd = new RectangleF();

        string m_format = "0.000000";

        private int _row = 1;
        private int _col = 1;

        public void CreateRowCol(bool istrain = false)
        {
            if (myListCell.Count > 0)
            {
                Parallel.ForEach(myListCell, (_cell) =>
                {
                    _cell.Dispose();
                });
            }

            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();

            myListCell.Clear();
            if (Column > 1)
                _coloffset = (RectFEnd.X - RectFStart.X) / (Column - 1);
            else
                _coloffset = 0;
            if (Row > 1)
                _rowoffset = (RectFEnd.Y - RectFStart.Y) / (Row - 1);
            else
                _rowoffset = 0;
            int iindextmp = 0;
            switch (myRecipe.PreLaserPos)
            {
                case LaserStartPos.LeftTop:

                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = 0; j < Column; j++)
                        {
                            RegionCellClass _cell = new RegionCellClass();
                            _cell.Index = iindextmp;
                            _cell.CellRow = i;
                            _cell.CellCol = j;
                            _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                            _cell.viewRectF = new RectangleF(RectFStart.X + j * _coloffset, RectFStart.Y + i * _rowoffset, RectFStart.Width, RectFStart.Height);
                            myListCell.Add(_cell);
                            iindextmp++;

                            _cell.PreIsOpenAuFind = myRecipe.IsOpenAuFind;
                            _cell.paraClass.tolerance = myRecipe.inspect_tolerance;
                            _cell.paraClass.angle = myRecipe.inspect_angle;
                            _cell.paraClass.samplesize = myRecipe.inspect_samplesize;

                            _cell.inspect_dir_open = myRecipe.inspect_dir_open;
                            _cell.paraClass_dir.tolerance = myRecipe.inspect_dir_tolerance;
                            _cell.paraClass_dir.angle = myRecipe.inspect_dir_angle;
                            _cell.paraClass_dir.samplesize = myRecipe.inspect_dir_samplesize;

                            if (istrain)
                            {
                                _cell.SetTrainImage(myRecipe.bmpORGPattern);
                                _cell.SetTrainDirImage(myRecipe.bmpOrgDirPattern);

                                //_cell.Train(myRecipe.bmpORGPattern);
                                //_cell.TrainDir(myRecipe.bmpOrgDirPattern);
                            }

                        }
                    }

                    break;
                case LaserStartPos.LeftBottom:

                    for (int i = Row; i > 0; i--)
                    {
                        for (int j = 0; j < Column; j++)
                        {
                            RegionCellClass _cell = new RegionCellClass();
                            _cell.Index = iindextmp;
                            _cell.CellRow = i;
                            _cell.CellCol = j;
                            _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                            _cell.viewRectF = new RectangleF(RectFStart.X + j * _coloffset, RectFStart.Y + (Row) * _rowoffset - i * _rowoffset, RectFStart.Width, RectFStart.Height);
                            myListCell.Add(_cell);
                            iindextmp++;

                            _cell.PreIsOpenAuFind = myRecipe.IsOpenAuFind;
                            _cell.paraClass.tolerance = myRecipe.inspect_tolerance;
                            _cell.paraClass.angle = myRecipe.inspect_angle;
                            _cell.paraClass.samplesize = myRecipe.inspect_samplesize;

                            _cell.inspect_dir_open = myRecipe.inspect_dir_open;
                            _cell.paraClass_dir.tolerance = myRecipe.inspect_dir_tolerance;
                            _cell.paraClass_dir.angle = myRecipe.inspect_dir_angle;
                            _cell.paraClass_dir.samplesize = myRecipe.inspect_dir_samplesize;

                            if (istrain)
                            {
                                _cell.SetTrainImage(myRecipe.bmpORGPattern);
                                _cell.SetTrainDirImage(myRecipe.bmpOrgDirPattern);

                                //_cell.Train(myRecipe.bmpORGPattern);
                                //_cell.TrainDir(myRecipe.bmpOrgDirPattern);
                            }
                        }
                    }

                    break;
                case LaserStartPos.RightTop:

                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = Column; j > 0; j--)
                        {
                            RegionCellClass _cell = new RegionCellClass();
                            _cell.Index = iindextmp;
                            _cell.CellRow = i;
                            _cell.CellCol = j;
                            _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                            _cell.viewRectF = new RectangleF(RectFStart.X + Column * _coloffset - j * _coloffset, RectFStart.Y + i * _rowoffset, RectFStart.Width, RectFStart.Height);
                            myListCell.Add(_cell);
                            iindextmp++;

                            _cell.PreIsOpenAuFind = myRecipe.IsOpenAuFind;
                            _cell.paraClass.tolerance = myRecipe.inspect_tolerance;
                            _cell.paraClass.angle = myRecipe.inspect_angle;
                            _cell.paraClass.samplesize = myRecipe.inspect_samplesize;

                            _cell.inspect_dir_open = myRecipe.inspect_dir_open;
                            _cell.paraClass_dir.tolerance = myRecipe.inspect_dir_tolerance;
                            _cell.paraClass_dir.angle = myRecipe.inspect_dir_angle;
                            _cell.paraClass_dir.samplesize = myRecipe.inspect_dir_samplesize;

                            if (istrain)
                            {
                                _cell.SetTrainImage(myRecipe.bmpORGPattern);
                                _cell.SetTrainDirImage(myRecipe.bmpOrgDirPattern);

                                //_cell.Train(myRecipe.bmpORGPattern);
                                //_cell.TrainDir(myRecipe.bmpOrgDirPattern);
                            }
                        }
                    }

                    break;
                case LaserStartPos.RightBottom:

                    for (int i = Row; i > 0; i--)
                    {
                        for (int j = Column; j > 0; j--)
                        {
                            RegionCellClass _cell = new RegionCellClass();
                            _cell.Index = iindextmp;
                            _cell.CellRow = i;
                            _cell.CellCol = j;
                            _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                            _cell.viewRectF = new RectangleF(RectFStart.X + Column * _coloffset - j * _coloffset, RectFStart.Y + (Row) * _rowoffset - i * _rowoffset, RectFStart.Width, RectFStart.Height);
                            myListCell.Add(_cell);
                            iindextmp++;

                            _cell.PreIsOpenAuFind = myRecipe.IsOpenAuFind;
                            _cell.paraClass.tolerance = myRecipe.inspect_tolerance;
                            _cell.paraClass.angle = myRecipe.inspect_angle;
                            _cell.paraClass.samplesize = myRecipe.inspect_samplesize;

                            _cell.inspect_dir_open = myRecipe.inspect_dir_open;
                            _cell.paraClass_dir.tolerance = myRecipe.inspect_dir_tolerance;
                            _cell.paraClass_dir.angle = myRecipe.inspect_dir_angle;
                            _cell.paraClass_dir.samplesize = myRecipe.inspect_dir_samplesize;

                            if (istrain)
                            {
                                _cell.SetTrainImage(myRecipe.bmpORGPattern);
                                _cell.SetTrainDirImage(myRecipe.bmpOrgDirPattern);

                                //_cell.Train(myRecipe.bmpORGPattern);
                                //_cell.TrainDir(myRecipe.bmpOrgDirPattern);
                            }
                        }
                    }

                    break;
            }

            //训练
            if (istrain)
            {
                //foreach (var _cell in myListCell)
                //{
                //    _cell.Train(myRecipe.bmpORGPattern);
                //    _cell.TrainDir(myRecipe.bmpOrgDirPattern);
                //}


                Parallel.ForEach(myListCell, (_cell) =>
                {
                    _cell.Train();
                    _cell.TrainDir();

                    //_cell.Train(myRecipe.bmpORGPattern);
                    //_cell.TrainDir(myRecipe.bmpOrgDirPattern);
                });

                GC.Collect();
                GC.Collect();
                GC.Collect();
                GC.Collect();
            }


        }
        public void CreateRowCol2()
        {
            myListCell.Clear();
            _coloffset = 0;// (RectFEnd.X - RectFStart.X) / (Column - 1);
            _rowoffset = (RectFEnd.Y - RectFStart.Y) / (Row - 1);
            //myChipArray = new BSA.ControlSpace.ChipClass[Row * Column];
            int iindextmp = 0;
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Column; j++)
                {
                    //myChipArray[iindextmp] = new BSA.ControlSpace.ChipClass();
                    //myChipArray[iindextmp].ORGTrayNo = "C1T" + Index.ToString("00");

                    RegionCellClass _cell = new RegionCellClass();
                    _cell.Index = iindextmp;
                    _cell.CellRow = i;
                    _cell.CellCol = j;
                    _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                    _cell.viewRectF = new RectangleF(RectFStart.X + j * _coloffset, RectFStart.Y + i * _rowoffset, RectFStart.Width, RectFStart.Height);
                    myListCell.Add(_cell);
                    iindextmp++;
                }
            }
        }
        public void CreateRowCol(int row, int col, bool istrain = false)
        {
            Row = row;
            Column = col;
            CreateRowCol(istrain);
        }
        public void SetCellResult(int irow, int icol, int debugindex = 0)
        {
            _row = irow;
            _col = icol;

            myListCell.Clear();
            int iindextmp = 0;
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _col; j++)
                {
                    RegionCellClass _cell = new RegionCellClass();
                    _cell.Index = iindextmp;

                    switch (debugindex)
                    {
                        case 0:
                            if (j < 2)
                                _cell.Result = "0";
                            else if (j == 2)
                                _cell.Result = "1";
                            else
                                _cell.Result = "2";
                            break;
                        default:
                            _cell.Result = "";
                            break;
                    }

                    myListCell.Add(_cell);
                    iindextmp++;
                }
            }

            int[] vs1 = getRandomNum((int)(_row * _col * 0.75), 0, _row * _col - 1);
            List<int> vs = vs1.ToList();
            foreach (int i in vs)
            {
                myListCell[i].Result = "0";
            }
            int jx = 0;
            List<int> vs2 = new List<int>();
            while (jx < _row * _col)
            {
                if (!vs.Contains(jx))
                {
                    vs2.Add(jx);
                }
                jx++;
            }

            jx = 0;
            foreach (int i in vs2)
            {
                if (jx < vs2.Count / 2)
                {
                    myListCell[i].Result = "1";
                }
                else
                {
                    myListCell[i].Result = "2";
                }
                jx++;
            }
        }
        public int[] getRandomNum(int num, int minValue, int maxValue)
        {
            if ((maxValue + 1 - minValue - num < 0))
                maxValue += num - (maxValue + 1 - minValue);
            Random ra = new Random(unchecked((int)DateTime.Now.Ticks));
            int[] arrNum = new int[num];
            int tmp = 0;
            StringBuilder sb = new StringBuilder(num * maxValue.ToString().Trim().Length);

            for (int i = 0; i <= num - 1; i++)
            {
                tmp = ra.Next(minValue, maxValue);
                while (sb.ToString().Contains("#" + tmp.ToString().Trim() + "#"))
                    tmp = ra.Next(minValue, maxValue + 1);
                arrNum[i] = tmp;
                sb.Append("#" + tmp.ToString().Trim() + "#");
            }
            return arrNum;
        }

        public FreeImageBitmap freeImageBitmapInput = null;
        public FreeImageBitmap freeImageBitmapOutput = null;
        public Bitmap freeImageBitmapOutputDraw = new Bitmap(1, 1);

        public void Dispose()
        {
            if (freeImageBitmapInput != null)
                freeImageBitmapInput.Dispose();

            if (freeImageBitmapOutput != null)
                freeImageBitmapOutput.Dispose();

            if (freeImageBitmapOutputDraw != null)
                freeImageBitmapOutputDraw.Dispose();

            //foreach (RegionCellClass regionCell in myListCell)
            //{
            //    regionCell.Dispose();
            //}
            //myListCell.Clear();
        }

        JzFindObjectClass m_Find = new JzFindObjectClass();
        JzToolsClass m_Tools = new JzToolsClass();

        private Bitmap m_InputImage = null;
        public Bitmap[] InputImages = null;
        public Bitmap[] OutputImages = null;
        private long m_ElapsedTime = 0;
        private bool m_Running = false;
        //private string m_BarcodeStr = string.Empty;
        //private long m_ElapsedTime = 0;
        private Bitmap m_OutputImage = null;
        private int m_CutHeight = 19000 - 800;
        private bool m_IsDrawRect = false;

        private bool m_IsPass = false;
        private string m_ResultDesc = string.Empty;
        //private InspectMode m_InspectMode = InspectMode.CHIP;

        ////private Bitmap m_InputImage = null;
        //public InspectMode InspectModex
        //{
        //    get { return m_InspectMode; }
        //    set { m_InspectMode = value; }
        //}
        private List<ShowRectClass> m_ShowRectList = new List<ShowRectClass>();
        public List<ShowRectClass> ShowRectList
        {
            get { return m_ShowRectList; }
        }
        public bool IsDrawRect
        {
            get { return m_IsDrawRect; }
            set { m_IsDrawRect = value; }
        }
        public int CutHeight
        {
            get { return m_CutHeight; }
            set { m_CutHeight = value; }
        }
        //public Bitmap[] InputImage
        //{
        //    get { return m_InputImage; }
        //    set
        //    {
        //        m_InputImage = value;
        //    }
        //}
        public Bitmap InputImage
        {
            get { return m_InputImage; }
            set
            {
                m_InputImage = value;
            }
        }
        public Bitmap OutputImage
        {
            get { return m_OutputImage; }
            //set
            //{
            //    m_InputImage = value;
            //}
        }
        public long ElapsedTime
        {
            get { return m_ElapsedTime; }
        }
        public bool Running
        {
            get { return m_Running; }
        }
        public bool IsPass
        {
            get { return m_IsPass; }
            //set { m_IsPass = value; }
        }
        public string ResultDesc
        {
            get { return m_ResultDesc; }
        }
        public void Run()
        {
            m_IsPass = true;
            m_ResultDesc = string.Empty;

            if (!myRecipe.Ischip_open_measure)
                return;

            System.Threading.Thread thread = new System.Threading.Thread(runTest);
            thread.IsBackground = true;
            thread.Start();

            //runTest2_freeImage();
        }
        //开辟一个测试校正的RUN
        public void RunCali()
        {
            m_IsPass = true;
            m_ResultDesc = string.Empty;

            System.Threading.Thread thread = new System.Threading.Thread(runTestCali);
            thread.IsBackground = true;
            thread.Start();

            //runTest2_freeImage();
        }
        private void runTestCali()
        {
            _saveDebugPicture = INI.Instance.IsSaveTestImage;
            //InitTrayUI();

            int iSize = 1;

            //freeImageBitmapOutput = new FreeImageBitmap(freeImageBitmapInput);

            //return;
            m_ElapsedTime = 0;
            m_Running = true;
            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            float updownoffset = propGrid_CaliClass.sort_offset;//方框上下波動範圍

            Bitmap m_bmpOpeateTest = freeImageBitmapInput.ToBitmap();

            int irow = 0;
            int icol = 0;

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            #region MARK点位置
            if (propGrid_CaliClass.use_mark_pointf)
            {
                Bitmap bmpinputx = (Bitmap)m_bmpOpeateTest.Clone(INI.Instance.mark_rect, PixelFormat.Format24bppRgb);
                m_Find.AH_SetThreshold(ref bmpinputx, INI.Instance.mark_thresholdvalue);
                m_Find.AH_FindBlob(bmpinputx, true);

                if (m_Find.FoundList.Count > 0)
                {
                    //Rectangle rectmax = m_Find.rectMaxRect;
                    Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + INI.Instance.mark_rect.X,
                       m_Find.rectMaxRect.Y + INI.Instance.mark_rect.Y,
                       m_Find.rectMaxRect.Width,
                       m_Find.rectMaxRect.Height);

                    Point MarkRunPtCenter = GetRectCenter(rectmax);

                    _markOffset = new PointF(MarkRunPtCenter.X - INI.Instance.mark_org.X, MarkRunPtCenter.Y - INI.Instance.mark_org.Y);
                }
                bmpinputx.Dispose();

                //_markOffset = new PointF(0, 0);
            }
            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"计算MARK点位置{myElapsedTime} ms");

            string _path = Universal.DEBUGRESULTPATH + "\\" + _index.ToString() + "-" + JzTimes.DateTimeSerialStringFFF + "";
            _path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + "-" + _saveFileName + "";
            //if (_saveDebugPicture)
            {
                if (!System.IO.Directory.Exists(_path))
                    System.IO.Directory.CreateDirectory(_path);
            }

            Rectangle rectx = new Rectangle((int)propGrid_CaliClass.autoFindRegion.X,
                (int)propGrid_CaliClass.autoFindRegion.Y,
                (int)propGrid_CaliClass.autoFindRegion.Width,
                (int)propGrid_CaliClass.autoFindRegion.Height);

            Bitmap bmpinput = (Bitmap)m_bmpOpeateTest.Clone(rectx, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int iSized = propGrid_CaliClass.samplingvalue;
            Bitmap bmpinputtemp = new Bitmap(bmpinput, new Size(bmpinput.Width / iSized, bmpinput.Height / iSized));
            m_Find.AH_SetThreshold(ref bmpinputtemp, propGrid_CaliClass.threshold_value);
            m_Find.AH_FindBlob(bmpinputtemp, true);
            //m_Find.AH_SetThreshold(ref bmpinput, propGrid_CaliClass.threshold_value);
            //m_Find.AH_FindBlob(bmpinput, true);

            List<Rectangle> boxes = new List<Rectangle>();
            List<MSRItemClass> items = new List<MSRItemClass>();
            foreach (FoundClass found in m_Find.FoundList)
            {
                found.Area *= iSized;
                found.rect = ResizeWithLocation3(found.rect, iSized);

                if (found.Area > propGrid_CaliClass.area_min && found.Area < propGrid_CaliClass.area_max
                    && found.rect.Width < propGrid_CaliClass.width_max && found.rect.Width > propGrid_CaliClass.width_min
                    && found.rect.Height < propGrid_CaliClass.height_max && found.rect.Height > propGrid_CaliClass.height_min
                    )
                {
                    Rectangle rx = new Rectangle(found.rect.X + rectx.X, found.rect.Y + rectx.Y, found.rect.Width, found.rect.Height);

                    //if(rx.X < 10000 || rx.X > 18000)
                    {
                        MSRItemClass mSRItemClass = new MSRItemClass();

                        boxes.Add(rx);
                        mSRItemClass.CenterPointF = GetRectCenter(rx);
                        mSRItemClass.Bounds = rx;
                        PointF ptfworld = new PointF(mSRItemClass.CenterPointF.X, mSRItemClass.CenterPointF.Y);

                        PointF ptfViewAddOffset = new PointF(mSRItemClass.CenterPointF.X - _markOffset.X,
                                                                                      mSRItemClass.CenterPointF.Y - _markOffset.Y);

                        ptfworld = ToWorld(ptfViewAddOffset);

                        ptfworld.Y = -ptfworld.Y;

                        mSRItemClass.RelatePointF = new PointF(ptfworld.X, ptfworld.Y);
                        mSRItemClass.RelatePointFWorldToView = ToView(ptfworld, new PointF(0, 0));

                        items.Add(mSRItemClass);
                    }
                }

            }

            #region 排序数据

            List<MSRItemClass> BranchList = items;

            int Highest = 100000;
            int HighestIndex = -1;
            int ReportIndex = 0;
            List<string> CheckList = new List<string>();

            int i = 0;
            int j = 0;
            //Clear All Index To 0 and Check the Highest

            foreach (MSRItemClass keyassign in BranchList)
            {

                keyassign.ReportRowCol = "";
                keyassign.ReportIndex = 0;
                ReportIndex = 1;

            }

            i = 0;
            while (true)
            {
                i = 0;
                Highest = 100000;
                HighestIndex = -1;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (keyassign.CenterPointF.Y < Highest && keyassign.RowTag == 0)
                        {
                            Highest = (int)keyassign.CenterPointF.Y;
                            HighestIndex = i;
                        }
                    }

                    i++;
                }

                if (HighestIndex == -1)
                    break;

                CheckList.Clear();

                //把相同位置的人找出來
                i = 0;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (IsInRange((int)keyassign.CenterPointF.Y, Highest, updownoffset))
                        {
                            CheckList.Add(keyassign.CenterPointF.X.ToString("00000000") + "," + i.ToString());
                        }
                    }
                    i++;
                }


                //if (j > 0 && j % 2 == 0)
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });
                ////CheckList.Sort();
                //else
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });
                ////CheckList.Sort();

                ////从大到小排序
                //CheckList.Sort((item1, item2) =>
                //{ return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });

                //从小到大排序
                CheckList.Sort((item1, item2) =>
                { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });

                i = 1;
                foreach (string Str in CheckList)
                {
                    string[] Strs = Str.Split(',');

                    BranchList[int.Parse(Strs[1])].ReportIndex = ReportIndex;
                    BranchList[int.Parse(Strs[1])].ReportRowCol = (j + 1).ToString() + "-" + i.ToString();
                    //BranchList[int.Parse(Strs[1])].ReportRowCol = CheckList.Count.ToString() + "-" + i.ToString();

                    ReportIndex++;
                    i++;
                }

                j++;

                irow = j;
                icol = i - 1;
            }

            //从小到大排序
            BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? 1 : -1; });

            //从大到小排序
            //BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? -1 : 1; });

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"测试计算{myElapsedTime} ms");

            freeImageBitmapOutputDraw.Dispose();
            freeImageBitmapOutputDraw = new Bitmap(m_bmpOpeateTest);
            Graphics g = Graphics.FromImage(freeImageBitmapOutputDraw);
            if (boxes.Count > 0)
                g.DrawRectangles(new Pen(Color.Red, 11), boxes.ToArray());
            SolidBrush B = new SolidBrush(Color.Lime);
            Font MyFont1 = new Font("Arial", 100);
            string reportstr = ",,,CXV,CYV,RXV,RYV,RW,RH,,CXW,CYW" + Environment.NewLine;
            int indexreport = 1;
            foreach (var item in items)
            {
                g.DrawString(item.ToDrawString(true), new Font("宋体", 20), Brushes.Lime, item.CenterPointF);
                reportstr += $"{item.ReportIndex},{item.ToReportString()},{Environment.NewLine}";
                indexreport++;
            }
            g.DrawString(stopwatch.ElapsedMilliseconds.ToString("0.000") + " ms", MyFont1, B, new PointF(5, 5));
            g.Dispose();
            g.Dispose();

            //bmpinputdraw.Save("D:\\report\\Img_" + JzTimes.DateTimeSerialStringFFF + ".png", System.Drawing.Imaging.ImageFormat.Png);
            //DS.ReplaceDisplayImage(bmpinputdraw);
            bmpinput.Dispose();

            string _outputimage_path = myPath.Replace(".cfg", "-" + Index.ToString("000") + ".bmp");
            _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{Index.ToString()}_" + _saveFileName + ".jpg";

            FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

            Task task = new Task(() =>
            {
                try
                {
                    if (INI.Instance.IsSaveStripImage)
                    {
                        SaveImageWithQuality(freeImageBitmapInputResult.ToBitmap(), _outputimage_path, INI.Instance.ImageQuality);
                        //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                        //freeImageBitmapOutputDraw.Dispose();
                        freeImageBitmapInputResult.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    logInfo.Log($"保存图片错误{ex.Message} ");
                }
            });
            task.Start();

            string _laserStr_path = $"{_path}\\autoCali_{JzTimes.DateTimeSerialStringFFF}.csv";
            SaveData(reportstr, _laserStr_path);

            #region 保存xml 供镭雕机调用

            if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
            {
                if (!Directory.Exists(INI.Instance.LaserSharePath))
                    Directory.CreateDirectory(INI.Instance.LaserSharePath);
                if (Directory.Exists(INI.Instance.LaserSharePath))
                {
                    XmlDocument xmldoc;
                    XmlNode xmlnode;
                    XmlElement xmlelem;

                    xmldoc = new XmlDocument();
                    XmlDeclaration xmldecl;
                    xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                    xmldoc.AppendChild(xmldecl);

                    //加入一个根元素
                    xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                    xmldoc.AppendChild(xmlelem);

                    XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                    xmlelem.AppendChild(xeMatrix);

                    XmlElement x2 = xmldoc.CreateElement("Center");
                    x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                    x2.SetAttribute("y", (0).ToString());
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Angle");
                    x2.InnerText = "0";
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Row");
                    x2.InnerText = irow.ToString();
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Col");
                    x2.InnerText = icol.ToString();
                    xeMatrix.AppendChild(x2);

                    XmlNode xeCells = xmldoc.CreateElement("Cells");
                    xmlelem.AppendChild(xeCells);

                    i = 0;
                    foreach (var itemClass in items)
                    {
                        float fx = itemClass.RelatePointF.X;// itemClass.xPCenterOffsetWorld.X;// + (int)numericUpDown1.Value;
                        float fy = itemClass.RelatePointF.Y;// itemClass.xPCenterOffsetWorld.Y;// + (int)numericUpDown2.Value;

                        //PointF PTTEMP = new PointF(itemClass.X, itemClass.Y);
                        //INI.Instance.MSRCalibration1.TransformViewToWorld(itemClass, out PTTEMP);

                        //fx = PTTEMP.X;
                        //fy = PTTEMP.Y;

                        x2 = xmldoc.CreateElement("cell" + i.ToString());
                        xeCells.AppendChild(x2);

                        XmlElement x3 = xmldoc.CreateElement("Center");
                        x3.SetAttribute("x", fx.ToString(m_format));
                        x3.SetAttribute("y", fy.ToString(m_format));
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("dbAngle");
                        x3.InnerText = 0.ToString(m_format);
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("nType");
                        x3.InnerText = 1.ToString();
                        x2.AppendChild(x3);

                        i++;
                    }

                    xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                }
            }



            #endregion

            //vsMessageBox.Close();
            //vsMessageBox.Dispose();
            //if (!System.IO.Directory.Exists("D:\\report"))
            //    System.IO.Directory.CreateDirectory("D:\\report");

            ////SaveDataEXD(reportstr, "D:\\report\\MsrData_" + "All" + ".csv");
            //SaveDataEXD(reportstr, "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv");
            //JetEazy.BasicSpace.VsMSG.Instance.Warning(ToChangeLanguage("数据存储于 ") + "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv", false);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"画出结果图{myElapsedTime} ms");

            if (m_bmpOpeateTest != null)
                m_bmpOpeateTest.Dispose();

            freeImageBitmapInput.Dispose();

            GC.Collect();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
        }
        public Rectangle ResizeWithLocation3(Rectangle rect, int ratio)
        {
            Size retSize;
            Point retPtF;

            if (ratio > 0)
            {
                retPtF = new Point(rect.X * ratio, rect.Y * ratio);
                retSize = new Size(rect.Width * ratio, rect.Height * ratio);
            }
            else
            {
                retPtF = new Point(rect.X / -ratio, rect.Y / -ratio);
                retSize = new Size(rect.Width / -ratio, rect.Height / -ratio);
            }

            retSize.Width = Math.Max(retSize.Width, 1);
            retSize.Height = Math.Max(retSize.Height, 1);

            return new Rectangle(retPtF.X, retPtF.Y, retSize.Width, retSize.Height);
        }
        bool IsInRange(float FromValue, float CompValue, float DiffValue)
        {
            return Math.Abs(FromValue - CompValue) < DiffValue;
        }

        int[] m_DurtimeRecord = new int[100];
        private void runTest()
        {
            switch (myRecipe.PreMode)
            {
                case ProcessImageMode.V10:
                    runTestV10_freeImage();
                    break;
                case ProcessImageMode.V9:
                    runTestV9_freeImage();
                    break;
                default:
                    runTest2_freeImage();
                    break;
            }
        }
        private void runTest1_mircoSoftware()
        {

            _saveDebugPicture = INI.Instance.IsSaveTestImage;

            //return;
            m_ElapsedTime = 0;
            m_Running = true;
            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();

            int istepcount = myRecipe.chip_captureCount * 2;

            if (Universal.IsOfflineDataVerifty)
                istepcount = 1;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            List<RegionCellClass>[] autoRegionCells = new List<RegionCellClass>[istepcount];
            OutputImages = new Bitmap[istepcount];

            int i = 0;
            while (i < autoRegionCells.Length)
            {
                autoRegionCells[i] = new List<RegionCellClass>();
                autoRegionCells[i].Clear();

                OutputImages[i] = new Bitmap(1, 1);

                i++;
            }

            //清除结果
            foreach (RegionCellClass cellClass in myListCell)
            {
                cellClass.Result = "";
                //cellClass.CellCorpWidth = RecipeTrayClass.Instance.chip_CropWidth;
                //cellClass.CellCorpSample = RecipeTrayClass.Instance.chip_CropSample;
                //cellClass.ReslutionX = INI.Instance.Chip_Reslution;
                //cellClass.ReslutionY = INI.Instance.Chip_Reslution;
            }

            //分步赋值 每张图片单独处理
            i = 0;
            while (i < autoRegionCells.Length)
            {
                foreach (RegionCellClass cellClass in myListCell)
                {
                    if (i == cellClass.CellCol)
                    {
                        autoRegionCells[i].Add(cellClass);
                    }
                }
                i++;
            }

            //判断图片是不是非法的
            bool bOK = true;
            foreach (Bitmap bmptemp0 in InputImages)
            {
                if (bmptemp0 == null)
                {
                    bOK = false;
                    break;
                }
                if (bmptemp0.Width == 1 || bmptemp0.Height == 1)
                {
                    bOK = false;
                    break;
                }
                if (
                    bmptemp0.Width < myRecipe.bmpORG.Width - 10
                    || bmptemp0.Height < myRecipe.bmpORG.Height - 10
                    )
                {
                    bOK = false;
                    break;
                }
            }
            if (!bOK)
            {
                stopwatch.Stop();
                m_ElapsedTime = stopwatch.ElapsedMilliseconds;
                m_Running = false;
                return;
            }

            string _path = Universal.DEBUGRESULTPATH + "\\" + _index.ToString() + "-" + JzTimes.DateTimeSerialStringFFF + "";
            _path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + "-" + _saveFileName + "";
            if (_saveDebugPicture)
            {
                if (!System.IO.Directory.Exists(_path))
                    System.IO.Directory.CreateDirectory(_path);
            }

            jzTimesDurRecord.Cut();

            //切小图
            Parallel.For(0, istepcount, (item) =>
            {
                foreach (RegionCellClass cellClass in autoRegionCells[item])
                {
                    cellClass.RealRectangleF =
                    new RectangleF(cellClass.viewRectF.X,
                                               cellClass.viewRectF.Y,
                                               cellClass.viewRectF.Width,
                                               cellClass.viewRectF.Height);
                    cellClass.BmpRun.Dispose();
                    cellClass.BmpRun = (Bitmap)InputImages[item].Clone(cellClass.viewRectF,
                                                     System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }
            });

            m_DurtimeRecord[0] = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();

            //计算 多线程
            Parallel.For(0, istepcount, (item) =>
            {
                Parallel.ForEach(autoRegionCells[item], (item1) =>
                {
                    item1.IsSaveDebugPicture = _saveDebugPicture;
                    item1.DebugPath = _path + "\\Step_" + item.ToString();
                    if (_saveDebugPicture)
                    {
                        if (!System.IO.Directory.Exists(item1.DebugPath))
                            System.IO.Directory.CreateDirectory(item1.DebugPath);
                    }

                    item1.Run();
                });
            });

            ////计算 单线程
            //for (int item = 0; item < istepcount; item++)
            //{
            //    foreach (AutoRegionCellClass item1 in autoRegionCells[item])
            //    {
            //        item1.IsSaveDebugPicture = _saveDebugPicture;
            //        item1.DebugPath = _path + "\\Step_" + item.ToString();
            //        if (_saveDebugPicture)
            //        {
            //            if (!System.IO.Directory.Exists(item1.DebugPath))
            //                System.IO.Directory.CreateDirectory(item1.DebugPath);
            //        }

            //        item1.Run();
            //    }
            //}

            m_DurtimeRecord[1] = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();

            //画图
            if (m_IsDrawRect)
            {
                //for (int item = 0; item < istepcount; item++)
                Parallel.For(0, istepcount, (item) =>
                {
                    JzToolsClass jzTools = new JzToolsClass();
                    OutputImages[item].Dispose();
                    OutputImages[item] = new Bitmap(InputImages[item]);
                    Graphics graphics = Graphics.FromImage(OutputImages[item]);

                    SolidBrush B = new SolidBrush(Color.Lime);
                    Font MyFont = new Font("Arial", 188);
                    Font MyFont1 = new Font("Arial", 88);

                    int ix = 0;
                    int jx = 0;
                    foreach (RegionCellClass cellClass in autoRegionCells[item])
                    {
                        //RectangleF rect = JzTools.Resize(cellClass.RectangleF, -1);
                        PointF ptf = new PointF(cellClass.viewRectF.X, cellClass.viewRectF.Y);
                        string str = string.Empty;

                        if (string.IsNullOrEmpty(cellClass.Result))
                            graphics.DrawString("NONE", MyFont, B, ptf);
                        else
                            graphics.DrawString(cellClass.Result, MyFont, B, ptf);
                    }
                    graphics.Dispose();
                    string _outputimage_path = myPath.Replace(".cfg", "-" + item.ToString("000") + ".jpg");
                    _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{item.ToString()}_" + _saveFileName + ".jpg";

                    if (_saveDebugPicture)
                    {
                        Bitmap bmpSaveToFile = new Bitmap(OutputImages[item],
                                                                                OutputImages[item].Width >> 1,
                                                                                OutputImages[item].Height >> 1);
                        bmpSaveToFile.Save(_outputimage_path, System.Drawing.Imaging.ImageFormat.Jpeg);
                        bmpSaveToFile.Dispose();
                    }

                    //OutputImages[item] = new Bitmap(bmpDraw0000);
                    //bmpDraw0000.Dispose();


                });
                //}
            }

            //SaveCurrent();
            //SaveData(Str, myPath.Replace(".cfg", ".csv"));
            //SaveData(Str, Universal.DEBUG_DATA_IMAGE + "\\" + _index.ToString() + "-" + _saveFileName + ".csv");
            //string reportPathFilename = INI.Instance.HistoryDataPath + "\\" + INI.Instance.HistoryDataBarcode + "\\report";
            //if (!Directory.Exists(reportPathFilename))
            //    Directory.CreateDirectory(reportPathFilename);
            //SaveData(Str, reportPathFilename + "\\" + _index.ToString() + "-" + _saveFileName + ".csv");

            m_DurtimeRecord[2] = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();

            i = 0;
            while (i < autoRegionCells.Length)
            {
                InputImages[i].Dispose();
                InputImages[i] = new Bitmap(1, 1);

                OutputImages[i].Dispose();
                OutputImages[i] = new Bitmap(1, 1);
                i++;
            }

            //清除小图
            Parallel.ForEach(myListCell, (item1) =>
            {
                item1.BmpRun.Dispose();
                item1.BmpRun = new Bitmap(1, 1);
            });

            GC.Collect();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
        }
        private void runTest2_freeImage()
        {

            _saveDebugPicture = INI.Instance.IsSaveTestImage;
            //InitTrayUI();

            int iSize = 1;

            //freeImageBitmapOutput = new FreeImageBitmap(freeImageBitmapInput);

            //return;
            m_ElapsedTime = 0;
            m_Running = true;
            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            Point MarkRunPtCenter = new Point(0, 0);
            #region MARK点位置
            if (INI.Instance.IsUseFixedMark)
            {
                Bitmap bmpinput = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(INI.Instance.mark_rect, PixelFormat.Format24bppRgb);
                m_Find.AH_SetThreshold(ref bmpinput, INI.Instance.mark_thresholdvalue);
                m_Find.AH_FindBlob(bmpinput, true);

                if (m_Find.FoundList.Count > 0)
                {
                    //Rectangle rectmax = m_Find.rectMaxRect;
                    Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + INI.Instance.mark_rect.X,
                       m_Find.rectMaxRect.Y + INI.Instance.mark_rect.Y,
                       m_Find.rectMaxRect.Width,
                       m_Find.rectMaxRect.Height);

                    //Point MarkRunPtCenter = GetRectCenter(rectmax);
                    MarkRunPtCenter = GetRectCenter(rectmax);
                    _markOffset = new PointF(MarkRunPtCenter.X - INI.Instance.mark_org.X, MarkRunPtCenter.Y - INI.Instance.mark_org.Y);
                }
                bmpinput.Dispose();

                //_markOffset = new PointF(0, 0);
            }
            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"计算MARK点位置{myElapsedTime} ms");


            //清除结果
            foreach (RegionCellClass cellClass in myListCell)
            {
                cellClass.Result = "";
            }

            string _path = Universal.DEBUGRESULTPATH + "\\" + _index.ToString() + "-" + JzTimes.DateTimeSerialStringFFF + "";
            _path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + "-" + _saveFileName + "";
            //if (_saveDebugPicture)
            {
                if (!System.IO.Directory.Exists(_path))
                    System.IO.Directory.CreateDirectory(_path);
            }

            #region 分割小图
            ////切小图
            //foreach (RegionCellClass cellClass in myListCell)
            //{
            //    cellClass.RealRectangleF =
            //    new RectangleF(cellClass.viewRectF.X,
            //                               cellClass.viewRectF.Y,
            //                               cellClass.viewRectF.Width,
            //                               cellClass.viewRectF.Height);
            //    cellClass.BmpRun.Dispose();
            //    cellClass.BmpRun = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(cellClass.viewRectF,
            //                                     System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //}

            PointF markp1 = new PointF(0, 0);
            PointF markp2 = new PointF(0, 0);

            Bitmap bmptemp = freeImageBitmapInput.ToBitmap();

            //AUColorImg24 imginput24 = new AUColorImg24(bmptemp.Width, bmptemp.Height);
            //AUUtility.DrawBitmapToAUColorImg24(bmptemp, ref imginput24);

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"转换图片{myElapsedTime} ms");

            //切小图
            int myrectindex = 0;
            foreach (RegionCellClass cellClass in myListCell)
            {
                //cellClass.viewRectF = myRecipe.rect_list[myrectindex];

                switch(myRecipe.PreMode)
                {
                    case ProcessImageMode.V3:
                    case ProcessImageMode.V11:
                        #region V3

                        RectangleF cropRectTemp =
                           new RectangleF(cellClass.viewRectF.X,
                                                  cellClass.viewRectF.Y,
                                                  cellClass.viewRectF.Width,
                                                  cellClass.viewRectF.Height);

                        cropRectTemp.Inflate(myRecipe.PreCenterExtendx, myRecipe.PreCenterExtendy);

                        m_Tools.BoundRect(ref cropRectTemp, bmptemp.Size);

                        cellClass.RealRectangleF =
                        new RectangleF(cropRectTemp.X,
                                                   cropRectTemp.Y,
                                                   cropRectTemp.Width,
                                                   cropRectTemp.Height);

                        if (cellClass.BmpRun != null)
                            cellClass.BmpRun.Dispose();

                        
                        cellClass.BmpRun = bmptemp.Clone(cropRectTemp, PixelFormat.Format24bppRgb);

                        #endregion

                        break;
                    default:

                        cellClass.RealRectangleF =
                new RectangleF(cellClass.viewRectF.X,
                                           cellClass.viewRectF.Y,
                                           cellClass.viewRectF.Width,
                                           cellClass.viewRectF.Height);

                        if (cellClass.BmpRun != null)
                            cellClass.BmpRun.Dispose();
                        cellClass.BmpRun = bmptemp.Clone(cellClass.viewRectF, PixelFormat.Format24bppRgb);


                        //cellClass.BmpRun = GetScaleRotateEX(cellClass.viewRectF.X + cellClass.viewRectF.Width / 2, cellClass.viewRectF.Y + cellClass.viewRectF.Height / 2, 0,
                        //    (int)cellClass.viewRectF.Width,
                        //    (int)cellClass.viewRectF.Height,
                        //    imginput24);

                        //if (cellClass.BmpRun != null)
                        //    cellClass.BmpRun.Dispose();
                        ////cellClass.BmpRun = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(cellClass.viewRectF,
                        ////                                 System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        //cellClass.BmpRun = new Bitmap((int)cellClass.viewRectF.Width,
                        //        (int)cellClass.viewRectF.Height);
                        //using (Graphics graphicsx = Graphics.FromImage(cellClass.BmpRun))
                        //{
                        //    graphicsx.DrawImage(bmptemp, new Rectangle(0, 0, (int)cellClass.viewRectF.Width,
                        //        (int)cellClass.viewRectF.Height), Rectangle.Round(cellClass.viewRectF), GraphicsUnit.Pixel);
                        //}

                        break;
                }

                cellClass.CropValue = myRecipe.CropWidth;
                cellClass.SampleValue = myRecipe.SampleValue;
                cellClass.m_ProcessImageMode = myRecipe.PreMode;

                cellClass.PreDir = myRecipe.PreDir;
                cellClass.PreThresholdValue = myRecipe.PreThresholdValue;
                cellClass.PreThresholdValueMark2 = myRecipe.PreThresholdValueMark2;
                cellClass.PreCenterExtendx = myRecipe.PreCenterExtendx;
                cellClass.PreCenterExtendy = myRecipe.PreCenterExtendy;
                cellClass.PreLineLeastDistance = myRecipe.PreLineLeastDistance;
                if (INI.Instance.IsForceInspect)
                    cellClass.IsByPass = false;
                else
                    cellClass.IsByPass = myRecipe.QcPass[myrectindex];


                cellClass.IsByPassFromManual = myRecipe.QcPassManual[myrectindex];

                cellClass.PreMarkMinArea = myRecipe.MarkMinArea;

                cellClass.Mark0 = myRecipe.Mark0;
                cellClass.Mark1 = myRecipe.Mark1;
                cellClass.MarkLine = myRecipe.MarkLine;

                cellClass.MarkLine1 = myRecipe.MarkLine1;
                cellClass.MarkLine2 = myRecipe.MarkLine2;

                cellClass.PreIsOpenAuFind = myRecipe.IsOpenAuFind;
                cellClass.paraClass.tolerance = myRecipe.inspect_tolerance;
                cellClass.paraClass.angle = myRecipe.inspect_angle;
                cellClass.paraClass.samplesize = myRecipe.inspect_samplesize;


                #region 对应抓边的参数


                cellClass.SideLeft = myRecipe.SideLeft;
                cellClass.SideRight = myRecipe.SideRight;
                cellClass.SideTop = myRecipe.SideTop;
                cellClass.SideBottom = myRecipe.SideBottom;

                cellClass.dir_left = myRecipe.dir_left;
                cellClass.dir_left_black = myRecipe.dir_left_black;
                cellClass.dir_right = myRecipe.dir_right;
                cellClass.dir_right_black = myRecipe.dir_right_black;
                cellClass.dir_top = myRecipe.dir_top;
                cellClass.dir_top_black = myRecipe.dir_top_black;
                cellClass.dir_bottom = myRecipe.dir_bottom;
                cellClass.dir_bottom_black = myRecipe.dir_bottom_black;

                #endregion

                #region 定位方向

                cellClass.inspect_dir_open = myRecipe.inspect_dir_open;
                cellClass.paraClass_dir.tolerance = myRecipe.inspect_dir_tolerance;
                cellClass.paraClass_dir.angle = myRecipe.inspect_dir_angle;
                cellClass.paraClass_dir.samplesize = myRecipe.inspect_dir_samplesize;
                RectangleF dirrecttemp = new RectangleF(myRecipe.DirRectF.X,
                    myRecipe.DirRectF.Y, myRecipe.DirRectF.Width, myRecipe.DirRectF.Height);
                dirrecttemp.Inflate(myRecipe.PreCenterExtendx + myRecipe.inspect_dir_extendx, myRecipe.PreCenterExtendy + myRecipe.inspect_dir_extendy);

                cellClass.DirRectF = new RectangleF(dirrecttemp.X,
                   dirrecttemp.Y, dirrecttemp.Width, dirrecttemp.Height);

                #endregion


                //cellClass.laserDotCoordinate.SetKeyPoints(new PointF(myRecipe.k0x, myRecipe.k0y),
                //                                                                      new PointF(myRecipe.k1x, myRecipe.k1y),
                //                                                                      new PointF(myRecipe.LaserLocX, myRecipe.LaserLocY));

                //if (myrectindex == 0)
                //{
                //    cellClass.IsByPass = false;
                //}
                //else
                //{
                //    cellClass.IsByPass = true;
                //}


                myrectindex++;
            }

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"分割小图{myElapsedTime} ms");

            #region 测试计算
            if (INI.Instance.IsOpenThread)
            {
                //foreach (RegionCellClass item1 in myListCell)
                Parallel.ForEach(myListCell, (item1) =>
                {
                    item1.IsSaveDebugPicture = _saveDebugPicture;
                    item1.DebugPath = _path + "\\single_" + Index.ToString();
                    if (_saveDebugPicture)
                    {
                        if (!System.IO.Directory.Exists(item1.DebugPath))
                            System.IO.Directory.CreateDirectory(item1.DebugPath);
                    }


                    item1.Run();
                });
                //}
            }
            else
            {
                foreach (RegionCellClass item1 in myListCell)
                //Parallel.ForEach(myListCell, (item1) =>
                {
                    item1.IsSaveDebugPicture = _saveDebugPicture;
                    item1.DebugPath = _path + "\\single_" + Index.ToString();
                    if (_saveDebugPicture)
                    {
                        if (!System.IO.Directory.Exists(item1.DebugPath))
                            System.IO.Directory.CreateDirectory(item1.DebugPath);
                    }


                    item1.Run();
                    //});
                }
            }
            //    //计算 多线程
            //    foreach (RegionCellClass item1 in myListCell)
            //        //Parallel.ForEach(myListCell, (item1) =>
            //    {
            //        item1.IsSaveDebugPicture = _saveDebugPicture;
            //        item1.DebugPath = _path + "\\single_" + Index.ToString();
            //        if (_saveDebugPicture)
            //        {
            //            if (!System.IO.Directory.Exists(item1.DebugPath))
            //                System.IO.Directory.CreateDirectory(item1.DebugPath);
            //        }


            //        item1.Run();
            //    //});
            //}
            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"测试计算{myElapsedTime} ms");


            PointF markWorld0 = myRecipe.ptMark0;
            PointF markWorld1 = myRecipe.ptMark1;

            #region  保存资料

            double angleOfLine = 0;
            double angleOfLineORG = 0;
            if (myRecipe.ptMark1.X > myRecipe.ptMark0.X)
                angleOfLineORG = Math.Atan2((myRecipe.ptMark1.Y - myRecipe.ptMark0.Y), (myRecipe.ptMark1.X - myRecipe.ptMark0.X)) * 180 / Math.PI;
            else
                angleOfLineORG = Math.Atan2((myRecipe.ptMark0.Y - myRecipe.ptMark1.Y), (myRecipe.ptMark0.X - myRecipe.ptMark1.X)) * 180 / Math.PI;

            //保存资料
            string dataStr = "Index,Name,ViewX,ViewY,WorldX,WorldY,Angle,nType,CenterX,CenterY,CenterXW,CenterYW,OFX,OFY,MRUNX,MRUNY,orgAngle,worldAngle," + Environment.NewLine;
            int index = 1;
            foreach (RegionCellClass cellClass in myListCell)
            {
                if (INI.Instance.IsOpenPrintFirst)
                {
                    if (index == 1)
                    {
                        cellClass.nType = 1;
                    }
                    else
                    {
                        cellClass.nType = 0;
                    }
                }

                if (!INI.Instance.IsUseFixedMark)
                    _markOffset = new PointF(0, 0);

                cellClass.xPCenterOffset = new PointF(cellClass.xPCenter.X - _markOffset.X, cellClass.xPCenter.Y - _markOffset.Y);
                cellClass.xPSecondOffset = new PointF(cellClass.xPSecond.X - _markOffset.X, cellClass.xPSecond.Y - _markOffset.Y);
                cellClass.xPMark2Offset = new PointF(cellClass.xP2.X - _markOffset.X, cellClass.xP2.Y - _markOffset.Y);


                //转换为世界坐标
                cellClass.xPCenterWorld = ToWorld(cellClass.xPCenter);
                cellClass.xP1World = ToWorld(cellClass.xP1);
                cellClass.xP2World = ToWorld(cellClass.xP2);

                cellClass.xP3World = ToWorld(cellClass.xP3);
                cellClass.xP4World = ToWorld(cellClass.xP4);
                //十字的world
                cellClass.xPCenterOffsetWorld = ToWorld(cellClass.xPCenterOffset);
                cellClass.xPSecondOffsetWorld = ToWorld(cellClass.xPSecondOffset);
                //矩形的world
                cellClass.xPMark2OffsetWorld = ToWorld(cellClass.xPMark2Offset);

                //PointF p1 = ToView(cellClass.xPCenterOffsetWorld, cellClass.xPCenterOffset);
                //PointF p2 = ToView(cellClass.xPSecondOffsetWorld, cellClass.xPSecondOffset);

                markWorld0 = ToWorld(new PointF(myRecipe.ptMark0.X + cellClass.viewRectF.X, myRecipe.ptMark0.Y + cellClass.viewRectF.Y));
                markWorld1 = ToWorld(new PointF(myRecipe.ptMark1.X + cellClass.viewRectF.X, myRecipe.ptMark1.Y + cellClass.viewRectF.Y));


                if (markWorld1.X > markWorld0.X)
                    angleOfLine = Math.Atan2((markWorld1.Y - markWorld0.Y), (markWorld1.X - markWorld0.X)) * 180 / Math.PI;
                else
                    angleOfLine = Math.Atan2((markWorld0.Y - markWorld1.Y), (markWorld0.X - markWorld1.X)) * 180 / Math.PI;

                dataStr += $"{index},{cellClass.lblName}," +
                    $"{PointF000ToString(cellClass.xPCenterOffset)}," +
                    $"{PointF000ToString(cellClass.xPCenterOffsetWorld)}," +
                    $"{cellClass.Angle().ToString(m_format)}," +
                    $"{cellClass.nType.ToString()}," +
                    $"{PointF000ToString(cellClass.xPCenter)}," +
                    $"{PointF000ToString(cellClass.xPCenterWorld)}," +
                    $"{PointF000ToString(_markOffset)}," +
                    $"{PointF000ToString(MarkRunPtCenter)}," +
                    $"{angleOfLineORG.ToString()}," +
                    $"{angleOfLine.ToString()}," +
                     $"{PointF000ToString(cellClass.xPSecondOffset)}," +
                    $"{PointF000ToString(cellClass.xPSecondOffsetWorld)}," +
                    //$"{INI.Instance.Cal_Bcx.ToString()}," +
                    //  $"{INI.Instance.Cal_Bcy.ToString()}," +
                    //    $"{INI.Instance.Cal_Bca.ToString()}," +
                    $"{Environment.NewLine}";

                index++;
            }

            string _dataStr_path = $"{_path}\\{JzTimes.DateTimeSerialStringFFF}.csv";
            SaveData(dataStr, _dataStr_path);

            #region 保存xml 供镭雕机调用
            string _laserStr = string.Empty;//左上角 右上角 右下角 左下角
            _laserStr += "Index,Name,OrgLength,RunLength,AvgAngle,TopMargin,LeftMargin,RightMargin,BottomMargin," +
                "TopLineAngle,CenterLineAngle,BottomLineAngle,LTAngle,RTAngle,RBAngle,LBAngle,TopWidth,BottomWidth,LeftHeight,RightHeight," + Environment.NewLine;
            if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
            {
                if (!Directory.Exists(INI.Instance.LaserSharePath))
                    Directory.CreateDirectory(INI.Instance.LaserSharePath);
                if (Directory.Exists(INI.Instance.LaserSharePath))
                {
                    XmlDocument xmldoc;
                    XmlNode xmlnode;
                    XmlElement xmlelem;

                    xmldoc = new XmlDocument();
                    XmlDeclaration xmldecl;
                    xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                    xmldoc.AppendChild(xmldecl);

                    //加入一个根元素
                    xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                    xmldoc.AppendChild(xmlelem);

                    XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                    xmlelem.AppendChild(xeMatrix);

                    XmlElement x2 = xmldoc.CreateElement("Center");
                    x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                    x2.SetAttribute("y", (0).ToString());
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Angle");
                    x2.InnerText = "0";
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Row");
                    x2.InnerText = Row.ToString();
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Col");
                    x2.InnerText = Column.ToString();
                    xeMatrix.AppendChild(x2);

                    XmlNode xeCells = xmldoc.CreateElement("Cells");
                    xmlelem.AppendChild(xeCells);

                    int i = 0;
                    foreach (RegionCellClass itemClass in myListCell)
                    {
                        double fx = itemClass.xPCenterOffsetWorld.X + myRecipe.LaserLocX + INI.Instance.Cal_Bcx;// + (int)numericUpDown1.Value;
                        double fy = itemClass.xPCenterOffsetWorld.Y + myRecipe.LaserLocY + INI.Instance.Cal_Bcy;// + (int)numericUpDown2.Value;
                        double angleCal = 0d;
                        //double angleCalX = 0d;
                        switch (myRecipe.PreMode)
                        {
                            case ProcessImageMode.V11:
                            case ProcessImageMode.V2:
                            case ProcessImageMode.V3:
                            case ProcessImageMode.V8://产生数据给镭雕

                                PointF PTUV2 = ToLaserCmd(itemClass.xPCenterOffsetWorld);

                                //fx = PTUV2.X + INI.Instance.Cal_Bcx;// + (int)numericUpDown1.Value;
                                //fy = PTUV2.Y + INI.Instance.Cal_Bcy;// + (int)numericUpDown2.Value;

                                #region 通过中点算到每边的距离

                                //上边的中点
                                PointF topcenter = GetCenterPointF(itemClass.xP1World, itemClass.xP2World);
                                PointF bottomcenter = GetCenterPointF(itemClass.xP3World, itemClass.xP4World);

                                PointF leftcenter = GetCenterPointF(itemClass.xP1World, itemClass.xP4World);
                                PointF rightcenter = GetCenterPointF(itemClass.xP2World, itemClass.xP3World);

                                double d1 = GetPointLength(itemClass.xPCenterWorld, topcenter);//上边距
                                double d2 = GetPointLength(itemClass.xPCenterWorld, leftcenter);//左边距
                                double d3 = GetPointLength(itemClass.xPCenterWorld, rightcenter);//右边距
                                double d4 = GetPointLength(itemClass.xPCenterWorld, bottomcenter);//下边距

                                double _width = GetPointLength(itemClass.xP1World, itemClass.xP2World);
                                double _height = GetPointLength(itemClass.xP1World, itemClass.xP4World);

                                double _width1 = GetPointLength(itemClass.xP3World, itemClass.xP4World);
                                double _height1 = GetPointLength(itemClass.xP3World, itemClass.xP2World);

                                if (itemClass.nType == 1)
                                {
                                    //判断长宽
                                    if (!IsInRangeEx(_width, myRecipe.stand_width + myRecipe.stand_width_upper, myRecipe.stand_width - myRecipe.stand_width_lower)
                                        ||
                                        !IsInRangeEx(_width1, myRecipe.stand_width + myRecipe.stand_width_upper, myRecipe.stand_width - myRecipe.stand_width_lower)
                                        ||
                                        !IsInRangeEx(_height, myRecipe.stand_height + myRecipe.stand_height_upper, myRecipe.stand_height - myRecipe.stand_height_lower)
                                        ||
                                        !IsInRangeEx(_height1, myRecipe.stand_height + myRecipe.stand_height_upper, myRecipe.stand_height - myRecipe.stand_height_lower)
                                        )
                                    {
                                        itemClass.nType = 0;
                                        itemClass.nDesc = $"{ToChangeLanguage("测量宽度或高度异常")}";
                                    }
                                }

                                //angleCal = GetAngleVector(itemClass.xPCenterWorld, rightcenter);
                                double a1 = GetAngleVector(leftcenter, rightcenter);
                                double a2 = GetAngleVector(itemClass.xP1World, itemClass.xP2World);
                                double a3 = GetAngleVector(itemClass.xP4World, itemClass.xP3World);


                                //左上角 右上角 右下角 左下角 角度
                                double c1 = GetAngleVector(itemClass.xP1World, itemClass.xP2World, itemClass.xP1World, itemClass.xP4World);
                                double c2 = GetAngleVector(itemClass.xP2World, itemClass.xP1World, itemClass.xP2World, itemClass.xP3World);
                                double c3 = GetAngleVector(itemClass.xP3World, itemClass.xP2World, itemClass.xP3World, itemClass.xP4World);
                                double c4 = GetAngleVector(itemClass.xP4World, itemClass.xP1World, itemClass.xP4World, itemClass.xP3World);

                                if (itemClass.nType == 1)
                                {
                                    if (!IsInRangeEx(Math.Abs(c1), 90 + INI.Instance.GC_Angle, 90 - INI.Instance.GC_Angle)
                                    ||
                                    !IsInRangeEx(Math.Abs(c2), 90 + INI.Instance.GC_Angle, 90 - INI.Instance.GC_Angle)
                                    ||
                                    !IsInRangeEx(Math.Abs(c3), 90 + INI.Instance.GC_Angle, 90 - INI.Instance.GC_Angle)
                                    ||
                                    !IsInRangeEx(Math.Abs(c4), 90 + INI.Instance.GC_Angle, 90 - INI.Instance.GC_Angle)
                                    )
                                    {
                                        itemClass.nType = 0;
                                        itemClass.nDesc = $"{ToChangeLanguage("测量直角角度异常")}";
                                    }
                                }

                                angleCal = (a1 + a2 + a3) / 3;
                                _laserStr += $"{index},{itemClass.lblName},{0},{0},{angleCal},{d1},{d2},{d3},{d4},{a1},{a2},{a3},{c1},{c2},{c3},{c4},{_width},{_width1},{_height},{_height1}" + Environment.NewLine;

                                #endregion

                                #region 通过直线算到每边的距离

                                ////计算到每边的距离
                                //LineClass l1 = new LineClass(itemClass.xP1World, itemClass.xP2World);
                                ////LineClass l2 = new LineClass(itemClass.xP1World, itemClass.xP3World);
                                ////l2.IsSwap = true;
                                ////LineClass l3 = new LineClass(itemClass.xP4World, itemClass.xP2World);
                                ////l3.IsSwap = true;
                                //LineClass l4 = new LineClass(itemClass.xP4World, itemClass.xP3World);

                                //QvLineFit qvLineFit2 = new QvLineFit();
                                //qvLineFit2.LeastSquareFit(new PointF[2] { itemClass.xP1World, itemClass.xP3World });
                                //qvLineFit2.Swap = true;


                                //QvLineFit2 qvLineFit21 = new QvLineFit2();
                                //qvLineFit21.LeastSquareFit(new PointF[2] { itemClass.xP2World, itemClass.xP4World });
                                //qvLineFit21.Swap = true;
                                ////QvLineFit qvLineFit3 = new QvLineFit();
                                ////qvLineFit3.LeastSquareFit(new PointF[2] { itemClass.xP2World, itemClass.xP4World });
                                ////qvLineFit3.Swap = true;
                                //double d1 = l1.GetVerticalLength(itemClass.xPCenterWorld);//上边距
                                //double d2 = qvLineFit2.GetPointLength(itemClass.xPCenterWorld);//左边距


                                ////    //l2.GetVerticalLength(itemClass.xPCenterWorld);
                                //double d3 = qvLineFit21.GetPointLength(itemClass.xPCenterWorld);//右边距

                                ////l3.GetVerticalLength(itemClass.xPCenterWorld);
                                //double d4 = l4.GetVerticalLength(itemClass.xPCenterWorld);//下边距


                                ////angleCal = GetAngle(itemClass.xP1World, itemClass.xP2World);

                                //angleCal = GetAngleVector(itemClass.xP1World, itemClass.xP2World);
                                ////_laserStr += $"{index},{itemClass.lblName},{0},{0},{itemClass.AngleRun()},{d1},{d4}," + Environment.NewLine;
                                //_laserStr += $"{index},{itemClass.lblName},{0},{0},{angleCal},{d1},{d2},{d3},{d4}," + Environment.NewLine;

                                #endregion

                                if (myRecipe.LaserLocX == 0 && myRecipe.LaserLocY == 0)
                                {
                                    switch (myRecipe.fourStartPosition)
                                    {
                                        case LaserOffsetStart.LeftBottom:
                                            PTUV2 = ToLaserCmd(itemClass.xP3World);
                                            break;
                                        default:
                                            PTUV2 = ToLaserCmd(itemClass.xPCenterOffsetWorld);
                                            break;
                                    }

                                    //itemClass.xPCenterLaserOffset = ToView(PTUV2, itemClass.xP1);
                                }
                                else
                                {
                                    var ldcx = new LaserDotCoordinate();
                                    var k0x = new PointF(0, 0);
                                    var k1x = new PointF((float)_width, (float)_height);
                                    var k2x = new PointF(myRecipe.LaserLocX, myRecipe.LaserLocY);
                                    var q0x = new PointF(itemClass.xP4World.X, itemClass.xP4World.Y);
                                    var q1x = new PointF(itemClass.xP2World.X, itemClass.xP2World.Y);


                                    switch (myRecipe.fourStartPosition)
                                    {
                                        case LaserOffsetStart.LeftBottom:

                                            k1x = new PointF((float)_width, (float)_height);
                                            ldcx.CalcPointQ2(k0x, k1x, k2x, q0x, q1x, out PointF q2x, out double phix);
                                            PTUV2 = ToLaserCmd(q2x);
                                            itemClass.xPCenterLaserOffset = ToView(q2x, itemClass.xP1);

                                            angleCal = phix;

                                            break;
                                        default:

                                            k1x = new PointF((float)_width / 2, (float)_height / 2);
                                            q0x = new PointF(itemClass.xPCenterWorld.X, itemClass.xPCenterWorld.Y);

                                            ldcx.CalcPointQ2(k0x, k1x, k2x, q0x, q1x, out PointF q2xx, out double phixx);
                                            PTUV2 = ToLaserCmd(q2xx);
                                            itemClass.xPCenterLaserOffset = ToView(q2xx, itemClass.xP1);

                                            angleCal = phixx;

                                            break;
                                    }


                                    //System.Diagnostics.Debug.WriteLine($"q2= {q2.X}, {q2.Y}");
                                    //System.Diagnostics.Debug.WriteLine($"phi= {phi}");
                                    //System.Diagnostics.Debug.WriteLine($"phi= {phi * 180 / Math.PI}");
                                }

                                if (itemClass.nType == 1)
                                {
                                    if (!IsInRangeEx(angleCal, myRecipe.stand_angle + myRecipe.stand_angle_upper, myRecipe.stand_angle - myRecipe.stand_angle_lower))
                                    {
                                        itemClass.nType = 0;
                                        itemClass.nDesc = $"{ToChangeLanguage("测量角度异常")}";
                                    }
                                }

                                switch (myRecipe.fourStartPosition)
                                {
                                    case LaserOffsetStart.LeftBottom:

                                        fx = PTUV2.X + INI.Instance.Cal_BcxLB + INI.Instance.Cal_Bcx;
                                        fy = PTUV2.Y + INI.Instance.Cal_BcyLB + INI.Instance.Cal_Bcy;

                                        break;
                                    default:

                                        fx = PTUV2.X + INI.Instance.Cal_Bcx;// + (int)numericUpDown1.Value;
                                        fy = PTUV2.Y + INI.Instance.Cal_Bcy;// + (int)numericUpDown2.Value;

                                        break;
                                }

                                //转到图像的坐标
                                //itemClass.xPCenterLaserOffset = ToView(itemClass.xPCenterLaserOffsetWorld, itemClass.xP1);
                                //itemClass.xPCenterLaserOffset.X += _markOffset.X;
                                //itemClass.xPCenterLaserOffset.Y += _markOffset.Y;


                                break;
                            case ProcessImageMode.V7:
                            case ProcessImageMode.V6:
                            case ProcessImageMode.V5:

                                #region no_use_src
#if NOUSE_SRC
                                fx = itemClass.xPCenterOffset.X + 
                                    myRecipe.LaserLocX / INI.Instance.DIRRES_X + 
                                    INI.Instance.Cal_Bcx / INI.Instance.DIRRES_X;

                                fy = itemClass.xPCenterOffset.Y +
                                 myRecipe.LaserLocY / INI.Instance.DIRRES_Y +
                                 INI.Instance.Cal_Bcy / INI.Instance.DIRRES_Y;

                                double baseangle = GetAngle(itemClass.xPCenterOffset, new PointF((float)fx, (float)fy));
                                angleCalX = GetAngle(itemClass.xP1, itemClass.xP2) + myRecipe.MarkLineAngle;

                                angleCal = angleCalX;

                                ////与水平面的夹角
                                //double angle0 = GetAngle(itemClass.xPCenterOffsetWorld, itemClass.xPSecondOffsetWorld);
                                ////double angle1 = GetAngle(new PointF(0, 0), new PointF(myRecipe.LaserLocX, myRecipe.LaserLocY));

                                ////double angle = Math.Abs(angle0 - angle1);

                                ////从直角坐标 和 也可以变换为极坐标：
                                //// x=pcos@
                                //// y=psin@

                                ////20241006
                                ////以Mark点为中心 旋转角度angle0

                                //if (myRecipe.PreMode == ProcessImageMode.V7)
                                //{
                                //    angle0 = GetAngle(itemClass.xPCenterOffsetWorld, itemClass.xPMark2OffsetWorld) - myRecipe.MarkLineAngle;

                                //}

                                NetToolsHelper.PointHelper pointHelperA = new NetToolsHelper.PointHelper(itemClass.xPCenterOffset.X, itemClass.xPCenterOffset.Y);
                                NetToolsHelper.PointHelper pointHelperP = new NetToolsHelper.PointHelper(fx, fy);
                                NetToolsHelper.PointHelper pointHelper3 = NetToolsHelper.RotatePoint(pointHelperP, pointHelperA, angleCal * Math.PI / 180, angleCal > 0);

                                fx = pointHelper3.x;
                                fy = pointHelper3.y;


                                itemClass.xPCenterLaserOffset = PointFDuiCheng(new PointF((float)fx, (float)itemClass.xPCenterOffset.Y), new PointF((float)fx, (float)fy));

                                //itemClass.xPCenterLaserOffset = new PointF((float)fx, (float)fy);
                                itemClass.xPCenterLaserOffsetWorld = ToWorld(itemClass.xPCenterLaserOffset);
                                //itemClass.xPCenterLaserOffset = ToView(itemClass.xPCenterLaserOffsetWorld, itemClass.xPCenterOffset);

                                ////PointF pA = ToView(itemClass.xPCenterOffsetWorld, itemClass.xPCenterOffset);
                                ////PointF pAorg = new PointF(itemClass.xPCenter.X, itemClass.xPCenter.Y);
                                ////PointF pAorgoffset = new PointF(itemClass.xPCenter.X - _markOffset.X, itemClass.xPCenter.Y - _markOffset.Y);

                                //itemClass.xPCenterLaserOffset.X += _markOffset.X;
                                //itemClass.xPCenterLaserOffset.Y += _markOffset.Y;

                                fx = itemClass.xPCenterLaserOffsetWorld.X;
                                fy = itemClass.xPCenterLaserOffsetWorld.Y;
#endif
                                #endregion


                                if (itemClass.lblName == "ROW001-COL001")
                                {
                                    itemClass.lblName = itemClass.lblName;
                                }


                                var ldc = new LaserDotCoordinate();
                                var k0 = new PointF(myRecipe.k0x, myRecipe.k0y);
                                //var k1 = new PointF(itemClass.xPMark2OffsetWorld.X - itemClass.xPCenterOffsetWorld.X,
                                //                    itemClass.xPMark2OffsetWorld.Y - itemClass.xPCenterOffsetWorld.Y);
                                var k1 = new PointF(myRecipe.k1x, myRecipe.k1y);
                                var k2 = new PointF(myRecipe.LaserLocX, myRecipe.LaserLocY);
                                var q0 = new PointF(itemClass.xPCenterOffsetWorld.X, itemClass.xPCenterOffsetWorld.Y);
                                var q1 = new PointF(itemClass.xPMark2OffsetWorld.X, itemClass.xPMark2OffsetWorld.Y);
                                ldc.CalcPointQ2(k0, k1, k2, q0, q1, out PointF q2, out double phi);
                                System.Diagnostics.Debug.WriteLine($"q2= {q2.X}, {q2.Y}");
                                System.Diagnostics.Debug.WriteLine($"phi= {phi}");
                                System.Diagnostics.Debug.WriteLine($"phi= {phi * 180 / Math.PI}");


                                itemClass.xPCenterLaserOffsetWorld = new PointF(q2.X + INI.Instance.Cal_Bcx, q2.Y + INI.Instance.Cal_Bcy);
                                angleCal = phi;

                                //转到图像的坐标
                                itemClass.xPCenterLaserOffset = ToView(itemClass.xPCenterLaserOffsetWorld, itemClass.xP1);
                                itemClass.xPCenterLaserOffset.X += _markOffset.X;
                                itemClass.xPCenterLaserOffset.Y += _markOffset.Y;

                                PointF PTU = ToLaserCmd(itemClass.xPCenterLaserOffsetWorld);
                                itemClass.xPCenterLaserOffsetWorld = new PointF(PTU.X, PTU.Y);

                                fx = itemClass.xPCenterLaserOffsetWorld.X;
                                fy = itemClass.xPCenterLaserOffsetWorld.Y;


                                double _org = GetPointLength(k0, k1);
                                double _run = GetPointLength(q0, q1);
                                _laserStr += $"{index},{itemClass.lblName},{_org},{_run},{phi}" + Environment.NewLine;

                                //fx = PTU.X;
                                //fy = PTU.Y;

                                break;

                        }


                        x2 = xmldoc.CreateElement("cell" + i.ToString());
                        xeCells.AppendChild(x2);

                        XmlElement x3 = xmldoc.CreateElement("Center");
                        x3.SetAttribute("x", fx.ToString(m_format));
                        x3.SetAttribute("y", fy.ToString(m_format));
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("dbAngle");
                        switch (myRecipe.PreMode)
                        {
                            case ProcessImageMode.V7:
                            case ProcessImageMode.V6:
                            case ProcessImageMode.V5:

                                x3.InnerText = (angleCal + myRecipe.LaserLocA + INI.Instance.Cal_Bca).ToString(m_format);
                                break;
                            default:
                                x3.InnerText = (angleCal + INI.Instance.Cal_Bca).ToString(m_format);
                                break;
                        }
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("nType");
                        x3.InnerText = itemClass.nType.ToString();
                        x2.AppendChild(x3);


                        bool bOKTemp = itemClass.nType == 1 || itemClass.nDesc == ToChangeLanguage("不检测");
                        m_IsPass &= bOKTemp;

                        i++;
                    }

                    xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                }
            }

            string _laserStr_path = $"{_path}\\laser_{JzTimes.DateTimeSerialStringFFF}.csv";
            SaveData(_laserStr, _laserStr_path);

            #endregion


            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"生成资料档案{myElapsedTime} ms");

            #region 画图
            int _showsize = 1;

            if (INI.Instance.IsOpenShowSize)
                _showsize = INI.Instance.ShowSizeValue;

            //画图
            if (m_IsDrawRect)
            {
                JzToolsClass jzTools = new JzToolsClass();
                freeImageBitmapOutputDraw.Dispose();
                freeImageBitmapOutputDraw = new Bitmap(freeImageBitmapInput.ToBitmap(),
                                                                   new Size(freeImageBitmapInput.ToBitmap().Width / _showsize,
                                                                                   freeImageBitmapInput.ToBitmap().Height / _showsize));

                Graphics graphics = Graphics.FromImage(freeImageBitmapOutputDraw);

                SolidBrush B = new SolidBrush(Color.Lime);
                SolidBrush Bfail = new SolidBrush(Color.Red);
                SolidBrush B_fail = new SolidBrush(Color.Red);
                //Font MyFont = new Font("Arial", 188);
                //Font MyFont1 = new Font("Arial", 88);
                Font MyFont1 = new Font("Arial", 100);
                Font MyFont2 = new Font("Arial", 18);
                float _lineWidth = 8.8f;

                int ix = 0;
                int jx = 0;
                foreach (RegionCellClass cellClass in myListCell)
                {
                    PointF ptf = new PointF(cellClass.viewRectF.X / _showsize, cellClass.viewRectF.Y / _showsize);
                    string str = string.Empty;

                    if (cellClass.nType == 1)
                    {
                        //graphics.DrawLine(new Pen(Color.Purple, _lineWidth / _showsize), pointToSize(cellClass.xP1, _showsize), pointToSize(cellClass.xP2, _showsize));
                        graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), pointToSize(cellClass.xP1, _showsize), pointToSize(cellClass.xP2, _showsize));
                        graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), pointToSize(cellClass.xP1, _showsize), pointToSize(cellClass.xP4, _showsize));
                        graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), pointToSize(cellClass.xP2, _showsize), pointToSize(cellClass.xP3, _showsize));
                        graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), pointToSize(cellClass.xP3, _showsize), pointToSize(cellClass.xP4, _showsize));

                        graphics.DrawString($"P1", MyFont2, B, pointToSize(cellClass.xP1, _showsize));
                        graphics.DrawString($"P2", MyFont2, B, pointToSize(cellClass.xP2, _showsize));
                        graphics.DrawString($"P3", MyFont2, B, pointToSize(cellClass.xP3, _showsize));
                        graphics.DrawString($"P4", MyFont2, B, pointToSize(cellClass.xP4, _showsize));

                        //交叉线
                        graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), pointToSize(cellClass.xP1, _showsize), pointToSize(cellClass.xP3, _showsize));
                        graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), pointToSize(cellClass.xP2, _showsize), pointToSize(cellClass.xP4, _showsize));

                        PointF _recultPointf = pointToSize(cellClass.xPCenter, _showsize);
                        RectangleF rectangleF = SimpleRectF(_recultPointf, 3, 3);
                        graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth / _showsize), new RectangleF[] { rectangleF });
                        graphics.DrawString($"{cellClass.lblName}-{cellClass.nDesc}", MyFont2, B, ptf);


                        switch (myRecipe.PreMode)
                        {
                            case ProcessImageMode.V11:
                            case ProcessImageMode.V2:
                            case ProcessImageMode.V3:
                            case ProcessImageMode.V8://将结果画到结果图中

                                switch (myRecipe.fourStartPosition)
                                {
                                    case LaserOffsetStart.LeftBottom:

                                        _recultPointf = pointToSize(cellClass.xP4, _showsize);
                                        rectangleF = SimpleRectF(_recultPointf, 3, 3);
                                        graphics.DrawRectangles(new Pen(Color.Red, _lineWidth / _showsize), new RectangleF[] { rectangleF });
                                        //graphics.DrawString($"{cellClass.lblName}", MyFont2, B, ptf);


                                        break;
                                    default:


                                        break;
                                }
                                PointF _drawLaserPointF = pointToSize(cellClass.xPCenterLaserOffset, _showsize);
                                RectangleF _drawLaserRectF = SimpleRectF(_drawLaserPointF, 3, 3);
                                if (myRecipe.LaserLocX == 0 && myRecipe.LaserLocY == 0)
                                {

                                }
                                else
                                {
                                    graphics.DrawRectangles(new Pen(Color.Green, _lineWidth / _showsize), new RectangleF[] { _drawLaserRectF });

                                    //PointF _recultPointf = pointToSize(cellClass.xPCenter, _showsize);
                                    graphics.DrawLine(new Pen(Color.Blue, _lineWidth / _showsize), _drawLaserPointF, _recultPointf);
                                }

                                break;
                            case ProcessImageMode.V7:
                            case ProcessImageMode.V6:
                            case ProcessImageMode.V5:

                                _drawLaserPointF = pointToSize(cellClass.xPCenterLaserOffset, _showsize);
                                _drawLaserRectF = SimpleRectF(_drawLaserPointF, 3, 3);
                                graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth / _showsize), new RectangleF[] { _drawLaserRectF });

                                //PointF _recultPointf = pointToSize(cellClass.xPCenter, _showsize);
                                graphics.DrawLine(new Pen(Color.Lime, _lineWidth / _showsize), _drawLaserPointF, _recultPointf);

                                break;
                        }


                    }
                    else
                    {
                        graphics.DrawString($"{cellClass.lblName}-{cellClass.nDesc}", MyFont2, Bfail, ptf);
                    }

                    if (INI.Instance.IsUseFixedMark)
                    {
                        //Mark点
                        PointF _drawMarkPointF = pointToSize(MarkRunPtCenter, _showsize);
                        RectangleF _DrawMarkRectF = SimpleRectF(_drawMarkPointF, 3, 3);
                        graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth / _showsize), new RectangleF[] { _DrawMarkRectF });
                        graphics.DrawRectangles(new Pen(Color.Blue, _lineWidth / _showsize), new RectangleF[] { rectToSize(INI.Instance.mark_rect, _showsize) });
                    }
                    //if (string.IsNullOrEmpty(cellClass.Result))
                    //    graphics.DrawString("NONE", MyFont, B_fail, ptf);
                    //else
                    //    graphics.DrawString(cellClass.Result, MyFont, B, ptf);
                }
                graphics.DrawString(stopwatch.ElapsedMilliseconds.ToString("0.000") + " ms", MyFont1, B, new PointF(5, 5));
                graphics.Dispose();

                string _outputimage_path = myPath.Replace(".cfg", "-" + Index.ToString("000") + ".bmp");
                _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{Index.ToString()}_" + _saveFileName + ".jpg";

                FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

                Task task = new Task(() =>
                {
                    try
                    {
                        if (INI.Instance.IsSaveStripImage)
                        {
                            SaveImageWithQuality(freeImageBitmapInputResult.ToBitmap(), _outputimage_path, INI.Instance.ImageQuality);
                            //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                            //freeImageBitmapOutputDraw.Dispose();
                            freeImageBitmapInputResult.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        logInfo.Log($"保存图片错误{ex.Message} ");
                    }
                });
                task.Start();

            }

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"画出结果图{myElapsedTime} ms");

            //清除小图
            Parallel.ForEach(myListCell, (item1) =>
            {
                item1.BmpRun.Dispose();
                item1.BmpRun = new Bitmap(1, 1);
            });

            if (bmptemp != null)
                bmptemp.Dispose();

            freeImageBitmapInput.Dispose();

            GC.Collect();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
        }
        private void runTestV9_freeImage()
        {

            _saveDebugPicture = INI.Instance.IsSaveTestImage;
            //InitTrayUI();

            int iSize = 1;
            int _showsize = 1;

            if (INI.Instance.IsOpenShowSize)
                _showsize = INI.Instance.ShowSizeValue;

            //freeImageBitmapOutput = new FreeImageBitmap(freeImageBitmapInput);

            //return;
            m_ElapsedTime = 0;
            m_Running = true;
            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;
            m_ShowRectList.Clear();

            //ShowRectClass showRectClass = new ShowRectClass();
            //showRectClass.Description = "测试效果PASS";
            //showRectClass.IsPass = true;
            //showRectClass.Bounds = new RectangleF(100, 100, 100, 100);
            //m_ShowRectList.Add(showRectClass);

            //showRectClass = new ShowRectClass();
            //showRectClass.Description = "测试效果FAIL";
            //showRectClass.IsPass = false;
            //showRectClass.Bounds = new RectangleF(300, 300, 100, 100);
            //m_ShowRectList.Add(showRectClass);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            Point MarkRunPtCenter = new Point(0, 0);



            //清除结果
            foreach (RegionCellClass cellClass in myListCell)
            {
                cellClass.Result = "";
            }

            string _path = Universal.DEBUGRESULTPATH + "\\" + _index.ToString() + "-" + JzTimes.DateTimeSerialStringFFF + "";
            _path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + "-" + _saveFileName + "";
            //if (_saveDebugPicture)
            {
                if (!System.IO.Directory.Exists(_path))
                    System.IO.Directory.CreateDirectory(_path);
            }

            #region 判断方向已经不使用了改成单个mark点定位

            //外定位框
            RectangleF rect_cropF = new RectangleF(myRecipe.AnaDirRectF.X, myRecipe.AnaDirRectF.Y, myRecipe.AnaDirRectF.Width, myRecipe.AnaDirRectF.Height);
            string alignStr = string.Empty;
            try
            {
                if (myRecipe.inspect_dir_open)
                {
                    //训练
                    myRecipe.AnaTrainDir(myRecipe.bmpOrgAnaDirPattern);

                    rect_cropF.Inflate(myRecipe.inspect_dir_extendx, myRecipe.inspect_dir_extendy);
                    Bitmap bmpdirtemp = freeImageBitmapInput.ToBitmap().Clone(rect_cropF, PixelFormat.Format32bppArgb);
                    PointF orgCenter = GetRectCenterF(myRecipe.AnaDirRectF);
                    //判断方向测试

                    //bmpdirtemp.Save("D:\\bmpdirtemp.bmp", ImageFormat.Bmp);
                    m_IsPass = myRecipe.AnaRunDir(bmpdirtemp, out string descStr) == 0;
                    m_ResultDesc = descStr;

                    if (m_IsPass)
                    {
                        if (myRecipe.Ana_myFind_dir.xResults.Count > 0)
                        {
                            MarkRunPtCenter = new System.Drawing.Point(
                                (int)(rect_cropF.X + myRecipe.Ana_myFind_dir.xResults[0].fCenterX - orgCenter.X),
                                (int)(rect_cropF.Y + myRecipe.Ana_myFind_dir.xResults[0].fCenterY - orgCenter.Y));

                            //alignStr = $"定位成功，分数{myRecipe.Ana_myFind_dir.xResults[0].fScore}";
                            alignStr = $"{ToChangeLanguage($"定位成功,分数")}{myRecipe.Ana_myFind_dir.xResults[0].fScore}";
                            rect_cropF.X += MarkRunPtCenter.X;
                            rect_cropF.Y += MarkRunPtCenter.Y;
                        }
                    }

                    bmpdirtemp.Dispose();
                }
                else
                {
                    m_IsPass = true;
                    //m_ResultDesc = $"未启用定位";
                    m_ResultDesc = $"{ToChangeLanguage($"未启用定位")}";
                }
            }
            catch (Exception ex)
            {
                m_IsPass = false;
                //m_ResultDesc = $"方向判断错误 {ex.Message}";
                m_ResultDesc = $"{ToChangeLanguage($"方向判断错误")} {ex.Message}";
            }

            if (Universal.IsDrawImage)
            {
                if (!m_IsPass)
                {
                    freeImageBitmapOutputDraw.Dispose();
                    freeImageBitmapOutputDraw = new Bitmap(freeImageBitmapInput.ToBitmap(),
                                                                       new Size(freeImageBitmapInput.ToBitmap().Width / _showsize,
                                                                                       freeImageBitmapInput.ToBitmap().Height / _showsize));

                    Graphics graphics = Graphics.FromImage(freeImageBitmapOutputDraw);
                    SolidBrush B = new SolidBrush(Color.Lime);
                    SolidBrush Bfail = new SolidBrush(Color.Red);
                    SolidBrush B_fail = new SolidBrush(Color.Red);
                    //Font MyFont = new Font("Arial", 188);
                    //Font MyFont1 = new Font("Arial", 88);
                    Font MyFont1 = new Font("Arial", INI.Instance.DrawFontSize);
                    //Font MyFont2 = new Font("Arial", 33);
                    float _lineWidth = INI.Instance.DrawLineWidth;
                    Pen _findrectpen = new Pen(Color.Orange, _lineWidth);
                    _findrectpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    //外框


                    //RectangleF[] rectangleFs = new RectangleF[] { rectf_des1, rectf_des2, rectf_des3, rectf_des4 };
                    graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), new RectangleF[] { rect_cropF });
                    graphics.DrawString($"{m_ResultDesc}", MyFont1, Bfail, new PointF(rect_cropF.X + rect_cropF.Width, rect_cropF.Y));
                    graphics.Dispose();

                    string _outputimage_path = myPath.Replace(".cfg", "-" + Index.ToString("000") + ".bmp");
                    _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{Index.ToString()}_" + _saveFileName + ".jpg";

                    FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

                    Task task = new Task(() =>
                    {
                        try
                        {
                            if (INI.Instance.IsSaveStripImage)
                            {
                                SaveImageWithQuality(freeImageBitmapInputResult.ToBitmap(), _outputimage_path, INI.Instance.ImageQuality);
                                //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                                //freeImageBitmapOutputDraw.Dispose();
                                freeImageBitmapInputResult.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            logInfo.Log($"保存图片错误{ex.Message} ");
                        }
                    });
                    task.Start();

                    myElapsedTime = jzTimesDurRecord.msDuriation;
                    jzTimesDurRecord.Cut();
                    logInfo.Log($"判断方向{m_ResultDesc} 时间{myElapsedTime} ms");

                    freeImageBitmapInput.Dispose();

                    GC.Collect();

                    stopwatch.Stop();
                    m_ElapsedTime = stopwatch.ElapsedMilliseconds;
                    m_Running = false;

                    return;
                }
            }
            else
            {
                if (!m_IsPass)
                {
                    //显示时间
                    ShowRectClass showRectClass = new ShowRectClass();
                    showRectClass.IsPass = true;
                    showRectClass.Bounds = new RectangleF(5, 5, 1, 1);
                    showRectClass.Description = $"CalTime:{stopwatch.ElapsedMilliseconds.ToString("0.000")} ms";
                    m_ShowRectList.Add(showRectClass);

                    //定位显示
                    ShowRectClass showRectClass1 = new ShowRectClass();
                    showRectClass1.IsPass = m_IsPass;
                    showRectClass1.Bounds = new RectangleF(rect_cropF.X, rect_cropF.Y, rect_cropF.Width, rect_cropF.Height);
                    showRectClass1.Description = $"{m_ResultDesc}";

                    myElapsedTime = jzTimesDurRecord.msDuriation;
                    jzTimesDurRecord.Cut();
                    logInfo.Log($"判断方向{m_ResultDesc} 时间{myElapsedTime} ms");

                    freeImageBitmapInput.Dispose();

                    GC.Collect();

                    stopwatch.Stop();
                    m_ElapsedTime = stopwatch.ElapsedMilliseconds;
                    m_Running = false;

                    return;
                }
            }
           

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"判断方向{myElapsedTime} ms");

            #region 读码

            RectangleF rect_cropBarcode = new RectangleF(myRecipe.AnaReadBarcodeRectF.X, myRecipe.AnaReadBarcodeRectF.Y,
                                                        myRecipe.AnaReadBarcodeRectF.Width, myRecipe.AnaReadBarcodeRectF.Height);

            string _barcodeStr = string.Empty;
            if (myRecipe.IsOpenReadBarcode)
            {
                Bitmap bmpbarcodetemp = freeImageBitmapInput.ToBitmap().Clone(rect_cropBarcode, PixelFormat.Format32bppArgb);
                _barcodeStr = myRecipe.AnaReadBarcode(bmpbarcodetemp);
                bmpbarcodetemp.Dispose();
            }
            m_ReadBarcodeStr = _barcodeStr;
            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"读码{myElapsedTime} ms");

            #region 测试计算

            Bitmap[] _bmpmaps = new Bitmap[4];
            PointF[] _pointfs = new PointF[4];
            RectangleF[] _rect_crops = new RectangleF[4];
            RectangleF[] _rect_result = new RectangleF[4];

            RectangleF[] _rect_alignBlob = new RectangleF[4];

            int i = 0;

            while (i < 4)
            {
                switch (i)
                {
                    case 0:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark1.X, myRecipe.fourmark1.Y, myRecipe.fourmark1.Width, myRecipe.fourmark1.Height);
                        break;
                    case 1:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark2.X, myRecipe.fourmark2.Y, myRecipe.fourmark2.Width, myRecipe.fourmark2.Height);
                        break;
                    case 2:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark3.X, myRecipe.fourmark3.Y, myRecipe.fourmark3.Width, myRecipe.fourmark3.Height);
                        break;
                    case 3:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark4.X, myRecipe.fourmark4.Y, myRecipe.fourmark4.Width, myRecipe.fourmark4.Height);
                        break;
                }

                _rect_crops[i].X += MarkRunPtCenter.X;
                _rect_crops[i].Y += MarkRunPtCenter.Y;

                _pointfs[i] = new PointF(_rect_crops[i].X, _rect_crops[i].Y);
                _rect_result[i] = new RectangleF(_rect_crops[i].X, _rect_crops[i].Y, _rect_crops[i].Width, _rect_crops[i].Height);
                _rect_alignBlob[i] = new RectangleF(_rect_crops[i].X, _rect_crops[i].Y, _rect_crops[i].Width, _rect_crops[i].Height);

                i++;
            }


            //RectangleF rectf_des1 = myRecipe.fourmark1;
            //_bmpmaps[0] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(myRecipe.fourmark1, PixelFormat.Format24bppRgb);
            //RectangleF rectf_des2 = myRecipe.fourmark2;
            //_bmpmaps[1] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(myRecipe.fourmark2, PixelFormat.Format24bppRgb);
            //RectangleF rectf_des3 = myRecipe.fourmark3;
            //_bmpmaps[2] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(myRecipe.fourmark3, PixelFormat.Format24bppRgb);
            //RectangleF rectf_des4 = myRecipe.fourmark4;
            //_bmpmaps[3] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(myRecipe.fourmark4, PixelFormat.Format24bppRgb);

            i = 0;
            bool bOK = false;
            PointF[] MarkRunPtCenterX = new PointF[4];

            #region 多线程计算
            //foreach (var markitem in myRecipe.MarkCollectionStr)
            Parallel.For(0, 4, ix =>
            {
                var markitem = myRecipe.MarkCollectionStr[ix];
                _rect_crops[ix].Inflate(markitem.Extendx, markitem.Extendy);
                _bmpmaps[ix] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(_rect_crops[ix], PixelFormat.Format24bppRgb);

                if (INI.Instance.IsSaveTestImage)
                {
                    string _markpicpath = _path + "\\single_pic";
                    if (!System.IO.Directory.Exists(_markpicpath))
                        System.IO.Directory.CreateDirectory(_markpicpath);

                    _bmpmaps[ix].Save($"{_markpicpath}\\pic_org_{ix}.png", ImageFormat.Png);
                }

                bOK = markitem.AnaRun(_bmpmaps[ix]) == 0;
                //m_IsPass &= bOK;
                //m_ResultDesc += $"Mark{i + 1}<{markitem.nDesc}>;";

                if (bOK)
                {
                    //算出前后偏移距离
                    PointF orgCenter = GetRectCenterF(_rect_crops[ix]);
                    float _w = _rect_crops[ix].Width;
                    float _h = _rect_crops[ix].Height;
                    switch (ix)
                    {
                        case 0:
                            orgCenter = GetRectCenterF(myRecipe.fourmark1);
                            _w = myRecipe.fourmark1.Width;
                            _h = myRecipe.fourmark1.Height;
                            break;
                        case 1:
                            orgCenter = GetRectCenterF(myRecipe.fourmark2);
                            _w = myRecipe.fourmark2.Width;
                            _h = myRecipe.fourmark2.Height;
                            break;
                        case 2:
                            orgCenter = GetRectCenterF(myRecipe.fourmark3);
                            _w = myRecipe.fourmark3.Width;
                            _h = myRecipe.fourmark3.Height;
                            break;
                        case 3:
                            orgCenter = GetRectCenterF(myRecipe.fourmark4);
                            _w = myRecipe.fourmark4.Width;
                            _h = myRecipe.fourmark4.Height;
                            break;
                    }
                    //PointF MarkRunPtCenterX = new System.Drawing.Point(
                    //            (int)(_rect_crops[i].X + markitem.Ana_myFind_dir.xResults[0].fCenterX),
                    //            (int)(_rect_crops[i].Y + myRecipe.Ana_myFind_dir.xResults[0].fCenterY));

                    if (markitem.Ana_myFind_dir.xResults.Count > 0)
                    {
                        MarkRunPtCenterX[ix] = new System.Drawing.Point(
                                    (int)(_rect_crops[ix].X + markitem.Ana_myFind_dir.xResults[0].fCenterX - orgCenter.X),
                                    (int)(_rect_crops[ix].Y + markitem.Ana_myFind_dir.xResults[0].fCenterY - orgCenter.Y));
                    }

                    _rect_alignBlob[ix].X += MarkRunPtCenterX[ix].X - MarkRunPtCenter.X;
                    _rect_alignBlob[ix].Y += MarkRunPtCenterX[ix].Y - MarkRunPtCenter.Y;

                    _bmpmaps[ix].Dispose();
                    _bmpmaps[ix] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(_rect_alignBlob[ix], PixelFormat.Format24bppRgb);

                    PointF ptf = markitem.GetImageMarkCenter(_bmpmaps[ix], false, out RectangleF max, out Bitmap bmptempx, out int maxareatemp);

                    if (INI.Instance.IsSaveTestImage)
                    {
                        string _markpicpath = _path + "\\single_pic";
                        if (!System.IO.Directory.Exists(_markpicpath))
                            System.IO.Directory.CreateDirectory(_markpicpath);

                        //_bmpmaps[i].Save($"{_markpicpath}\\pic_org_{i}.png", ImageFormat.Png);
                        bmptempx.Save($"{_markpicpath}\\pic{ix}.png", ImageFormat.Png);
                    }

                    bOK = markitem.CheckBlobSize(max, maxareatemp) == 0;
                    //m_IsPass &= bOK;
                    //m_ResultDesc += $"<{markitem.nDesc2}>;";
                    _rect_result[ix] = new RectangleF(max.X + _rect_alignBlob[ix].X, max.Y + _rect_alignBlob[ix].Y, max.Width, max.Height);
                    _pointfs[ix] = new PointF(ptf.X + _rect_alignBlob[ix].X, ptf.Y + _rect_alignBlob[ix].Y);

                }

                //i++;
            }
            );
            #endregion

            foreach (var markitem in myRecipe.MarkCollectionStr)
            {
                bOK = markitem.nDesc.Contains("F0");
                m_IsPass &= bOK;
                m_ResultDesc += $"Mark{i + 1}<{markitem.nDesc}>;";
                if (bOK)
                {
                    bOK = markitem.nDesc2.Contains("OK");
                    m_IsPass &= bOK;
                    m_ResultDesc += $"<{markitem.nDesc2}>;";
                }
            }

            #region 单线程计算
#if SINGLE_PROCESS
 i = 0;
            bool bOK = false;
            PointF[] MarkRunPtCenterX = new PointF[4];
            foreach (var markitem in myRecipe.MarkCollectionStr)
            {
                _rect_crops[i].Inflate(markitem.Extendx, markitem.Extendy);
                _bmpmaps[i] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(_rect_crops[i], PixelFormat.Format24bppRgb);

                if (INI.Instance.IsAutoTestSavePicture)
                {
                    string _markpicpath = _path + "\\single_pic";
                    if (!System.IO.Directory.Exists(_markpicpath))
                        System.IO.Directory.CreateDirectory(_markpicpath);

                    _bmpmaps[i].Save($"{_markpicpath}\\pic_org_{i}.png", ImageFormat.Png);
                }

                bOK = markitem.AnaRun(_bmpmaps[i]) == 0;
                m_IsPass &= bOK;
                m_ResultDesc += $"Mark{i + 1}<{markitem.nDesc}>;";

                if (bOK)
                {
                    //算出前后偏移距离
                    PointF orgCenter = GetRectCenterF(_rect_crops[i]);
                    float _w = _rect_crops[i].Width;
                    float _h = _rect_crops[i].Height;
                    switch (i)
                    {
                        case 0:
                            orgCenter = GetRectCenterF(myRecipe.fourmark1);
                            _w = myRecipe.fourmark1.Width;
                            _h = myRecipe.fourmark1.Height;
                            break;
                        case 1:
                            orgCenter = GetRectCenterF(myRecipe.fourmark2);
                            _w = myRecipe.fourmark2.Width;
                            _h = myRecipe.fourmark2.Height;
                            break;
                        case 2:
                            orgCenter = GetRectCenterF(myRecipe.fourmark3);
                            _w = myRecipe.fourmark3.Width;
                            _h = myRecipe.fourmark3.Height;
                            break;
                        case 3:
                            orgCenter = GetRectCenterF(myRecipe.fourmark4);
                            _w = myRecipe.fourmark4.Width;
                            _h = myRecipe.fourmark4.Height;
                            break;
                    }
                    //PointF MarkRunPtCenterX = new System.Drawing.Point(
                    //            (int)(_rect_crops[i].X + markitem.Ana_myFind_dir.xResults[0].fCenterX),
                    //            (int)(_rect_crops[i].Y + myRecipe.Ana_myFind_dir.xResults[0].fCenterY));

                    if (markitem.Ana_myFind_dir.xResults.Count > 0)
                    {
                        MarkRunPtCenterX[i] = new System.Drawing.Point(
                                    (int)(_rect_crops[i].X + markitem.Ana_myFind_dir.xResults[0].fCenterX - orgCenter.X),
                                    (int)(_rect_crops[i].Y + markitem.Ana_myFind_dir.xResults[0].fCenterY - orgCenter.Y));
                    }

                    _rect_alignBlob[i].X += MarkRunPtCenterX[i].X - MarkRunPtCenter.X;
                    _rect_alignBlob[i].Y += MarkRunPtCenterX[i].Y - MarkRunPtCenter.Y;

                    //_rect_alignBlob[i] = new RectangleF(orgCenter.X + MarkRunPtCenterX.X,
                    //   orgCenter.Y + MarkRunPtCenterX.Y,
                    //    _w,
                    //    _h);

                    //_rect_crops[i].X += MarkRunPtCenterX.X;
                    //_rect_crops[i].Y += MarkRunPtCenterX.Y;

                    _bmpmaps[i].Dispose();
                    _bmpmaps[i] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(_rect_alignBlob[i], PixelFormat.Format24bppRgb);
                    //_bmpmaps[i] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(_rect_crops[i], PixelFormat.Format24bppRgb);

                    PointF ptf = markitem.GetImageMarkCenter(_bmpmaps[i], false, out RectangleF max, out Bitmap bmptempx, out int maxareatemp);

                    if (INI.Instance.IsAutoTestSavePicture)
                    {
                        string _markpicpath = _path + "\\single_pic";
                        if (!System.IO.Directory.Exists(_markpicpath))
                            System.IO.Directory.CreateDirectory(_markpicpath);

                        //_bmpmaps[i].Save($"{_markpicpath}\\pic_org_{i}.png", ImageFormat.Png);
                        bmptempx.Save($"{_markpicpath}\\pic{i}.png", ImageFormat.Png);
                    }

                    bOK = markitem.CheckBlobSize(max, maxareatemp) == 0;
                    m_IsPass &= bOK;
                    m_ResultDesc += $"<{markitem.nDesc2}>;";
                    _rect_result[i] = new RectangleF(max.X + _rect_alignBlob[i].X, max.Y + _rect_alignBlob[i].Y, max.Width, max.Height);
                    _pointfs[i] = new PointF(ptf.X + _rect_alignBlob[i].X, ptf.Y + _rect_alignBlob[i].Y);
                    //switch (i)
                    //{
                    //    case 0:
                    //        rectf_des1 = new RectangleF(max.X + myRecipe.fourmark1.X, max.Y + myRecipe.fourmark1.Y, max.Width, max.Height);
                    //        _pointfs[i] = new PointF(ptf.X + myRecipe.fourmark1.X, ptf.Y + myRecipe.fourmark1.Y);
                    //        break;
                    //    case 1:
                    //        rectf_des2 = new RectangleF(max.X + myRecipe.fourmark2.X, max.Y + myRecipe.fourmark2.Y, max.Width, max.Height);
                    //        _pointfs[i] = new PointF(ptf.X + myRecipe.fourmark2.X, ptf.Y + myRecipe.fourmark2.Y);
                    //        break;
                    //    case 2:
                    //        rectf_des3 = new RectangleF(max.X + myRecipe.fourmark3.X, max.Y + myRecipe.fourmark3.Y, max.Width, max.Height);
                    //        _pointfs[i] = new PointF(ptf.X + myRecipe.fourmark3.X, ptf.Y + myRecipe.fourmark3.Y);
                    //        break;
                    //    case 3:
                    //        rectf_des4 = new RectangleF(max.X + myRecipe.fourmark4.X, max.Y + myRecipe.fourmark4.Y, max.Width, max.Height);
                    //        _pointfs[i] = new PointF(ptf.X + myRecipe.fourmark4.X, ptf.Y + myRecipe.fourmark4.Y);
                    //        break;
                    //}
                }

                i++;
            }
#endif
            #endregion

            PointF ptf1 = _pointfs[0];
            PointF ptf2 = _pointfs[1];
            PointF ptf3 = _pointfs[2];
            PointF ptf4 = _pointfs[3];

            RectangleF rectangleFmark1 = SimpleRectF(ptf1, 2, 2);
            RectangleF rectangleFmark2 = SimpleRectF(ptf2, 2, 2);
            RectangleF rectangleFmark3 = SimpleRectF(ptf3, 2, 2);
            RectangleF rectangleFmark4 = SimpleRectF(ptf4, 2, 2);

            #region MASK_NO_USE
            ////点1
            //RectangleF rectf_des1 = myRecipe.fourmark1;
            //System.Drawing.PointF ptf1 = calMarkBlob(freeImageBitmapInput.ToBitmap(), 
            //    myRecipe.fourmark1, 
            //    myRecipe.thresholdv_mark1, 
            //    out rectf_des1, 
            //    myRecipe.findwhite_mark1);
            //RectangleF rectangleFmark1 = SimpleRectF(ptf1, 2, 2);

            ////点2
            //RectangleF rectf_des2 = myRecipe.fourmark2;
            //System.Drawing.PointF ptf2 = calMarkBlob(freeImageBitmapInput.ToBitmap(),
            //    myRecipe.fourmark2,
            //    myRecipe.thresholdv_mark2,
            //    out rectf_des2,
            //    myRecipe.findwhite_mark2);
            //RectangleF rectangleFmark2 = SimpleRectF(ptf2, 2, 2);

            ////点3
            //RectangleF rectf_des3= myRecipe.fourmark3;
            //System.Drawing.PointF ptf3 = calMarkBlob(freeImageBitmapInput.ToBitmap(),
            //    myRecipe.fourmark3,
            //    myRecipe.thresholdv_mark3,
            //    out rectf_des3,
            //    myRecipe.findwhite_mark3);
            //RectangleF rectangleFmark3 = SimpleRectF(ptf3, 2, 2);

            ////点4
            //RectangleF rectf_des4 = myRecipe.fourmark4;
            //System.Drawing.PointF ptf4 = calMarkBlob(freeImageBitmapInput.ToBitmap(),
            //    myRecipe.fourmark4,
            //    myRecipe.thresholdv_mark4,
            //    out rectf_des4,
            //    myRecipe.findwhite_mark4);
            //RectangleF rectangleFmark4 = SimpleRectF(ptf4, 2, 2);
            #endregion

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"测试计算{myElapsedTime} ms");

            #region  保存资料

            PointF ptfw1 = ToWorld(ptf1);
            PointF ptfw2 = ToWorld(ptf2);
            PointF ptfw3 = ToWorld(ptf3);
            PointF ptfw4 = ToWorld(ptf4);

            List<PointF> _laser_pointfs = new List<PointF>();

            PointF ptflaser1 = ToLaserCmd(ptfw1);
            PointF ptflaser2 = ToLaserCmd(ptfw2);
            PointF ptflaser3 = ToLaserCmd(ptfw3);
            PointF ptflaser4 = ToLaserCmd(ptfw4);

            _laser_pointfs.Add(ptflaser1);
            _laser_pointfs.Add(ptflaser2);
            _laser_pointfs.Add(ptflaser3);
            _laser_pointfs.Add(ptflaser4);

            double dMarkDistance12 = Math.Round(GetPointLength(ptflaser1, ptflaser2), 4);
            double dMarkDistance34 = Math.Round(GetPointLength(ptflaser3, ptflaser4), 4);

            //alignStr += Environment.NewLine;
            //alignStr += $"Mark1和Mark2间距{dMarkDistance12}超出范围[{myRecipe.stand_width - myRecipe.stand_width_lower},{myRecipe.stand_width + myRecipe.stand_width_upper}]";
            //alignStr += Environment.NewLine;

            //m_IsPass = IsInRangeEx(dMarkDistance12, myRecipe.stand_width + myRecipe.stand_width_upper, myRecipe.stand_width - myRecipe.stand_width_lower);

            if (!IsInRangeEx(dMarkDistance12, myRecipe.stand_width + myRecipe.stand_width_upper, myRecipe.stand_width - myRecipe.stand_width_lower))
            {
                m_IsPass = false;
                alignStr += Environment.NewLine;
                alignStr += $"{ToChangeLanguage($"Mark1和Mark2间距")}{dMarkDistance12}{ToChangeLanguage("超出范围")}[{myRecipe.stand_width - myRecipe.stand_width_lower},{myRecipe.stand_width + myRecipe.stand_width_upper}]";

                alignStr += Environment.NewLine;
            }
            else
            {
                alignStr += Environment.NewLine;
                //alignStr += $"Mark1和Mark2间距{dMarkDistance12}范围[{myRecipe.stand_width - myRecipe.stand_width_lower},{myRecipe.stand_width + myRecipe.stand_width_upper}]";
                alignStr += $"{ToChangeLanguage($"Mark1和Mark2间距")}{dMarkDistance12}{ToChangeLanguage("范围")}[{myRecipe.stand_width - myRecipe.stand_width_lower},{myRecipe.stand_width + myRecipe.stand_width_upper}]";

                alignStr += Environment.NewLine;
            }


            //保存资料
            string dataStr = $"Barcode,{_barcodeStr}" + Environment.NewLine;
            dataStr += "Index,Name,ViewX,ViewY,WorldX,WorldY,Distance," + Environment.NewLine;
            dataStr += $"1,point1,{PointF000ToString(ptf1)},{PointF000ToString(ptflaser1)},{dMarkDistance12.ToString()}," + Environment.NewLine;
            dataStr += $"2,point2,{PointF000ToString(ptf2)},{PointF000ToString(ptflaser2)},{dMarkDistance12.ToString()}," + Environment.NewLine;
            dataStr += $"3,point3,{PointF000ToString(ptf3)},{PointF000ToString(ptflaser3)},{dMarkDistance34.ToString()}," + Environment.NewLine;
            dataStr += $"4,point4,{PointF000ToString(ptf4)},{PointF000ToString(ptflaser4)},{dMarkDistance34.ToString()}," + Environment.NewLine;

            string _dataStr_path = $"{_path}\\{JzTimes.DateTimeSerialStringFFF}.csv";
            SaveData(dataStr, _dataStr_path);

            #region 保存xml 供镭雕机调用
            if (m_IsPass)
            {
                if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
                {
                    if (!Directory.Exists(INI.Instance.LaserSharePath))
                        Directory.CreateDirectory(INI.Instance.LaserSharePath);
                    if (Directory.Exists(INI.Instance.LaserSharePath))
                    {
                        XmlDocument xmldoc;
                        XmlNode xmlnode;
                        XmlElement xmlelem;

                        xmldoc = new XmlDocument();
                        XmlDeclaration xmldecl;
                        xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                        xmldoc.AppendChild(xmldecl);

                        //加入一个根元素
                        xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                        xmldoc.AppendChild(xmlelem);

                        //   < !--AlignType 定位类型:
                        // 1 - 单颗位置定位，给出所有单元的定位位置，相关标签: < Matrix > < Cells >
                        // 2 - 整版使用Mark定位，给出多个Mark(比如圆孔)的定位坐标 ， 相关标签<Marks> < Mark >
                        //-->


                        XmlNode xeAlignType = xmldoc.CreateElement("AlignType");
                        xeAlignType.InnerText = "2";
                        xmlelem.AppendChild(xeAlignType);

                        XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                        xmlelem.AppendChild(xeMatrix);

                        XmlElement x2 = xmldoc.CreateElement("Center");
                        x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                        x2.SetAttribute("y", (0).ToString());
                        xeMatrix.AppendChild(x2);
                        x2 = xmldoc.CreateElement("Angle");
                        x2.InnerText = "0";
                        xeMatrix.AppendChild(x2);
                        x2 = xmldoc.CreateElement("Row");
                        x2.InnerText = Row.ToString();
                        xeMatrix.AppendChild(x2);
                        x2 = xmldoc.CreateElement("Col");
                        x2.InnerText = Column.ToString();
                        xeMatrix.AppendChild(x2);

                        XmlNode xeCells = xmldoc.CreateElement("Cells");
                        xmlelem.AppendChild(xeCells);

                        i = 0;
                        foreach (RegionCellClass itemClass in myListCell)
                        {
                            double fx = 0;// itemClass.xPCenterOffsetWorld.X + myRecipe.LaserLocX + INI.Instance.Cal_Bcx;// + (int)numericUpDown1.Value;
                            double fy = 0;// itemClass.xPCenterOffsetWorld.Y + myRecipe.LaserLocY + INI.Instance.Cal_Bcy;// + (int)numericUpDown2.Value;
                            double angleCal = 0d;

                            x2 = xmldoc.CreateElement("cell" + i.ToString());
                            xeCells.AppendChild(x2);

                            XmlElement x3 = xmldoc.CreateElement("Center");
                            x3.SetAttribute("x", fx.ToString(m_format));
                            x3.SetAttribute("y", fy.ToString(m_format));
                            x2.AppendChild(x3);
                            x3 = xmldoc.CreateElement("dbAngle");
                            x3.InnerText = (angleCal).ToString(m_format);
                            x2.AppendChild(x3);
                            x3 = xmldoc.CreateElement("nType");
                            x3.InnerText = itemClass.nType.ToString();
                            x2.AppendChild(x3);

                            i++;
                        }

                        XmlElement xeMarks = xmldoc.CreateElement("Marks");
                        xeMarks.SetAttribute("mark_num", _laser_pointfs.Count.ToString());
                        xmlelem.AppendChild(xeMarks);

                        i = 0;
                        foreach (PointF ptfx in _laser_pointfs)
                        {
                            double fx = ptfx.X + INI.Instance.Cal_Bcx;
                            double fy = ptfx.Y + INI.Instance.Cal_Bcy;

                            x2 = xmldoc.CreateElement("Mark" + i.ToString());
                            x2.SetAttribute("CenterX", fx.ToString(m_format));
                            x2.SetAttribute("CenterY", fy.ToString(m_format));
                            xeMarks.AppendChild(x2);

                            i++;
                        }

                        xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                    }
                }
            }

            #endregion


            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"生成资料档案{myElapsedTime} ms");

            #region 画图
            if (Universal.IsDrawImage)
            {
                //画图
                if (m_IsDrawRect)
                {
                    //JzToolsClass jzTools = new JzToolsClass();
                    freeImageBitmapOutputDraw.Dispose();
                    freeImageBitmapOutputDraw = new Bitmap(freeImageBitmapInput.ToBitmap(),
                                                                       new Size(freeImageBitmapInput.ToBitmap().Width / _showsize,
                                                                                       freeImageBitmapInput.ToBitmap().Height / _showsize));

                    Graphics graphics = Graphics.FromImage(freeImageBitmapOutputDraw);

                    SolidBrush B = new SolidBrush(Color.Lime);
                    SolidBrush Bfail = new SolidBrush(Color.Red);
                    //SolidBrush B_fail = new SolidBrush(Color.Red);
                    //Font MyFont = new Font("Arial", 188);
                    //Font MyFont1 = new Font("Arial", 88);
                    Font MyFont1 = new Font("Arial", INI.Instance.DrawFontSize);
                    //Font MyFont2 = new Font("Arial", 33);
                    float _lineWidth = INI.Instance.DrawLineWidth;
                    Pen _findrectpen = new Pen(Color.Orange, _lineWidth);
                    _findrectpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    //外框


                    //RectangleF[] rectangleFs = new RectangleF[] { rectf_des1, rectf_des2, rectf_des3, rectf_des4 };
                    graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), new RectangleF[] { rect_cropF, rect_cropBarcode, rectangleFmark1, rectangleFmark2, rectangleFmark3, rectangleFmark4 });
                    graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), _rect_result);
                    //graphics.DrawRectangles(new Pen(Color.Blue, _lineWidth), new RectangleF[] { myRecipe.fourmark1, myRecipe.fourmark2, myRecipe.fourmark3, myRecipe.fourmark4 });
                    graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), _rect_alignBlob);

                    graphics.DrawRectangles(_findrectpen, _rect_crops);
                    if (m_IsPass)
                        graphics.DrawString($"{alignStr}", MyFont1, B, new PointF(rect_cropF.X + rect_cropF.Width, rect_cropF.Y));
                    else
                        graphics.DrawString($"{alignStr}", MyFont1, Bfail, new PointF(rect_cropF.X + rect_cropF.Width, rect_cropF.Y));

                    graphics.DrawString($"{_barcodeStr}", MyFont1, B, new PointF(rect_cropBarcode.X + rect_cropBarcode.Width, rect_cropBarcode.Y));

                    i = 0;
                    foreach (var markitem in myRecipe.MarkCollectionStr)
                    {
                        if (markitem.nDesc.Contains("F0") && markitem.nDesc2.Contains("OK"))
                            graphics.DrawString($"Mark{i + 1}<{markitem.nDesc}><{markitem.nDesc2}>", MyFont1, B, new PointF(_rect_result[i].X + _rect_result[i].Width, _rect_result[i].Y));
                        else
                            graphics.DrawString($"Mark{i + 1}<{markitem.nDesc}><{markitem.nDesc2}>", MyFont1, Bfail, new PointF(_rect_result[i].X + _rect_result[i].Width, _rect_result[i].Y));
                        i++;
                    }

                    graphics.DrawString(stopwatch.ElapsedMilliseconds.ToString("0.000") + " ms", MyFont1, B, new PointF(5, 5));
                    graphics.Dispose();

                    string _outputimage_path = myPath.Replace(".cfg", "-" + Index.ToString("000") + ".bmp");
                    _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{Index.ToString()}_" + _saveFileName + ".jpg";

                    FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

                    Task task = new Task(() =>
                    {
                        try
                        {
                            if (INI.Instance.IsSaveStripImage)
                            {
                                SaveImageWithQuality(freeImageBitmapInputResult.ToBitmap(), _outputimage_path, INI.Instance.ImageQuality);
                                //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                                //freeImageBitmapOutputDraw.Dispose();
                                freeImageBitmapInputResult.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            logInfo.Log($"保存图片错误{ex.Message} ");
                        }
                    });
                    task.Start();

                }

            }
            else
            {
                //画图
                if (m_IsDrawRect)
                {
                    //显示时间
                    ShowRectClass showRectClass = new ShowRectClass();
                    showRectClass.IsPass = true;
                    showRectClass.Bounds = new RectangleF(5, 5, 1, 1);
                    showRectClass.Description = $"CalTime:{stopwatch.ElapsedMilliseconds.ToString("0.000")} ms";
                    m_ShowRectList.Add(showRectClass);

                    //定位显示
                    ShowRectClass showRectClass1 = new ShowRectClass();
                    showRectClass1.IsPass = m_IsPass;
                    showRectClass1.Bounds = new RectangleF(rect_cropF.X, rect_cropF.Y, rect_cropF.Width, rect_cropF.Height);
                    showRectClass1.Description = $"{alignStr}";
                    m_ShowRectList.Add(showRectClass1);

                    if (myRecipe.IsOpenReadBarcode)
                    {
                        //条码显示
                        ShowRectClass showRectClass2 = new ShowRectClass();
                        showRectClass2.IsPass = true;
                        showRectClass2.Bounds = new RectangleF(rect_cropBarcode.X, rect_cropBarcode.Y, rect_cropBarcode.Width, rect_cropBarcode.Height);
                        showRectClass2.Description = $"{_barcodeStr}";
                        m_ShowRectList.Add(showRectClass2);
                    }
                    //Mark点显示
                    i = 0;
                    foreach (var markitem in myRecipe.MarkCollectionStr)
                    {
                        RectangleF temp = new RectangleF(_rect_result[i].X, _rect_result[i].Y, _rect_result[i].Width, _rect_result[i].Height);
                        RectangleF temp1 = new RectangleF(_rect_result[i].X, _rect_result[i].Y, _rect_result[i].Width, _rect_result[i].Height);
                        temp1.Inflate(temp.Width, temp.Height);

                        ShowRectClass showRectClass3 = new ShowRectClass();
                        showRectClass3.Bounds = new RectangleF(temp.X, temp.Y, temp.Width, temp.Height);
                        showRectClass3.Description = $"Mark{i + 1}<{markitem.nDesc}><{markitem.nDesc2}>";
                        showRectClass3.IsPass = markitem.nDesc.Contains("F0") && markitem.nDesc2.Contains("OK");
                        m_ShowRectList.Add(showRectClass3);
                        //显示一样大小的框
                        ShowRectClass showRectClass4 = new ShowRectClass();
                        showRectClass4.Bounds = new RectangleF(temp1.X, temp1.Y, temp1.Width, temp1.Height);
                        showRectClass4.Description = "";
                        showRectClass4.IsPass = true;
                        m_ShowRectList.Add(showRectClass4);

                        i++;
                    }
                }
            }

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"画出结果图{myElapsedTime} ms");

            freeImageBitmapInput.Dispose();

            GC.Collect();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
        }
        private void runTestV10_freeImage()
        {

            _saveDebugPicture = INI.Instance.IsSaveTestImage;
            //InitTrayUI();

            int iSize = 1;
            int _showsize = 1;

            if (INI.Instance.IsOpenShowSize)
                _showsize = INI.Instance.ShowSizeValue;

            //freeImageBitmapOutput = new FreeImageBitmap(freeImageBitmapInput);

            //return;
            m_ElapsedTime = 0;
            m_Running = true;
            JzTimes jzTimesDurRecord = new JzTimes();
            jzTimesDurRecord.Cut();
            int myElapsedTime = 0;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            Point MarkRunPtCenter = new Point(0, 0);



            //清除结果
            foreach (RegionCellClass cellClass in myListCell)
            {
                cellClass.Result = "";
            }

            string _path = Universal.DEBUGRESULTPATH + "\\" + _index.ToString() + "-" + JzTimes.DateTimeSerialStringFFF + "";
            _path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + "-" + _saveFileName + "";
            //if (_saveDebugPicture)
            {
                if (!System.IO.Directory.Exists(_path))
                    System.IO.Directory.CreateDirectory(_path);
            }

            #region 判断方向已经不使用了改成单个mark点定位

            //外定位框
            RectangleF rect_cropF = new RectangleF(myRecipe.AnaDirRectF.X, myRecipe.AnaDirRectF.Y, myRecipe.AnaDirRectF.Width, myRecipe.AnaDirRectF.Height);
            string alignStr = string.Empty;
            try
            {
                //训练
                myRecipe.AnaTrainDir(myRecipe.bmpOrgAnaDirPattern);

                rect_cropF.Inflate(myRecipe.inspect_dir_extendx, myRecipe.inspect_dir_extendy);
                Bitmap bmpdirtemp = freeImageBitmapInput.ToBitmap().Clone(rect_cropF, PixelFormat.Format32bppArgb);
                PointF orgCenter = GetRectCenterF(myRecipe.AnaDirRectF);
                //判断方向测试

                //bmpdirtemp.Save("D:\\bmpdirtemp.bmp", ImageFormat.Bmp);
                m_IsPass = myRecipe.AnaRunDir(bmpdirtemp, out string descStr) == 0;
                m_ResultDesc = descStr;

                if (m_IsPass)
                {
                    if (myRecipe.Ana_myFind_dir.xResults.Count > 0)
                    {
                        MarkRunPtCenter = new System.Drawing.Point(
                            (int)(rect_cropF.X + myRecipe.Ana_myFind_dir.xResults[0].fCenterX - orgCenter.X),
                            (int)(rect_cropF.Y + myRecipe.Ana_myFind_dir.xResults[0].fCenterY - orgCenter.Y));

                        alignStr = $"定位成功，分数{myRecipe.Ana_myFind_dir.xResults[0].fScore}";

                        rect_cropF.X += MarkRunPtCenter.X;
                        rect_cropF.Y += MarkRunPtCenter.Y;
                    }
                }

                bmpdirtemp.Dispose();
            }
            catch (Exception ex)
            {
                m_IsPass = false;
                m_ResultDesc = $"方向判断错误 {ex.Message}";
            }

            if (!m_IsPass)
            {
                freeImageBitmapOutputDraw.Dispose();
                freeImageBitmapOutputDraw = new Bitmap(freeImageBitmapInput.ToBitmap(),
                                                                   new Size(freeImageBitmapInput.ToBitmap().Width / _showsize,
                                                                                   freeImageBitmapInput.ToBitmap().Height / _showsize));

                Graphics graphics = Graphics.FromImage(freeImageBitmapOutputDraw);
                SolidBrush B = new SolidBrush(Color.Lime);
                SolidBrush Bfail = new SolidBrush(Color.Red);
                SolidBrush B_fail = new SolidBrush(Color.Red);
                Font MyFont = new Font("Arial", 188);
                //Font MyFont1 = new Font("Arial", 88);
                Font MyFont1 = new Font("Arial", 100);
                Font MyFont2 = new Font("Arial", 33);
                float _lineWidth = 11f;
                Pen _findrectpen = new Pen(Color.Orange, _lineWidth);
                _findrectpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                //外框


                //RectangleF[] rectangleFs = new RectangleF[] { rectf_des1, rectf_des2, rectf_des3, rectf_des4 };
                graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), new RectangleF[] { rect_cropF });
                graphics.DrawString($"{m_ResultDesc}", MyFont1, Bfail, new PointF(rect_cropF.X + rect_cropF.Width, rect_cropF.Y));
                graphics.Dispose();

                string _outputimage_path = myPath.Replace(".cfg", "-" + Index.ToString("000") + ".bmp");
                _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{Index.ToString()}_" + _saveFileName + ".jpg";

                FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

                Task task = new Task(() =>
                {
                    try
                    {
                        if (INI.Instance.IsSaveStripImage)
                        {
                            SaveImageWithQuality(freeImageBitmapInputResult.ToBitmap(), _outputimage_path, INI.Instance.ImageQuality);
                            //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                            //freeImageBitmapOutputDraw.Dispose();
                            freeImageBitmapInputResult.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        logInfo.Log($"保存图片错误{ex.Message} ");
                    }
                });
                task.Start();

                myElapsedTime = jzTimesDurRecord.msDuriation;
                jzTimesDurRecord.Cut();
                logInfo.Log($"判断方向{m_ResultDesc} 时间{myElapsedTime} ms");

                freeImageBitmapInput.Dispose();

                GC.Collect();

                stopwatch.Stop();
                m_ElapsedTime = stopwatch.ElapsedMilliseconds;
                m_Running = false;

                return;
            }

            #endregion

            #region 测试计算

            FindRectByVM findRectByVM = new FindRectByVM(SolMode.Sol_FindMark);

            Bitmap[] _bmpmaps = new Bitmap[4];
            PointF[] _pointfs = new PointF[4];
            RectangleF[] _rect_crops = new RectangleF[4];
            RectangleF[] _rect_result = new RectangleF[4];
            int i = 0;

            while (i < 4)
            {
                switch (i)
                {
                    case 0:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark1.X, myRecipe.fourmark1.Y, myRecipe.fourmark1.Width, myRecipe.fourmark1.Height);
                        break;
                    case 1:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark2.X, myRecipe.fourmark2.Y, myRecipe.fourmark2.Width, myRecipe.fourmark2.Height);
                        break;
                    case 2:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark3.X, myRecipe.fourmark3.Y, myRecipe.fourmark3.Width, myRecipe.fourmark3.Height);
                        break;
                    case 3:
                        _rect_crops[i] = new RectangleF(myRecipe.fourmark4.X, myRecipe.fourmark4.Y, myRecipe.fourmark4.Width, myRecipe.fourmark4.Height);
                        break;
                }

                _rect_crops[i].X += MarkRunPtCenter.X;
                _rect_crops[i].Y += MarkRunPtCenter.Y;

                _pointfs[i] = new PointF(_rect_crops[i].X, _rect_crops[i].Y);
                _rect_result[i] = new RectangleF(_rect_crops[i].X, _rect_crops[i].Y, _rect_crops[i].Width, _rect_crops[i].Height);

                i++;
            }

            i = 0;
            bool bOK = false;
            foreach (var markitem in myRecipe.MarkCollectionStr)
            {
                _rect_crops[i].Inflate(markitem.Extendx, markitem.Extendy);
                _bmpmaps[i] = (Bitmap)freeImageBitmapInput.ToBitmap().Clone(_rect_crops[i], PixelFormat.Format24bppRgb);

                PointF tmppointf = findRectByVM.RunFindMark(_bmpmaps[i], out float scoretemp);

                if (scoretemp >= markitem.Tolerance)
                {
                    bOK = true;
                    markitem.nDesc = $"F0{ToChangeLanguage("定位成功,分数")}{scoretemp}";
                }
                else
                {
                    bOK = false;
                    markitem.nDesc = $"-1{ToChangeLanguage("定位失败,分数")}{scoretemp}";
                }

                //bOK = markitem.AnaRun(_bmpmaps[i]) == 0;
                m_IsPass &= bOK;
                m_ResultDesc += $"Mark{i + 1}<{markitem.nDesc}>;";

                if (bOK)
                {
                    if (tmppointf.X != 0 && tmppointf.Y != 0)
                    {
                        markitem.nDesc2 = $"{ToChangeLanguage("定位点OK")}";
                    }
                    else
                    {
                        markitem.nDesc2 = $"{ToChangeLanguage("定位点NG")}";
                    }

                    m_IsPass &= bOK;
                    m_ResultDesc += $"<{markitem.nDesc2}>;";
                    _pointfs[i] = new PointF(tmppointf.X + _rect_crops[i].X, tmppointf.Y + _rect_crops[i].Y);
                }

                //if (bOK)
                //{
                //    PointF ptf = markitem.GetImageMarkCenter(_bmpmaps[i], false, out RectangleF max, out Bitmap bmptempx, out int maxareatemp);

                //    if (INI.Instance.IsAutoTestSavePicture)
                //    {
                //        string _markpicpath = _path + "\\single_pic";
                //        if (!System.IO.Directory.Exists(_markpicpath))
                //            System.IO.Directory.CreateDirectory(_markpicpath);

                //        bmptempx.Save($"{_markpicpath}\\pic{i}.png", ImageFormat.Png);
                //    }

                //    bOK = markitem.CheckBlobSize(max, maxareatemp) == 0;
                //    m_IsPass &= bOK;
                //    m_ResultDesc += $"<{markitem.nDesc2}>;";
                //    _rect_result[i] = new RectangleF(max.X + _rect_crops[i].X, max.Y + _rect_crops[i].Y, max.Width, max.Height);
                //    _pointfs[i] = new PointF(ptf.X + _rect_crops[i].X, ptf.Y + _rect_crops[i].Y);

                //}

                i++;
            }


            PointF ptf1 = _pointfs[0];
            PointF ptf2 = _pointfs[1];
            PointF ptf3 = _pointfs[2];
            PointF ptf4 = _pointfs[3];


            RectangleF rectangleFmark1 = SimpleRectF(ptf1, 2, 2);
            RectangleF rectangleFmark2 = SimpleRectF(ptf2, 2, 2);
            RectangleF rectangleFmark3 = SimpleRectF(ptf3, 2, 2);
            RectangleF rectangleFmark4 = SimpleRectF(ptf4, 2, 2);

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"测试计算{myElapsedTime} ms");

            #region  保存资料

            PointF ptfw1 = ToWorld(ptf1);
            PointF ptfw2 = ToWorld(ptf2);
            PointF ptfw3 = ToWorld(ptf3);
            PointF ptfw4 = ToWorld(ptf4);

            List<PointF> _laser_pointfs = new List<PointF>();

            PointF ptflaser1 = ToLaserCmd(ptfw1);
            PointF ptflaser2 = ToLaserCmd(ptfw2);
            PointF ptflaser3 = ToLaserCmd(ptfw3);
            PointF ptflaser4 = ToLaserCmd(ptfw4);

            _laser_pointfs.Add(ptflaser1);
            _laser_pointfs.Add(ptflaser2);
            _laser_pointfs.Add(ptflaser3);
            _laser_pointfs.Add(ptflaser4);

            //保存资料
            string dataStr = "Index,Name,ViewX,ViewY,WorldX,WorldY," + Environment.NewLine;
            dataStr += $"1,point1,{PointF000ToString(ptf1)},{PointF000ToString(ptflaser1)}" + Environment.NewLine;
            dataStr += $"2,point2,{PointF000ToString(ptf2)},{PointF000ToString(ptflaser2)}" + Environment.NewLine;
            dataStr += $"3,point3,{PointF000ToString(ptf3)},{PointF000ToString(ptflaser3)}" + Environment.NewLine;
            dataStr += $"4,point4,{PointF000ToString(ptf4)},{PointF000ToString(ptflaser4)}" + Environment.NewLine;

            string _dataStr_path = $"{_path}\\{JzTimes.DateTimeSerialStringFFF}.csv";
            SaveData(dataStr, _dataStr_path);

            #region 保存xml 供镭雕机调用
            if (m_IsPass)
            {
                if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
                {
                    if (!Directory.Exists(INI.Instance.LaserSharePath))
                        Directory.CreateDirectory(INI.Instance.LaserSharePath);
                    if (Directory.Exists(INI.Instance.LaserSharePath))
                    {
                        XmlDocument xmldoc;
                        XmlNode xmlnode;
                        XmlElement xmlelem;

                        xmldoc = new XmlDocument();
                        XmlDeclaration xmldecl;
                        xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                        xmldoc.AppendChild(xmldecl);

                        //加入一个根元素
                        xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                        xmldoc.AppendChild(xmlelem);

                        //   < !--AlignType 定位类型:
                        // 1 - 单颗位置定位，给出所有单元的定位位置，相关标签: < Matrix > < Cells >
                        // 2 - 整版使用Mark定位，给出多个Mark(比如圆孔)的定位坐标 ， 相关标签<Marks> < Mark >
                        //-->


                        XmlNode xeAlignType = xmldoc.CreateElement("AlignType");
                        xeAlignType.InnerText = "2";
                        xmlelem.AppendChild(xeAlignType);

                        XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                        xmlelem.AppendChild(xeMatrix);

                        XmlElement x2 = xmldoc.CreateElement("Center");
                        x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                        x2.SetAttribute("y", (0).ToString());
                        xeMatrix.AppendChild(x2);
                        x2 = xmldoc.CreateElement("Angle");
                        x2.InnerText = "0";
                        xeMatrix.AppendChild(x2);
                        x2 = xmldoc.CreateElement("Row");
                        x2.InnerText = Row.ToString();
                        xeMatrix.AppendChild(x2);
                        x2 = xmldoc.CreateElement("Col");
                        x2.InnerText = Column.ToString();
                        xeMatrix.AppendChild(x2);

                        XmlNode xeCells = xmldoc.CreateElement("Cells");
                        xmlelem.AppendChild(xeCells);

                        i = 0;
                        foreach (RegionCellClass itemClass in myListCell)
                        {
                            double fx = 0;// itemClass.xPCenterOffsetWorld.X + myRecipe.LaserLocX + INI.Instance.Cal_Bcx;// + (int)numericUpDown1.Value;
                            double fy = 0;// itemClass.xPCenterOffsetWorld.Y + myRecipe.LaserLocY + INI.Instance.Cal_Bcy;// + (int)numericUpDown2.Value;
                            double angleCal = 0d;

                            x2 = xmldoc.CreateElement("cell" + i.ToString());
                            xeCells.AppendChild(x2);

                            XmlElement x3 = xmldoc.CreateElement("Center");
                            x3.SetAttribute("x", fx.ToString(m_format));
                            x3.SetAttribute("y", fy.ToString(m_format));
                            x2.AppendChild(x3);
                            x3 = xmldoc.CreateElement("dbAngle");
                            x3.InnerText = (angleCal).ToString(m_format);
                            x2.AppendChild(x3);
                            x3 = xmldoc.CreateElement("nType");
                            x3.InnerText = itemClass.nType.ToString();
                            x2.AppendChild(x3);

                            i++;
                        }

                        XmlElement xeMarks = xmldoc.CreateElement("Marks");
                        xeMarks.SetAttribute("mark_num", _laser_pointfs.Count.ToString());
                        xmlelem.AppendChild(xeMarks);

                        i = 0;
                        foreach (PointF ptfx in _laser_pointfs)
                        {
                            double fx = ptfx.X + INI.Instance.Cal_Bcx;
                            double fy = ptfx.Y + INI.Instance.Cal_Bcy;

                            x2 = xmldoc.CreateElement("Mark" + i.ToString());
                            x2.SetAttribute("CenterX", fx.ToString(m_format));
                            x2.SetAttribute("CenterY", fy.ToString(m_format));
                            xeMarks.AppendChild(x2);

                            i++;
                        }

                        xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                    }
                }
            }

            #endregion


            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"生成资料档案{myElapsedTime} ms");

            #region 画图


            //画图
            if (m_IsDrawRect)
            {
                //JzToolsClass jzTools = new JzToolsClass();
                freeImageBitmapOutputDraw.Dispose();
                freeImageBitmapOutputDraw = new Bitmap(freeImageBitmapInput.ToBitmap(),
                                                                   new Size(freeImageBitmapInput.ToBitmap().Width / _showsize,
                                                                                   freeImageBitmapInput.ToBitmap().Height / _showsize));

                Graphics graphics = Graphics.FromImage(freeImageBitmapOutputDraw);

                SolidBrush B = new SolidBrush(Color.Lime);
                SolidBrush Bfail = new SolidBrush(Color.Red);
                SolidBrush B_fail = new SolidBrush(Color.Red);
                //Font MyFont = new Font("Arial", 188);
                //Font MyFont1 = new Font("Arial", 88);
                Font MyFont1 = new Font("Arial", 100);
                //Font MyFont2 = new Font("Arial", 33);
                float _lineWidth = 11f;
                Pen _findrectpen = new Pen(Color.Orange, _lineWidth);
                _findrectpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                //外框


                //RectangleF[] rectangleFs = new RectangleF[] { rectf_des1, rectf_des2, rectf_des3, rectf_des4 };
                graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), new RectangleF[] { rect_cropF, rectangleFmark1, rectangleFmark2, rectangleFmark3, rectangleFmark4 });
                graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), _rect_result);

                graphics.DrawRectangles(_findrectpen, _rect_crops);
                graphics.DrawString($"{alignStr}", MyFont1, B, new PointF(rect_cropF.X + rect_cropF.Width, rect_cropF.Y));

                i = 0;
                foreach (var markitem in myRecipe.MarkCollectionStr)
                {
                    if (markitem.nDesc.Contains("F0") && markitem.nDesc2.Contains("OK"))
                        graphics.DrawString($"Mark{i + 1}<{markitem.nDesc}><{markitem.nDesc2}>", MyFont1, B, new PointF(_rect_result[i].X + _rect_result[i].Width, _rect_result[i].Y));
                    else
                        graphics.DrawString($"Mark{i + 1}<{markitem.nDesc}><{markitem.nDesc2}>", MyFont1, Bfail, new PointF(_rect_result[i].X + _rect_result[i].Width, _rect_result[i].Y));
                    i++;
                }

                graphics.DrawString(stopwatch.ElapsedMilliseconds.ToString("0.000") + " ms", MyFont1, B, new PointF(5, 5));
                graphics.Dispose();

                string _outputimage_path = myPath.Replace(".cfg", "-" + Index.ToString("000") + ".bmp");
                _outputimage_path = INI.Instance.ResultImagePath + "\\" + _index.ToString() + $"-Step_{Index.ToString()}_" + _saveFileName + ".jpg";

                FreeImageBitmap freeImageBitmapInputResult = new FreeImageBitmap(freeImageBitmapOutputDraw);

                Task task = new Task(() =>
                {
                    try
                    {
                        if (INI.Instance.IsSaveStripImage)
                        {
                            SaveImageWithQuality(freeImageBitmapInputResult.ToBitmap(), _outputimage_path, INI.Instance.ImageQuality);
                            //freeImageBitmapInputResult.Save(_outputimage_path, FREE_IMAGE_FORMAT.FIF_JPEG);
                            //freeImageBitmapOutputDraw.Dispose();
                            freeImageBitmapInputResult.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        logInfo.Log($"保存图片错误{ex.Message} ");
                    }
                });
                task.Start();

            }

            #endregion

            myElapsedTime = jzTimesDurRecord.msDuriation;
            jzTimesDurRecord.Cut();
            logInfo.Log($"画出结果图{myElapsedTime} ms");

            freeImageBitmapInput.Dispose();

            GC.Collect();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
        }
        public void SaveImageWithQuality(Bitmap bmpinput, string outputImagePath, long quality)
        {
            using (Image image = bmpinput)
            {
                // 设置压缩参数
                EncoderParameters encoderParameters = new EncoderParameters(1);
                EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                encoderParameters.Param[0] = encoderParameter;

                // 获取图像编码信息
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                // 保存图片，应用压缩参数
                image.Save(outputImagePath, jpgEncoder, encoderParameters);
            }
        }

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
        /// <summary>
        /// MVS 图像转换成 Bitmap
        /// </summary>
        /// <param name="imageInout">MVS 图像</param>
        /// <returns></returns>
        Bitmap MvsImageToBitmap(VM.PlatformSDKCS.ImageBaseData imageInout)
        {
            PixelFormat pixelFormat;
            if (imageInout.Pixelformat == 17301505)
                pixelFormat = PixelFormat.Format8bppIndexed;
            else if (imageInout.Pixelformat == 35127316)
                pixelFormat = PixelFormat.Format24bppRgb;
            else
                pixelFormat = PixelFormat.Format32bppArgb;

            if (imageInout.ImageData == null)
                return new Bitmap(1, 1);


            Bitmap bmpTemp = ConvertFromRGBA(imageInout.ImageData, imageInout.Width, imageInout.Height, pixelFormat);
            return bmpTemp;
        }
        /// <summary>
        /// Byte[] 转换成Bitmap
        /// </summary>
        /// <param name="rgbaData">图像</param>
        /// <param name="width">图像宽</param>
        /// <param name="height">图像高</param>
        /// <param name="pixelFormat">图像格式</param>
        /// <returns></returns>
        Bitmap ConvertFromRGBA(byte[] rgbaData, int width, int height, PixelFormat pixelFormat)
        {

            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                pixelFormat);
            try
            {
                IntPtr intPtr = bitmapData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(rgbaData, 0, intPtr, rgbaData.Length);

            }
            finally
            {
                // 解锁位图像素区域
                bitmap.UnlockBits(bitmapData);

                if (pixelFormat == PixelFormat.Format8bppIndexed)
                {
                    //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度
                    ColorPalette tempPalette;
                    using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                    {
                        tempPalette = tempBmp.Palette;
                    }
                    for (int i = 0; i < tempPalette.Entries.Length; i++)
                    {
                        tempPalette.Entries[i] = Color.FromArgb(i, i, i);
                    }

                    bitmap.Palette = tempPalette;
                }
            }
            return bitmap;
        }

        public int Width = 10;
        public int Height = 10;
        public Bitmap GetScaleRotateEX(float x, float y, float fAngle,int ewidth,int eheight, AUColorImg24 imginput24)
        {
            xFindResult result = new xFindResult();
            result.fAngle = fAngle;
            result.fCenterX = x;
            result.fCenterY = y;

            AUColorImg24 imgoutput24 = new AUColorImg24();
            imgoutput24 = new AUColorImg24(ewidth, eheight);
            ScaleRotateEX2(result, imginput24, ref imgoutput24);

            Bitmap bmpoutput = new Bitmap(imgoutput24.GetWidth(), imgoutput24.GetHeight());
            AUUtility.DrawAUColorImg24ToBitmap(imgoutput24, ref bmpoutput);

            return bmpoutput;
        }
        /// <summary>
        /// 算出偏移及旋轉值
        /// </summary>
        /// <param name="result"></param>
        /// <param name="imginput24"></param>
        /// <param name="imgoutput24"></param>
        public void ScaleRotateEX2(xFindResult result, AUColorImg24 imginput24, ref AUColorImg24 imgoutput24)
        {
            //imgoutput24.Dispose();
            //imgoutput24 = new AUColorImg24(imginput24.GetWidth(), imginput24.GetHeight());

            double fTargetCX = imginput24.GetWidth() / 2.0d;
            double fTargetCY = imginput24.GetHeight() / 2.0d;
            //double fAffineCX = imginput24.GetWidth() / 2.0d;
            //double fAffineCY = imginput24.GetHeight() / 2.0d;

            double fCosSida = Math.Cos(-result.fAngle * Math.PI / 180.0f);
            double fSinSida = Math.Sin(-result.fAngle * Math.PI / 180.0f);
            double fX = (result.fCenterX - fTargetCX) * fCosSida - (result.fCenterY - fTargetCY) * fSinSida;
            double fY = (result.fCenterX - fTargetCX) * fSinSida + (result.fCenterY - fTargetCY) * fCosSida;

            //double fX1 = fAffineCX - fX;
            //double fY1 = fAffineCY - fY;

            ////eInterpolationBits_1,4,8 8 for best but slowest
            //AUImage.ScaleRotate(imginput24, imgoutput24,
            //        (float)fTargetCX,
            //        (float)fTargetCY,
            //        //Result.fCenterX, Result.fCenterY,
            //        (float)fX1, (float)fY1,
            //        result.fAngle,
            //        1.0f,
            //        1.0f,
            //        eInterpolationBits.eInterpolationBits_8);


            float fSrcCX = result.fCenterX; //旋轉中心 X
            float fSrcCY = result.fCenterY; //旋轉中心 Y
            float fDstCX = imgoutput24.GetWidth() / 2; //目標影像中心 X
            float fDstCY = imgoutput24.GetHeight() / 2; //目標影像中心 Y

            //eInterpolationBits_1,4,8 8 for best but slowest
            AUImage.ScaleRotate(imginput24, imgoutput24,
                    //Result.fCenterX, Result.fCenterY,
                    fSrcCX, fSrcCY,
                    fDstCX, fDstCY,
                    result.fAngle,
                    1.0f,
                    1.0f,
                    eInterpolationBits.eInterpolationBits_8);

            //取得旋轉和偏移的值

            //Rotation = result.fAngle;
            //Offset = (float)Math.Sqrt(Math.Pow(fX, 2) + Math.Pow(fY, 2));
            //Offset *= MTResolution;

            //AlignDegree = result.fAngle;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public PointF GetCenterPointF(PointF P1, PointF P2)
        {
            return new PointF((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
        }
        PointF PointFDuiCheng(PointF center, PointF p1)
        {
            float x = center.X * 2 - p1.X;
            float y = center.Y * 2 - p1.Y;
            return new PointF(x, y);
        }
        double GetAngle(PointF xP2World, PointF xP1World)
        {
            double angleOfLine = 0;
            if (xP2World.X > xP1World.X)
                angleOfLine = Math.Atan2((xP2World.Y - xP1World.Y), (xP2World.X - xP1World.X)) * 180 / Math.PI;
            else
                angleOfLine = Math.Atan2((xP1World.Y - xP2World.Y), (xP1World.X - xP2World.X)) * 180 / Math.PI;
            return angleOfLine;
        }
        /// <summary>
        /// 通过向量计算两直线的角度
        /// </summary>
        /// <param name="p1">直线1的起点</param>
        /// <param name="p2">直线1的终点</param>
        /// <param name="p3">直线2的起点</param>
        /// <param name="p4">直线2的终点</param>
        /// <returns></returns>
        double GetAngleVector(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            JetEazy.QMath.QVector q0 = BuildVector(p1, p2);
            JetEazy.QMath.QVector q1 = BuildVector(p3, p4);

            // 計算向量夾角 A
            double dotProduct = q0 * q1;
            // 計算夾角的cos值
            double cosValue = dotProduct / (q0.NormLength * q1.NormLength);
            // 確保 cosValue 在 [-1, 1] 範圍內，避免浮點數精度問題
            cosValue = Math.Max(-1, Math.Min(1, cosValue));
            // 計算角度（以弧度表示）
            double _A = Math.Acos(cosValue);

            // 叉积判断方向，如果 b 在 a 的左边，取反
            if (q0.x * q1.y - q0.y * q1.x > 0)
            {
                _A = -_A;
            }

            double _angle = _A * 180 / Math.PI;
            return _angle;
        }
        /// <summary>
        /// 通过向量计算与水平面的角度
        /// </summary>
        /// <param name="p1">点1 当作原点</param>
        /// <param name="p2">点2 </param>
        /// <returns>返回角度 正数为逆时针 负数为顺时针</returns>
        double GetAngleVector(PointF p1, PointF p2)
        {
            JetEazy.QMath.QVector q0 = BuildVector(p1, p2);
            JetEazy.QMath.QVector q1 = BuildVector(p1, new PointF(p1.X + 1, p1.Y));

            // 計算向量夾角 A
            double dotProduct = q0 * q1;
            // 計算夾角的cos值
            double cosValue = dotProduct / (q0.NormLength * q1.NormLength);
            // 確保 cosValue 在 [-1, 1] 範圍內，避免浮點數精度問題
            cosValue = Math.Max(-1, Math.Min(1, cosValue));
            // 計算角度（以弧度表示）
            double _A = Math.Acos(cosValue);

            // 叉积判断方向，如果 b 在 a 的左边，取反
            if (q0.x * q1.y - q0.y * q1.x > 0)
            {
                _A = -_A;
            }

            double _angle = _A * 180 / Math.PI;
            return _angle;
        }
        /// <summary>
        /// 使用 p1, p2 建立向量
        /// </summary>
        QVector BuildVector(PointF p1, PointF p2)
        {
            return new QVector(p2.X - p1.X, p2.Y - p1.Y);
        }
        public PointF ToWorld(PointF eView)
        {
            PointF ret = eView;
            if (INI.Instance.IsUseStandBoard)
            {
                INI.Instance.MSRCaliCameraToWorld.TransformViewToWorld(eView, out ret);
            }
            else
            {
                if (INI.Instance.IsOnlyUseLeft)
                {
                    INI.Instance.MSRCalibration1.TransformViewToWorld(eView, out ret);
                }
                else
                {
                    if (eView.X <= INI.Instance.BoundaryValue)
                    {
                        INI.Instance.MSRCalibration1.TransformViewToWorld(eView, out ret);
                    }
                    else
                    {
                        INI.Instance.MSRCalibration2.TransformViewToWorld(eView, out ret);
                    }
                }
            }
            ret.Y = -ret.Y;
            return ret;
        }
        public PointF ToView(PointF eWorld, PointF eLoc)
        {
            eWorld.Y = -eWorld.Y;
            PointF ret = eWorld;
            if (INI.Instance.IsUseStandBoard)
            {
                INI.Instance.MSRCaliCameraToWorld.TransformWorldToView(eWorld, out ret);
            }
            else
            {
                if (INI.Instance.IsOnlyUseLeft)
                {
                    INI.Instance.MSRCalibration1.TransformWorldToView(eWorld, out ret);
                }
                else
                {
                    if (eLoc.X <= INI.Instance.BoundaryValue)
                    {
                        INI.Instance.MSRCalibration1.TransformWorldToView(eWorld, out ret);
                    }
                    else
                    {
                        INI.Instance.MSRCalibration2.TransformWorldToView(eWorld, out ret);
                    }
                }
            }
            return ret;
        }
        public PointF ToLaserCmd(PointF eView)
        {
            PointF ret = eView;
            INI.Instance.MSRCaliLaserWorldToLaserCmd.TransformViewToWorld(eView, out ret);
            return ret;
        }
        void SaveData(string DataStr, string FileName)
        {
            File.WriteAllText(FileName, DataStr, Encoding.Default);
        }
        PointF pointToSize(PointF ptinput, int isize)
        {
            PointF ret = new PointF(ptinput.X / isize, ptinput.Y / isize);
            return ret;
        }
        RectangleF rectToSize(Rectangle rect, int isize)
        {
            PointF ret = new PointF(rect.X * 1.0f / isize, rect.Y * 1.0f / isize);
            RectangleF rectangleF = new RectangleF(ret.X, ret.Y, rect.Width * 1.0f / isize, rect.Height * 1.0f / isize);
            return rectangleF;
        }
        Point GetRectCenter(Rectangle Rect)
        {
            return new Point(Rect.X + (Rect.Width >> 1), Rect.Y + (Rect.Height >> 1));
        }
        PointF GetRectCenterF(RectangleF Rect)
        {
            return new PointF(Rect.X + (Rect.Width / 2.0f), Rect.Y + (Rect.Height / 2.0f));
        }
        RectangleF SimpleRectF(PointF Pt, int Width, int Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt)
        {
            return new RectangleF(Pt.X, Pt.Y, 1, 1);
        }
        public double GetPointLength(PointF P1, PointF P2)
        {
            return Math.Sqrt((double)Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2));
        }
        public string PointF000ToString(PointF PTF)
        {
            return PTF.X.ToString("0.000") + "," + PTF.Y.ToString("0.000");
        }
        public PointF StringToPointF(string Str)
        {
            string[] strs = Str.Split(',');
            return new PointF(float.Parse(strs[0]), float.Parse(strs[1]));
        }
        public bool IsInRangeEx(double FromValue, double MaxValue, double MinValue)
        {
            return (FromValue >= MinValue) && (FromValue <= MaxValue);
        }
        #region TrayClassDataSave
        public string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }

        TrayClass TrayUI;

        public void InitTrayUI()
        {
            TrayUI = new TrayClass();
            TrayUI.Initial(myRecipe.TrayColCount,
                                myRecipe.TrayRowCount,
                                220,
                                220,
                                9,
                                "",
                                false);
            SetTray();
        }
        public void SetTray()
        {
            //pnlChipPickUI.BackgroundImage = TrayUI.bmpTray;
            //pnlChipPickUI.BackgroundImage = TrayUI.bmpTray90;
        }
        public void ClearTray()
        {
            //pnlChipPickUI.BackgroundImage = null;
        }
        public void SetTrayUI(int col, int row, string su, int suIndexStr)
        {
            TrayUI.SetTrayUI(col, row, su, suIndexStr);
            SetTray();
        }
        public void ClearTrayUI()
        {
            TrayUI.ClearTrayUI();
            SetTray();
        }
        public void SetTrayBinUI(string binStr)
        {
            binStr = binStr.Replace("@", ",").Replace("BIN", "");
            TrayUI.SetBinString(binStr);
            TrayUI.DrawMap();
            SetTray();
        }
        public void SaveChannelData()
        {
            if (string.IsNullOrEmpty(m_ChipClassDataFilePath))
                return;

            if (!System.IO.Directory.Exists(m_ChipClassDataFilePath))
                System.IO.Directory.CreateDirectory(m_ChipClassDataFilePath);

            string filename = JetEazy.BasicSpace.JzTimes.DateTimeSerialStringFFF;
            if (!string.IsNullOrEmpty(m_ChipClassBarcode))
            {
                filename = m_ChipClassBarcode;
            }

            string dataStr = string.Empty;
            dataStr += TrayUI.myrow.ToString() + Environment.NewLine;
            dataStr += TrayUI.mycol.ToString() + Environment.NewLine;
            dataStr += TrayUI.GetBinString() + Environment.NewLine;

            SaveData(dataStr, m_ChipClassDataFilePath + "\\" +
                filename + "-C" + ((int)TrackArea.TrackINSPECT).ToString() + "T" + filename + ".txt");
        }
        public void TrayDispose()
        {
            TrayUI.Suicide();
        }
        //void SaveData(string DataStr, string FileName)
        //{
        //    System.IO.File.WriteAllText(FileName, DataStr, Encoding.Default);
        //}
        #endregion

        //JzFindObjectClass m_Findex = new JzFindObjectClass();
        PointF calMarkBlob(Bitmap bmpinput, RectangleF cropRect, int threshold, out RectangleF maxrect, bool iswhite = true)
        {
            PointF ret = new PointF(cropRect.X + cropRect.Width / 2, cropRect.Y + cropRect.Height / 2);
            maxrect = new RectangleF(cropRect.X + 1, cropRect.Y + 1, cropRect.Width - 2, cropRect.Height - 2);
            Bitmap bmptemp = (Bitmap)bmpinput.Clone(cropRect, PixelFormat.Format24bppRgb);
            m_Find.AH_SetThreshold(ref bmptemp, threshold);
            m_Find.AH_FindBlob(bmptemp, iswhite);

            if (m_Find.FoundList.Count > 0)
            {
                int maxindex = m_Find.GetMaxRectIndex();
                ret = new PointF((float)m_Find.FoundList[maxindex].rotatedRectangleF.fCX + cropRect.X,
                                 (float)m_Find.FoundList[maxindex].rotatedRectangleF.fCY + cropRect.Y);

                maxrect = new RectangleF(m_Find.rectMaxRect.X + cropRect.X, m_Find.rectMaxRect.Y + cropRect.Y, m_Find.rectMaxRect.Width, m_Find.rectMaxRect.Height);
            }
            bmptemp.Dispose();

            return ret;
        }

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
    }
}