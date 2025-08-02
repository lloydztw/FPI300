using Common.RecipeSpace;
using FreeImageAPI;
using JetEazy;
using JetEazy.BasicSpace;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.RunSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using TravellerMINIX6.OPSpace;
using VisionDesigner;
using VM.PlatformSDKCS;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class RecipeMainX2Class : RecipeBaseClass
    {
        protected RecipeMainX2Class()
        {

        }
        private static RecipeMainX2Class _instance = null;
        public static RecipeMainX2Class Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeMainX2Class();
                return _instance;
            }
        }

        public List<RegionCellX2Class> xRegionCells = new List<RegionCellX2Class>();

        public Bitmap bmpOrg = new Bitmap(1, 1);
        public Bitmap bmpbase0 = new Bitmap(1, 1);
        public Bitmap bmpbase1 = new Bitmap(1, 1);
        public Bitmap bmptemplate0 = new Bitmap(1, 1);
        public Bitmap bmptemplate1 = new Bitmap(1, 1);
        public System.Drawing.PointF template0Center = new System.Drawing.PointF(0, 0);
        public System.Drawing.PointF template1Center = new System.Drawing.PointF(0, 0);
        public System.Drawing.PointF distancebase0tobase1 = new System.Drawing.PointF(0, 0);

        public RectangleF xRectRegionPrint = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpprinttemplate = new Bitmap(1, 1);
        public Bitmap bmpprintmask = new Bitmap(1, 1);

        //public Bitmap bmpprint = new Bitmap(1, 1);
        //public Bitmap bmpprinttemp = new Bitmap(1, 1);
        public RectangleF xRectRegionBase0 = new RectangleF(0, 0, 100, 100);
        public RectangleF xRectRegionBase1 = new RectangleF(0, 0, 100, 100);

        public MvdFindClass mvdbase0_Find = new MvdFindClass();
        public MvdFindClass mvdbase1_Find = new MvdFindClass();
        public MvdFindClass mvdprinttemp_Find = new MvdFindClass();

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

        public string xLotNoStr = "NONE";

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

            PassCount = int.Parse(ReadINIValue("Recipe Basic", "PassCount", "0", INIFILE));
            NGCount = int.Parse(ReadINIValue("Recipe Basic", "NGCount", "0", INIFILE));

            xRectRegionBase0 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase0", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xRectRegionBase1 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase1", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            template0Center = StringtoPointF(ReadINIValue("Recipe Basic", "template0Center", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            template1Center = StringtoPointF(ReadINIValue("Recipe Basic", "template1Center", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            distancebase0tobase1 = StringtoPointF(ReadINIValue("Recipe Basic", "distancebase0tobase1", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

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
                string bmpbase0path = $"{PathIndexStr}\\base0.bmp";
                if (System.IO.File.Exists(bmpbase0path))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpbase0path);
                    bmpbase0.Dispose();
                    bmpbase0 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmpbase1path = $"{PathIndexStr}\\base1.bmp";
                if (System.IO.File.Exists(bmpbase1path))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpbase1path);
                    bmpbase1.Dispose();
                    bmpbase1 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmptemplate0path = $"{PathIndexStr}\\template0.bmp";
                if (System.IO.File.Exists(bmptemplate0path))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmptemplate0path);
                    bmptemplate0.Dispose();
                    bmptemplate0 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
                string bmptemplate1path = $"{PathIndexStr}\\template1.bmp";
                if (System.IO.File.Exists(bmptemplate1path))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmptemplate1path);
                    bmptemplate1.Dispose();
                    bmptemplate1 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }
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

            InspectX2Class.Instance.Initial(Path, Index, "Inspect_default_info.ini");
            InspectX2Class.Instance.Load();
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

            string bmporgpath = $"{PathIndexStr}\\org.bmp";
            bmpOrg.Save(bmporgpath, System.Drawing.Imaging.ImageFormat.Bmp);

            //建立所有的region
            CreateViews();
            int iOK = ViewTrainLoad();
            if (iOK != 0)
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载参数训练失败！");

            InspectX2Class.Instance.Save();
        }
        public void SaveBase()
        {
            WriteINIValue("Recipe Basic", "xRectRegionBase0", RectFtoStringSimple(xRectRegionBase0), INIFILE);
            WriteINIValue("Recipe Basic", "xRectRegionBase1", RectFtoStringSimple(xRectRegionBase1), INIFILE);
            WriteINIValue("Recipe Basic", "template0Center", PointFtoStringSimple(template0Center), INIFILE);
            WriteINIValue("Recipe Basic", "template1Center", PointFtoStringSimple(template1Center), INIFILE);
            WriteINIValue("Recipe Basic", "distancebase0tobase1", PointFtoStringSimple(distancebase0tobase1), INIFILE);
            string bmpbase0path = $"{PathIndexStr}\\base0.bmp";
            bmpbase0.Save(bmpbase0path, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmpbase1path = $"{PathIndexStr}\\base1.bmp";
            bmpbase1.Save(bmpbase1path, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmptemplate0path = $"{PathIndexStr}\\template0.bmp";
            bmptemplate0.Save(bmptemplate0path, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmptemplate1path = $"{PathIndexStr}\\template1.bmp";
            bmptemplate1.Save(bmptemplate1path, System.Drawing.Imaging.ImageFormat.Bmp);
        }
        public void SavePrintTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(xRectRegionPrint), INIFILE);
            string bmpprinttemplatepath = $"{PathIndexStr}\\bmpprinttemplate.bmp";
            bmpprinttemplate.Save(bmpprinttemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
            bmpprintmask.Save(bmpprintmaskpath, System.Drawing.Imaging.ImageFormat.Bmp);
            InspectX2Class.Instance.SaveRoi();
            SaveCodeTemplate();
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
        public int Base0Train()
        {
            mvdbase0_Find.bmpObj_Image = bmptemplate0;
            bool bOK = mvdbase0_Find.HikTrainBmp();
            return (bOK ? 0 : -1);
        }
        public int Base0Run(Bitmap ebmpInput)
        {
            mvdbase0_Find.bmpRun_Image = ebmpInput;
            bool bOK = mvdbase0_Find.HikRunBmp();
            return (bOK ? 0 : -1);
        }
        public int Base0Run(CMvdImage eMvdInput)
        {
            mvdbase0_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdbase0_Find.HikRun2();
            return (bOK ? 0 : -1);
        }
        public int Base0Run(CMvdImage eMvdInput, RectangleF eRectF)
        {
            mvdbase0_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdbase0_Find.HikRun3(eRectF);
            return (bOK ? 0 : -1);
        }
        public int Base1Train()
        {
            mvdbase1_Find.bmpObj_Image = bmptemplate1;
            bool bOK = mvdbase1_Find.HikTrainBmp();
            return (bOK ? 0 : -1);
        }
        public int Base1Run(Bitmap ebmpInput)
        {
            mvdbase1_Find.bmpRun_Image = ebmpInput;
            bool bOK = mvdbase1_Find.HikRunBmp();
            return (bOK ? 0 : -1);
        }
        public int Base1Run(CMvdImage eMvdInput)
        {
            mvdbase1_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdbase1_Find.HikRun2();
            return (bOK ? 0 : -1);
        }
        public int Base1Run(CMvdImage eMvdInput, RectangleF eRectF)
        {
            mvdbase1_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdbase1_Find.HikRun3(eRectF);
            return (bOK ? 0 : -1);
        }
        public int PrintTempTrain()
        {
            mvdprinttemp_Find.bmpObj_Image = bmpprinttemplate;
            bool bOK = mvdprinttemp_Find.HikTrainBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(Bitmap ebmpInput)
        {
            mvdprinttemp_Find.bmpRun_Image = ebmpInput;
            bool bOK = mvdprinttemp_Find.HikRunBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(CMvdImage eMvdInput)
        {
            mvdprinttemp_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdprinttemp_Find.HikRun2();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(CMvdImage eMvdInput, RectangleF eRectF)
        {
            mvdprinttemp_Find.xMvdRun_Image = eMvdInput;
            bool bOK = mvdprinttemp_Find.HikRun3(eRectF);
            return (bOK ? 0 : -1);
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
            //收集所有页面读取到的二维码
            List<string> _collectCodeList = new List<string>();
            _collectCodeList.Clear();
            foreach (RegionCellX2Class cell in xRegionCells)
            {
                if (cell.RunCodeInfo == null)
                    continue;
                string barcodeStr = cell.RunCodeInfo.Content;
                if (!string.IsNullOrEmpty(barcodeStr))
                    _collectCodeList.Add(barcodeStr);
            }

            List<string> _collectRepeatCodeList = new List<string>();
            //查询数据库中所有的条码
            isgood = JzCheckRepeatClass.Instance.MySqlTableQuery(_collectCodeList, ref _collectRepeatCodeList) <= 0;
            List<string> _collectNoRepeatCodeList = new List<string>();
            //匹配到各个分支
            if (!isgood)
            {
                _collectNoRepeatCodeList.Clear();
                foreach (RegionCellX2Class cell in xRegionCells)
                {
                    if (cell.RunCodeInfo == null)
                        continue;
                    string barcodeStr = cell.RunCodeInfo.Content;
                    if (!string.IsNullOrEmpty(barcodeStr))
                    {
                        bool bOK = cell.CheckRepeatCode(_collectRepeatCodeList, 0);
                        isgood &= bOK;
                        if (bOK)
                            _collectNoRepeatCodeList.Add(barcodeStr);
                    }
                }
                JzCheckRepeatClass.Instance.MySqlTableInsert(_collectNoRepeatCodeList);
            }
            else
            {
                JzCheckRepeatClass.Instance.MySqlTableInsert(_collectCodeList);
            }

            //比对同一片的重复码
            foreach (RegionCellX2Class cell in xRegionCells)
            {
                bool bOK = cell.CheckRepeatCode(_collectCodeList);
                isgood &= bOK;
            }
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
            foreach (RegionCellX2Class cell in xRegionCells)
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


        public int PassCount = 0;
        public int NGCount = 0;
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
            int iret = Base0Train();
            if (iret == 0)
                iret = Base1Train();
            if (iret == 0)
                iret = PrintTempTrain();
            if (iret == 0)
            {
                //计算初始位置
                ProcessRunClass.Instance.cMvdInput = BitmapToCMvdImage(bmpOrg);
                ProcessRunClass.Instance.RunRecipe();
                bool bFoundALIGNERR = false;
                foreach (RegionCellX2Class cell in xRegionCells)
                {
                    if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    {
                        bFoundALIGNERR = true;
                        break;
                    }
                }
                if (bFoundALIGNERR)
                {
                    iret = -1;
                }
            }
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
                for (int j = 0; j < xColumn; j++)
                {
                    RegionCellX2Class _cell = new RegionCellX2Class();
                    _cell.Index = _index;
                    _cell.CellRow = i;
                    _cell.CellCol = j;
                    _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                    _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);
                    xRegionCells.Add(_cell);
                    _index++;
                }
            }

            AnalyzeDatasData();
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

    public class InspectX2Class : RecipeBaseClass
    {
        public InspectX2Class()
        {

        }
        private static InspectX2Class _instance = null;
        public static InspectX2Class Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InspectX2Class();
                return _instance;
            }
        }

        const string _Cat0 = "A00.检测设置";
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A01.是否检测偏移")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckOffset { get; set; } = false;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A02.是否检测字符")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckInspect { get; set; } = true;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A03.是否检测2D码")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckCode { get; set; } = false;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A04.比对2D码")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckMappingCode { get; set; } = false;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A05.比对2D重复码")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckRepeatCode { get; set; } = false;

        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        [DisplayName("A01.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        [DisplayName("A02.二值化阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;

        const string _Cat2 = "A02.规格设置";
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位pixel")]
        [DisplayName("A01.字符缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位pixel")]
        [DisplayName("A02.字符缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位pixel")]
        [DisplayName("A03.字符缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharArea { get; set; } = 30.1f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位pixel")]
        [DisplayName("A04.背景缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xBackgroudWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位pixel")]
        [DisplayName("A05.背景缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xBackgroudHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位pixel")]
        [DisplayName("A06.背景缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xBackgroudArea { get; set; } = 30.1f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位mm")]
        [DisplayName("A07.偏移X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xOffsetX { get; set; } = 0.3f;
        [CategoryAttribute(_Cat2), DescriptionAttribute("单位mm")]
        [DisplayName("A08.偏移Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xOffsetY { get; set; } = 0.3f;

        [Browsable(false)]
        public int RoiCount { get; set; } = 0;
        [Browsable(false)]
        public List<RectangleF> rectangles { get; set; } = new List<RectangleF>();
        //[Browsable(false)]
        //public RectangleF CodeRegion { get; set; } = new RectangleF(0, 0, 10, 10);

        public override void Load(bool eCancel = false)
        {
            xTolerance = float.Parse(ReadINIValue("Inspect", "xTolerance", "0.5", INIFILE));
            xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", INIFILE));
            xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", INIFILE));
            xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", INIFILE));
            xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", INIFILE));
            xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", INIFILE));
            xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", INIFILE));
            xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", INIFILE));
            xOffsetX = float.Parse(ReadINIValue("Inspect", "xOffsetX", "0.3", INIFILE));
            xOffsetY = float.Parse(ReadINIValue("Inspect", "xOffsetY", "0.3", INIFILE));

            //xTolerance = float.Parse(ReadINIValue("Inspect", "xTolerance", xTolerance.ToString(), INIFILE));
            //xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE));
            //xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE));
            //xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE));
            //xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE));
            //xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE));
            //xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE));
            //xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE));
            //xOffsetX = float.Parse(ReadINIValue("Inspect", "xOffsetX", xOffsetX.ToString(), INIFILE));
            //xOffsetY = float.Parse(ReadINIValue("Inspect", "xOffsetY", xOffsetY.ToString(), INIFILE));

            RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            int i = 0;
            rectangles.Clear();
            while (i < RoiCount)
            {
                RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
                rectangles.Add(rectf);

                i++;
            }
            //CodeRegion = StringtoRectF(ReadINIValue("Code", $"CodeRegion", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));

            bCheckOffset = ReadINIValue("Inspect", "bCheckOffset", "0", INIFILE) == "1";
            bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            bCheckCode = ReadINIValue("Inspect", "bCheckCode", "0", INIFILE) == "1";
            bCheckMappingCode = ReadINIValue("Inspect", "bCheckMappingCode", "0", INIFILE) == "1";
            bCheckRepeatCode = ReadINIValue("Inspect", "bCheckRepeatCode", "0", INIFILE) == "1";
        }
        public override void Save()
        {
            WriteINIValue("Inspect", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE);
            WriteINIValue("Inspect", "xOffsetX", xOffsetX.ToString(), INIFILE);
            WriteINIValue("Inspect", "xOffsetY", xOffsetY.ToString(), INIFILE);

            WriteINIValue("Inspect", "bCheckOffset", (bCheckOffset ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "bCheckCode", (bCheckCode ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "bCheckMappingCode", (bCheckMappingCode ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "bCheckRepeatCode", (bCheckRepeatCode ? "1" : "0"), INIFILE);
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
            //WriteINIValue("Code", $"CodeRegion", RectFtoStringSimple(CodeRegion), INIFILE);
        }



    }
}
