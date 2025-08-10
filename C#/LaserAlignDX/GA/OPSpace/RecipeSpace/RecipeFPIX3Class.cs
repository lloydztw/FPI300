using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy;
using JetEazy.BasicSpace;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.RunSpace;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Traveller106;
using TravellerMINIX6.OPSpace;
using VisionDesigner;
using VisionDesigner.BlobFind;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class RecipeFPIX3Class : RecipeBaseClass
    {
        protected RecipeFPIX3Class()
        {

        }
        private static RecipeFPIX3Class _instance = null;
        public static RecipeFPIX3Class Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeFPIX3Class();
                return _instance;
            }
        }
        public List<RegionCellX3Class> xRegionCells = new List<RegionCellX3Class>();
        public List<Rectangle> xOutBlocs = new List<Rectangle>();

        public Bitmap bmpOrg = new Bitmap(1, 1);
        public Bitmap bmpOrgNoTray = new Bitmap(1, 1);
        public Bitmap bmpOrgFly = new Bitmap(1, 1);
        //public Bitmap bmpbase0 = new Bitmap(1, 1);
        //public Bitmap bmpbase1 = new Bitmap(1, 1);
        //public Bitmap bmptemplate0 = new Bitmap(1, 1);
        //public Bitmap bmptemplate1 = new Bitmap(1, 1);
        //public System.Drawing.PointF template0Center = new System.Drawing.PointF(0, 0);
        //public System.Drawing.PointF template1Center = new System.Drawing.PointF(0, 0);
        //public System.Drawing.PointF distancebase0tobase1 = new System.Drawing.PointF(0, 0);

        public RectangleF xRectRegionPrint = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpprinttemplate = new Bitmap(1, 1);
        public Bitmap bmpprintmask = new Bitmap(1, 1);
        public Bitmap bmpDefectTemplate
        {
            get
            {
                return (Bitmap)bmpprinttemplate.Clone(xRegionTrain, bmpprinttemplate.PixelFormat);
            }
        }
        /// <summary>
        /// 训练的区域
        /// </summary>
        public RectangleF xRegionTrain = new RectangleF(0, 0, 100, 100);

        public RectangleF xLineLeft = new RectangleF(0, 0, 100, 100);
        public RectangleF xLineTop = new RectangleF(0, 0, 100, 100);
        public RectangleF xLineRight = new RectangleF(0, 0, 100, 100);
        public RectangleF xLineBottom = new RectangleF(0, 0, 100, 100);

        public RectangleF xRectRegionPrintNoTray = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpprintNoTraytemplate = new Bitmap(1, 1);

        public RectangleF xRectRegionPrintFly = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpprintFlytemplate = new Bitmap(1, 1);

        //public Bitmap bmpprint = new Bitmap(1, 1);
        //public Bitmap bmpprinttemp = new Bitmap(1, 1);
        //public RectangleF xRectRegionBase0 = new RectangleF(0, 0, 100, 100);
        //public RectangleF xRectRegionBase1 = new RectangleF(0, 0, 100, 100);

        //public MvdFindClass mvdbase0_Find = new MvdFindClass();
        //public MvdFindClass mvdbase1_Find = new MvdFindClass();
        public PointF ptPrinttemp = new PointF(-1, -1);
        public MvdFindClass mvdprinttemp_Find = new MvdFindClass();
        public PointF ptPrintFlytemp = new PointF(-1, -1);
        public MvdFindClass mvdprintFlytemp_Find = new MvdFindClass();

        public Mvd2DReaderClass mvd2DReader = new Mvd2DReaderClass();

        public RectangleF xRectCodeRegion = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpcodetemplate = new Bitmap(1, 1);

        public int xRow = 1;
        public int xColumn = 1;
        public int xLeftTopX = 1;
        public int xLeftTopY = 1;
        public float xRowOffset = 2f;
        public float xColumnOffset = 2f;
        public float xChipWidth = 10;
        public float xChipHeight = 10;
        public int xExtendx = 100;
        public int xExtendy = 100;

        public int xChNum = 1;
        public int xChValue = 255;

        public string xLotNoStr = "NONE";

        public int PassCount = 0;
        public int NGCount = 0;

        #region 实际矩阵XY

        public float xRealLeftX = 0;
        public float xRealLeftY = 0;
        public float xRealOffsetX = 1;
        public float xRealOffsetY = 1;

        #endregion


        public override void Load(bool eCancel = false)
        {
            xLotNoStr = ReadINIValue("Collect", "xLotNoStr", "NONE", INIFILE);

            xRow = int.Parse(ReadINIValue("Recipe Basic", "xRow", "1", INIFILE));
            xColumn = int.Parse(ReadINIValue("Recipe Basic", "xColumn", "1", INIFILE));
            xLeftTopX = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopX", "1", INIFILE));
            xLeftTopY = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopY", "1", INIFILE));
            xRowOffset = float.Parse(ReadINIValue("Recipe Basic", "xRowOffset", "2", INIFILE));
            xColumnOffset = float.Parse(ReadINIValue("Recipe Basic", "xColumnOffset", "2", INIFILE));
            xChipWidth = float.Parse(ReadINIValue("Recipe Basic", "xChipWidth", "10", INIFILE));
            xChipHeight = float.Parse(ReadINIValue("Recipe Basic", "xChipHeight", "10", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Recipe Basic", "xExtendx", "100", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Recipe Basic", "xExtendy", "100", INIFILE));
            xRealLeftX = float.Parse(ReadINIValue("Recipe Basic", "xRealLeftX", "0", INIFILE));
            xRealLeftY = float.Parse(ReadINIValue("Recipe Basic", "xRealLeftY", "0", INIFILE));
            xRealOffsetX = float.Parse(ReadINIValue("Recipe Basic", "xRealOffsetX", "1", INIFILE));
            xRealOffsetY = float.Parse(ReadINIValue("Recipe Basic", "xRealOffsetY", "1", INIFILE));


            PassCount = int.Parse(ReadINIValue("Recipe Basic", "PassCount", "0", INIFILE));
            NGCount = int.Parse(ReadINIValue("Recipe Basic", "NGCount", "0", INIFILE));

            xChNum = int.Parse(ReadINIValue("Recipe Basic", "xChNum", "1", INIFILE));
            xChValue = int.Parse(ReadINIValue("Recipe Basic", "xChValue", "255", INIFILE));

            //xRectRegionBase0 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase0", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRectRegionBase1 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase1", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //template0Center = StringtoPointF(ReadINIValue("Recipe Basic", "template0Center", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            //template1Center = StringtoPointF(ReadINIValue("Recipe Basic", "template1Center", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            //distancebase0tobase1 = StringtoPointF(ReadINIValue("Recipe Basic", "distancebase0tobase1", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xRegionTrain = StringtoRectF(ReadINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            xRectRegionPrintNoTray = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrintNoTray", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xRectRegionPrintFly = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrintFly", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            ptPrinttemp = StringtoPointF(ReadINIValue("Recipe Basic", "ptPrinttemp", PointFtoStringSimple(new PointF(-1, -1)), INIFILE));
            ptPrintFlytemp = StringtoPointF(ReadINIValue("Recipe Basic", "ptPrintFlytemp", PointFtoStringSimple(new PointF(-1, -1)), INIFILE));


            xLineLeft = StringtoRectF(ReadINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineTop = StringtoRectF(ReadINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineRight = StringtoRectF(ReadINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineBottom = StringtoRectF(ReadINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));


            //xRow = int.Parse(ReadINIValue("Recipe Basic", "xRow", xRow.ToString(), INIFILE));
            //xColumn = int.Parse(ReadINIValue("Recipe Basic", "xColumn", xColumn.ToString(), INIFILE));
            //xLeftTopX = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopX", xLeftTopX.ToString(), INIFILE));
            //xLeftTopY = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopY", xLeftTopY.ToString(), INIFILE));
            //xRowOffset = float.Parse(ReadINIValue("Recipe Basic", "xRowOffset", xRowOffset.ToString(), INIFILE));
            //xColumnOffset = float.Parse(ReadINIValue("Recipe Basic", "xColumnOffset", xColumnOffset.ToString(), INIFILE));
            //xChipWidth = float.Parse(ReadINIValue("Recipe Basic", "xChipWidth", xChipWidth.ToString(), INIFILE));
            //xChipHeight = float.Parse(ReadINIValue("Recipe Basic", "xChipHeight", xChipHeight.ToString(), INIFILE));

            //PassCount = int.Parse(ReadINIValue("Recipe Basic", "PassCount", PassCount.ToString(), INIFILE));
            //NGCount = int.Parse(ReadINIValue("Recipe Basic", "NGCount", NGCount.ToString(), INIFILE));

            //xRectRegionBase0 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase0", RectFtoStringSimple(xRectRegionBase0), INIFILE));
            //xRectRegionBase1 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase1", RectFtoStringSimple(xRectRegionBase1), INIFILE));
            //template0Center = StringtoPointF(ReadINIValue("Recipe Basic", "template0Center", PointFtoStringSimple(template0Center), INIFILE));
            //template1Center = StringtoPointF(ReadINIValue("Recipe Basic", "template1Center", PointFtoStringSimple(template1Center), INIFILE));
            //distancebase0tobase1 = StringtoPointF(ReadINIValue("Recipe Basic", "distancebase0tobase1", PointFtoStringSimple(distancebase0tobase1), INIFILE));
            //xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(xRectRegionPrint), INIFILE));

            //xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(xRectCodeRegion), INIFILE));

            if (!eCancel)
            {
                #region 初始化加载图片
                string bmporgpath = $"{PathIndexStr}\\org.bmp";
                if (System.IO.File.Exists(bmporgpath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmporgpath);
                    bmpOrg.Dispose();
                    bmpOrg = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmporgNoTraypath = $"{PathIndexStr}\\orgNoTray.bmp";
                if (System.IO.File.Exists(bmporgNoTraypath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmporgNoTraypath);
                    bmpOrgNoTray.Dispose();
                    bmpOrgNoTray = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmporgFlypath = $"{PathIndexStr}\\orgFly.bmp";
                if (System.IO.File.Exists(bmporgFlypath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmporgFlypath);
                    bmpOrgFly.Dispose();
                    bmpOrgFly = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                //string bmpbase0path = $"{PathIndexStr}\\base0.bmp";
                //if (System.IO.File.Exists(bmpbase0path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpbase0path);
                //    bmpbase0.Dispose();
                //    bmpbase0 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmpbase1path = $"{PathIndexStr}\\base1.bmp";
                //if (System.IO.File.Exists(bmpbase1path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpbase1path);
                //    bmpbase1.Dispose();
                //    bmpbase1 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmptemplate0path = $"{PathIndexStr}\\template0.bmp";
                //if (System.IO.File.Exists(bmptemplate0path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmptemplate0path);
                //    bmptemplate0.Dispose();
                //    bmptemplate0 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmptemplate1path = $"{PathIndexStr}\\template1.bmp";
                //if (System.IO.File.Exists(bmptemplate1path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmptemplate1path);
                //    bmptemplate1.Dispose();
                //    bmptemplate1 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                string bmpprinttemplatepath = $"{PathIndexStr}\\bmpprinttemplate.bmp";
                if (System.IO.File.Exists(bmpprinttemplatepath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprinttemplatepath);
                    bmpprinttemplate.Dispose();
                    bmpprinttemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmpprintNoTraytemplatepath = $"{PathIndexStr}\\bmpprintNoTraytemplate.bmp";
                if (System.IO.File.Exists(bmpprintNoTraytemplatepath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprintNoTraytemplatepath);
                    bmpprintNoTraytemplate.Dispose();
                    bmpprintNoTraytemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmpprintFlytemplatepath = $"{PathIndexStr}\\bmpprintFlytemplate.bmp";
                if (System.IO.File.Exists(bmpprintFlytemplatepath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprintFlytemplatepath);
                    bmpprintFlytemplate.Dispose();
                    bmpprintFlytemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
                if (System.IO.File.Exists(bmpprintmaskpath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprintmaskpath);
                    bmpprintmask.Dispose();
                    bmpprintmask = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmpcodepath = $"{PathIndexStr}\\bmpcode.bmp";
                if (System.IO.File.Exists(bmpcodepath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpcodepath);
                    bmpcodetemplate.Dispose();
                    bmpcodetemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                #endregion

                //建立所有的region
                CreateViews();
                int iOK = ViewTrainLoad();
                if (iOK != 0)
                    JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载参数训练失败！");
            }

            //InspectX2Class.Instance.Initial(Path, Index, "Inspect_default_info.ini");
            //InspectX2Class.Instance.Load();

            InspectX3ParaClass.Instance.Initial(Path, Index, "Inspect_default_info.ini");
            InspectX3ParaClass.Instance.Load();

            FlyParaClass.Instance.Initial(Path, Index, "Fly_default_info.ini");
            FlyParaClass.Instance.Load();

            NoTrayParaClass.Instance.Initial(Path, Index, "NoTray_default_info.ini");
            NoTrayParaClass.Instance.Load();
        }
        public override void Save()
        {
            WriteINIValue("Recipe Basic", "xRow", xRow.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xColumn", xColumn.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xLeftTopX", xLeftTopX.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xLeftTopY", xLeftTopY.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRowOffset", xRowOffset.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xColumnOffset", xColumnOffset.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChipWidth", xChipWidth.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChipHeight", xChipHeight.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealLeftX", xRealLeftX.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealLeftY", xRealLeftY.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealOffsetX", xRealOffsetX.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealOffsetY", xRealOffsetY.ToString(), INIFILE);

            WriteINIValue("Recipe Basic", "ptPrinttemp", PointFtoStringSimple(ptPrinttemp), INIFILE);
            WriteINIValue("Recipe Basic", "ptPrintFlytemp", PointFtoStringSimple(ptPrintFlytemp), INIFILE);

            WriteINIValue("Recipe Basic", "xChNum", xChNum.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChValue", xChValue.ToString(), INIFILE);

            string bmporgpath = $"{PathIndexStr}\\org.bmp";
            bmpOrg.Save(bmporgpath, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmporgNoTraypath = $"{PathIndexStr}\\orgNoTray.bmp";
            bmpOrgNoTray.Save(bmporgNoTraypath, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmporgFlypath = $"{PathIndexStr}\\orgFly.bmp";
            bmpOrgFly.Save(bmporgFlypath, System.Drawing.Imaging.ImageFormat.Bmp);

            //建立所有的region
            CreateViews();
            int iOK = ViewTrainLoad();
            if (iOK != 0)
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载参数训练失败！");

            //InspectX2Class.Instance.Save();

            InspectX3ParaClass.Instance.Save();
            FlyParaClass.Instance.Save();
            NoTrayParaClass.Instance.Save();
        }

        public void SavePrintTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(xRectRegionPrint), INIFILE);
            string bmpprinttemplatepath = $"{PathIndexStr}\\bmpprinttemplate.bmp";
            bmpprinttemplate.Save(bmpprinttemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
            bmpprintmask.Save(bmpprintmaskpath, System.Drawing.Imaging.ImageFormat.Bmp);
            InspectX3ParaClass.Instance.SaveRoi();
            SaveCodeTemplate();
        }
        public void SavePrintTemplateRegionTrain()
        {
            WriteINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(xRegionTrain), INIFILE);
        }
        public void SaveLinesRegion()
        {
            WriteINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(xLineLeft), INIFILE);
            WriteINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(xLineTop), INIFILE);
            WriteINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(xLineRight), INIFILE);
            WriteINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(xLineBottom), INIFILE);
        }
        public void SavePrintNoTrayTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectRegionPrintNoTray", RectFtoStringSimple(xRectRegionPrintNoTray), INIFILE);
            string bmpprintNoTraytemplatepath = $"{PathIndexStr}\\bmpprintNoTraytemplate.bmp";
            bmpprintNoTraytemplate.Save(bmpprintNoTraytemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            //string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
            //bmpprintmask.Save(bmpprintmaskpath, System.Drawing.Imaging.ImageFormat.Bmp);
            //InspectX2Class.Instance.SaveRoi();
            //SaveCodeTemplate();
        }
        public void SavePrintFlyTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectRegionPrintFly", RectFtoStringSimple(xRectRegionPrintFly), INIFILE);
            string bmpprintFlytemplatepath = $"{PathIndexStr}\\bmpprintFlytemplate.bmp";
            bmpprintFlytemplate.Save(bmpprintFlytemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            //string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
            //bmpprintmask.Save(bmpprintmaskpath, System.Drawing.Imaging.ImageFormat.Bmp);
            //InspectX2Class.Instance.SaveRoi();
            //SaveCodeTemplate();
        }
        public void SaveCodeTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(xRectCodeRegion), INIFILE);
            string bmpcodepath = $"{PathIndexStr}\\bmpcode.bmp";
            bmpcodetemplate.Save(bmpcodepath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        public void SaveLotNo()
        {
            WriteINIValue("Collect", "xLotNoStr", xLotNoStr, INIFILE);
        }

        #region 定位TRAIN&RUN

        public Size PrintTemplateSize
        {
            get { return mvdprinttemp_Find.bmpObj_Image.Size; }
        }

        public int PrintTempTrain()
        {
            mvdprinttemp_Find.bmpObj_Image = bmpDefectTemplate;// (Bitmap)bmpprinttemplate.Clone(xRegionTrain, bmpprinttemplate.PixelFormat);

            //CMvdRectangleF cMvd = new CMvdRectangleF(
            //    xRegionTrain.Width / 2,
            //    xRegionTrain.Height / 2,
            //    xRegionTrain.Width,
            //    xRegionTrain.Height);

            bool bOK = mvdprinttemp_Find.HikTrainBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(Bitmap ebmpInput)
        {
            mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
            mvdprinttemp_Find.bmpRun_Image = ebmpInput;
            bool bOK = mvdprinttemp_Find.HikRunBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(CMvdImage eMvdInput)
        {
            mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
            mvdprinttemp_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdprinttemp_Find.HikRun2();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(CMvdImage eMvdInput, RectangleF eRectF)
        {
            mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
            mvdprinttemp_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdprinttemp_Find.HikRun3(eRectF);
            return (bOK ? 0 : -1);
        }


        public int PrintTempFlyTrain()
        {
            mvdprintFlytemp_Find.bmpObj_Image = bmpprintFlytemplate;
            bool bOK = mvdprintFlytemp_Find.HikTrainBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempFlyRun(Bitmap ebmpInput)
        {
            mvdprintFlytemp_Find.xMvdAngle = FlyParaClass.Instance.xAngle;
            mvdprintFlytemp_Find.xMvdTolerance = FlyParaClass.Instance.xTolerance;
            mvdprintFlytemp_Find.bmpRun_Image = ebmpInput;
            bool bOK = mvdprintFlytemp_Find.HikRunBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempFlyRun(CMvdImage eMvdInput)
        {
            mvdprintFlytemp_Find.xMvdAngle = FlyParaClass.Instance.xAngle;
            mvdprintFlytemp_Find.xMvdTolerance = FlyParaClass.Instance.xTolerance;
            mvdprintFlytemp_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdprintFlytemp_Find.HikRun2();
            return (bOK ? 0 : -1);
        }
        public int PrintTempFlyRun(CMvdImage eMvdInput, RectangleF eRectF)
        {
            mvdprintFlytemp_Find.xMvdAngle = FlyParaClass.Instance.xAngle;
            mvdprintFlytemp_Find.xMvdTolerance = FlyParaClass.Instance.xTolerance;
            mvdprintFlytemp_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdprintFlytemp_Find.HikRun3(eRectF);
            return (bOK ? 0 : -1);
        }


        public bool CheckSpecialAngle(Bitmap ebmpInput, out List<CBlobInfo> m_list,out float retAngle,out PointF retCenter)
        {

            bool bOK = false;
            retAngle = 0;
            retCenter = new PointF();

            VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;
            VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = null;

            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            //二值化
            cImageBinaryToolObj.InputImage = BitmapToCMvdImage(ebmpInput);
            cImageBinaryToolObj.ROI = null;
            cImageBinaryToolObj.SetRunParam("LowThreshold", FlyParaClass.Instance.xThresholdValue.ToString());
            //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
            cImageBinaryToolObj.Run();

            //blob
            cBlobFindToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            //cBlobFindToolObj.RegionImage = BitmapToCMvdImage(eBmpMask);
            cBlobFindToolObj.ROI = null;
            if (FlyParaClass.Instance.xBlobMode == Eazy_Project_III.BlobMode.White)
                cBlobFindToolObj.SetRunParam("Polarity", "BrightObject");
            else
                cBlobFindToolObj.SetRunParam("Polarity", "DarkObject");
            cBlobFindToolObj.BasicParam.ShowBlobImageStatus = true;
            cBlobFindToolObj.Run();
            VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = cBlobFindToolObj.Result;

            m_list = new List<CBlobInfo>();
            foreach (var blob in cBlobFindToolObj.Result.BlobInfo)
            {
                if (blob.AreaF >= FlyParaClass.Instance.xBlobAreaMin && blob.AreaF <= FlyParaClass.Instance.xBlobAreaMax)
                {
                    m_list.Add(blob);
                }
            }

            if (m_list.Count == 2)
            {
                CBlobInfo b0 = m_list[0];
                CBlobInfo b1 = m_list[1];

                // CreateInstance

                VisionDesigner.P2PMeasure.CP2PMeasureTool cP2PMeasureToolObj = new VisionDesigner.P2PMeasure.CP2PMeasureTool();

                // Set basic parameter

                cP2PMeasureToolObj.BasicParam.Point1 = new MVD_POINT_F(b0.RectInfo.CenterX, b0.RectInfo.CenterY);

                cP2PMeasureToolObj.BasicParam.Point2 = new MVD_POINT_F(b1.RectInfo.CenterX, b1.RectInfo.CenterY);

                // Running

                cP2PMeasureToolObj.Run();

                // Get the result

                VisionDesigner.P2PMeasure.CP2PMeasureResult cP2PMeasureRes = cP2PMeasureToolObj.Result;

                if (FlyParaClass.Instance.xIsShuiPing)
                {
                    retAngle = cP2PMeasureRes.Angle;
                }
                else
                {
                    retAngle = cP2PMeasureRes.Angle + 90;
                }

                //if (cP2PMeasureRes.Angle < 0)
                //{
                //    if (FlyParaClass.Instance.xIsShuiPing)
                //    {
                //        retAngle = -cP2PMeasureRes.Angle;
                //    }
                //    else
                //    {
                //        retAngle = -cP2PMeasureRes.Angle + 90;
                //    }
                //}
                //else
                //{
                //    if (FlyParaClass.Instance.xIsShuiPing)
                //    {
                //        retAngle = cP2PMeasureRes.Angle;
                //    }
                //    else
                //    {
                //        retAngle = cP2PMeasureRes.Angle - 90;
                //    }
                //}

                retCenter = new PointF(cP2PMeasureRes.MidPoint.fX, cP2PMeasureRes.MidPoint.fY);
                //Console.WriteLine("Angle: {0}", cP2PMeasureRes.Angle);

                //Console.WriteLine("Distance: {0}", cP2PMeasureRes.Dist);

                cP2PMeasureToolObj.Dispose();
                cP2PMeasureToolObj = null;

                bOK = true;
            }

            cImageBinaryToolObj.Dispose();
            cBlobFindToolObj.Dispose();

            cImageBinaryToolObj = null;
            cBlobFindToolObj = null;
            return bOK;
        }

        #endregion

        #region TCP_DATA

        public int SetByPass(bool[] eBypass)
        {
            if (eBypass == null)
                return -1;
            if (eBypass.Length != xRegionCells.Count)
                return -2;
            int i = 0;
            while (i < eBypass.Length)
            {
                xRegionCells[i].ByPass = eBypass[i];
                i++;
            }
            return 0;
        }
        public int SetBarcode(string[] eBarcodeStr)
        {
            if (eBarcodeStr == null)
                return -1;
            if (eBarcodeStr.Length != xRegionCells.Count)
                return -2;
            int i = 0;
            while (i < eBarcodeStr.Length)
            {
                xRegionCells[i].SetBarcodeStr = eBarcodeStr[i];
                i++;
            }
            return 0;
        }
        public bool RunRepeatCode()
        {
            bool isgood = true;
            ////收集所有页面读取到的二维码
            //List<string> _collectCodeList = new List<string>();
            //_collectCodeList.Clear();
            //foreach (RegionCellX2Class cell in xRegionCells)
            //{
            //    if (cell.RunCodeInfo == null)
            //        continue;
            //    string barcodeStr = cell.RunCodeInfo.Content;
            //    if (!string.IsNullOrEmpty(barcodeStr))
            //        _collectCodeList.Add(barcodeStr);
            //}

            //List<string> _collectRepeatCodeList = new List<string>();
            ////查询数据库中所有的条码
            //isgood = JzCheckRepeatClass.Instance.MySqlTableQuery(_collectCodeList, ref _collectRepeatCodeList) <= 0;
            //List<string> _collectNoRepeatCodeList = new List<string>();
            ////匹配到各个分支
            //if (!isgood)
            //{
            //    _collectNoRepeatCodeList.Clear();
            //    foreach (RegionCellX2Class cell in xRegionCells)
            //    {
            //        if (cell.RunCodeInfo == null)
            //            continue;
            //        string barcodeStr = cell.RunCodeInfo.Content;
            //        if (!string.IsNullOrEmpty(barcodeStr))
            //        {
            //            bool bOK = cell.CheckRepeatCode(_collectRepeatCodeList, 0);
            //            isgood &= bOK;
            //            if (bOK)
            //                _collectNoRepeatCodeList.Add(barcodeStr);
            //        }
            //    }
            //    JzCheckRepeatClass.Instance.MySqlTableInsert(_collectNoRepeatCodeList);
            //}
            //else
            //{
            //    JzCheckRepeatClass.Instance.MySqlTableInsert(_collectCodeList);
            //}

            ////比对同一片的重复码
            //foreach (RegionCellX2Class cell in xRegionCells)
            //{
            //    bool bOK = cell.CheckRepeatCode(_collectCodeList);
            //    isgood &= bOK;
            //}
            return isgood;
        }

        #endregion 

        #region 统计数据

        public float[] AnalyzeDatas = new float[9];
        public void AnalyzeDatasData()
        {
            int count = Enum.GetValues(typeof(InspectReason)).Length;
            AnalyzeDatas = new float[count + 2];
            int i = 0;
            while (i < AnalyzeDatas.Length)
            {
                AnalyzeDatas[i] = 0;
                i++;
            }
        }
        /// <summary>
        /// 分析数据 返回bool
        /// </summary>
        /// <returns>true:PASS false:FAIL</returns>
        public bool AnalyzeDatasRun()
        {
            foreach (RegionCellX3Class cell in xRegionCells)
            {
                if (cell.inspectReasons.Count == 0)
                {
                    AnalyzeDatas[(int)InspectReason.PASS]++;
                    continue;
                }
                foreach (InspectReason reason in cell.inspectReasons)
                {
                    AnalyzeDatas[(int)reason]++;
                }
            }
            //芯片数
            AnalyzeDatas[AnalyzeDatas.Length - 2] = xRow * xColumn;
            //良率
            AnalyzeDatas[AnalyzeDatas.Length - 1] = AnalyzeDatas[(int)InspectReason.PASS] * 1.0f / AnalyzeDatas[AnalyzeDatas.Length - 2] * 100;
            float ins_count = AnalyzeDatas[(int)InspectReason.PASS] + AnalyzeDatas[(int)InspectReason.INS_NOOPEN];
            bool bOK = ins_count == xRow * xColumn;

            Add(bOK);
            return bOK;
        }


        //public int PassCount = 0;
        //public int NGCount = 0;
        public void ResetZero()
        {
            PassCount = 0;
            NGCount = 0;
            SaveLotCount();
        }
        void Add(bool eIsPass = true)
        {
            if (eIsPass)
            {
                PassCount++;
            }
            else
            {
                NGCount++;
            }
            SaveLotCount();
        }
        void SaveLotCount()
        {
            WriteINIValue("Recipe Basic", "PassCount", PassCount.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "NGCount", NGCount.ToString(), INIFILE);
        }


        #endregion


        public int ViewTrainLoad()
        {
            int iret = 0;// Base0Train();
            //if (iret == 0)
            //    iret = Base1Train();
            if (iret == 0)
                iret = PrintTempTrain();
            //if (iret == 0)
            //{
            //    //计算初始位置
            //    ProcessRunFPIClass.Instance.cMvdInput = BitmapToCMvdImage(bmpOrg);
            //    ProcessRunFPIClass.Instance.RunRecipe();
            //    bool bFoundALIGNERR = false;
            //    foreach (RegionCellX3Class cell in xRegionCells)
            //    {
            //        if (cell.inspectReason == InspectReason.INS_ALIGNERR)
            //        {
            //            bFoundALIGNERR = true;
            //            break;
            //        }
            //    }
            //    if (bFoundALIGNERR)
            //    {
            //        iret = -1;
            //    }
            //}
            if (iret == 0)
                iret = PrintTempFlyTrain();
            return iret;
        }
        public void CreateViews()
        {
            xRegionCells.Clear();

            RectangleF _baserect = new RectangleF(xLeftTopX,
                xLeftTopY,
                xChipWidth / INI.Instance.ImageResolution,
                xChipHeight / INI.Instance.ImageResolution);
            float _rowoffset = xRowOffset / INI.Instance.ImageResolution;
            float _coloffset = xColumnOffset / INI.Instance.ImageResolution;

            int _index = 0;
            for (int i = 0; i < xRow; i++)
            {
                if (i % 2 == 1)
                {
                    for (int j = xColumn - 1; j > -1; j--)
                    {
                        RegionCellX3Class _cell = new RegionCellX3Class();
                        _cell.Index = _index;
                        _cell.CellRow = i;
                        _cell.CellCol = j;
                        _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                        _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);

                        _cell.OrgX = xRealLeftX + j * xRealOffsetX;
                        _cell.OrgY = xRealLeftY + i * xRealOffsetY;

                        xRegionCells.Add(_cell);
                        _index++;
                    }
                }
                else
                {
                    for (int j = 0; j < xColumn; j++)
                    {
                        RegionCellX3Class _cell = new RegionCellX3Class();
                        _cell.Index = _index;
                        _cell.CellRow = i;
                        _cell.CellCol = j;
                        _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                        _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);

                        _cell.OrgX = xRealLeftX + j * xRealOffsetX;
                        _cell.OrgY = xRealLeftY + i * xRealOffsetY;

                        xRegionCells.Add(_cell);
                        _index++;
                    }
                }
            }

            //AnalyzeDatasData();

            //原始的顺序排列
            //int _index = 0;
            //for (int i = 0; i < xRow; i++)
            //{
            //    for (int j = 0; j < xColumn; j++)
            //    {
            //        RegionCellX3Class _cell = new RegionCellX3Class();
            //        _cell.Index = _index;
            //        _cell.CellRow = i;
            //        _cell.CellCol = j;
            //        _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
            //        _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);
            //        xRegionCells.Add(_cell);
            //        _index++;
            //    }
            //}

        }
        protected CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
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
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
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
                        // 获取32bpp像素值
                        byte b = _BitImageBufferBytes[bitmapIndex];
                        byte g = _BitImageBufferBytes[bitmapIndex + 1];
                        byte r = _BitImageBufferBytes[bitmapIndex + 2];
                        byte a = _BitImageBufferBytes[bitmapIndex + 3];
                        bitmapIndex += 4;
                        // 转换为灰度值（8bpp）
                        byte gray = (byte)((r * 0.299 + g * 0.587 + b * 0.114) * (a / 255.0));

                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = gray;// _BitImageBufferBytes[bitmapIndex++];
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
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }

    }

    public class FlyParaClass : RecipeBaseClass
    {
        public FlyParaClass()
        {

        }
        private static FlyParaClass _instance = null;
        public static FlyParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlyParaClass();
                return _instance;
            }
        }

        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A01.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A02.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A03.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendx { get; set; } = 20;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A04.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendy { get; set; } = 20;

        const string _Cat2 = "A02.双头吸嘴找角度设置";

        [CategoryAttribute(_Cat2), DescriptionAttribute("两个吸嘴的计算开启")]
        [DisplayName("A00.开启双头计算")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool xIsOpenMuit { get; set; } = false;

        [CategoryAttribute(_Cat2), DescriptionAttribute("二值化的阈值")]
        [DisplayName("A01.灰阶阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;
        [CategoryAttribute(_Cat2), DescriptionAttribute("寻找的特征选择黑色还是白色 吸嘴是黑色选择黑色")]
        [DisplayName("A02.检测模式")]
        [TypeConverter(typeof(JzEnumConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public BlobMode xBlobMode { get; set; } = BlobMode.Black;
        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最小的面积")]
        [DisplayName("A03.Blob面积最小值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMin { get; set; } = 5000;
        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最大的面积")]
        [DisplayName("A03.Blob面积最大值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMax { get; set; } = 99999999;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true水平 false垂直")]
        [DisplayName("A04.吸嘴是否水平校正")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool xIsShuiPing { get; set; } = true;



        public override void Load(bool eCancel = false)
        {
            xTolerance = float.Parse(ReadINIValue("Fly", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Fly", "xAngle", "30", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Fly", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Fly", "xExtendy", "20", INIFILE));

            xIsOpenMuit = ReadINIValue("Basic", "xIsOpenMuit", "0", INIFILE) == "1";
            xThresholdValue = int.Parse(ReadINIValue("Basic", "xThresholdValue", "128", INIFILE));
            xBlobMode = (BlobMode)int.Parse(ReadINIValue("Basic", "xBlobMode", "1", INIFILE));
            xBlobAreaMin = int.Parse(ReadINIValue("Basic", "xBlobAreaMin", "10", INIFILE));
            xBlobAreaMax = int.Parse(ReadINIValue("Basic", "xBlobAreaMax", "20000", INIFILE));
            xIsShuiPing = ReadINIValue("Basic", "xIsShuiPing", "1", INIFILE) == "1";
        }
        public override void Save()
        {
            WriteINIValue("Fly", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Fly", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Fly", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Fly", "xExtendy", xExtendy.ToString(), INIFILE);

            WriteINIValue("Basic", "xIsOpenMuit", (xIsOpenMuit ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobMode", ((int)xBlobMode).ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMin", xBlobAreaMin.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMax", xBlobAreaMax.ToString(), INIFILE);
            WriteINIValue("Basic", "xIsShuiPing", (xIsShuiPing ? "1" : "0"), INIFILE);
        }

    }

    public class InspectX3ParaClass : RecipeBaseClass
    {
        public InspectX3ParaClass()
        {

        }
        private static InspectX3ParaClass _instance = null;
        public static InspectX3ParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InspectX3ParaClass();
                return _instance;
            }
        }

        const string _Cat0 = "A00.启用设置";
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A01.开启尺寸测量")]
        [Browsable(true)]
        public bool bOpenLineMeasure { get; set; } = false;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A02.开启缺陷检测")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckInspect { get; set; } = false;
        

        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A01.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A02.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A03.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendx { get; set; } = 20;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A04.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendy { get; set; } = 20;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围内重叠率")]
        [DisplayName("A05.匹配重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        [Browsable(true)]
        public int xMaxOverlap { get; set; } = 80;

        [CategoryAttribute(_Cat1), DescriptionAttribute("在搜索范围内重合的比例")]
        [DisplayName("A06.格点重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1)]
        [Browsable(true)]
        public float xChipOverlap { get; set; } = 0.5f;


        #region 找直线的参数

        const string _Cat2 = "A02.尺寸检测参数设置";
        
        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A01.左边查找方向")]
        [Browsable(true)]
        public bool bPositive0 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A02.左边极性")]
        [Browsable(true)]
        public bool bEdgePolarity0 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A03.上边查找方向")]
        [Browsable(true)]
        public bool bPositive1 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A04.上边极性")]
        [Browsable(true)]
        public bool bEdgePolarity1 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A05.右边查找方向")]
        [Browsable(true)]
        public bool bPositive2 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A06.右边极性")]
        [Browsable(true)]
        public bool bEdgePolarity2 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A07.下边查找方向")]
        [Browsable(true)]
        public bool bPositive3 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A08.下边极性")]
        [Browsable(true)]
        public bool bEdgePolarity3 { get; set; } = true;


        #endregion

        #region 缺陷检测设置

        const string _Cat3 = "A03.缺陷检测参数设置";

        [CategoryAttribute(_Cat3), DescriptionAttribute("")]
        [DisplayName("A00.二值化阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A01.缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A02.缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A03.缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharArea { get; set; } = 30.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A04.背景缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A05.背景缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A06.背景缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudArea { get; set; } = 30.1f;

        [Browsable(false)]
        public int RoiCount { get; set; } = 0;
        [Browsable(false)]
        public List<RectangleF> rectangles { get; set; } = new List<RectangleF>();

        #endregion



        public override void Load(bool eCancel = false)
        {
            xTolerance = float.Parse(ReadINIValue("Basic", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Basic", "xAngle", "30", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", INIFILE));
            xMaxOverlap = int.Parse(ReadINIValue("Basic", "xMaxOverlap", "80", INIFILE));
            xChipOverlap = float.Parse(ReadINIValue("Basic", "xChipOverlap", "0.5", INIFILE));

            bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", INIFILE));
            xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", INIFILE));
            xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", INIFILE));
            xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", INIFILE));
            xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", INIFILE));
            xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", INIFILE));
            xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", INIFILE));

            RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            int i = 0;
            rectangles.Clear();
            while (i < RoiCount)
            {
                RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
                rectangles.Add(rectf);

                i++;
            }

            bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", INIFILE) == "1";
            bPositive0 = ReadINIValue("Basic", "bPositive0", "1", INIFILE) == "1";
            bPositive1 = ReadINIValue("Basic", "bPositive1", "1", INIFILE) == "1";
            bPositive2 = ReadINIValue("Basic", "bPositive2", "1", INIFILE) == "1";
            bPositive3 = ReadINIValue("Basic", "bPositive3", "1", INIFILE) == "1";
            bEdgePolarity0 = ReadINIValue("Basic", "bEdgePolarity0", "1", INIFILE) == "1";
            bEdgePolarity1 = ReadINIValue("Basic", "bEdgePolarity1", "1", INIFILE) == "1";
            bEdgePolarity2 = ReadINIValue("Basic", "bEdgePolarity2", "1", INIFILE) == "1";
            bEdgePolarity3 = ReadINIValue("Basic", "bEdgePolarity3", "1", INIFILE) == "1";
        }
        public override void Save()
        {
            WriteINIValue("Basic", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Basic", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            WriteINIValue("Basic", "xMaxOverlap", xMaxOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xChipOverlap", xChipOverlap.ToString(), INIFILE);

            WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE);

            WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);

        }

        public void SaveRoi()
        {
            RoiCount = rectangles.Count;
            WriteINIValue("Inspect", "RoiCount", RoiCount.ToString(), INIFILE);
            int i = 0;
            while (i < RoiCount)
            {
                WriteINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(rectangles[i]), INIFILE);

                i++;
            }
        }

    }

    public class NoTrayParaClass : RecipeBaseClass
    {
        public NoTrayParaClass()
        {

        }
        private static NoTrayParaClass _instance = null;
        public static NoTrayParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NoTrayParaClass();
                return _instance;
            }
        }

        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("二值化的阈值")]
        [DisplayName("A01.灰阶阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        [DisplayName("A02.检测模式")]
        [TypeConverter(typeof(JzEnumConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public BlobMode xBlobMode { get; set; } = BlobMode.Black;
        [CategoryAttribute(_Cat1), DescriptionAttribute("搜寻范围扩大X方向的像素")]
        [DisplayName("A03.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendx { get; set; } = 20;
        [CategoryAttribute(_Cat1), DescriptionAttribute("搜寻范围扩大Y方向的像素")]
        [DisplayName("A04.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendy { get; set; } = 20;


        const string _Cat2 = "A02.规格设置";
        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最小的面积")]
        [DisplayName("A01.Blob面积最小值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMin { get; set; } = 5000;
        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最大的面积")]
        [DisplayName("A02.Blob面积最大值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMax { get; set; } = 99999999;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        [DisplayName("A03.存当前测试图")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public bool xSaveDebugImage { get; set; } = false;



        public override void Load(bool eCancel = false)
        {
            xThresholdValue = int.Parse(ReadINIValue("Basic", "xThresholdValue", "128", INIFILE));
            xBlobMode = (BlobMode)int.Parse(ReadINIValue("Basic", "xBlobMode", "1", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", INIFILE));

            xBlobAreaMin = int.Parse(ReadINIValue("Basic", "xBlobAreaMin", "5000", INIFILE));
            xBlobAreaMax = int.Parse(ReadINIValue("Basic", "xBlobAreaMax", "99999999", INIFILE));

            xSaveDebugImage = ReadINIValue("Basic", "xSaveDebugImage", "0", INIFILE) == "1";
        }
        public override void Save()
        {
            WriteINIValue("Basic", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobMode", ((int)xBlobMode).ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), INIFILE);

            WriteINIValue("Basic", "xBlobAreaMin", xBlobAreaMin.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMax", xBlobAreaMax.ToString(), INIFILE);

            WriteINIValue("Basic", "xSaveDebugImage", (xSaveDebugImage ? "1" : "0"), INIFILE);
        }

    }

    public class LineScanCalibrateClass : RecipeBaseClass
    {
        const int POINT_COUNT = 4;

        JetEazy.BasicSpace.CAoiCalibration cAoiCalibration = new JetEazy.BasicSpace.CAoiCalibration();
        PointF[,] _v1 = new PointF[2, 2];
        PointF[,] _w1 = new PointF[2, 2];

        //JetEazy.BasicSpace.CAoiCalibration cAoiCalibration2 = new JetEazy.BasicSpace.CAoiCalibration();
        //PointF[,] _v2 = new PointF[2, 2];
        //PointF[,] _w2 = new PointF[2, 2];

        public LineScanCalibrateClass()
        {

        }
        //private static LineScanCalibrateClass _instance = null;
        //public static LineScanCalibrateClass Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new LineScanCalibrateClass();
        //        return _instance;
        //    }
        //}
        public override void Initial(string epath, int ercpindex, string enamefile)
        {
            base.Initial(epath, ercpindex, enamefile);
            string dir = $"{Path}\\Calibration";
            INIFILE = $"{dir}\\{Name}";
        }
        public override void ChangeIndex(int eindex)
        {
            //base.ChangeIndex(eindex);
        }
        public PointF ViewToWorld(PointF ptview)
        {
            PointF ptworld = new PointF(ptview.X, ptview.Y);
            cAoiCalibration.TransformViewToWorld(ptview, out ptworld);
            return ptworld;
        }
        public PointF WorldToView(PointF ptworld)
        {
            PointF ptview = new PointF(ptworld.X, ptworld.Y);
            cAoiCalibration.TransformViewToWorld(ptworld, out ptview);
            return ptview;
        }
        //public PointF T2ViewToWorld(PointF ptview)
        //{
        //    PointF ptworld = new PointF(ptview.X, ptview.Y);
        //    cAoiCalibration2.TransformViewToWorld(ptview, out ptworld);
        //    return ptworld;
        //}
        //public PointF T2WorldToView(PointF ptworld)
        //{
        //    PointF ptview = new PointF(ptworld.X, ptworld.Y);
        //    cAoiCalibration2.TransformViewToWorld(ptworld, out ptview);
        //    return ptview;
        //}

        public PointF[] ptsview = new PointF[POINT_COUNT];
        public PointF[] ptsworld = new PointF[POINT_COUNT];
        //public PointF[] pts2view = new PointF[POINT_COUNT];
        //public PointF[] pts2world = new PointF[POINT_COUNT];

        //public override void Initial(string epath, int ercpindex, string enamefile)
        //{
        //    base.Initial(epath, ercpindex, enamefile);
        //}
        public override void Load(bool eCancel = false)
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                ptsview[i] = StringtoPointF(ReadINIValue("Tray", $"ptsview_{i}", $"{i},{i}", INIFILE));
                ptsworld[i] = StringtoPointF(ReadINIValue("Tray", $"ptsworld_{i}", $"{i},{i}", INIFILE));

                //pts2view[i] = StringtoPointF(ReadINIValue("Tray2", $"pts2view_{i}", $"{i},{i}", INIFILE));
                //pts2world[i] = StringtoPointF(ReadINIValue("Tray2", $"pts2world_{i}", $"{i},{i}", INIFILE));

                i++;
            }

            run();
        }
        public override void Save()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                WriteINIValue("Tray", $"ptsview_{i}", PointFtoStringSimple(ptsview[i]), INIFILE);
                WriteINIValue("Tray", $"ptsworld_{i}", PointFtoStringSimple(ptsworld[i]), INIFILE);
                //WriteINIValue("Tray2", $"pts2view_{i}", PointFtoStringSimple(pts2view[i]), INIFILE);
                //WriteINIValue("Tray2", $"pts2world_{i}", PointFtoStringSimple(pts2world[i]), INIFILE);

                i++;
            }

            run();
        }

        private void run()
        {
            _v1[0, 1] = ptsview[0];
            _v1[1, 1] = ptsview[1];
            _v1[0, 0] = ptsview[2];
            _v1[1, 0] = ptsview[3];

            _w1[0, 1] = ptsworld[0];
            _w1[1, 1] = ptsworld[1];
            _w1[0, 0] = ptsworld[2];
            _w1[1, 0] = ptsworld[3];
            cAoiCalibration.Dispose();
            cAoiCalibration.SetCalibrationPoints(_v1, _w1);
            cAoiCalibration.CalculateTransformMatrix();

            //_v2[0, 1] = pts2view[0];
            //_v2[1, 1] = pts2view[1];
            //_v2[0, 0] = pts2view[2];
            //_v2[1, 0] = pts2view[3];

            //_w2[0, 1] = pts2world[0];
            //_w2[1, 1] = pts2world[1];
            //_w2[0, 0] = pts2world[2];
            //_w2[1, 0] = pts2world[3];
            //cAoiCalibration2.Dispose();
            //cAoiCalibration2.SetCalibrationPoints(_v2, _w2);
            //cAoiCalibration2.CalculateTransformMatrix();

        }
    }

}
